using RPGSystem.Models.Characters;
using RPGSystem.Models.Rolls;
using RPGSystem.Services;

namespace RPGSystem.Models.Classes.Features.Actions;

public class ActionSurgeAction : IFeatureAction
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
            Description = "You gain one additional action on your turn.",
            AppliedEffects = new List<string>
            {
                FighterFeatures.ActionSurge,
                "Free Action"
            },
            Explanations = new List<RollExplanation>
            {
                new RollExplanation
                {
                    Type = RollExplanationType.Feature,
                    Source = FighterFeatures.ActionSurge,
                    Text = "Action Surge lets the fighter take one additional action on their turn."
                }
            }
        };
    }
}