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

        public CharacterController(DiceService diceService, CharacterService characterService)
        {
            _diceService = diceService;
            _characterService = characterService;
        }

        [HttpGet]
        public IActionResult Sheet()
        {
            var character = _characterService.GetTestCharacter();

            var vm = new CharacterSheetViewModel
            {
                Character = character,
                RollHistory = _rollHistory
            };
            
            return View(vm);
        }
        [HttpPost]
        public IActionResult RollAbility(string abilityName)
        {
            var character = _characterService.GetTestCharacter();

            var ability = character.GetAbility(abilityName);

            int roll = _diceService.RollD20();
            int modifier = ability.Modifier;

            var result = new RollResult
            {
                Actor = ability.Name,
                Type = RollType.Check,
                DiceRoll = roll,
                Modifier = modifier
            };

            _rollHistory.Insert(0, result);

            var vm = new CharacterSheetViewModel
            {
                Character = character,
                RollHistory = _rollHistory
            };

            return View("Sheet", vm);
        }
        [HttpPost]
        public IActionResult RollSavingThrow(string abilityName)
        {
            var character = _characterService.GetTestCharacter();

            var ability = character.GetAbility(abilityName);

            int roll = _diceService.RollD20();

            int modifier = character.GetSavingThrowBonus(ability);

            var result = new RollResult
            {
                Actor = ability.Name,
                Type = RollType.Save,
                DiceRoll = roll,
                Modifier = modifier
            };

            _rollHistory.Insert(0, result);

            var vm = new CharacterSheetViewModel
            {
                Character = character,
                RollHistory = _rollHistory
            };

            return View("Sheet", vm);
        }
        [HttpPost]
        public IActionResult RollSkill(string skillName)
        {
            var character = _characterService.GetTestCharacter();

            var skill = character.Skills.First(s => s.Name == skillName);

            int roll = _diceService.RollD20();

            int modifier = skill.GetBonus(character.GetProficiencyBonus());

            var result = new RollResult
            {
                Actor = skill.Name,
                Type = RollType.Check,
                DiceRoll = roll,
                Modifier = modifier
            };

            _rollHistory.Insert(0, result);

            var vm = new CharacterSheetViewModel
            {
                Character = character,
                RollHistory = _rollHistory
            };

            return View("Sheet", vm);
        }
        [HttpPost]
        public IActionResult RollAttack()
        {
            var character = _characterService.GetTestCharacter();

            var weapon = character.EquippedWeapon;

            int roll = _diceService.RollD20();

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

            var vm = new CharacterSheetViewModel
            {
                Character = character,
                RollHistory = _rollHistory
            };

            return View("Sheet", vm);
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

            var vm = new CharacterSheetViewModel
            {
                Character = character,
                RollHistory = _rollHistory
            };

            return View("Sheet", vm);
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
    }
}
