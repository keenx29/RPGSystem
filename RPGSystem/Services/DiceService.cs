namespace RPGSystem.Services
{
    public class DiceService
    {
        private readonly Random _random = new();

        public int RollD20()
        {
            return _random.Next(1, 21);
        }
    }
}

