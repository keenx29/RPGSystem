using RPGSystem.Models.Characters;
using RPGSystem.Models.Rolls;

namespace RPGSystem.ViewModels
{
    public class CharacterSheetViewModel
    {
        public Character Character { get; set; } = new();
        public List<RollResult> RollHistory { get; set; } = new();
        public AdvantageState SelectedAdvantageState { get; set; } = AdvantageState.Normal;
    }
}
