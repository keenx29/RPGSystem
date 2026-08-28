using RPGSystem.Data.Entities;
using RPGSystem.Helpers;
using RPGSystem.Models.Characters;
using RPGSystem.Models.Classes;
using RPGSystem.Models.Classes.Features;
using RPGSystem.Models.Items;
using RPGSystem.Models.Rolls;
using RPGSystem.ViewModels;

namespace RPGSystem.Services
{
    public class CharacterService
    {
        private readonly List<Character> _characters;
        private Character _character;
        private readonly DiceService _diceService;
        private readonly CharacterPersistenceService _persistenceService;

        public CharacterService(
            DiceService diceService,
            CharacterPersistenceService persistenceService)
        {
            _diceService = diceService;
            _persistenceService = persistenceService;

            _characters = new List<Character>
            {
                GetFighterTestCharacter(),
                GetRogueTestCharacter(),
                GetBarbarianTestCharacter(),
                GetMonkTestCharacter()
            };

            ApplySavedCharacterStates();

            _character = _characters.First();
        }
        private void ApplySavedCharacterStates()
        {
            var savedCharacters = _persistenceService
                .LoadCharacterStates()
                .ToDictionary(c => c.Id);

            foreach (var character in _characters)
            {
                if (savedCharacters.TryGetValue(character.Id, out var savedCharacter))
                {
                    ApplySavedState(character, savedCharacter);
                }
            }
        }
        private void ApplySavedState(Character character, CharacterEntity savedCharacter)
        {
            character.Name = savedCharacter.Name;
            character.Level = savedCharacter.Level;
            character.MaxHP = savedCharacter.MaxHP;
            character.CurrentHP = savedCharacter.CurrentHP;
            character.MovementSpeed = savedCharacter.MovementSpeed;
            character.HitDiceRemaining = savedCharacter.HitDiceRemaining;
            character.PendingAbilityScoreImprovementPoints = savedCharacter.PendingAbilityScoreImprovementPoints;
            character.DeathSaveSuccesses = savedCharacter.DeathSaveSuccesses;
            character.DeathSaveFailures = savedCharacter.DeathSaveFailures;
            character.IsStable = savedCharacter.IsStable;
            character.IsDead = savedCharacter.IsDead;
            character.Race = savedCharacter.Race;
            character.Background = savedCharacter.Background;
            character.Alignment = savedCharacter.Alignment;
            character.PersonalityTraits = savedCharacter.PersonalityTraits;
            character.Ideals = savedCharacter.Ideals;
            character.Bonds = savedCharacter.Bonds;
            character.Flaws = savedCharacter.Flaws;
            character.Notes = savedCharacter.Notes;

            character.Inventory = savedCharacter.Items
                .Where(i => i.Location == "Inventory")
                .Select(ToItem)
                .ToList();

            character.EquippedWeapons = savedCharacter.Items
                .Where(i => i.Location == "EquippedWeapon")
                .Select(ToItem)
                .OfType<Weapon>()
                .ToList();

            character.EquippedArmor = savedCharacter.Items
                .Where(i => i.Location == "EquippedArmor")
                .Select(ToItem)
                .OfType<Armor>()
                .FirstOrDefault();

            character.EquippedShield = savedCharacter.Items
                .Where(i => i.Location == "EquippedShield")
                .Select(ToItem)
                .OfType<Armor>()
                .FirstOrDefault();

            foreach (var savedAbility in savedCharacter.Abilities)
            {
                var ability = character.GetAbility(savedAbility.Type);
                ability.Score = savedAbility.Score;
                ability.IsSavingThrowProficient = savedAbility.IsSavingThrowProficient;
            }

            foreach (var savedSkill in savedCharacter.Skills)
            {
                var skill = character.GetSkill(savedSkill.Type);
                skill.IsProficient = savedSkill.IsProficient;
                skill.IsExpertise = savedSkill.IsExpertise;
            }

            RefreshClassProgression(character);
        }
        private void ApplyClassSetup(Character character)
        {
            var characterClass = CharacterClassFactory.Create(character.ClassType);

            character.ApplySavingThrowProficiencies(characterClass.SavingThrowProficiencies);

            RefreshClassProgression(character);
        }

