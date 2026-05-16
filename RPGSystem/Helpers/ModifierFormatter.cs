namespace RPGSystem.Helpers
{
    public static class ModifierFormatter
    {
        public static string FormatWithoutSpace(int value)
        {
            if (value >= 0)
            {
                return $"+{value}";
            }
            return value.ToString();
        }
        public static string FormatWithSpace(int value)
        {
            if (value >= 0)
            {
                return $"+ {value}";
            }
            return $"- {Math.Abs(value)}";
        }
    }
}