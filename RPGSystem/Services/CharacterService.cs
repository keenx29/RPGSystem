using RPGSystem.Helpers;
using RPGSystem.Models;

namespace RPGSystem.Services
{
    public class CharacterService
    {
        private readonly Character _character;
        private readonly DiceService _diceService;

        public CharacterService(DiceService diceService)
        {
            _diceService = diceService;
            _character = GetTestCharacter();
        }
        public Character GetCharacter()
        {
            return _character;
        }
        public void LevelUp()
        {
            var ability = _character.GetAbility(AbilityType.Constitution);

            int hitDie = CharacterClassData.GetHitDie(_character.ClassType);

            int hpGain =
                _diceService.RollDice($"1d{hitDie}")
                + ability.Modifier;

            _character.LevelUp(hpGain);
        }
        public void ShortRest()
        {
            _character.CurrentHP = Math.Min(
                _character.MaxHP,
                _character.CurrentHP + (_character.MaxHP / 4) //TODO: Short rest hit die logic
            );
        }
        public void LongRest()
        {
            _character.CurrentHP = _character.MaxHP;
        }
        public void ModifyHP(int amount, string mode)
        {
            if (mode == "damage")
            {
                _character.CurrentHP -= amount;
            }
            else if (mode == "heal")
            {
                _character.CurrentHP += amount;
            }

            _character.CurrentHP = Math.Clamp(_character.CurrentHP, 0, _character.MaxHP);
        }
        public void EquipWeapon(Guid weaponId)
        {
            var weapon = _character.Inventory
                .OfType<Weapon>()
                .First(w => w.Id == weaponId);

            if (_character.EquippedWeapon != null)
                _character.Inventory.Add(_character.EquippedWeapon);

            _character.EquippedWeapon = weapon;
            _character.Inventory.Remove(weapon);
        }
        public void UnequipWeapon()
        {
            _character.EquippedWeapon = null;
        }
        public void EquipArmor(Guid armorId)
        {
            var armor = _character.Inventory
                .OfType<Armor>()
                .First(a => a.Id == armorId);

            if (_character.EquippedArmor != null)
                _character.Inventory.Add(_character.EquippedArmor);

            _character.EquippedArmor = armor;
            _character.Inventory.Remove(armor);
        }
        public void UnequipArmor()
        {
            _character.EquippedArmor = null;
        }
        
        public Character GetTestCharacter()
        {
            var strength = new Ability { Name = "Strength", Type=AbilityType.Strength, Score = 16, IsSavingThrowProficient=true};
            var dexterity = new Ability { Name = "Dexterity", Type=AbilityType.Dexterity, Score = 14 };
            var constitution = new Ability { Name = "Constitution", Type=AbilityType.Constitution, Score = 14, IsSavingThrowProficient = true };
            var intelligence = new Ability { Name = "Intelligence",Type=AbilityType.Intelligence, Score = 10 };
            var wisdom = new Ability { Name = "Wisdom", Type = AbilityType.Wisdom, Score = 12 };
            var charisma = new Ability { Name = "Charisma", Type = AbilityType.Charisma, Score = 8 };

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