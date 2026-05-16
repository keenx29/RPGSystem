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
        public IActionResult RollStat(string statName)
        {
            var character = _characterService.GetTestCharacter();

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

            int roll = _diceService.RollD20();

            var result = new RollResult
            {
                Actor = statName,
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
        public IActionResult RollSkill(string skillName)
        {
            var character = _characterService.GetTestCharacter();

            var skill = character.Skills.First(s => s.Name == skillName);

            int roll = _diceService.RollD20();

            int modifier = character.GetSkillBonus(skill);

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
                character.GetStrengthModifier()// TODO: STR/DEX logic
                + character.GetProficiencyBonus()// TODO: Weapon Proficiencies
                + weapon.AttackBonus;

            var result = new RollResult
            {
                Actor = weapon.Name, 
                Type = RollType.Attack,
                DiceRoll = roll,
                Modifier = modifier,
            };
            _rollHistory.Insert(0, result);

            var viewModel = new CharacterSheetViewModel
            {
                Character = character,
                RollHistory = _rollHistory
            };

            return View("Sheet", viewModel);
        }
        [HttpPost]
        public IActionResult RollDamage()
        {
            var character = _characterService.GetTestCharacter();
            var weapon = character.EquippedWeapon;

            int roll = _diceService.RollDice(weapon.DamageDice);

            int modifier = character.GetStrengthModifier();// TODO: Damage Bonuses (i.e. Rage)

            var result = new RollResult
            {
                Actor = weapon.Name,
                Type = RollType.Damage,
                DiceRoll = roll,
                Modifier = modifier,
                DamageType = weapon.DamageType
            };

            _rollHistory.Insert(0, result);

            var viewModel = new CharacterSheetViewModel
            {
                Character = character,
                RollHistory = _rollHistory
            };

            return View("Sheet", viewModel);
        }
    }
}
