using RPGSystem.Models.Rolls;

namespace RPGSystem.Models.Classes.Features
{
    public class SneakAttackModifier : ICombatModifier
    {
        private readonly int _dice;

        public SneakAttackModifier(int dice)
        {
            _dice = dice;
        }

        public RollModification Apply(RollContext context)
        {
            return new RollModification
            {
                ExtraDice = $"{_dice}d6"
            };
        }
    }
}
