using RPGSystem.Helpers;
using RPGSystem.Models;
using RPGSystem.Models.Classes;
using RPGSystem.Models.Classes.Features;

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
        public void UseSecondWind()
        {
            var feature = _character.GetFeature(FighterFeatures.SecondWind);

            if (feature == null || !feature.IsAvailable)
                return;

            int healAmount = _diceService.RollDice("1d10") + _character.Level;

            _character.Heal(healAmount);

            feature.UsesRemaining--;
        }
        public void LevelUp()
        {
            var ability = _character.GetAbility(AbilityType.Constitution);

            var characterClass = CharacterClassFactory.Create(_character.ClassType);

            int hpGain =
                _diceService.RollDice($"1d{characterClass.HitDie}")
                + ability.Modifier;

            _character.LevelUp(hpGain);

            _character.ClassFeatures = characterClass.GetFeaturesForLevel(_character.Level);
        }
        public void ShortRest()
        {
            _character.ShortRest();
        }
        public void LongRest()
        {
            _character.LongRest();
        }
        public void ModifyHP(int amount, HpChangeType type)
        {
            if (type == HpChangeType.Damage)
                TakeDamage(amount);
            else
                Heal(amount);
        }
        public void TakeDamage(int amount)
        {
            _character.TakeDamage(amount);
        }
        public void Heal(int amount)
        {
            _character.Heal(amount);
        }
        public void EquipWeapon(Guid weaponId)
        {
            var weapon = _character.Inventory.OfType<Weapon>().First(w => w.Id == weaponId);

            _character.EquipWeapon(weapon);
        }
        public void UnequipWeapon()
        {
            _character.UnequipWeapon();
        }
        public void EquipArmor(Guid armorId)
        {
            var armor = _character.Inventory.OfType<Armor>().First(a => a.Id == armorId);

            _character.EquipArmor(armor);
        }
        public void UnequipArmor()
        {
            _character.UnequipArmor();
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
                ClassType = CharacterClassType.Fighter,

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
                },

            };

            var characterClass = CharacterClassFactory.Create(character.ClassType);

            character.ClassFeatures = characterClass.GetFeaturesForLevel(character.Level);

            return character;
        }
    }
}