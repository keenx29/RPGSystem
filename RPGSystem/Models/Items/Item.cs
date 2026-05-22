namespace RPGSystem.Models.Items
{ 
    public class Item
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = "";
        public string Description { get; set; } = "";

        public double Weight { get; set; }
        public bool IsStackable { get; set; } = false;

        public ItemType Type { get; set; }
        public IItemEffect? Effect { get; set; }
    }
}
