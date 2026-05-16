namespace RPGSystem.Models
{
    public class Ability
    {
        public string Name { get; set; } = "";
        public int Score { get; set; }

        public int Modifier => (Score - 10) / 2;
    }
}
