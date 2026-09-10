namespace RPGSystem.Models.Characters
{
    public static class DefenseOptionCatalog
    {
        public static IReadOnlyList<string> DamageTypes { get; } =
        [
            "Acid",
            "Bludgeoning",
            "Cold",
            "Fire",
            "Force",
            "Lightning",
            "Necrotic",
            "Piercing",
            "Poison",
            "Psychic",
            "Radiant",
            "Slashing",
            "Thunder"
        ];

        public static bool IsValidOption(
            DefenseType type,
            string name)
        {
            if (type == DefenseType.ConditionImmunity)
            {
                return Enum.TryParse<ConditionType>(
                    name,
                    true,
                    out _);
            }

            return DamageTypes.Contains(
                name,
                StringComparer.OrdinalIgnoreCase);
        }
    }
}