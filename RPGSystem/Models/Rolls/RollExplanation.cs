namespace RPGSystem.Models.Rolls
{
    public class RollExplanation
    {
        public RollExplanationType Type { get; set; }

        public string Source { get; set; } = "";

        public string Text { get; set; } = "";

        public int? Value { get; set; }

        public string? Dice { get; set; }
    }
}