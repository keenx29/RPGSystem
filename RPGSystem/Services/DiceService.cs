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
            if (!TryParseDiceExpression(notation, out int count, out int sides, out int modifier))
                throw new ArgumentException("Invalid dice notation.", nameof(notation));

            return RollDiceBase(count, sides) + modifier;
        }
        public RollResult RollDiceDetailed(string notation)
        {
            if (!TryParseDiceExpression(notation, out int count, out int sides, out int modifier))
                throw new ArgumentException("Invalid dice notation.", nameof(notation));

            return new RollResult
            {
                DiceRoll = RollDiceBase(count, sides),
                Modifier = modifier,
                Formula = notation
            };
        }
        public string DoubleDiceExpression(string diceExpression)
        {
            if (!TryParseDiceExpression(diceExpression, out int count, out int sides, out int modifier))
                return diceExpression;

            string modifierText = modifier switch
            {
                > 0 => $"+{modifier}",
                < 0 => modifier.ToString(),
                _ => ""
            };

            return $"{count * 2}d{sides}{modifierText}";
        }

        public bool IsValidDiceNotation(string? notation)
        {
            return TryParseDiceExpression(notation, out _, out _, out _);
        }
        public string GetBaseDiceNotation(string notation)
        {
            if (!TryParseDiceExpression(notation, out int count, out int sides, out _))
                throw new ArgumentException("Invalid dice notation.", nameof(notation));

            return $"{count}d{sides}";
        }

        public int GetDiceModifier(string notation)
        {
            if (!TryParseDiceExpression(notation, out _, out _, out int modifier))
                throw new ArgumentException("Invalid dice notation.", nameof(notation));

            return modifier;
        }
        private bool TryParseDiceExpression(
            string? notation,
            out int count,
            out int sides,
            out int modifier)
        {
            count = 0;
            sides = 0;
            modifier = 0;

            if (string.IsNullOrWhiteSpace(notation))
                return false;

            var value = notation
                .Trim()
                .ToLowerInvariant()
                .Replace(" ", "");

            int dIndex = value.IndexOf('d');

            if (dIndex <= 0 || dIndex == value.Length - 1)
                return false;

            int modifierIndex = -1;

            for (int i = dIndex + 2; i < value.Length; i++)
            {
                if (value[i] == '+' || value[i] == '-')
                {
                    modifierIndex = i;
                    break;
                }
            }

            string countText = value[..dIndex];
            string sidesText = modifierIndex >= 0
                ? value[(dIndex + 1)..modifierIndex]
                : value[(dIndex + 1)..];

            string? modifierText = modifierIndex >= 0
                ? value[modifierIndex..]
                : null;

            if (!int.TryParse(countText, out count) ||
                !int.TryParse(sidesText, out sides))
            {
                return false;
            }

            if (modifierText != null &&
                !int.TryParse(modifierText, out modifier))
            {
                return false;
            }

            return count > 0 &&
                   count <= 20 &&
                   sides > 0 &&
                   sides <= 100;
        }

        private int RollDiceBase(int count, int sides)
        {
            int total = 0;

            for (int i = 0; i < count; i++)
            {
                total += _random.Next(1, sides + 1);
            }

            return total;
        }
    }
}

