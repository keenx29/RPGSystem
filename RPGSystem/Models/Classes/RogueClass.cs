using RPGSystem.Models.Classes.Features;

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
                    ResetType = FeatureResetType.None
                });
            }

            return features;
        }
        public int GetSneakAttackDice(int level)
        {
            return ((level - 1) / 2) + 1;
        }
    }
}
