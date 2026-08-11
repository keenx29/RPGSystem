using RPGSystem.Models.Characters;
using RPGSystem.Models.Classes.Features;
using RPGSystem.Models.Classes.Features.Actions;

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
                    ResetType = FeatureResetType.None,
                    ActivationType = FeatureActivationType.Passive,
                    Details = "..."
                });
            }
            if (level >= 2)
            {
                features.Add(new ClassFeatureInstance
                {
                    Name = MonkFeatures.UnarmoredMovement,
                    ActionType = FeatureActionType.None,
                    ResetType = FeatureResetType.None,
                    ActivationType = FeatureActivationType.Passive,
                    Details = "Your speed increases by 10 ft while you are not wearing armor or wielding a shield."
                });

                features.Add(new ClassFeatureInstance
                {
                    Name = MonkFeatures.FlurryOfBlows,
                    ActionType = FeatureActionType.ResourceUse,
                    ResourceName = FeatureResourceNames.Ki,
                    ResourceCost = 1,
                    Action = new FlurryOfBlowsAction(),
                    ActivationType = FeatureActivationType.BonusAction,
                    Details = "Spend 1 Ki to make two unarmed strikes as a bonus action after taking the Attack action.",
                });

                features.Add(new ClassFeatureInstance
                {
                    Name = MonkFeatures.PatientDefense,
                    ActionType = FeatureActionType.ResourceUse,
                    ResourceName = FeatureResourceNames.Ki,
                    ResourceCost = 1,
                    Action = new PatientDefenseAction(),
                    ActivationType = FeatureActivationType.BonusAction,
                    Details = "Spend 1 Ki to take the Dodge action as a bonus action.",
                });

                features.Add(new ClassFeatureInstance
                {
                    Name = MonkFeatures.StepOfTheWind,
                    ActionType = FeatureActionType.ResourceUse,
                    ResourceName = FeatureResourceNames.Ki,
                    ResourceCost = 1,
                    Action = new StepOfTheWindAction(),
                    ActivationType = FeatureActivationType.BonusAction,
                    Details = "Spend 1 Ki to Dash or Disengage as a bonus action.",
                });
            }

            return features;
        }
        public override List<FeatureResource> GetResourcesForLevel(int level)
        {
            var resources = new List<FeatureResource>();

            if (level >= 2)
            {
                resources.Add(new FeatureResource
                {
                    Name = FeatureResourceNames.Ki,
                    Current = level,
                    Max = level,
                    ResetType = FeatureResetType.ShortRest
                });
            }

            return resources;
        }
        public override int? GetUnarmoredArmorClass(Character character)
        {
            int dex = character.GetAbility(AbilityType.Dexterity).Modifier;
            int wis = character.GetAbility(AbilityType.Wisdom).Modifier;

            return 10 + dex + wis;
        }
    }
}
