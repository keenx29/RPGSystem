namespace RPGSystem.Models
{
    public class Weapon
    {
        public string Name { get; set; } = "";

        public string DamageDice { get; set; } = "1d8";

        public bool IsFinesse { get; set; }

        public int AttackBonus { get; set; }
    }
}