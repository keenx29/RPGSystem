using RPGSystem.Models.Items;

namespace RPGSystem.Data.Entities
{
    public class ItemEntity
    {
        public Guid Id { get; set; }

        public Guid CharacterId { get; set; }

        public CharacterEntity Character { get; set; } = null!;

        public string Location { get; set; } = "Inventory";

        public string Kind { get; set; } = "Item";

        public string Name { get; set; } = "";

        public string Description { get; set; } = "";

        public double Weight { get; set; }

        public ItemType Type { get; set; }

        public string? DamageDice { get; set; }

        public string? DamageType { get; set; }

        public int AttackBonus { get; set; }

        public WeaponScalingType ScalingType { get; set; }

        public WeaponProficiencyType ProficiencyType { get; set; }

        public string? ProficiencyName { get; set; }

        public int BaseArmorClass { get; set; }

        public ArmorType ArmorType { get; set; }

        public string? EffectType { get; set; }

        public string? EffectDice { get; set; }
    }
}