using Microsoft.AspNetCore.Mvc;
using RPGSystem.Models;
using RPGSystem.Services;

namespace RPGSystem.Controllers
{
    public class CharacterController : Controller
    {
        private readonly DiceService _diceService;

        public CharacterController(DiceService diceService)
        {
            _diceService = diceService;
        }

        [HttpGet]
        public IActionResult Sheet()
        {
            Character character = new Character
            {
                Name = "Thorin",
                    
                Strength = 16,
                Dexterity = 12,
                Constitution = 14,

                Intelligence = 10,
                Wisdom = 8,
                Charisma = 13
            };

            CharacterSheetViewModel vm = new CharacterSheetViewModel
            {
                Character = character
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult RollStat(string statName)
        {
            Character character = new Character
            {
                Name = "Thorin",

                Strength = 16,
                Dexterity = 12,
                Constitution = 14,

                Intelligence = 10,
                Wisdom = 8,
                Charisma = 13
            };

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
                StatName = statName
            };

            CharacterSheetViewModel vm = new CharacterSheetViewModel
            {
                Character = character,
                RollResult = result
            };

            return View("Sheet", vm);
        }
    }
}
