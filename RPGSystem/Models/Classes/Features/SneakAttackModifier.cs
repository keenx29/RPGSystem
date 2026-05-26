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
                return new RollModification();

            if (context.Weapon == null)
                return new RollModification();

            bool isValidWeapon =
                context.Weapon.ScalingType == WeaponScalingType.Dexterity ||
                context.Weapon.ScalingType == WeaponScalingType.Finesse;

            if (!isValidWeapon)
                return new RollModification();

            return new RollModification
            {
                ExtraDice = $"{_dice}d6",
                Source = RogueFeatures.SneakAttack,
                Description = $"{_dice}d6 sneak attack"
            };
        }
    }
}
