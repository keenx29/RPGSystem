namespace RPGSystem.Models
{
    public class RollResult
    {
        public string Title { get; set; } = "";
        public int DiceRoll { get; set; }
        public string RollType { get; set; } = "";

        public int Modifier { get; set; }

        public int Total => DiceRoll + Modifier;
        public string? WeaponName { get; set; }
    }
}
