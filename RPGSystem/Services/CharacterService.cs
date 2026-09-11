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
            // TODO: Display weapon ability scaling in equipped weapon panel
        }
        private void ApplySavedCharacterStates()
        {
            var savedCharacters = _persistenceService
                .LoadCharacterStates();

            var savedCharactersById = savedCharacters
                .ToDictionary(c => c.Id);

            foreach (var character in _characters)
            {
                if (savedCharactersById.TryGetValue(character.Id, out var savedCharacter))
                {
                    ApplySavedState(character, savedCharacter);
                }
            }

            var existingCharacterIds = _characters
                .Select(c => c.Id)
                .ToHashSet();

            var userCreatedCharacters = savedCharacters
                .Where(c => !existingCharacterIds.Contains(c.Id));

            foreach (var savedCharacter in userCreatedCharacters)
            {
                var character = CreateStarterCharacterTemplate(savedCharacter.ClassType);

                ApplySavedState(character, savedCharacter);

                _characters.Add(character);
            }
        }
        private void ApplySavedState(Character character, CharacterEntity savedCharacter)
        {
            character.Id = savedCharacter.Id;
            character.ClassType = savedCharacter.ClassType;
            character.Name = savedCharacter.Name;
            character.PortraitPath = savedCharacter.PortraitPath;
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
            character.CopperPieces = savedCharacter.CopperPieces;
            character.SilverPieces = savedCharacter.SilverPieces;
            character.ElectrumPieces = savedCharacter.ElectrumPieces;
            character.GoldPieces = savedCharacter.GoldPieces;
            character.PlatinumPieces = savedCharacter.PlatinumPieces;

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

            character.Senses = savedCharacter.Senses
                .Select(sense => new CharacterSense
                {
                    Id = sense.Id,
                    Name = sense.Name,
                    RangeFeet = sense.RangeFeet,
                    Description = sense.Description
                })
                .ToList();

            character.DamageResistances = GetDefenseNames(
                savedCharacter,
                DefenseType.Resistance);

            character.DamageVulnerabilities = GetDefenseNames(
                savedCharacter,
                DefenseType.Vulnerability);

            character.DamageImmunities = GetDefenseNames(
                savedCharacter,
                DefenseType.DamageImmunity);

            character.ConditionImmunities = GetDefenseNames(
                savedCharacter,
                DefenseType.ConditionImmunity);
            character.Conditions = savedCharacter.Conditions
                .Select(c => c.Type)
                .ToList();

            foreach (var savedFeature in savedCharacter.FeatureStates)
            {
                var feature = character.GetFeature(savedFeature.FeatureName);

                if (feature == null)
                    continue;

                feature.UsesRemaining = savedFeature.UsesRemaining;
                feature.IsActive = savedFeature.IsActive;
            }

            foreach (var savedResource in savedCharacter.FeatureResources)
            {
                var resource = character.GetResource(savedResource.Name);

                if (resource == null)
                    continue;

                resource.Current = savedResource.Current;
                resource.Max = savedResource.Max;
                resource.ResetType = savedResource.ResetType;
            }
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
        public void CreateCharacter(CreateCharacterViewModel model, string? portraitPath)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return;
            }

            var character = CreateStarterCharacterTemplate(model.ClassType);

            if (model.AbilityScoreMode == AbilityScoreMode.StandardArray)
            {
                ApplyStandardArray(
                    character,
                    model.GetSelectedAbilityScores());

                RefreshLevelOneHitPoints(character);
            }
            character.Id = Guid.NewGuid();
            character.Name = model.Name.Trim();
            character.Race = model.Race?.Trim() ?? "";
            character.Background = model.Background?.Trim() ?? "";
            character.PortraitPath = portraitPath ?? "";
            _characters.Add(character);
            _character = character;

            SaveState();
        }
        private Character CreateStarterCharacterTemplate(
            CharacterClassType classType)
        {
            return classType switch
            {
                CharacterClassType.Fighter => CreateFighterStarterTemplate(),
                CharacterClassType.Rogue => CreateRogueStarterTemplate(),
                CharacterClassType.Barbarian => CreateBarbarianStarterTemplate(),
                CharacterClassType.Monk => CreateMonkStarterTemplate(),
                _ => CreateFighterStarterTemplate()
            };
        }
        private Character CreateLevelOneCharacter(
            CharacterClassType classType,
            int strength,
            int dexterity,
            int constitution,
            int intelligence,
            int wisdom,
            int charisma,
            IEnumerable<SkillType> proficientSkills,
            IEnumerable<Weapon>? equippedWeapons = null,
            Armor? equippedArmor = null,
            Armor? equippedShield = null,
            IEnumerable<Item>? inventory = null,
            int goldPieces = 10)
        {
            var character = new Character
            {
                Id = Guid.NewGuid(),
                Level = 1,
                ClassType = classType,
                MovementSpeed = 30,
                GoldPieces = goldPieces,
                EquippedWeapons = equippedWeapons?.ToList() ?? new List<Weapon>(),
                EquippedArmor = equippedArmor,
                EquippedShield = equippedShield,
                Inventory = inventory?.ToList() ?? new List<Item>(),
                Abilities =
                [
                    new Ability { Name = "Strength", Type = AbilityType.Strength, Score = strength },
                    new Ability { Name = "Dexterity", Type = AbilityType.Dexterity, Score = dexterity },
                    new Ability { Name = "Constitution", Type = AbilityType.Constitution, Score = constitution },
                    new Ability { Name = "Intelligence", Type = AbilityType.Intelligence, Score = intelligence },
                    new Ability { Name = "Wisdom", Type = AbilityType.Wisdom, Score = wisdom },
                    new Ability { Name = "Charisma", Type = AbilityType.Charisma, Score = charisma }
                ]
            };

            character.Skills = SkillFactory.CreateDefaultSkills(character);
            character.ApplySkillProficiencies(proficientSkills);

            ApplyClassSetup(character);
            RefreshLevelOneHitPoints(character);

            return character;
        }
        private Character CreateFighterStarterTemplate()
        {
            return CreateLevelOneCharacter(
                CharacterClassType.Fighter,
                strength: 15,
                dexterity: 13,
                constitution: 14,
                intelligence: 8,
                wisdom: 12,
                charisma: 10,
                proficientSkills: [SkillType.Athletics, SkillType.Perception],
                equippedWeapons:
                [
                    new Weapon
            {
                Name = "Longsword",
                Type = ItemType.Weapon,
                Weight = 3,
                DamageDice = "1d8",
                DamageType = "slashing",
                ScalingType = WeaponScalingType.Strength,
                ProficiencyType = WeaponProficiencyType.Martial,
                ProficiencyName = "Longsword"
            }
                ],
                equippedArmor: new Armor
                {
                    Name = "Chain Mail",
                    Type = ItemType.Armor,
                    Weight = 55,
                    BaseArmorClass = 16,
                    ArmorType = ArmorType.Heavy
                },
                equippedShield: new Armor
                {
                    Name = "Shield",
                    Type = ItemType.Armor,
                    Weight = 6,
                    BaseArmorClass = 2,
                    ArmorType = ArmorType.Shield
                },
                inventory:
                [
                    new Weapon
            {
                Name = "Light Crossbow",
                Type = ItemType.Weapon,
                Weight = 5,
                DamageDice = "1d8",
                DamageType = "piercing",
                ScalingType = WeaponScalingType.Dexterity,
                ProficiencyType = WeaponProficiencyType.Simple,
                ProficiencyName = "Light Crossbow"
            },
            new Item
            {
                Name = "Bolts (20)",
                Type = ItemType.Ammo,
                Weight = 1.5,
                Description = "A case containing 20 crossbow bolts."
            },
            new Item
            {
                Name = "Explorer's Pack",
                Type = ItemType.Pack,
                Weight = 20,
                Description = "Basic adventuring supplies."
            }
                ]);
        }
        private Character CreateRogueStarterTemplate()
        {
            return CreateLevelOneCharacter(
                CharacterClassType.Rogue,
                strength: 8,
                dexterity: 15,
                constitution: 14,
                intelligence: 13,
                wisdom: 12,
                charisma: 10,
                proficientSkills:
                [
                    SkillType.Acrobatics,
                    SkillType.Investigation,
                    SkillType.Perception,
                    SkillType.Stealth
                ],
                equippedWeapons:
                [
                    new Weapon
            {
                Name = "Rapier",
                Type = ItemType.Weapon,
                Weight = 2,
                DamageDice = "1d8",
                DamageType = "piercing",
                ScalingType = WeaponScalingType.Finesse,
                ProficiencyType = WeaponProficiencyType.Specific,
                ProficiencyName = "Rapier"
            }
                ],
                equippedArmor: new Armor
                {
                    Name = "Leather Armor",
                    Type = ItemType.Armor,
                    Weight = 10,
                    BaseArmorClass = 11,
                    ArmorType = ArmorType.Light
                },
                inventory:
                [
                    new Weapon
            {
                Name = "Shortbow",
                Type = ItemType.Weapon,
                Weight = 2,
                DamageDice = "1d6",
                DamageType = "piercing",
                ScalingType = WeaponScalingType.Dexterity,
                ProficiencyType = WeaponProficiencyType.Simple,
                ProficiencyName = "Shortbow"
            },
            new Item
            {
                Name = "Arrows (20)",
                Type = ItemType.Ammo,
                Weight = 1,
                Description = "A quiver containing 20 arrows."
            },
            new Item
            {
                Name = "Thieves' Tools",
                Type = ItemType.Tool,
                Weight = 1,
                Description = "Tools used to disarm traps and open locks."
            },
            new Item
            {
                Name = "Burglar's Pack",
                Type = ItemType.Pack,
                Weight = 20,
                Description = "Basic adventuring supplies for a rogue."
            }
                ]);
        }
        private Character CreateBarbarianStarterTemplate()
        {
            return CreateLevelOneCharacter(
                CharacterClassType.Barbarian,
                strength: 15,
                dexterity: 14,
                constitution: 13,
                intelligence: 8,
                wisdom: 12,
                charisma: 10,
                proficientSkills:
                [
                    SkillType.Athletics,
            SkillType.Survival
                ],
                equippedWeapons:
                [
                    new Weapon
            {
                Name = "Greataxe",
                Type = ItemType.Weapon,
                Weight = 7,
                DamageDice = "1d12",
                DamageType = "slashing",
                ScalingType = WeaponScalingType.Strength,
                ProficiencyType = WeaponProficiencyType.Martial,
                ProficiencyName = "Greataxe"
            }
                ],
                inventory:
                [
                    new Weapon
            {
                Name = "Handaxes (2)",
                Type = ItemType.Weapon,
                Weight = 4,
                DamageDice = "1d6",
                DamageType = "slashing",
                ScalingType = WeaponScalingType.Strength,
                ProficiencyType = WeaponProficiencyType.Simple,
                ProficiencyName = "Handaxe"
            },
            new Weapon
            {
                Name = "Javelins (4)",
                Type = ItemType.Weapon,
                Weight = 8,
                DamageDice = "1d6",
                DamageType = "piercing",
                ScalingType = WeaponScalingType.Strength,
                ProficiencyType = WeaponProficiencyType.Simple,
                ProficiencyName = "Javelin"
            },
            new Item
            {
                Name = "Explorer's Pack",
                Type = ItemType.Pack,
                Weight = 20,
                Description = "Basic adventuring supplies for travel and exploration."
            }
                ],
                goldPieces: 0);
        }
        private Character CreateMonkStarterTemplate()
        {
            return CreateLevelOneCharacter(
                CharacterClassType.Monk,
                strength: 10,
                dexterity: 15,
                constitution: 13,
                intelligence: 8,
                wisdom: 14,
                charisma: 12,
                proficientSkills:
                [
                    SkillType.Acrobatics,
            SkillType.Insight
                ],
                equippedWeapons:
                [
                    new Weapon
            {
                Name = "Shortsword",
                Type = ItemType.Weapon,
                Weight = 2,
                DamageDice = "1d6",
                DamageType = "piercing",
                ScalingType = WeaponScalingType.Dexterity,
                ProficiencyType = WeaponProficiencyType.Specific,
                ProficiencyName = "Shortsword"
            }
                ],
                inventory:
                [
                    new Weapon
            {
                Name = "Darts (10)",
                Type = ItemType.Weapon,
                Weight = 2.5,
                DamageDice = "1d4",
                DamageType = "piercing",
                ScalingType = WeaponScalingType.Finesse,
                ProficiencyType = WeaponProficiencyType.Simple,
                ProficiencyName = "Dart"
            },
            new Item
            {
                Name = "Explorer's Pack",
                Type = ItemType.Pack,
                Weight = 20,
                Description = "Basic adventuring supplies for travel and exploration."
            }
                ],
                goldPieces: 0);
        }
        private static void RefreshLevelOneHitPoints(Character character)
        {
            var characterClass = CharacterClassFactory.Create(character.ClassType);

            character.HitDiceRemaining = 1;
            character.MaxHP = Math.Max(
                1,
                characterClass.HitDie +
                character.GetAbility(AbilityType.Constitution).Modifier);

            character.CurrentHP = character.MaxHP;
        }
        private static void ApplyStandardArray(
            Character character,
            IReadOnlyList<int?> selectedScores)
        {
            var abilityTypes = new[]
            {
                AbilityType.Strength,
                AbilityType.Dexterity,
                AbilityType.Constitution,
                AbilityType.Intelligence,
                AbilityType.Wisdom,
                AbilityType.Charisma
            };

            for (int index = 0; index < abilityTypes.Length; index++)
            {
                character.GetAbility(abilityTypes[index]).Score =
                    selectedScores[index]!.Value;
            }
        }
        public void SaveCharacters()
        {
            SaveState();
        }
        private void SaveState()
        {
            _persistenceService.SaveCharacters(_characters);
        }
        public RollResult? DeleteCharacter(Guid characterId)
        {
            if (IsDemoCharacter(characterId))
            {
                return CreateFeedback("Demo characters cannot be deleted. Use reset instead.");
            }

            var character = _characters.FirstOrDefault(c => c.Id == characterId);

            if (character == null)
            {
                return CreateFeedback("Character not found.");
            }

            _characters.Remove(character);
            _persistenceService.DeleteCharacter(characterId);

            if (_character.Id == characterId)
            {
                _character = _characters.First();
            }

            return CreateFeedback($"{character.Name} was deleted.");
        }
        public RollResult? ResetDemoCharacter(Guid characterId)
        {
            if (!IsDemoCharacter(characterId))
            {
                return CreateFeedback("Only demo characters can be reset.");
            }

            var index = _characters.FindIndex(c => c.Id == characterId);

            if (index == -1)
            {
                return CreateFeedback("Character not found.");
            }

            var classType = _characters[index].ClassType;
            var resetCharacter = CreateDemoCharacterTemplate(classType);

            _characters[index] = resetCharacter;

            if (_character.Id == characterId)
            {
                _character = resetCharacter;
            }

            SaveState();

            return CreateFeedback($"{resetCharacter.Name} was reset to its original demo state.");
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
        public RollResult? AddCondition(ConditionType condition)
        {
            if (_character.IsImmuneToCondition(condition))
            {
                return CreateFeedback(
                    $"{_character.Name} is immune to the {condition} condition.");
            }

            if (_character.HasCondition(condition))
            {
                return CreateFeedback(
                    $"{_character.Name} already has the {condition} condition.");
            }

            _character.AddCondition(condition);
            SaveState();

            return null;
        }
        public void RemoveCondition(ConditionType condition)
        {
            _character.RemoveCondition(condition);
            SaveState();
        }
        public void ClearConditions()
        {
            _character.ClearConditions();
            SaveState();
        }
        public RollResult? AddSense(AddCharacterSenseViewModel model)
        {
            var name = model.Name?.Trim();

            if (string.IsNullOrWhiteSpace(name))
                return CreateFeedback("Sense name is required.");

            if (model.RangeFeet is < 0 or > 1000)
                return CreateFeedback("Sense range must be between 0 and 1000 feet.");

            if (_character.Senses.Any(sense =>
                sense.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                return CreateFeedback($"{name} is already added.");
            }

            _character.Senses.Add(new CharacterSense
            {
                Name = name,
                RangeFeet = model.RangeFeet,
                Description = model.Description?.Trim()
            });

            SaveState();

            return null;
        }
        public void RemoveSense(Guid senseId)
        {
            var sense = _character.Senses
                .FirstOrDefault(currentSense => currentSense.Id == senseId);

            if (sense == null)
                return;

            _character.Senses.Remove(sense);

            SaveState();
        }
        public RollResult? AddDefense(AddDefenseViewModel model)
        {
            var name = model.Name?.Trim();

            if (string.IsNullOrWhiteSpace(name))
                return CreateFeedback("Defense name is required.");

            if (!DefenseOptionCatalog.IsValidOption(model.Type, name))
            {
                return CreateFeedback("Select a valid defense option.");
            }

            if (!Enum.IsDefined(model.Type))
                return CreateFeedback("Invalid defense type.");

            var defenses = GetDefenseList(model.Type);

            if (defenses.Any(defense =>
                defense.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                return CreateFeedback($"{name} is already listed.");
            }

            defenses.Add(name);

            if (model.Type == DefenseType.ConditionImmunity &&
                Enum.TryParse<ConditionType>(name, true, out var condition))
                {
                    _character.RemoveCondition(condition);
                }

            SaveState();

            return null;
        }
        public void RemoveDefense(DefenseType type, string name)
        {
            var defenses = GetDefenseList(type);

            var defense = defenses.FirstOrDefault(currentDefense =>
                currentDefense.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (defense == null)
                return;

            defenses.Remove(defense);

            SaveState();
        }
        private List<string> GetDefenseList(DefenseType type)
        {
            return type switch
            {
                DefenseType.Resistance => _character.DamageResistances,
                DefenseType.Vulnerability => _character.DamageVulnerabilities,
                DefenseType.DamageImmunity => _character.DamageImmunities,
                DefenseType.ConditionImmunity => _character.ConditionImmunities,
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }
        public RollResult RollAbility(AbilityType type, AdvantageState adv)
        {
            var ability = _character.GetAbility(type);
            var advantage = ResolveAdvantage(RollType.Check, adv, type);
            var d20Outcome = _diceService.RollD20Detailed(
                advantage.FinalState);
            int roll = d20Outcome.SelectedRoll;
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
                DiscardedD20Roll = d20Outcome.DiscardedRoll,
                Modifier = ability.Modifier,
                Formula = $"1d20 {ability.Modifier:+ #;- #;+ 0} {ability.Name}",
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
            var d20Outcome = _diceService.RollD20Detailed(
                advantage.FinalState);
            int roll = d20Outcome.SelectedRoll;
            var proficiencyBonus =  _character.GetSavingThrowBonus(ability) - ability.Modifier;
            var formula = $"1d20 {ability.Modifier:+ #;- #;+ 0} {ability.Name}";

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
                DiscardedD20Roll = d20Outcome.DiscardedRoll,
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
            var skill = _character.GetSkill(skillType);
            var advantage = ResolveAdvantage(RollType.Check, adv, skill.RelatedAbility.Type);
            var d20Outcome = _diceService.RollD20Detailed(
                advantage.FinalState);
            int roll = d20Outcome.SelectedRoll;
            var proficiencyBonus = _character.GetProficiencyBonus();
            var skillBonus = skill.GetBonus(proficiencyBonus);
            var formula = $"1d20 {skill.RelatedAbility.Modifier:+ #;- #;+ 0} {skill.RelatedAbility.Name}";
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
                DiscardedD20Roll = d20Outcome.DiscardedRoll,
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
            {
                return CreateFeedback("No weapon equipped for attack roll.");
            }

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

            var d20Outcome = _diceService.RollD20Detailed(
                advantage.FinalState);

            int roll = d20Outcome.SelectedRoll;

            var isProficient = _character.IsProficientWithWeapon(weapon);

            var proficiencyBonus = isProficient
                ? _character.GetProficiencyBonus()
                : 0;

            int modifier = ability.Modifier + proficiencyBonus + weapon.AttackBonus;

            var formula = $"1d20 {ability.Modifier:+ #;- #;+ 0} {ability.Name}";

            if (proficiencyBonus != 0)
            {
                formula += $" + {proficiencyBonus} Proficiency";
            }

            if (weapon.AttackBonus != 0)
            {
                formula += $" {weapon.AttackBonus:+ #;- #;+ 0} Weapon Bonus";
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
                DiscardedD20Roll = d20Outcome.DiscardedRoll,
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
            // TODO: Fix string formatting for negative modifier on weapon bonus
            // Should be fixed just needs testing after fixing add item weapon dice bonus
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
                    Text = $"{weapon.Name} damage uses {ability.Name} because of the weapon scaling type."
                },
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

            var damageDice = isCritical
                ? _diceService.DoubleDiceExpression(weapon.DamageDice)
                : weapon.DamageDice;

            var baseDamageDice = _diceService.GetBaseDiceNotation(damageDice);

            if (isCritical)
            {
                explanations.Add(new RollExplanation
                {
                    Type = RollExplanationType.Critical,
                    Source = "Critical Hit",
                    Text = "Critical damage doubles the weapon damage dice.",
                    Dice = baseDamageDice
                });
            }

            var weaponDamage = _diceService.RollDiceDetailed(damageDice);

            int roll = weaponDamage.DiceRoll;
            int modifier = ability.Modifier + weaponDamage.Modifier;
            int extraDamage = 0;
            var appliedEffects = new List<string>();

            var formula = baseDamageDice;

            if (weaponDamage.Modifier != 0)
            {
                formula += $" {ModifierFormatter.FormatWithSpace(weaponDamage.Modifier)} Weapon Bonus";

                explanations.Add(new RollExplanation
                {
                    Type = RollExplanationType.Bonus,
                    Source = weapon.Name,
                    Text = $"Weapon damage bonus applied: {weaponDamage.Modifier:+#;-#;0}.",
                    Value = weaponDamage.Modifier
                });
            }

            if (ability.Modifier != 0)
            {
                formula += $" {ModifierFormatter.FormatWithSpace(ability.Modifier)} {ability.Name}";
            }

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
                    formula += $" {mod.FlatBonus:+ #;- #;+ 0} {source}";

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

                    formula += $" + {extraDice} {source}";

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
                Formula = formula,
                Description = isCritical
                    ? $"Critical damage roll with {weapon.Name}"
                    : $"Damage roll with {weapon.Name}",
                AppliedEffects = appliedEffects,
                IsCriticalDamage = isCritical,
                Explanations = explanations,
            };
        }
        public RollResult? RollHitDie()
        {
            if (!_character.CanSpendHitDie)
                return CreateFeedback("No Hit Dice can be spent right now.");

            var characterClass = CharacterClassFactory.Create(_character.ClassType);
            var constitution = _character.GetAbility(AbilityType.Constitution);

            int dieRoll = _diceService.RollDice($"1d{characterClass.HitDie}");
            int healing = Math.Max(0, dieRoll + constitution.Modifier);

            _character.HitDiceRemaining--;
            _character.Heal(healing);

            SaveState();

            return new RollResult
            {
                Actor = "Hit Die",
                Type = RollType.Heal,
                DiceRoll = dieRoll,
                Modifier = constitution.Modifier,
                Formula = $"1d{characterClass.HitDie} + {constitution.Modifier} CON",
                Description = $"Spent one Hit Die and restored {healing} HP."
            };
        }
        public RollResult RollDeathSave(AdvantageState adv)
        {
            if (!_character.ShouldMakeDeathSaves)
            {
                return CreateFeedback("Death saving throws are only needed at 0 HP while not stable or dead.");
            }

            var advantage = ResolveAdvantage(RollType.DeathSave, adv);

            var d20Outcome = _diceService.RollD20Detailed(
                advantage.FinalState);

            int roll = d20Outcome.SelectedRoll;

            _character.ApplyDeathSavingThrow(roll);

            SaveState();

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
                DiscardedD20Roll = d20Outcome.DiscardedRoll,
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

            SaveState();

            return CreateFeatureResult(
                "Stabilize",
                $"{_character.Name} is stable and no longer making death saving throws.");
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
            SaveState();
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
                Item = item
            };

            var result = item.Effect.Apply(context);

            if (result != null)
            {
                _character.Inventory.Remove(item);
                SaveState();
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

                    var useResult = ExecuteFeatureAction(feature);
                    SaveState();
                    return useResult;
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

                    var resourceUseResult = ExecuteFeatureAction(feature);
                    SaveState();
                    return resourceUseResult;
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

            SaveState();

            return new RollResult
            {
                Actor = "Level Up",
                Type = RollType.MaxHP,
                DiceRoll = roll,
                Modifier = ability.Modifier,
                Formula =
                    $"1d{characterClass.HitDie}" +
                    $"{ModifierFormatter.FormatWithSpace(ability.Modifier)} Constitution",
                Description = $"Maximum hit points increased by {hpGain}."
            };
        }
        public int GetHitDie()
        {
            var characterClass = CharacterClassFactory.Create(_character.ClassType);

            return characterClass.HitDie;
        }
        public RollResult ShortRest()
        {
            if (_character.IsDead)
                return CreateFeedback("A dead character cannot take a short rest.");

            var restoredEffects = new List<string>();

            foreach (var feature in _character.ClassFeatures)
            {
                bool canRestore =
                    feature.ResetType == FeatureResetType.ShortRest &&
                    feature.UsesRemaining < feature.MaxUses;

                if (canRestore)
                {
                    restoredEffects.Add(feature.Name);
                }
            }

            foreach (var resource in _character.FeatureResources)
            {
                bool canRestore =
                    resource.ResetType == FeatureResetType.ShortRest &&
                    resource.Current < resource.Max;

                if (canRestore)
                {
                    restoredEffects.Add(resource.Name);
                }
            }

            _character.ShortRest();
            SaveState();

            return new RollResult
            {
                Actor = "Short Rest",
                Type = RollType.Feature,
                Description = restoredEffects.Any()
                    ? "Short rest completed. Restored the listed features and resources."
                    : "Short rest completed. No features or resources needed to be restored.",
                AppliedEffects = restoredEffects
            };
        }
        public RollResult LongRest()
        {
            if (_character.IsDead)
                return CreateFeedback("A dead character cannot take a long rest.");

            var appliedEffects = new List<string>();
            var explanations = new List<RollExplanation>();

            int hpBefore = _character.CurrentHP;
            int hitDiceBefore = _character.HitDiceRemaining;

            bool hadDeathSaveState =
                _character.DeathSaveSuccesses > 0 ||
                _character.DeathSaveFailures > 0 ||
                _character.IsStable;

            foreach (var feature in _character.ClassFeatures)
            {
                bool resetsOnLongRest =
                    feature.ResetType == FeatureResetType.ShortRest ||
                    feature.ResetType == FeatureResetType.LongRest;

                if (resetsOnLongRest && feature.UsesRemaining < feature.MaxUses)
                {
                    appliedEffects.Add(feature.Name);

                    explanations.Add(new RollExplanation
                    {
                        Type = RollExplanationType.Feature,
                        Source = feature.Name,
                        Text = $"Uses restored from {feature.UsesRemaining}/{feature.MaxUses} to {feature.MaxUses}/{feature.MaxUses}."
                    });
                }
            }

            foreach (var resource in _character.FeatureResources)
            {
                bool resetsOnLongRest =
                    resource.ResetType == FeatureResetType.ShortRest ||
                    resource.ResetType == FeatureResetType.LongRest;

                if (resetsOnLongRest && resource.Current < resource.Max)
                {
                    appliedEffects.Add(resource.Name);

                    explanations.Add(new RollExplanation
                    {
                        Type = RollExplanationType.Feature,
                        Source = resource.Name,
                        Text = $"Resource restored from {resource.Current}/{resource.Max} to {resource.Max}/{resource.Max}."
                    });
                }
            }

            _character.LongRest();
            SaveState();

            if (hpBefore < _character.MaxHP)
            {
                appliedEffects.Insert(0, "Hit Points");

                explanations.Insert(0, new RollExplanation
                {
                    Type = RollExplanationType.Info,
                    Source = "Hit Points",
                    Text = $"Restored from {hpBefore}/{_character.MaxHP} to {_character.CurrentHP}/{_character.MaxHP}."
                });
            }

            if (_character.HitDiceRemaining > hitDiceBefore)
            {
                appliedEffects.Add("Hit Dice");

                explanations.Add(new RollExplanation
                {
                    Type = RollExplanationType.Info,
                    Source = "Hit Dice",
                    Text = $"Recovered from {hitDiceBefore}/{_character.MaxHitDice} to {_character.HitDiceRemaining}/{_character.MaxHitDice}."
                });
            }

            if (hadDeathSaveState)
            {
                appliedEffects.Add("Death Saves");

                explanations.Add(new RollExplanation
                {
                    Type = RollExplanationType.Info,
                    Source = "Death Saves",
                    Text = "Death save state was reset."
                });
            }

            return new RollResult
            {
                Actor = "Long Rest",
                Type = RollType.Feature,
                Description = appliedEffects.Any()
                    ? "Long rest completed. Restored the listed character state."
                    : "Long rest completed. No tracked values needed restoring.",
                AppliedEffects = appliedEffects,
                Explanations = explanations
            };
        }
        public void ModifyHP(int amount, HpChangeType type)
        {
            if (type == HpChangeType.Damage)
                TakeDamage(amount);
            else
                Heal(amount);
            SaveState();
        }
        public void TakeDamage(int amount)
        {
            _character.TakeDamage(amount);
        }
        public void Heal(int amount)
        {
            _character.Heal(amount);
        }
        public RollResult? AddInventoryItem(AddInventoryItemViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return CreateFeedback("Item name is required.");
            }

            if (model.Weight < 0)
            {
                return CreateFeedback("Item weight cannot be negative.");
            }

            var healingDice = string.IsNullOrWhiteSpace(model.HealingDice)
                ? "2d4+2"
                : model.HealingDice.Trim();

            if (model.ItemKind == "HealingPotion" &&
                !_diceService.IsValidDiceNotation(healingDice))
            {
                return CreateFeedback(
                    "Healing dice must use a format like 2d4+2 or 4d4+4.");
            }

            Item item = model.ItemKind switch
            {
                "HealingPotion" => new Item
                {
                    Name = string.IsNullOrWhiteSpace(model.Name)
                        ? "Healing Potion"
                        : model.Name.Trim(),

                    Description = string.IsNullOrWhiteSpace(model.Description)
                        ? $"Restores {healingDice} hit points when used."
                        : model.Description.Trim(),

                    Weight = model.Weight,

                    Type = ItemType.Consumable,

                    Effect = new HealEffect(healingDice)
                },

                "General" => new Item
                {
                    Name = model.Name.Trim(),
                    Weight = model.Weight,
                    Description = model.Description?.Trim() ?? "",
                    Type = ItemType.General
                },
                "Weapon" => new Weapon
                {
                    
                    Name = model.Name.Trim(),
                    Weight = model.Weight,
                    Description = model.Description?.Trim() ?? "",
                    Type = ItemType.Weapon,
                    DamageDice = string.IsNullOrWhiteSpace(model.DamageDice) ? "1d4" : model.DamageDice.Trim(),
                    DamageType = string.IsNullOrWhiteSpace(model.DamageType) ? "bludgeoning" : model.DamageType.Trim(),
                    ScalingType = model.ScalingType,
                    ProficiencyType = model.WeaponProficiencyType,
                    ProficiencyName = model.Name.Trim(),
                    AttackBonus = model.AttackBonus
                },

                "Armor" => new Armor
                {
                    Name = model.Name.Trim(),
                    Weight = model.Weight,
                    Description = model.Description?.Trim() ?? "",
                    Type = ItemType.Armor,
                    ArmorType = model.ArmorType,
                    BaseArmorClass = model.BaseArmorClass,
                },

                _ => new Item
                {
                    Name = model.Name.Trim(),
                    Weight = model.Weight,
                    Description = model.Description?.Trim() ?? "",
                }
            };

            if (model.ItemKind == "Weapon")
            {
                var damageDice = string.IsNullOrWhiteSpace(model.DamageDice)
                    ? "1d4"
                    : model.DamageDice.Trim();

                if (!_diceService.IsValidDiceNotation(damageDice))
                {
                    return CreateFeedback("Weapon damage dice must use a format like 1d8 or 2d6.");
                }

                if (model.AttackBonus < -5 || model.AttackBonus > 5)
                {
                    return CreateFeedback("Weapon attack bonus must be between -5 and +5.");
                }
            }

            if (model.ItemKind == "Armor")
            {
                if (model.ArmorType == ArmorType.Shield)
                {
                    if (model.BaseArmorClass < 1 || model.BaseArmorClass > 5)
                    {
                        return CreateFeedback("Shield AC bonus must be between 1 and 5.");
                    }
                }
                else if (model.BaseArmorClass < 10 || model.BaseArmorClass > 18)
                {
                    return CreateFeedback("Armor base AC must be between 10 and 18.");
                }
            }
            _character.Inventory.Add(item);
            SaveState();

            return null;
        }
        public void RemoveInventoryItem(Guid itemId)
        {
            var item = _character.Inventory.FirstOrDefault(i => i.Id == itemId);

            if (item == null)
            {
                return;
            }

            _character.Inventory.Remove(item);
            SaveState();
        }
        public RollResult? EquipWeapon(Guid weaponId)
        {
            var weapon = _character.Inventory
                .OfType<Weapon>()
                .FirstOrDefault(w => w.Id == weaponId);

            if (weapon == null)
            {
                return CreateFeedback("Weapon was not found in inventory.");
            }

            _character.EquipWeapon(weapon);
            SaveState();

            return null;
        }
        public void UnequipWeapon(Guid weaponId)
        {
            _character.UnequipWeapon(weaponId);
            SaveState();
        }
        public RollResult? EquipArmor(Guid armorId)
        {
            var armor = _character.Inventory
                .OfType<Armor>()
                .FirstOrDefault(a => a.Id == armorId && a.ArmorType != ArmorType.Shield);

            if (armor == null)
            {
                return CreateFeedback("Armor was not found in inventory.");
            }

            _character.EquipArmor(armor);
            SaveState();

            return null;
        }
        public void UnequipArmor(Guid armorId)
        {
            _character.UnequipArmor(armorId);
            SaveState();
        }
        public RollResult? EquipShield(Guid shieldId)
        {
            var shield = _character.Inventory
                .OfType<Armor>()
                .FirstOrDefault(a => a.Id == shieldId && a.ArmorType == ArmorType.Shield);

            if (shield == null)
            {
                return CreateFeedback("Shield was not found in inventory.");
            }

            _character.EquipShield(shield);
            SaveState();

            return null;
        }
        public void UnequipShield(Guid shieldId)
        {
            _character.UnequipShield(shieldId);
            SaveState();
        }
        public void IncreaseAbilityScore(AbilityType abilityType)
        {
            _character.IncreaseAbilityScore(abilityType);
            SaveState();
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
            SaveState();
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
            SaveState();
            return null;
        }
        public RollResult? SetSavingThrowProficiency(AbilityType abilityType, bool isProficient)
        {
            var characterClass = CharacterClassFactory.Create(_character.ClassType);
            var ability = _character.GetAbility(abilityType);

            if (isProficient &&
                !characterClass.HasSavingThrowProficiency(abilityType))
            {
                return CreateFeedback($"{ability.Name} is not available as a saving throw proficiency for this class.");
            }

            if (isProficient && ability.IsSavingThrowProficient)
            {
                return CreateFeedback($"{ability.Name} saving throw is already proficient.");
            }

            _character.SetSavingThrowProficiency(abilityType, isProficient);
            SaveState();

            return null;
        }
        public RollResult? UpdateCurrency(UpdateCurrencyViewModel model)
        {
            if (model.CopperPieces < 0 ||
                model.SilverPieces < 0 ||
                model.ElectrumPieces < 0 ||
                model.GoldPieces < 0 ||
                model.PlatinumPieces < 0)
            {
                return CreateFeedback("Currency values cannot be negative.");
            }

            _character.CopperPieces = model.CopperPieces;
            _character.SilverPieces = model.SilverPieces;
            _character.ElectrumPieces = model.ElectrumPieces;
            _character.GoldPieces = model.GoldPieces;
            _character.PlatinumPieces = model.PlatinumPieces;

            SaveState();

            return null;
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
            SaveState();
        }
        public bool IsDemoCharacter(Guid characterId)
        {
            return characterId == DemoCharacterIds.Fighter ||
                   characterId == DemoCharacterIds.Rogue ||
                   characterId == DemoCharacterIds.Barbarian ||
                   characterId == DemoCharacterIds.Monk;
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
                        Description = "Restores 2d4 + 2 hit points when used.",
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
                Description = "Restores 2d4 + 2 hit points when used.",
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
                Description = "Restores 2d4 + 2 hit points when used.",
                Type = ItemType.Consumable,
                Effect = new HealEffect("2d4+2")
            },

            new Item
            {
                Name = "Greater Healing Potion",
                Description = "Restores 4d4 + 4 hit points when used.",
                Type = ItemType.Consumable,
                Effect = new HealEffect("4d4+4")
            }
        }
            };
            character.Skills = SkillFactory.CreateDefaultSkills(character);

            character.ApplySkillProficiencies(new[]
            {
                SkillType.Athletics,
                SkillType.Intimidation
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
                Description = "Restores 2d4 + 2 hit points when used.",
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
        private List<string> GetDefenseNames(
            CharacterEntity character,
            DefenseType type)
        {
            return character.Defenses
                .Where(defense => defense.Type == type)
                .Select(defense => defense.Name)
                .ToList();
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
        private Character CreateDemoCharacterTemplate(CharacterClassType classType)
        {
            return classType switch
            {
                CharacterClassType.Fighter => GetFighterTestCharacter(),
                CharacterClassType.Rogue => GetRogueTestCharacter(),
                CharacterClassType.Barbarian => GetBarbarianTestCharacter(),
                CharacterClassType.Monk => GetMonkTestCharacter(),
                _ => GetFighterTestCharacter()
            };
        }
    }
}
