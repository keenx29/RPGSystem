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
        private readonly List<Character> _characters;
        private Character _character;
        private readonly DiceService _diceService;

        public CharacterService(DiceService diceService)
        {
            _diceService = diceService;

            _characters = new List<Character>
            {
                GetFighterTestCharacter(),
                GetRogueTestCharacter(),
                GetBarbarianTestCharacter(),
                GetMonkTestCharacter()
            };

            _character = _characters.First();
        }
        private RollResult CreateFeatureResult(
            string featureName,
            string description,
            List<string>? appliedEffects = null,
            List<RollExplanation>? explanations = null)
        {
            return new RollResult
            {
                Actor = featureName,
                Type = RollType.Feature,
                DiceRoll = 0,
                Modifier = 0,
                Formula = "",
                Description = description,
                AppliedEffects = appliedEffects ?? new List<string>(),
                Explanations = explanations ?? new List<RollExplanation>()
            };
        }
        public IReadOnlyList<Character> GetCharacters()
        {
            return _characters;
        }

        public void SelectCharacter(Guid characterId)
        {
            var character = _characters.FirstOrDefault(c => c.Id == characterId);

            if (character == null)
                return;

            _character = character;
        }
        public Character GetCharacter()
        {
            return _character;
        }
        private class AdvantageResolution
        {
            public AdvantageState FinalState { get; set; } = AdvantageState.Normal;

            public List<string> AppliedEffects { get; set; } = new();

            public List<RollExplanation> Explanations { get; set; } = new();
        }
        private Weapon? FindWeapon(Guid weaponId)
        {
            var equippedWeapon = _character.EquippedWeapons
                .FirstOrDefault(w => w.Id == weaponId);

            if (equippedWeapon != null)
                return equippedWeapon;

            return _character.Inventory
                .OfType<Weapon>()
                .FirstOrDefault(w => w.Id == weaponId);
        }
        private AdvantageResolution ResolveAdvantage(
    RollType rollType,
    AdvantageState selectedAdvantage,
    AbilityType? abilityType = null)
        {
            var result = new AdvantageResolution();

            bool grantsAdvantage = false;
            bool grantsDisadvantage = false;
            var recklessAttack = _character.GetFeature(BarbarianFeatures.RecklessAttack);

            if (rollType == RollType.Attack && recklessAttack?.IsActive == true)
            {
                if (abilityType == AbilityType.Strength)
                {
                    grantsAdvantage = true;
                    result.AppliedEffects.Add(BarbarianFeatures.RecklessAttack);
                    result.Explanations.Add(new RollExplanation
                    {
                        Type = RollExplanationType.Advantage,
                        Source = BarbarianFeatures.RecklessAttack,
                        Text = "Reckless Attack gives advantage on Strength-based melee weapon attack rolls."
                    });
                }
                else
                {
                    result.Explanations.Add(new RollExplanation
                    {
                        Type = RollExplanationType.Ignored,
                        Source = BarbarianFeatures.RecklessAttack,
                        Text = "Reckless Attack was active but did not apply because this attack does not use Strength."
                    });
                }
            }
            var dangerSense = _character.GetFeature(BarbarianFeatures.DangerSense);

            if (rollType == RollType.Save &&
                abilityType == AbilityType.Dexterity &&
                dangerSense != null)
            {
                bool blocked =
                    _character.HasCondition(ConditionType.Blinded) ||
                    _character.HasCondition(ConditionType.Deafened) ||
                    _character.HasCondition(ConditionType.Incapacitated);

                if (blocked)
                {
                    result.Explanations.Add(new RollExplanation
                    {
                        Type = RollExplanationType.Ignored,
                        Source = BarbarianFeatures.DangerSense,
                        Text = "Danger Sense did not apply because the character is blinded, deafened, or incapacitated."
                    });
                }
                else
                {
                    grantsAdvantage = true;
                    result.AppliedEffects.Add(BarbarianFeatures.DangerSense);
                    result.Explanations.Add(new RollExplanation
                    {
                        Type = RollExplanationType.Advantage,
                        Source = BarbarianFeatures.DangerSense,
                        Text = "Danger Sense gives advantage on Dexterity saving throws in this simplified rules model."
                    });
                }
            }
            if (selectedAdvantage == AdvantageState.Advantage)
            {
                grantsAdvantage = true;
                result.Explanations.Add(new RollExplanation
                {
                    Type = RollExplanationType.Advantage,
                    Source = "Manual Roll Mode",
                    Text = "Player selected advantage for this roll."
                });
            }

            if (selectedAdvantage == AdvantageState.Disadvantage)
            {
                grantsDisadvantage = true;
                result.Explanations.Add(new RollExplanation
                {
                    Type = RollExplanationType.Disadvantage,
                    Source = "Manual Roll Mode",
                    Text = "Player selected disadvantage for this roll."
                });
            }

            if ((rollType == RollType.Attack || rollType == RollType.Check) &&
                _character.HasCondition(ConditionType.Poisoned))
            {
                grantsDisadvantage = true;
                result.AppliedEffects.Add("Poisoned");
                result.Explanations.Add(new RollExplanation
                {
                    Type = RollExplanationType.Condition,
                    Source = "Poisoned",
                    Text = "Poisoned gives disadvantage on attack rolls and ability checks."
                });
            }

            if ((rollType == RollType.Attack || rollType == RollType.Check) &&
                _character.HasCondition(ConditionType.Frightened))
            {
                grantsDisadvantage = true;
                result.AppliedEffects.Add("Frightened");
                result.Explanations.Add(new RollExplanation
                {
                    Type = RollExplanationType.Condition,
                    Source = "Frightened",
                    Text = "Frightened gives disadvantage on attack rolls and ability checks."
                });
            }

            if (rollType == RollType.Attack &&
                _character.HasCondition(ConditionType.Invisible))
            {
                grantsAdvantage = true;
                result.AppliedEffects.Add("Invisible");
                result.Explanations.Add(new RollExplanation
                {
                    Type = RollExplanationType.Condition,
                    Source = "Invisible",
                    Text = "Invisible gives advantage on attack rolls."
                });
            }

            if (rollType == RollType.Save &&
                abilityType == AbilityType.Dexterity &&
                _character.HasCondition(ConditionType.Restrained))
            {
                grantsDisadvantage = true;
                result.AppliedEffects.Add("Restrained");
                result.Explanations.Add(new RollExplanation
                {
                    Type = RollExplanationType.Condition,
                    Source = "Restrained",
                    Text = "Restrained gives disadvantage on Dexterity saving throws."
                });
            }

            if (grantsAdvantage && grantsDisadvantage)
            {
                result.FinalState = AdvantageState.Normal;
                result.Explanations.Add(new RollExplanation
                {
                    Type = RollExplanationType.Cancellation,
                    Source = "Advantage Rules",
                    Text = "Advantage and disadvantage cancel each other out."
                });

                return result;
            }

            result.FinalState = grantsAdvantage
                ? AdvantageState.Advantage
                : grantsDisadvantage
                    ? AdvantageState.Disadvantage
                    : AdvantageState.Normal;

            return result;
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
            var advantage = ResolveAdvantage(RollType.Check, adv, type);
            int roll = _diceService.RollD20(advantage.FinalState);
            return new RollResult
            {
                Actor = ability.Name,
                Type = RollType.Check,
                DiceRoll = roll,
                NaturalRoll = roll,
                Modifier = ability.Modifier,
                Formula = $"1d20 + {ability.Modifier} {ability.Name}",
                Description = $"Ability check",
                AdvantageType = advantage.FinalState,
                AppliedEffects = advantage.AppliedEffects,
                Explanations = advantage.Explanations
            };
        }
        public RollResult RollSavingThrow(AbilityType type, AdvantageState adv)
        {
            var ability = _character.GetAbility(type);
            var advantage = ResolveAdvantage(RollType.Save, adv, type);
            int roll = _diceService.RollD20(advantage.FinalState);
            var proficiencyBonus =  _character.GetSavingThrowBonus(ability) - ability.Modifier;
            var formula = $"1d20 + {ability.Modifier} {ability.Name} Save";
            if (proficiencyBonus != 0)
            {
                formula += $" + {proficiencyBonus} Proficiency";
            }
            return new RollResult
            {
                Actor = ability.Name,
                Type = RollType.Save,
                DiceRoll = roll,
                NaturalRoll = roll,
                Modifier = _character.GetSavingThrowBonus(ability),
                Formula = formula,
                Description = $"Saving throw",
                AdvantageType = advantage.FinalState,
                AppliedEffects = advantage.AppliedEffects,
                Explanations = advantage.Explanations
            };
        }
        public RollResult RollSkill(SkillType skillType, AdvantageState adv)
        {
            var skill = _character.GetSkill(skillType);
            var advantage = ResolveAdvantage(RollType.Check, adv, skill.RelatedAbility.Type);
            int roll = _diceService.RollD20(advantage.FinalState);
            var proficiencyBonus = _character.GetProficiencyBonus();
            var skillBonus = skill.GetBonus(proficiencyBonus);

            var formula = $"1d20 + {skill.RelatedAbility.Modifier} {skill.RelatedAbility.Name}";

            if (skill.IsExpertise)
                formula += $" + {proficiencyBonus * 2} Expertise";
            else if (skill.IsProficient)
                formula += $" + {proficiencyBonus} Proficiency";

            var explanations = advantage.Explanations;

            if (skill.IsExpertise)
            {
                explanations.Add(new RollExplanation
                {
                    Type = RollExplanationType.Bonus,
                    Source = RogueFeatures.Expertise,
                    Text = "Expertise doubles the proficiency bonus for this skill.",
                    Value = proficiencyBonus * 2
                });
            }
            else if (skill.IsProficient)
            {
                explanations.Add(new RollExplanation
                {
                    Type = RollExplanationType.Bonus,
                    Source = "Proficiency",
                    Text = "Proficiency bonus is added to this skill check.",
                    Value = proficiencyBonus
                });
            }
            return new RollResult
            {
                Actor = skill.Name,
                Type = RollType.Check,
                DiceRoll = roll,
                NaturalRoll = roll,
                Modifier = skillBonus,
                Formula = formula,
                Description = $"{skill.Name} skill check",
                AdvantageType = advantage.FinalState,
                AppliedEffects = advantage.AppliedEffects,
                Explanations = explanations,
            };
        }
        public RollResult RollAttack(AdvantageState adv)
        {
            var weapon = _character.EquippedWeapons.FirstOrDefault();

            if (weapon == null)
                throw new InvalidOperationException("Character has no equipped weapon.");

            return RollAttack(weapon.Id, adv);
        }
        public RollResult RollAttack(Guid weaponId, AdvantageState adv)
        {
            var weapon = FindWeapon(weaponId);

            if (weapon == null)
                throw new InvalidOperationException("Weapon not found.");

            var ability = _character.GetAttackAbility(weapon);

            var advantage = ResolveAdvantage(RollType.Attack, adv, ability.Type);
            
            int roll = _diceService.RollD20(advantage.FinalState);

            var proficiencyBonus = _character.IsProficientWithWeapon(weapon)
                ? _character.GetProficiencyBonus()
                : 0;
            int modifier = ability.Modifier + proficiencyBonus + weapon.AttackBonus;

            var formula = $"1d20 + {ability.Modifier} {ability.Name}";

            if (proficiencyBonus != 0)
            {
                formula += $" + {proficiencyBonus} Proficiency";
            }
            if (weapon.AttackBonus != 0) 
            {
                formula += $" + {weapon.AttackBonus} Weapon Bonus";
            }
            var explanations = advantage.Explanations;

            explanations.Add(new RollExplanation
            {
                Type = RollExplanationType.Info,
                Source = weapon.Name,
                Text = $"{weapon.Name} uses {ability.Name} for this attack based on its scaling type."
            });

            return new RollResult
            {
                Actor = weapon.Name,
                Type = RollType.Attack,
                DiceRoll = roll,
                NaturalRoll = roll,
                Modifier = modifier,
                Formula = formula,
                Description = $"Attack roll with {weapon.Name}",
                SourceItemId = weapon.Id,
                AppliedEffects = advantage.AppliedEffects,
                Explanations = explanations,
                AdvantageType = advantage.FinalState
            };
        }
        public RollResult RollDamage(Guid weaponId)
        {
            return RollDamage(weaponId, isCritical: false);
        }

        public RollResult RollCriticalDamage(Guid weaponId)
        {
            return RollDamage(weaponId, isCritical: true);
        }

        private RollResult RollDamage(Guid weaponId, bool isCritical)
        {
            var weapon = FindWeapon(weaponId);

            if (weapon == null)
                throw new InvalidOperationException("Weapon not found.");

            var explanations = new List<RollExplanation>();

            var ability = _character.GetAttackAbility(weapon);

            explanations.Add(new RollExplanation
            {
                Type = RollExplanationType.Info,
                Source = weapon.Name,
                Text = $"{weapon.Name} damage uses {ability.Name} because of the weapon scaling type."
            });

            var damageDice = isCritical
                ? _diceService.DoubleDiceExpression(weapon.DamageDice)
                : weapon.DamageDice;

            
            if (isCritical)
            {
                explanations.Add(new RollExplanation
                {
                    Type = RollExplanationType.Critical,
                    Source = "Critical Hit",
                    Text = "Critical damage doubles the weapon damage dice.",
                    Dice = damageDice
                });
            }

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
                Ability = ability,
                Type = RollType.Damage,
                IsCriticalDamage = isCritical
            };

            foreach (var feature in _character.ClassFeatures)
            {
                if (!feature.IsActive || feature.Modifier == null)
                    continue;

                var mod = feature.Modifier.Apply(context);

                var source = string.IsNullOrWhiteSpace(mod.Source)
                    ? feature.Name
                    : mod.Source;

                if (mod.WasIgnored)
                {
                    explanations.Add(new RollExplanation
                    {
                        Type = RollExplanationType.Ignored,
                        Source = source,
                        Text = mod.IgnoreReason
                    });
                }

                if (!mod.HasEffect)
                    continue;

                modifier += mod.FlatBonus;

                if (mod.FlatBonus != 0)
                {
                    formulaParts.Add($"{mod.FlatBonus} {source}");

                    explanations.Add(new RollExplanation
                    {
                        Type = RollExplanationType.Bonus,
                        Source = source,
                        Text = mod.Description,
                        Value = mod.FlatBonus
                    });
                }

                if (!string.IsNullOrEmpty(mod.ExtraDice))
                {
                    var extraDice = isCritical
                        ? _diceService.DoubleDiceExpression(mod.ExtraDice)
                        : mod.ExtraDice;

                    formulaParts.Add($"{extraDice} {source}");
                    extraDamage += _diceService.RollDice(extraDice);

                    explanations.Add(new RollExplanation
                    {
                        Type = RollExplanationType.ExtraDice,
                        Source = source,
                        Text = isCritical
                            ? $"{mod.Description} Critical damage doubles these extra dice."
                            : mod.Description,
                        Dice = extraDice
                    });
                }

                appliedEffects.Add(source);
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
                IsCriticalDamage = isCritical,
                Explanations = explanations,
            };
        }
        public RollResult? UseSecondWind()
        {
            var feature = _character.GetFeature(FighterFeatures.SecondWind);

            if (feature == null)
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
                Formula = $"1d10 + {_character.Level} Fighter level",
                Description = $"Heal roll using Second Wind",
                AppliedEffects = new List<string> { FighterFeatures.SecondWind },
                Explanations = new List<RollExplanation>
                {
                    new RollExplanation
                    {
                        Type = RollExplanationType.Feature,
                        Source = FighterFeatures.SecondWind,
                        Text = "Second Wind restores 1d10 + fighter level hit points."
                    }
                }
            };
        }
        public RollResult? ToggleFeature(string name)
        {
            var feature = _character.ClassFeatures
                .FirstOrDefault(f => f.Name == name);

            if (feature == null)
                return null;
            if (!feature.IsActive && feature.MaxUses > 0)
            {
                if (!feature.IsAvailable)
                    return null;

                feature.UsesRemaining--;
            }

            feature.IsActive = !feature.IsActive;
            return CreateFeatureResult(
                feature.Name,
                feature.IsActive
                    ? $"{feature.Name} is now active."
                    : $"{feature.Name} is no longer active.",
                new List<string> { feature.IsActive ? "Active" : "Inactive" },
                new List<RollExplanation>
                {
                    new RollExplanation
                    {
                        Type = RollExplanationType.Feature,
                        Source = feature.Name,
                        Text = feature.IsActive
                            ? $"{feature.Name} has been enabled."
                            : $"{feature.Name} has been disabled."
                    }
                });
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
        private RollResult? ExecuteFeatureAction(ClassFeatureInstance feature)
        {
            if (feature.Action == null)
                return CreateFeatureResult(
                    feature.Name,
                    $"{feature.Name} was used.",
                    new List<string> { feature.Name },
                    new List<RollExplanation>
                    {
                new RollExplanation
                {
                    Type = RollExplanationType.Feature,
                    Source = feature.Name,
                    Text = "This feature does not have a custom action yet."
                }
                    });

            return feature.Action.Execute(_character, feature, _diceService);
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

                    if (feature.MaxUses > 0)
                        feature.UsesRemaining--;

                    return ExecuteFeatureAction(feature);

                case FeatureActionType.ResourceUse:
                    if (feature.ResourceName == null)
                        return null;

                    bool success = _character.SpendResource(
                        feature.ResourceName,
                        feature.ResourceCost);

                    if (!success)
                        return null;

                    return ExecuteFeatureAction(feature);
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

            _character.FeatureResources = characterClass.GetResourcesForLevel(_character.Level);
            
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
        public void UnequipWeapon(Guid weaponId)
        {
            _character.UnequipWeapon(weaponId);
        }
        public void EquipArmor(Guid armorId)
        {
            var armor = _character.Inventory.OfType<Armor>().First(a => a.Id == armorId);

            _character.EquipArmor(armor);
        }
        public void UnequipArmor(Guid armorId)
        {
            _character.UnequipArmor(armorId);
        }
        public void EquipShield(Guid shieldId)
        {
            var shield = _character.Inventory
                .OfType<Armor>()
                .First(a => a.Id == shieldId && a.ArmorType == ArmorType.Shield);

            _character.EquipShield(shield);
        }

        public void UnequipShield(Guid shieldId)
        {
            _character.UnequipShield(shieldId);
        }
        public Character GetFighterTestCharacter()
        {
            var strength = new Ability { Name = "Strength", Type = AbilityType.Strength, Score = 16 };
            var dexterity = new Ability { Name = "Dexterity", Type = AbilityType.Dexterity, Score = 14 };
            var constitution = new Ability { Name = "Constitution", Type = AbilityType.Constitution, Score = 14 };
            var intelligence = new Ability { Name = "Intelligence", Type = AbilityType.Intelligence, Score = 10 };
            var wisdom = new Ability { Name = "Wisdom", Type = AbilityType.Wisdom, Score = 12 };
            var charisma = new Ability { Name = "Charisma", Type = AbilityType.Charisma, Score = 8 };


            var character = new Character
            {
                Name = "Tyrion",
                Level = 5,
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
                    new Skill { Name = "Athletics", Type = SkillType.Athletics, RelatedAbility = strength},
                    new Skill { Name = "Perception", Type = SkillType.Perception, RelatedAbility = wisdom },
                    new Skill { Name = "Stealth", Type = SkillType.Stealth, RelatedAbility = dexterity }
                },

                EquippedWeapon = new Weapon
                {
                    Name = "Longsword",
                    AttackBonus = 1,
                    DamageDice = "1d8",
                    DamageType = "slashing",
                    ScalingType = WeaponScalingType.Strength,
                    ProficiencyType = WeaponProficiencyType.Simple,
                    ProficiencyName = "Longsword"
                },
                EquippedArmor = new Armor
                {
                    Name = "Leather Armor",
                    BaseArmorClass = 13,
                    ArmorType = ArmorType.Light,

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
                        ProficiencyType = WeaponProficiencyType.Martial,
                        ProficiencyName = "Rapier"
                    },
                    new Armor
                    {
                        Name="Hide Armor",
                        BaseArmorClass=15,
                        ArmorType= ArmorType.Medium,
                    },
                    new Armor
                    {
                        Name="Chainmail",
                        BaseArmorClass=15,
                        ArmorType= ArmorType.Heavy,
                    },
                    new Armor
                    {
                        Name = "Shield",
                        BaseArmorClass = 2,
                        ArmorType = ArmorType.Shield
                    },
                    new Item
                    {
                        Name="Healing Potion",
                        Type= ItemType.Consumable,
                        Effect= new HealEffect("2d4+2"),
                    }
                },

            };
            character.ApplySkillProficiencies(new[]
                {
                    SkillType.Athletics,
                    SkillType.Perception
                });
            ApplyClassSetup(character);
            return character;
        }
        public Character GetRogueTestCharacter()
        {
            var strength = new Ability { Name = "Strength", Type = AbilityType.Strength, Score = 10 };
            var dexterity = new Ability { Name = "Dexterity", Type = AbilityType.Dexterity, Score = 16 };
            var constitution = new Ability { Name = "Constitution", Type = AbilityType.Constitution, Score = 14 };
            var intelligence = new Ability { Name = "Intelligence", Type = AbilityType.Intelligence, Score = 12 };
            var wisdom = new Ability { Name = "Wisdom", Type = AbilityType.Wisdom, Score = 13 };
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
            },
            new Skill
            {
                Name = "Perception",
                Type = SkillType.Perception,
                RelatedAbility = wisdom,
            },
            new Skill
            {
                Name = "Acrobatics",
                Type = SkillType.Acrobatics,
                RelatedAbility = dexterity,
            },
            new Skill
            {
                Name = "Sleight of Hand",
                Type = SkillType.SleightOfHand,
                RelatedAbility = dexterity,
            }
        },

                EquippedWeapon = new Weapon
                {
                    Name = "Rapier",
                    AttackBonus = 1,
                    DamageDice = "1d8",
                    DamageType = "piercing",
                    ScalingType = WeaponScalingType.Finesse,
                    ProficiencyType = WeaponProficiencyType.Martial,
                    ProficiencyName = "Rapier"
                },

                EquippedArmor = new Armor
                {
                    Name = "Leather Armor",
                    BaseArmorClass = 11,
                    ArmorType = ArmorType.Light
                },

                Inventory = new List<Item>
        {
            new Weapon
            {
                Name = "Greataxe",
                AttackBonus = 1,
                DamageDice = "1d12",
                DamageType = "slashing",
                ScalingType = WeaponScalingType.Strength,
                ProficiencyType = WeaponProficiencyType.Martial,
                ProficiencyName = "Greataxe"
            },
            new Weapon
            {
                Name = "Shortbow",
                AttackBonus = 1,
                DamageDice = "1d6",
                DamageType = "piercing",
                ScalingType = WeaponScalingType.Dexterity,
                ProficiencyType = WeaponProficiencyType.Simple,
                ProficiencyName = "Shortbow"
            },

            new Weapon
            {
                Name = "Dagger",
                AttackBonus = 1,
                DamageDice = "1d4",
                DamageType = "piercing",
                ScalingType = WeaponScalingType.Finesse,
                ProficiencyType = WeaponProficiencyType.Simple,
                ProficiencyName = "Dagger"
            },
            new Armor
            {
                Name="Hide Armor",
                BaseArmorClass=15,
                ArmorType= ArmorType.Medium,
            },
            new Armor
            {
                Name="Chainmail",
                BaseArmorClass=15,
                ArmorType= ArmorType.Heavy,
            },

            new Item
            {
                Name = "Healing Potion",
                Type = ItemType.Consumable,
                Effect = new HealEffect("2d4+2")
            }
        }
            };
            character.ApplySkillProficiencies(new[]
            {
                SkillType.Stealth,
                SkillType.Perception,
                SkillType.Acrobatics,
                SkillType.SleightOfHand
            });

            character.ApplySkillExpertise(new[]
            {
                SkillType.Stealth,
                SkillType.SleightOfHand
            });
            ApplyClassSetup(character);
            return character;
        }
        public Character GetBarbarianTestCharacter()
        {
            var strength = new Ability
            {
                Name = "Strength",
                Type = AbilityType.Strength,
                Score = 18
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
                Score = 16
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
            },

            new Skill
            {
                Name = "Intimidation",
                Type = SkillType.Intimidation,
                RelatedAbility = charisma,
            },

            new Skill
            {
                Name = "Survival",
                Type = SkillType.Survival,
                RelatedAbility = wisdom,
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
                    ScalingType = WeaponScalingType.Strength,
                    ProficiencyType = WeaponProficiencyType.Martial,
                    ProficiencyName = "Greataxe"
                },

                EquippedArmor = new Armor
                {
                    Name = "Hide Armor",
                    BaseArmorClass = 12,
                    ArmorType = ArmorType.Medium,
                },

                Inventory = new List<Item>
        {
            new Weapon
            {
                Name = "Handaxe",
                AttackBonus = 1,
                DamageDice = "1d6",
                DamageType = "slashing",
                ScalingType = WeaponScalingType.Strength,
                ProficiencyType = WeaponProficiencyType.Simple,
                ProficiencyName = "Handaxe"
            },

            new Weapon
            {
                Name = "Javelin",
                AttackBonus = 1,
                DamageDice = "1d6",
                DamageType = "piercing",
                ScalingType = WeaponScalingType.Strength,
                ProficiencyType = WeaponProficiencyType.Simple,
                ProficiencyName = "Javelin"
            },
            new Weapon
            {
                Name = "Shortbow",
                AttackBonus = 1,
                DamageDice = "1d6",
                DamageType = "piercing",
                ScalingType = WeaponScalingType.Dexterity,
                ProficiencyType = WeaponProficiencyType.Simple,
                ProficiencyName = "Shortbow"
            },
            new Armor
            {
                Name = "Shield",
                BaseArmorClass = 2,
                ArmorType = ArmorType.Shield
            },
            new Armor
            {
                Name = "Leather Armor",
                BaseArmorClass = 13,
                ArmorType = ArmorType.Light,

            },
            new Armor
            {
                Name="Chainmail",
                BaseArmorClass=15,
                ArmorType= ArmorType.Heavy,
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
            character.ApplySkillProficiencies(new[]
            {
                SkillType.Athletics,
                SkillType.Intimidation,
                SkillType.Survival
            });
            ApplyClassSetup(character);
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
                Score = 18
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
                Score = 16
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
                MovementSpeed = 30,
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
                    },

                    new Skill
                    {
                        Name = "Stealth",
                        Type = SkillType.Stealth,
                        RelatedAbility = dexterity,
                    },

                    new Skill
                    {
                        Name = "Perception",
                        Type = SkillType.Perception,
                        RelatedAbility = wisdom,
                    },

                    new Skill
                    {
                        Name = "Athletics",
                        Type = SkillType.Athletics,
                        RelatedAbility = strength
                    }
                },

                EquippedWeapons = new List<Weapon>
            {
                new Weapon
                {
                    Name = "Quarterstaff",
                    AttackBonus = 1,
                    DamageDice = "1d8",
                    DamageType = "bludgeoning",
                    ScalingType = WeaponScalingType.Dexterity,
                    ProficiencyType = WeaponProficiencyType.Simple,
                    ProficiencyName = "Quarterstaff"
                },
                new Weapon
                {
                    Name = "Unarmed Strike",
                    AttackBonus = 0,
                    DamageDice = "1d4",
                    DamageType = "bludgeoning",
                    ScalingType = WeaponScalingType.Dexterity,
                    ProficiencyType = WeaponProficiencyType.Simple,
                    ProficiencyName = "Unarmed Strike"
                }
            },


                Inventory = new List<Item>
        {
            new Weapon
            {
                Name = "Shortsword",
                AttackBonus = 1,
                DamageDice = "1d6",
                DamageType = "piercing",
                ScalingType = WeaponScalingType.Dexterity,
                ProficiencyType = WeaponProficiencyType.Martial,
                ProficiencyName = "Shortsword"
            },
            new Weapon
            {
                Name= "Rapier",
                AttackBonus = 1,
                DamageDice = "1d8",
                DamageType="piercing",
                ScalingType= WeaponScalingType.Finesse,
                ProficiencyType = WeaponProficiencyType.Martial,
                ProficiencyName = "Rapier"
            },

            new Weapon
            {
                Name = "Dart",
                AttackBonus = 1,
                DamageDice = "1d4",
                DamageType = "piercing",
                ScalingType = WeaponScalingType.Dexterity,
                ProficiencyType = WeaponProficiencyType.Simple,
                ProficiencyName = "Dart"
            },
            new Armor
            {
                Name = "Leather Armor",
                BaseArmorClass = 13,
                ArmorType = ArmorType.Light,

            },

            new Item
            {
                Name = "Healing Potion",
                Type = ItemType.Consumable,
                Effect = new HealEffect("2d4+2")
            }
        },
            };
            character.ApplySkillProficiencies(new[]
            {
                SkillType.Acrobatics,
                SkillType.Stealth,
                SkillType.Perception
            });
            ApplyClassSetup(character);
            return character;
        }
        private void ApplyClassSetup(Character character)
        {
            var characterClass = CharacterClassFactory.Create(character.ClassType);

            character.ApplySavingThrowProficiencies(characterClass.SavingThrowProficiencies);
            character.ClassFeatures = characterClass.GetFeaturesForLevel(character.Level);
            character.FeatureResources = characterClass.GetResourcesForLevel(character.Level);
        }
    }
}