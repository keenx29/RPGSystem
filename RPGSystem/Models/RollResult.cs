using Microsoft.VisualBasic;

namespace RPGSystem.Models
{
    public class RollResult
    {
        public string Actor { get; set; } = "";

        public RollType Type { get; set; }

        public int DiceRoll { get; set; }

        public int Modifier { get; set; }

        public int Total => DiceRoll + Modifier;

        public string? DamageType { get; set; }
        public AdvantageState AdvantageType { get; set; } = AdvantageState.Normal;
    }
}
