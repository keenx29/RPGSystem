using RPGSystem.Models.Characters;

namespace RPGSystem.Data.Entities
{
    public class DefenseEntryEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CharacterId { get; set; }
        public CharacterEntity Character { get; set; } = null!;

        public DefenseType Type { get; set; }
        public string Name { get; set; } = "";
    }
}