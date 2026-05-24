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
                case CharacterClassType.Rogue:
                    return new RogueClass();
                case CharacterClassType.Barbarian:
                    return new BarbarianClass();
                default:
                    throw new Exception("Unknown class type");
            }
        }
    }
}
