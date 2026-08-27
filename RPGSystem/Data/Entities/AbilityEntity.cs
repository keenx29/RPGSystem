using RPGSystem.Models.Characters;

namespace RPGSystem.Data.Entities
{
    public class AbilityEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid CharacterId { get; set; }

        public CharacterEntity Character { get; set; } = null!;

        public string Name { get; set; } = "";

        public AbilityType Type { get; set; }

        public int Score { get; set; }

        public bool IsSavingThrowProficient { get; set; }
    }
}