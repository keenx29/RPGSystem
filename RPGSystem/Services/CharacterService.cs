using RPGSystem.Helpers;
using RPGSystem.Models.Characters;
using RPGSystem.Models.Classes;
using RPGSystem.Models.Classes.Features;
using RPGSystem.Models.Items;
using RPGSystem.Models.Rolls;

namespace RPGSystem.Services
{
    public class CharacterService
    {
        private readonly Character _character;
        private readonly DiceService _diceService;

        public CharacterService(DiceService diceService)
        {
            _diceService = diceService;
            _character = GetRogueTestCharacter();
        }
        public Character GetCharacter()
        {
            return _character;
        }
        public void AddCondition(ConditionType condition)
        {
            _character.AddCondition(condition);
        }

        public void RemoveCondition(ConditionType condition)
        {
            _character.RemoveCondition(condition);
        }

        public void ClearConditions()
        {
            _character.ClearConditions();
        }
        public RollResult RollAbility(AbilityType type, AdvantageState adv)
        {
            var ability = _character.GetAbility(type);

            int roll = _diceService.RollD20(adv);

            return new RollResult
            {
                Actor = ability.Name,
                Type = RollType.Check,
                DiceRoll = roll,
                Modifier = ability.Modifier
            };
        }
        public RollResult RollSavingThrow(AbilityType type, AdvantageState adv)
        {
            var ability = _character.GetAbility(type);

            int roll = _diceService.RollD20(adv);

            return new RollResult
            {
                Actor = ability.Name,
                Type = RollType.Save,
                DiceRoll = roll,
                Modifier = _character.GetSavingThrowBonus(ability)
            };
        }
        public RollResult RollSkill(SkillType skillType, AdvantageState adv)
        {
            var skill = _character.GetSkill(skillType);

            int roll = _diceService.RollD20(adv);

            return new RollResult
            {
                Actor = skill.Name,
                Type = RollType.Check,
                DiceRoll = roll,
                Modifier = skill.GetBonus(_character.GetProficiencyBonus())
            };
        }
        public RollResult RollAttack(AdvantageState adv)
        {
            // TODO: STR/DEX logic // TODO: Weapon Proficiencies
            var weapon = _character.EquippedWeapon;

            var ability = _character.GetAttackAbility(weapon);

            int roll = _diceService.RollD20(adv);

            int modifier = ability.Modifier + _character.GetProficiencyBonus() + weapon.AttackBonus;

            return new RollResult
            {
                Actor = weapon.Name,
                Type = RollType.Attack,
                DiceRoll = roll,
                Modifier = modifier,
                Formula = $"1d20 + {ability.Modifier} {ability.Name} + {_character.GetProficiencyBonus()} proficiency + {weapon.AttackBonus} weapon",
                Description = $"Attack roll with {weapon.Name}",
                SourceItemId = weapon.Id,
            };
        }
        public RollResult RollDamage(Guid weaponId)
        {
            return RollDamage(weaponId,isCritical: false);
        }

        public RollResult RollCriticalDamage(Guid weaponId)
        {
            return RollDamage(weaponId,isCritical: true);
        }

