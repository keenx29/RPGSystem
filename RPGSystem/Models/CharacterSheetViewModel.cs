namespace RPGSystem.Models
{
    public class CharacterSheetViewModel
    {
        public Character Character { get; set; } = new();

        public RollResult? RollResult { get; set; }
    }
}
