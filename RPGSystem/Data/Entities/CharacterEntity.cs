using RPGSystem.Models.Classes;

namespace RPGSystem.Data.Entities
{
    public class CharacterEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = "";

        public CharacterClassType ClassType { get; set; }

        public int Level { get; set; }

        public int MaxHP { get; set; }

        public int CurrentHP { get; set; }

        public int MovementSpeed { get; set; }

        public string Race { get; set; } = "";

        public string Background { get; set; } = "";

        public string Alignment { get; set; } = "";

        public string PersonalityTraits { get; set; } = "";

        public string Ideals { get; set; } = "";

        public string Bonds { get; set; } = "";

        public string Flaws { get; set; } = "";

        public string Notes { get; set; } = "";

        public List<AbilityEntity> Abilities { get; set; } = new();

        public List<SkillEntity> Skills { get; set; } = new();
        public int HitDiceRemaining { get; set; }

        public int PendingAbilityScoreImprovementPoints { get; set; }

        public int DeathSaveSuccesses { get; set; }

        public int DeathSaveFailures { get; set; }

        public bool IsStable { get; set; }

        public bool IsDead { get; set; }
    }
}