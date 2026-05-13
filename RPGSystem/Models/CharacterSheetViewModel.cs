namespace RPGSystem.Models
{
    public class CharacterSheetViewModel
    {
        public Character Character { get; set; } = new();
        public List<RollResult> RollHistory { get; set; } = new();
    }
}
