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
                StatName = statName
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
                StatName = skill.Name
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

            int d20 = _diceService.RollD20();

            int abilityMod = character.GetStrengthModifier(); // TODO: STR/DEX logic
            int prof = character.GetProficiencyBonus();// TODO: Weapon Proficiencies
            int weaponBonus = weapon.AttackBonus;

            int attackTotal = d20 + abilityMod + prof + weaponBonus;

            // Damage roll (basic)
            int damageRoll = _diceService.RollDice(weapon.DamageDice);
            int damageTotal = damageRoll + abilityMod;// TODO: Damage Bonuses (i.e. Rage)

            // Attack result
            _rollHistory.Insert(0, new RollResult
            {
                StatName = $"{weapon.Name} Attack",
                DiceRoll = d20,
                Modifier = abilityMod + prof + weaponBonus,
                RollType = "Attack"
            });

            // Damage result
            _rollHistory.Insert(0, new RollResult
            {
                StatName = $"{weapon.Name} Damage",
                DiceRoll = damageRoll,
                Modifier = abilityMod,
                RollType = "Damage"
            });

            CharacterSheetViewModel vm = new CharacterSheetViewModel
            {
                Character = character,
                RollHistory = _rollHistory
            };

            return View("Sheet", vm);
        }
    }
}
