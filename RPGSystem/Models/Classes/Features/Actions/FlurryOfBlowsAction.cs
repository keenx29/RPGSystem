using RPGSystem.Models.Characters;
using RPGSystem.Models.Rolls;
using RPGSystem.Services;

namespace RPGSystem.Models.Classes.Features.Actions;

public class FlurryOfBlowsAction : IFeatureAction
{
    public RollResult Execute(
        Character character,
        ClassFeatureInstance feature,
        DiceService diceService)
    {
        return new RollResult
        {
            Actor = feature.Name,
            Type = RollType.Feature,
            DiceRoll = 0,
            Modifier = 0,
            Description = "Spent 1 Ki to make two unarmed strikes as a bonus action after taking the Attack action.",
            AppliedEffects = new List<string>
            {
                MonkFeatures.FlurryOfBlows,
                "Bonus Action"
            },
            Explanations = new List<RollExplanation>
            {
                new RollExplanation
                {
                    Type = RollExplanationType.Feature,
                    Source = MonkFeatures.FlurryOfBlows,
                    Text = "Flurry of Blows lets the monk spend 1 Ki to make two unarmed strikes as a bonus action after taking the Attack action."
                }
            }
        };
    }
}