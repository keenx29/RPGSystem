namespace RPGSystem.Models
{
    public class Weapon : Item
    {
        public string DamageDice { get; set; } = "1d6";

        public string DamageType { get; set; } = "slashing";

        public bool IsFinesse { get; set; }

        public int AttackBonus { get; set; }
        public AbilityType ScalingAbility { get; set; } = AbilityType.Strength;
    }
}