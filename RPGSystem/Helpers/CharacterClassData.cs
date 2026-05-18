using RPGSystem.Models;

namespace RPGSystem.Helpers
{
    public static class CharacterClassData
    {
        public static int GetHitDie(CharacterClassType type)
        {
            switch (type)
            {
                case CharacterClassType.Fighter:
                    return 10;
                default:
                    return 8;
            }
        }
    }
}
