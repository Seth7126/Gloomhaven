using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Client;
using MapRuleLibrary.Source.Shared;
using MapRuleLibrary.State;
using MapRuleLibrary.YML.Achievements;
using MapRuleLibrary.YML.Events;
using MapRuleLibrary.YML.PersonalQuests;
using MapRuleLibrary.YML.Shared;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SharedLibrary;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;
using StateCodeGenerator;

namespace MapRuleLibrary.Party;

[Serializable]
public class CMapParty : ISerializable
{
	private int m_PartyGold;

	private List<CMapCharacter> m_OldRetiredCharactersList;

	private List<CMapCharacter> m_CharactersRetiredThisSession = new List<CMapCharacter>();

	[NonSerialized]
	private List<CItem> m_MultiplayerItemReserve = new List<CItem>();

	[NonSerialized]
	private List<CMapCharacter> m_OldSelectedCharacters;

	[NonSerialized]
	private List<CItem> m_MultiplayerSoldItems = new List<CItem>();

	private volatile List<CItem> UnboundItems;

	private volatile List<CMapCharacter> Characters;

	public bool IsInitialised { get; private set; }

	public List<CPartyAchievement> Achievements { get; private set; }

	public int ProsperityXP { get; set; }

	public int Reputation { get; set; }

	public CCardDeck CityEventDeck { get; set; }

	public CCardDeck RoadEventDeck { get; set; }

	public CCardDeck PersonalQuestDeck { get; set; }

	public bool JOTLEventsLoaded { get; set; }

	public CPlayerStats HeroSummonsStats { get; private set; }

	public CPlayerStats MonstersStats { get; private set; }

	public List<CPlayerStatsScenario> LastScenarioHeroSummon { get; private set; }

	public List<CPlayerStatsScenario> LastScenarioMonster { get; private set; }

	public CMapCharacter[] SelectedCharactersArray { get; private set; }

	public int TotalEarnedGold { get; set; }

	public CNextScenarioEffects NextScenarioEffects { get; private set; }

	public List<CPlayerStatsScenario> CurrentScenarioStats { get; private set; }

	public List<CPlayerStatsScenario> LastScenarioStats { get; private set; }

	public HashSet<string> IntroductionDoneIds { get; set; }

	public List<string> UnlockedCharacterIDs { get; set; }

	public List<string> NewUnlockedCharacterIDs { get; set; }

	public List<CPlayerRecords> RetiredCharacterRecords { get; private set; }

	public Dictionary<string, List<CEnhancement>> ClassPersistentEnhancements { get; private set; }

	public List<CMapCharacter> CharactersRetiredThisSession => m_CharactersRetiredThisSession;

	public bool IsCreatingCharacter { get; set; }

	public List<CItem> MultiplayerSoldItems => m_MultiplayerSoldItems;

	public List<CItem> MultiplayerItemReserve => m_MultiplayerItemReserve;

	public IEnumerable<CMapCharacter> SelectedCharacters => SelectedCharactersArray.Where((CMapCharacter w) => w != null);

	public List<CItem> CheckUnboundItems
	{
		get
		{
			lock (UnboundItems)
			{
				return UnboundItems.ToList();
			}
		}
	}

	public List<CMapCharacter> CheckCharacters
	{
		get
		{
			lock (Characters)
			{
				return Characters.ToList();
			}
		}
	}

	public float ThreatLevel
	{
		get
		{
			if (SelectedCharactersArray == null || SelectedCharacters.Count() <= 0)
			{
				DLLDebug.LogError("Unable to calculate threat level as there are no selected characters");
				return 1f;
			}
			return (float)SelectedCharacters.Count() / 2f * AdventureState.MapState.Difficulty.ThreatModifier;
		}
	}

	public int PartyLevel
	{
		get
		{
			if (SelectedCharactersArray == null || SelectedCharacters.Count() <= 0)
			{
				return 1;
			}
			return (int)Math.Ceiling((float)SelectedCharacters.Sum((CMapCharacter x) => x.Level) / (2f * (float)SelectedCharacters.Count()));
		}
	}

	public int ProsperityLevel => CalculateProsperityLevel(ProsperityXP);

	public int ShopDiscount
	{
		get
		{
			if (AdventureState.MapState.IsCampaign)
			{
				int num = 6;
				if (AdventureState.MapState.HeadquartersState.Headquarters.ReputationToShopDiscount != null)
				{
					foreach (int item in AdventureState.MapState.HeadquartersState.Headquarters.ReputationToShopDiscount)
					{
						if (Reputation >= item)
						{
							num--;
							continue;
						}
						return num;
					}
				}
				return num;
			}
			return 0;
		}
	}

	public int ScenarioLevel => Math.Min(7, PartyLevel + ((AdventureState.MapState != null) ? AdventureState.MapState.Difficulty.ScenarioLevelModifier : 0));

	public int InProgressScenarioLevel
	{
		get
		{
			if (AdventureState.MapState.InProgressQuestState == null)
			{
				return ScenarioLevel;
			}
			return AdventureState.MapState.InProgressQuestState.ScenarioLevelToUse;
		}
	}

	public int PartyGold
	{
		get
		{
			if (AdventureState.MapState == null || AdventureState.MapState.GoldMode == EGoldMode.PartyGold)
			{
				return m_PartyGold;
			}
			throw new Exception("Must not access Party Gold when gold mode is not set to EGoldMode.PartyGold");
		}
		private set
		{
			if (AdventureState.MapState.GoldMode == EGoldMode.PartyGold)
			{
				int partyGold = m_PartyGold;
				m_PartyGold = Math.Max(0, value);
				if (partyGold != m_PartyGold)
				{
					CGoldChanged_MapClientMessage message = new CGoldChanged_MapClientMessage(m_PartyGold, partyGold);
					if (MapRuleLibraryClient.Instance?.MessageHandler != null)
					{
						MapRuleLibraryClient.Instance.MessageHandler(message);
					}
				}
				return;
			}
			throw new Exception("Must not access Party Gold when gold mode is not set to EGoldMode.PartyGold");
		}
	}

