using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Client;
using MapRuleLibrary.MapState;
using MapRuleLibrary.State;
using MapRuleLibrary.YML.PersonalQuests;
using MapRuleLibrary.YML.Shared;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;
using StateCodeGenerator;
using UnityEngine;

namespace MapRuleLibrary.Party;

[Serializable]
[DebuggerDisplay("ID: {CharacterID} Name: {CharacterName}")]
public class CMapCharacter : ISerializable
{
	public const int MaxPlayerLevel = 9;

	public int ExperiencePersonalGoal;

	public int EnhancementsBought;

	public int Donations;

	public int BattleGoalPerks;

	private int m_CharacterGold;

	private volatile List<CItem> EquippedItems;

	private volatile List<CItem> BoundItems;

	private const int PRICE_RESET_LEVEL = 0;

	[NonSerialized]
	private List<CItem> m_MultiplayerSoldItems = new List<CItem>();

	public string CharacterName { get; private set; }

	public string DisplayCharacterName { get; set; }

	public string CharacterID { get; private set; }

	public int EXP { get; private set; }

	public int Level { get; private set; }

	public List<int> OwnedAbilityCardIDs { get; private set; }

	public List<int> HandAbilityCardIDs { get; private set; }

	public List<CItem> AllCharacterItems => CheckEquippedItems.Concat(CheckBoundItems).ToList();

	public List<CharacterPerk> Perks { get; private set; }

	public int PerkChecks { get; private set; }

	public int PerkPoints { get; private set; }

	public int UnlockedPerkPoints { get; private set; }

	public int TimesLevelReset { get; private set; }

	public CFreeLevelResetTicket LastFreeLevelResetTicket { get; set; }

	public string SkinId { get; set; }

	public List<PositiveConditionPair> PositiveConditions { get; private set; }

	public List<NegativeConditionPair> NegativeConditions { get; private set; }

	public CPlayerStats PlayerStats { get; private set; }

	public CPlayerRecords PlayerRecords { get; private set; }

	public List<string> NewEquippedItemsWithModifiers { get; private set; }

	public CPersonalQuestState PersonalQuest { get; private set; }

	public List<CPersonalQuestState> PossiblePersonalQuests { get; private set; }

	public List<PositiveConditionPair> NextScenarioPositiveConditions { get; private set; }

	public List<NegativeConditionPair> NextScenarioNegativeConditions { get; private set; }

	public List<CCompletedSoloQuestData> CompletedSoloQuestData { get; private set; }

	public int CurrentSmallItemOverride { get; private set; }

	public int PerksStartedWith { get; private set; }

	private List<CEnhancement> m_CharacterPersistentEnhancements { get; set; }

	public int MaxCards { get; private set; }

	public int LevelPrevious { get; private set; }

	public bool IsUnderMyControl { get; set; }

	public List<PositiveConditionPair> TempPositiveConditions { get; private set; }

	public List<NegativeConditionPair> TempNegativeConditions { get; private set; }

	public List<PositiveConditionPair> TempBlessConditions { get; private set; }

	public ECharacterState CharacterState { get; private set; }

	public List<CItem> CheckEquippedItems
	{
		get
		{
			lock (EquippedItems)
			{
				return EquippedItems.ToList();
			}
		}
	}

	public List<CItem> CheckBoundItems
	{
		get
		{
			lock (BoundItems)
			{
				return BoundItems.ToList();
			}
		}
	}

	public CharacterYMLData CharacterYMLData => ScenarioRuleClient.SRLYML.Characters.SingleOrDefault((CharacterYMLData s) => s.ID == CharacterID);

