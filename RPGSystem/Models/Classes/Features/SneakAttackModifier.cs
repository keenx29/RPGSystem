using RPGSystem.Models.Items;
using RPGSystem.Models.Rolls;

namespace RPGSystem.Models.Classes.Features
{
    public class SneakAttackModifier : ICombatModifier
    {
        private readonly int _dice;

        public SneakAttackModifier(int dice)
        {
            _dice = dice;
        }

        public RollModification Apply(RollContext context)
        {
            if (context.Type != RollType.Damage)
                return RollModification.None();

            if (context.Weapon == null)
            {
                return RollModification.Ignored(
                    RogueFeatures.SneakAttack,
                    "Sneak Attack could not be applied because no weapon was found for this damage roll.");
            }

            bool isValidWeapon =
                context.Weapon.ScalingType == WeaponScalingType.Dexterity ||
                context.Weapon.ScalingType == WeaponScalingType.Finesse;

            if (!isValidWeapon)
            {
                return RollModification.Ignored(
                    RogueFeatures.SneakAttack,
                    "Sneak Attack requires a finesse or dexterity-based weapon in this simplified rules model.");
            }

            return new RollModification
            {
                ExtraDice = $"{_dice}d6",
                Source = RogueFeatures.SneakAttack,
                Description = $"Sneak Attack adds {_dice}d6 extra damage."
            };
        }
    }
}