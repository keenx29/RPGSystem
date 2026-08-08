using RPGSystem.Models.Characters;
using RPGSystem.Models.Rolls;

namespace RPGSystem.ViewModels
{
    public class CharacterSheetViewModel
    {
        public List<Character> AvailableCharacters { get; set; } = new();
        public Guid SelectedCharacterId { get; set; }
        public Character Character { get; set; } = new();
        public int HitDie { get; set; }
        public List<RollResult> RollHistory { get; set; } = new();
        public AdvantageState SelectedAdvantageState { get; set; } = AdvantageState.Normal;
    }
}
