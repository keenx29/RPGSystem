using RPGSystem.Models.Characters;
using RPGSystem.Models.Items;
using RPGSystem.Models.Rolls;

namespace RPGSystem.Models.Classes.Features
{
    public class RageModifier : ICombatModifier
    {
        private readonly int _damageBonus;

        public RageModifier(int damageBonus)
        {
            _damageBonus = damageBonus;
        }

        public RollModification Apply(RollContext context)
        {
            if (context.Type != RollType.Damage)
                return RollModification.None();

            if (context.Weapon == null)
            {
                return RollModification.Ignored(
                    BarbarianFeatures.Rage,
                    "Rage could not be applied because no weapon was found for this damage roll.");
            }

            if (context.Ability == null)
            {
                return RollModification.Ignored(
                    BarbarianFeatures.Rage,
                    "Rage could not be applied because the attack ability was not determined.");
            }

            if (context.Ability.Type != AbilityType.Strength)
            {
                return RollModification.Ignored(
                    BarbarianFeatures.Rage,
                    "Rage damage applies only when the attack uses Strength.");
            }

            return new RollModification
            {
                FlatBonus = _damageBonus,
                Source = BarbarianFeatures.Rage,
                Description = $"Rage adds +{_damageBonus} damage because the attack uses Strength."
            };
        }
    }
}