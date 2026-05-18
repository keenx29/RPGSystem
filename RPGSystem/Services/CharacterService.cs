using RPGSystem.Models;

namespace RPGSystem.Services
{
    public class CharacterService
    {
        private Character _character;
        public CharacterService()
        {
            _character = GetTestCharacter();
        }
        public void UnequipWeapon()
        {
            _character.EquippedWeapon = null;
        }

        public void UnequipArmor()
        {
            _character.EquippedArmor = null;
        }
        public Character GetCharacter()
        {
            return _character;
        }
        public Character GetTestCharacter()
        {
            var strength = new Ability { Name = "Strength", Score = 16, IsSavingThrowProficient=true};
            var dexterity = new Ability { Name = "Dexterity", Score = 14 };
            var constitution = new Ability { Name = "Constitution", Score = 14, IsSavingThrowProficient = true };
            var intelligence = new Ability { Name = "Intelligence", Score = 10 };
            var wisdom = new Ability { Name = "Wisdom", Score = 12 };
            var charisma = new Ability { Name = "Charisma", Score = 8 };

            var character = new Character
            {
                Name = "Tyrion",
                Level = 4,
                MovementSpeed = 30,

                Abilities = new List<Ability>
                {
                    strength, dexterity, constitution,
                    intelligence, wisdom, charisma
                },

                Skills = new List<Skill>
                {
                    new Skill { Name = "Athletics", RelatedAbility = strength, IsProficient = true },
                    new Skill { Name = "Perception", RelatedAbility = wisdom },
                    new Skill { Name = "Stealth", RelatedAbility = dexterity, IsProficient = true }
                },

                EquippedWeapon = new Weapon
                {
                    Name = "Longsword",
                    AttackBonus = 1,
                    DamageDice = "1d8",
                    DamageType = "slashing"
                },
                EquippedArmor = new Armor
                {
                    Name = "Leather Tunic",
                    BaseArmorClass = 13
                    
                },
                Inventory = new List<Item>
                {
                    new Weapon { Name= "Axe", AttackBonus = 1, DamageDice = "1d8", DamageType="slashing" },
                    new Armor { Name="Chainmail", BaseArmorClass=15}
                }
            };

            return character;
        }
    }
}