using RPGSystem.Models.Characters;

namespace RPGSystem.Models.Items
{
    public class Weapon : Item
    {
        public string DamageDice { get; set; } = "1d6";

        public string DamageType { get; set; } = "slashing";

        public int AttackBonus { get; set; }
        public WeaponScalingType ScalingType { get; set; } = WeaponScalingType.Strength;
        public WeaponProficiencyType ProficiencyType { get; set; }

        public string? ProficiencyName { get; set; }
    }
}