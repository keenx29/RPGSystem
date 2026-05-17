namespace RPGSystem.Models
{
    public class Ability
    {
        public string Name { get; set; } = "";
        public int Score { get; set; }
        public bool IsSavingThrowProficient { get; set; } = false;
        public int Modifier => (Score - 10) / 2;
    }
}
