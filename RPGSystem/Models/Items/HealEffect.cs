using RPGSystem.Models.Characters;
using RPGSystem.Models.Rolls;
using RPGSystem.Services;

namespace RPGSystem.Models.Items
{
    public class HealEffect : IItemEffect
    {
        private readonly string _notation;

        public HealEffect(string notation)
        {
            _notation = notation;
        }

        public RollResult Apply(EffectContext context)
        {
            var result = context.DiceService.RollDiceDetailed(_notation);

            context.Character.Heal(result.Total);

            result.Actor = "Healing Potion";

            result.Type = RollType.Heal;

            return result;
        }
    }
}
