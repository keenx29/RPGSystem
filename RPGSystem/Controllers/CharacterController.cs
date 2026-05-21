using Microsoft.AspNetCore.Mvc;
using RPGSystem.Helpers;
using RPGSystem.Models;
using RPGSystem.Services;

namespace RPGSystem.Controllers
{
    public class CharacterController : Controller
    {
        private readonly DiceService _diceService;
        private readonly RollService _rollService;
        private readonly CharacterService _characterService;
        private static List<RollResult> _rollHistory = new();
        private static RollStateService _rollState;

        public CharacterController(
            DiceService diceService, 
            CharacterService characterService, 
            RollStateService rollState, 
            RollService rollService)
        {
            _diceService = diceService;
            _characterService = characterService;
            _rollState = rollState;
            _rollService = rollService;
        }

        [HttpGet]
        public IActionResult Sheet()
        {
            var vm = new CharacterSheetViewModel
            {
                Character = _characterService.GetCharacter(),
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
        public IActionResult RollAbility(AbilityType abilityType)
        {
            var result = _rollService.RollAbility(abilityType, _rollState.SelectedAdvantageState);

            _rollHistory.Insert(0, result);

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult RollSavingThrow(AbilityType abilityType)
        {
            var result = _rollService.RollSavingThrow(abilityType, _rollState.SelectedAdvantageState);

            _rollHistory.Insert(0, result);

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult RollSkill(string skillName)
        {
            var result = _rollService.RollSkill(skillName, _rollState.SelectedAdvantageState);

            _rollHistory.Insert(0, result);

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult RollAttack()
        {
            var result = _rollService.RollAttack( _rollState.SelectedAdvantageState);

            _rollHistory.Insert(0, result);

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult RollDamage()
        {
            var result = _rollService.RollDamage();

            _rollHistory.Insert(0, result);

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult EquipWeapon(Guid weaponId)
        {
            _characterService.EquipWeapon(weaponId);
            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult EquipArmor(Guid armorId)
        {
            _characterService.EquipArmor(armorId);
            return RedirectToAction("Sheet");
        }

        [HttpPost]
        public IActionResult ModifyHP(int amount, string mode)
        {
            Enum.TryParse(mode, true, out HpChangeType type);
            _characterService.ModifyHP(amount, type);
            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult ShortRest()
        {
            _characterService.ShortRest();
            return RedirectToAction("Sheet");
        }

        [HttpPost]
        public IActionResult LongRest()
        {
            _characterService.LongRest();
            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult LevelUp()
        {
            _characterService.LevelUp();
            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult UseSecondWind()
        {
            _characterService.UseSecondWind();
            return RedirectToAction("Sheet");
        }
    }
}
