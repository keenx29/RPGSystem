using RPGSystem.Models.Classes;
using RPGSystem.Models.Classes.Features;
using RPGSystem.Models.Items;
using System.Resources;

namespace RPGSystem.Models.Characters
{
    public class Character
    {
        // Identity
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "";
        public CharacterClassType ClassType { get; set; }

        // Core data
        public List<Ability> Abilities { get; set; } = new();
        public List<Skill> Skills { get; set; } = new();
        public int MaxHP { get; set; } = 20;
        public int CurrentHP { get; set; } = 20;
        public int MovementSpeed { get; set; } = 30;
        public int TotalMovementSpeed
        {
            get
            {
                var speed = MovementSpeed;

                if (HasFeature(MonkFeatures.UnarmoredMovement) 
                    && EquippedArmor == null 
                    && EquippedShield == null)
                {
                    speed += 10;
                }

                return speed;
            }
        }
        public int HitDiceRemaining { get; set; }
        public int MaxHitDice => Level;
        public int PendingAbilityScoreImprovementPoints { get; set; }
        public List<string> DamageResistances { get; set; } = new();

        public List<string> DamageVulnerabilities { get; set; } = new();

        public List<string> DamageImmunities { get; set; } = new();

        public List<string> ConditionImmunities { get; set; } = new();

        // Derived stats
        public int Level { get; set; }
        public int ArmorClass { get; set; } = 10;
        // Character State
        public List<ConditionType> Conditions { get; set; } = new();
        public int DeathSaveSuccesses { get; set; }
        public int DeathSaveFailures { get; set; }
        public bool IsStable { get; set; }
        public bool IsDead { get; set; }

        public bool IsUnconscious => CurrentHP == 0;
        public bool ShouldMakeDeathSaves => CurrentHP == 0 && !IsStable && !IsDead;

        // Class Data
        public List<ClassFeatureInstance> ClassFeatures { get; set; } = new();
        public List<FeatureResource> FeatureResources { get; set; } = new();

        // Equipment
        public List<Weapon> EquippedWeapons { get; set; } = new();
        public Weapon? EquippedWeapon
        {
            get => EquippedWeapons.FirstOrDefault();
            set
            {
                EquippedWeapons.Clear();

                if (value != null)
                    EquippedWeapons.Add(value);
            }
        }
        public Armor? EquippedArmor { get; set; }
        public Armor? EquippedShield { get; set; }
        public List<Item> Inventory { get; set; } = new();
        public List<Item> AttunedItems { get; set; } = new();
        public void EquipShield(Armor shield)
        {
            if (EquippedShield != null)
                Inventory.Add(EquippedShield);

            EquippedShield = shield;
            Inventory.Remove(shield);
        }

        public void UnequipShield(Guid shieldId)
        {
            if (EquippedShield != null && EquippedShield.Id == shieldId)
            {
                Inventory.Add(EquippedShield);
                EquippedShield = null;
            }
        }
        public void ClearConditions()
        {
            Conditions.Clear();
        }
        public bool HasCondition(ConditionType condition)
        {
            return Conditions.Contains(condition);
        }

        public void AddCondition(ConditionType condition)
        {
            if (!Conditions.Contains(condition))
                Conditions.Add(condition);
        }

        public void RemoveCondition(ConditionType condition)
        {
            Conditions.Remove(condition);
        }
        public void ResetDeathSaves()
        {
            DeathSaveSuccesses = 0;
            DeathSaveFailures = 0;
            IsStable = false;
            IsDead = false;
        }
        public void SpendHitDice(int amount)
        {
            HitDiceRemaining = Math.Max(0, HitDiceRemaining - amount);
        }

