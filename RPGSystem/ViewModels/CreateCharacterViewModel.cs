using RPGSystem.Models.Characters;
using RPGSystem.Models.Classes;
using System.ComponentModel.DataAnnotations;

namespace RPGSystem.ViewModels
{
    public class CreateCharacterViewModel
    {
        [Required(ErrorMessage = "Character name is required.")]
        [StringLength(50, ErrorMessage = "Character name cannot be longer than 50 characters.")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Choose a race.")]
        [StringLength(50, ErrorMessage = "Race cannot be longer than 50 characters.")]
        public string Race { get; set; } = "";

        [Required(ErrorMessage = "Choose a background.")]
        [StringLength(50, ErrorMessage = "Background cannot be longer than 50 characters.")]
        public string Background { get; set; } = "";

        public CharacterClassType ClassType { get; set; }
        public AbilityScoreMode AbilityScoreMode { get; set; } =
            AbilityScoreMode.ClassTemplate;
        public int? StrengthScore { get; set; }
        public int? DexterityScore { get; set; }
        public int? ConstitutionScore { get; set; }
        public int? IntelligenceScore { get; set; }
        public int? WisdomScore { get; set; }
        public int? CharismaScore { get; set; }

        public IReadOnlyList<int?> GetSelectedAbilityScores()
        {
            return
            [
                StrengthScore,
                DexterityScore,
                ConstitutionScore,
                IntelligenceScore,
                WisdomScore,
                CharismaScore
            ];
        }

        public IFormFile? PortraitFile { get; set; }
    }
}