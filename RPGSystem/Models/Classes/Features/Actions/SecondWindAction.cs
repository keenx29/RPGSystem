using RPGSystem.Models.Characters;
using RPGSystem.Models.Classes.Features;
using RPGSystem.Models.Rolls;
using RPGSystem.Services;

namespace RPGSystem.Models.Classes.Features.Actions;

public class SecondWindAction : IFeatureAction
{
    public RollResult Execute(
        Character character,
        ClassFeatureInstance feature,
        DiceService diceService)
    {
        var rollResult = diceService.RollDiceDetailed("1d10");

        int oldHp = character.CurrentHP;

        int healing = rollResult.DiceRoll + character.Level;

        character.CurrentHP = Math.Min(
            character.CurrentHP + healing,
            character.MaxHP);

        int actualHealing = character.CurrentHP - oldHp;

        rollResult.Actor = feature.Name;
        rollResult.Type = RollType.Heal;

        rollResult.Modifier = character.Level;

        rollResult.Formula = $"1d10 + {character.Level} Fighter level";

        rollResult.Description =
            $"{feature.Name} restored {actualHealing} HP.";

        rollResult.AppliedEffects.Add(FighterFeatures.SecondWind);
        rollResult.AppliedEffects.Add("Bonus Action");

        rollResult.Explanations.Add(
            new RollExplanation
            {
                Type = RollExplanationType.Feature,
                Source = FighterFeatures.SecondWind,
                Text = "Second Wind restores 1d10 + fighter level hit points.",
                Value = character.Level
            });
        return rollResult;
    }
}