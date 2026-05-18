namespace RPGSystem.Models
{
    public class MagicItem : Item, IAttunable
    {
        public bool RequiresAttunement { get; set; }
        public bool IsAttuned { get; set; }
    }
}
