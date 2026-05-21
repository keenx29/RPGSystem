namespace RPGSystem.Models.Classes.Features
{
    public class ClassFeatureInstance
    {
        public string Name { get; set; } = "";

        public int UsesRemaining { get; set; }

        public int MaxUses { get; set; }

        public bool IsAvailable => UsesRemaining > 0;
    }
}
