using RPGSystem.Models;

namespace RPGSystem.Services
{
    public class RollService
    {
        private readonly DiceService _diceService;
        private readonly CharacterService _characterService;
        private Character Character => _characterService.GetCharacter();
        public RollService(DiceService diceService,CharacterService characterService)
        {
            _diceService = diceService;
            _characterService = characterService;
        }
        public RollResult RollAbility(AbilityType type, AdvantageState adv)
        {
            var ability = Character.GetAbility(type);

            int roll = _diceService.RollD20(adv);

            return new RollResult
            {
                Actor = ability.Name,
                Type = RollType.Check,
                DiceRoll = roll,
                Modifier = ability.Modifier
            };
        }
        public RollResult RollSavingThrow(AbilityType type, AdvantageState adv)
        {
            var ability = Character.GetAbility(type);

            int roll = _diceService.RollD20(adv);

            return new RollResult
            {
                Actor = ability.Name,
                Type = RollType.Save,
                DiceRoll = roll,
                Modifier = Character.GetSavingThrowBonus(ability)
            };
        }
        public RollResult RollSkill(string skillName, AdvantageState adv)
        {
            //TODO: Add SkillType enum to use instead of string lookup
            var skill = Character.Skills.First(s => s.Name == skillName);

            int roll = _diceService.RollD20(adv);

            return new RollResult
            {
                Actor = skill.Name,
                Type = RollType.Check,
                DiceRoll = roll,
                Modifier = skill.GetBonus(Character.GetProficiencyBonus())
            };
        }
        public RollResult RollAttack(AdvantageState adv)
        {
            // TODO: STR/DEX logic // TODO: Weapon Proficiencies
            var weapon = Character.EquippedWeapon;

            int roll = _diceService.RollD20(adv);

            int modifier =
                Character.GetAbility(AbilityType.Strength).Modifier +
                Character.GetProficiencyBonus() +
                weapon.AttackBonus;

            return new RollResult
            {
                Actor = weapon.Name,
                Type = RollType.Attack,
                DiceRoll = roll,
                Modifier = modifier
            };
        }
        public RollResult RollDamage()
        {
            // TODO: Damage Bonuses (i.e. Rage)
            // TODO: Damage mod based on ability (i.e. Dex for rogues)
            var weapon = Character.EquippedWeapon;

            int roll = _diceService.RollDice(weapon.DamageDice);

            int modifier = Character.GetAbility(AbilityType.Strength).Modifier;

            return new RollResult
            {
                Actor = weapon.Name,
                Type = RollType.Damage,
                DiceRoll = roll,
                Modifier = modifier,
                DamageType = weapon.DamageType
            };
        }
    }
}