        private RollResult RollDamage(Guid weaponId,bool isCritical)
        {
            var weapon = _character.EquippedWeapon?.Id == weaponId
                ? _character.EquippedWeapon
                : _character.Inventory
                .OfType<Weapon>()
                .FirstOrDefault(w => w.Id == weaponId);

            if (weapon == null)
                throw new InvalidOperationException("Weapon not found.");

            var ability = _character.GetAttackAbility(weapon);

            var damageDice = isCritical
                ? _diceService.DoubleDiceExpression(weapon.DamageDice)
                : weapon.DamageDice;

            int roll = _diceService.RollDice(damageDice);

            int modifier = ability.Modifier;
            int extraDamage = 0;
            var appliedEffects = new List<string>();
            var formulaParts = new List<string>
            {
                damageDice,
                $"{ability.Modifier} {ability.Name}"
              };

            var context = new RollContext
            {
                Character = _character,
                Weapon = weapon,
                Type = RollType.Damage
            };

            foreach (var feature in _character.ClassFeatures)
            {
                if (!feature.IsActive || feature.Modifier == null)
                    continue;

                var mod = feature.Modifier.Apply(context);
                if (!mod.HasEffect)
                    continue;
                modifier += mod.FlatBonus;
                if (mod.FlatBonus != 0)
                    formulaParts.Add($"{mod.FlatBonus} {mod.Source}");

                if (!string.IsNullOrEmpty(mod.ExtraDice))
                {
                    var extraDice = isCritical
                        ? _diceService.DoubleDiceExpression(mod.ExtraDice)
                        : mod.ExtraDice;
                    formulaParts.Add($"{extraDice} {mod.Source}");
                    extraDamage += _diceService.RollDice(extraDice);
                }

                appliedEffects.Add(feature.Name);
            }

            return new RollResult
            {
                Actor = weapon.Name,
                Type = RollType.Damage,
                DiceRoll = roll,
                Modifier = modifier + extraDamage,
                DamageType = weapon.DamageType,
                Formula = string.Join(" + ", formulaParts),
                Description = isCritical
                    ? $"Critical damage roll with {weapon.Name}"
                    : $"Damage roll with {weapon.Name}",
                AppliedEffects = appliedEffects,
                IsCriticalDamage = isCritical
            };
        }
        public RollResult? UseSecondWind()
        {
            var feature = _character.GetFeature(FighterFeatures.SecondWind);

            if (feature == null )
                return null;

            int roll = _diceService.RollDice("1d10");

            int healAmount = roll + _character.Level;

            _character.Heal(healAmount);

            return new RollResult
            {
                Actor = "Second Wind",
                Type = RollType.Heal,
                DiceRoll = roll,
                Modifier = _character.Level,
            };
        }
        public void ToggleFeature(string name)
        {
            var feature = _character.ClassFeatures
                .FirstOrDefault(f => f.Name == name);

            if (feature == null)
                return;
            if (!feature.IsActive && !feature.IsAvailable && feature.MaxUses > 0)
                return;
            if (!feature.IsActive)
            {
                feature.UsesRemaining--;
            }
            
            feature.IsActive = !feature.IsActive;
        }
        public RollResult? UseItem(Guid itemId)
        {
            var item = _character.Inventory.First(x => x.Id == itemId);
            var context = new EffectContext
            {
                Character = _character,
                DiceService = _diceService,
            };
            if (item != null && item.Effect != null)    
            {
                var result = item.Effect.Apply(context);
                if (result != null)
                {
                    _character.Inventory.Remove(item);
                    return result;
                }
            }
            return null;
        }
        public RollResult? UseFeature(string featureName)
        {
            var feature = _character.GetFeature(featureName);

            if (feature == null)
                return null;

            switch (feature.ActionType)
            {
                case FeatureActionType.Use:

                    if (!feature.IsAvailable)
                        return null;

                    feature.UsesRemaining--;

                    var result = HandleFeatureEffect(feature);

                    return result;

                case FeatureActionType.ResourceUse:

                    if (feature.ResourceName == null)
                        return null;

                    bool success =
                        _character.SpendResource(
                            feature.ResourceName,
                            feature.ResourceCost
                        );

                    if (!success)
                        return null;

                    result = HandleFeatureEffect(feature);

                    return result;
            }
            return null;
        }
        private RollResult? HandleFeatureEffect(ClassFeatureInstance feature)
        {
            switch (feature.Name)
            {
                case FighterFeatures.SecondWind:
                    var result = UseSecondWind();
                    return result;

                case FighterFeatures.ActionSurge:
                    break;

                case MonkFeatures.FlurryOfBlows:
                    break;

                case MonkFeatures.PatientDefense:
                    break;

                case MonkFeatures.StepOfTheWind:
                    break;
                default:
                    break;
            }
            return null;
        }
        public RollResult? LevelUp()
        {
            var ability = _character.GetAbility(AbilityType.Constitution);

            var characterClass = CharacterClassFactory.Create(_character.ClassType);

            int roll = _diceService.RollDice($"1d{characterClass.HitDie}");

            int hpGain = roll + ability.Modifier;

            _character.LevelUp(hpGain);

            _character.ClassFeatures = characterClass.GetFeaturesForLevel(_character.Level);
            return new RollResult
            {
                Actor = "Level Up",
                Type = RollType.MaxHP,
                DiceRoll = roll,
                Modifier = ability.Modifier,
            };
        }
        public int GetHitDie()
        {
            var characterClass = CharacterClassFactory.Create(_character.ClassType);

            return characterClass.HitDie;
        }
        public RollResult? ShortRest(int hitDiceCount)
        {
            _character.ShortRest();

            if (hitDiceCount <= 0)
                return null;

            hitDiceCount = Math.Min(hitDiceCount, _character.HitDiceRemaining);

            if (hitDiceCount <= 0)
                return null;

            int hitDie = GetHitDie();
            int constitutionModifier = _character.GetAbility(AbilityType.Constitution).Modifier;

            int diceTotal = 0;

            for (int i = 0; i < hitDiceCount; i++)
            {
                diceTotal += _diceService.RollDice($"1d{hitDie}");
            }

            int modifier = constitutionModifier * hitDiceCount;
            int healAmount = Math.Max(0, diceTotal + modifier);

            _character.SpendHitDice(hitDiceCount);
            _character.Heal(healAmount);

            return new RollResult
            {
                Actor = "Short Rest",
                Type = RollType.Heal,
                DiceRoll = diceTotal,
                Modifier = modifier,
                Formula = $"{hitDiceCount}d{hitDie} + {modifier} CON",
                Description = $"Spent {hitDiceCount} hit dice during a short rest.",
                AppliedEffects = new List<string> { $"Hit Dice" }
            };
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
                HitDiceRemaining = 4,
                MovementSpeed = 30,
                ClassType = CharacterClassType.Fighter,

                Abilities = new List<Ability>
                {
                    strength, dexterity, constitution,
                    intelligence, wisdom, charisma
                },

                Skills = new List<Skill>
                {
                    new Skill { Name = "Athletics", Type = SkillType.Athletics, RelatedAbility = strength, IsProficient = true },
                    new Skill { Name = "Perception", Type = SkillType.Perception, RelatedAbility = wisdom },
                    new Skill { Name = "Stealth", Type = SkillType.Stealth, RelatedAbility = dexterity, IsProficient = true }
                },

                EquippedWeapon = new Weapon
                {
                    Name = "Longsword",
                    AttackBonus = 1,
                    DamageDice = "1d8",
                    DamageType = "slashing",
                    ScalingType = WeaponScalingType.Strength,
                },
                EquippedArmor = new Armor
                {
                    Name = "Leather Tunic",
                    BaseArmorClass = 13
                    
                },
                Inventory = new List<Item>
                {
                    new Weapon 
                    { 
                        Name= "Rapier",
                        AttackBonus = 1,
                        DamageDice = "1d8",
                        DamageType="piercing",
                        ScalingType= WeaponScalingType.Finesse,
                    },
                    new Armor { Name="Chainmail", BaseArmorClass=15},
                    new Item
                    {
                        Name="Healing Potion",
                        Type= ItemType.Consumable,
                        Effect= new HealEffect("2d4+2"),
                    }
                },

            };

