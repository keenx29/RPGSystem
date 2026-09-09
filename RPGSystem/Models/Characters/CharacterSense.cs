namespace RPGSystem.Models.Characters
{
    public class CharacterSense
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "";

        public int? RangeFeet { get; set; }

        public string? Description { get; set; }
    }
}