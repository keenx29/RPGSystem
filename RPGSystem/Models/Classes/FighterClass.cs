using RPGSystem.Models.Classes.Features;

namespace RPGSystem.Models.Classes
{
    public class FighterClass : CharacterClass
    {
        public override CharacterClassType Type =>
            CharacterClassType.Fighter;

        public override int HitDie => 10;

        public override List<ClassFeatureInstance> GetFeaturesForLevel(int level)
        {
            var features = new List<ClassFeatureInstance>();

            if (level >= 1)
                features.Add(new ClassFeatureInstance
                {
                    Name = FighterFeatures.SecondWind,
                    UsesRemaining = 1,
                    MaxUses = 1
                });

            if (level >= 2)
                features.Add(new ClassFeatureInstance
                {
                    Name = FighterFeatures.ActionSurge,
                    UsesRemaining = 1,
                    MaxUses = 1
                });

            if (level >= 5)
                features.Add(new ClassFeatureInstance
                {
                    Name = FighterFeatures.ExtraAttack,
                    UsesRemaining = 0,
                    MaxUses = 0
                });

            return features;
        }
    }
}
