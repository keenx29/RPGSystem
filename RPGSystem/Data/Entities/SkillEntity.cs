using RPGSystem.Models.Characters;

namespace RPGSystem.Data.Entities
{
    public class SkillEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid CharacterId { get; set; }

        public CharacterEntity Character { get; set; } = null!;

        public string Name { get; set; } = "";

        public SkillType Type { get; set; }

        public AbilityType RelatedAbilityType { get; set; }

        public bool IsProficient { get; set; }

        public bool IsExpertise { get; set; }
    }
}