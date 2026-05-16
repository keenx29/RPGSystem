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
            Character character = _characterService.GetTestCharacter();

            CharacterSheetViewModel vm = new CharacterSheetViewModel
            {
                Character = character,
                RollHistory = _rollHistory
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult RollStat(string statName)
        {
            Character character = _characterService.GetTestCharacter();

            int modifier = 0;

            switch (statName)
            {
                case "Strength":
                    modifier = character.GetStrengthModifier();
                    break;

                case "Dexterity":
                    modifier = character.GetDexterityModifier();
                    break;

                case "Constitution":
                    modifier = character.GetConstitutionModifier();
                    break;

                case "Intelligence":
                    modifier = character.GetIntelligenceModifier();
                    break;

                case "Wisdom":
                    modifier = character.GetWisdomModifier();
                    break;

                case "Charisma":
                    modifier = character.GetCharismaModifier();
                    break;
            }

            int diceRoll = _diceService.RollD20();

            RollResult result = new RollResult
            {
                DiceRoll = diceRoll,
                Modifier = modifier,
                Title = $"{statName} Check"
            };
            _rollHistory.Insert(0, result);

            CharacterSheetViewModel vm = new CharacterSheetViewModel
            {
                Character = character,
                RollHistory = _rollHistory
            };

            return View("Sheet", vm);
        }

        [HttpPost]
        public IActionResult RollSkill(string skillName)
        {
            Character character = _characterService.GetTestCharacter();

            Skill skill = character.Skills.First(s => s.Name == skillName);

            int roll = _diceService.RollD20();

            int modifier = character.GetSkillBonus(skill);

            RollResult result = new RollResult
            {
                DiceRoll = roll,
                Modifier = modifier,
                Title = $"{skill.Name} Check"
            };

            _rollHistory.Insert(0, result);

            CharacterSheetViewModel vm = new CharacterSheetViewModel
            {
                Character = character,
                RollHistory = _rollHistory
            };

            return View("Sheet", vm);
        }
        [HttpPost]
        public IActionResult RollAttack()
        {
            Character character = _characterService.GetTestCharacter();

            Weapon weapon = character.EquippedWeapon;

            int roll = _diceService.RollD20();

            int modifier =
                character.GetStrengthModifier()// TODO: STR/DEX logic
                + character.GetProficiencyBonus()// TODO: Weapon Proficiencies
                + weapon.AttackBonus;

            _rollHistory.Insert(0, new RollResult
            {
                DiceRoll = roll,
                Modifier = modifier,
                Title = $"{weapon.Name} Attack",
                RollType = "Attack",
                WeaponName = weapon.Name
            });

            return View("Sheet", new CharacterSheetViewModel
            {
                Character = character,
                RollHistory = _rollHistory
            });
        }
        [HttpPost]
        public IActionResult RollDamage()
        {
            Character character = _characterService.GetTestCharacter();
            var weapon = character.EquippedWeapon;

            int damageRoll = _diceService.RollDice(weapon.DamageDice);

            int modifier = character.GetStrengthModifier();// TODO: Damage Bonuses (i.e. Rage)

            _rollHistory.Insert(0, new RollResult
            {
                DiceRoll = damageRoll,
                Modifier = modifier,
                Title = $"{weapon.Name} Damage",
                RollType = "Damage",
                WeaponName = weapon.Name
            });

            return View("Sheet", new CharacterSheetViewModel
            {
                Character = character,
                RollHistory = _rollHistory
            });
        }
        //[HttpPost]
        //public IActionResult RollAttack()
        //{
        //    Character character = _characterService.GetTestCharacter();

        //    Weapon weapon = character.EquippedWeapon;

        //    int d20 = _diceService.RollD20();

        //    int abilityMod = character.GetStrengthModifier(); 
        //    int prof = character.GetProficiencyBonus();
        //    int weaponBonus = weapon.AttackBonus;

        //    int attackTotal = d20 + abilityMod + prof + weaponBonus;

        //    // Damage roll (basic)
        //    int damageRoll = _diceService.RollDice(weapon.DamageDice);
        //    int damageTotal = damageRoll + abilityMod;

        //    // Attack result
        //    _rollHistory.Insert(0, new RollResult
        //    {
        //        StatName = $"{weapon.Name} Attack",
        //        DiceRoll = d20,
        //        Modifier = abilityMod + prof + weaponBonus,
        //        RollType = "Attack"
        //    });

        //    // Damage result
        //    _rollHistory.Insert(0, new RollResult
        //    {
        //        StatName = $"{weapon.Name} Damage",
        //        DiceRoll = damageRoll,
        //        Modifier = abilityMod,
        //        RollType = "Damage"
        //    });

        //    CharacterSheetViewModel vm = new CharacterSheetViewModel
        //    {
        //        Character = character,
        //        RollHistory = _rollHistory
        //    };

        //    return View("Sheet", vm);
        //}
    }
}
