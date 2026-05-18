using RPGSystem.Models;

namespace RPGSystem.Services
{
    public class RollStateService
    {
        public AdvantageState SelectedAdvantageState { get; set; } = AdvantageState.Normal;
    }
}
