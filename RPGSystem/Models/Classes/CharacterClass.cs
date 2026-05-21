using RPGSystem.Models.Classes.Features;

namespace RPGSystem.Models.Classes
{
    public abstract class CharacterClass
    {
        public abstract CharacterClassType Type { get; }

        public abstract int HitDie { get; }

        public abstract List<ClassFeatureInstance> GetFeaturesForLevel(int level);
    }
}
