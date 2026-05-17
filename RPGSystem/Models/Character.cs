namespace RPGSystem.Models
{
    public class Character
    {
        // Identity
        public string Name { get; set; } = "";

        // Core data
        public List<Ability> Abilities { get; set; } = new();
        public List<Skill> Skills { get; set; } = new();
        public int MaxHP { get; set; }
        public int CurrentHP { get; set; }
        public int MovementSpeed { get; set; } = 30;

        // Derived stats
        public int Level { get; set; }
        public int ArmorClass { get; set; } = 10;

        // Equipment
        public Weapon? EquippedWeapon { get; set; }
        public Armor? EquippedArmor { get; set; }
        public List<Item> Inventory { get; set; } = new();

        // System logic (minimal only)

        public int GetProficiencyBonus()
        {
            return 2 + ((Level - 1) / 4);
        }

        public int GetInitiative()
        {
            return GetAbility("Dexterity").Modifier;
        }

        public int GetPassivePerception()
        {
            return 10 + GetAbility("Wisdom").Modifier;
        }

        // Helper: ability lookup (needed only once)
        public Ability GetAbility(string name)
        {
            return Abilities.First(a => a.Name == name);
        }
        // Helper: saving throw bonus calculation
        public int GetSavingThrowBonus(Ability ability)
        {
            int bonus = ability.Modifier;

            if (ability.IsSavingThrowProficient)
            {
                bonus += GetProficiencyBonus();
            }

            return bonus;
        }

        // Helper: skill bonus (delegates to Skill class)
        public int GetSkillBonus(Skill skill)
        {
            return skill.GetBonus(GetProficiencyBonus());
        }
        public int GetArmorClass()
        {
            if (EquippedArmor != null)
                ArmorClass = EquippedArmor.ArmorClass;
            return ArmorClass;
        }
        public void EquipWeapon(Weapon weapon)
        {
            EquippedWeapon = weapon;
        }
        public void EquipArmor(Armor armor)
        {
            EquippedArmor = armor;
        }

    }
}