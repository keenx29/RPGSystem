using Microsoft.AspNetCore.Mvc;
using RPGSystem.Models.Characters;
using RPGSystem.Models.Rolls;
using RPGSystem.Services;
using RPGSystem.ViewModels;

namespace RPGSystem.Controllers
{
    public class CharacterController : Controller
    {
        private readonly DiceService _diceService;
        private readonly CharacterService _characterService;
        private static List<RollResult> _rollHistory = new();
        private static RollStateService _rollState;

        public CharacterController(
            DiceService diceService, 
            CharacterService characterService, 
            RollStateService rollState)
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
            var result = _characterService.RollAbility(abilityType, _rollState.SelectedAdvantageState);

            _rollHistory.Insert(0, result);

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult RollSavingThrow(AbilityType abilityType)
        {
            var result = _characterService.RollSavingThrow(abilityType, _rollState.SelectedAdvantageState);

            _rollHistory.Insert(0, result);

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult RollSkill(SkillType skillType)
        {
            var result = _characterService.RollSkill(skillType, _rollState.SelectedAdvantageState);

            _rollHistory.Insert(0, result);

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult RollAttack()
        {
            var result = _characterService.RollAttack( _rollState.SelectedAdvantageState);

            _rollHistory.Insert(0, result);

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult RollDamage()
        {
            var result = _characterService.RollDamage();

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
        public IActionResult UseItem(Guid itemId)
        {
            var result = _characterService.UseItem(itemId);

            if (result != null)
                _rollHistory.Insert(0, result);

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
            var result = _characterService.LevelUp();

            if (result != null)
                _rollHistory.Insert(0, result);

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult UseSecondWind()
        {
            var result = _characterService.UseSecondWind();

            if (result != null)
                _rollHistory.Insert(0, result);

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult UseActionSurge()
        {
            _characterService.UseActionSurge();
            return RedirectToAction("Sheet");
        }

    }
}
