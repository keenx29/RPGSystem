using RPGSystem.Models.Classes.Features;

namespace RPGSystem.Models.Classes
{
    public class MonkClass : CharacterClass
    {
        public override CharacterClassType Type =>
            CharacterClassType.Monk;
        public override int HitDie => 8;

        public override List<ClassFeatureInstance> GetFeaturesForLevel(int level)
        {
            var features = new List<ClassFeatureInstance>();

            if (level >= 2)
            {
                features.Add(new ClassFeatureInstance
                {
                    Name = MonkFeatures.FlurryOfBlows,
                    ActionType = FeatureActionType.Use
                });

                features.Add(new ClassFeatureInstance
                {
                    Name = MonkFeatures.PatientDefense,
                    ActionType = FeatureActionType.Use
                });

                features.Add(new ClassFeatureInstance
                {
                    Name = MonkFeatures.StepOfTheWind,
                    ActionType = FeatureActionType.Use
                });
            }

            return features;
        }
    }
}
