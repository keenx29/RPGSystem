using RPGSystem.Models.Characters;

namespace RPGSystem.ViewModels
{
    public class AbilityCardViewModel
    {
        public Ability Ability { get; set; } = null!;

        public bool CanIncreaseScore { get; set; }
    }
}