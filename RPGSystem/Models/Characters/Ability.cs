namespace RPGSystem.Models.Characters
{
    public class Ability
    {
        public string Name { get; set; } = "";
        public AbilityType Type { get; set; }
        public int Score { get; set; }
        public bool IsSavingThrowProficient { get; set; } = false;
        public int Modifier =>
            (int)Math.Floor((Score - 10) / 2.0);
    }
}
