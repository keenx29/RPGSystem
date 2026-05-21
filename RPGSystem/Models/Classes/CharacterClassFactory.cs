namespace RPGSystem.Models.Classes
{
    public static class CharacterClassFactory
    {
        public static CharacterClass Create(CharacterClassType type)
        {
            switch (type)
            {
                case CharacterClassType.Fighter:
                    return new FighterClass();
                default:
                    throw new Exception("Unknown class type");
            }
        }
    }
}
