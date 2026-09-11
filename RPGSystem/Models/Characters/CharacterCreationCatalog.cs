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
        public static IReadOnlyList<int> StandardArray { get; } =
    [15, 14, 13, 12, 10, 8];

        public static bool IsValidStandardArray(
            IReadOnlyList<int?> selectedScores)
        {
            if (selectedScores.Count != StandardArray.Count ||
                selectedScores.Any(score => !score.HasValue))
            {
                return false;
            }

            var selectedValues = selectedScores
                .Select(score => score!.Value)
                .OrderBy(score => score);

            return selectedValues.SequenceEqual(
                StandardArray.OrderBy(score => score));
        }
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