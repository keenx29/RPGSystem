using Microsoft.VisualBasic;

namespace RPGSystem.Models.Rolls
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
        public string Formula { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string> AppliedEffects { get; set; } = new();
        public Guid? SourceItemId { get; set; }
        public bool CanRollDamage { get; set; }

    }
}
