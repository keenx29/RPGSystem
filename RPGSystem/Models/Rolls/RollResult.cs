
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
        public List<RollExplanation> Explanations { get; set; } = new();
        public Guid? SourceItemId { get; set; }
        public int? NaturalRoll { get; set; }

        public bool IsCriticalSuccess =>
            Type == RollType.Attack && NaturalRoll == 20;

        public bool IsCriticalFailure =>
            Type == RollType.Attack && NaturalRoll == 1;
        public bool CanRollDamage => Type == RollType.Attack && SourceItemId.HasValue;
        public bool CanRollCriticalDamage => CanRollDamage && IsCriticalSuccess;
        public bool IsCriticalDamage { get; set; }
        public int? DiscardedD20Roll { get; set; }

        public bool IsNaturalTwenty => NaturalRoll == 20;

        public bool IsNaturalOne => NaturalRoll == 1;
        public static RollResult Info(string actor, string description)
        {
            return new RollResult
            {
                Actor = actor,
                Type = RollType.Feature,
                Description = description,
                DiceRoll = 0,
                NaturalRoll = 0,
                Modifier = 0
            };
        }
    }
}
