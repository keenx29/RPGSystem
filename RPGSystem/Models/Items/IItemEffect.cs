using RPGSystem.Models.Characters;
using RPGSystem.Models.Rolls;

namespace RPGSystem.Models.Items
{
    public interface IItemEffect
    {
        RollResult? Apply(EffectContext context);
    }
}
