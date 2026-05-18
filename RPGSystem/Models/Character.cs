namespace RPGSystem.Models
{
    public class Character
    {
        // Identity
        public string Name { get; set; } = "";
        public CharacterClassType ClassType { get; set; }

        // Core data
        public List<Ability> Abilities { get; set; } = new();
        public List<Skill> Skills { get; set; } = new();
        public int MaxHP { get; set; } = 20;
        public int CurrentHP { get; set; } = 20;
        public int MovementSpeed { get; set; } = 30;

        // Derived stats
        public int Level { get; set; }
        public int ArmorClass { get; set; } = 10;

        // Equipment
        public Weapon? EquippedWeapon { get; set; }
        public Armor? EquippedArmor { get; set; }
        public List<Item> Inventory { get; set; } = new();
        public List<Item> AttunedItems { get; set; } = new();

        public void LevelUp(int hpGain)
        {
            Level++;

            MaxHP += hpGain;

            CurrentHP = MaxHP;
        }

        public int GetProficiencyBonus()
        {
            return 2 + ((Level - 1) / 4);
        }

        public int GetInitiative()
        {
            return GetAbility(AbilityType.Dexterity).Modifier;
        }

        public int GetPassivePerception()
        {
            return 10 + GetAbility(AbilityType.Wisdom).Modifier;
        }

        public Ability GetAbility(AbilityType type)
        {
            return Abilities.First(a => a.Type == type);
        }

        public int GetSavingThrowBonus(Ability ability)
        {
            int bonus = ability.Modifier;

            if (ability.IsSavingThrowProficient)
            {
                bonus += GetProficiencyBonus();
            }

            return bonus;
        }

        public int GetSkillBonus(Skill skill)
        {
            return skill.GetBonus(GetProficiencyBonus());
        }

        public int GetArmorClass()
        {
            if (EquippedArmor != null)
                return EquippedArmor.BaseArmorClass;

            return ArmorClass;
        }
    }
}