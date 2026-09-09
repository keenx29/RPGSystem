namespace RPGSystem.Data.Entities
{
    public class CharacterSenseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CharacterId { get; set; }
        public CharacterEntity Character { get; set; } = null!;

        public string Name { get; set; } = "";
        public int? RangeFeet { get; set; }
        public string Description { get; set; } = "";
    }
}