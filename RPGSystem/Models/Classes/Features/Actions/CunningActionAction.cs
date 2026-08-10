using RPGSystem.Models.Characters;
using RPGSystem.Models.Rolls;
using RPGSystem.Services;

namespace RPGSystem.Models.Classes.Features.Actions;

public class CunningActionAction : IFeatureAction
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
            Description = "You can take Dash, Disengage, or Hide as a bonus action.",
            AppliedEffects = new List<string>
            {
                RogueFeatures.CunningAction,
                "Bonus Action"
            },
            Explanations = new List<RollExplanation>
            {
                new RollExplanation
                {
                    Type = RollExplanationType.Feature,
                    Source = RogueFeatures.CunningAction,
                    Text = "Cunning Action lets the rogue use Dash, Disengage, or Hide as a bonus action."
                }
            }
        };
    }
}