        private void RefreshClassProgression(Character character)
        {
            var characterClass = CharacterClassFactory.Create(character.ClassType);

            character.ClassFeatures = characterClass.GetFeaturesForLevel(character.Level);
            character.FeatureResources = characterClass.GetResourcesForLevel(character.Level);
        }
        public void SaveCharacters()
        {
            _persistenceService.SaveCharacters(_characters);
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
            var explanations = new List<RollExplanation>(advantage.Explanations);

            explanations.Add(new RollExplanation
            {
                Type = RollExplanationType.Info,
                Source = ability.Name,
                Text = $"{ability.Name} modifier applied: {ability.Modifier:+#;-#;0}."
            });
            return new RollResult
            {
                Actor = ability.Name,
                Type = RollType.Check,
                DiceRoll = roll,
                NaturalRoll = roll,
                Modifier = ability.Modifier,
                Formula = $"1d20 {ability.Modifier:+ #;- #;0} {ability.Name}",
                Description = $"Ability check",
                AdvantageType = advantage.FinalState,
                AppliedEffects = advantage.AppliedEffects,
                Explanations = explanations,
            };
        }
        public RollResult RollSavingThrow(AbilityType type, AdvantageState adv)
        {
            var ability = _character.GetAbility(type);
            var advantage = ResolveAdvantage(RollType.Save, adv, type);
            int roll = _diceService.RollD20(advantage.FinalState);
            var proficiencyBonus =  _character.GetSavingThrowBonus(ability) - ability.Modifier;
            var formula = $"1d20 {ability.Modifier:+ #;- #;0} {ability.Name}";

            var explanations = new List<RollExplanation>(advantage.Explanations);

            explanations.Add(new RollExplanation
            {
                Type = RollExplanationType.Info,
                Source = ability.Name,
                Text = $"{ability.Name} modifier applied: {ability.Modifier:+#;-#;0}."
            });

            if (ability.IsSavingThrowProficient)
            {
                formula += $" + {_character.GetProficiencyBonus()} Proficiency";
                explanations.Add(new RollExplanation
                {
                    Type = RollExplanationType.Info,
                    Source = "Proficiency",
                    Text = $"Saving throw proficiency bonus applied: +{_character.GetProficiencyBonus()}."
                });
            }
            else
            {
                explanations.Add(new RollExplanation
                {
                    Type = RollExplanationType.Info,
                    Source = "Proficiency",
                    Text = "No saving throw proficiency bonus applied."
                });
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
                Explanations = explanations,
            };
        }
        public RollResult RollSkill(SkillType skillType, AdvantageState adv)
        {
            //TODO: Formula for negative modifiers
            var skill = _character.GetSkill(skillType);
            var advantage = ResolveAdvantage(RollType.Check, adv, skill.RelatedAbility.Type);
            int roll = _diceService.RollD20(advantage.FinalState);
            var proficiencyBonus = _character.GetProficiencyBonus();
            var skillBonus = skill.GetBonus(proficiencyBonus);
            var formula = $"1d20 {skill.RelatedAbility.Modifier:+ #;- #;0} {skill.RelatedAbility.Name}";
            var explanations = new List<RollExplanation>(advantage.Explanations);

            explanations.Add(new RollExplanation
            {
                Type = RollExplanationType.Info,
                Source = skill.RelatedAbility.Name,
                Text = $"{skill.RelatedAbility.Name} modifier applied: {skill.RelatedAbility.Modifier:+#;-#;0}."
            });

            if (skill.IsExpertise)
            {
                formula += $" + {_character.GetProficiencyBonus() * 2} Expertise";
                
                explanations.Add(new RollExplanation
                {
                    Type = RollExplanationType.Info,
                    Source = "Expertise",
                    Text = $"Expertise applied: double proficiency bonus: +{_character.GetProficiencyBonus() * 2}."
                });
            }
            else if (skill.IsProficient)
            {
                formula += $" + {_character.GetProficiencyBonus()} Proficiency";
                
                explanations.Add(new RollExplanation
                {
                    Type = RollExplanationType.Info,
                    Source = "Proficiency",
                    Text = $"Skill proficiency bonus applied: +{_character.GetProficiencyBonus()}."
                });
            }
            else
            {
                explanations.Add(new RollExplanation
                {
                    Type = RollExplanationType.Info,
                    Source = "Proficiency",
                    Text = "No skill proficiency bonus applied."
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
            {
                return CreateFeedback("Weapon was not found for this attack roll.");
            }

            var ability = _character.GetAttackAbility(weapon);

            var advantage = ResolveAdvantage(RollType.Attack, adv, ability.Type);

            int roll = _diceService.RollD20(advantage.FinalState);

            var isProficient = _character.IsProficientWithWeapon(weapon);

            var proficiencyBonus = isProficient
                ? _character.GetProficiencyBonus()
                : 0;

            int modifier = ability.Modifier + proficiencyBonus + weapon.AttackBonus;

            var formula = $"1d20 {ability.Modifier:+ #;- #;0} {ability.Name}";

            if (proficiencyBonus != 0)
            {
                formula += $" + {proficiencyBonus} Proficiency";
            }

            if (weapon.AttackBonus != 0)
            {
                formula += $" {weapon.AttackBonus:+#;-#;0} Weapon Bonus";
            }

            var explanations = new List<RollExplanation>(advantage.Explanations);

            explanations.Add(new RollExplanation
            {
                Type = RollExplanationType.Info,
                Source = weapon.Name,
                Text = $"{weapon.Name} uses {ability.Name} for this attack based on its scaling type."
            });

            explanations.Add(new RollExplanation
            {
                Type = RollExplanationType.Info,
                Source = ability.Name,
                Text = $"{ability.Name} modifier applied: {ability.Modifier:+#;-#;0}."
            });

            explanations.Add(new RollExplanation
            {
                Type = RollExplanationType.Info,
                Source = "Proficiency",
                Text = isProficient
                    ? $"Character is proficient with {weapon.Name}, so proficiency bonus is added: +{proficiencyBonus}."
                    : $"Character is not proficient with {weapon.Name}, so proficiency bonus is not added."
            });

            if (weapon.AttackBonus != 0)
            {
                explanations.Add(new RollExplanation
                {
                    Type = RollExplanationType.Info,
                    Source = weapon.Name,
                    Text = $"Weapon attack bonus applied: {weapon.AttackBonus:+#;-#;0}."
                });
            }

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
            {
                return CreateFeedback("Weapon was not found for this damage roll.");
            }

            var ability = _character.GetAttackAbility(weapon);
            var explanations = new List<RollExplanation>
            {
                new RollExplanation
                {
                    Type = RollExplanationType.Info,
                    Source = weapon.Name,
                    Text = $"{weapon.Name} deals {weapon.DamageDice} {weapon.DamageType} damage."
                },
                new RollExplanation
                {
                    Type = RollExplanationType.Info,
                    Source = ability.Name,
                    Text = $"{ability.Name} damage modifier applied: {ability.Modifier:+#;-#;0}."
                }
            };
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
        public RollResult RollDeathSave(AdvantageState adv)
        {
            if (!_character.ShouldMakeDeathSaves)
            {
                return CreateFeedback("Death saving throws are only needed at 0 HP while not stable or dead.");
            }

            var advantage = ResolveAdvantage(RollType.DeathSave, adv);

            int roll = _diceService.RollD20(advantage.FinalState);

            _character.ApplyDeathSavingThrow(roll);

            var explanations = new List<RollExplanation>(advantage.Explanations)
            {
                new RollExplanation
                {
                    Type = RollExplanationType.Info,
                    Source = "Death Save",
                    Text = "A death saving throw succeeds on 10 or higher and fails on 9 or lower."
                }
            };

            if (roll == 1)
            {
                explanations.Add(new RollExplanation
                {
                    Type = RollExplanationType.Critical,
                    Source = "Natural 1",
                    Text = "A natural 1 counts as two death save failures."
                });
            }

            if (roll == 20)
            {
                explanations.Add(new RollExplanation
                {
                    Type = RollExplanationType.Critical,
                    Source = "Natural 20",
                    Text = "A natural 20 restores the character to 1 HP."
                });
            }

            return new RollResult
            {
                Actor = _character.Name,
                Type = RollType.DeathSave,
                DiceRoll = roll,
                NaturalRoll = roll,
                Modifier = 0,
                Formula = "1d20",
                Description = "Death saving throw",
                AppliedEffects = advantage.AppliedEffects,
                Explanations = explanations,
                AdvantageType = advantage.FinalState
            };
        }
        public RollResult Stabilize()
        {
            if (_character.CurrentHP > 0)
            {
                return CreateFeedback("The character does not need to be stabilized.");
            }

            if (_character.IsDead)
            {
                return CreateFeedback("The character cannot be stabilized because they are dead.");
            }

            _character.Stabilize();

            return CreateFeatureResult(
                "Stabilize",
                $"{_character.Name} is stable and no longer making death saving throws.");
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
            var item = _character.Inventory.FirstOrDefault(x => x.Id == itemId);

            if (item == null)
            {
                return CreateFeedback("Item was not found in inventory.");
            }

            if (item.Effect == null)
            {
                return CreateFeedback($"{item.Name} cannot be used.");
            }

            var context = new EffectContext
            {
                Character = _character,
                DiceService = _diceService,
            };

            var result = item.Effect.Apply(context);

            if (result != null)
            {
                _character.Inventory.Remove(item);
            }
            return result ?? CreateFeedback($"{item.Name} had no effect.");
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
            {
                return CreateFeedback($"{featureName} was not found.");
            }

            switch (feature.ActionType)
            {
                case FeatureActionType.Use:
                    if (!feature.IsAvailable)
                    {
                        return CreateFeedback($"{feature.Name} has no uses remaining.");
                    }

                    if (feature.MaxUses > 0)
                        feature.UsesRemaining--;

                    return ExecuteFeatureAction(feature);

                case FeatureActionType.ResourceUse:
                    if (string.IsNullOrWhiteSpace(feature.ResourceName))
                    {
                        return CreateFeedback($"{feature.Name} is missing a resource requirement.");
                    }

                    bool success = _character.SpendResource(
                        feature.ResourceName,
                        feature.ResourceCost);

                    if (!success)
                    {
                        return CreateFeedback($"Not enough {feature.ResourceName} to use {feature.Name}.");
                    }

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
            
            if (characterClass.GrantsAbilityScoreImprovement(_character.Level))
            {
                _character.PendingAbilityScoreImprovementPoints += 2;
            }

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
            //TODO: Separate hit dice and short rest logic
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
        public void AddInventoryItem(AddInventoryItemViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return;
            }

            Item item = model.ItemKind switch
            {
                "Weapon" => new Weapon
                {
                    Name = model.Name,
                    Type = ItemType.Weapon,
                    DamageDice = string.IsNullOrWhiteSpace(model.DamageDice) ? "1d4" : model.DamageDice,
                    DamageType = string.IsNullOrWhiteSpace(model.DamageType) ? "bludgeoning" : model.DamageType,
                    ScalingType = model.ScalingType,
                    ProficiencyType = model.WeaponProficiencyType,
                    ProficiencyName = model.Name,
                    AttackBonus = model.AttackBonus
                },

                "Armor" => new Armor
                {
                    Name = model.Name,
                    Type = ItemType.Armor,
                    ArmorType = model.ArmorType,
                    BaseArmorClass = model.BaseArmorClass,
                },

                _ => new Item
                {
                    Name = model.Name,
                    Type = model.Type
                }
            };

            _character.Inventory.Add(item);
        }
        public void RemoveInventoryItem(Guid itemId)
        {
            var item = _character.Inventory.FirstOrDefault(i => i.Id == itemId);

            if (item == null)
            {
                return;
            }

            _character.Inventory.Remove(item);
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
        public void IncreaseAbilityScore(AbilityType abilityType)
        {
            _character.IncreaseAbilityScore(abilityType);
        }
        public RollResult? SetSkillProficiency(SkillType skillType, bool isProficient)
        {
            var characterClass = CharacterClassFactory.Create(_character.ClassType);
            var skill = _character.GetSkill(skillType);

            if (isProficient)
            {
                if (!characterClass.CanChooseSkillProficiency(skillType))
                {
                    return CreateFeedback($"{skillType} is not available as a skill proficiency for this class.");
                }

                if (skill.IsProficient)
                {
                    return CreateFeedback($"{skillType} is already proficient.");
                }

                var selectedClassSkillCount = _character.Skills
                    .Count(s =>
                        s.IsProficient &&
                        characterClass.CanChooseSkillProficiency(s.Type));

                if (selectedClassSkillCount >= characterClass.SkillProficiencyChoiceCount)
                {
                    return CreateFeedback("The maximum number of skill proficiencies is already applied.");
                }
            }

            _character.SetSkillProficiency(skillType, isProficient);
            return null;
        }
        public RollResult? SetSkillExpertise(SkillType skillType, bool isExpertise)
        {
            var characterClass = CharacterClassFactory.Create(_character.ClassType);
            var skill = _character.GetSkill(skillType);

            if (isExpertise)
            {
                if (!skill.IsProficient)
                {
                    return CreateFeedback("Expertise can only be applied to proficient skills.");
                }

                var expertiseLimit = characterClass.GetExpertiseChoiceCount(_character.Level);

                if (expertiseLimit <= 0)
                {
                    return CreateFeedback($"{_character.ClassType} cannot select skill expertise.");
                }

                var selectedExpertiseCount = _character.Skills.Count(s => s.IsExpertise);

                if (!skill.IsExpertise && selectedExpertiseCount >= expertiseLimit)
                {
                    return CreateFeedback("The maximum number of expertise choices is already applied.");
                }
            }

            _character.SetSkillExpertise(skillType, isExpertise);
            return null;
        }
        public bool SetSavingThrowProficiency(AbilityType abilityType, bool isProficient)
        {
            var characterClass = CharacterClassFactory.Create(_character.ClassType);

            if (isProficient &&
                !characterClass.HasSavingThrowProficiency(abilityType))
            {
                return false;
            }

            return _character.SetSavingThrowProficiency(abilityType, isProficient);
        }
        public void UpdateCharacterNotes(UpdateCharacterNotesViewModel model)
        {
            _character.Race = model.Race ?? "";
            _character.Background = model.Background ?? "";
            _character.Alignment = model.Alignment ?? "";
            _character.PersonalityTraits = model.PersonalityTraits ?? "";
            _character.Ideals = model.Ideals ?? "";
            _character.Bonds = model.Bonds ?? "";
            _character.Flaws = model.Flaws ?? "";
            _character.Notes = model.Notes ?? "";
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
                Id = DemoCharacterIds.Fighter,
                Name = "Tyrion",
                Level = 5,
                HitDiceRemaining = 4,
                MovementSpeed = 30,
                ClassType = CharacterClassType.Fighter,
                Race = "Dwarf",
                Background = "Soldier",
                Alignment = "Neutral Good",
                PersonalityTraits = "Direct, loyal, and practical.",
                Ideals = "Discipline and duty matter more than glory.",
                Bonds = "Protects the people who fight beside him.",
                Flaws = "Sometimes trusts strength more than planning.",
                DamageResistances = new List<string>
                {
                    "Poison"
                },
                Senses = new List<CharacterSense>
                {
                    new CharacterSense
                    {
                        Name = "Darkvision",
                        RangeFeet = 60,
                        Description = "Can see in dim light within 60 ft as if it were bright light."
                    }
                },
                Abilities = new List<Ability>
                {
                    strength, dexterity, constitution,
                    intelligence, wisdom, charisma
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
            character.Skills = SkillFactory.CreateDefaultSkills(character);
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
                Id = DemoCharacterIds.Rogue,
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
            
            character.Skills = SkillFactory.CreateDefaultSkills(character);

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
                Id = DemoCharacterIds.Barbarian,
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
            character.Skills = SkillFactory.CreateDefaultSkills(character);

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
                Id = DemoCharacterIds.Monk,
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
            character.Skills = SkillFactory.CreateDefaultSkills(character);

            character.ApplySkillProficiencies(new[]
            {
                SkillType.Acrobatics,
                SkillType.Stealth,
                SkillType.Perception
            });

            ApplyClassSetup(character);

            return character;
        }
        
        private RollResult CreateFeedback(string message)
        {
            return RollResult.Info("System", message);
        }
        private Item ToItem(ItemEntity entity)
        {
            if (entity.Kind == "Weapon")
            {
                return new Weapon
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Description = entity.Description,
                    Weight = entity.Weight,
                    Type = ItemType.Weapon,
                    DamageDice = entity.DamageDice ?? "1d4",
                    DamageType = entity.DamageType ?? "bludgeoning",
                    AttackBonus = entity.AttackBonus,
                    ScalingType = entity.ScalingType,
                    ProficiencyType = entity.ProficiencyType,
                    ProficiencyName = entity.ProficiencyName
                };
            }

            if (entity.Kind == "Armor")
            {
                return new Armor
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Description = entity.Description,
                    Weight = entity.Weight,
                    Type = ItemType.Armor,
                    BaseArmorClass = entity.BaseArmorClass,
                    ArmorType = entity.ArmorType
                };
            }

            return new Item
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Weight = entity.Weight,
                Type = entity.Type,
                Effect = entity.EffectType == "Heal"
                    ? new HealEffect(entity.EffectDice ?? "2d4+2")
                    : null
            };
        }
    }
}