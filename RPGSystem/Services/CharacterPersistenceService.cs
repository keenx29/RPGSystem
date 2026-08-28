using Microsoft.EntityFrameworkCore;
using RPGSystem.Data;
using RPGSystem.Data.Entities;
using RPGSystem.Models.Characters;
using RPGSystem.Models.Items;

namespace RPGSystem.Services
{
    public class CharacterPersistenceService
    {
        private readonly IDbContextFactory<RpgDbContext> _contextFactory;

        public CharacterPersistenceService(IDbContextFactory<RpgDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public List<CharacterEntity> LoadCharacterStates()
        {
            using var context = _contextFactory.CreateDbContext();

            return context.Characters
                .Include(c => c.Abilities)
                .Include(c => c.Skills)
                .Include(c => c.Items)
                .Include(c => c.Conditions)
                .Include(c => c.FeatureStates)
                .Include(c => c.FeatureResources)
                .AsNoTracking()
                .ToList();
        }

        public void SaveCharacters(IEnumerable<Character> characters)
        {
            using var context = _contextFactory.CreateDbContext();

            foreach (var character in characters)
            {
                var existing = context.Characters
                    .Include(c => c.Abilities)
                    .Include(c => c.Skills)
                    .Include(c => c.Items)
                    .Include(c => c.Conditions)
                    .Include(c => c.FeatureStates)
                    .Include(c => c.FeatureResources)
                    .FirstOrDefault(c => c.Id == character.Id);

                if (existing == null)
                {
                    context.Characters.Add(ToEntity(character));
                    continue;
                }

                UpdateEntity(existing, character);
                UpdateAbilities(existing, character);
                UpdateSkills(existing, character);
                UpdateItems(context, existing, character);
                UpdateConditions(context, existing, character);
                UpdateFeatureStates(context, existing, character);
                UpdateFeatureResources(context, existing, character);
            }

            context.SaveChanges();
        }

        private CharacterEntity ToEntity(Character character)
        {
            var entity = new CharacterEntity();

            UpdateEntity(entity, character);

            entity.Abilities = character.Abilities
                .Select(a => ToAbilityEntity(a, character.Id))
                .ToList();

            entity.Skills = character.Skills
                .Select(s => ToSkillEntity(s, character.Id))
                .ToList();

            return entity;
        }

        private void UpdateEntity(CharacterEntity entity, Character character)
        {
            entity.Id = character.Id;
            entity.Name = character.Name;
            entity.ClassType = character.ClassType;
            entity.Level = character.Level;
            entity.MaxHP = character.MaxHP;
            entity.CurrentHP = character.CurrentHP;
            entity.MovementSpeed = character.MovementSpeed;
            entity.HitDiceRemaining = character.HitDiceRemaining;
            entity.PendingAbilityScoreImprovementPoints = character.PendingAbilityScoreImprovementPoints;
            entity.DeathSaveSuccesses = character.DeathSaveSuccesses;
            entity.DeathSaveFailures = character.DeathSaveFailures;
            entity.IsStable = character.IsStable;
            entity.IsDead = character.IsDead;
            entity.Race = character.Race;
            entity.Background = character.Background;
            entity.Alignment = character.Alignment;
            entity.PersonalityTraits = character.PersonalityTraits;
            entity.Ideals = character.Ideals;
            entity.Bonds = character.Bonds;
            entity.Flaws = character.Flaws;
            entity.Notes = character.Notes;
        }

        private AbilityEntity ToAbilityEntity(Ability ability, Guid characterId)
        {
            return new AbilityEntity
            {
                CharacterId = characterId,
                Name = ability.Name,
                Type = ability.Type,
                Score = ability.Score,
                IsSavingThrowProficient = ability.IsSavingThrowProficient
            };
        }

        private SkillEntity ToSkillEntity(Skill skill, Guid characterId)
        {
            return new SkillEntity
            {
                CharacterId = characterId,
                Name = skill.Name,
                Type = skill.Type,
                RelatedAbilityType = skill.RelatedAbility.Type,
                IsProficient = skill.IsProficient,
                IsExpertise = skill.IsExpertise
            };
        }
        private void UpdateAbilities(CharacterEntity entity, Character character)
        {
            foreach (var ability in character.Abilities)
            {
                var existingAbility = entity.Abilities
                    .FirstOrDefault(a => a.Type == ability.Type);

                if (existingAbility == null)
                {
                    entity.Abilities.Add(ToAbilityEntity(ability, character.Id));
                    continue;
                }

                existingAbility.Name = ability.Name;
                existingAbility.Score = ability.Score;
                existingAbility.IsSavingThrowProficient = ability.IsSavingThrowProficient;
            }
        }
        private void UpdateSkills(CharacterEntity entity, Character character)
        {
            foreach (var skill in character.Skills)
            {
                var existingSkill = entity.Skills
                    .FirstOrDefault(s => s.Type == skill.Type);

                if (existingSkill == null)
                {
                    entity.Skills.Add(ToSkillEntity(skill, character.Id));
                    continue;
                }

                existingSkill.Name = skill.Name;
                existingSkill.RelatedAbilityType = skill.RelatedAbility.Type;
                existingSkill.IsProficient = skill.IsProficient;
                existingSkill.IsExpertise = skill.IsExpertise;
            }
        }
        private void UpdateItems(
            RpgDbContext context,
            CharacterEntity entity,
            Character character)
        {
            context.Items.RemoveRange(entity.Items);

            var currentItems = new List<ItemEntity>();

            currentItems.AddRange(character.Inventory.Select(item =>
                ToItemEntity(item, character.Id, "Inventory")));

            currentItems.AddRange(character.EquippedWeapons.Select(item =>
                ToItemEntity(item, character.Id, "EquippedWeapon")));

            if (character.EquippedArmor != null)
            {
                currentItems.Add(ToItemEntity(
                    character.EquippedArmor,
                    character.Id,
                    "EquippedArmor"));
            }

            if (character.EquippedShield != null)
            {
                currentItems.Add(ToItemEntity(
                    character.EquippedShield,
                    character.Id,
                    "EquippedShield"));
            }

            context.Items.AddRange(currentItems);
        }
        private ItemEntity ToItemEntity(Item item, Guid characterId, string location)
        {
            var entity = new ItemEntity
            {
                Id = item.Id,
                CharacterId = characterId,
                Location = location,
                Name = item.Name,
                Description = item.Description,
                Weight = item.Weight,
                Type = item.Type
            };

            if (item is Weapon weapon)
            {
                entity.Kind = "Weapon";
                entity.DamageDice = weapon.DamageDice;
                entity.DamageType = weapon.DamageType;
                entity.AttackBonus = weapon.AttackBonus;
                entity.ScalingType = weapon.ScalingType;
                entity.ProficiencyType = weapon.ProficiencyType;
                entity.ProficiencyName = weapon.ProficiencyName;
            }
            else if (item is Armor armor)
            {
                entity.Kind = "Armor";
                entity.BaseArmorClass = armor.BaseArmorClass;
                entity.ArmorType = armor.ArmorType;
            }
            else
            {
                entity.Kind = "Item";

                if (item.Effect is HealEffect healEffect)
                {
                    entity.EffectType = "Heal";
                    entity.EffectDice = healEffect.Notation;
                }
            }

            return entity;
        }
        private void UpdateConditions(RpgDbContext context, CharacterEntity entity, Character character)
        {
            context.Conditions.RemoveRange(entity.Conditions);

            context.Conditions.AddRange(character.Conditions.Select(condition =>
                new ConditionEntity
                {
                    CharacterId = character.Id,
                    Type = condition
                }));
        }
        private void UpdateFeatureStates(RpgDbContext context, CharacterEntity entity, Character character)
        {
            context.FeatureStates.RemoveRange(entity.FeatureStates);

            context.FeatureStates.AddRange(character.ClassFeatures.Select(feature =>
                new FeatureStateEntity
                {
                    CharacterId = character.Id,
                    FeatureName = feature.Name,
                    UsesRemaining = feature.UsesRemaining,
                    IsActive = feature.IsActive
                }));
        }
        private void UpdateFeatureResources(RpgDbContext context, CharacterEntity entity, Character character)
        {
            context.FeatureResources.RemoveRange(entity.FeatureResources);

            context.FeatureResources.AddRange(character.FeatureResources.Select(resource =>
                new FeatureResourceEntity
                {
                    CharacterId = character.Id,
                    Name = resource.Name,
                    Current = resource.Current,
                    Max = resource.Max,
                    ResetType = resource.ResetType
                }));
        }
    }
}