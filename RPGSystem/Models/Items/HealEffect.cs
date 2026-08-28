using RPGSystem.Models.Characters;
using RPGSystem.Models.Rolls;
using RPGSystem.Services;

namespace RPGSystem.Models.Items
{
    public class HealEffect : IItemEffect
    {
        public string Notation { get; }

        public HealEffect(string notation)
        {
            Notation = notation;
        }

        public RollResult Apply(EffectContext context)
        {
            var result = context.DiceService.RollDiceDetailed(Notation);

            context.Character.Heal(result.Total);

            result.Actor = "Healing Potion";

            result.Type = RollType.Heal;

            return result;
        }
    }
}
