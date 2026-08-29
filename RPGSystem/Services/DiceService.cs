using Microsoft.AspNetCore.Mvc;
using RPGSystem.Models.Rolls;

namespace RPGSystem.Services
{
    public class DiceService
    {
        private readonly Random _random = new();
        public int RollD20(AdvantageState advantage = AdvantageState.Normal)
        {
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
                total += _random.Next(1, sides + 1);
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
            if (!IsValidDiceNotation(diceExpression))
                return diceExpression;

            var parts = diceExpression.ToLower().Split('d');
            int diceCount = int.Parse(parts[0]);
            int sides = int.Parse(parts[1]);

            return $"{diceCount * 2}d{sides}";
        }
        public bool IsValidDiceNotation(string? notation)
        {
            if (string.IsNullOrWhiteSpace(notation))
                return false;

            var parts = notation.ToLower().Split('d');

            if (parts.Length != 2)
                return false;

            if (!int.TryParse(parts[0], out int count))
                return false;

            if (!int.TryParse(parts[1], out int sides))
                return false;

            return count > 0 &&
                   count <= 20 &&
                   sides > 0 &&
                   sides <= 100;
        }

    }
}

