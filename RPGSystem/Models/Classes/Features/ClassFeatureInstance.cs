using RPGSystem.Models.Rolls;

namespace RPGSystem.Models.Classes.Features
{
    public class ClassFeatureInstance
    {
        public string Name { get; set; } = "";

        public int UsesRemaining { get; set; }

        public int MaxUses { get; set; }
        public string? ResourceName { get; set; }
        public int ResourceCost { get; set; }
        public FeatureResetType ResetType { get; set; } = FeatureResetType.None;
        public bool IsAvailable => MaxUses == 0 || UsesRemaining > 0;
        public bool IsActive { get; set; }
        public ICombatModifier? Modifier { get; set; }
        public FeatureActionType ActionType { get; set; }

    }
}
