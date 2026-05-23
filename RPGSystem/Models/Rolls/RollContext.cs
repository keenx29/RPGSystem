using RPGSystem.Models.Characters;
using RPGSystem.Models.Items;

namespace RPGSystem.Models.Rolls
{
    public class RollContext
    {
        public Character Character { get; set; }
        public RollType Type { get; set; }
    }
}
