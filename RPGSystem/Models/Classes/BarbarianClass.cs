using RPGSystem.Models.Characters;
using RPGSystem.Models.Classes.Features;
using RPGSystem.Models.Items;

namespace RPGSystem.Models.Classes
{
    public class BarbarianClass : CharacterClass
    {
        public override CharacterClassType Type =>
            CharacterClassType.Barbarian;
        public override int HitDie => 12;

        public override List<ClassFeatureInstance> GetFeaturesForLevel(int level)
        {
            var features = new List<ClassFeatureInstance>();

            if (level >= 1)
            {
                features.Add(new ClassFeatureInstance
                {
                    Name = BarbarianFeatures.Rage,
                    ActionType = FeatureActionType.Toggle,
                    IsActive = false,
                    UsesRemaining = 2,
                    MaxUses = 2,
                    Modifier = new RageModifier(2),
                    ResetType = FeatureResetType.LongRest,
                    ActivationType = FeatureActivationType.BonusAction,
                    Details = "Toggle Rage. Adds damage to Strength-based melee attacks.",
                });
            }
            if (level >= 2)
            {
                features.Add(new ClassFeatureInstance
                {
                    Name = BarbarianFeatures.RecklessAttack,
                    ActionType = FeatureActionType.Toggle,
                    IsActive = false,
                    ResetType = FeatureResetType.None,
                    ActivationType = FeatureActivationType.FreeAction,
                    Details = "Toggle to gain advantage on Strength-based attack rolls.",
                });

                features.Add(new ClassFeatureInstance
                {
                    Name = BarbarianFeatures.DangerSense,
                    ActionType = FeatureActionType.None,
                    ResetType = FeatureResetType.None,
                    ActivationType = FeatureActivationType.Passive,
                    Details = "Advantage on Dexterity saving throws unless blinded, deafened, or incapacitated.",
                });
            }

            return features;
        }
        public override bool IsProficientWithWeapon(Weapon weapon)
        {
            return weapon.ProficiencyType == WeaponProficiencyType.Simple
                || weapon.ProficiencyType == WeaponProficiencyType.Martial;
        }
        public override int? GetUnarmoredArmorClass(Character character)
        {
            int dex = character.GetAbility(AbilityType.Dexterity).Modifier;
            int con = character.GetAbility(AbilityType.Constitution).Modifier;

            return 10 + dex + con;
        }
        public override IReadOnlyCollection<AbilityType> SavingThrowProficiencies =>
            new[] { AbilityType.Strength, AbilityType.Constitution };
        public override IReadOnlyCollection<SkillType> AvailableSkillProficiencies =>
            new[]
            {
                SkillType.AnimalHandling,
                SkillType.Athletics,
                SkillType.Intimidation,
                SkillType.Nature,
                SkillType.Perception,
                SkillType.Survival
            };
    }
}
