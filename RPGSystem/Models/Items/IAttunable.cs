namespace RPGSystem.Models.Items
{
    public interface IAttunable
    {
        bool RequiresAttunement { get; }
        bool IsAttuned { get; set; }
    }
}
