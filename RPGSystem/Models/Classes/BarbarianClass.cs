using RPGSystem.Models.Classes.Features;

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
                    Modifier = new RageModifier(2)
                });
            }

            return features;
        }
    }
}
