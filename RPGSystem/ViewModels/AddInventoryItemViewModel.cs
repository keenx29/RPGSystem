using RPGSystem.Models.Items;

namespace RPGSystem.ViewModels
{
    public class AddInventoryItemViewModel
    {
        public string ItemKind { get; set; } = "Item";

        public string Name { get; set; } = "";

        public ItemType Type { get; set; } = ItemType.General;

        public string? DamageDice { get; set; }

        public string? DamageType { get; set; }

        public WeaponScalingType ScalingType { get; set; } = WeaponScalingType.Strength;

        public WeaponProficiencyType WeaponProficiencyType { get; set; } = WeaponProficiencyType.Simple;

        public ArmorType ArmorType { get; set; } = ArmorType.Light;

        public int BaseArmorClass { get; set; } = 10;

        public int AttackBonus { get; set; }

        public int ArmorBonus { get; set; }
    }
}