        public void RestoreHitDice(int amount)
        {
            HitDiceRemaining = Math.Min(MaxHitDice, HitDiceRemaining + amount);
        }
        public void TakeDamage(int amount)
        {
            CurrentHP = Math.Max(0, CurrentHP - amount);
        }
        public void Heal(int amount)
        {
            if (amount <= 0 || IsDead)
                return;

            CurrentHP = Math.Min(MaxHP, CurrentHP + amount);

            if (CurrentHP > 0)
                ResetDeathSaves();
        }
        public void EquipWeapon(Weapon weapon)
        {
            if (EquippedWeapons.Any(w => w.Id == weapon.Id))
                return;

            EquippedWeapons.Add(weapon);
            Inventory.Remove(weapon);
        }
        public void UnequipWeapon(Guid weaponId)
        {
            var weapon = EquippedWeapons.FirstOrDefault(w => w.Id == weaponId);

            if (weapon == null)
                return;

            EquippedWeapons.Remove(weapon);
            Inventory.Add(weapon);
        }
        public void EquipArmor(Armor armor)
        {
            if (EquippedArmor != null)
                Inventory.Add(EquippedArmor);

            EquippedArmor = armor;
            Inventory.Remove(armor);
        }
        public void UnequipArmor(Guid armorId)
        {
            if (EquippedArmor != null && EquippedArmor.Id == armorId)
            {
                Inventory.Add(EquippedArmor);
                EquippedArmor = null;
            }
        }
        public FeatureResource? GetResource(string name)
        {
            return FeatureResources.FirstOrDefault(r => r.Name == name);
        }
        public bool SpendResource(string name, int amount)
        {
            var resource = GetResource(name);

            if (resource == null)
                return false;

            if (resource.Current < amount)
                return false;

            resource.Current -= amount;

            return true;
        }
        public void RestoreAllResources()
        {
            foreach (var resource in FeatureResources)
            {
                resource.Current = resource.Max;
            }
        }
        public ClassFeatureInstance? GetFeature(string name)
        {
            return ClassFeatures.FirstOrDefault(f => f.Name == name);
        }
        public void UseFeature(string name, Action<ClassFeatureInstance> effect)
        {
            //TODO: future generic feature system (maybe)
        }
        public void ShortRest()
        {
            //TODO: Hit Dice System
            RestoreFeatureUsesForShortRest();
            RestoreResourcesForShortRest();
        }
        public void LongRest()
        {
            CurrentHP = MaxHP;

            RestoreFeatureUsesForLongRest();
            RestoreResourcesForLongRest();

            int hitDiceToRestore = Math.Max(1, MaxHitDice / 2);
            RestoreHitDice(hitDiceToRestore);

            ResetDeathSaves();
        }
        public void LevelUp(int hpGain)
        {
            Level++;

            MaxHP += hpGain;

            CurrentHP = MaxHP;

            HitDiceRemaining = Math.Min(MaxHitDice, HitDiceRemaining + 1);
        }
        public Ability GetAttackAbility(Weapon weapon)
        {
            return weapon.ScalingType switch
            {
                WeaponScalingType.Dexterity => GetAbility(AbilityType.Dexterity),

                WeaponScalingType.Finesse =>
                    GetAbility(AbilityType.Dexterity).Modifier >
                    GetAbility(AbilityType.Strength).Modifier
                        ? GetAbility(AbilityType.Dexterity)
                        : GetAbility(AbilityType.Strength),

                _ => GetAbility(AbilityType.Strength)
            };
        }
        public int GetProficiencyBonus()
        {
            return 2 + (Level - 1) / 4;
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
        public Skill GetSkill(SkillType type)
        {
            return Skills.First(s => s.Type == type);
        }
        public int GetAttackBonus(Weapon weapon)
        {
            var ability = GetAttackAbility(weapon);
            var proficiencyBonus = IsProficientWithWeapon(weapon)
                ? GetProficiencyBonus()
                : 0;
            int bonus = ability.Modifier + proficiencyBonus + weapon.AttackBonus;

            return bonus;
        }

        public int GetDamageBonus(Weapon weapon)
        {
            var ability = GetAttackAbility(weapon);

            return ability.Modifier;
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
            int dexModifier = GetAbility(AbilityType.Dexterity).Modifier;

            if (EquippedArmor != null)
            {
                return EquippedArmor.ArmorType switch
                {
                    ArmorType.Light => EquippedArmor.BaseArmorClass + dexModifier + GetShieldBonus(),
                    ArmorType.Medium => EquippedArmor.BaseArmorClass + Math.Min(dexModifier, 2) + GetShieldBonus(),
                    ArmorType.Heavy => EquippedArmor.BaseArmorClass + GetShieldBonus(),
                    _ => EquippedArmor.BaseArmorClass + GetShieldBonus()
                };
            }

            var characterClass = CharacterClassFactory.Create(ClassType);
            var unarmoredAc = characterClass.GetUnarmoredArmorClass(this);

            return unarmoredAc ?? 10 + dexModifier+ GetShieldBonus();
        }
        public bool IsWearingArmor()
        {
            return EquippedArmor != null;
        }
        public bool HasFeature(string featureName)
        {
            return ClassFeatures.Any(f => f.Name == featureName);
        }
        public bool IsProficientWithWeapon(Weapon weapon)
        {
            var characterClass = CharacterClassFactory.Create(ClassType);
            return characterClass.IsProficientWithWeapon(weapon);
        }
        public void ApplySavingThrowProficiencies(IReadOnlyCollection<AbilityType> proficiencies)
        {
            foreach (var ability in Abilities)
            {
                ability.IsSavingThrowProficient = proficiencies.Contains(ability.Type);
            }
        }
        public void ApplySkillProficiencies(IEnumerable<SkillType> skillTypes)
        {
            foreach (var skill in Skills)
            {
                skill.IsProficient = skillTypes.Contains(skill.Type);
            }
        }

        public void ApplySkillExpertise(IEnumerable<SkillType> skillTypes)
        {
            foreach (var skill in Skills)
            {
                if (skillTypes.Contains(skill.Type))
                {
                    skill.IsExpertise = true;
                    skill.IsProficient = true;
                }
            }
        }
        public bool IsProficientWithArmor(Armor armor)
        {
            var characterClass = CharacterClassFactory.Create(ClassType);
            return characterClass.IsProficientWithArmor(armor);
        }

        public bool IsProficientWithShield()
        {
            var characterClass = CharacterClassFactory.Create(ClassType);
            return characterClass.IsProficientWithShield();
        }
        public bool IncreaseAbilityScore(AbilityType abilityType)
        {
            if (PendingAbilityScoreImprovementPoints <= 0)
            {
                return false;
            }

            var ability = GetAbility(abilityType);

            if (ability.Score >= 20)
            {
                return false;
            }

            ability.Score++;
            PendingAbilityScoreImprovementPoints--;

            return true;
        }
        public string FormatDefenseList(List<string> values)
        {
            return values.Any()
                ? string.Join(", ", values)
                : "None";
        }
        private int GetShieldBonus()
        {
            return EquippedShield != null ? EquippedShield.BaseArmorClass : 0;
        }
        private static bool ResetsOnShortRest(FeatureResetType resetType)
        {
            return resetType == FeatureResetType.ShortRest;
        }

        private static bool ResetsOnLongRest(FeatureResetType resetType)
        {
            return resetType == FeatureResetType.ShortRest ||
                   resetType == FeatureResetType.LongRest;
        }
        private void RestoreFeatureUsesForShortRest()
        {
            foreach (var feature in ClassFeatures)
            {
                if (ResetsOnShortRest(feature.ResetType))
                    feature.UsesRemaining = feature.MaxUses;
            }
        }

        private void RestoreFeatureUsesForLongRest()
        {
            foreach (var feature in ClassFeatures)
            {
                if (ResetsOnLongRest(feature.ResetType))
                    feature.UsesRemaining = feature.MaxUses;
            }
        }

        private void RestoreResourcesForShortRest()
        {
            foreach (var resource in FeatureResources)
            {
                if (ResetsOnShortRest(resource.ResetType))
                    resource.Current = resource.Max;
            }
        }

        private void RestoreResourcesForLongRest()
        {
            foreach (var resource in FeatureResources)
            {
                if (ResetsOnLongRest(resource.ResetType))
                    resource.Current = resource.Max;
            }
        }
    }
}