using RPGSystem.Models.Classes.Features;

namespace RPGSystem.Data.Entities
{
    public class FeatureResourceEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid CharacterId { get; set; }

        public CharacterEntity Character { get; set; } = null!;

        public string Name { get; set; } = "";

        public int Current { get; set; }

        public int Max { get; set; }

        public FeatureResetType ResetType { get; set; }
    }
}