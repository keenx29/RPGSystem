using RPGSystem.Models.Items;

namespace RPGSystem.Helpers
{
    public static class WeaponScalingFormatter
    {
        public static string Explain(WeaponScalingType scalingType)
        {
            return scalingType switch
            {
                WeaponScalingType.Strength => "Uses Strength for attack and damage.",
                WeaponScalingType.Dexterity => "Uses Dexterity for attack and damage.",
                WeaponScalingType.Finesse => "Uses the better modifier between Strength and Dexterity.",
                _ => "Uses the character's attack ability."
            };
        }
    }
}