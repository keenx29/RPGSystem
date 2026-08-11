using RPGSystem.Models.Characters;
using RPGSystem.Models.Classes.Features;
using RPGSystem.Models.Classes.Features.Actions;
using RPGSystem.Models.Items;

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
                    Action = new SecondWindAction(),
                    ActivationType = FeatureActivationType.BonusAction,
                    Details = "Regain 1d10 + Fighter level HP. Recharges on short rest.",
                });

            if (level >= 2)
                features.Add(new ClassFeatureInstance
                {
                    Name = FighterFeatures.ActionSurge,
                    UsesRemaining = 1,
                    MaxUses = 1,
                    ActionType = FeatureActionType.Use,
                    ResetType = FeatureResetType.ShortRest,
                    Action = new ActionSurgeAction(),
                    ActivationType = FeatureActivationType.FreeAction,
                    Details = "Gain one additional action on your turn. Recharges on short rest.",
                });

            if (level >= 5)
                features.Add(new ClassFeatureInstance
                {
                    Name = FighterFeatures.ExtraAttack,
                    UsesRemaining = 0,
                    MaxUses = 0,
                    ActionType = FeatureActionType.None,
                    ResetType = FeatureResetType.None,
                    ActivationType = FeatureActivationType.Passive,
                    Details = "When you take the Attack action, you can attack twice.",
                });

            return features;
        }
        public override bool IsProficientWithWeapon(Weapon weapon)
        {
            return weapon.ProficiencyType == WeaponProficiencyType.Simple
                || weapon.ProficiencyType == WeaponProficiencyType.Martial;
        }
        public override IReadOnlyCollection<AbilityType> SavingThrowProficiencies =>
            new[] { AbilityType.Strength, AbilityType.Constitution };
        public override IReadOnlyCollection<SkillType> AvailableSkillProficiencies =>
            new[]
            {
                SkillType.Acrobatics,
                SkillType.AnimalHandling,
                SkillType.Athletics,
                SkillType.History,
                SkillType.Insight,
                SkillType.Intimidation,
                SkillType.Perception,
                SkillType.Survival
            };
    }
}