	public List<CAbilityCard> HandAbilityCards => CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == CharacterID).AbilityCardsPool.Where((CAbilityCard w) => HandAbilityCardIDs.Contains(w.ID)).ToList();

	public List<CAbilityCard> OwnedAbilityCards => CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == CharacterID).AbilityCardsPool.Where((CAbilityCard w) => OwnedAbilityCardIDs.Contains(w.ID)).ToList();

	public List<CAbilityCard> UnownedAbilityCards => CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == CharacterID).AbilityCardsPool.Where((CAbilityCard w) => !OwnedAbilityCardIDs.Contains(w.ID)).ToList();

	public int CharacterGold
	{
		get
		{
			if (AdventureState.MapState == null || AdventureState.MapState.GoldMode == EGoldMode.CharacterGold)
			{
				return m_CharacterGold;
			}
			throw new Exception("Must not access Party Gold when gold mode is not set to EGoldMode.CharacterGold");
		}
		private set
		{
			if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold)
			{
				int characterGold = CharacterGold;
				m_CharacterGold = Math.Max(0, value);
				if (characterGold != m_CharacterGold)
				{
					CGoldChanged_MapClientMessage message = new CGoldChanged_MapClientMessage(m_CharacterGold, characterGold, CharacterID, CharacterName);
					if (MapRuleLibraryClient.Instance?.MessageHandler != null)
					{
						MapRuleLibraryClient.Instance.MessageHandler(message);
					}
				}
				return;
			}
			throw new Exception("Must not access Party Gold when gold mode is not set to EGoldMode.CharacterGold");
		}
	}

	public List<CEnhancement> Enhancements
	{
		get
		{
			if (AdventureState.MapState.EnhancementMode == EEnhancementMode.ClassPersistent)
			{
				return AdventureState.MapState.MapParty.ClassPersistentEnhancements[CharacterID];
			}
			return m_CharacterPersistentEnhancements;
		}
	}

	public int[] HealthTable => CharacterYMLData.HealthTable;

	public List<int> XPTable => AdventureState.MapState.HeadquartersState.Headquarters.LevelToXP;

	public int MaxHealth => HealthTable[Level];

	public int SmallItemMax => GetSmallItemMax();

	public int BaseSmallItemMax => (int)Math.Ceiling((float)Level / 2f);

	public List<CItem> MultiplayerSoldItems => m_MultiplayerSoldItems;

	public CMapCharacter()
	{
	}

	public CMapCharacter(CMapCharacter state, ReferenceDictionary references)
	{
		CharacterName = state.CharacterName;
		DisplayCharacterName = state.DisplayCharacterName;
		CharacterID = state.CharacterID;
		EXP = state.EXP;
		Level = state.Level;
		OwnedAbilityCardIDs = references.Get(state.OwnedAbilityCardIDs);
		if (OwnedAbilityCardIDs == null && state.OwnedAbilityCardIDs != null)
		{
			OwnedAbilityCardIDs = new List<int>();
			for (int i = 0; i < state.OwnedAbilityCardIDs.Count; i++)
			{
				int item = state.OwnedAbilityCardIDs[i];
				OwnedAbilityCardIDs.Add(item);
			}
			references.Add(state.OwnedAbilityCardIDs, OwnedAbilityCardIDs);
		}
		HandAbilityCardIDs = references.Get(state.HandAbilityCardIDs);
		if (HandAbilityCardIDs == null && state.HandAbilityCardIDs != null)
		{
			HandAbilityCardIDs = new List<int>();
			for (int j = 0; j < state.HandAbilityCardIDs.Count; j++)
			{
				int item2 = state.HandAbilityCardIDs[j];
				HandAbilityCardIDs.Add(item2);
			}
			references.Add(state.HandAbilityCardIDs, HandAbilityCardIDs);
		}
		Perks = references.Get(state.Perks);
		if (Perks == null && state.Perks != null)
		{
			Perks = new List<CharacterPerk>();
			for (int k = 0; k < state.Perks.Count; k++)
			{
				CharacterPerk characterPerk = state.Perks[k];
				CharacterPerk characterPerk2 = references.Get(characterPerk);
				if (characterPerk2 == null && characterPerk != null)
				{
					characterPerk2 = new CharacterPerk(characterPerk, references);
					references.Add(characterPerk, characterPerk2);
				}
				Perks.Add(characterPerk2);
			}
			references.Add(state.Perks, Perks);
		}
		PerkChecks = state.PerkChecks;
		PerkPoints = state.PerkPoints;
		TimesLevelReset = state.TimesLevelReset;
		LastFreeLevelResetTicket = references.Get(state.LastFreeLevelResetTicket);
		if (LastFreeLevelResetTicket == null && state.LastFreeLevelResetTicket != null)
		{
			LastFreeLevelResetTicket = new CFreeLevelResetTicket(state.LastFreeLevelResetTicket, references);
			references.Add(state.LastFreeLevelResetTicket, LastFreeLevelResetTicket);
		}
		SkinId = state.SkinId;
		PositiveConditions = references.Get(state.PositiveConditions);
		if (PositiveConditions == null && state.PositiveConditions != null)
		{
			PositiveConditions = new List<PositiveConditionPair>();
			for (int l = 0; l < state.PositiveConditions.Count; l++)
			{
				PositiveConditionPair positiveConditionPair = state.PositiveConditions[l];
				PositiveConditionPair positiveConditionPair2 = references.Get(positiveConditionPair);
				if (positiveConditionPair2 == null && positiveConditionPair != null)
				{
					positiveConditionPair2 = new PositiveConditionPair(positiveConditionPair, references);
					references.Add(positiveConditionPair, positiveConditionPair2);
				}
				PositiveConditions.Add(positiveConditionPair2);
			}
			references.Add(state.PositiveConditions, PositiveConditions);
		}
		NegativeConditions = references.Get(state.NegativeConditions);
		if (NegativeConditions == null && state.NegativeConditions != null)
		{
			NegativeConditions = new List<NegativeConditionPair>();
			for (int m = 0; m < state.NegativeConditions.Count; m++)
			{
				NegativeConditionPair negativeConditionPair = state.NegativeConditions[m];
				NegativeConditionPair negativeConditionPair2 = references.Get(negativeConditionPair);
				if (negativeConditionPair2 == null && negativeConditionPair != null)
				{
					negativeConditionPair2 = new NegativeConditionPair(negativeConditionPair, references);
					references.Add(negativeConditionPair, negativeConditionPair2);
				}
				NegativeConditions.Add(negativeConditionPair2);
			}
			references.Add(state.NegativeConditions, NegativeConditions);
		}
		PlayerStats = references.Get(state.PlayerStats);
		if (PlayerStats == null && state.PlayerStats != null)
		{
			PlayerStats = new CPlayerStats(state.PlayerStats, references);
			references.Add(state.PlayerStats, PlayerStats);
		}
		PlayerRecords = references.Get(state.PlayerRecords);
		if (PlayerRecords == null && state.PlayerRecords != null)
		{
			PlayerRecords = new CPlayerRecords(state.PlayerRecords, references);
			references.Add(state.PlayerRecords, PlayerRecords);
		}
		NewEquippedItemsWithModifiers = references.Get(state.NewEquippedItemsWithModifiers);
		if (NewEquippedItemsWithModifiers == null && state.NewEquippedItemsWithModifiers != null)
		{
			NewEquippedItemsWithModifiers = new List<string>();
			for (int n = 0; n < state.NewEquippedItemsWithModifiers.Count; n++)
			{
				string item3 = state.NewEquippedItemsWithModifiers[n];
				NewEquippedItemsWithModifiers.Add(item3);
			}
			references.Add(state.NewEquippedItemsWithModifiers, NewEquippedItemsWithModifiers);
		}
		PersonalQuest = references.Get(state.PersonalQuest);
		if (PersonalQuest == null && state.PersonalQuest != null)
		{
			PersonalQuest = new CPersonalQuestState(state.PersonalQuest, references);
			references.Add(state.PersonalQuest, PersonalQuest);
		}
		PossiblePersonalQuests = references.Get(state.PossiblePersonalQuests);
		if (PossiblePersonalQuests == null && state.PossiblePersonalQuests != null)
		{
			PossiblePersonalQuests = new List<CPersonalQuestState>();
			for (int num = 0; num < state.PossiblePersonalQuests.Count; num++)
			{
				CPersonalQuestState cPersonalQuestState = state.PossiblePersonalQuests[num];
				CPersonalQuestState cPersonalQuestState2 = references.Get(cPersonalQuestState);
				if (cPersonalQuestState2 == null && cPersonalQuestState != null)
				{
					cPersonalQuestState2 = new CPersonalQuestState(cPersonalQuestState, references);
					references.Add(cPersonalQuestState, cPersonalQuestState2);
				}
				PossiblePersonalQuests.Add(cPersonalQuestState2);
			}
			references.Add(state.PossiblePersonalQuests, PossiblePersonalQuests);
		}
		NextScenarioPositiveConditions = references.Get(state.NextScenarioPositiveConditions);
		if (NextScenarioPositiveConditions == null && state.NextScenarioPositiveConditions != null)
		{
			NextScenarioPositiveConditions = new List<PositiveConditionPair>();
			for (int num2 = 0; num2 < state.NextScenarioPositiveConditions.Count; num2++)
			{
				PositiveConditionPair positiveConditionPair3 = state.NextScenarioPositiveConditions[num2];
				PositiveConditionPair positiveConditionPair4 = references.Get(positiveConditionPair3);
				if (positiveConditionPair4 == null && positiveConditionPair3 != null)
				{
					positiveConditionPair4 = new PositiveConditionPair(positiveConditionPair3, references);
					references.Add(positiveConditionPair3, positiveConditionPair4);
				}
				NextScenarioPositiveConditions.Add(positiveConditionPair4);
			}
			references.Add(state.NextScenarioPositiveConditions, NextScenarioPositiveConditions);
		}
		NextScenarioNegativeConditions = references.Get(state.NextScenarioNegativeConditions);
		if (NextScenarioNegativeConditions == null && state.NextScenarioNegativeConditions != null)
		{
			NextScenarioNegativeConditions = new List<NegativeConditionPair>();
			for (int num3 = 0; num3 < state.NextScenarioNegativeConditions.Count; num3++)
			{
				NegativeConditionPair negativeConditionPair3 = state.NextScenarioNegativeConditions[num3];
				NegativeConditionPair negativeConditionPair4 = references.Get(negativeConditionPair3);
				if (negativeConditionPair4 == null && negativeConditionPair3 != null)
				{
					negativeConditionPair4 = new NegativeConditionPair(negativeConditionPair3, references);
					references.Add(negativeConditionPair3, negativeConditionPair4);
				}
				NextScenarioNegativeConditions.Add(negativeConditionPair4);
			}
			references.Add(state.NextScenarioNegativeConditions, NextScenarioNegativeConditions);
		}
		CompletedSoloQuestData = references.Get(state.CompletedSoloQuestData);
		if (CompletedSoloQuestData == null && state.CompletedSoloQuestData != null)
		{
			CompletedSoloQuestData = new List<CCompletedSoloQuestData>();
			for (int num4 = 0; num4 < state.CompletedSoloQuestData.Count; num4++)
			{
				CCompletedSoloQuestData cCompletedSoloQuestData = state.CompletedSoloQuestData[num4];
				CCompletedSoloQuestData cCompletedSoloQuestData2 = references.Get(cCompletedSoloQuestData);
				if (cCompletedSoloQuestData2 == null && cCompletedSoloQuestData != null)
				{
					cCompletedSoloQuestData2 = new CCompletedSoloQuestData(cCompletedSoloQuestData, references);
					references.Add(cCompletedSoloQuestData, cCompletedSoloQuestData2);
				}
				CompletedSoloQuestData.Add(cCompletedSoloQuestData2);
			}
			references.Add(state.CompletedSoloQuestData, CompletedSoloQuestData);
		}
		CurrentSmallItemOverride = state.CurrentSmallItemOverride;
		PerksStartedWith = state.PerksStartedWith;
		ExperiencePersonalGoal = state.ExperiencePersonalGoal;
		EnhancementsBought = state.EnhancementsBought;
		Donations = state.Donations;
		BattleGoalPerks = state.BattleGoalPerks;
		m_CharacterGold = state.m_CharacterGold;
		m_CharacterPersistentEnhancements = references.Get(state.m_CharacterPersistentEnhancements);
		if (m_CharacterPersistentEnhancements == null && state.m_CharacterPersistentEnhancements != null)
		{
			m_CharacterPersistentEnhancements = new List<CEnhancement>();
			for (int num5 = 0; num5 < state.m_CharacterPersistentEnhancements.Count; num5++)
			{
				CEnhancement cEnhancement = state.m_CharacterPersistentEnhancements[num5];
				CEnhancement cEnhancement2 = references.Get(cEnhancement);
				if (cEnhancement2 == null && cEnhancement != null)
				{
					cEnhancement2 = new CEnhancement(cEnhancement, references);
					references.Add(cEnhancement, cEnhancement2);
				}
				m_CharacterPersistentEnhancements.Add(cEnhancement2);
			}
			references.Add(state.m_CharacterPersistentEnhancements, m_CharacterPersistentEnhancements);
		}
		MaxCards = state.MaxCards;
		LevelPrevious = state.LevelPrevious;
		IsUnderMyControl = state.IsUnderMyControl;
		TempPositiveConditions = references.Get(state.TempPositiveConditions);
		if (TempPositiveConditions == null && state.TempPositiveConditions != null)
		{
			TempPositiveConditions = new List<PositiveConditionPair>();
			for (int num6 = 0; num6 < state.TempPositiveConditions.Count; num6++)
			{
				PositiveConditionPair positiveConditionPair5 = state.TempPositiveConditions[num6];
				PositiveConditionPair positiveConditionPair6 = references.Get(positiveConditionPair5);
				if (positiveConditionPair6 == null && positiveConditionPair5 != null)
				{
					positiveConditionPair6 = new PositiveConditionPair(positiveConditionPair5, references);
					references.Add(positiveConditionPair5, positiveConditionPair6);
				}
				TempPositiveConditions.Add(positiveConditionPair6);
			}
			references.Add(state.TempPositiveConditions, TempPositiveConditions);
		}
		TempNegativeConditions = references.Get(state.TempNegativeConditions);
		if (TempNegativeConditions == null && state.TempNegativeConditions != null)
		{
			TempNegativeConditions = new List<NegativeConditionPair>();
			for (int num7 = 0; num7 < state.TempNegativeConditions.Count; num7++)
			{
				NegativeConditionPair negativeConditionPair5 = state.TempNegativeConditions[num7];
				NegativeConditionPair negativeConditionPair6 = references.Get(negativeConditionPair5);
				if (negativeConditionPair6 == null && negativeConditionPair5 != null)
				{
					negativeConditionPair6 = new NegativeConditionPair(negativeConditionPair5, references);
					references.Add(negativeConditionPair5, negativeConditionPair6);
				}
				TempNegativeConditions.Add(negativeConditionPair6);
			}
			references.Add(state.TempNegativeConditions, TempNegativeConditions);
		}
		TempBlessConditions = references.Get(state.TempBlessConditions);
		if (TempBlessConditions == null && state.TempBlessConditions != null)
		{
			TempBlessConditions = new List<PositiveConditionPair>();
			for (int num8 = 0; num8 < state.TempBlessConditions.Count; num8++)
			{
				PositiveConditionPair positiveConditionPair7 = state.TempBlessConditions[num8];
				PositiveConditionPair positiveConditionPair8 = references.Get(positiveConditionPair7);
				if (positiveConditionPair8 == null && positiveConditionPair7 != null)
				{
					positiveConditionPair8 = new PositiveConditionPair(positiveConditionPair7, references);
					references.Add(positiveConditionPair7, positiveConditionPair8);
				}
				TempBlessConditions.Add(positiveConditionPair8);
			}
			references.Add(state.TempBlessConditions, TempBlessConditions);
		}
		CharacterState = state.CharacterState;
		EquippedItems = references.Get(state.EquippedItems);
		if (EquippedItems == null && state.EquippedItems != null)
		{
			EquippedItems = new List<CItem>();
			for (int num9 = 0; num9 < state.EquippedItems.Count; num9++)
			{
				CItem cItem = state.EquippedItems[num9];
				CItem cItem2 = references.Get(cItem);
				if (cItem2 == null && cItem != null)
				{
					cItem2 = new CItem(cItem, references);
					references.Add(cItem, cItem2);
				}
				EquippedItems.Add(cItem2);
			}
			references.Add(state.EquippedItems, EquippedItems);
		}
		BoundItems = references.Get(state.BoundItems);
		if (BoundItems == null && state.BoundItems != null)
		{
			BoundItems = new List<CItem>();
			for (int num10 = 0; num10 < state.BoundItems.Count; num10++)
			{
				CItem cItem3 = state.BoundItems[num10];
				CItem cItem4 = references.Get(cItem3);
				if (cItem4 == null && cItem3 != null)
				{
					cItem4 = new CItem(cItem3, references);
					references.Add(cItem3, cItem4);
				}
				BoundItems.Add(cItem4);
			}
			references.Add(state.BoundItems, BoundItems);
		}
		m_MultiplayerSoldItems = references.Get(state.m_MultiplayerSoldItems);
		if (m_MultiplayerSoldItems != null || state.m_MultiplayerSoldItems == null)
		{
			return;
		}
		m_MultiplayerSoldItems = new List<CItem>();
		for (int num11 = 0; num11 < state.m_MultiplayerSoldItems.Count; num11++)
		{
			CItem cItem5 = state.m_MultiplayerSoldItems[num11];
			CItem cItem6 = references.Get(cItem5);
			if (cItem6 == null && cItem5 != null)
			{
				cItem6 = new CItem(cItem5, references);
				references.Add(cItem5, cItem6);
			}
			m_MultiplayerSoldItems.Add(cItem6);
		}
		references.Add(state.m_MultiplayerSoldItems, m_MultiplayerSoldItems);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("CharacterName", CharacterName);
		info.AddValue("DisplayCharacterName", DisplayCharacterName);
		info.AddValue("CharacterID", CharacterID);
		info.AddValue("m_CharacterGold", m_CharacterGold);
		info.AddValue("EXP", EXP);
		info.AddValue("Level", Level);
		info.AddValue("CharacterPersistentEnhancements", m_CharacterPersistentEnhancements);
		info.AddValue("OwnedAbilityCardIDs", OwnedAbilityCardIDs);
		info.AddValue("HandAbilityCardIDs", HandAbilityCardIDs);
		info.AddValue("Perks", Perks);
		info.AddValue("PerkPoints", PerkPoints);
		info.AddValue("PerkChecks", PerkChecks);
		info.AddValue("TimesLevelReset", TimesLevelReset);
		info.AddValue("LastFreeLevelResetTicket", LastFreeLevelResetTicket);
		info.AddValue("SkinId", SkinId);
		info.AddValue("PositiveConditions", PositiveConditions);
		info.AddValue("NegativeConditions", NegativeConditions);
		info.AddValue("PlayerRecords", PlayerRecords);
		info.AddValue("NewEquippedItemsWithModifiers", NewEquippedItemsWithModifiers);
		info.AddValue("PersonalQuest", PersonalQuest);
		info.AddValue("PossiblePersonalQuests", PossiblePersonalQuests);
		info.AddValue("PerksStartedWith", PerksStartedWith);
		info.AddValue("ExperiencePersonalGoal", ExperiencePersonalGoal);
		info.AddValue("EnhancementsBought", EnhancementsBought);
		info.AddValue("Donations", Donations);
		info.AddValue("BattleGoalPerks", BattleGoalPerks);
		info.AddValue("NextScenarioNegativeConditions", NextScenarioNegativeConditions);
		info.AddValue("NextScenarioPositiveConditions", NextScenarioPositiveConditions);
		info.AddValue("CurrentSmallItemOverride", CurrentSmallItemOverride);
		info.AddValue("CompletedSoloQuestData", CompletedSoloQuestData);
		info.AddValue("PlayerStats", PlayerStats);
		lock (EquippedItems)
		{
			info.AddValue("EquippedItems", EquippedItems);
		}
		lock (BoundItems)
		{
			info.AddValue("BoundItems", BoundItems);
		}
	}

	public CMapCharacter(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "CharacterName":
					CharacterName = info.GetString("CharacterName");
					break;
				case "DisplayCharacterName":
					DisplayCharacterName = info.GetString("DisplayCharacterName");
					break;
				case "CharacterID":
					CharacterID = info.GetString("CharacterID");
					break;
				case "m_CharacterGold":
					m_CharacterGold = info.GetInt32("m_CharacterGold");
					break;
				case "EXP":
					EXP = info.GetInt32("EXP");
					break;
				case "Level":
					Level = info.GetInt32("Level");
					break;
				case "CharacterPersistentEnhancements":
					m_CharacterPersistentEnhancements = (List<CEnhancement>)info.GetValue("CharacterPersistentEnhancements", typeof(List<CEnhancement>));
					break;
				case "OwnedAbilityCardIDs":
					OwnedAbilityCardIDs = (List<int>)info.GetValue("OwnedAbilityCardIDs", typeof(List<int>));
					break;
				case "HandAbilityCardIDs":
					HandAbilityCardIDs = (List<int>)info.GetValue("HandAbilityCardIDs", typeof(List<int>));
					break;
				case "EquippedItems":
					EquippedItems = (List<CItem>)info.GetValue("EquippedItems", typeof(List<CItem>));
					break;
				case "BoundItems":
					BoundItems = (List<CItem>)info.GetValue("BoundItems", typeof(List<CItem>));
					break;
				case "Perks":
					Perks = (List<CharacterPerk>)info.GetValue("Perks", typeof(List<CharacterPerk>));
					break;
				case "PerkPoints":
					PerkPoints = info.GetInt32("PerkPoints");
					break;
				case "PerkChecks":
					PerkChecks = info.GetInt32("PerkChecks");
					break;
				case "TimesLevelReset":
					TimesLevelReset = info.GetInt32("TimesLevelReset");
					break;
				case "LastFreeLevelResetTicket":
					LastFreeLevelResetTicket = (CFreeLevelResetTicket)info.GetValue("LastFreeLevelResetTicket", typeof(CFreeLevelResetTicket));
					break;
				case "SkinId":
					SkinId = info.GetString("SkinId");
					break;
				case "PositiveConditions":
					PositiveConditions = (List<PositiveConditionPair>)info.GetValue("PositiveConditions", typeof(List<PositiveConditionPair>));
					break;
				case "NegativeConditions":
					NegativeConditions = (List<NegativeConditionPair>)info.GetValue("NegativeConditions", typeof(List<NegativeConditionPair>));
					break;
				case "PlayerStats":
					PlayerStats = (CPlayerStats)info.GetValue("PlayerStats", typeof(CPlayerStats));
					break;
				case "PlayerRecords":
					PlayerRecords = (CPlayerRecords)info.GetValue("PlayerRecords", typeof(CPlayerRecords));
					break;
				case "NewEquippedItemsWithModifiers":
					NewEquippedItemsWithModifiers = (List<string>)info.GetValue("NewEquippedItemsWithModifiers", typeof(List<string>));
					break;
				case "PersonalQuest":
					PersonalQuest = (CPersonalQuestState)info.GetValue("PersonalQuest", typeof(CPersonalQuestState));
					break;
				case "PossiblePersonalQuests":
					PossiblePersonalQuests = (List<CPersonalQuestState>)info.GetValue("PossiblePersonalQuests", typeof(List<CPersonalQuestState>));
					break;
				case "ExperiencePersonalGoal":
					ExperiencePersonalGoal = info.GetInt32("ExperiencePersonalGoal");
					break;
				case "PerksStartedWith":
					PerksStartedWith = info.GetInt32("PerksStartedWith");
					break;
				case "EnhancementsBought":
					EnhancementsBought = info.GetInt32("EnhancementsBought");
					break;
				case "Donations":
					Donations = info.GetInt32("Donations");
					break;
				case "BattleGoalPerks":
					BattleGoalPerks = info.GetInt32("BattleGoalPerks");
					break;
				case "NextScenarioNegativeConditions":
					NextScenarioNegativeConditions = (List<NegativeConditionPair>)info.GetValue("NextScenarioNegativeConditions", typeof(List<NegativeConditionPair>));
					break;
				case "NextScenarioPositiveConditions":
					NextScenarioPositiveConditions = (List<PositiveConditionPair>)info.GetValue("NextScenarioPositiveConditions", typeof(List<PositiveConditionPair>));
					break;
				case "CurrentSmallItemOverride":
					CurrentSmallItemOverride = info.GetInt32("CurrentSmallItemOverride");
					break;
				case "CompletedSoloQuestData":
					CompletedSoloQuestData = (List<CCompletedSoloQuestData>)info.GetValue("CompletedSoloQuestData", typeof(List<CCompletedSoloQuestData>));
					break;
				case "Character":
					CharacterID = ((ECharacter)info.GetValue("Character", typeof(ECharacter))/*cast due to .constrained prefix*/).ToString() + "ID";
					break;
				case "CharacterState":
					CharacterState = (ECharacterState)info.GetValue("CharacterState", typeof(ECharacterState));
					break;
				case "Enhancements":
					m_CharacterPersistentEnhancements = (List<CEnhancement>)info.GetValue("Enhancements", typeof(List<CEnhancement>));
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CMapCharacter entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		if (TempPositiveConditions == null)
		{
			TempPositiveConditions = new List<PositiveConditionPair>();
		}
		if (TempNegativeConditions == null)
		{
			TempNegativeConditions = new List<NegativeConditionPair>();
		}
		if (NextScenarioPositiveConditions == null)
		{
			NextScenarioPositiveConditions = new List<PositiveConditionPair>();
		}
		if (NextScenarioNegativeConditions == null)
		{
			NextScenarioNegativeConditions = new List<NegativeConditionPair>();
		}
		if (m_CharacterPersistentEnhancements == null)
		{
			m_CharacterPersistentEnhancements = new List<CEnhancement>();
		}
		if (CompletedSoloQuestData == null)
		{
			CompletedSoloQuestData = new List<CCompletedSoloQuestData>();
		}
		LevelPrevious = Math.Min(1, Level - 1);
		if (PlayerStats == null)
		{
			PlayerStats = new CPlayerStats(CharacterID);
		}
		if (PlayerRecords == null)
		{
			PlayerRecords = new CPlayerRecords(CharacterID, CharacterName);
		}
		if (BoundItems != null)
		{
			BoundItems.RemoveAll((CItem x) => x == null);
		}
		if (EquippedItems != null)
		{
			EquippedItems.RemoveAll((CItem x) => x == null);
		}
	}

	public CMapCharacter(string characterID, string characterName, int perksToStartWith)
	{
		CharacterName = characterName;
		DisplayCharacterName = characterName;
		CharacterID = characterID;
		m_CharacterGold = 0;
		EXP = 0;
		Level = 1;
		LevelPrevious = 1;
		m_CharacterPersistentEnhancements = new List<CEnhancement>();
		OwnedAbilityCardIDs = new List<int>();
		HandAbilityCardIDs = new List<int>();
		EquippedItems = new List<CItem>();
		BoundItems = new List<CItem>();
		Perks = new List<CharacterPerk>();
		PerkPoints = perksToStartWith;
		PerksStartedWith = perksToStartWith;
		PositiveConditions = new List<PositiveConditionPair>();
		NegativeConditions = new List<NegativeConditionPair>();
		TempPositiveConditions = new List<PositiveConditionPair>();
		TempNegativeConditions = new List<NegativeConditionPair>();
		NextScenarioPositiveConditions = new List<PositiveConditionPair>();
		NextScenarioNegativeConditions = new List<NegativeConditionPair>();
		PlayerStats = new CPlayerStats(characterID);
		PlayerRecords = new CPlayerRecords(characterID, characterName);
		NewEquippedItemsWithModifiers = new List<string>();
		ExperiencePersonalGoal = 0;
		EnhancementsBought = 0;
		Donations = 0;
		BattleGoalPerks = 0;
		CompletedSoloQuestData = new List<CCompletedSoloQuestData>();
		CCharacterClass cCharacterClass = CharacterClassManager.Classes.SingleOrDefault((CCharacterClass x) => x.ID == characterID);
		if (cCharacterClass != null)
		{
			foreach (PerksYMLData item in ScenarioRuleClient.SRLYML.Perks.Where((PerksYMLData x) => x.CharacterID == characterID))
			{
				Perks.Add(new CharacterPerk(item.ID, characterID));
			}
		}
		ValidatePerksOnLoad();
		MaxCards = cCharacterClass?.NumberAbilityCardsInBattle ?? 0;
		if (cCharacterClass != null && cCharacterClass.CharacterYML.GetCharacterSkin == CharacterYMLData.ECharacterSkin.Alternate)
		{
			SkinId = "ALT";
		}
		SetCards();
		CContentUnlocked_MapClientMessage message = new CContentUnlocked_MapClientMessage(CharacterID, "character", null);
		if (MapRuleLibraryClient.Instance.MessageHandler != null)
		{
			MapRuleLibraryClient.Instance.MessageHandler(message);
			MapRuleLibraryClient.Instance.MessageHandler(new CCharacterCreated_MapClientMessage(this));
		}
		else
		{
			DLLDebug.LogWarning("Message handler not set");
		}
	}

	public void OnMapStateAdventureStarted()
	{
		MaxCards = CharacterClassManager.Classes.SingleOrDefault((CCharacterClass x) => x.ID == CharacterID)?.NumberAbilityCardsInBattle ?? 0;
		ValidatePerksOnLoad();
		lock (BoundItems)
		{
			if (BoundItems != null && BoundItems.Count > 0)
			{
				foreach (CItem boundItem in BoundItems)
				{
					if (boundItem != null && boundItem.NetworkID == 0)
					{
						boundItem.NetworkID = AdventureState.MapState.GetNextItemNetworkID();
						SimpleLog.AddToSimpleLog("(CMapCharacter.OnMapStateAdventureStarted) (" + CharacterID + " BoundItems) Updating NetworkID for " + boundItem.Name + " to " + boundItem.NetworkID);
					}
				}
			}
		}
		lock (EquippedItems)
		{
			if (EquippedItems != null && EquippedItems.Count > 0)
			{
				foreach (CItem equippedItem in EquippedItems)
				{
					if (equippedItem != null && equippedItem.NetworkID == 0)
					{
						equippedItem.NetworkID = AdventureState.MapState.GetNextItemNetworkID();
						SimpleLog.AddToSimpleLog("(CMapCharacter.OnMapStateAdventureStarted) (" + CharacterID + " EquippedItems) Updating NetworkID for " + equippedItem.Name + " to " + equippedItem.NetworkID);
					}
				}
			}
		}
		if (AdventureState.MapState.IsCampaign)
		{
			if (PersonalQuest != null)
			{
				PersonalQuest.OnMapStateAdventureStarted();
			}
			else
			{
				DLLDebug.LogError("Attempted to refresh YML for characters personal quest, but personal quest was null. CharacterID: " + CharacterID + " CharacterName: " + CharacterName);
			}
		}
	}

	public void DrawPossiblePersonalQuests()
	{
		PossiblePersonalQuests = AdventureState.MapState.MapParty.DrawPossiblePersonalQuests().ConvertAll((PersonalQuestYMLData it) => new CPersonalQuestState(it, CharacterID, CharacterName));
	}

	public void SetCards()
	{
		OwnedAbilityCardIDs.Clear();
		HandAbilityCardIDs.Clear();
		foreach (CAbilityCard item in CharacterClassManager.Classes.SingleOrDefault((CCharacterClass x) => x.ID == CharacterID).AbilityCardsPool.FindAll((CAbilityCard c) => c.Level <= Level))
		{
			if (!OwnedAbilityCardIDs.Contains(item.ID))
			{
				OwnedAbilityCardIDs.Add(item.ID);
			}
			if (HandAbilityCardIDs.Count < MaxCards && !HandAbilityCardIDs.Contains(item.ID))
			{
				HandAbilityCardIDs.Add(item.ID);
			}
		}
	}

	public void SetCharacterLevel(int level)
	{
		Level = level;
	}

	public void GiveCreateCharacterGoldAndItems()
	{
		ModifyGold((1 + Math.Max(1, AdventureState.MapState.MapParty.ProsperityLevel)) * AdventureState.MapState.HeadquartersState.Headquarters.CreateCharacterGoldPerLevelAmount, useGoldModifier: true);
		AdventureState.MapState.ApplyRewards(TreasureTableProcessing.RollTreasureTables(AdventureState.MapState.MapRNG, MapYMLShared.ValidateTreasureTableRewards(AdventureState.MapState.HeadquartersState.Headquarters.CreateCharacterTreasureTables, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter)), AdventureState.MapState.MapParty.ScenarioLevel, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter)));
		SimpleLog.AddToSimpleLog("MapRNG (give gold): " + AdventureState.MapState.PeekMapRNG);
	}

	public void ModifyGold(int amount, bool useGoldModifier)
	{
		if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold)
		{
			if (useGoldModifier && AdventureState.MapState.Difficulty.HasGoldModifier)
			{
				amount = (int)Math.Round((float)amount * AdventureState.MapState.Difficulty.GoldModifier);
			}
			CharacterGold = Math.Max(0, CharacterGold + amount);
			if (amount > 0)
			{
				AdventureState.MapState.MapParty.TotalEarnedGold += amount;
			}
			return;
		}
		throw new Exception("Unable to modify character " + CharacterID + " gold if GoldMode is not EGoldMode.CharacterGold");
	}

	public void GainEXP(int amount, float xpModifier)
	{
		if (XPTable == null)
		{
			DLLDebug.LogError("Unable to find XP Table for " + CharacterID + ". Unable to level up character " + CharacterID);
			return;
		}
		if (HealthTable == null)
		{
			DLLDebug.LogError("Unable to find Health Table for " + CharacterID + ".  Unable to level up character " + CharacterID);
			return;
		}
		if (amount > 0)
		{
			amount = (int)Math.Round((float)amount * xpModifier);
		}
		EXP = Math.Max(0, EXP + amount);
		if (amount != 0)
		{
			CXpChanged_MapClientMessage message = new CXpChanged_MapClientMessage(CharacterID, CharacterName);
			if (MapRuleLibraryClient.Instance?.MessageHandler != null)
			{
				MapRuleLibraryClient.Instance.MessageHandler(message);
			}
		}
		if (EXP >= GetNextXPThreshold())
		{
			CCharacterLevelupAvailable_MapClientMessage message2 = new CCharacterLevelupAvailable_MapClientMessage(CharacterID, CharacterName);
			if (MapRuleLibraryClient.Instance?.MessageHandler != null)
			{
				MapRuleLibraryClient.Instance.MessageHandler(message2);
			}
		}
	}

	public int GetNextXPThreshold()
	{
		if (Level + 1 < XPTable.Count)
		{
			return XPTable[Level + 1];
		}
		return int.MaxValue;
	}

	public int GetXPThreshold(int level)
	{
		if (level < XPTable.Count)
		{
			return XPTable[level];
		}
		return int.MaxValue;
	}

	public int GetLevelsToLevelUp()
	{
		int num = 0;
		for (int i = Level + 1; i < XPTable.Count && EXP >= GetXPThreshold(i); i++)
		{
			num++;
		}
		return num;
	}

	public void MaxLevel()
	{
		Level = 9;
		PerkPoints = 100;
	}

	public void LevelUp(bool adjustPerkPointsToo = true)
	{
		if (EXP < GetNextXPThreshold())
		{
			return;
		}
		Level++;
		AdventureState.MapState.CheckTrophyAchievements(new CLevelUp_AchievementTrigger());
		GetSmallItemMax();
		AdventureState.MapState.CheckLockedContent();
		if (adjustPerkPointsToo)
		{
			UpdatePerkPoints(PerkPoints + 1);
		}
		if (MapRuleLibraryClient.Instance?.MessageHandler != null)
		{
			MapRuleLibraryClient.Instance.MessageHandler(new CCharacterLevelledUp_MapClientMessage(CharacterID, CharacterName));
		}
		if (EXP >= GetNextXPThreshold())
		{
			CCharacterLevelupAvailable_MapClientMessage message = new CCharacterLevelupAvailable_MapClientMessage(CharacterID, CharacterName);
			if (MapRuleLibraryClient.Instance?.MessageHandler != null)
			{
				MapRuleLibraryClient.Instance.MessageHandler(message);
			}
		}
		SimpleLog.AddToSimpleLog("[Scenario Level]: (After Level Up) Current Scenario level is " + AdventureState.MapState.MapParty.ScenarioLevel);
	}

	public void UpdatePerkPoints(int value)
	{
		if (value < 0)
		{
			DLLDebug.LogError("Set Negative Perk Points");
		}
		if (PerkPoints != value)
		{
			UnlockedPerkPoints = Mathf.Max(0, value - PerkPoints);
			PerkPoints = value;
			CCharacterPerkPointsChanged_MapClientMessage message = new CCharacterPerkPointsChanged_MapClientMessage(CharacterID, CharacterName, PerkPoints);
			if (MapRuleLibraryClient.Instance?.MessageHandler != null)
			{
				MapRuleLibraryClient.Instance.MessageHandler(message);
			}
		}
	}

	public void UpdatePerkChecks(int value, int increase = 0, bool battleGoal = false)
	{
		if (value < 0)
		{
			DLLDebug.LogError("Set Negative Perk Checks");
		}
		PerkChecks = value;
		if (battleGoal)
		{
			BattleGoalPerks += Math.Max(0, increase);
		}
		if (PerkChecks >= 3)
		{
			PerkChecks -= 3;
			UpdatePerkPoints(PerkPoints + 1);
		}
	}

	public void GainCard(CAbilityCard card)
	{
		if (!OwnedAbilityCardIDs.Contains(card.ID))
		{
			OwnedAbilityCardIDs.Add(card.ID);
			DLLDebug.Log(CharacterID + " has gained the card " + card.Name);
			CCharacterAbilityCardGained_MapClientMessage message = new CCharacterAbilityCardGained_MapClientMessage(CharacterID, CharacterName, card);
			MapRuleLibraryClient.Instance.MessageHandler(message);
		}
	}

	public void ResetLevels()
	{
		TimesLevelReset++;
		foreach (CharacterPerk perk in Perks)
		{
			perk.IsActive = false;
		}
		UpdatePerkPoints(PerksStartedWith + AdventureState.MapState.FindAppliedPerkPointRewardsForCharacter(CharacterID));
		Level = 1;
		LevelPrevious = 1;
		SetCards();
		UnequipItems(CheckEquippedItems);
		int price = 0;
		Enhancements.ForEach(delegate(CEnhancement it)
		{
			price += it.PaidPrice;
			it.BuyEnhancement(EEnhancement.NoEnhancement, 0);
		});
		Enhancements.Clear();
		if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold)
		{
			ModifyGold(price, useGoldModifier: false);
		}
		else
		{
			AdventureState.MapState.MapParty.ModifyPartyGold(price, useGoldModifier: false);
		}
		CRegenerateAllMapScenarios_MapClientMessage message = new CRegenerateAllMapScenarios_MapClientMessage();
		MapRuleLibraryClient.Instance.MessageHandler(message);
		MapRuleLibraryClient.Instance.MessageHandler(new CXpChanged_MapClientMessage(CharacterID, CharacterName));
		if (EXP >= GetNextXPThreshold())
		{
			MapRuleLibraryClient.Instance.MessageHandler(new CCharacterLevelupAvailable_MapClientMessage(CharacterID, CharacterName));
		}
	}

	public int CalculatePriceResetLevel()
	{
		if (LastFreeLevelResetTicket != null && !LastFreeLevelResetTicket.IsUsed)
		{
			return 0;
		}
		_ = TimesLevelReset;
		return 0;
	}

	public void ApplyEnhancements(List<CEnhancement> applyThese = null)
	{
		try
		{
			CCharacterClass cCharacterClass = CharacterClassManager.Classes.SingleOrDefault((CCharacterClass s) => s.ID == CharacterID);
			if (cCharacterClass != null)
			{
				if (applyThese != null && applyThese.Count > 0)
				{
					cCharacterClass.ApplyEnhancements(applyThese);
				}
				else if (Enhancements.Count > 0)
				{
					cCharacterClass.ApplyEnhancements(Enhancements);
				}
			}
			else
			{
				DLLDebug.LogError("Character not found when applying enhancements");
			}
		}
		catch (Exception ex)
		{
			DLLDebug.LogError("An exception occurred during Apply Enhancements\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public CPlayerActor GetActor()
	{
		return ScenarioManager.Scenario.PlayerActors.SingleOrDefault((CPlayerActor x) => x.CharacterClass.ID == CharacterID);
	}

	public void SetActorPersonalQuest()
	{
		CPlayerActor cPlayerActor = ScenarioManager.Scenario.PlayerActors.SingleOrDefault((CPlayerActor x) => x.CharacterClass.ID == CharacterID);
		if (cPlayerActor != null && PersonalQuest != null)
		{
			cPlayerActor.PersonalQuestID = PersonalQuest.ID;
		}
	}

	public void ApplyMapConditions()
	{
		CMapScenarioState currentMapScenarioState = AdventureState.MapState.CurrentMapScenarioState;
		if (currentMapScenarioState != null && !currentMapScenarioState.CurrentState.IsFirstLoad)
		{
			return;
		}
		CPlayerActor cPlayerActor = ScenarioManager.Scenario.AllPlayers.SingleOrDefault((CPlayerActor x) => x.CharacterClass.ID == CharacterID);
		if (cPlayerActor != null)
		{
			if (cPlayerActor.IsDead)
			{
				return;
			}
			NegativeConditions.AddRange(TempNegativeConditions);
			PositiveConditions.AddRange(TempPositiveConditions);
			NegativeConditions.AddRange(NextScenarioNegativeConditions);
			PositiveConditions.AddRange(NextScenarioPositiveConditions);
			TempNegativeConditions.Clear();
			TempPositiveConditions.Clear();
			NextScenarioNegativeConditions.Clear();
			NextScenarioPositiveConditions.Clear();
			TempBlessConditions = PositiveConditions.FindAll((PositiveConditionPair x) => x.PositiveCondition == CCondition.EPositiveCondition.Bless);
			foreach (NegativeConditionPair item in NegativeConditions.ToList())
			{
				cPlayerActor.ApplyCondition(cPlayerActor, item.NegativeCondition, item.RoundDuration, item.ConditionDecTrigger, "", isMapCondition: true);
			}
			{
				foreach (PositiveConditionPair item2 in PositiveConditions.Where((PositiveConditionPair x) => x.PositiveCondition != CCondition.EPositiveCondition.Bless).ToList())
				{
					cPlayerActor.ApplyCondition(cPlayerActor, item2.PositiveCondition, item2.RoundDuration, item2.ConditionDecTrigger, "", isMapCondition: true);
				}
				return;
			}
		}
		DLLDebug.LogError("Unable to find player " + CharacterID + " in scenario players");
	}

	public bool ApplyMapBlessCondition()
	{
		if (AdventureState.MapState.CurrentMapScenarioState.CurrentState.IsFirstLoad)
		{
			CPlayerActor cPlayerActor = ScenarioManager.Scenario.AllPlayers.SingleOrDefault((CPlayerActor x) => x.CharacterClass.ID == CharacterID);
			if (cPlayerActor != null)
			{
				if (!cPlayerActor.IsDead && TempBlessConditions.Count > 0)
				{
					cPlayerActor.ApplyCondition(cPlayerActor, TempBlessConditions[0].PositiveCondition, TempBlessConditions[0].RoundDuration, TempBlessConditions[0].ConditionDecTrigger, "", isMapCondition: true);
					TempBlessConditions.RemoveAt(0);
					return true;
				}
			}
			else
			{
				DLLDebug.LogError("Unable to find player " + CharacterID + " in scenario players");
			}
			return false;
		}
		return false;
	}

	public void RemoveMapConditions(RewardCondition.EConditionMapDuration conditionDuration)
	{
		foreach (NegativeConditionPair item in NegativeConditions.ToList())
		{
			if (item == null || item.MapDuration == conditionDuration)
			{
				NegativeConditions.Remove(item);
			}
		}
		foreach (PositiveConditionPair item2 in PositiveConditions.ToList())
		{
			if (item2 == null || item2.MapDuration == conditionDuration)
			{
				PositiveConditions.Remove(item2);
			}
		}
	}

	public void ApplyMapDamage(int damage)
	{
		if (AdventureState.MapState.CurrentMapScenarioState.CurrentState.IsFirstLoad)
		{
			CPlayerActor cPlayerActor = ScenarioManager.Scenario.PlayerActors.SingleOrDefault((CPlayerActor x) => x.CharacterClass.ID == CharacterID);
			if (cPlayerActor != null)
			{
				cPlayerActor.ApplyImmediateDamage(damage);
			}
			else
			{
				DLLDebug.LogError("Unable to find player " + CharacterID + " in scenario players");
			}
		}
	}

	public void ApplyPassiveItems(PlayerState playerState)
	{
		CMapScenarioState currentMapScenarioState = AdventureState.MapState.CurrentMapScenarioState;
		if (currentMapScenarioState == null || currentMapScenarioState.CurrentState.IsFirstLoad)
		{
			CPlayerActor cPlayerActor = ScenarioManager.Scenario.AllPlayers.SingleOrDefault((CPlayerActor x) => x.CharacterClass.ID == CharacterID);
			if (cPlayerActor != null)
			{
				cPlayerActor.ActivatePassiveItems(firstLoad: true, playerState);
			}
			else
			{
				DLLDebug.LogError("Unable to find player " + CharacterID + " in scenario players");
			}
		}
	}

	public List<CharacterPerk> GetPassivePerks()
	{
		List<CharacterPerk> list = new List<CharacterPerk>();
		CharacterPerk characterPerk = Perks.FirstOrDefault((CharacterPerk it) => it.IsActive && it.Perk.IgnoreNegativeItemEffects);
		if (characterPerk != null)
		{
			list.Add(characterPerk);
		}
		CharacterPerk characterPerk2 = Perks.FirstOrDefault((CharacterPerk it) => it.IsActive && it.Perk.IgnoreNegativeScenarioEffects);
		if (characterPerk2 != null)
		{
			list.Add(characterPerk2);
		}
		return list;
	}

	public List<IGrouping<AttackModifierYMLData, AttackModifierYMLData>> GetPerksConditionalModifierGroups()
	{
		return (from it in Perks.SelectMany((CharacterPerk it) => it.Perk.CardsToAdd)
			where it.IsConditionalModifier
			group it by it).ToList();
	}

	public List<IGrouping<AttackModifierYMLData, AttackModifierYMLData>> GetActivePerksConditionalModifierGroups()
	{
		return (from it in Perks.Where((CharacterPerk it) => it.IsActive).SelectMany((CharacterPerk it) => it.Perk.CardsToAdd)
			where it.IsConditionalModifier
			group it by it).ToList();
	}

	public bool HasPerkIgnoreNegativeItemEffects()
	{
		return Perks.Exists((CharacterPerk it) => it.IsActive && it.Perk.IgnoreNegativeItemEffects);
	}

	public bool HasPerkIgnoreNegativeScenarioEffects()
	{
		return Perks.Exists((CharacterPerk it) => it.IsActive && it.Perk.IgnoreNegativeScenarioEffects);
	}

	public void ValidatePerksOnLoad()
	{
		for (int num = Perks.Count - 1; num >= 0; num--)
		{
			CharacterPerk characterPerk = Perks[num];
			if (!ScenarioRuleClient.SRLYML.Perks.Contains(characterPerk.Perk) || characterPerk.Perk == null)
			{
				Perks.Remove(characterPerk);
				if (characterPerk.IsActive)
				{
					UpdatePerkPoints(PerkPoints + 1);
				}
			}
		}
		foreach (PerksYMLData perkYML in ScenarioRuleClient.SRLYML.Perks.Where((PerksYMLData w) => w.CharacterID == CharacterID))
		{
			List<CharacterPerk> list = Perks.FindAll((CharacterPerk p) => p.Perk == perkYML);
			int num2 = perkYML.Available - list.Count;
			if (num2 > 0)
			{
				for (int num3 = 0; num3 < num2; num3++)
				{
					Perks.Add(new CharacterPerk(perkYML.ID, CharacterID));
				}
			}
			else
			{
				if (num2 >= 0)
				{
					continue;
				}
				for (int num4 = 0; num4 < Math.Abs(num2); num4++)
				{
					Perks.Remove(list[num4]);
					if (list[num4].IsActive)
					{
						UpdatePerkPoints(PerkPoints + 1);
					}
				}
			}
		}
	}

	public void AddItemToEquippedItems(CItem addItem)
	{
		lock (EquippedItems)
		{
			EquippedItems.Add(addItem);
		}
	}

	public bool RemoveItemFromEquippedItems(CItem removeItem)
	{
		lock (EquippedItems)
		{
			if (EquippedItems.Remove(removeItem))
			{
				return true;
			}
			return false;
		}
	}

	public void ClearEquippedItems()
	{
		lock (EquippedItems)
		{
			EquippedItems.Clear();
		}
	}

	public void ResetEquippedItems()
	{
		lock (EquippedItems)
		{
			foreach (CItem equippedItem in EquippedItems)
			{
				equippedItem.SlotState = CItem.EItemSlotState.Equipped;
			}
		}
	}

	public void AddItemToBoundItems(CItem addItem)
	{
		lock (BoundItems)
		{
			BoundItems.Add(addItem);
		}
	}

	public void RemoveItemFromBoundItems(CItem removeItem)
	{
		lock (BoundItems)
		{
			BoundItems.Remove(removeItem);
		}
	}

	public void ClearBoundItems()
	{
		lock (BoundItems)
		{
			BoundItems.Clear();
		}
	}

	public int GetSmallItemMax()
	{
		int currentSmallItemOverride = CurrentSmallItemOverride;
		CurrentSmallItemOverride = 0;
		foreach (CItem checkEquippedItem in CheckEquippedItems)
		{
			CurrentSmallItemOverride += checkEquippedItem.YMLData.Data.SmallSlots;
		}
		int num = BaseSmallItemMax + CurrentSmallItemOverride;
		if (currentSmallItemOverride != CurrentSmallItemOverride)
		{
			List<CItem> list = (from it in CheckEquippedItems
				where it.YMLData.Slot == CItem.EItemSlot.SmallItem
				orderby it.SlotIndex
				select it).ToList();
			int num2 = list.Count();
			for (int num3 = 0; num3 < num2; num3++)
			{
				list[num3].SlotIndex = num3;
			}
			List<CItem> list2 = new List<CItem>();
			int num4 = num2 - 1;
			while (num4 >= 0 && num4 >= num)
			{
				list2.Add(list[num4]);
				DLLDebug.Log(CharacterID + " unequipped " + list[num4].YMLData.StringID + " (NetworkID: " + list[num4].NetworkID + ") due to small item max changing.");
				num4--;
			}
			if (list2.Count() > 0)
			{
				UnequipItems(list2);
			}
		}
		return num;
	}

	public void CheckSmallItemOverride(CItem item)
	{
		if (item.YMLData.Data.SmallSlots != 0)
		{
			GetSmallItemMax();
		}
	}

	public bool EquipItem(CItem item, int slotIndex = int.MaxValue, bool forceEquip = false)
	{
		if (forceEquip || (CheckBoundItems.Contains(item) && DoEquip(item, slotIndex)))
		{
			if (forceEquip)
			{
				item.SlotIndex = slotIndex;
				item.IsSlotIndexSet = true;
				item.SlotState = CItem.EItemSlotState.Equipped;
			}
			RemoveItemFromBoundItems(item);
			AddItemToEquippedItems(item);
			if (!forceEquip)
			{
				CheckSmallItemOverride(item);
			}
			if (NewEquippedItemsWithModifiers == null)
			{
				NewEquippedItemsWithModifiers = new List<string>();
			}
			foreach (KeyValuePair<AttackModifierYMLData, int> additionalModifier in item.YMLData.Data.AdditionalModifiers)
			{
				if (!NewEquippedItemsWithModifiers.Contains(additionalModifier.Key.Name))
				{
					NewEquippedItemsWithModifiers.Add(additionalModifier.Key.Name);
				}
			}
			CCharacterItemEquipped_MapClientMessage message = new CCharacterItemEquipped_MapClientMessage(this, item);
			if (MapRuleLibraryClient.Instance?.MessageHandler != null)
			{
				MapRuleLibraryClient.Instance.MessageHandler(message);
			}
			AdventureState.MapState.CheckLockedContent();
			return true;
		}
		return false;
	}

	public void UnequipPreviouslyEquippedItems(CItem itemToEquip, int targetSlotIndex)
	{
		switch (itemToEquip.YMLData.Slot)
		{
		case CItem.EItemSlot.SmallItem:
			if (CheckEquippedItems.Contains(itemToEquip))
			{
				UnequipItems(new List<CItem> { itemToEquip });
			}
			break;
		case CItem.EItemSlot.OneHand:
		{
			if (CheckEquippedItems.Contains(itemToEquip))
			{
				UnequipItems(new List<CItem> { itemToEquip });
				break;
			}
			CItem cItem = CheckEquippedItems.FirstOrDefault((CItem x) => x.YMLData.Slot == CItem.EItemSlot.TwoHand);
			if (cItem != null)
			{
				UnequipItems(new List<CItem> { cItem });
			}
			break;
		}
		case CItem.EItemSlot.TwoHand:
			foreach (CItem item in CheckEquippedItems.FindAll((CItem x) => x.YMLData.Slot == CItem.EItemSlot.OneHand))
			{
				UnequipItems(new List<CItem> { item });
			}
			break;
		}
		CItem cItem2 = CheckEquippedItems.FirstOrDefault((CItem x) => x.YMLData.Slot == itemToEquip.YMLData.Slot && x.SlotIndex == targetSlotIndex);
		if (cItem2 != null)
		{
			UnequipItems(new List<CItem> { cItem2 });
		}
	}

	public void UnequipItems(List<CItem> items)
	{
		bool flag = false;
		foreach (CItem item in items)
		{
			item.SlotIndex = int.MaxValue;
			item.IsSlotIndexSet = false;
			item.SlotState = CItem.EItemSlotState.None;
			RemoveEquippedItem(item);
			AddItemToBoundItems(item);
			flag |= item.YMLData.Data.SmallSlots != 0;
		}
		if (flag)
		{
			GetSmallItemMax();
		}
		CCharacterItemUnequipped_MapClientMessage message = new CCharacterItemUnequipped_MapClientMessage(this, items);
		if (MapRuleLibraryClient.Instance?.MessageHandler != null)
		{
			MapRuleLibraryClient.Instance.MessageHandler(message);
		}
	}

	public void RemoveEquippedItem(CItem item)
	{
		if (!RemoveItemFromEquippedItems(item) || NewEquippedItemsWithModifiers == null)
		{
			return;
		}
		foreach (string item2 in item.YMLData.Data.AdditionalModifiers.Select((KeyValuePair<AttackModifierYMLData, int> k) => k.Key.Name))
		{
			NewEquippedItemsWithModifiers.Remove(item2);
		}
	}

	private bool DoEquip(CItem item, int slotIndex = int.MaxValue)
	{
		if (!item.CanEquipItem(CharacterID))
		{
			return false;
		}
		switch (item.YMLData.Slot)
		{
		case CItem.EItemSlot.Head:
		case CItem.EItemSlot.Body:
		case CItem.EItemSlot.Legs:
			if (!CheckEquippedItems.Any((CItem a) => a.YMLData.Slot == item.YMLData.Slot))
			{
				item.SlotIndex = 0;
				item.IsSlotIndexSet = true;
				item.SlotState = CItem.EItemSlotState.Equipped;
				return true;
			}
			break;
		case CItem.EItemSlot.OneHand:
			if (!CheckEquippedItems.Any((CItem a) => a.YMLData.Slot == CItem.EItemSlot.TwoHand))
			{
				if (slotIndex != int.MaxValue)
				{
					item.SlotIndex = slotIndex;
					item.IsSlotIndexSet = true;
					item.SlotState = CItem.EItemSlotState.Equipped;
					return true;
				}
				List<CItem> list = CheckEquippedItems.FindAll((CItem i) => i.YMLData.Slot == CItem.EItemSlot.OneHand);
				if (list.Count <= 1)
				{
					item.SlotIndex = ((list.Count > 0 && list[0].SlotIndex == 0) ? 1 : 0);
					item.IsSlotIndexSet = true;
					item.SlotState = CItem.EItemSlotState.Equipped;
					return true;
				}
			}
			break;
		case CItem.EItemSlot.TwoHand:
			if (!CheckEquippedItems.Any((CItem a) => a.YMLData.Slot == CItem.EItemSlot.OneHand || a.YMLData.Slot == CItem.EItemSlot.TwoHand))
			{
				item.SlotIndex = 0;
				item.IsSlotIndexSet = true;
				item.SlotState = CItem.EItemSlotState.Equipped;
				return true;
			}
			break;
		case CItem.EItemSlot.SmallItem:
		{
			if (TryGetSmallItemSlotIndex(slotIndex, out var resultSlotIndex))
			{
				item.SlotIndex = resultSlotIndex;
				item.IsSlotIndexSet = true;
				item.SlotState = CItem.EItemSlotState.Equipped;
				return true;
			}
			break;
		}
		}
		return false;
		bool TryGetSmallItemSlotIndex(int num, out int reference)
		{
			if (num != int.MaxValue)
			{
				reference = num;
				return num < SmallItemMax;
			}
			return TryGetFreeSmallItemSlotIndex(out reference);
		}
	}

	private bool TryGetFreeSmallItemSlotIndex(out int slotIndex)
	{
		IEnumerable<CItem> source = CheckEquippedItems.Where((CItem x) => x.YMLData.Slot == CItem.EItemSlot.SmallItem);
		int smallItemMax = SmallItemMax;
		int i;
		for (i = 0; i < smallItemMax; i++)
		{
			if (!source.Any((CItem x) => x.SlotIndex == i))
			{
				slotIndex = i;
				return true;
			}
		}
		slotIndex = -1;
		return false;
	}

	public void UnbindItem(CItem item)
	{
		if (CheckBoundItems.Contains(item))
		{
			RemoveItemFromBoundItems(item);
			AdventureState.MapState.MapParty.AddItemToUnboundItems(item);
			CCharacterItemUnbound_MapClientMessage message = new CCharacterItemUnbound_MapClientMessage(this, item);
			if (MapRuleLibraryClient.Instance?.MessageHandler != null)
			{
				MapRuleLibraryClient.Instance.MessageHandler(message);
			}
		}
	}

	public int EquippedItemsNum(CItem.EItemSlot slot)
	{
		int num = 0;
		foreach (CItem checkEquippedItem in CheckEquippedItems)
		{
			if (checkEquippedItem.YMLData.Slot == slot || (slot == CItem.EItemSlot.OneHand && checkEquippedItem.YMLData.Slot == CItem.EItemSlot.TwoHand))
			{
				num += ((checkEquippedItem.YMLData.Slot != CItem.EItemSlot.TwoHand) ? 1 : 2);
			}
		}
		return num;
	}

	public int CalculateCostToEquip(CItem item, CItem.EItemSlot slot)
	{
		if ((slot == CItem.EItemSlot.OneHand || slot == item.YMLData.Slot) && item.YMLData.Slot == CItem.EItemSlot.TwoHand)
		{
			return 2;
		}
		if (slot != item.YMLData.Slot)
		{
			return 0;
		}
		return 1;
	}

	public bool CanBeAssignedToSlot(CItem item, CItem.EItemSlot selectedItemSlot)
	{
		if (item.YMLData.Slot != selectedItemSlot)
		{
			if (selectedItemSlot == CItem.EItemSlot.OneHand)
			{
				return item.YMLData.Slot == CItem.EItemSlot.TwoHand;
			}
			return false;
		}
		return true;
	}

	public int SlotCapacity(CItem.EItemSlot slot)
	{
		return slot switch
		{
			CItem.EItemSlot.OneHand => 2, 
			CItem.EItemSlot.SmallItem => SmallItemMax, 
			_ => 1, 
		};
	}

	public bool ContainsEquipedItemWithId(CItem item)
	{
		return CheckEquippedItems.Exists((CItem it) => item.ID == it.ID);
	}

	public void UpdatePlayerStats(CPlayerStats scenarioStats)
	{
		if (PlayerStats != null)
		{
			PlayerStats.Kills.AddRange(scenarioStats.Kills);
			PlayerStats.Deaths.AddRange(scenarioStats.Deaths);
			PlayerStats.DamageDealt.AddRange(scenarioStats.DamageDealt);
			PlayerStats.Actor.AddRange(scenarioStats.Actor);
			PlayerStats.DamageReceived.AddRange(scenarioStats.DamageReceived);
			PlayerStats.DestroyedObstacles.AddRange(scenarioStats.DestroyedObstacles);
			PlayerStats.Infusions.AddRange(scenarioStats.Infusions);
			PlayerStats.Consumed.AddRange(scenarioStats.Consumed);
			PlayerStats.Abilities.AddRange(scenarioStats.Abilities);
			PlayerStats.Modifiers.AddRange(scenarioStats.Modifiers);
			PlayerStats.Heals.AddRange(scenarioStats.Heals);
			PlayerStats.Items.AddRange(scenarioStats.Items);
			PlayerStats.Loot.AddRange(scenarioStats.Loot);
			PlayerStats.Monsters.AddRange(scenarioStats.Monsters);
			PlayerStats.Door.AddRange(scenarioStats.Door);
			PlayerStats.Trap.AddRange(scenarioStats.Trap);
			PlayerStats.Hand.AddRange(scenarioStats.Hand);
			PlayerStats.XP.AddRange(scenarioStats.XP);
			PlayerStats.BattleGoalPerks.AddRange(scenarioStats.BattleGoalPerks);
			PlayerStats.Donations.AddRange(scenarioStats.Donations);
			PlayerStats.PersonalQuests.AddRange(scenarioStats.PersonalQuests);
			PlayerStats.Enhancements.AddRange(scenarioStats.Enhancements);
			PlayerStats.LoseCards.AddRange(scenarioStats.LoseCards);
			PlayerStats.DiscardCard.AddRange(scenarioStats.DiscardCard);
			PlayerStats.EndTurn.AddRange(scenarioStats.EndTurn);
			PlayerStats.LostAdjacency.AddRange(scenarioStats.LostAdjacency);
		}
	}

	public void GainTempCondition(NegativeConditionPair condition)
	{
		TempNegativeConditions.Add(condition);
		CCharacterCondition_MapClientMessage message = new CCharacterCondition_MapClientMessage(CharacterID, CharacterName, condition.NegativeCondition);
		if (MapRuleLibraryClient.Instance?.MessageHandler != null)
		{
			MapRuleLibraryClient.Instance.MessageHandler(message);
		}
	}

	public void GainTempCondition(PositiveConditionPair condition)
	{
		TempPositiveConditions.Add(condition);
		CCharacterCondition_MapClientMessage message = new CCharacterCondition_MapClientMessage(CharacterID, CharacterName, condition.PositiveCondition);
		if (MapRuleLibraryClient.Instance?.MessageHandler != null)
		{
			MapRuleLibraryClient.Instance.MessageHandler(message);
		}
	}

	public void GainNextScenarioCondition(NegativeConditionPair condition)
	{
		NextScenarioNegativeConditions.Add(condition);
		CCharacterCondition_MapClientMessage message = new CCharacterCondition_MapClientMessage(CharacterID, CharacterName, condition.NegativeCondition);
		if (MapRuleLibraryClient.Instance?.MessageHandler != null)
		{
			MapRuleLibraryClient.Instance.MessageHandler(message);
		}
	}

	public void GainNextScenarioCondition(PositiveConditionPair condition)
	{
		NextScenarioPositiveConditions.Add(condition);
		CCharacterCondition_MapClientMessage message = new CCharacterCondition_MapClientMessage(CharacterID, CharacterName, condition.PositiveCondition);
		if (MapRuleLibraryClient.Instance?.MessageHandler != null)
		{
			MapRuleLibraryClient.Instance.MessageHandler(message);
		}
	}

	public void AssignPersonalQuest(CPersonalQuestState personalQuest, bool removeCard = true)
	{
		if (personalQuest != null)
		{
			PersonalQuest = personalQuest;
			if (removeCard)
			{
				AdventureState.MapState.MapParty.PersonalQuestDeck.RemoveCard(personalQuest.ID);
			}
		}
		else
		{
			DLLDebug.LogError("Attempted to assign a null personal quest to CMapCharacter.\n" + Environment.StackTrace);
		}
	}
}
