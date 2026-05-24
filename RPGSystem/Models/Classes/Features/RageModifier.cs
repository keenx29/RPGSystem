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
        //TODO: Weapon and WeaponsScalingType validation
        if (context.Type != RollType.Damage)
            return new RollModification();

        if (context.Character.EquippedWeapon == null)
            return new RollModification();

        bool isStrengthWeapon =
            context.Character.EquippedWeapon.ScalingType == WeaponScalingType.Strength;

        if (!isStrengthWeapon)
            return new RollModification();

        return new RollModification
        {
            FlatBonus = _damageBonus,
        };
    }
}
}
