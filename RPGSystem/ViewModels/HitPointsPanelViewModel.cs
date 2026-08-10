using RPGSystem.Models.Characters;

namespace RPGSystem.ViewModels
{
    public class HitPointsPanelViewModel
    {
        public Character Character { get; set; } = new();
        public int HitDie { get; set; }
    }
}