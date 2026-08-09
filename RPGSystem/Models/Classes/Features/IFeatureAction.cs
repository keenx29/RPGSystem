using RPGSystem.Models.Characters;
using RPGSystem.Models.Rolls;
using RPGSystem.Services;

namespace RPGSystem.Models.Classes.Features
{
    public interface IFeatureAction
    {
        RollResult Execute(
            Character character,
            ClassFeatureInstance feature,
            DiceService diceService);
    }
}
