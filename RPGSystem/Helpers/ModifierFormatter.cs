namespace RPGSystem.Helpers
{
    public static class ModifierFormatter
    {
        public static string Format(int value)
        {
            if (value >= 0)
            {
                return $"+{value}";
            }
            return value.ToString();
        }
    }
}