using RPGSystem.Models.Classes;

namespace RPGSystem.Models.Characters
{
    public static class CharacterPortraitCatalog
    {
        public static string GetDefaultPortraitPath(
            CharacterClassType classType)
        {
            return classType switch
            {
                CharacterClassType.Fighter =>
                    "/images/portraits/fighter-default.jpg",

                CharacterClassType.Rogue =>
                    "/images/portraits/rogue-default.jpg",

                CharacterClassType.Barbarian =>
                    "/images/portraits/barbarian-default.jpg",

                CharacterClassType.Monk =>
                    "/images/portraits/monk-default.jpg",

                _ => "/images/portraits/fighter-default.jpg"
            };
        }
    }
}