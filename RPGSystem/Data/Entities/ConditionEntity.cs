using RPGSystem.Models.Characters;

namespace RPGSystem.Data.Entities
{
    public class ConditionEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid CharacterId { get; set; }

        public CharacterEntity Character { get; set; } = null!;

        public ConditionType Type { get; set; }
    }
}