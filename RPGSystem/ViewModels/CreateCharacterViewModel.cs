using System.ComponentModel.DataAnnotations;
using RPGSystem.Models.Classes;

namespace RPGSystem.ViewModels
{
    public class CreateCharacterViewModel
    {
        [Required(ErrorMessage = "Character name is required.")]
        [StringLength(50, ErrorMessage = "Character name cannot be longer than 50 characters.")]
        public string Name { get; set; } = "";

        public CharacterClassType ClassType { get; set; }

        [Required(ErrorMessage = "Choose a race.")]
        [StringLength(50, ErrorMessage = "Race cannot be longer than 50 characters.")]
        public string Race { get; set; } = "";

        [Required(ErrorMessage = "Choose a background.")]
        [StringLength(50, ErrorMessage = "Background cannot be longer than 50 characters.")]
        public string Background { get; set; } = "";
        public IFormFile? PortraitFile { get; set; }
    }
}