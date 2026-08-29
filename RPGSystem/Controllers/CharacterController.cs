using Microsoft.AspNetCore.Mvc;
using RPGSystem.Models.Characters;
using RPGSystem.Models.Items;
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
        public IActionResult Index()
        {
            var characters = _characterService.GetCharacters();

            return View(characters);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateCharacterViewModel());
        }
        [HttpGet]
        public IActionResult Sheet()
        {
            var character = _characterService.GetCharacter();
            var vm = new CharacterSheetViewModel
            {
                Character = character,
                HitDie = _characterService.GetHitDie(),
                RollHistory = _rollHistory,
                SelectedAdvantageState = _rollState.SelectedAdvantageState,
                AvailableCharacters = _characterService.GetCharacters().ToList(),
                SelectedCharacterId = character.Id
            };
            
            return View(vm);
        }
        [HttpPost]
        public IActionResult OpenCharacter(Guid characterId)
        {
            _characterService.SelectCharacter(characterId);

            _rollHistory.Clear();
            _rollState.SelectedAdvantageState = AdvantageState.Normal;

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult SelectCharacter(Guid characterId)
        {
            _characterService.SelectCharacter(characterId);

            _rollHistory.Clear();
            _rollState.SelectedAdvantageState = AdvantageState.Normal;

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult CreateCharacter(CreateCharacterViewModel model)
        {
            _characterService.CreateCharacter(model);

            _rollHistory.Clear();
            _rollState.SelectedAdvantageState = AdvantageState.Normal;

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult SaveCharacters()
        {
            _characterService.SaveCharacters();

            _rollHistory.Insert(0, RollResult.Info("Database", "Character data saved."));

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult DeleteCharacter(Guid characterId)
        {
            var result = _characterService.DeleteCharacter(characterId);

            _rollHistory.Clear();
            _rollState.SelectedAdvantageState = AdvantageState.Normal;

            if (result != null)
                _rollHistory.Insert(0, result);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ResetDemoCharacter(Guid characterId)
        {
            var result = _characterService.ResetDemoCharacter(characterId);

            _rollHistory.Clear();
            _rollState.SelectedAdvantageState = AdvantageState.Normal;

            if (result != null)
                _rollHistory.Insert(0, result);

            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult AddCondition(ConditionType condition)
        {
            _characterService.AddCondition(condition);

            return RedirectToAction("Sheet");
        }

        [HttpPost]
        public IActionResult RemoveCondition(ConditionType condition)
        {
            _characterService.RemoveCondition(condition);

            return RedirectToAction("Sheet");
        }

        [HttpPost]
        public IActionResult ClearConditions()
        {
            _characterService.ClearConditions();

            return RedirectToAction("Sheet");
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
        public IActionResult RollAttack(Guid? weaponId)
        {
            var result = weaponId.HasValue
                ? _characterService.RollAttack(weaponId.Value, _rollState.SelectedAdvantageState)
                : _characterService.RollAttack(_rollState.SelectedAdvantageState);

            _rollHistory.Insert(0, result);

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult RollDamage(Guid weaponId)
        {
            var result = _characterService.RollDamage(weaponId);

            _rollHistory.Insert(0, result);

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult RollCriticalDamage(Guid weaponId)
        {
            var result = _characterService.RollCriticalDamage(weaponId);

            _rollHistory.Insert(0, result);

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult RollDeathSave()
        {
            var result = _characterService.RollDeathSave(_rollState.SelectedAdvantageState);

            _rollHistory.Insert(0, result);

            return RedirectToAction("Sheet");
        }

        [HttpPost]
        public IActionResult Stabilize()
        {
            var result = _characterService.Stabilize();

            _rollHistory.Insert(0, result);

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult AddInventoryItem(AddInventoryItemViewModel model)
        {
            _characterService.AddInventoryItem(model);

            return RedirectToAction("Sheet");
        }

        [HttpPost]
        public IActionResult RemoveInventoryItem(Guid itemId)
        {
            _characterService.RemoveInventoryItem(itemId);

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult EquipWeapon(Guid weaponId)
        {
            _characterService.EquipWeapon(weaponId);
            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult UnequipWeapon(Guid weaponId)
        {
            _characterService.UnequipWeapon(weaponId);

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult EquipArmor(Guid armorId)
        {
            _characterService.EquipArmor(armorId);
            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult UnequipArmor(Guid armorId)
        {
            _characterService.UnequipArmor(armorId);

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult EquipShield(Guid shieldId)
        {
            _characterService.EquipShield(shieldId);

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult UnequipShield(Guid shieldId)
        {
            _characterService.UnequipShield(shieldId);

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
        public IActionResult UseFeature(string featureName)
        {
            var result = _characterService.UseFeature(featureName);

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
        public IActionResult ShortRest(int hitDiceCount)
        {
            var result = _characterService.ShortRest(hitDiceCount);

            if (result != null)
                _rollHistory.Insert(0, result);

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
        public IActionResult IncreaseAbilityScore(AbilityType abilityType)
        {
            _characterService.IncreaseAbilityScore(abilityType);

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult ToggleFeature(string featureName)
        {
            var result = _characterService.ToggleFeature(featureName);

            if (result != null)
                _rollHistory.Insert(0, result);

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult SetSkillProficiency(SkillType skillType, bool isProficient)
        {
            var result = _characterService.SetSkillProficiency(skillType, isProficient);

            if (result != null)
                _rollHistory.Insert(0, result);
            

            return RedirectToAction("Sheet");
        }

        [HttpPost]
        public IActionResult SetSkillExpertise(SkillType skillType, bool isExpertise)
        {
            var result = _characterService.SetSkillExpertise(skillType, isExpertise);

            if (result != null)
                _rollHistory.Insert(0, result);

            return RedirectToAction("Sheet");
        }

        [HttpPost]
        public IActionResult SetSavingThrowProficiency(AbilityType abilityType, bool isProficient)
        {
            _characterService.SetSavingThrowProficiency(abilityType, isProficient);

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult UpdateCharacterNotes(UpdateCharacterNotesViewModel model)
        {
            _characterService.UpdateCharacterNotes(model);

            return RedirectToAction("Sheet");
        }
    }
}
