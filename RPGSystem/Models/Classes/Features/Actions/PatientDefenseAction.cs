using RPGSystem.Models.Characters;
using RPGSystem.Models.Rolls;
using RPGSystem.Services;

namespace RPGSystem.Models.Classes.Features.Actions;

public class PatientDefenseAction : IFeatureAction
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
            Description = "Spent 1 Ki to take the Dodge action as a bonus action.",
            AppliedEffects = new List<string>
            {
                MonkFeatures.PatientDefense,
                "Bonus Action"
            },
            Explanations = new List<RollExplanation>
            {
                new RollExplanation
                {
                    Type = RollExplanationType.Feature,
                    Source = MonkFeatures.PatientDefense,
                    Text = "Patient Defense lets the monk spend 1 Ki to take the Dodge action as a bonus action."
                }
            }
        };
    }
}