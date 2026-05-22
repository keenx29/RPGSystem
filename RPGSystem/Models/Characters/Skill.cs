namespace RPGSystem.Models.Characters
{
    public class Skill
    {
        public string Name { get; set; } = "";
        public SkillType Type { get; set; }

        public Ability RelatedAbility { get; set; }

        public bool IsProficient { get; set; }
        public bool IsExpertise { get; set; }

        public int GetBonus(int proficiencyBonus)
        {
            int bonus = RelatedAbility.Modifier;

            if (IsProficient)
                bonus += proficiencyBonus;

            if (IsExpertise)
                bonus += proficiencyBonus;

            return bonus;
        }
    }
}