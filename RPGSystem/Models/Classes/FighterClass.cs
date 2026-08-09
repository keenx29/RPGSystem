using RPGSystem.Models.Classes.Features;
using RPGSystem.Models.Classes.Features.Actions;

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
                    MaxUses = 1,
                    ActionType = FeatureActionType.Use,
                    ResetType = FeatureResetType.ShortRest,
                    Action = new SecondWindAction()
                });

            if (level >= 2)
                features.Add(new ClassFeatureInstance
                {
                    Name = FighterFeatures.ActionSurge,
                    UsesRemaining = 1,
                    MaxUses = 1,
                    ActionType = FeatureActionType.Use,
                    ResetType = FeatureResetType.ShortRest,
                });

            if (level >= 5)
                features.Add(new ClassFeatureInstance
                {
                    Name = FighterFeatures.ExtraAttack,
                    UsesRemaining = 0,
                    MaxUses = 0,
                    ActionType = FeatureActionType.None,
                    ResetType = FeatureResetType.None
                });

            return features;
        }
    }
}
