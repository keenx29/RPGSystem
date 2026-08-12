namespace RPGSystem.Models.Characters
{
    public static class SkillFactory
    {
        public static List<Skill> CreateDefaultSkills(Character character)
        {
            return new List<Skill>
            {
                new Skill
                {
                    Name = "Acrobatics",
                    Type = SkillType.Acrobatics,
                    RelatedAbility = character.GetAbility(AbilityType.Dexterity)
                },
                new Skill
                {
                    Name = "Animal Handling",
                    Type = SkillType.AnimalHandling,
                    RelatedAbility = character.GetAbility(AbilityType.Wisdom)
                },
                new Skill
                {
                    Name = "Arcana",
                    Type = SkillType.Arcana,
                    RelatedAbility = character.GetAbility(AbilityType.Intelligence)
                },
                new Skill
                {
                    Name = "Athletics",
                    Type = SkillType.Athletics,
                    RelatedAbility = character.GetAbility(AbilityType.Strength)
                },
                new Skill
                {
                    Name = "Deception",
                    Type = SkillType.Deception,
                    RelatedAbility = character.GetAbility(AbilityType.Charisma)
                },
                new Skill
                {
                    Name = "History",
                    Type = SkillType.History,
                    RelatedAbility = character.GetAbility(AbilityType.Intelligence)
                },
                new Skill
                {
                    Name = "Insight",
                    Type = SkillType.Insight,
                    RelatedAbility = character.GetAbility(AbilityType.Wisdom)
                },
                new Skill
                {
                    Name = "Intimidation",
                    Type = SkillType.Intimidation,
                    RelatedAbility = character.GetAbility(AbilityType.Charisma)
                },
                new Skill
                {
                    Name = "Investigation",
                    Type = SkillType.Investigation,
                    RelatedAbility = character.GetAbility(AbilityType.Intelligence)
                },
                new Skill
                {
                    Name = "Medicine",
                    Type = SkillType.Medicine,
                    RelatedAbility = character.GetAbility(AbilityType.Wisdom)
                },
                new Skill
                {
                    Name = "Nature",
                    Type = SkillType.Nature,
                    RelatedAbility = character.GetAbility(AbilityType.Intelligence)
                },
                new Skill
                {
                    Name = "Perception",
                    Type = SkillType.Perception,
                    RelatedAbility = character.GetAbility(AbilityType.Wisdom)
                },
                new Skill
                {
                    Name = "Performance",
                    Type = SkillType.Performance,
                    RelatedAbility = character.GetAbility(AbilityType.Charisma)
                },
                new Skill
                {
                    Name = "Persuasion",
                    Type = SkillType.Persuasion,
                    RelatedAbility = character.GetAbility(AbilityType.Charisma)
                },
                new Skill
                {
                    Name = "Religion",
                    Type = SkillType.Religion,
                    RelatedAbility = character.GetAbility(AbilityType.Intelligence)
                },
                new Skill
                {
                    Name = "Sleight of Hand",
                    Type = SkillType.SleightOfHand,
                    RelatedAbility = character.GetAbility(AbilityType.Dexterity)
                },
                new Skill
                {
                    Name = "Stealth",
                    Type = SkillType.Stealth,
                    RelatedAbility = character.GetAbility(AbilityType.Dexterity)
                },
                new Skill
                {
                    Name = "Survival",
                    Type = SkillType.Survival,
                    RelatedAbility = character.GetAbility(AbilityType.Wisdom)
                }
            };
        }
    }
}