namespace RPGSystem.Models
{
    public class Character
    {
        public string Name { get; set; } = "";
        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Constitution { get; set; }
        public int Intelligence { get; set; }
        public int Wisdom { get; set; }
        public int Charisma { get; set; }
        public int GetStrengthModifier()
        {
            return (Strength - 10) / 2;
        }

        public int GetDexterityModifier()
        {
            return (Dexterity - 10) / 2;
        }

        public int GetConstitutionModifier()
        {
            return (Constitution - 10) / 2;
        }

        public int GetIntelligenceModifier()
        {
            return (Intelligence - 10) / 2;
        }

        public int GetWisdomModifier()
        {
            return (Wisdom - 10) / 2;
        }

        public int GetCharismaModifier()
        {
            return (Charisma - 10) / 2;
        }
    }
}

