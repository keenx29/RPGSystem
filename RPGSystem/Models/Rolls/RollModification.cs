namespace RPGSystem.Models.Rolls
{
    public class RollModification
    {
        public int FlatBonus { get; set; }
        public string? ExtraDice { get; set; }
        public string Source { get; set; } = "";
        public string Description { get; set; } = "";
        public string IgnoreReason { get; set; } = "";

        public bool HasEffect =>
            FlatBonus != 0 ||
            !string.IsNullOrWhiteSpace(ExtraDice);

        public bool WasIgnored =>
            !HasEffect && !string.IsNullOrWhiteSpace(IgnoreReason);

        public static RollModification None()
        {
            return new RollModification();
        }

        public static RollModification Ignored(string source, string reason)
        {
            return new RollModification
            {
                Source = source,
                IgnoreReason = reason
            };
        }
    }
}