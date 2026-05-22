using RPGSystem.Models.Characters;
using RPGSystem.Services;

namespace RPGSystem.Models.Items
{
    public class EffectContext
    {
        public Character Character { get; set; }
        public DiceService DiceService { get; set; }
    }
}
