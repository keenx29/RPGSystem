namespace RPGSystem.Models
{
    public class Skill
    {
        public string Name { get; set; } = "";

        public AbilityType RelatedAbility { get; set; }

        public bool IsProficient { get; set; }
    }
}
