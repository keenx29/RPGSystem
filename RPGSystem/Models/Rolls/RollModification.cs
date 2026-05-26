namespace RPGSystem.Models.Rolls
{
    public class RollModification
    {
        public int FlatBonus { get; set; }
        public string? ExtraDice { get; set; }
        public string Source { get; set; } = "";
        public string Description { get; set; } = "";

        public bool HasEffect =>
            FlatBonus != 0 ||
            !string.IsNullOrWhiteSpace(ExtraDice);
    }
}