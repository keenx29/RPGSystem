namespace RPGSystem.Models.Characters
{
    public static class CharacterCreationCatalog
    {
        public static IReadOnlyList<string> Races { get; } =
        [
            "Dwarf",
            "Elf",
            "Halfling",
            "Human",
            "Dragonborn",
            "Gnome",
            "Half-Elf",
            "Half-Orc",
            "Tiefling"
        ];

        public static IReadOnlyList<string> Backgrounds { get; } =
        [
            "Acolyte",
            "Charlatan",
            "Criminal",
            "Entertainer",
            "Folk Hero",
            "Guild Artisan",
            "Hermit",
            "Noble",
            "Outlander",
            "Sage",
            "Sailor",
            "Soldier",
            "Urchin"
        ];

        public static bool IsSupportedRace(string? race)
        {
            return Races.Contains(race, StringComparer.Ordinal);
        }

        public static bool IsSupportedBackground(string? background)
        {
            return Backgrounds.Contains(background, StringComparer.Ordinal);
        }
    }
}