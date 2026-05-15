namespace RPGSystem.Models
{
    public class RollResult
    {
        public int DiceRoll { get; set; }
        public string RollType { get; set; } = "";

        public int Modifier { get; set; }

        public int Total => DiceRoll + Modifier;

        public string StatName { get; set; } = "";
    }
}
