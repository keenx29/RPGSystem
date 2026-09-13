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
        private readonly CharacterPortraitService _characterPortraitService;
        private static List<RollResult> _rollHistory = new();
        private static RollStateService _rollState;

        public CharacterController(
            DiceService diceService, 
            CharacterService characterService, 
            RollStateService rollState,
            CharacterPortraitService characterPortraitService)
        {
            _diceService = diceService;
            _characterService = characterService;
            _rollState = rollState;
            _characterPortraitService = characterPortraitService;
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
        [HttpGet]
        public IActionResult AddItem()
        {
            return View(new AddInventoryItemViewModel());
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
        public async Task<IActionResult> CreateCharacter(
    CreateCharacterViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    "Character name is required.");
            }

            if (!CharacterCreationCatalog.IsSupportedRace(model.Race))
            {
                ModelState.AddModelError(
                    nameof(model.Race),
                    "Choose a race from the list.");
            }

            if (!CharacterCreationCatalog.IsSupportedBackground(model.Background))
            {
                ModelState.AddModelError(
                    nameof(model.Background),
                    "Choose a background from the list.");
            }

            if (model.AbilityScoreMode == AbilityScoreMode.StandardArray &&
                !CharacterCreationCatalog.IsValidStandardArray(
                    model.GetSelectedAbilityScores()))
            {
                ModelState.AddModelError(
                    nameof(model.AbilityScoreMode),
                    "Use each standard array score exactly once.");
            }

            if (!ModelState.IsValid)
            {
                return View("Create", model);
            }

            string? portraitPath;

            try
            {
                portraitPath = await _characterPortraitService
                    .SavePortraitAsync(model.PortraitFile);
            }
            catch (InvalidOperationException exception)
            {
                ModelState.AddModelError(
                    nameof(model.PortraitFile),
                    exception.Message);

                return View("Create", model);
            }

            _characterService.CreateCharacter(model, portraitPath);

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
            var result = _characterService.AddCondition(condition);

            if (result != null)
                _rollHistory.Insert(0, result);

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
        public IActionResult AddSense(AddCharacterSenseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _rollHistory.Insert(0,
                    RollResult.Info("System", "Enter a valid sense name and range."));
            }
            else
            {
                var result = _characterService.AddSense(model);

                if (result != null)
                    _rollHistory.Insert(0, result);
            }

            return RedirectToAction("Sheet");
        }

        [HttpPost]
        public IActionResult RemoveSense(Guid senseId)
        {
            _characterService.RemoveSense(senseId);

            return RedirectToAction("Sheet");
        }

        [HttpPost]
        public IActionResult AddDefense(AddDefenseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _rollHistory.Insert(0,
                    RollResult.Info("System", "Enter a valid defense name."));
            }
            else
            {
                var result = _characterService.AddDefense(model);

                if (result != null)
                    _rollHistory.Insert(0, result);
            }

            return RedirectToAction("Sheet");
        }

        [HttpPost]
        public IActionResult RemoveDefense(DefenseType type, string name)
        {
            _characterService.RemoveDefense(type, name);

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
        public IActionResult RollQuickDie(int sides)
        {
            int[] supportedDice = [4, 6, 8, 10, 12, 20, 100];

            if (!supportedDice.Contains(sides))
            {
                _rollHistory.Insert(0,
                    RollResult.Info("Dice Roller", "Unsupported dice type."));

                return RedirectToAction("Sheet");
            }

            var result = _diceService.RollDiceDetailed($"1d{sides}");

            result.Actor = "Dice Roller";
            result.Type = RollType.Dice;
            result.Description = $"Dice roll for d{sides}.";

            if (sides == 20)
            {
                result.NaturalRoll = result.DiceRoll;
            }

            _rollHistory.Insert(0, result);

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
        public IActionResult RollHitDie()
        {
            var result = _characterService.RollHitDie();

            if (result != null)
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
            var result = _characterService.AddInventoryItem(model);

            if (result != null)
                _rollHistory.Insert(0, result);

            return RedirectToAction("Sheet");
        }

        [HttpPost]
        public IActionResult RemoveInventoryItem(Guid itemId)
        {
            _characterService.RemoveInventoryItem(itemId);

            return RedirectToAction("Sheet");
        }

        [HttpPost]
        public IActionResult UpdateInventoryItemQuantity(
            Guid itemId,
            int quantity,
            int adjustment = 0)
        {
            _characterService.UpdateInventoryItemQuantity(
                itemId,
                quantity + adjustment);

            return RedirectToAction("Sheet");
        }

        [HttpPost]
        public IActionResult UpdateCurrency(UpdateCurrencyViewModel model)
        {
            var result = _characterService.UpdateCurrency(model);

            if (result != null)
                _rollHistory.Insert(0, result);

            return RedirectToAction("Sheet");
        }
        [HttpPost]
        public IActionResult EquipWeapon(Guid weaponId)
        {
            var result = _characterService.EquipWeapon(weaponId);

            if (result != null)
                _rollHistory.Insert(0, result);

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
            var result = _characterService.EquipArmor(armorId);

            if (result != null)
                _rollHistory.Insert(0, result);

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
            var result = _characterService.EquipShield(shieldId);

            if (result != null)
                _rollHistory.Insert(0, result);

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
        public IActionResult ShortRest()
        {
            var result = _characterService.ShortRest();

            if (result != null)
                _rollHistory.Insert(0, result);

            return RedirectToAction("Sheet");
        }

        [HttpPost]
        public IActionResult LongRest()
        {
            var result = _characterService.LongRest();

            if (result != null)
                _rollHistory.Insert(0, result);

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
            var result = _characterService.SetSavingThrowProficiency(abilityType, isProficient);

            if (result != null)
                _rollHistory.Insert(0, result);

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
