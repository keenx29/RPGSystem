namespace RPGSystem.Models
{
    public class CharacterSheetViewModel
    {
        public Character Character { get; set; } = new();
        public List<RollResult> RollHistory { get; set; } = new();
        public AdvantageState SelectedAdvantageState { get; set; } = AdvantageState.Normal;
    }
}
