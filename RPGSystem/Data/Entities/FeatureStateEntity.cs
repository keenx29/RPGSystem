namespace RPGSystem.Data.Entities
{
    public class FeatureStateEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid CharacterId { get; set; }

        public CharacterEntity Character { get; set; } = null!;

        public string FeatureName { get; set; } = "";

        public int UsesRemaining { get; set; }

        public bool IsActive { get; set; }
    }
}