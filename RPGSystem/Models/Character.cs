namespace RPGSystem.Models
{
    public class Character
    {
        //Base Stats
        public string Name { get; set; } = "";
        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Constitution { get; set; }
        public int Intelligence { get; set; }
        public int Wisdom { get; set; }
        public int Charisma { get; set; }

        //Derived Stats
        public int Level { get; set; }
        public int ArmorClass { get; set; }
        public int MaxHP { get; set; }
        public int CurrentHP { get; set; }
        public int MovementSpeed { get; set; } = 30;

        //Skills
        public List<Skill> Skills { get; set; } = new();

        //Inventory
        public Weapon EquippedWeapon { get; set; }

        public int GetStrengthModifier()
        {
            return (Strength - 10) / 2;
        }

        public int GetDexterityModifier()
        {
            return (Dexterity - 10) / 2;
        }

        public int GetConstitutionModifier()
        {
            return (Constitution - 10) / 2;
        }

        public int GetIntelligenceModifier()
        {
            return (Intelligence - 10) / 2;
        }

        public int GetWisdomModifier()
        {
            return (Wisdom - 10) / 2;
        }

        public int GetCharismaModifier()
        {
            return (Charisma - 10) / 2;
        }

        public int GetInitiative()
        {
            return GetDexterityModifier();
        }

        public int GetPassivePerception()
        {
            return 10 + GetWisdomModifier();
        }

        public int GetProficiencyBonus()
        {
            return 2 + ((Level - 1) / 4);
        }

        public int GetAbilityModifier(AbilityType ability)
        {
            switch (ability)
            {
                case AbilityType.Strength:
                    return GetStrengthModifier();

                case AbilityType.Dexterity:
                    return GetDexterityModifier();

                case AbilityType.Constitution:
                    return GetConstitutionModifier();

                case AbilityType.Intelligence:
                    return GetIntelligenceModifier();

                case AbilityType.Wisdom:
                    return GetWisdomModifier();

                case AbilityType.Charisma:
                    return GetCharismaModifier();

                default:
                    return 0;
            }
        }

        public int GetSkillBonus(Skill skill)
        {
            int bonus = GetAbilityModifier(skill.RelatedAbility);

            if (skill.IsProficient)
            {
                bonus += GetProficiencyBonus();
            }

            return bonus;
        }
        
        public string FormatModifier(int value)
        {
            if (value >= 0)
            {
                return $"+{value}";
            }
            return value.ToString();
        }
    }
}

