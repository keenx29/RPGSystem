using RPGSystem.Models.Items;

namespace RPGSystem.ViewModels
{
    public class AddInventoryItemViewModel
    {
        public string ItemKind { get; set; } = "General";

        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public double Weight { get; set; }
        public bool IsStackable { get; set; }
        public int Quantity { get; set; } = 1;
        public string? DamageDice { get; set; }
        public string? HealingDice { get; set; }

        public string? DamageType { get; set; }

        public WeaponScalingType ScalingType { get; set; } = WeaponScalingType.Strength;

        public WeaponProficiencyType WeaponProficiencyType { get; set; } = WeaponProficiencyType.Simple;
        public bool IsThrown { get; set; }

        public int? NormalRangeFeet { get; set; }

        public int? LongRangeFeet { get; set; }

        public ArmorType ArmorType { get; set; } = ArmorType.Light;

        public int BaseArmorClass { get; set; } = 10;

        public int AttackBonus { get; set; }

        public int ArmorBonus { get; set; }
    }
}