	public int TotalSelectedCharactersGold
	{
		get
		{
			if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold)
			{
				return SelectedCharacters.Sum((CMapCharacter x) => x.CharacterGold);
			}
			throw new Exception("Must not TotalSelectedCharactersGold when gold mode is not set to EGoldMode.CharacterGold");
		}
	}

	public List<CMapCharacter> UnselectedCharacters => CheckCharacters.Except(SelectedCharacters).ToList();

	public List<List<string>> GetAllSelectedCharactersSelectedAbilityCards
	{
		get
		{
			List<List<string>> list = new List<List<string>>();
			foreach (CMapCharacter selectedCharacter in SelectedCharacters)
			{
				List<string> list2 = new List<string>();
				foreach (CAbilityCard handAbilityCard in selectedCharacter.HandAbilityCards)
				{
					list2.Add(handAbilityCard.Name);
				}
				list.Add(list2);
			}
			return list;
		}
	}

	public List<List<string>> GetAllSelectedCharactersUnselectedAbilityCards
	{
		get
		{
			List<List<string>> list = new List<List<string>>();
			foreach (CMapCharacter selectedCharacter in SelectedCharacters)
			{
				List<string> list2 = new List<string>();
				foreach (CAbilityCard ownedAbilityCard in selectedCharacter.OwnedAbilityCards)
				{
					if (!selectedCharacter.HandAbilityCards.Contains(ownedAbilityCard))
					{
						list2.Add(ownedAbilityCard.Name);
					}
				}
				list.Add(list2);
			}
			return list;
		}
	}

	public List<CItem> BoundItems => CheckCharacters.SelectMany((CMapCharacter sm) => sm.CheckBoundItems).ToList();

	public List<CPartyAchievement> ClaimedAchievements => Achievements.Where((CPartyAchievement a) => a.State == EAchievementState.RewardsClaimed).ToList();

	public CMapParty()
	{
	}

	public CMapParty(CMapParty state, ReferenceDictionary references)
	{
		IsInitialised = state.IsInitialised;
		Achievements = references.Get(state.Achievements);
		if (Achievements == null && state.Achievements != null)
		{
			Achievements = new List<CPartyAchievement>();
			for (int i = 0; i < state.Achievements.Count; i++)
			{
				CPartyAchievement cPartyAchievement = state.Achievements[i];
				CPartyAchievement cPartyAchievement2 = references.Get(cPartyAchievement);
				if (cPartyAchievement2 == null && cPartyAchievement != null)
				{
					cPartyAchievement2 = new CPartyAchievement(cPartyAchievement, references);
					references.Add(cPartyAchievement, cPartyAchievement2);
				}
				Achievements.Add(cPartyAchievement2);
			}
			references.Add(state.Achievements, Achievements);
		}
		ProsperityXP = state.ProsperityXP;
		Reputation = state.Reputation;
		CityEventDeck = references.Get(state.CityEventDeck);
		if (CityEventDeck == null && state.CityEventDeck != null)
		{
			CityEventDeck = new CCardDeck(state.CityEventDeck, references);
			references.Add(state.CityEventDeck, CityEventDeck);
		}
		RoadEventDeck = references.Get(state.RoadEventDeck);
		if (RoadEventDeck == null && state.RoadEventDeck != null)
		{
			RoadEventDeck = new CCardDeck(state.RoadEventDeck, references);
			references.Add(state.RoadEventDeck, RoadEventDeck);
		}
		PersonalQuestDeck = references.Get(state.PersonalQuestDeck);
		if (PersonalQuestDeck == null && state.PersonalQuestDeck != null)
		{
			PersonalQuestDeck = new CCardDeck(state.PersonalQuestDeck, references);
			references.Add(state.PersonalQuestDeck, PersonalQuestDeck);
		}
		JOTLEventsLoaded = state.JOTLEventsLoaded;
		HeroSummonsStats = references.Get(state.HeroSummonsStats);
		if (HeroSummonsStats == null && state.HeroSummonsStats != null)
		{
			HeroSummonsStats = new CPlayerStats(state.HeroSummonsStats, references);
			references.Add(state.HeroSummonsStats, HeroSummonsStats);
		}
		MonstersStats = references.Get(state.MonstersStats);
		if (MonstersStats == null && state.MonstersStats != null)
		{
			MonstersStats = new CPlayerStats(state.MonstersStats, references);
			references.Add(state.MonstersStats, MonstersStats);
		}
		LastScenarioHeroSummon = references.Get(state.LastScenarioHeroSummon);
		if (LastScenarioHeroSummon == null && state.LastScenarioHeroSummon != null)
		{
			LastScenarioHeroSummon = new List<CPlayerStatsScenario>();
			for (int j = 0; j < state.LastScenarioHeroSummon.Count; j++)
			{
				CPlayerStatsScenario cPlayerStatsScenario = state.LastScenarioHeroSummon[j];
				CPlayerStatsScenario cPlayerStatsScenario2 = references.Get(cPlayerStatsScenario);
				if (cPlayerStatsScenario2 == null && cPlayerStatsScenario != null)
				{
					cPlayerStatsScenario2 = new CPlayerStatsScenario(cPlayerStatsScenario, references);
					references.Add(cPlayerStatsScenario, cPlayerStatsScenario2);
				}
				LastScenarioHeroSummon.Add(cPlayerStatsScenario2);
			}
			references.Add(state.LastScenarioHeroSummon, LastScenarioHeroSummon);
		}
		LastScenarioMonster = references.Get(state.LastScenarioMonster);
		if (LastScenarioMonster == null && state.LastScenarioMonster != null)
		{
			LastScenarioMonster = new List<CPlayerStatsScenario>();
			for (int k = 0; k < state.LastScenarioMonster.Count; k++)
			{
				CPlayerStatsScenario cPlayerStatsScenario3 = state.LastScenarioMonster[k];
				CPlayerStatsScenario cPlayerStatsScenario4 = references.Get(cPlayerStatsScenario3);
				if (cPlayerStatsScenario4 == null && cPlayerStatsScenario3 != null)
				{
					cPlayerStatsScenario4 = new CPlayerStatsScenario(cPlayerStatsScenario3, references);
					references.Add(cPlayerStatsScenario3, cPlayerStatsScenario4);
				}
				LastScenarioMonster.Add(cPlayerStatsScenario4);
			}
			references.Add(state.LastScenarioMonster, LastScenarioMonster);
		}
		SelectedCharactersArray = references.Get(state.SelectedCharactersArray);
		if (SelectedCharactersArray == null && state.SelectedCharactersArray != null)
		{
			SelectedCharactersArray = new CMapCharacter[state.SelectedCharactersArray.Length];
			for (int l = 0; l < state.SelectedCharactersArray.Length; l++)
			{
				CMapCharacter cMapCharacter = references.Get(state.SelectedCharactersArray[l]);
				if (cMapCharacter == null && state.SelectedCharactersArray[l] != null)
				{
					cMapCharacter = new CMapCharacter(state.SelectedCharactersArray[l], references);
					references.Add(state.SelectedCharactersArray[l], cMapCharacter);
				}
				SelectedCharactersArray[l] = cMapCharacter;
			}
			references.Add(state.SelectedCharactersArray, SelectedCharactersArray);
		}
		TotalEarnedGold = state.TotalEarnedGold;
		NextScenarioEffects = references.Get(state.NextScenarioEffects);
		if (NextScenarioEffects == null && state.NextScenarioEffects != null)
		{
			NextScenarioEffects = new CNextScenarioEffects(state.NextScenarioEffects, references);
			references.Add(state.NextScenarioEffects, NextScenarioEffects);
		}
		CurrentScenarioStats = references.Get(state.CurrentScenarioStats);
		if (CurrentScenarioStats == null && state.CurrentScenarioStats != null)
		{
			CurrentScenarioStats = new List<CPlayerStatsScenario>();
			for (int m = 0; m < state.CurrentScenarioStats.Count; m++)
			{
				CPlayerStatsScenario cPlayerStatsScenario5 = state.CurrentScenarioStats[m];
				CPlayerStatsScenario cPlayerStatsScenario6 = references.Get(cPlayerStatsScenario5);
				if (cPlayerStatsScenario6 == null && cPlayerStatsScenario5 != null)
				{
					cPlayerStatsScenario6 = new CPlayerStatsScenario(cPlayerStatsScenario5, references);
					references.Add(cPlayerStatsScenario5, cPlayerStatsScenario6);
				}
				CurrentScenarioStats.Add(cPlayerStatsScenario6);
			}
			references.Add(state.CurrentScenarioStats, CurrentScenarioStats);
		}
		LastScenarioStats = references.Get(state.LastScenarioStats);
		if (LastScenarioStats == null && state.LastScenarioStats != null)
		{
			LastScenarioStats = new List<CPlayerStatsScenario>();
			for (int n = 0; n < state.LastScenarioStats.Count; n++)
			{
				CPlayerStatsScenario cPlayerStatsScenario7 = state.LastScenarioStats[n];
				CPlayerStatsScenario cPlayerStatsScenario8 = references.Get(cPlayerStatsScenario7);
				if (cPlayerStatsScenario8 == null && cPlayerStatsScenario7 != null)
				{
					cPlayerStatsScenario8 = new CPlayerStatsScenario(cPlayerStatsScenario7, references);
					references.Add(cPlayerStatsScenario7, cPlayerStatsScenario8);
				}
				LastScenarioStats.Add(cPlayerStatsScenario8);
			}
			references.Add(state.LastScenarioStats, LastScenarioStats);
		}
		IntroductionDoneIds = references.Get(state.IntroductionDoneIds);
		if (IntroductionDoneIds == null && state.IntroductionDoneIds != null)
		{
			IntroductionDoneIds = new HashSet<string>(state.IntroductionDoneIds.Comparer);
			foreach (string introductionDoneId in state.IntroductionDoneIds)
			{
				IntroductionDoneIds.Add(introductionDoneId);
			}
			references.Add(state.IntroductionDoneIds, IntroductionDoneIds);
		}
		UnlockedCharacterIDs = references.Get(state.UnlockedCharacterIDs);
		if (UnlockedCharacterIDs == null && state.UnlockedCharacterIDs != null)
		{
			UnlockedCharacterIDs = new List<string>();
			for (int num = 0; num < state.UnlockedCharacterIDs.Count; num++)
			{
				string item = state.UnlockedCharacterIDs[num];
				UnlockedCharacterIDs.Add(item);
			}
			references.Add(state.UnlockedCharacterIDs, UnlockedCharacterIDs);
		}
		NewUnlockedCharacterIDs = references.Get(state.NewUnlockedCharacterIDs);
		if (NewUnlockedCharacterIDs == null && state.NewUnlockedCharacterIDs != null)
		{
			NewUnlockedCharacterIDs = new List<string>();
			for (int num2 = 0; num2 < state.NewUnlockedCharacterIDs.Count; num2++)
			{
				string item2 = state.NewUnlockedCharacterIDs[num2];
				NewUnlockedCharacterIDs.Add(item2);
			}
			references.Add(state.NewUnlockedCharacterIDs, NewUnlockedCharacterIDs);
		}
		RetiredCharacterRecords = references.Get(state.RetiredCharacterRecords);
		if (RetiredCharacterRecords == null && state.RetiredCharacterRecords != null)
		{
			RetiredCharacterRecords = new List<CPlayerRecords>();
			for (int num3 = 0; num3 < state.RetiredCharacterRecords.Count; num3++)
			{
				CPlayerRecords cPlayerRecords = state.RetiredCharacterRecords[num3];
				CPlayerRecords cPlayerRecords2 = references.Get(cPlayerRecords);
				if (cPlayerRecords2 == null && cPlayerRecords != null)
				{
					cPlayerRecords2 = new CPlayerRecords(cPlayerRecords, references);
					references.Add(cPlayerRecords, cPlayerRecords2);
				}
				RetiredCharacterRecords.Add(cPlayerRecords2);
			}
			references.Add(state.RetiredCharacterRecords, RetiredCharacterRecords);
		}
		ClassPersistentEnhancements = references.Get(state.ClassPersistentEnhancements);
		if (ClassPersistentEnhancements == null && state.ClassPersistentEnhancements != null)
		{
			ClassPersistentEnhancements = new Dictionary<string, List<CEnhancement>>(state.ClassPersistentEnhancements.Comparer);
			foreach (KeyValuePair<string, List<CEnhancement>> classPersistentEnhancement in state.ClassPersistentEnhancements)
			{
				string key = classPersistentEnhancement.Key;
				List<CEnhancement> list = references.Get(classPersistentEnhancement.Value);
				if (list == null && classPersistentEnhancement.Value != null)
				{
					list = new List<CEnhancement>();
					for (int num4 = 0; num4 < classPersistentEnhancement.Value.Count; num4++)
					{
						CEnhancement cEnhancement = classPersistentEnhancement.Value[num4];
						CEnhancement cEnhancement2 = references.Get(cEnhancement);
						if (cEnhancement2 == null && cEnhancement != null)
						{
							cEnhancement2 = new CEnhancement(cEnhancement, references);
							references.Add(cEnhancement, cEnhancement2);
						}
						list.Add(cEnhancement2);
					}
					references.Add(classPersistentEnhancement.Value, list);
				}
				ClassPersistentEnhancements.Add(key, list);
			}
			references.Add(state.ClassPersistentEnhancements, ClassPersistentEnhancements);
		}
		IsCreatingCharacter = state.IsCreatingCharacter;
		m_PartyGold = state.m_PartyGold;
		m_OldRetiredCharactersList = references.Get(state.m_OldRetiredCharactersList);
		if (m_OldRetiredCharactersList == null && state.m_OldRetiredCharactersList != null)
		{
			m_OldRetiredCharactersList = new List<CMapCharacter>();
			for (int num5 = 0; num5 < state.m_OldRetiredCharactersList.Count; num5++)
			{
				CMapCharacter cMapCharacter2 = state.m_OldRetiredCharactersList[num5];
				CMapCharacter cMapCharacter3 = references.Get(cMapCharacter2);
				if (cMapCharacter3 == null && cMapCharacter2 != null)
				{
					cMapCharacter3 = new CMapCharacter(cMapCharacter2, references);
					references.Add(cMapCharacter2, cMapCharacter3);
				}
				m_OldRetiredCharactersList.Add(cMapCharacter3);
			}
			references.Add(state.m_OldRetiredCharactersList, m_OldRetiredCharactersList);
		}
		m_CharactersRetiredThisSession = references.Get(state.m_CharactersRetiredThisSession);
		if (m_CharactersRetiredThisSession == null && state.m_CharactersRetiredThisSession != null)
		{
			m_CharactersRetiredThisSession = new List<CMapCharacter>();
			for (int num6 = 0; num6 < state.m_CharactersRetiredThisSession.Count; num6++)
			{
				CMapCharacter cMapCharacter4 = state.m_CharactersRetiredThisSession[num6];
				CMapCharacter cMapCharacter5 = references.Get(cMapCharacter4);
				if (cMapCharacter5 == null && cMapCharacter4 != null)
				{
					cMapCharacter5 = new CMapCharacter(cMapCharacter4, references);
					references.Add(cMapCharacter4, cMapCharacter5);
				}
				m_CharactersRetiredThisSession.Add(cMapCharacter5);
			}
			references.Add(state.m_CharactersRetiredThisSession, m_CharactersRetiredThisSession);
		}
		m_MultiplayerItemReserve = references.Get(state.m_MultiplayerItemReserve);
		if (m_MultiplayerItemReserve == null && state.m_MultiplayerItemReserve != null)
		{
			m_MultiplayerItemReserve = new List<CItem>();
			for (int num7 = 0; num7 < state.m_MultiplayerItemReserve.Count; num7++)
			{
				CItem cItem = state.m_MultiplayerItemReserve[num7];
				CItem cItem2 = references.Get(cItem);
				if (cItem2 == null && cItem != null)
				{
					cItem2 = new CItem(cItem, references);
					references.Add(cItem, cItem2);
				}
				m_MultiplayerItemReserve.Add(cItem2);
			}
			references.Add(state.m_MultiplayerItemReserve, m_MultiplayerItemReserve);
		}
		m_OldSelectedCharacters = references.Get(state.m_OldSelectedCharacters);
		if (m_OldSelectedCharacters == null && state.m_OldSelectedCharacters != null)
		{
			m_OldSelectedCharacters = new List<CMapCharacter>();
			for (int num8 = 0; num8 < state.m_OldSelectedCharacters.Count; num8++)
			{
				CMapCharacter cMapCharacter6 = state.m_OldSelectedCharacters[num8];
				CMapCharacter cMapCharacter7 = references.Get(cMapCharacter6);
				if (cMapCharacter7 == null && cMapCharacter6 != null)
				{
					cMapCharacter7 = new CMapCharacter(cMapCharacter6, references);
					references.Add(cMapCharacter6, cMapCharacter7);
				}
				m_OldSelectedCharacters.Add(cMapCharacter7);
			}
			references.Add(state.m_OldSelectedCharacters, m_OldSelectedCharacters);
		}
		m_MultiplayerSoldItems = references.Get(state.m_MultiplayerSoldItems);
		if (m_MultiplayerSoldItems == null && state.m_MultiplayerSoldItems != null)
		{
			m_MultiplayerSoldItems = new List<CItem>();
			for (int num9 = 0; num9 < state.m_MultiplayerSoldItems.Count; num9++)
			{
				CItem cItem3 = state.m_MultiplayerSoldItems[num9];
				CItem cItem4 = references.Get(cItem3);
				if (cItem4 == null && cItem3 != null)
				{
					cItem4 = new CItem(cItem3, references);
					references.Add(cItem3, cItem4);
				}
				m_MultiplayerSoldItems.Add(cItem4);
			}
			references.Add(state.m_MultiplayerSoldItems, m_MultiplayerSoldItems);
		}
		UnboundItems = references.Get(state.UnboundItems);
		if (UnboundItems == null && state.UnboundItems != null)
		{
			UnboundItems = new List<CItem>();
			for (int num10 = 0; num10 < state.UnboundItems.Count; num10++)
			{
				CItem cItem5 = state.UnboundItems[num10];
				CItem cItem6 = references.Get(cItem5);
				if (cItem6 == null && cItem5 != null)
				{
					cItem6 = new CItem(cItem5, references);
					references.Add(cItem5, cItem6);
				}
				UnboundItems.Add(cItem6);
			}
			references.Add(state.UnboundItems, UnboundItems);
		}
		Characters = references.Get(state.Characters);
		if (Characters != null || state.Characters == null)
		{
			return;
		}
		Characters = new List<CMapCharacter>();
		for (int num11 = 0; num11 < state.Characters.Count; num11++)
		{
			CMapCharacter cMapCharacter8 = state.Characters[num11];
			CMapCharacter cMapCharacter9 = references.Get(cMapCharacter8);
			if (cMapCharacter9 == null && cMapCharacter8 != null)
			{
				cMapCharacter9 = new CMapCharacter(cMapCharacter8, references);
				references.Add(cMapCharacter8, cMapCharacter9);
			}
			Characters.Add(cMapCharacter9);
		}
		references.Add(state.Characters, Characters);
	}

	public static int CalculateProsperityLevel(int xp)
	{
		int num = 0;
		if (AdventureState.MapState != null && AdventureState.MapState.HeadquartersState.Headquarters.LevelToProsperity != null)
		{
			foreach (int item in AdventureState.MapState.HeadquartersState.Headquarters.LevelToProsperity)
			{
				if (xp >= item)
				{
					num++;
					continue;
				}
				return num;
			}
		}
		return num;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("IsInitialised", IsInitialised);
		info.AddValue("Achievements", Achievements);
		info.AddValue("ProsperityXP", ProsperityXP);
		info.AddValue("Reputation", Reputation);
		info.AddValue("CityEventDeck", CityEventDeck);
		info.AddValue("RoadEventDeck", RoadEventDeck);
		info.AddValue("PersonalQuestDeck", PersonalQuestDeck);
		info.AddValue("JOTLEventsLoaded", JOTLEventsLoaded);
		info.AddValue("SelectedCharactersArray", SelectedCharactersArray);
		info.AddValue("TotalEarnedGold", TotalEarnedGold);
		info.AddValue("NextScenarioEffects", NextScenarioEffects);
		info.AddValue("m_PartyGold", m_PartyGold);
		info.AddValue("IntroductionDoneIds", IntroductionDoneIds);
		info.AddValue("UnlockedCharacterIDs", UnlockedCharacterIDs);
		info.AddValue("NewUnlockedCharacterIDs", NewUnlockedCharacterIDs);
		info.AddValue("RetiredCharacterRecords", RetiredCharacterRecords);
		info.AddValue("ClassPersistentEnhancements", ClassPersistentEnhancements);
		lock (UnboundItems)
		{
			info.AddValue("UnboundItems", UnboundItems);
		}
		lock (Characters)
		{
			info.AddValue("Characters", Characters);
		}
	}

	public CMapParty(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "IsInitialised":
					IsInitialised = info.GetBoolean("IsInitialised");
					break;
				case "Achievements":
					Achievements = (List<CPartyAchievement>)info.GetValue("Achievements", typeof(List<CPartyAchievement>));
					break;
				case "ProsperityXP":
					ProsperityXP = info.GetInt32("ProsperityXP");
					break;
				case "Reputation":
					Reputation = info.GetInt32("Reputation");
					break;
				case "CityEventDeck":
					CityEventDeck = (CCardDeck)info.GetValue("CityEventDeck", typeof(CCardDeck));
					break;
				case "RoadEventDeck":
					RoadEventDeck = (CCardDeck)info.GetValue("RoadEventDeck", typeof(CCardDeck));
					break;
				case "PersonalQuestDeck":
					PersonalQuestDeck = (CCardDeck)info.GetValue("PersonalQuestDeck", typeof(CCardDeck));
					break;
				case "JOTLEventsLoaded":
					JOTLEventsLoaded = info.GetBoolean("JOTLEventsLoaded");
					break;
				case "UnboundItems":
					UnboundItems = (List<CItem>)info.GetValue("UnboundItems", typeof(List<CItem>));
					break;
				case "HeroSummonsStats":
					HeroSummonsStats = (CPlayerStats)info.GetValue("HeroSummonsStats", typeof(CPlayerStats));
					break;
				case "MonstersStats":
					MonstersStats = (CPlayerStats)info.GetValue("MonstersStats", typeof(CPlayerStats));
					break;
				case "Characters":
					Characters = (List<CMapCharacter>)info.GetValue("Characters", typeof(List<CMapCharacter>));
					break;
				case "SelectedCharactersArray":
					SelectedCharactersArray = (CMapCharacter[])info.GetValue("SelectedCharactersArray", typeof(CMapCharacter[]));
					break;
				case "TotalEarnedGold":
					TotalEarnedGold = info.GetInt32("TotalEarnedGold");
					break;
				case "NextScenarioEffects":
					NextScenarioEffects = (CNextScenarioEffects)info.GetValue("NextScenarioEffects", typeof(CNextScenarioEffects));
					break;
				case "LastScenarioStats":
					try
					{
						LastScenarioStats = (List<CPlayerStatsScenario>)info.GetValue("LastScenarioStats", typeof(List<CPlayerStatsScenario>));
					}
					catch
					{
						LastScenarioStats = new List<CPlayerStatsScenario>();
					}
					break;
				case "LastScenarioHeroSummon":
					LastScenarioHeroSummon = (List<CPlayerStatsScenario>)info.GetValue("LastScenarioHeroSummon", typeof(List<CPlayerStatsScenario>));
					break;
				case "LastScenarioMonster":
					LastScenarioMonster = (List<CPlayerStatsScenario>)info.GetValue("LastScenarioMonster", typeof(List<CPlayerStatsScenario>));
					break;
				case "m_PartyGold":
					m_PartyGold = info.GetInt32("m_PartyGold");
					break;
				case "IntroductionDoneIds":
					IntroductionDoneIds = (HashSet<string>)info.GetValue("IntroductionDoneIds", typeof(HashSet<string>));
					break;
				case "UnlockedCharacterIDs":
					UnlockedCharacterIDs = (List<string>)info.GetValue("UnlockedCharacterIDs", typeof(List<string>));
					break;
				case "NewUnlockedCharacterIDs":
					NewUnlockedCharacterIDs = (List<string>)info.GetValue("NewUnlockedCharacterIDs", typeof(List<string>));
					break;
				case "RetiredCharacterRecords":
					RetiredCharacterRecords = (List<CPlayerRecords>)info.GetValue("RetiredCharacterRecords", typeof(List<CPlayerRecords>));
					break;
				case "ClassPersistentEnhancements":
					ClassPersistentEnhancements = (Dictionary<string, List<CEnhancement>>)info.GetValue("ClassPersistentEnhancements", typeof(Dictionary<string, List<CEnhancement>>));
					break;
				case "Prosperity":
					ProsperityXP = info.GetInt32("Prosperity");
					break;
				case "RetiredCharacters":
					m_OldRetiredCharactersList = (List<CMapCharacter>)info.GetValue("RetiredCharacters", typeof(List<CMapCharacter>));
					break;
				case "SelectedCharacters":
					m_OldSelectedCharacters = (List<CMapCharacter>)info.GetValue("SelectedCharacters", typeof(List<CMapCharacter>));
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CMapParty entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		if (!IsInitialised)
		{
			IsInitialised = true;
		}
		if (LastScenarioHeroSummon == null)
		{
			LastScenarioHeroSummon = new List<CPlayerStatsScenario>();
		}
		if (LastScenarioMonster == null)
		{
			LastScenarioMonster = new List<CPlayerStatsScenario>();
		}
		if (HeroSummonsStats == null)
		{
			HeroSummonsStats = new CPlayerStats(null);
		}
		if (MonstersStats == null)
		{
			MonstersStats = new CPlayerStats(null);
		}
		if (LastScenarioStats == null)
		{
			LastScenarioStats = new List<CPlayerStatsScenario>();
		}
		if (CurrentScenarioStats == null)
		{
			CurrentScenarioStats = new List<CPlayerStatsScenario>();
		}
		if (UnboundItems != null)
		{
			UnboundItems.RemoveAll((CItem x) => x == null);
		}
		if (m_OldSelectedCharacters != null)
		{
			SelectedCharactersArray = new CMapCharacter[4];
			for (int num = 0; num < m_OldSelectedCharacters.Count; num++)
			{
				SelectedCharactersArray[num] = m_OldSelectedCharacters[num];
			}
		}
		List<CMapCharacter> checkCharacters = CheckCharacters;
		for (int num2 = checkCharacters.Count - 1; num2 >= 0; num2--)
		{
			CMapCharacter cMapCharacter = checkCharacters[num2];
			if (cMapCharacter == null)
			{
				RemoveCharacterFromCharactersList(cMapCharacter);
				checkCharacters.Remove(cMapCharacter);
			}
		}
		CMapCharacter cMapCharacter2 = checkCharacters.SingleOrDefault((CMapCharacter s) => s.CharacterID == "2ID");
		if (cMapCharacter2 != null)
		{
			RemoveCharacterFromCharactersList(cMapCharacter2);
			checkCharacters.Remove(cMapCharacter2);
			if (SelectedCharactersArray.Contains(cMapCharacter2))
			{
				RemoveSelectedCharacter(cMapCharacter2);
			}
		}
		if (UnlockedCharacterIDs == null)
		{
			UnlockedCharacterIDs = new List<string>();
			for (int num3 = checkCharacters.Count - 1; num3 >= 0; num3--)
			{
				CMapCharacter cMapCharacter3 = checkCharacters[num3];
				if (cMapCharacter3.CharacterState == ECharacterState.Unlocked)
				{
					UnlockedCharacterIDs.Add(cMapCharacter3.CharacterID);
				}
				else
				{
					RemoveCharacterFromCharactersList(cMapCharacter3);
					checkCharacters.Remove(cMapCharacter3);
					if (SelectedCharactersArray.Contains(cMapCharacter3))
					{
						RemoveSelectedCharacter(cMapCharacter3);
					}
				}
			}
		}
		for (int num4 = 0; num4 < SelectedCharactersArray.Length; num4++)
		{
			if (SelectedCharactersArray[num4] != null && !checkCharacters.Contains(SelectedCharactersArray[num4]))
			{
				RemoveSelectedCharacter(SelectedCharactersArray[num4]);
			}
		}
		if (NewUnlockedCharacterIDs == null)
		{
			NewUnlockedCharacterIDs = new List<string>();
		}
		if (RetiredCharacterRecords == null)
		{
			RetiredCharacterRecords = new List<CPlayerRecords>();
		}
		if (m_OldRetiredCharactersList != null)
		{
			RetiredCharacterRecords.AddRange(m_OldRetiredCharactersList.Select((CMapCharacter x) => x.PlayerRecords));
		}
		if (IntroductionDoneIds == null)
		{
			IntroductionDoneIds = new HashSet<string>();
		}
		if (ClassPersistentEnhancements == null)
		{
			ClassPersistentEnhancements = new Dictionary<string, List<CEnhancement>>();
			foreach (CCharacterClass @class in CharacterClassManager.Classes)
			{
				ClassPersistentEnhancements.Add(@class.CharacterID, new List<CEnhancement>());
			}
		}
		if (SelectedCharactersArray == null)
		{
			return;
		}
		for (int num5 = 0; num5 < SelectedCharactersArray.Length; num5++)
		{
			CMapCharacter cMapCharacter4 = SelectedCharactersArray[num5];
			if (cMapCharacter4 == null)
			{
				continue;
			}
			for (int num6 = 0; num6 < SelectedCharactersArray.Length; num6++)
			{
				if (num5 != num6)
				{
					CMapCharacter cMapCharacter5 = SelectedCharactersArray[num6];
					if (cMapCharacter5 != null && cMapCharacter4.CharacterID == cMapCharacter5.CharacterID)
					{
						SelectedCharactersArray[num6] = null;
					}
				}
			}
		}
	}

	public CMapParty(string stack)
	{
		DLLDebug.Log("Creating new MapParty.\n" + stack);
		Characters = new List<CMapCharacter>();
		RetiredCharacterRecords = new List<CPlayerRecords>();
		SelectedCharactersArray = new CMapCharacter[4];
		UnlockedCharacterIDs = new List<string>();
		NewUnlockedCharacterIDs = new List<string>();
		IntroductionDoneIds = new HashSet<string>();
		ProsperityXP = 0;
		Reputation = 0;
		UnboundItems = new List<CItem>();
		NextScenarioEffects = new CNextScenarioEffects();
		LastScenarioStats = new List<CPlayerStatsScenario>();
		CurrentScenarioStats = new List<CPlayerStatsScenario>();
		LastScenarioHeroSummon = new List<CPlayerStatsScenario>();
		LastScenarioMonster = new List<CPlayerStatsScenario>();
		HeroSummonsStats = new CPlayerStats(null);
		MonstersStats = new CPlayerStats(null);
		Achievements = new List<CPartyAchievement>();
		foreach (AchievementYMLData achievement in MapRuleLibraryClient.MRLYML.Achievements)
		{
			Achievements.Add(new CPartyAchievement(achievement));
		}
		CityEventDeck = new CCardDeck(MapRuleLibraryClient.MRLYML.InitialEvents.CityEvents);
		RoadEventDeck = new CCardDeck(MapRuleLibraryClient.MRLYML.InitialEvents.RoadEvents);
		if (MapRuleLibraryClient.MRLYML.InitialEvents.JOTLEvents != null && MapRuleLibraryClient.MRLYML.InitialEvents.JOTLEvents.Count > 0)
		{
			JOTLEventsLoaded = true;
			if (!RoadEventDeck.Cards.Any((string x) => x.ToUpper().Contains("JOTL")))
			{
				CCardDeck cCardDeck = new CCardDeck(MapRuleLibraryClient.MRLYML.InitialEvents.JOTLEvents);
				RoadEventDeck.ExpandDeck(cCardDeck.Cards);
			}
		}
		else
		{
			JOTLEventsLoaded = false;
		}
		PersonalQuestDeck = new CCardDeck(MapRuleLibraryClient.MRLYML.PersonalQuests.Select((PersonalQuestYMLData x) => x.ID).ToList());
		ClassPersistentEnhancements = new Dictionary<string, List<CEnhancement>>();
		foreach (CCharacterClass @class in CharacterClassManager.Classes)
		{
			ClassPersistentEnhancements.Add(@class.CharacterID, new List<CEnhancement>());
		}
	}

	public void Init()
	{
		if (!IsInitialised)
		{
			SimpleLog.AddToSimpleLog("Shuffling City Event Deck");
			CityEventDeck.Shuffle();
			SimpleLog.AddToSimpleLog("City Event Deck Top 5 Cards: " + CityEventDeck.GetEventCardsOnTop(5));
			SimpleLog.AddToSimpleLog("Shuffling Road Event Deck");
			RoadEventDeck.Shuffle();
			SimpleLog.AddToSimpleLog("Road Event Deck Top 5 Cards: " + CityEventDeck.GetEventCardsOnTop(5));
			SimpleLog.AddToSimpleLog("Shuffling Personal Quest Deck");
			PersonalQuestDeck.Shuffle();
			SimpleLog.AddToSimpleLog("Personal Quest Deck Top 5 Cards: " + CityEventDeck.GetEventCardsOnTop(5));
			IsInitialised = true;
		}
	}

	public void AddSelectedCharacterToNextOpenSlot(CMapCharacter characterToAdd)
	{
		for (int i = 0; i < 4; i++)
		{
			if (SelectedCharactersArray[i] == null)
			{
				if (!SelectedCharactersArray.Any((CMapCharacter x) => x != null && x.CharacterID == characterToAdd.CharacterID))
				{
					SelectedCharactersArray[i] = characterToAdd;
				}
				else
				{
					DLLDebug.LogError("Attempted to add a character to the selected characters of CharacterID: " + characterToAdd.CharacterID + " when a character with that CharacterID is already contained in the party");
				}
				break;
			}
		}
	}

	public void RetireCharacter(string characterID)
	{
		CMapCharacter cMapCharacter = SelectedCharacters.SingleOrDefault((CMapCharacter x) => x.CharacterID == characterID);
		if (cMapCharacter != null)
		{
			string personalQuestID = cMapCharacter.PersonalQuest.ID;
			PersonalQuestYMLData personalQuestYMLData = MapRuleLibraryClient.MRLYML.PersonalQuests.SingleOrDefault((PersonalQuestYMLData e) => e.ID == personalQuestID);
			if (personalQuestYMLData != null && personalQuestYMLData.Discard == true)
			{
				PersonalQuestDeck.RemoveCard(personalQuestID, CCardDeck.EDiscard.Destroy);
			}
			RemoveSelectedCharacter(cMapCharacter);
			CharactersRetiredThisSession.Add(cMapCharacter);
			RemoveCharacterFromCharactersList(cMapCharacter);
			RetiredCharacterRecords.Add(cMapCharacter.PlayerRecords);
			AdventureState.MapState.CheckTrophyAchievements(new CRetireCharacter_AchievementTrigger());
			if (cMapCharacter.PerksStartedWith >= AdventureState.MapState.HeadquartersState.CurrentStartingPerksAmount)
			{
				AdventureState.MapState.HeadquartersState.CurrentStartingPerksAmount++;
			}
			foreach (CItem allCharacterItem in cMapCharacter.AllCharacterItems)
			{
				RemoveItem(allCharacterItem, cMapCharacter);
				if (allCharacterItem.Tradeable)
				{
					AdventureState.MapState.HeadquartersState.AddItemToMerchantStock(allCharacterItem);
				}
			}
			AdventureState.MapState.ApplyRewards(TreasureTableProcessing.RollTreasureTables(AdventureState.MapState.MapRNG, MapYMLShared.ValidateTreasureTableRewards(AdventureState.MapState.HeadquartersState.Headquarters.RetirementTreasureTables, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter)), AdventureState.MapState.MapParty.ScenarioLevel, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter)));
			CCharacterRetired_MapClientMessage message = new CCharacterRetired_MapClientMessage(characterID, personalQuestID);
			if (MapRuleLibraryClient.Instance?.MessageHandler != null)
			{
				MapRuleLibraryClient.Instance.MessageHandler(message);
			}
			else
			{
				DLLDebug.LogWarning("Message handler not set");
			}
			SimpleLog.AddToSimpleLog("MapRNG (retire character): " + AdventureState.MapState.PeekMapRNG);
		}
		else
		{
			DLLDebug.LogError("Couldn't find character to retire");
		}
	}

	public void OnMapStateAdventureStarted()
	{
		foreach (AchievementYMLData achievement in MapRuleLibraryClient.MRLYML.Achievements)
		{
			if (!Achievements.Any((CPartyAchievement a) => a.ID == achievement.ID))
			{
				Achievements.Add(new CPartyAchievement(achievement));
			}
		}
		List<CPartyAchievement> list = new List<CPartyAchievement>();
		foreach (CPartyAchievement achievement2 in Achievements)
		{
			if (achievement2.Achievement == null)
			{
				list.Add(achievement2);
			}
		}
		foreach (PersonalQuestYMLData personalQuest in MapRuleLibraryClient.MRLYML.PersonalQuests)
		{
			if (PersonalQuestDeck.DiscardedCards.Contains(personalQuest.ID) && PersonalQuestDeck.Cards.Contains(personalQuest.ID))
			{
				PersonalQuestDeck.DiscardedCards.Remove(personalQuest.ID);
			}
		}
		if (AdventureState.MapState.IsCampaign)
		{
			foreach (CMapCharacter checkCharacter in CheckCharacters)
			{
				if (checkCharacter.PersonalQuest == null)
				{
					RemoveCharacterFromCharactersList(checkCharacter);
					if (SelectedCharactersArray.Contains(checkCharacter))
					{
						RemoveSelectedCharacter(checkCharacter);
					}
				}
			}
		}
		foreach (PersonalQuestYMLData personalQuestData in MapRuleLibraryClient.MRLYML.PersonalQuests)
		{
			if (CheckCharacters.Any((CMapCharacter x) => x.PersonalQuest.ID == personalQuestData.ID) || PersonalQuestDeck.Cards.Any((string x) => x == personalQuestData.ID))
			{
				continue;
			}
			List<TreasureTable> initialTables = ScenarioRuleClient.SRLYML.TreasureTables.Where((TreasureTable w) => personalQuestData.FinalStepTreasureTable.Contains(w.Name)).ToList();
			foreach (RewardGroup item in TreasureTableProcessing.RollTreasureTables(new SharedLibrary.Random(), MapYMLShared.ValidateTreasureTableRewards(initialTables, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter)), AdventureState.MapState.MapParty.ScenarioLevel, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter)))
			{
				foreach (Reward reward in item.Rewards)
				{
					if (reward.Type == ETreasureType.UnlockCharacter)
					{
						UnlockedCharacter(reward.CharacterID);
					}
				}
			}
		}
		foreach (CPartyAchievement item2 in list)
		{
			Achievements.Remove(item2);
		}
		foreach (CPartyAchievement achievement3 in Achievements)
		{
			achievement3.OnMapStateAdventureStarted();
		}
		lock (UnboundItems)
		{
			if (UnboundItems != null && UnboundItems.Count > 0)
			{
				foreach (CItem unboundItem in UnboundItems)
				{
					if (unboundItem != null && unboundItem.NetworkID == 0)
					{
						unboundItem.NetworkID = AdventureState.MapState.GetNextItemNetworkID();
						SimpleLog.AddToSimpleLog("[UNBOUND ITEMS] Updating NetworkID for " + unboundItem.Name + " to " + unboundItem.NetworkID);
					}
				}
			}
		}
		foreach (CMapCharacter checkCharacter2 in CheckCharacters)
		{
			checkCharacter2.OnMapStateAdventureStarted();
		}
		foreach (CCharacterClass @class in CharacterClassManager.Classes)
		{
			if (!ClassPersistentEnhancements.ContainsKey(@class.CharacterID))
			{
				ClassPersistentEnhancements.Add(@class.CharacterID, new List<CEnhancement>());
			}
		}
		if (CityEventDeck != null)
		{
			SimpleLog.AddToSimpleLog("City Event Deck Top 5 Cards: " + CityEventDeck.GetEventCardsOnTop(5));
		}
		if (RoadEventDeck != null)
		{
			SimpleLog.AddToSimpleLog("Road Event Deck Top 5 Cards: " + RoadEventDeck.GetEventCardsOnTop(5));
		}
	}

	public void ModifyPartyGold(int amount, bool useGoldModifier)
	{
		if (AdventureState.MapState.GoldMode == EGoldMode.PartyGold)
		{
			if (useGoldModifier && AdventureState.MapState.Difficulty.HasGoldModifier)
			{
				amount = (int)Math.Round((float)amount * AdventureState.MapState.Difficulty.GoldModifier);
			}
			PartyGold += amount;
			if (amount > 0)
			{
				AdventureState.MapState.MapParty.TotalEarnedGold += amount;
			}
			return;
		}
		throw new Exception("Unable to modify party gold if GoldMode is not EGoldMode.PartyGold");
	}

	public void RemoveSelectedCharacter(CMapCharacter character)
	{
		for (int i = 0; i < SelectedCharactersArray.Length; i++)
		{
			if (SelectedCharactersArray[i] != null && SelectedCharactersArray[i].CharacterID == character.CharacterID)
			{
				SelectedCharactersArray[i] = null;
			}
		}
	}

	public void AddSelectedCharacter(CMapCharacter character, int slot)
	{
		CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == character.CharacterID).ResetCards();
		character.ApplyEnhancements();
	}

	public int[] GetEmptyCharacterSlots()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < SelectedCharactersArray.Count(); i++)
		{
			if (SelectedCharactersArray[i] == null)
			{
				list.Add(i);
			}
		}
		return list.ToArray();
	}

	public void ResetItems()
	{
		foreach (CMapCharacter checkCharacter in CheckCharacters)
		{
			checkCharacter.ResetEquippedItems();
		}
	}

	public CMapCharacter IsItemEquippedByParty(CItem item)
	{
		foreach (CMapCharacter checkCharacter in CheckCharacters)
		{
			foreach (CItem checkEquippedItem in checkCharacter.CheckEquippedItems)
			{
				if (checkEquippedItem.ItemGuid == item.ItemGuid)
				{
					return checkCharacter;
				}
			}
		}
		return null;
	}

	public void UnequipItemIfEquippedByParty(CItem item)
	{
		foreach (CMapCharacter checkCharacter in CheckCharacters)
		{
			foreach (CItem item2 in checkCharacter.CheckEquippedItems.ToList())
			{
				if (item.ItemGuid == item2.ItemGuid)
				{
					DLLDebug.Log("Item " + item.Name + " has been unequipped from character " + checkCharacter.CharacterID);
					checkCharacter.UnequipItems(new List<CItem> { item });
				}
			}
		}
	}

	public List<PlayerState> ExportPlayerStates(ScenarioState state)
	{
		List<PlayerState> list = new List<PlayerState>();
		foreach (CMapCharacter character in SelectedCharacters)
		{
			CCharacterClass cCharacterClass = CharacterClassManager.Classes.Single((CCharacterClass s) => s.CharacterID == character.CharacterID);
			List<Tuple<int, int>> list2 = new List<Tuple<int, int>>();
			foreach (int cardID in character.HandAbilityCardIDs)
			{
				CAbilityCard cAbilityCard = cCharacterClass.AbilityCardsPool.Find((CAbilityCard x) => x.ID == cardID);
				list2.Add(new Tuple<int, int>(cAbilityCard.ID, cAbilityCard.ID));
			}
			double num = character.HealthTable[character.Level];
			if (AdventureState.MapState.Difficulty.HasHeroHealthModifier)
			{
				num *= (double)AdventureState.MapState.Difficulty.HeroHealthModifier;
				num = Math.Round(num);
			}
			if (AdventureState.MapState.Difficulty.HasBlessCards)
			{
				for (int num2 = 0; num2 < AdventureState.MapState.Difficulty.BlessCards; num2++)
				{
					character.PositiveConditions.Add(new PositiveConditionPair(CCondition.EPositiveCondition.Bless, RewardCondition.EConditionMapDuration.NextScenario, int.MaxValue, EConditionDecTrigger.Never));
				}
			}
			if (AdventureState.MapState.Difficulty.HasCurseCards)
			{
				for (int num3 = 0; num3 < AdventureState.MapState.Difficulty.CurseCards; num3++)
				{
					character.NegativeConditions.Add(new NegativeConditionPair(CCondition.ENegativeCondition.Curse, RewardCondition.EConditionMapDuration.NextScenario, int.MaxValue, EConditionDecTrigger.Never));
				}
			}
			PlayerState item = new PlayerState(character.CharacterID, 0, null, state.Maps[0].MapGuid, null, (int)num, (int)num, character.Level, new List<PositiveConditionPair>(), new List<NegativeConditionPair>(), playedThisRound: false, CActor.ECauseOfDeath.StillAlive, 1, character.CharacterName, 0, 0, isLongResting: false, new AbilityDeckState(list2), new AttackModifierDeckState(cCharacterClass), character.CheckEquippedItems)
			{
				IsRevealed = true
			};
			list.Add(item);
			SimpleLog.AddToSimpleLog("Creating PlayerState for MapCharacter of CharacterID: " + character.CharacterID + " and adding to ScenarioState");
		}
		return list;
	}

	public void AddItem(CItem newItem)
	{
		AddItemToUnboundItems(newItem);
		CItemAddedToParty_MapClientMessage message = new CItemAddedToParty_MapClientMessage(newItem);
		if (MapRuleLibraryClient.Instance?.MessageHandler != null)
		{
			MapRuleLibraryClient.Instance.MessageHandler(message);
		}
	}

	public void RemoveItem(CItem item, CMapCharacter character)
	{
		int slotIndex = item.SlotIndex;
		if (character != null)
		{
			if (character.CheckEquippedItems.Contains(item))
			{
				character.UnequipItems(new List<CItem> { item });
			}
			character.UnbindItem(item);
		}
		RemoveItemFromUnboundItems(item);
		CItemRemovedFromParty_MapClientMessage message = new CItemRemovedFromParty_MapClientMessage(item, slotIndex);
		if (MapRuleLibraryClient.Instance?.MessageHandler != null)
		{
			MapRuleLibraryClient.Instance.MessageHandler(message);
		}
	}

	public void AddItemToUnboundItems(CItem addItem)
	{
		lock (UnboundItems)
		{
			UnboundItems.Add(addItem);
		}
	}

	public void RemoveItemFromUnboundItems(CItem removeItem)
	{
		lock (UnboundItems)
		{
			UnboundItems.Remove(removeItem);
		}
	}

	public void ClearUnboundItems()
	{
		lock (UnboundItems)
		{
			UnboundItems.Clear();
		}
	}

	public void BindItem(string characterID, string characterName, CItem itemToBind, bool equip = false)
	{
		RemoveItemFromUnboundItems(itemToBind);
		CMapCharacter cMapCharacter = CheckCharacters.SingleOrDefault((CMapCharacter c) => c.CharacterID == characterID && c.CharacterName == characterName);
		if (cMapCharacter != null)
		{
			cMapCharacter.AddItemToBoundItems(itemToBind);
			CCharacterItemBound_MapClientMessage message = new CCharacterItemBound_MapClientMessage(cMapCharacter, itemToBind);
			if (MapRuleLibraryClient.Instance?.MessageHandler != null)
			{
				MapRuleLibraryClient.Instance.MessageHandler(message);
			}
			AdventureState.MapState.CheckLockedContent();
			if (equip && !cMapCharacter.ContainsEquipedItemWithId(itemToBind))
			{
				cMapCharacter.EquipItem(itemToBind);
			}
		}
		else
		{
			DLLDebug.LogError("Unable to find character " + characterID);
		}
	}

	public void AddCharacterToCharactersList(CMapCharacter addCharacter)
	{
		lock (Characters)
		{
			if (!Characters.Contains(addCharacter))
			{
				Characters.Add(addCharacter);
			}
		}
	}

	public void RemoveCharacterFromCharactersList(CMapCharacter removeCharacter)
	{
		lock (Characters)
		{
			Characters.Remove(removeCharacter);
		}
	}

	public int GetCurrentGlobalItemCount(CItem item)
	{
		List<CMapCharacter> checkCharacters = CheckCharacters;
		return CheckUnboundItems.Where((CItem w) => w.Name == item.Name).Count() + (from w in checkCharacters.SelectMany((CMapCharacter sm) => sm.CheckBoundItems)
			where w.Name == item.Name
			select w).Count() + (from w in checkCharacters.SelectMany((CMapCharacter sm2) => sm2.CheckEquippedItems)
			where w.Name == item.Name
			select w).Count() + AdventureState.MapState.HeadquartersState.CheckMerchantStock.Where((CItem w) => w.Name == item.Name).Count();
	}

	public List<CItem> GetAllSelectedPartyItems()
	{
		return CheckUnboundItems.Concat(SelectedCharacters.SelectMany((CMapCharacter x) => x.CheckBoundItems.Concat(x.CheckEquippedItems))).ToList();
	}

	public List<CItem> GetAllBoundPartyItems(CMapCharacter excludedCharacter = null)
	{
		return CheckCharacters.FindAll((CMapCharacter x) => x != excludedCharacter).SelectMany((CMapCharacter x) => x.CheckBoundItems.Concat(x.CheckEquippedItems)).ToList();
	}

	public List<CItem> GetAllPartyItems(bool includeMultiplayerItemReserve = false)
	{
		List<CItem> itemList = CheckUnboundItems.Concat(CheckCharacters.SelectMany((CMapCharacter x) => x.CheckBoundItems.Concat(x.CheckEquippedItems))).ToList();
		if (includeMultiplayerItemReserve)
		{
			IncorporateMultiplayerItemReserve(ref itemList);
		}
		return itemList;
	}

	public List<CItem> GetAllUnlockedItems(bool includeMultiplayerItemReserve = false)
	{
		List<CItem> itemList = AdventureState.MapState.HeadquartersState.CheckMerchantStock.Concat(CheckUnboundItems.Concat(CheckCharacters.SelectMany((CMapCharacter x) => x.CheckBoundItems.Concat(x.CheckEquippedItems)))).ToList();
		if (includeMultiplayerItemReserve)
		{
			IncorporateMultiplayerItemReserve(ref itemList);
		}
		return itemList;
	}

	private void IncorporateMultiplayerItemReserve(ref List<CItem> itemList)
	{
		foreach (CItem item in MultiplayerItemReserve)
		{
			if (!itemList.Exists((CItem x) => x.ItemGuid == item.ItemGuid))
			{
				itemList.Add(item);
			}
		}
	}

	public void UpdateProsperityXP(int prosperityXP)
	{
		if (ProsperityXP != prosperityXP)
		{
			int prosperityXP2 = ProsperityXP;
			int prosperityLevel = ProsperityLevel;
			if (prosperityXP < ProsperityXP)
			{
				int val = AdventureState.MapState.HeadquartersState.Headquarters.LevelToProsperity[ProsperityLevel - 1];
				prosperityXP = Math.Max(prosperityXP, val);
			}
			ProsperityXP = prosperityXP;
			if (ProsperityLevel != prosperityLevel && AdventureState.MapState.IsCampaign)
			{
				UnlockNewProsperityItems();
				AdventureState.MapState.HeadquartersState.EnhancementSlots = AdventureState.MapState.FindAppliedEnhancementSlotRewards() + (AdventureState.MapState.IsCampaign ? AdventureState.MapState.MapParty.ProsperityLevel : 0);
			}
			CProsperityChanged_MapClientMessage message = new CProsperityChanged_MapClientMessage(ProsperityXP, prosperityXP2, ProsperityLevel, prosperityLevel);
			if (MapRuleLibraryClient.Instance?.MessageHandler != null)
			{
				MapRuleLibraryClient.Instance.MessageHandler(message);
			}
		}
	}

	public void UpdateReputation(int reputation)
	{
		reputation = Math.Min(20, Math.Max(-20, reputation));
		if (Reputation != reputation)
		{
			Reputation = reputation;
			CReputationChanged_MapClientMessage message = new CReputationChanged_MapClientMessage(Reputation);
			if (MapRuleLibraryClient.Instance?.MessageHandler != null)
			{
				MapRuleLibraryClient.Instance.MessageHandler(message);
			}
		}
	}

	public void UnlockNewProsperityItems()
	{
		foreach (ItemCardYMLData itemCard in ScenarioRuleClient.SRLYML.ItemCards)
		{
			if (!itemCard.IsProsperityItem || itemCard.ProsperityRequirement > ProsperityLevel)
			{
				continue;
			}
			CItem getItem = itemCard.GetItem;
			while (GetCurrentGlobalItemCount(getItem) < AdventureState.MapState.HeadquartersState.Headquarters.GetMaxItemStock(getItem.YMLData.Rarity))
			{
				CItem cItem = getItem.Copy(AdventureState.MapState.GetGUIDBasedOnMapRNGState(), AdventureState.MapState.GetNextItemNetworkID());
				SimpleLog.AddToSimpleLog("(Campaign) (UnlockNewProsperityItems) Adding new item " + cItem.Name + " with NetworkID " + cItem.NetworkID);
				AdventureState.MapState.HeadquartersState.AddItemToMerchantStock(cItem);
				CContentUnlocked_MapClientMessage message = new CContentUnlocked_MapClientMessage(getItem.Name, "item_stock", null);
				if (MapRuleLibraryClient.Instance.MessageHandler != null)
				{
					MapRuleLibraryClient.Instance.MessageHandler(message);
				}
				else
				{
					DLLDebug.LogWarning("Message handler not set");
				}
			}
		}
	}

	public List<CItem> CalculateItemsUnlockedByProsperityLevel(int prosperityLevel)
	{
		List<CItem> list = new List<CItem>();
		foreach (ItemCardYMLData itemCard in ScenarioRuleClient.SRLYML.ItemCards)
		{
			if (itemCard.IsProsperityItem && itemCard.ProsperityRequirement == prosperityLevel)
			{
				CItem item = itemCard.GetItem;
				if (GetCurrentGlobalItemCount(item) < AdventureState.MapState.HeadquartersState.Headquarters.GetMaxItemStock(item.YMLData.Rarity) || AdventureState.MapState.HeadquartersState.CheckMerchantStock.Exists((CItem it) => it.ID == item.ID))
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	public List<Reward> CalculateRewardsUnlockedByProsperityLevel(int prosperityLevel)
	{
		List<Reward> list = CalculateItemsUnlockedByProsperityLevel(prosperityLevel).ConvertAll((CItem it) => new Reward(it.ID, 1, ETreasureType.UnlockProsperityItemStock, null, EGiveToCharacterType.None));
		list.Add(new Reward(ETreasureType.EnhancementSlots, 1, ETreasureDistributionType.PerMercenaryInParty, null));
		return list;
	}

	public void DeleteCharacter(CMapCharacter character)
	{
		if (Thread.CurrentThread != MapRuleLibraryClient.Instance.MessageThreadHandler)
		{
			MapRuleLibraryClient.Instance.AddQueueMessage(new CCharacterDeleted_MapDLLMessage(character), processImmediately: false);
			return;
		}
		foreach (CItem item in character.AllCharacterItems.ToList())
		{
			RemoveItem(item, character);
			if (item.Tradeable)
			{
				AdventureState.MapState.HeadquartersState.AddItemToMerchantStock(item);
			}
		}
		if (character.PersonalQuest != null)
		{
			AdventureState.MapState.MapParty.PersonalQuestDeck.AddCard(character.PersonalQuest.ID, CCardDeck.EAddCard.Any, allowDups: false);
			AdventureState.MapState.MapParty.PersonalQuestDeck.Shuffle(CCardDeck.EShuffle.DeckOnly);
		}
		RemoveSelectedCharacter(character);
		RemoveCharacterFromCharactersList(character);
	}

	public List<PersonalQuestYMLData> DrawPossiblePersonalQuests()
	{
		List<PersonalQuestYMLData> list = new List<PersonalQuestYMLData>();
		string personalQuestID;
		for (int i = 0; i < 2; i++)
		{
			if (AdventureState.MapState.DebugNextPersonalQuests.Count > i)
			{
				personalQuestID = AdventureState.MapState.DebugNextPersonalQuests[i].ID;
			}
			else
			{
				personalQuestID = PersonalQuestDeck.DrawCard(CCardDeck.EShuffle.All, CCardDeck.EDiscard.Bottom);
			}
			if (!string.IsNullOrEmpty(personalQuestID))
			{
				PersonalQuestYMLData personalQuestYMLData = MapRuleLibraryClient.MRLYML.PersonalQuests.SingleOrDefault((PersonalQuestYMLData e) => e.ID == personalQuestID);
				if (personalQuestYMLData != null)
				{
					list.Add(personalQuestYMLData);
				}
				else
				{
					DLLDebug.LogError("Couldn't draw a personal quest");
				}
			}
		}
		return list;
	}

	public bool UnlockedCharacter(string characterID)
	{
		if (UnlockedCharacterIDs.Contains(characterID))
		{
			return false;
		}
		UnlockedCharacterIDs.Add(characterID);
		AdventureState.MapState.CheckTrophyAchievements(new CUnlockClass_AchievementTrigger());
		if (AdventureState.MapState.IntroCompleted)
		{
			NewUnlockedCharacterIDs.Add(characterID);
			CCNewUnlockedClassesChanged_MapClientMessage message = new CCNewUnlockedClassesChanged_MapClientMessage(NewUnlockedCharacterIDs);
			if (MapRuleLibraryClient.Instance?.MessageHandler != null)
			{
				MapRuleLibraryClient.Instance.MessageHandler(message);
			}
		}
		return true;
	}

	public void RemoveNewUnlockedCharacter(string characterID)
	{
		if (NewUnlockedCharacterIDs.Remove(characterID))
		{
			CCNewUnlockedClassesChanged_MapClientMessage message = new CCNewUnlockedClassesChanged_MapClientMessage(NewUnlockedCharacterIDs);
			if (MapRuleLibraryClient.Instance?.MessageHandler != null)
			{
				MapRuleLibraryClient.Instance.MessageHandler(message);
			}
		}
	}

	public bool HasIntroduced(string id)
	{
		return IntroductionDoneIds.Contains(id);
	}

	public bool MarkIntroDone(string id)
	{
		return IntroductionDoneIds.Add(id);
	}

	public bool ExistsCharacterToRetire()
	{
		return SelectedCharacters.Any((CMapCharacter it) => it.PersonalQuest != null && it.PersonalQuest.IsFinished);
	}

	public bool CharacterPendingReward(string id)
	{
		return SelectedCharacters.Any((CMapCharacter it) => it.PersonalQuest != null && it.PersonalQuest.IsFinished && it.PersonalQuest.CurrentRewards != null && it.PersonalQuest.CurrentRewards.Any((RewardGroup cr) => cr.CharacterIDs != null && cr.CharacterIDs.Contains(id)));
	}
}
