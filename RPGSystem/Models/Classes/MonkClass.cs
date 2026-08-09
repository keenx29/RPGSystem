using RPGSystem.Models.Characters;
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
            if (level >= 1)
            {
                features.Add(new ClassFeatureInstance
                {
                    Name = MonkFeatures.MartialArts,
                    ActionType = FeatureActionType.None,
                    ResetType = FeatureResetType.None
                });
            }
            if (level >= 2)
            {
                features.Add(new ClassFeatureInstance
                {
                    Name = MonkFeatures.UnarmoredMovement,
                    ActionType = FeatureActionType.None,
                    ResetType = FeatureResetType.None
                });

                features.Add(new ClassFeatureInstance
                {
                    Name = MonkFeatures.FlurryOfBlows,
                    ActionType = FeatureActionType.ResourceUse,
                    ResourceName = "Ki",
                    ResourceCost = 1
                });

                features.Add(new ClassFeatureInstance
                {
                    Name = MonkFeatures.PatientDefense,
                    ActionType = FeatureActionType.ResourceUse,
                    ResourceName = "Ki",
                    ResourceCost = 1
                });

                features.Add(new ClassFeatureInstance
                {
                    Name = MonkFeatures.StepOfTheWind,
                    ActionType = FeatureActionType.ResourceUse,
                    ResourceName = "Ki",
                    ResourceCost = 1
                });
            }

            return features;
        }
        public override int? GetUnarmoredArmorClass(Character character)
        {
            int dex = character.GetAbility(AbilityType.Dexterity).Modifier;
            int wis = character.GetAbility(AbilityType.Wisdom).Modifier;

            return 10 + dex + wis;
        }
    }
}
