namespace RPGSystem.Models.Classes
{
    public abstract class CharacterClass
    {
        public abstract CharacterClassType Type { get; }

        public abstract int HitDie { get; }

        public abstract List<string> GetFeaturesForLevel(int level);
    }
}
