using RPGSystem.Models.Characters;

namespace RPGSystem.ViewModels
{
    public class CharacterSummaryPanelViewModel
    {
        public Character Character { get; set; } = new();
        public List<Character> AvailableCharacters { get; set; } = new();
        public Guid SelectedCharacterId { get; set; }
    }
}