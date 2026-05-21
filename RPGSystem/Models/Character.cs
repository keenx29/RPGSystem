using RPGSystem.Models.Classes.Features;

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

        // Class Data
        public List<ClassFeatureInstance> ClassFeatures { get; set; } = new();

        // Equipment
        public Weapon? EquippedWeapon { get; set; }
        public Armor? EquippedArmor { get; set; }
        public List<Item> Inventory { get; set; } = new();
        public List<Item> AttunedItems { get; set; } = new();

        public void TakeDamage(int amount)
        {
            CurrentHP = Math.Max(0, CurrentHP - amount);
        }
        public void Heal(int amount)
        {
            CurrentHP = Math.Min(MaxHP, CurrentHP + amount);
        }
        public void EquipWeapon(Weapon weapon)
        {
            if (EquippedWeapon != null)
                Inventory.Add(EquippedWeapon);

            EquippedWeapon = weapon;
            Inventory.Remove(weapon);
        }
        public void UnequipWeapon()
        {
            if (EquippedWeapon != null)
            {
                Inventory.Add(EquippedWeapon);
                EquippedWeapon = null;
            }
        }
        public void EquipArmor(Armor armor)
        {
            if (EquippedArmor != null)
                Inventory.Add(EquippedArmor);

            EquippedArmor = armor;
            Inventory.Remove(armor);
        }
        public void UnequipArmor()
        {
            if (EquippedArmor != null)
            {
                Inventory.Add(EquippedArmor);
                EquippedArmor = null;
            }
        }
        public ClassFeatureInstance? GetFeature(string name)
        {
            return ClassFeatures.FirstOrDefault(f => f.Name == name);
        }
        public void UseFeature(string name, Action<ClassFeatureInstance> effect)
        {
            //TODO: future generic feature system
        }
        public void ShortRest()
        {
            CurrentHP = Math.Min(MaxHP, CurrentHP + (MaxHP / 4)); //TODO: Hit die logic

            foreach (var feature in ClassFeatures)
            {
                if (feature.Name == FighterFeatures.SecondWind ||
                    feature.Name == FighterFeatures.ActionSurge)
                {
                    feature.UsesRemaining = feature.MaxUses;
                }
            }
        }
        public void LongRest()
        {
            CurrentHP = MaxHP;

            foreach (var feature in ClassFeatures)
            {
                feature.UsesRemaining = feature.MaxUses;
            }
        }
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