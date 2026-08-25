using RPGSystem.Models.Characters;
using RPGSystem.Models.Classes.Features;
using RPGSystem.Models.Items;

namespace RPGSystem.Models.Classes
{
    public abstract class CharacterClass
    {
        public abstract CharacterClassType Type { get; }
        public abstract int HitDie { get; }
        public virtual int SkillProficiencyChoiceCount => 2;
        public abstract IReadOnlyCollection<AbilityType> SavingThrowProficiencies { get; }
        public virtual IReadOnlyCollection<SkillType> AvailableSkillProficiencies =>
            Array.Empty<SkillType>();
        public abstract List<ClassFeatureInstance> GetFeaturesForLevel(int level);
        public virtual int? GetUnarmoredArmorClass(Character character)
        {
            return null;
        }
        public virtual List<FeatureResource> GetResourcesForLevel(int level)
        {
            return new List<FeatureResource>();
        }
        public virtual bool IsProficientWithWeapon(Weapon weapon)
        {
            return false;
        }
        
        public virtual bool IsProficientWithArmor(Armor armor)
        {
            return false;
        }

        public virtual bool IsProficientWithShield()
        {
            return false;
        }
        public virtual bool GrantsAbilityScoreImprovement(int level)
        {
            return level == 4
                || level == 8
                || level == 12
                || level == 16
                || level == 19;
        }
        public bool CanChooseSkillProficiency(SkillType skillType)
        {
            return AvailableSkillProficiencies.Contains(skillType);
        }

        public bool HasSavingThrowProficiency(AbilityType abilityType)
        {
            return SavingThrowProficiencies.Contains(abilityType);
        }
        public virtual int GetExpertiseChoiceCount(int level)
        {
            return 0;
        }
    }
}
