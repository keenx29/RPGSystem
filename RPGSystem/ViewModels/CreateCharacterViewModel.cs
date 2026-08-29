using RPGSystem.Models.Classes;

namespace RPGSystem.ViewModels
{
    public class CreateCharacterViewModel
    {
        public string Name { get; set; } = "";

        public CharacterClassType ClassType { get; set; }

        public string Race { get; set; } = "";

        public string Background { get; set; } = "";
    }
}