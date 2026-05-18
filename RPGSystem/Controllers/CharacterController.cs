using Microsoft.AspNetCore.Mvc;
using RPGSystem.Models;
using RPGSystem.Services;

namespace RPGSystem.Controllers
{
    public class CharacterController : Controller
    {
        private readonly DiceService _diceService;
        private readonly CharacterService _characterService;
        private static List<RollResult> _rollHistory = new();
        private static RollStateService _rollState;

        public CharacterController(DiceService diceService, CharacterService characterService, RollStateService rollState)
        {
            _diceService = diceService;
            _characterService = characterService;
            _rollState = rollState;
        }

        [HttpGet]
        public IActionResult Sheet()
        {
            var vm = new CharacterSheetViewModel
            {
                Character = _characterService.GetTestCharacter(),
                RollHistory = _rollHistory,
                SelectedAdvantageState = _rollState.SelectedAdvantageState
            };
            
            return View(vm);
        }
        [HttpPost]
        public IActionResult SetAdvantageState(string state)
        {
            Enum.TryParse(state, out AdvantageState parsed);

            _rollState.SelectedAdvantageState = parsed;

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult RollAbility(string abilityName)
        {
            var character = _characterService.GetTestCharacter();

            var ability = character.GetAbility(abilityName);

            var advantage = _rollState.SelectedAdvantageState;

            int roll = _diceService.RollD20(advantage);

            int modifier = ability.Modifier;

            var result = new RollResult
            {
                Actor = ability.Name,
                Type = RollType.Check,
                DiceRoll = roll,
                Modifier = modifier
            };

            _rollHistory.Insert(0, result);

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult RollSavingThrow(string abilityName)
        {
            var character = _characterService.GetTestCharacter();

            var ability = character.GetAbility(abilityName);

            var advantage = _rollState.SelectedAdvantageState;

            int roll = _diceService.RollD20(advantage);

            int modifier = character.GetSavingThrowBonus(ability);

            var result = new RollResult
            {
                Actor = ability.Name,
                Type = RollType.Save,
                DiceRoll = roll,
                Modifier = modifier
            };

            _rollHistory.Insert(0, result);

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult RollSkill(string skillName)
        {
            var character = _characterService.GetTestCharacter();

            var skill = character.Skills.First(s => s.Name == skillName);

            var advantage = _rollState.SelectedAdvantageState;

            int roll = _diceService.RollD20(advantage);

            int modifier = skill.GetBonus(character.GetProficiencyBonus());

            var result = new RollResult
            {
                Actor = skill.Name,
                Type = RollType.Check,
                DiceRoll = roll,
                Modifier = modifier
            };

            _rollHistory.Insert(0, result);

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult RollAttack()
        {
            var character = _characterService.GetTestCharacter();

            var weapon = character.EquippedWeapon;

            var advantage = _rollState.SelectedAdvantageState;

            int roll = _diceService.RollD20(advantage);

            int modifier =
                character.GetAbility("Strength").Modifier // TODO: STR/DEX logic
                + character.GetProficiencyBonus() // TODO: Weapon Proficiencies
                + weapon.AttackBonus;

            var result = new RollResult
            {
                Actor = weapon.Name, 
                Type = RollType.Attack,
                DiceRoll = roll,
                Modifier = modifier,
            };
            _rollHistory.Insert(0, result);

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult RollDamage()
        {
            var character = _characterService.GetTestCharacter();
            var weapon = character.EquippedWeapon;

            int roll = _diceService.RollDice(weapon.DamageDice);

            int modifier = character.GetAbility("Strength").Modifier;// TODO: Damage Bonuses (i.e. Rage)

            var result = new RollResult
            {
                Actor = weapon.Name,
                Type = RollType.Damage,
                DiceRoll = roll,
                Modifier = modifier,
                DamageType = weapon.DamageType
            };

            _rollHistory.Insert(0, result);

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult EquipWeapon(string weaponName)
        {
            var character = _characterService.GetTestCharacter();
            var weapon = character.Inventory.OfType<Weapon>().First(w => w.Name == weaponName);

            character.EquipWeapon(weapon);

            var vm = new CharacterSheetViewModel
            {
                Character = character,
                RollHistory = _rollHistory
            };

            return View("Sheet", vm);
        }
        [HttpPost]
        public IActionResult EquipArmor(string armorName)
        {
            var character = _characterService.GetTestCharacter();
            var armor = character.Inventory.OfType<Armor>().First(w => w.Name == armorName);

            character.EquipArmor(armor);

            var vm = new CharacterSheetViewModel
            {
                Character = character,
                RollHistory = _rollHistory
            };

            return View("Sheet", vm);
        }
        [HttpPost]
        public IActionResult ModifyHP(int amount, string mode)
        {
            var character = _characterService.GetTestCharacter();

            if (mode == "damage")
            {
                character.CurrentHP -= amount;
            }
            else if (mode == "heal")
            {
                character.CurrentHP += amount;
            }

            character.CurrentHP = Math.Clamp(character.CurrentHP, 0, character.MaxHP);
            
            var vm = new CharacterSheetViewModel
            {
                Character = character,
                RollHistory = _rollHistory
            };

            return View("Sheet", vm);
        }
        [HttpPost]
        public IActionResult ShortRest()
        {
            var character = _characterService.GetTestCharacter();
            character.CurrentHP = Math.Min(character.MaxHP, character.CurrentHP + (character.MaxHP / 4));

            var vm = new CharacterSheetViewModel
            {
                Character = character,
                RollHistory = _rollHistory
            };

            return View("Sheet", vm);
        }
        [HttpPost]
        public IActionResult LongRest()
        {
            var character = _characterService.GetTestCharacter();
            character.CurrentHP = character.MaxHP;

            var vm = new CharacterSheetViewModel
            {
                Character = character,
                RollHistory = _rollHistory
            };

            return View("Sheet", vm);
        }
        [HttpPost]
        public IActionResult LevelUp()
        {
            var character = _characterService.GetTestCharacter();
            var ability = character.GetAbility("Constitution");
            int hpGain =
            _diceService.RollDice($"1d{character.GetHitDie()}")
            + ability.Modifier;

            character.LevelUp(hpGain);

            var vm = new CharacterSheetViewModel
            {
                Character = character,
                RollHistory = _rollHistory
            };

            return View("Sheet", vm);
        }
    }
}
