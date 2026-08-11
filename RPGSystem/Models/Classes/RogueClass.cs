using RPGSystem.Models.Classes.Features;
using RPGSystem.Models.Classes.Features.Actions;
using RPGSystem.Models.Items;

namespace RPGSystem.Models.Classes
{
    public class RogueClass : CharacterClass
    {
        public override CharacterClassType Type =>
           CharacterClassType.Rogue;
        public override int HitDie => 8;

        public override List<ClassFeatureInstance> GetFeaturesForLevel(int level)
        {
            var features = new List<ClassFeatureInstance>();

            if (level >= 1)
            {
                features.Add(new ClassFeatureInstance
                {
                    Name = RogueFeatures.SneakAttack,
                    ActionType = FeatureActionType.Toggle,
                    IsActive = false,
                    Modifier = new SneakAttackModifier(GetSneakAttackDice(level)),
                });
            }
            if (level >= 2)
            {
                features.Add(new ClassFeatureInstance
                {
                    Name = RogueFeatures.CunningAction,
                    ActionType = FeatureActionType.Use,
                    ResetType = FeatureResetType.None,
                    Action = new CunningActionAction(),
                    ActivationType = FeatureActivationType.BonusAction,
                    Details = "Dash, Disengage, or Hide as a bonus action.",
                });
            }

            return features;
        }
        public int GetSneakAttackDice(int level)
        {
            return ((level - 1) / 2) + 1;
        }
        public override bool IsProficientWithWeapon(Weapon weapon)
        {
            if (weapon.ProficiencyType == WeaponProficiencyType.Simple)
            {
                return true;
            }

            var name = weapon.ProficiencyName ?? weapon.Name;

            return name == "Rapier"
                || name == "Shortsword"
                || name == "Longsword"
                || name == "Hand Crossbow";
        }
    }
}
