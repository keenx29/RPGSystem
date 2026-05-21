using RPGSystem.Models.Classes.Features;

namespace RPGSystem.Models.Classes
{
    public class FighterClass : CharacterClass
    {
        public override CharacterClassType Type =>
            CharacterClassType.Fighter;

        public override int HitDie => 10;

        public override List<string> GetFeaturesForLevel(int level)
        {
            var features = new List<string>();

            if (level >= 1)
                features.Add(FighterFeatures.SecondWind);

            if (level >= 2)
                features.Add(FighterFeatures.ActionSurge);

            if (level >= 5)
                features.Add(FighterFeatures.ExtraAttack);

            return features;
        }
    }
}
