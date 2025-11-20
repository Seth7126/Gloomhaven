using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using SharedLibrary.Logger;
using SharedLibrary.YML;

public class CSRLYML
{
	public enum EYMLMode
	{
		None,
		Global,
		StandardRuleset,
		ModdedRuleset
	}

	private EYMLMode _ymlMode;

	public ScenarioManager.EDLLMode MapMode { get; set; }

	public CSRLYMLModeData GlobalData { get; private set; }

	public CSRLYMLModeData RulesetData { get; private set; }

	public CSRLYMLModeData ModdedData { get; private set; }

	public EYMLMode YMLMode { get; set; }

	public List<RemoveYMLData> RemoveList => YMLMode switch
	{
		EYMLMode.Global => GlobalData.RemoveList.LoadedYML, 
		EYMLMode.StandardRuleset => GlobalData.RemoveList.LoadedYML.Concat(RulesetData.RemoveList.LoadedYML).ToList(), 
		EYMLMode.ModdedRuleset => ModdedData.RemoveList.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public List<HeroSummonYMLData> HeroSummons => YMLMode switch
	{
		EYMLMode.Global => GlobalData.HeroSummons.LoadedYML, 
		EYMLMode.StandardRuleset => GlobalData.HeroSummons.LoadedYML.Concat(RulesetData.HeroSummons.LoadedYML).ToList(), 
		EYMLMode.ModdedRuleset => ModdedData.HeroSummons.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public List<CharacterYMLData> Characters => YMLMode switch
	{
		EYMLMode.Global => GlobalData.Characters.LoadedYML, 
		EYMLMode.StandardRuleset => GlobalData.Characters.LoadedYML.Concat(RulesetData.Characters.LoadedYML).ToList(), 
		EYMLMode.ModdedRuleset => ModdedData.Characters.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public List<AbilityCardYMLData> AbilityCards => YMLMode switch
	{
		EYMLMode.Global => GlobalData.AbilityCards.LoadedYML, 
		EYMLMode.StandardRuleset => GlobalData.AbilityCards.LoadedYML.Concat(RulesetData.AbilityCards.LoadedYML).ToList(), 
		EYMLMode.ModdedRuleset => ModdedData.AbilityCards.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public List<ItemCardYMLData> ItemCards => YMLMode switch
	{
		EYMLMode.Global => GlobalData.ItemCards.LoadedYML, 
		EYMLMode.StandardRuleset => GlobalData.ItemCards.LoadedYML.Concat(RulesetData.ItemCards.LoadedYML).ToList(), 
		EYMLMode.ModdedRuleset => ModdedData.ItemCards.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public List<PerksYMLData> Perks => YMLMode switch
	{
		EYMLMode.Global => GlobalData.Perks.LoadedYML, 
		EYMLMode.StandardRuleset => GlobalData.Perks.LoadedYML.Concat(RulesetData.Perks.LoadedYML).ToList(), 
		EYMLMode.ModdedRuleset => ModdedData.Perks.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public EnhancementYML Enhancements
	{
		get
		{
			switch (YMLMode)
			{
			case EYMLMode.Global:
				return GlobalData.Enhancements;
			case EYMLMode.StandardRuleset:
				if (!RulesetData.Enhancements.IsLoaded)
				{
					return GlobalData.Enhancements;
				}
				return RulesetData.Enhancements;
			case EYMLMode.ModdedRuleset:
				return ModdedData.Enhancements;
			default:
				throw new Exception("Attempting to read YML data while no mode is set!");
			}
		}
	}

	public List<IconGlossaryYML.IconGlossaryEntry> IconGlossary => YMLMode switch
	{
		EYMLMode.Global => GlobalData.IconGlossary.Entries, 
		EYMLMode.StandardRuleset => GlobalData.IconGlossary.Entries.Concat(RulesetData.IconGlossary.Entries).ToList(), 
		EYMLMode.ModdedRuleset => ModdedData.IconGlossary.Entries, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public List<AttackModifierYMLData> AttackModifiers => YMLMode switch
	{
		EYMLMode.Global => GlobalData.AttackModifiers.LoadedYML, 
		EYMLMode.StandardRuleset => GlobalData.AttackModifiers.LoadedYML.Concat(RulesetData.AttackModifiers.LoadedYML).ToList(), 
		EYMLMode.ModdedRuleset => ModdedData.AttackModifiers.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public List<AttackModifierDeckYMLData> AttackModifierDecks => YMLMode switch
	{
		EYMLMode.Global => GlobalData.AttackModifierDecks.LoadedYML, 
		EYMLMode.StandardRuleset => GlobalData.AttackModifierDecks.LoadedYML.Concat(RulesetData.AttackModifierDecks.LoadedYML).ToList(), 
		EYMLMode.ModdedRuleset => ModdedData.AttackModifierDecks.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public List<MonsterCardYMLData> MonsterCards => YMLMode switch
	{
		EYMLMode.Global => GlobalData.MonsterCards.LoadedYML, 
		EYMLMode.StandardRuleset => GlobalData.MonsterCards.LoadedYML.Concat(RulesetData.MonsterCards.LoadedYML).ToList(), 
		EYMLMode.ModdedRuleset => ModdedData.MonsterCards.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public List<MonsterYMLData> Monsters => YMLMode switch
	{
		EYMLMode.Global => GlobalData.Monsters.LoadedYML, 
		EYMLMode.StandardRuleset => GlobalData.Monsters.LoadedYML.Concat(RulesetData.Monsters.LoadedYML).ToList(), 
		EYMLMode.ModdedRuleset => ModdedData.Monsters.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public List<TreasureTable> TreasureTables => YMLMode switch
	{
		EYMLMode.Global => GlobalData.TreasureTables.LoadedYML, 
		EYMLMode.StandardRuleset => GlobalData.TreasureTables.LoadedYML.Concat(RulesetData.TreasureTables.LoadedYML).ToList(), 
		EYMLMode.ModdedRuleset => ModdedData.TreasureTables.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public List<ScenarioDefinition> Scenarios => YMLMode switch
	{
		EYMLMode.Global => GlobalData.Scenarios.LoadedYML, 
		EYMLMode.StandardRuleset => GlobalData.Scenarios.LoadedYML.Concat(RulesetData.Scenarios.LoadedYML).ToList(), 
		EYMLMode.ModdedRuleset => ModdedData.Scenarios.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public MonsterDataYML MonsterData
	{
		get
		{
			switch (YMLMode)
			{
			case EYMLMode.Global:
				return GlobalData.MonsterData;
			case EYMLMode.StandardRuleset:
				if (!RulesetData.MonsterData.IsLoaded)
				{
					return GlobalData.MonsterData;
				}
				return RulesetData.MonsterData;
			case EYMLMode.ModdedRuleset:
				return ModdedData.MonsterData;
			default:
				throw new Exception("Attempting to read YML data while no mode is set!");
			}
		}
	}

	public ScenarioRoomsYML ScenarioRooms
	{
		get
		{
			switch (YMLMode)
			{
			case EYMLMode.Global:
				return GlobalData.ScenarioRooms;
			case EYMLMode.StandardRuleset:
				if (!RulesetData.ScenarioRooms.IsLoaded)
				{
					return GlobalData.ScenarioRooms;
				}
				return RulesetData.ScenarioRooms;
			case EYMLMode.ModdedRuleset:
				return ModdedData.ScenarioRooms;
			default:
				throw new Exception("Attempting to read YML data while no mode is set!");
			}
		}
	}

	public List<ScenarioAbilitiesYMLData> ScenarioAbilities => YMLMode switch
	{
		EYMLMode.Global => GlobalData.ScenarioAbilities.LoadedYML, 
		EYMLMode.StandardRuleset => GlobalData.ScenarioAbilities.LoadedYML.Concat(RulesetData.ScenarioAbilities.LoadedYML).ToList(), 
		EYMLMode.ModdedRuleset => ModdedData.ScenarioAbilities.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public List<CharacterResourceData> CharacterResources => YMLMode switch
	{
		EYMLMode.Global => GlobalData.CharacterResources.LoadedYML, 
		EYMLMode.StandardRuleset => GlobalData.CharacterResources.LoadedYML.Concat(RulesetData.CharacterResources.LoadedYML).ToList(), 
		EYMLMode.ModdedRuleset => ModdedData.CharacterResources.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public List<ItemConfigYMLData> ItemConfigs => YMLMode switch
	{
		EYMLMode.Global => GlobalData.ItemConfigs.LoadedYML, 
		EYMLMode.StandardRuleset => GlobalData.ItemConfigs.LoadedYML.Concat(RulesetData.ItemConfigs.LoadedYML).ToList(), 
		EYMLMode.ModdedRuleset => ModdedData.ItemConfigs.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public List<AbilityCardConfigYMLData> AbilityCardConfigs => YMLMode switch
	{
		EYMLMode.Global => GlobalData.AbilityCardConfigs.LoadedYML, 
		EYMLMode.StandardRuleset => GlobalData.AbilityCardConfigs.LoadedYML.Concat(RulesetData.AbilityCardConfigs.LoadedYML).ToList(), 
		EYMLMode.ModdedRuleset => ModdedData.AbilityCardConfigs.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public List<CharacterConfigYMLData> CharacterConfigs => YMLMode switch
	{
		EYMLMode.Global => GlobalData.CharacterConfigs.LoadedYML, 
		EYMLMode.StandardRuleset => GlobalData.CharacterConfigs.LoadedYML.Concat(RulesetData.CharacterConfigs.LoadedYML).ToList(), 
		EYMLMode.ModdedRuleset => ModdedData.CharacterConfigs.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public List<MonsterConfigYMLData> MonsterConfigs => YMLMode switch
	{
		EYMLMode.Global => GlobalData.MonsterConfigs.LoadedYML, 
		EYMLMode.StandardRuleset => GlobalData.MonsterConfigs.LoadedYML.Concat(RulesetData.MonsterConfigs.LoadedYML).ToList(), 
		EYMLMode.ModdedRuleset => ModdedData.MonsterConfigs.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public List<MapConfigYMLData> MapConfigs => YMLMode switch
	{
		EYMLMode.Global => GlobalData.MapConfigs.LoadedYML, 
		EYMLMode.StandardRuleset => GlobalData.MapConfigs.LoadedYML.Concat(RulesetData.MapConfigs.LoadedYML).ToList(), 
		EYMLMode.ModdedRuleset => ModdedData.MapConfigs.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public List<Tuple<string, byte[]>> CustomLevels => YMLMode switch
	{
		EYMLMode.Global => GlobalData.CustomLevels, 
		EYMLMode.StandardRuleset => GlobalData.CustomLevels.Concat(RulesetData.CustomLevels).ToList(), 
		EYMLMode.ModdedRuleset => ModdedData.CustomLevels, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public MonsterYMLData GetMonsterData(string id)
	{
		if (id == "Empty")
		{
			return null;
		}
		return YMLMode switch
		{
			EYMLMode.Global => GlobalData.Monsters.GetMonsterData(id, MapMode), 
			EYMLMode.StandardRuleset => RulesetData.Monsters.GetMonsterData(id, MapMode), 
			EYMLMode.ModdedRuleset => ModdedData.Monsters.GetMonsterData(id, MapMode), 
			_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
		};
	}

	public ScenarioDefinition GetScenarioDefinition(string id)
	{
		if (id == "Empty")
		{
			return null;
		}
		return YMLMode switch
		{
			EYMLMode.Global => GlobalData.Scenarios.GetScenario(id, MapMode), 
			EYMLMode.StandardRuleset => RulesetData.Scenarios.GetScenario(id, MapMode), 
			EYMLMode.ModdedRuleset => ModdedData.Scenarios.GetScenario(id, MapMode), 
			_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
		};
	}

	public CSRLYML()
	{
		GlobalData = new CSRLYMLModeData();
		RulesetData = new CSRLYMLModeData();
	}

	public void UnloadRuleset()
	{
		RulesetData = new CSRLYMLModeData();
		ModdedData = new CSRLYMLModeData();
	}

	public void ResetModdedData()
	{
		ModdedData = new CSRLYMLModeData();
	}

	public bool Validate()
	{
		bool result = true;
		foreach (RemoveYMLData remove in RemoveList)
		{
			if (!remove.Validate())
			{
				SharedClient.ValidationRecord.RecordParseFailure("RemoveYML", "Failed to validate RemoveYML YML " + remove.FileName);
				result = false;
			}
		}
		Dictionary<int, string> dictionary = new Dictionary<int, string>();
		foreach (HeroSummonYMLData heroSummon in HeroSummons)
		{
			if (!heroSummon.Validate())
			{
				SharedClient.ValidationRecord.RecordParseFailure("HeroSummonYML", "Failed to validate Hero Summon YML " + heroSummon.FileName);
				result = false;
			}
			int hashCode = heroSummon.ID.GetHashCode();
			if (dictionary.ContainsKey(hashCode))
			{
				SharedClient.ValidationRecord.RecordParseFailure("HeroSummonYML", "Failed to validate Hero Summon YML " + heroSummon.FileName + "\nID HashCode collision for IDs: " + heroSummon.ID + " and " + dictionary[hashCode]);
				result = false;
			}
			else
			{
				dictionary.Add(hashCode, heroSummon.ID);
			}
		}
		if (Characters.Count < 1)
		{
			SharedClient.ValidationRecord.RecordParseFailure("CharacterYML", "The number of loaded characters is less than the required minimum (" + 1 + ")");
			result = false;
		}
		foreach (CharacterYMLData character in Characters)
		{
			if (!character.Validate())
			{
				SharedClient.ValidationRecord.RecordParseFailure("CharacterYML", "Failed to validate Character YML " + character.FileName);
				result = false;
			}
		}
		if (AbilityCards.Count < 1)
		{
			SharedClient.ValidationRecord.RecordParseFailure("AbilityCardYML", "The number of loaded Ability Cards is less than the required minimum (" + 1 + ")");
			result = false;
		}
		else
		{
			foreach (AbilityCardYMLData abilityCard in AbilityCards)
			{
				if (!abilityCard.Validate())
				{
					SharedClient.ValidationRecord.RecordParseFailure("AbilityCardYML", "Failed to validate Ability Card YML " + abilityCard.FileName);
					result = false;
				}
			}
		}
		if (ItemCards.Count < 1)
		{
			SharedClient.ValidationRecord.RecordParseFailure("ItemCardYML", "The number of loaded Item Cards is less than the required minimum (" + 1 + ")");
			result = false;
		}
		else
		{
			foreach (ItemCardYMLData itemCard in ItemCards)
			{
				if (!itemCard.Validate())
				{
					SharedClient.ValidationRecord.RecordParseFailure("ItemCardYML", "Failed to validate Item Card YML " + itemCard.FileName);
					result = false;
				}
			}
		}
		if (Perks.Count < 1)
		{
			SharedClient.ValidationRecord.RecordParseFailure("PerksYML", "The number of loaded Perks is less than the required minimum (" + 1 + ")");
			result = false;
		}
		else
		{
			foreach (PerksYMLData perk in Perks)
			{
				if (!perk.Validate())
				{
					SharedClient.ValidationRecord.RecordParseFailure("PerksYML", "Failed to validate Perk YML " + perk.FileName);
					result = false;
				}
			}
		}
		if (!Enhancements.Validate())
		{
			SharedClient.ValidationRecord.RecordParseFailure("EnhancementsYML", "Failed to validate Enhancements YML " + Enhancements.FileName);
			result = false;
		}
		if (IconGlossary.Count < 1)
		{
			SharedClient.ValidationRecord.RecordParseFailure("IconGlossaryYML", "The number of loaded Icon Glossary Entries is less than the required minimum (" + 1 + ")");
			result = false;
		}
		else
		{
			foreach (IconGlossaryYML.IconGlossaryEntry item in IconGlossary)
			{
				if (!item.Validate())
				{
					SharedClient.ValidationRecord.RecordParseFailure("IconGlossaryYML", "Failed to validate Icon Glossary YML " + item.FileName);
					result = false;
				}
			}
		}
		if (AttackModifiers.Count < 1)
		{
			SharedClient.ValidationRecord.RecordParseFailure("AttackModifiersYML", "The number of Attack modifier cards is less than the required minimum (" + 1 + ")");
			result = false;
		}
		else
		{
			Dictionary<int, string> dictionary2 = new Dictionary<int, string>();
			foreach (AttackModifierYMLData attackModifier in AttackModifiers)
			{
				if (!attackModifier.Validate())
				{
					SharedClient.ValidationRecord.RecordParseFailure("AttackModifiersYML", "Failed to validate Attack Modifier YML " + attackModifier.FileName);
					result = false;
				}
				int deterministicHashCode = YMLShared.GetDeterministicHashCode(attackModifier.Name);
				if (dictionary2.ContainsKey(deterministicHashCode))
				{
					SharedClient.ValidationRecord.RecordParseFailure("AttackModifiersYML", "Failed to validate Attack Modifier YML " + attackModifier.FileName + "\nID HashCode collision for Names: " + attackModifier.Name + " and " + dictionary2[deterministicHashCode]);
					result = false;
				}
				else
				{
					dictionary2.Add(deterministicHashCode, attackModifier.Name);
				}
			}
		}
		if (AttackModifierDecks.Count < 2)
		{
			SharedClient.ValidationRecord.RecordParseFailure("AttackModifierDecksYML", "The number of loaded Attack Modifier Decks is less than the required minimum (" + 2 + ")");
			result = false;
		}
		else
		{
			bool flag = false;
			foreach (AttackModifierDeckYMLData attackModifierDeck in AttackModifierDecks)
			{
				if (!attackModifierDeck.Validate())
				{
					SharedClient.ValidationRecord.RecordParseFailure("AttackModifierDecksYML", "Failed to validate Attack Modifier Deck YML " + attackModifierDeck.FileName);
					result = false;
				}
				if (attackModifierDeck.IsMonsterDeck)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				SharedClient.ValidationRecord.RecordParseFailure("AttackModifierDecksYML", "No Monster Attack Modifier deck was loaded.  One Monster Attack modifier deck must be defined.");
				result = false;
			}
		}
		if (MonsterCards.Count < 1)
		{
			SharedClient.ValidationRecord.RecordParseFailure("MonsterCardYML", "The number of loaded Monster Cards is less than the required minimum (" + 1 + ")");
			result = false;
		}
		else
		{
			foreach (MonsterCardYMLData monsterCard in MonsterCards)
			{
				if (!monsterCard.Validate())
				{
					SharedClient.ValidationRecord.RecordParseFailure("MonsterCardYML", "Failed to validate Monster Card YML " + monsterCard.FileName);
					result = false;
				}
			}
		}
		if (TreasureTables.Count < 1)
		{
			SharedClient.ValidationRecord.RecordParseFailure("TreasureTablesYML", "The number of loaded Treasure Tables is less than the required minimum (" + 1 + ")");
			result = false;
		}
		else
		{
			foreach (TreasureTable treasureTable in TreasureTables)
			{
				if (!treasureTable.Validate())
				{
					SharedClient.ValidationRecord.RecordParseFailure("TreasureTablesYML", "Failed to validate Treasure Table YML " + treasureTable.FileName);
					result = false;
				}
			}
		}
		if (Scenarios.Count + CustomLevels.Count < 1)
		{
			SharedClient.ValidationRecord.RecordParseFailure("ScenarioYML", "The number of loaded scenarios is less than the required minimum (" + 1 + ")");
			result = false;
		}
		else
		{
			foreach (ScenarioDefinition scenario in Scenarios)
			{
				if (!scenario.Validate())
				{
					SharedClient.ValidationRecord.RecordParseFailure("ScenarioYML", "Failed to validate Scenario YML " + scenario.FileName);
					result = false;
				}
			}
		}
		if (!MonsterData.Validate())
		{
			SharedClient.ValidationRecord.RecordParseFailure("MonsterDataYML", "Failed to validate MonsterData YML " + MonsterData.FileName);
			result = false;
		}
		if (!ScenarioRooms.Validate())
		{
			SharedClient.ValidationRecord.RecordParseFailure("ScenarioRoomsYML", "Failed to validate ScenarioRooms YML " + ScenarioRooms.FileName);
			result = false;
		}
		foreach (ScenarioAbilitiesYMLData scenarioAbility in ScenarioAbilities)
		{
			if (!scenarioAbility.Validate())
			{
				SharedClient.ValidationRecord.RecordParseFailure("ScenarioAbilitiesYML", "Failed to validate ScenarioAbilities YML " + scenarioAbility.FileName);
				result = false;
			}
		}
		foreach (ItemConfigYMLData itemConfig in ItemConfigs)
		{
			if (!itemConfig.Validate())
			{
				SharedClient.ValidationRecord.RecordParseFailure("ItemConfigYML", "Failed to validate ItemConfig YML " + itemConfig.FileName);
				result = false;
			}
		}
		foreach (AbilityCardConfigYMLData abilityCardConfig in AbilityCardConfigs)
		{
			if (!abilityCardConfig.Validate())
			{
				SharedClient.ValidationRecord.RecordParseFailure("AbilityCardConfig", "Failed to validate AbilityCardConfig YML " + abilityCardConfig.FileName);
				result = false;
			}
		}
		foreach (CharacterConfigYMLData characterConfig in CharacterConfigs)
		{
			if (!characterConfig.Validate())
			{
				SharedClient.ValidationRecord.RecordParseFailure("CharacterConfigYML", "Failed to validate CharacterConfig YML " + characterConfig.FileName);
				result = false;
			}
		}
		foreach (MonsterConfigYMLData monsterConfig in MonsterConfigs)
		{
			if (!monsterConfig.Validate())
			{
				SharedClient.ValidationRecord.RecordParseFailure("MonsterConfigYML", "Failed to validate MonsterConfig YML " + monsterConfig.FileName);
				result = false;
			}
		}
		foreach (MapConfigYMLData mapConfig in MapConfigs)
		{
			if (!mapConfig.Validate())
			{
				SharedClient.ValidationRecord.RecordParseFailure("MapConfigYML", "Failed to validate MapConfig YML " + mapConfig.FileName);
				result = false;
			}
		}
		return result;
	}

	public CCustomLevelData GetCustomLevel(string name)
	{
		byte[] array = CustomLevels.SingleOrDefault((Tuple<string, byte[]> s) => s.Item1.Equals(name, StringComparison.OrdinalIgnoreCase))?.Item2;
		if (array != null)
		{
			try
			{
				CCustomLevelData result = null;
				using (MemoryStream serializationStream = new MemoryStream(array))
				{
					result = new BinaryFormatter().Deserialize(serializationStream) as CCustomLevelData;
				}
				return result;
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Unable to get Custom Level Data object from path.\n" + ex.Message + "\n" + ex.StackTrace);
			}
		}
		return null;
	}
}