            var characterClass = CharacterClassFactory.Create(character.ClassType);

            character.ClassFeatures = characterClass.GetFeaturesForLevel(character.Level);

            return character;
        }
        public Character GetRogueTestCharacter()
        {
            var strength = new Ability { Name = "Strength", Type = AbilityType.Strength, Score = 10 };
            var dexterity = new Ability { Name = "Dexterity", Type = AbilityType.Dexterity, Score = 16, IsSavingThrowProficient = true };
            var constitution = new Ability { Name = "Constitution", Type = AbilityType.Constitution, Score = 14 };
            var intelligence = new Ability { Name = "Intelligence", Type = AbilityType.Intelligence, Score = 12 };
            var wisdom = new Ability { Name = "Wisdom", Type = AbilityType.Wisdom, Score = 13, IsSavingThrowProficient = true };
            var charisma = new Ability { Name = "Charisma", Type = AbilityType.Charisma, Score = 8 };

            var character = new Character
            {
                Name = "Vex",
                Level = 4,
                HitDiceRemaining = 4,
                MovementSpeed = 30,
                ClassType = CharacterClassType.Rogue,

                Abilities = new List<Ability>
        {
            strength, dexterity, constitution,
            intelligence, wisdom, charisma
        },

                Skills = new List<Skill>
        {
            new Skill
            {
                Name = "Stealth",
                Type = SkillType.Stealth,
                RelatedAbility = dexterity,
                IsProficient = true
            },
            new Skill
            {
                Name = "Perception",
                Type = SkillType.Perception,
                RelatedAbility = wisdom,
                IsProficient = true
            },
            new Skill
            {
                Name = "Acrobatics",
                Type = SkillType.Acrobatics,
                RelatedAbility = dexterity,
                IsProficient = true
            },
            new Skill
            {
                Name = "Sleight of Hand",
                Type = SkillType.SleightOfHand,
                RelatedAbility = dexterity,
                IsProficient = true
            }
        },

                EquippedWeapon = new Weapon
                {
                    Name = "Rapier",
                    AttackBonus = 1,
                    DamageDice = "1d8",
                    DamageType = "piercing",
                    ScalingType = WeaponScalingType.Finesse
                },

                EquippedArmor = new Armor
                {
                    Name = "Leather Armor",
                    BaseArmorClass = 11
                },

                Inventory = new List<Item>
        {
            new Weapon
            {
                Name = "Shortbow",
                AttackBonus = 1,
                DamageDice = "1d6",
                DamageType = "piercing",
                ScalingType = WeaponScalingType.Dexterity
            },

            new Weapon
            {
                Name = "Dagger",
                AttackBonus = 1,
                DamageDice = "1d4",
                DamageType = "piercing",
                ScalingType = WeaponScalingType.Finesse
            },

            new Armor
            {
                Name = "Studded Leather",
                BaseArmorClass = 12
            },

            new Item
            {
                Name = "Healing Potion",
                Type = ItemType.Consumable,
                Effect = new HealEffect("2d4+2")
            }
        }
            };

            var characterClass = CharacterClassFactory.Create(character.ClassType);

            character.ClassFeatures = characterClass.GetFeaturesForLevel(character.Level);

            return character;
        }
        public Character GetBarbarianTestCharacter()
        {
            var strength = new Ability
            {
                Name = "Strength",
                Type = AbilityType.Strength,
                Score = 18,
                IsSavingThrowProficient = true
            };

            var dexterity = new Ability
            {
                Name = "Dexterity",
                Type = AbilityType.Dexterity,
                Score = 14
            };

            var constitution = new Ability
            {
                Name = "Constitution",
                Type = AbilityType.Constitution,
                Score = 16,
                IsSavingThrowProficient = true
            };

            var intelligence = new Ability
            {
                Name = "Intelligence",
                Type = AbilityType.Intelligence,
                Score = 8
            };

            var wisdom = new Ability
            {
                Name = "Wisdom",
                Type = AbilityType.Wisdom,
                Score = 12
            };

            var charisma = new Ability
            {
                Name = "Charisma",
                Type = AbilityType.Charisma,
                Score = 10
            };

            var character = new Character
            {
                Name = "Grom",
                Level = 4,
                HitDiceRemaining = 4,
                MovementSpeed = 30,
                ClassType = CharacterClassType.Barbarian,

                Abilities = new List<Ability>
        {
            strength, dexterity, constitution,
            intelligence, wisdom, charisma
        },

                Skills = new List<Skill>
        {
            new Skill
            {
                Name = "Athletics",
                Type = SkillType.Athletics,
                RelatedAbility = strength,
                IsProficient = true
            },

            new Skill
            {
                Name = "Intimidation",
                Type = SkillType.Intimidation,
                RelatedAbility = charisma,
                IsProficient = true
            },

            new Skill
            {
                Name = "Survival",
                Type = SkillType.Survival,
                RelatedAbility = wisdom,
                IsProficient = true
            },

            new Skill
            {
                Name = "Perception",
                Type = SkillType.Perception,
                RelatedAbility = wisdom
            }
        },

                EquippedWeapon = new Weapon
                {
                    Name = "Greataxe",
                    AttackBonus = 1,
                    DamageDice = "1d12",
                    DamageType = "slashing",
                    ScalingType = WeaponScalingType.Strength
                },

                EquippedArmor = new Armor
                {
                    Name = "Hide Armor",
                    BaseArmorClass = 12
                },

                Inventory = new List<Item>
        {
            new Weapon
            {
                Name = "Handaxe",
                AttackBonus = 1,
                DamageDice = "1d6",
                DamageType = "slashing",
                ScalingType = WeaponScalingType.Strength
            },

            new Weapon
            {
                Name = "Javelin",
                AttackBonus = 1,
                DamageDice = "1d6",
                DamageType = "piercing",
                ScalingType = WeaponScalingType.Strength
            },

            new Armor
            {
                Name = "Chain Shirt",
                BaseArmorClass = 13
            },

            new Item
            {
                Name = "Healing Potion",
                Type = ItemType.Consumable,
                Effect = new HealEffect("2d4+2")
            },

            new Item
            {
                Name = "Greater Healing Potion",
                Type = ItemType.Consumable,
                Effect = new HealEffect("4d4+4")
            }
        }
            };

            var characterClass = CharacterClassFactory.Create(character.ClassType);

            character.ClassFeatures =
                characterClass.GetFeaturesForLevel(character.Level);

            return character;
        }
        public Character GetMonkTestCharacter()
        {
            var strength = new Ability
            {
                Name = "Strength",
                Type = AbilityType.Strength,
                Score = 10
            };

            var dexterity = new Ability
            {
                Name = "Dexterity",
                Type = AbilityType.Dexterity,
                Score = 18,
                IsSavingThrowProficient = true
            };

            var constitution = new Ability
            {
                Name = "Constitution",
                Type = AbilityType.Constitution,
                Score = 14
            };

            var intelligence = new Ability
            {
                Name = "Intelligence",
                Type = AbilityType.Intelligence,
                Score = 10
            };

            var wisdom = new Ability
            {
                Name = "Wisdom",
                Type = AbilityType.Wisdom,
                Score = 16,
                IsSavingThrowProficient = true
            };

            var charisma = new Ability
            {
                Name = "Charisma",
                Type = AbilityType.Charisma,
                Score = 8
            };

            var character = new Character
            {
                Name = "Kael",
                Level = 4,
                HitDiceRemaining = 4,
                MovementSpeed = 40,
                ClassType = CharacterClassType.Monk,

                Abilities = new List<Ability>
        {
            strength, dexterity, constitution,
            intelligence, wisdom, charisma
        },

                Skills = new List<Skill>
        {
            new Skill
            {
                Name = "Acrobatics",
                Type = SkillType.Acrobatics,
                RelatedAbility = dexterity,
                IsProficient = true
            },

            new Skill
            {
                Name = "Stealth",
                Type = SkillType.Stealth,
                RelatedAbility = dexterity,
                IsProficient = true
            },

            new Skill
            {
                Name = "Perception",
                Type = SkillType.Perception,
                RelatedAbility = wisdom,
                IsProficient = true
            },

            new Skill
            {
                Name = "Athletics",
                Type = SkillType.Athletics,
                RelatedAbility = strength
            }
        },

                EquippedWeapon = new Weapon
                {
                    Name = "Quarterstaff",
                    AttackBonus = 1,
                    DamageDice = "1d8",
                    DamageType = "bludgeoning",
                    ScalingType = WeaponScalingType.Dexterity
                },

                EquippedArmor = new Armor
                {
                    Name = "Cloth Robes",
                    BaseArmorClass = 10
                },

                Inventory = new List<Item>
        {
            new Weapon
            {
                Name = "Shortsword",
                AttackBonus = 1,
                DamageDice = "1d6",
                DamageType = "piercing",
                ScalingType = WeaponScalingType.Dexterity
            },

            new Weapon
            {
                Name = "Dart",
                AttackBonus = 1,
                DamageDice = "1d4",
                DamageType = "piercing",
                ScalingType = WeaponScalingType.Dexterity
            },

            new Item
            {
                Name = "Healing Potion",
                Type = ItemType.Consumable,
                Effect = new HealEffect("2d4+2")
            }
        },

                FeatureResources = new List<FeatureResource>
        {
            new FeatureResource
            {
                Name = "Ki",
                Current = 4,
                Max = 4
            }
        }
            };

            var characterClass =
                CharacterClassFactory.Create(character.ClassType);

            character.ClassFeatures =
                characterClass.GetFeaturesForLevel(character.Level);

            return character;
        }
    }
}