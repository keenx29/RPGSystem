using Microsoft.AspNetCore.Mvc;
using RPGSystem.Models.Rolls;

namespace RPGSystem.Services
{
    public class DiceService
    {
        private readonly Random _random = new();
        public int RollD20(AdvantageState advantage = AdvantageState.Normal)
        {
            return 20;
            if (advantage == AdvantageState.Advantage)
            {
                int firstRoll = RollOnce();
                int secondRoll = RollOnce();

                return Math.Max(firstRoll, secondRoll);
            }

            if (advantage == AdvantageState.Disadvantage)
            {
                int firstRoll = RollOnce();
                int secondRoll = RollOnce();

                return Math.Min(firstRoll, secondRoll);
            }

            return RollOnce();
        }
        public int RollOnce()
        {
            return _random.Next(1, 21);
        }
        public int RollDice(string notation)
        {
            var parts = notation.ToLower().Split('d');

            int count = int.Parse(parts[0]);
            int sides = int.Parse(parts[1]);

            int total = 0;

            for (int i = 0; i < count; i++)
            {
                total += new Random().Next(1, sides + 1);
            }

            return total;
        }
        public RollResult RollDiceDetailed(string notation)
        {
            var parts = notation.Split('+', StringSplitOptions.RemoveEmptyEntries);

            int roll = RollDice(parts[0]);

            int modifier = 0;

            if (parts.Length > 1)
                modifier = int.Parse(parts[1]);

            return new RollResult
            {
                DiceRoll = roll,
                Modifier = modifier,
            };
        }
        public string DoubleDiceExpression(string diceExpression)
        {
            var parts = diceExpression.Split('d');

            if (parts.Length != 2)
                return diceExpression;

            if (!int.TryParse(parts[0], out int diceCount))
                return diceExpression;

            return $"{diceCount * 2}d{parts[1]}";
        }
    }
}

