using RPGSystem.Models;

namespace RPGSystem.Services
{
    public class CharacterService
    {
        public Character GetTestCharacter()
        {
            return new Character
            {
                Name = "Thorin",

                Level = 3,

                Strength = 16,
                Dexterity = 12,
                Constitution = 14,

                Intelligence = 10,
                Wisdom = 8,
                Charisma = 13,

                ArmorClass = 15,

                MaxHP = 28,
                CurrentHP = 28,

                Skills = new List<Skill>
                {
                    new Skill
                    {
                        Name = "Athletics",
                        RelatedAbility = AbilityType.Strength,
                        IsProficient = true
                    },

                    new Skill
                    {
                        Name = "Perception",
                        RelatedAbility = AbilityType.Wisdom,
                        IsProficient = false
                    },

                    new Skill
                    {
                        Name = "Stealth",
                        RelatedAbility = AbilityType.Dexterity,
                        IsProficient = true
                    }
                }
            };
        }
    }
}