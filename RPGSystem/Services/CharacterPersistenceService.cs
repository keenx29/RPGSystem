using Microsoft.EntityFrameworkCore;
using RPGSystem.Data;
using RPGSystem.Data.Entities;
using RPGSystem.Models.Characters;

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
                    .FirstOrDefault(c => c.Id == character.Id);

                if (existing == null)
                {
                    context.Characters.Add(ToEntity(character));
                    continue;
                }

                UpdateEntity(existing, character);
                UpdateAbilities(existing, character);
                UpdateSkills(existing, character);
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
    }
}