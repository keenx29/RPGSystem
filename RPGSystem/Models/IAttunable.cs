namespace RPGSystem.Models
{
    public interface IAttunable
    {
        bool RequiresAttunement { get; }
        bool IsAttuned { get; set; }
    }
}
