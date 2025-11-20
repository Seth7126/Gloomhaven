using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Client;
using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;
using MapRuleLibrary.PhaseManager;
using MapRuleLibrary.Source.YML.VisibilitySpheres;
using MapRuleLibrary.YML.Achievements;
using MapRuleLibrary.YML.BattleGoals;
using MapRuleLibrary.YML.Events;
using MapRuleLibrary.YML.Locations;
using MapRuleLibrary.YML.Message;
using MapRuleLibrary.YML.PersonalQuests;
using MapRuleLibrary.YML.Quest;
using MapRuleLibrary.YML.Shared;
using Platforms.Activities;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using SharedLibrary;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;
using StateCodeGenerator;

namespace MapRuleLibrary.State;

[Serializable]
public class CMapState : ISerializable
{
	public class MapStateItemDuplicates
	{
		public string ItemGuid { get; private set; }

		public int NumberOfDuplicates { get; private set; }

		public MapStateItemDuplicates(string itemGuid, int numberOfDuplicates)
		{
			ItemGuid = itemGuid;
			NumberOfDuplicates = numberOfDuplicates;
		}
	}

	private SharedLibrary.Random m_MapRNG;

	private uint m_LastItemNetworkID;

	private CAdventureDifficulty m_Difficulty;

	public List<BattleGoalYMLData> DebugNextBattleGoals = new List<BattleGoalYMLData>();

	public List<PersonalQuestYMLData> DebugNextPersonalQuests = new List<PersonalQuestYMLData>();

	public volatile bool AdventureStateStarting;

	public string SoloID = "";

	public const int m_MaxJobQuestsAvailable = 3;

	public const int m_MaxQuestsRemainingUntilTimeout = 5;

	public const float m_PreviousJobQuestsVillagePercentage = 0.25f;

	private bool m_PlayingTutorial;

	private bool m_ForceRegenerate;

	public ScenarioManager.EDLLMode DLLMode { get; private set; }

	public DLCRegistry.EDLCKey DLCEnabled { get; private set; }

	public CMapParty MapParty { get; private set; }

	public CMapPhase CurrentMapPhase { get; private set; }

	public List<CChapter> UnlockedChapters { get; private set; }

	public bool TutorialCompleted { get; set; }

	public bool IntroCompleted { get; set; }

	public bool MapFTUECompleted { get; set; } = true;

	public EAdventureDifficulty DifficultySetting { get; private set; }

	public StateShared.EHouseRulesFlag HouseRulesSetting { get; private set; }

	public CHeadquartersState HeadquartersState { get; private set; }

	public CTempleState TempleState { get; private set; }

	public List<CVillageState> VillageStates { get; private set; }

	public List<CMapMessageState> MapMessageStates { get; private set; }

	public List<CVisibilitySphereState> VisibilitySphereStates { get; private set; }

	public List<CJobQuestState> JobQuestStates { get; private set; }

	public List<CJobQuestState> CurrentJobQuestStates { get; private set; }

	public List<CJobQuestState> PreviousJobQuestStates { get; private set; }

	public bool IsModded { get; private set; }

	public List<string> UsedEventNames { get; private set; }

	public int MinRequiredCharacters { get; set; }

	public bool CanDrawCityEvent { get; set; }

	public RewardGroup StartupRewardGroup { get; protected set; }

	public Extensions.RandomState StartupTreasureTablesRewardGenerationRNGState { get; private set; }

	public List<string> QueuedUnlockedQuestIDs { get; private set; }

	public List<string> QueuedCompletedQuestIDs { get; private set; }

	public List<Reward> QueuedCompletionRewards { get; private set; }

	public List<string> QueuedPlatformAchievementsToUnlock { get; private set; }

	public List<string> AlreadyRewardedChestTreasureTableIDs { get; private set; }

	public Dictionary<string, CLevelMessage> ScenarioStartMessages { get; private set; }

	public Dictionary<string, CLevelMessage> ScenarioCompleteMessages { get; private set; }

	public Dictionary<string, List<CLevelMessage>> ScenarioRoomRevealMessages { get; private set; }

	public bool IsInitialised { get; private set; }

	public bool IsCampaign => DLLMode == ScenarioManager.EDLLMode.Campaign;

	public int Seed { get; private set; }

	public Extensions.RandomState MapRNGState { get; private set; }

	public SharedLibrary.Random MapRNG
	{
		get
		{
			return m_MapRNG;
		}
		private set
		{
			m_MapRNG = value;
		}
	}

	public int PeekMapRNG => m_MapRNG.Save().Restore().Next();

	public uint LastItemNetworkID => m_LastItemNetworkID;

	public EGoldMode GoldMode { get; private set; }

	public EEnhancementMode EnhancementMode { get; private set; }

	public MapStateSaveOwner SaveOwner { get; private set; }

	public CCustomLevelData CurrentSingleScenario { get; set; }

	public CAdventureDifficulty Difficulty
	{
		get
		{
			if (m_Difficulty == null)
			{
				m_Difficulty = GetDifficulty();
			}
			return m_Difficulty;
		}
	}

	public bool IsPlayingTutorial => m_PlayingTutorial;

	public bool ForceRegenerate => m_ForceRegenerate;

	public EMapPhaseType CurrentMapPhaseType
	{
		get
		{
			if (CurrentMapPhase != null)
			{
				return CurrentMapPhase.Type;
			}
			return EMapPhaseType.None;
		}
	}

	public bool IsInScenarioPhase
	{
		get
		{
			if (CurrentMapPhaseType != EMapPhaseType.InScenario)
			{
				return CurrentMapPhaseType == EMapPhaseType.InScenarioDebugMode;
			}
			return true;
		}
	}

	public int MaxPreviousJobs => (int)Math.Round((float)AllUnlockedVillages.Count * 0.25f);

	public CLocationState JustCompletedLocationState { get; set; }

	public ActivitiesProgressData ActivitiesProgressData { get; set; }

	public List<int> CampaignCompleteActivityQuestsIds { get; private set; } = new List<int>();

	public Dictionary<string, ScenarioState> ScenarioStates { get; private set; }

	public List<CLocationState> AllLocations
	{
		get
		{
			List<CLocationState> list = new List<CLocationState>();
			list.Add(HeadquartersState);
			list.AddRange(HeadquartersState.QuestStates.Select((CQuestState s) => s.ScenarioState).ToList());
			list.AddRange(VillageStates);
			list.AddRange(VillageStates.SelectMany((CVillageState sm) => sm.QuestStates.Select((CQuestState s) => s.ScenarioState)).ToList());
			list.AddRange(CurrentJobQuestStates.Select((CJobQuestState s) => s.ScenarioState).ToList());
			return list;
		}
	}

	public List<CLocationState> AllVillages
	{
		get
		{
			List<CLocationState> list = new List<CLocationState>();
			list.Add(HeadquartersState);
			list.AddRange(VillageStates);
			return list;
		}
	}

	public List<CLocationState> AllUnlockedVillages
	{
		get
		{
			List<CLocationState> list = new List<CLocationState>();
			list.Add(HeadquartersState);
			list.AddRange(VillageStates.Where((CVillageState sm) => sm.LocationState == ELocationState.Unlocked));
			return list;
		}
	}

	public List<CLocationState> AllUnlockedVillagesWithNoJob
	{
		get
		{
			List<CLocationState> list = new List<CLocationState>();
			if (!CurrentJobQuestStates.Any((CJobQuestState q) => q.JobVillageID == HeadquartersState.Headquarters.ID && HeadquartersState.Headquarters.JobMapLocations.Count > 0))
			{
				list.Add(HeadquartersState);
			}
			foreach (CVillageState villageState in VillageStates.Where((CVillageState sm) => sm.LocationState == ELocationState.Unlocked && sm.Village.JobMapLocations.Count > 0))
			{
				if (!CurrentJobQuestStates.Any((CJobQuestState q) => q.JobVillageID == villageState.Village.ID))
				{
					list.Add(villageState);
				}
			}
			return list;
		}
	}

	public List<CMapScenarioState> AllScenarios
	{
		get
		{
			List<CMapScenarioState> list = new List<CMapScenarioState>();
			list.AddRange(HeadquartersState.QuestStates.Select((CQuestState s) => s.ScenarioState).ToList());
			list.AddRange(VillageStates.SelectMany((CVillageState sm) => sm.QuestStates.Select((CQuestState s) => s.ScenarioState)).ToList());
			list.AddRange(CurrentJobQuestStates.Select((CJobQuestState s) => s.ScenarioState).ToList());
			return list;
		}
	}

	public List<CQuestState> AllQuests
	{
		get
		{
			List<CQuestState> list = new List<CQuestState>();
			list.AddRange(HeadquartersState.QuestStates);
			list.AddRange(VillageStates.SelectMany((CVillageState sm) => sm.QuestStates));
			list.AddRange(CurrentJobQuestStates);
			return list;
		}
	}

	public List<CQuestState> AllQuestStates
	{
		get
		{
			List<CQuestState> list = new List<CQuestState>();
			list.AddRange(HeadquartersState.QuestStates);
			list.AddRange(VillageStates.SelectMany((CVillageState sm) => sm.QuestStates));
			list.AddRange(JobQuestStates);
			return list;
		}
	}

	public List<CQuestState> AllIncompleteQuests
	{
		get
		{
			List<CQuestState> list = new List<CQuestState>();
			list.AddRange(HeadquartersState.QuestStates.Where((CQuestState q) => q.QuestState != CQuestState.EQuestState.Completed).ToList());
			list.AddRange(VillageStates.SelectMany((CVillageState sm) => sm.QuestStates.Where((CQuestState q) => q.QuestState != CQuestState.EQuestState.Completed).ToList()));
			list.AddRange(CurrentJobQuestStates);
			return list;
		}
	}

	public List<CQuestState> AllLockedQuests
	{
		get
		{
			List<CQuestState> list = new List<CQuestState>();
			list.AddRange(HeadquartersState.QuestStates);
			list.AddRange(VillageStates.SelectMany((CVillageState sm) => sm.QuestStates));
			list.AddRange(JobQuestStates);
			return list.Where((CQuestState q) => q.QuestState == CQuestState.EQuestState.Locked).ToList();
		}
	}

	public List<CQuestState> AllUnlockedQuests
	{
		get
		{
			List<CQuestState> list = new List<CQuestState>();
			list.AddRange(HeadquartersState.QuestStates);
			list.AddRange(VillageStates.SelectMany((CVillageState sm) => sm.QuestStates));
			list.AddRange(JobQuestStates);
			return list.Where((CQuestState q) => q.QuestState == CQuestState.EQuestState.Unlocked).ToList();
		}
	}

	public List<CQuestState> AllBlockedQuests
	{
		get
		{
			List<CQuestState> list = new List<CQuestState>();
			list.AddRange(HeadquartersState.QuestStates);
			list.AddRange(VillageStates.SelectMany((CVillageState sm) => sm.QuestStates));
			list.AddRange(JobQuestStates);
			return list.Where((CQuestState q) => q.QuestState == CQuestState.EQuestState.Blocked).ToList();
		}
	}

	public List<CQuestState> AllCompletedQuests
	{
		get
		{
			List<CQuestState> list = new List<CQuestState>();
			list.AddRange(HeadquartersState.QuestStates.Where((CQuestState q) => q.QuestState == CQuestState.EQuestState.Completed).ToList());
			list.AddRange(VillageStates.SelectMany((CVillageState sm) => sm.QuestStates.Where((CQuestState q) => q.QuestState == CQuestState.EQuestState.Completed).ToList()));
			return list;
		}
	}

	public List<CJobQuestState> AllUnlockedJobQuestStates
	{
		get
		{
			List<CJobQuestState> list = new List<CJobQuestState>();
			list.AddRange(JobQuestStates.FindAll((CJobQuestState q) => q.QuestState == CQuestState.EQuestState.Unlocked));
			return list;
		}
	}

	public List<CJobQuestState> AllAvailableUnlockedJobQuestStates
	{
		get
		{
			List<CJobQuestState> list = new List<CJobQuestState>();
			list.AddRange(JobQuestStates.FindAll((CJobQuestState q) => q.QuestState == CQuestState.EQuestState.Unlocked && !AdventureState.MapState.CurrentJobQuestStates.Contains(q) && !AdventureState.MapState.PreviousJobQuestStates.Contains(q)));
			return list;
		}
	}

	public List<CQuestState> AllTravelQuests
	{
		get
		{
			List<CQuestState> list = new List<CQuestState>();
			list.AddRange(HeadquartersState.QuestStates);
			list.AddRange(VillageStates.SelectMany((CVillageState sm) => sm.QuestStates));
			return list.Where((CQuestState q) => q.Quest.Type == EQuestType.Travel).ToList();
		}
	}

	public CQuestState InProgressQuestState => AllIncompleteQuests.SingleOrDefault((CQuestState q) => q.InProgress);

	public CMapScenarioState CurrentMapScenarioState
	{
		get
		{
			if (!m_PlayingTutorial)
			{
				if (InProgressQuestState == null)
				{
					return null;
				}
				return InProgressQuestState.ScenarioState;
			}
			return HeadquartersState.StartingScenarioStates[HeadquartersState.CurrentStartingScenarioIndex];
		}
	}

	public List<CItem> GetAllUnlockedItems => HeadquartersState.CheckMerchantStock.Concat(MapParty.CheckUnboundItems.Concat(MapParty.CheckCharacters.SelectMany((CMapCharacter x) => x.CheckBoundItems.Concat(x.CheckEquippedItems)))).ToList();

	public int HighestUnlockedChapter => UnlockedChapters.Select((CChapter s) => s.Chapter).Max();

	public CMapState()
	{
	}

	public CMapState(CMapState state, ReferenceDictionary references)
	{
		DLLMode = state.DLLMode;
		DLCEnabled = state.DLCEnabled;
		MapParty = references.Get(state.MapParty);
		if (MapParty == null && state.MapParty != null)
		{
			MapParty = new CMapParty(state.MapParty, references);
			references.Add(state.MapParty, MapParty);
		}
		CurrentMapPhase = references.Get(state.CurrentMapPhase);
		if (CurrentMapPhase == null && state.CurrentMapPhase != null)
		{
			CurrentMapPhase = new CMapPhase(state.CurrentMapPhase, references);
			references.Add(state.CurrentMapPhase, CurrentMapPhase);
		}
		UnlockedChapters = references.Get(state.UnlockedChapters);
		if (UnlockedChapters == null && state.UnlockedChapters != null)
		{
			UnlockedChapters = new List<CChapter>();
			for (int i = 0; i < state.UnlockedChapters.Count; i++)
			{
				CChapter cChapter = state.UnlockedChapters[i];
				CChapter cChapter2 = references.Get(cChapter);
				if (cChapter2 == null && cChapter != null)
				{
					cChapter2 = new CChapter(cChapter, references);
					references.Add(cChapter, cChapter2);
				}
				UnlockedChapters.Add(cChapter2);
			}
			references.Add(state.UnlockedChapters, UnlockedChapters);
		}
		TutorialCompleted = state.TutorialCompleted;
		IntroCompleted = state.IntroCompleted;
		MapFTUECompleted = state.MapFTUECompleted;
		DifficultySetting = state.DifficultySetting;
		HouseRulesSetting = state.HouseRulesSetting;
		HeadquartersState = references.Get(state.HeadquartersState);
		if (HeadquartersState == null && state.HeadquartersState != null)
		{
			HeadquartersState = new CHeadquartersState(state.HeadquartersState, references);
			references.Add(state.HeadquartersState, HeadquartersState);
		}
		TempleState = references.Get(state.TempleState);
		if (TempleState == null && state.TempleState != null)
		{
			TempleState = new CTempleState(state.TempleState, references);
			references.Add(state.TempleState, TempleState);
		}
		VillageStates = references.Get(state.VillageStates);
		if (VillageStates == null && state.VillageStates != null)
		{
			VillageStates = new List<CVillageState>();
			for (int j = 0; j < state.VillageStates.Count; j++)
			{
				CVillageState cVillageState = state.VillageStates[j];
				CVillageState cVillageState2 = references.Get(cVillageState);
				if (cVillageState2 == null && cVillageState != null)
				{
					cVillageState2 = new CVillageState(cVillageState, references);
					references.Add(cVillageState, cVillageState2);
				}
				VillageStates.Add(cVillageState2);
			}
			references.Add(state.VillageStates, VillageStates);
		}
		MapMessageStates = references.Get(state.MapMessageStates);
		if (MapMessageStates == null && state.MapMessageStates != null)
		{
			MapMessageStates = new List<CMapMessageState>();
			for (int k = 0; k < state.MapMessageStates.Count; k++)
			{
				CMapMessageState cMapMessageState = state.MapMessageStates[k];
				CMapMessageState cMapMessageState2 = references.Get(cMapMessageState);
				if (cMapMessageState2 == null && cMapMessageState != null)
				{
					cMapMessageState2 = new CMapMessageState(cMapMessageState, references);
					references.Add(cMapMessageState, cMapMessageState2);
				}
				MapMessageStates.Add(cMapMessageState2);
			}
			references.Add(state.MapMessageStates, MapMessageStates);
		}
		VisibilitySphereStates = references.Get(state.VisibilitySphereStates);
		if (VisibilitySphereStates == null && state.VisibilitySphereStates != null)
		{
			VisibilitySphereStates = new List<CVisibilitySphereState>();
			for (int l = 0; l < state.VisibilitySphereStates.Count; l++)
			{
				CVisibilitySphereState cVisibilitySphereState = state.VisibilitySphereStates[l];
				CVisibilitySphereState cVisibilitySphereState2 = references.Get(cVisibilitySphereState);
				if (cVisibilitySphereState2 == null && cVisibilitySphereState != null)
				{
					cVisibilitySphereState2 = new CVisibilitySphereState(cVisibilitySphereState, references);
					references.Add(cVisibilitySphereState, cVisibilitySphereState2);
				}
				VisibilitySphereStates.Add(cVisibilitySphereState2);
			}
			references.Add(state.VisibilitySphereStates, VisibilitySphereStates);
		}
		JobQuestStates = references.Get(state.JobQuestStates);
		if (JobQuestStates == null && state.JobQuestStates != null)
		{
			JobQuestStates = new List<CJobQuestState>();
			for (int m = 0; m < state.JobQuestStates.Count; m++)
			{
				CJobQuestState cJobQuestState = state.JobQuestStates[m];
				CJobQuestState cJobQuestState2 = references.Get(cJobQuestState);
				if (cJobQuestState2 == null && cJobQuestState != null)
				{
					cJobQuestState2 = new CJobQuestState(cJobQuestState, references);
					references.Add(cJobQuestState, cJobQuestState2);
				}
				JobQuestStates.Add(cJobQuestState2);
			}
			references.Add(state.JobQuestStates, JobQuestStates);
		}
		CurrentJobQuestStates = references.Get(state.CurrentJobQuestStates);
		if (CurrentJobQuestStates == null && state.CurrentJobQuestStates != null)
		{
			CurrentJobQuestStates = new List<CJobQuestState>();
			for (int n = 0; n < state.CurrentJobQuestStates.Count; n++)
			{
				CJobQuestState cJobQuestState3 = state.CurrentJobQuestStates[n];
				CJobQuestState cJobQuestState4 = references.Get(cJobQuestState3);
				if (cJobQuestState4 == null && cJobQuestState3 != null)
				{
					cJobQuestState4 = new CJobQuestState(cJobQuestState3, references);
					references.Add(cJobQuestState3, cJobQuestState4);
				}
				CurrentJobQuestStates.Add(cJobQuestState4);
			}
			references.Add(state.CurrentJobQuestStates, CurrentJobQuestStates);
		}
		PreviousJobQuestStates = references.Get(state.PreviousJobQuestStates);
		if (PreviousJobQuestStates == null && state.PreviousJobQuestStates != null)
		{
			PreviousJobQuestStates = new List<CJobQuestState>();
			for (int num = 0; num < state.PreviousJobQuestStates.Count; num++)
			{
				CJobQuestState cJobQuestState5 = state.PreviousJobQuestStates[num];
				CJobQuestState cJobQuestState6 = references.Get(cJobQuestState5);
				if (cJobQuestState6 == null && cJobQuestState5 != null)
				{
					cJobQuestState6 = new CJobQuestState(cJobQuestState5, references);
					references.Add(cJobQuestState5, cJobQuestState6);
				}
				PreviousJobQuestStates.Add(cJobQuestState6);
			}
			references.Add(state.PreviousJobQuestStates, PreviousJobQuestStates);
		}
		IsModded = state.IsModded;
		UsedEventNames = references.Get(state.UsedEventNames);
		if (UsedEventNames == null && state.UsedEventNames != null)
		{
			UsedEventNames = new List<string>();
			for (int num2 = 0; num2 < state.UsedEventNames.Count; num2++)
			{
				string item = state.UsedEventNames[num2];
				UsedEventNames.Add(item);
			}
			references.Add(state.UsedEventNames, UsedEventNames);
		}
		MinRequiredCharacters = state.MinRequiredCharacters;
		CanDrawCityEvent = state.CanDrawCityEvent;
		StartupRewardGroup = references.Get(state.StartupRewardGroup);
		if (StartupRewardGroup == null && state.StartupRewardGroup != null)
		{
			StartupRewardGroup = new RewardGroup(state.StartupRewardGroup, references);
			references.Add(state.StartupRewardGroup, StartupRewardGroup);
		}
		StartupTreasureTablesRewardGenerationRNGState = references.Get(state.StartupTreasureTablesRewardGenerationRNGState);
		if (StartupTreasureTablesRewardGenerationRNGState == null && state.StartupTreasureTablesRewardGenerationRNGState != null)
		{
			StartupTreasureTablesRewardGenerationRNGState = new Extensions.RandomState(state.StartupTreasureTablesRewardGenerationRNGState, references);
			references.Add(state.StartupTreasureTablesRewardGenerationRNGState, StartupTreasureTablesRewardGenerationRNGState);
		}
		QueuedUnlockedQuestIDs = references.Get(state.QueuedUnlockedQuestIDs);
		if (QueuedUnlockedQuestIDs == null && state.QueuedUnlockedQuestIDs != null)
		{
			QueuedUnlockedQuestIDs = new List<string>();
			for (int num3 = 0; num3 < state.QueuedUnlockedQuestIDs.Count; num3++)
			{
				string item2 = state.QueuedUnlockedQuestIDs[num3];
				QueuedUnlockedQuestIDs.Add(item2);
			}
			references.Add(state.QueuedUnlockedQuestIDs, QueuedUnlockedQuestIDs);
		}
		QueuedCompletedQuestIDs = references.Get(state.QueuedCompletedQuestIDs);
		if (QueuedCompletedQuestIDs == null && state.QueuedCompletedQuestIDs != null)
		{
			QueuedCompletedQuestIDs = new List<string>();
			for (int num4 = 0; num4 < state.QueuedCompletedQuestIDs.Count; num4++)
			{
				string item3 = state.QueuedCompletedQuestIDs[num4];
				QueuedCompletedQuestIDs.Add(item3);
			}
			references.Add(state.QueuedCompletedQuestIDs, QueuedCompletedQuestIDs);
		}
		QueuedCompletionRewards = references.Get(state.QueuedCompletionRewards);
		if (QueuedCompletionRewards == null && state.QueuedCompletionRewards != null)
		{
			QueuedCompletionRewards = new List<Reward>();
			for (int num5 = 0; num5 < state.QueuedCompletionRewards.Count; num5++)
			{
				Reward reward = state.QueuedCompletionRewards[num5];
				Reward reward2 = references.Get(reward);
				if (reward2 == null && reward != null)
				{
					reward2 = new Reward(reward, references);
					references.Add(reward, reward2);
				}
				QueuedCompletionRewards.Add(reward2);
			}
			references.Add(state.QueuedCompletionRewards, QueuedCompletionRewards);
		}
		QueuedPlatformAchievementsToUnlock = references.Get(state.QueuedPlatformAchievementsToUnlock);
		if (QueuedPlatformAchievementsToUnlock == null && state.QueuedPlatformAchievementsToUnlock != null)
		{
			QueuedPlatformAchievementsToUnlock = new List<string>();
			for (int num6 = 0; num6 < state.QueuedPlatformAchievementsToUnlock.Count; num6++)
			{
				string item4 = state.QueuedPlatformAchievementsToUnlock[num6];
				QueuedPlatformAchievementsToUnlock.Add(item4);
			}
			references.Add(state.QueuedPlatformAchievementsToUnlock, QueuedPlatformAchievementsToUnlock);
		}
		AlreadyRewardedChestTreasureTableIDs = references.Get(state.AlreadyRewardedChestTreasureTableIDs);
		if (AlreadyRewardedChestTreasureTableIDs == null && state.AlreadyRewardedChestTreasureTableIDs != null)
		{
			AlreadyRewardedChestTreasureTableIDs = new List<string>();
			for (int num7 = 0; num7 < state.AlreadyRewardedChestTreasureTableIDs.Count; num7++)
			{
				string item5 = state.AlreadyRewardedChestTreasureTableIDs[num7];
				AlreadyRewardedChestTreasureTableIDs.Add(item5);
			}
			references.Add(state.AlreadyRewardedChestTreasureTableIDs, AlreadyRewardedChestTreasureTableIDs);
		}
		ScenarioStartMessages = references.Get(state.ScenarioStartMessages);
		if (ScenarioStartMessages == null && state.ScenarioStartMessages != null)
		{
			ScenarioStartMessages = new Dictionary<string, CLevelMessage>(state.ScenarioStartMessages.Comparer);
			foreach (KeyValuePair<string, CLevelMessage> scenarioStartMessage in state.ScenarioStartMessages)
			{
				string key = scenarioStartMessage.Key;
				CLevelMessage cLevelMessage = references.Get(scenarioStartMessage.Value);
				if (cLevelMessage == null && scenarioStartMessage.Value != null)
				{
					cLevelMessage = new CLevelMessage(scenarioStartMessage.Value, references);
					references.Add(scenarioStartMessage.Value, cLevelMessage);
				}
				ScenarioStartMessages.Add(key, cLevelMessage);
			}
			references.Add(state.ScenarioStartMessages, ScenarioStartMessages);
		}
		ScenarioCompleteMessages = references.Get(state.ScenarioCompleteMessages);
		if (ScenarioCompleteMessages == null && state.ScenarioCompleteMessages != null)
		{
			ScenarioCompleteMessages = new Dictionary<string, CLevelMessage>(state.ScenarioCompleteMessages.Comparer);
			foreach (KeyValuePair<string, CLevelMessage> scenarioCompleteMessage in state.ScenarioCompleteMessages)
			{
				string key2 = scenarioCompleteMessage.Key;
				CLevelMessage cLevelMessage2 = references.Get(scenarioCompleteMessage.Value);
				if (cLevelMessage2 == null && scenarioCompleteMessage.Value != null)
				{
					cLevelMessage2 = new CLevelMessage(scenarioCompleteMessage.Value, references);
					references.Add(scenarioCompleteMessage.Value, cLevelMessage2);
				}
				ScenarioCompleteMessages.Add(key2, cLevelMessage2);
			}
			references.Add(state.ScenarioCompleteMessages, ScenarioCompleteMessages);
		}
		ScenarioRoomRevealMessages = references.Get(state.ScenarioRoomRevealMessages);
		if (ScenarioRoomRevealMessages == null && state.ScenarioRoomRevealMessages != null)
		{
			ScenarioRoomRevealMessages = new Dictionary<string, List<CLevelMessage>>(state.ScenarioRoomRevealMessages.Comparer);
			foreach (KeyValuePair<string, List<CLevelMessage>> scenarioRoomRevealMessage in state.ScenarioRoomRevealMessages)
			{
				string key3 = scenarioRoomRevealMessage.Key;
				List<CLevelMessage> list = references.Get(scenarioRoomRevealMessage.Value);
				if (list == null && scenarioRoomRevealMessage.Value != null)
				{
					list = new List<CLevelMessage>();
					for (int num8 = 0; num8 < scenarioRoomRevealMessage.Value.Count; num8++)
					{
						CLevelMessage cLevelMessage3 = scenarioRoomRevealMessage.Value[num8];
						CLevelMessage cLevelMessage4 = references.Get(cLevelMessage3);
						if (cLevelMessage4 == null && cLevelMessage3 != null)
						{
							cLevelMessage4 = new CLevelMessage(cLevelMessage3, references);
							references.Add(cLevelMessage3, cLevelMessage4);
						}
						list.Add(cLevelMessage4);
					}
					references.Add(scenarioRoomRevealMessage.Value, list);
				}
				ScenarioRoomRevealMessages.Add(key3, list);
			}
			references.Add(state.ScenarioRoomRevealMessages, ScenarioRoomRevealMessages);
		}
		IsInitialised = state.IsInitialised;
		Seed = state.Seed;
		MapRNGState = references.Get(state.MapRNGState);
		if (MapRNGState == null && state.MapRNGState != null)
		{
			MapRNGState = new Extensions.RandomState(state.MapRNGState, references);
			references.Add(state.MapRNGState, MapRNGState);
		}
		m_MapRNG = references.Get(state.m_MapRNG);
		if (m_MapRNG == null && state.m_MapRNG != null)
		{
			m_MapRNG = new SharedLibrary.Random(state.m_MapRNG, references);
			references.Add(state.m_MapRNG, m_MapRNG);
		}
		m_LastItemNetworkID = state.m_LastItemNetworkID;
		GoldMode = state.GoldMode;
		EnhancementMode = state.EnhancementMode;
		SaveOwner = references.Get(state.SaveOwner);
		if (SaveOwner == null && state.SaveOwner != null)
		{
			SaveOwner = new MapStateSaveOwner(state.SaveOwner, references);
			references.Add(state.SaveOwner, SaveOwner);
		}
		CurrentSingleScenario = references.Get(state.CurrentSingleScenario);
		if (CurrentSingleScenario == null && state.CurrentSingleScenario != null)
		{
			CurrentSingleScenario = new CCustomLevelData(state.CurrentSingleScenario, references);
			references.Add(state.CurrentSingleScenario, CurrentSingleScenario);
		}
		m_Difficulty = references.Get(state.m_Difficulty);
		if (m_Difficulty == null && state.m_Difficulty != null)
		{
			m_Difficulty = new CAdventureDifficulty(state.m_Difficulty, references);
			references.Add(state.m_Difficulty, m_Difficulty);
		}
		JustCompletedLocationState = references.Get(state.JustCompletedLocationState);
		if (JustCompletedLocationState == null && state.JustCompletedLocationState != null)
		{
			JustCompletedLocationState = new CLocationState(state.JustCompletedLocationState, references);
			references.Add(state.JustCompletedLocationState, JustCompletedLocationState);
		}
		ActivitiesProgressData = state.ActivitiesProgressData.Clone();
		CampaignCompleteActivityQuestsIds = references.Get(state.CampaignCompleteActivityQuestsIds);
		if (CampaignCompleteActivityQuestsIds == null && state.CampaignCompleteActivityQuestsIds != null)
		{
			CampaignCompleteActivityQuestsIds = new List<int>();
			for (int num9 = 0; num9 < state.CampaignCompleteActivityQuestsIds.Count; num9++)
			{
				int item6 = state.CampaignCompleteActivityQuestsIds[num9];
				CampaignCompleteActivityQuestsIds.Add(item6);
			}
			references.Add(state.CampaignCompleteActivityQuestsIds, CampaignCompleteActivityQuestsIds);
		}
		DebugNextBattleGoals = references.Get(state.DebugNextBattleGoals);
		if (DebugNextBattleGoals == null && state.DebugNextBattleGoals != null)
		{
			DebugNextBattleGoals = new List<BattleGoalYMLData>();
			for (int num10 = 0; num10 < state.DebugNextBattleGoals.Count; num10++)
			{
				BattleGoalYMLData battleGoalYMLData = state.DebugNextBattleGoals[num10];
				BattleGoalYMLData battleGoalYMLData2 = references.Get(battleGoalYMLData);
				if (battleGoalYMLData2 == null && battleGoalYMLData != null)
				{
					battleGoalYMLData2 = new BattleGoalYMLData(battleGoalYMLData, references);
					references.Add(battleGoalYMLData, battleGoalYMLData2);
				}
				DebugNextBattleGoals.Add(battleGoalYMLData2);
			}
			references.Add(state.DebugNextBattleGoals, DebugNextBattleGoals);
		}
		DebugNextPersonalQuests = references.Get(state.DebugNextPersonalQuests);
		if (DebugNextPersonalQuests == null && state.DebugNextPersonalQuests != null)
		{
			DebugNextPersonalQuests = new List<PersonalQuestYMLData>();
			for (int num11 = 0; num11 < state.DebugNextPersonalQuests.Count; num11++)
			{
				PersonalQuestYMLData personalQuestYMLData = state.DebugNextPersonalQuests[num11];
				PersonalQuestYMLData personalQuestYMLData2 = references.Get(personalQuestYMLData);
				if (personalQuestYMLData2 == null && personalQuestYMLData != null)
				{
					personalQuestYMLData2 = new PersonalQuestYMLData(personalQuestYMLData, references);
					references.Add(personalQuestYMLData, personalQuestYMLData2);
				}
				DebugNextPersonalQuests.Add(personalQuestYMLData2);
			}
			references.Add(state.DebugNextPersonalQuests, DebugNextPersonalQuests);
		}
		AdventureStateStarting = state.AdventureStateStarting;
		SoloID = state.SoloID;
		ScenarioStates = references.Get(state.ScenarioStates);
		if (ScenarioStates == null && state.ScenarioStates != null)
		{
			ScenarioStates = new Dictionary<string, ScenarioState>(state.ScenarioStates.Comparer);
			foreach (KeyValuePair<string, ScenarioState> scenarioState2 in state.ScenarioStates)
			{
				string key4 = scenarioState2.Key;
				ScenarioState scenarioState = references.Get(scenarioState2.Value);
				if (scenarioState == null && scenarioState2.Value != null)
				{
					scenarioState = new ScenarioState(scenarioState2.Value, references);
					references.Add(scenarioState2.Value, scenarioState);
				}
				ScenarioStates.Add(key4, scenarioState);
			}
			references.Add(state.ScenarioStates, ScenarioStates);
		}
		m_PlayingTutorial = state.m_PlayingTutorial;
		m_ForceRegenerate = state.m_ForceRegenerate;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("DLLMode", DLLMode);
		info.AddValue("DLCEnabled", DLCEnabled);
		info.AddValue("MapParty", MapParty);
		info.AddValue("CurrentMapPhase", CurrentMapPhase);
		info.AddValue("UnlockedChapters", UnlockedChapters);
		info.AddValue("TutorialCompleted", TutorialCompleted);
		info.AddValue("IntroCompleted", IntroCompleted);
		info.AddValue("DifficultySetting", DifficultySetting);
		info.AddValue("HouseRulesSetting", HouseRulesSetting);
		info.AddValue("HeadquartersState", HeadquartersState);
		info.AddValue("TempleState", TempleState);
		info.AddValue("VillageStates", VillageStates);
		info.AddValue("MapMessageStates", MapMessageStates);
		info.AddValue("VisibilitySphereStates", VisibilitySphereStates);
		info.AddValue("JobQuestStates", JobQuestStates);
		info.AddValue("CurrentJobQuestStates", CurrentJobQuestStates);
		info.AddValue("PreviousJobQuestStates", PreviousJobQuestStates);
		info.AddValue("IsModded", IsModded);
		info.AddValue("IsInitialised", IsInitialised);
		info.AddValue("Seed", Seed);
		info.AddValue("UsedEventNames", UsedEventNames);
		info.AddValue("CanDrawCityEvent", CanDrawCityEvent);
		info.AddValue("StartupRewardGroup", StartupRewardGroup);
		info.AddValue("StartupTreasureTablesRewardGenerationRNGState", StartupTreasureTablesRewardGenerationRNGState);
		info.AddValue("QueuedUnlockedQuestIDs", QueuedUnlockedQuestIDs);
		info.AddValue("QueuedCompletedQuestIDs", QueuedCompletedQuestIDs);
		info.AddValue("QueuedQuestCompletionRewards", QueuedCompletionRewards);
		info.AddValue("QueuedPlatformAchievementsToUnlock", QueuedPlatformAchievementsToUnlock);
		info.AddValue("MapRNGState", m_MapRNG.Save());
		info.AddValue("ScenarioStartMessages", ScenarioStartMessages);
		info.AddValue("ScenarioCompleteMessages", ScenarioCompleteMessages);
		info.AddValue("ScenarioRoomRevealMessages", ScenarioRoomRevealMessages);
		info.AddValue("GoldMode", GoldMode);
		info.AddValue("EnhancementMode", EnhancementMode);
		info.AddValue("SaveOwner", SaveOwner);
		info.AddValue("MinRequiredCharacters", MinRequiredCharacters);
		info.AddValue("CurrentSingleScenario", CurrentSingleScenario);
		info.AddValue("m_LastItemNetworkID", m_LastItemNetworkID);
		info.AddValue("AlreadyRewardedChestTreasureTableIDs", AlreadyRewardedChestTreasureTableIDs);
		info.AddValue("MapFTUECompleted", MapFTUECompleted);
		info.AddValue("JustCompletedLocationState", JustCompletedLocationState);
		info.AddValue("ActivitiesProgressData", ActivitiesProgressData);
		info.AddValue("CampaignCompleteActivityQuestsIds", CampaignCompleteActivityQuestsIds);
	}

	public CMapState(SerializationInfo info, StreamingContext context)
	{
		bool flag = false;
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "DLLMode":
					DLLMode = (ScenarioManager.EDLLMode)info.GetValue("DLLMode", typeof(ScenarioManager.EDLLMode));
					break;
				case "DLCEnabled":
					DLCEnabled = (DLCRegistry.EDLCKey)info.GetValue("DLCEnabled", typeof(DLCRegistry.EDLCKey));
					break;
				case "MapParty":
					MapParty = (CMapParty)info.GetValue("MapParty", typeof(CMapParty));
					break;
				case "CurrentMapPhase":
					CurrentMapPhase = (CMapPhase)info.GetValue("CurrentMapPhase", typeof(CMapPhase));
					break;
				case "UnlockedChapters":
					UnlockedChapters = (List<CChapter>)info.GetValue("UnlockedChapters", typeof(List<CChapter>));
					break;
				case "TutorialCompleted":
					TutorialCompleted = info.GetBoolean("TutorialCompleted");
					break;
				case "IntroCompleted":
					IntroCompleted = info.GetBoolean("IntroCompleted");
					break;
				case "DifficultySetting":
					DifficultySetting = (EAdventureDifficulty)info.GetValue("DifficultySetting", typeof(EAdventureDifficulty));
					break;
				case "HouseRulesSetting":
					HouseRulesSetting = (StateShared.EHouseRulesFlag)info.GetValue("HouseRulesSetting", typeof(StateShared.EHouseRulesFlag));
					flag = true;
					break;
				case "HeadquartersState":
					HeadquartersState = (CHeadquartersState)info.GetValue("HeadquartersState", typeof(CHeadquartersState));
					break;
				case "TempleState":
					TempleState = (CTempleState)info.GetValue("TempleState", typeof(CTempleState));
					break;
				case "VillageStates":
					VillageStates = (List<CVillageState>)info.GetValue("VillageStates", typeof(List<CVillageState>));
					break;
				case "MapMessageStates":
					MapMessageStates = (List<CMapMessageState>)info.GetValue("MapMessageStates", typeof(List<CMapMessageState>));
					break;
				case "VisibilitySphereStates":
					VisibilitySphereStates = (List<CVisibilitySphereState>)info.GetValue("VisibilitySphereStates", typeof(List<CVisibilitySphereState>));
					break;
				case "JobQuestStates":
					JobQuestStates = (List<CJobQuestState>)info.GetValue("JobQuestStates", typeof(List<CJobQuestState>));
					break;
				case "CurrentJobQuestStates":
					CurrentJobQuestStates = (List<CJobQuestState>)info.GetValue("CurrentJobQuestStates", typeof(List<CJobQuestState>));
					break;
				case "PreviousJobQuestStates":
					PreviousJobQuestStates = (List<CJobQuestState>)info.GetValue("PreviousJobQuestStates", typeof(List<CJobQuestState>));
					break;
				case "IsModded":
					IsModded = info.GetBoolean("IsModded");
					break;
				case "IsInitialised":
					IsInitialised = info.GetBoolean("IsInitialised");
					break;
				case "Seed":
					Seed = info.GetInt32("Seed");
					break;
				case "UsedEventNames":
					UsedEventNames = (List<string>)info.GetValue("UsedEventNames", typeof(List<string>));
					break;
				case "CanDrawCityEvent":
					CanDrawCityEvent = info.GetBoolean("CanDrawCityEvent");
					break;
				case "StartupRewardGroup":
					StartupRewardGroup = (RewardGroup)info.GetValue("StartupRewardGroup", typeof(RewardGroup));
					break;
				case "StartupTreasureTablesRewardGenerationRNGState":
					StartupTreasureTablesRewardGenerationRNGState = (Extensions.RandomState)info.GetValue("StartupTreasureTablesRewardGenerationRNGState", typeof(Extensions.RandomState));
					break;
				case "QueuedUnlockedQuestIDs":
					QueuedUnlockedQuestIDs = (List<string>)info.GetValue("QueuedUnlockedQuestIDs", typeof(List<string>));
					break;
				case "QueuedCompletedQuestIDs":
					QueuedCompletedQuestIDs = (List<string>)info.GetValue("QueuedCompletedQuestIDs", typeof(List<string>));
					break;
				case "QueuedQuestCompletionRewards":
					QueuedCompletionRewards = (List<Reward>)info.GetValue("QueuedQuestCompletionRewards", typeof(List<Reward>));
					break;
				case "QueuedPlatformAchievementsToUnlock":
					QueuedPlatformAchievementsToUnlock = (List<string>)info.GetValue("QueuedPlatformAchievementsToUnlock", typeof(List<string>));
					break;
				case "ScenarioStates":
					ScenarioStates = (Dictionary<string, ScenarioState>)info.GetValue("ScenarioStates", typeof(Dictionary<string, ScenarioState>));
					break;
				case "MapRNGState":
					MapRNGState = (Extensions.RandomState)info.GetValue("MapRNGState", typeof(Extensions.RandomState));
					if (MapRNGState != null)
					{
						m_MapRNG = MapRNGState.Restore();
					}
					break;
				case "ScenarioStartMessages":
					ScenarioStartMessages = (Dictionary<string, CLevelMessage>)info.GetValue("ScenarioStartMessages", typeof(Dictionary<string, CLevelMessage>));
					break;
				case "ScenarioCompleteMessages":
					ScenarioCompleteMessages = (Dictionary<string, CLevelMessage>)info.GetValue("ScenarioCompleteMessages", typeof(Dictionary<string, CLevelMessage>));
					break;
				case "ScenarioRoomRevealMessages":
					ScenarioRoomRevealMessages = (Dictionary<string, List<CLevelMessage>>)info.GetValue("ScenarioRoomRevealMessages", typeof(Dictionary<string, List<CLevelMessage>>));
					break;
				case "GoldMode":
					GoldMode = (EGoldMode)info.GetValue("GoldMode", typeof(EGoldMode));
					break;
				case "EnhancementMode":
					EnhancementMode = (EEnhancementMode)info.GetValue("EnhancementMode", typeof(EEnhancementMode));
					break;
				case "SaveOwner":
					SaveOwner = (MapStateSaveOwner)info.GetValue("SaveOwner", typeof(MapStateSaveOwner));
					break;
				case "MinRequiredCharacters":
					MinRequiredCharacters = info.GetInt32("MinRequiredCharacters");
					break;
				case "CurrentSingleScenario":
					CurrentSingleScenario = (CCustomLevelData)info.GetValue("CurrentSingleScenario", typeof(CCustomLevelData));
					break;
				case "m_LastItemNetworkID":
					m_LastItemNetworkID = (uint)info.GetValue("m_LastItemNetworkID", typeof(uint));
					break;
				case "AlreadyRewardedChestTreasureTableIDs":
					AlreadyRewardedChestTreasureTableIDs = (List<string>)info.GetValue("AlreadyRewardedChestTreasureTableIDs", typeof(List<string>));
					break;
				case "MapFTUECompleted":
					MapFTUECompleted = info.GetBoolean("MapFTUECompleted");
					break;
				case "JustCompletedLocationState":
					JustCompletedLocationState = (CLocationState)info.GetValue("JustCompletedLocationState", typeof(CLocationState));
					break;
				case "ActivitiesProgressData":
					ActivitiesProgressData = ((ActivitiesProgressData)info.GetValue("ActivitiesProgressData", typeof(ActivitiesProgressData))) ?? new ActivitiesProgressData();
					break;
				case "CampaignCompleteActivityQuestsIds":
					CampaignCompleteActivityQuestsIds = ((List<int>)info.GetValue("CampaignCompleteActivityQuestsIds", typeof(List<int>))) ?? new List<int>();
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CMapState entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		if (DLLMode == ScenarioManager.EDLLMode.None)
		{
			DLLMode = ScenarioManager.EDLLMode.Guildmaster;
		}
		if (m_MapRNG == null)
		{
			m_MapRNG = new SharedLibrary.Random(Seed);
		}
		if (ScenarioStartMessages == null)
		{
			ScenarioStartMessages = new Dictionary<string, CLevelMessage>();
		}
		if (ScenarioCompleteMessages == null)
		{
			ScenarioCompleteMessages = new Dictionary<string, CLevelMessage>();
		}
		if (ScenarioRoomRevealMessages == null)
		{
			ScenarioRoomRevealMessages = new Dictionary<string, List<CLevelMessage>>();
		}
		if (VisibilitySphereStates == null)
		{
			VisibilitySphereStates = new List<CVisibilitySphereState>();
		}
		if (QueuedUnlockedQuestIDs == null)
		{
			QueuedUnlockedQuestIDs = new List<string>();
		}
		if (QueuedCompletedQuestIDs == null)
		{
			QueuedCompletedQuestIDs = new List<string>();
		}
		if (QueuedCompletionRewards == null)
		{
			QueuedCompletionRewards = new List<Reward>();
		}
		if (QueuedPlatformAchievementsToUnlock == null)
		{
			QueuedPlatformAchievementsToUnlock = new List<string>();
		}
		if (JobQuestStates == null)
		{
			JobQuestStates = new List<CJobQuestState>();
		}
		if (CurrentJobQuestStates == null)
		{
			CurrentJobQuestStates = new List<CJobQuestState>();
		}
		if (PreviousJobQuestStates == null)
		{
			PreviousJobQuestStates = new List<CJobQuestState>();
		}
		if (TempleState == null)
		{
			TempleState = new CTempleState();
		}
		if (EnhancementMode == EEnhancementMode.None)
		{
			EnhancementMode = EEnhancementMode.CharacterPersistent;
		}
		if (AlreadyRewardedChestTreasureTableIDs == null)
		{
			AlreadyRewardedChestTreasureTableIDs = new List<string>();
		}
		if (MinRequiredCharacters == 0)
		{
			MinRequiredCharacters = 2;
		}
		if (!flag)
		{
			HouseRulesSetting |= StateShared.EHouseRulesFlag.FrosthavenLOS;
			HouseRulesSetting |= StateShared.EHouseRulesFlag.FrosthavenRollingAttackModifiers;
			HouseRulesSetting |= StateShared.EHouseRulesFlag.FrosthavenSummonFocus;
		}
	}

	public void UpdateScenarioStateItems(ScenarioState scenario)
	{
		if (scenario == null)
		{
			return;
		}
		foreach (PlayerState player in scenario.Players)
		{
			foreach (CItem item in player.Items)
			{
				if (item.NetworkID == 0)
				{
					item.NetworkID = GetNextItemNetworkID();
					SimpleLog.AddToSimpleLog("(UpdateScenarioStateItems) Updating NetworkID for " + item.Name + " to " + item.NetworkID);
				}
			}
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		if (CurrentMapPhaseType == EMapPhaseType.ShowMapMessages)
		{
			CurrentMapPhase = new CMapPhase(EMapPhaseType.InHQ);
		}
		if (ActivitiesProgressData == null)
		{
			ActivitiesProgressData activitiesProgressData = (ActivitiesProgressData = new ActivitiesProgressData());
		}
		if (CampaignCompleteActivityQuestsIds == null)
		{
			List<int> list = (CampaignCompleteActivityQuestsIds = new List<int>());
		}
	}

	public void UpdateScenarioStates()
	{
		if (ScenarioStates == null || ScenarioStates.Count <= 0)
		{
			return;
		}
		if (InProgressQuestState != null)
		{
			if (ScenarioStates.TryGetValue(InProgressQuestState.ScenarioState.ID + "_AutoSave", out var value))
			{
				InProgressQuestState.ScenarioState.UpdateAutoSaveState(value);
			}
			if (ScenarioStates.TryGetValue(InProgressQuestState.ScenarioState.ID + "_Initial", out var value2))
			{
				InProgressQuestState.ScenarioState.UpdateInitialSaveState(value2);
			}
		}
		ScenarioStates = null;
		RegenerateAllMapScenarios(excludeInProgressQuest: true);
		m_ForceRegenerate = false;
	}

	public void ValidateQuestStates()
	{
		for (int num = CurrentJobQuestStates.Count - 1; num >= 0; num--)
		{
			CJobQuestState cJobQuestState = CurrentJobQuestStates[num];
			if (cJobQuestState == null)
			{
				CurrentJobQuestStates.Remove(cJobQuestState);
			}
		}
		List<CQuestState> list = AllQuests.FindAll((CQuestState q) => q.InProgress).ToList();
		if (list.Count > 1)
		{
			foreach (CQuestState item in list)
			{
				item.ResetQuest(regenerate: false);
			}
			CurrentMapPhase = new CMapPhase(EMapPhaseType.InHQ);
			DLLDebug.LogWarning("Multiple quest states found in progress - reset to unlocked and set MapPhase to InHQ");
		}
		if (InProgressQuestState != null && !IsInScenarioPhase)
		{
			InProgressQuestState.ResetQuest();
			CurrentMapPhase = new CMapPhase(EMapPhaseType.InHQ);
			DLLDebug.LogWarning("An In Progress Quest State was found but MapPhase was not set to InScenario - reset to unlocked and set MapPhase to InHQ");
		}
		foreach (CJobQuestState jobQuestState in JobQuestStates)
		{
			if (jobQuestState.InProgress && !CurrentJobQuestStates.Contains(jobQuestState))
			{
				jobQuestState.ResetQuest(regenerate: false);
			}
		}
	}

	public int GetHighestUnlockedSubChapter(int chapter)
	{
		if (UnlockedChapters.Where((CChapter w) => w.Chapter == chapter).Count() > 0 && (from s in UnlockedChapters
			where s.Chapter == chapter
			select s.SubChapter).Count() > 0)
		{
			return (from s in UnlockedChapters
				where s.Chapter == chapter
				select s.SubChapter).Max();
		}
		return 0;
	}

	public CMapState(int seed, EAdventureDifficulty difficulty, StateShared.EHouseRulesFlag houseRulesSettings, EGoldMode goldMode, EEnhancementMode enhancementMode, DLCRegistry.EDLCKey dlcEnabled, bool isModded, MapStateSaveOwner saveOwner, ScenarioManager.EDLLMode mode, bool mapFtueCompleted = false)
	{
		Seed = seed;
		m_MapRNG = new SharedLibrary.Random(seed);
		MapRNGState = m_MapRNG.Save();
		DifficultySetting = difficulty;
		HouseRulesSetting = houseRulesSettings;
		GoldMode = goldMode;
		EnhancementMode = enhancementMode;
		DLCEnabled = dlcEnabled;
		IsModded = isModded;
		SaveOwner = saveOwner;
		DLLMode = mode;
		MinRequiredCharacters = 2;
		m_LastItemNetworkID = 2u;
		UnlockedChapters = new List<CChapter>
		{
			new CChapter(1, 0)
		};
		MapParty = new CMapParty(Environment.StackTrace);
		CurrentMapPhase = new CMapPhase(EMapPhaseType.None);
		HeadquartersState = new CHeadquartersState();
		TempleState = new CTempleState();
		VillageStates = new List<CVillageState>();
		MapMessageStates = new List<CMapMessageState>();
		VisibilitySphereStates = new List<CVisibilitySphereState>();
		JobQuestStates = new List<CJobQuestState>();
		CurrentJobQuestStates = new List<CJobQuestState>();
		PreviousJobQuestStates = new List<CJobQuestState>();
		QueuedUnlockedQuestIDs = new List<string>();
		QueuedCompletedQuestIDs = new List<string>();
		QueuedCompletionRewards = new List<Reward>();
		QueuedPlatformAchievementsToUnlock = new List<string>();
		CanDrawCityEvent = true;
		AlreadyRewardedChestTreasureTableIDs = new List<string>();
		MapFTUECompleted = mapFtueCompleted;
		ScenarioStates = new Dictionary<string, ScenarioState>();
		ScenarioStartMessages = new Dictionary<string, CLevelMessage>();
		ScenarioCompleteMessages = new Dictionary<string, CLevelMessage>();
		ScenarioRoomRevealMessages = new Dictionary<string, List<CLevelMessage>>();
		foreach (CVillage village in MapRuleLibraryClient.MRLYML.Villages)
		{
			VillageStates.Add(new CVillageState(village));
		}
		foreach (CMapMessage mapMessage in MapRuleLibraryClient.MRLYML.MapMessages)
		{
			MapMessageStates.Add(new CMapMessageState(mapMessage));
		}
		foreach (CVisibilitySphere visibilitySphere in MapRuleLibraryClient.MRLYML.VisibilitySpheres)
		{
			VisibilitySphereStates.Add(new CVisibilitySphereState(visibilitySphere));
		}
		foreach (CQuest quest in MapRuleLibraryClient.MRLYML.Quests)
		{
			if (quest.Type == EQuestType.Job)
			{
				JobQuestStates.Add(new CJobQuestState(quest, new CVector3(0f, 0f, 0f)));
			}
		}
		UsedEventNames = new List<string>();
		IsInitialised = false;
		ActivitiesProgressData = new ActivitiesProgressData();
		ValidateQuests();
	}

	public void SetSaveOwner(MapStateSaveOwner saveOwner)
	{
		SaveOwner = saveOwner;
	}

	public void Initialise(bool skipTutorial = false, bool skipIntro = false)
	{
		if (!IsInitialised)
		{
			HeadquartersState.Init();
			HeadquartersState.UnlockLocation();
			MapParty.Init();
			foreach (CQuestState allQuest in AllQuests)
			{
				allQuest.Init();
			}
			foreach (CJobQuestState jobQuestState in JobQuestStates)
			{
				jobQuestState.Init();
			}
			if (skipTutorial || skipIntro)
			{
				SkipFTUEInternal(skipTutorial, skipIntro);
			}
			IsInitialised = true;
			SimpleLog.AddToSimpleLog("MapRNG (Initialised new Map State): " + AdventureState.MapState.PeekMapRNG);
		}
		OnMapStateAdventureStarted();
	}

	public void OnMapStateAdventureStarted()
	{
		AdventureStateStarting = true;
		if (Thread.CurrentThread != MapRuleLibraryClient.Instance.MessageThreadHandler)
		{
			MapRuleLibraryClient.Instance.AddQueueMessage(new CMapDLLMessage(EMapDLLMessageType.OnMapStateAdventureStarted), processImmediately: false);
			return;
		}
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		SimpleLog.AddToSimpleLog("MapRNG (OnMapStateAdventureStarted started): " + AdventureState.MapState.PeekMapRNG);
		RemoveOldFTUEScenarioStates();
		HeadquartersState.OnMapStateAdventureStarted();
		foreach (CVillageState villageState in VillageStates)
		{
			villageState.OnMapStateAdventureStarted();
		}
		List<CVillageState> list = new List<CVillageState>();
		foreach (CVillageState villageState2 in VillageStates)
		{
			if (villageState2.Village == null)
			{
				list.Add(villageState2);
			}
		}
		foreach (CVillageState item in list)
		{
			VillageStates.Remove(item);
		}
		foreach (CVillage village in MapRuleLibraryClient.MRLYML.Villages)
		{
			if (!VillageStates.Any((CVillageState v) => v.Village == village))
			{
				VillageStates.Add(new CVillageState(village));
			}
		}
		foreach (CMapMessage mapMessage in MapRuleLibraryClient.MRLYML.MapMessages)
		{
			if (!MapMessageStates.Any((CMapMessageState m) => m.MapMessage.MessageID == mapMessage.MessageID))
			{
				CMapMessageState cMapMessageState = new CMapMessageState(mapMessage);
				MapMessageStates.Add(cMapMessageState);
				cMapMessageState.Init();
			}
		}
		foreach (CMapMessageState mapMessageState in MapMessageStates)
		{
			mapMessageState.OnMapStateAdventureStarted();
		}
		List<CMapMessageState> list2 = new List<CMapMessageState>();
		foreach (CMapMessageState mapMessageState2 in MapMessageStates)
		{
			if (mapMessageState2.MapMessage == null)
			{
				list2.Add(mapMessageState2);
			}
		}
		foreach (CMapMessageState item2 in list2)
		{
			MapMessageStates.Remove(item2);
		}
		List<CVisibilitySphereState> list3 = new List<CVisibilitySphereState>();
		foreach (CVisibilitySphereState visibilitySphereState in VisibilitySphereStates)
		{
			if (visibilitySphereState.VisibilitySphere == null)
			{
				list3.Add(visibilitySphereState);
			}
		}
		foreach (CVisibilitySphereState item3 in list3)
		{
			VisibilitySphereStates.Remove(item3);
		}
		foreach (CVisibilitySphere visibilitySphere in MapRuleLibraryClient.MRLYML.VisibilitySpheres)
		{
			if (!VisibilitySphereStates.Any((CVisibilitySphereState v) => v.VisibilitySphere.ID == visibilitySphere.ID))
			{
				CVisibilitySphereState cVisibilitySphereState = new CVisibilitySphereState(visibilitySphere);
				VisibilitySphereStates.Add(cVisibilitySphereState);
				cVisibilitySphereState.Init();
			}
		}
		foreach (CJobQuestState jobQuestState in JobQuestStates)
		{
			jobQuestState.OnMapStateAdventureStarted();
		}
		List<CJobQuestState> list4 = new List<CJobQuestState>();
		foreach (CJobQuestState jobQuestState2 in JobQuestStates)
		{
			if (jobQuestState2.Quest == null)
			{
				list4.Add(jobQuestState2);
			}
		}
		foreach (CJobQuestState item4 in list4)
		{
			JobQuestStates.Remove(item4);
		}
		foreach (CQuest quest in MapRuleLibraryClient.MRLYML.Quests)
		{
			if (quest.Type == EQuestType.Job && JobQuestStates.SingleOrDefault((CJobQuestState q) => q.Quest.ID == quest.ID) == null)
			{
				CJobQuestState cJobQuestState = new CJobQuestState(quest, new CVector3(0f, 0f, 0f));
				JobQuestStates.Add(cJobQuestState);
				cJobQuestState.Init();
			}
		}
		MapParty.OnMapStateAdventureStarted();
		if (!IntroCompleted)
		{
			CheckFTUECompletion(applyRewards: false);
		}
		else
		{
			CheckForStartupTreasureTableUpdates();
		}
		ValidateQuests();
		FixDuplicateMapStateItems();
		if (m_LastItemNetworkID == 0)
		{
			m_LastItemNetworkID = 2u;
		}
		if (ScenarioStates != null && ScenarioStates.Count > 0)
		{
			foreach (KeyValuePair<string, ScenarioState> scenarioState in ScenarioStates)
			{
				UpdateScenarioStateItems(scenarioState.Value);
			}
		}
		if (CurrentSingleScenario != null)
		{
			UpdateScenarioStateItems(CurrentSingleScenario.ScenarioState);
		}
		if (HeadquartersState != null)
		{
			if (HeadquartersState.StartingScenarioStates != null && HeadquartersState.StartingScenarioStates.Count > 0)
			{
				foreach (CMapScenarioState startingScenarioState in HeadquartersState.StartingScenarioStates)
				{
					if (startingScenarioState != null)
					{
						if (startingScenarioState.InitialState != null)
						{
							UpdateScenarioStateItems(startingScenarioState.InitialState);
						}
						if (startingScenarioState.AutoSaveState != null)
						{
							UpdateScenarioStateItems(startingScenarioState.AutoSaveState);
						}
					}
				}
			}
			if (HeadquartersState.QuestStates != null && HeadquartersState.QuestStates.Count > 0)
			{
				foreach (CQuestState questState in HeadquartersState.QuestStates)
				{
					if (questState != null && questState.ScenarioState != null)
					{
						if (questState.ScenarioState.InitialState != null)
						{
							UpdateScenarioStateItems(questState.ScenarioState.InitialState);
						}
						if (questState.ScenarioState.AutoSaveState != null)
						{
							UpdateScenarioStateItems(questState.ScenarioState.AutoSaveState);
						}
					}
				}
			}
		}
		if (VillageStates != null && VillageStates.Count > 0)
		{
			foreach (CVillageState villageState3 in VillageStates)
			{
				if (villageState3 == null || villageState3.QuestStates == null || villageState3.QuestStates.Count <= 0)
				{
					continue;
				}
				foreach (CQuestState questState2 in villageState3.QuestStates)
				{
					if (questState2 != null && questState2.ScenarioState != null)
					{
						if (questState2.ScenarioState.InitialState != null)
						{
							UpdateScenarioStateItems(questState2.ScenarioState.InitialState);
						}
						if (questState2.ScenarioState.AutoSaveState != null)
						{
							UpdateScenarioStateItems(questState2.ScenarioState.AutoSaveState);
						}
					}
				}
			}
		}
		if (CurrentMapScenarioState != null)
		{
			if (CurrentMapScenarioState.InitialState != null)
			{
				UpdateScenarioStateItems(CurrentMapScenarioState.InitialState);
			}
			if (CurrentMapScenarioState.AutoSaveState != null)
			{
				UpdateScenarioStateItems(CurrentMapScenarioState.AutoSaveState);
			}
		}
		SimpleLog.AddToSimpleLog("MapRNG (OnMapStateAdventureStarted complete): " + AdventureState.MapState.PeekMapRNG);
		AdventureStateStarting = false;
		stopwatch.Stop();
		Console.WriteLine("OnMapStateAdventureStarted time elapsed : {0}", stopwatch.Elapsed);
	}

	public void OnMapLoaded(bool isJoiningMPClient)
	{
		if (Thread.CurrentThread != MapRuleLibraryClient.Instance.MessageThreadHandler)
		{
			MapRuleLibraryClient.Instance.AddQueueMessage(new COnMapLoaded_MapDLLMessage(isJoiningMPClient), processImmediately: false);
			return;
		}
		SimpleLog.AddToSimpleLog("MapRNG (OnMapLoaded started): " + AdventureState.MapState.PeekMapRNG);
		if (!isJoiningMPClient && !IsInScenarioPhase)
		{
			CheckLockedContent(loadedMap: true);
			CheckCurrentJobQuests();
			if (AdventureState.MapState.IsCampaign)
			{
				MapParty.UnlockNewProsperityItems();
			}
			foreach (CMapCharacter checkCharacter in MapParty.CheckCharacters)
			{
				checkCharacter.GetSmallItemMax();
			}
		}
		SimpleLog.AddToSimpleLog("MapRNG (OnMapLoaded complete): " + AdventureState.MapState.PeekMapRNG);
	}

	public List<MapStateItemDuplicates> GetItemDuplicateGuids()
	{
		return (from g in HeadquartersState.CheckMerchantStock.Concat(MapParty.CheckUnboundItems.Concat(MapParty.CheckCharacters.SelectMany((CMapCharacter x) => x.CheckBoundItems.Concat(x.CheckEquippedItems))))
			group g by g.ItemGuid into s
			where s.Count() > 1
			select new MapStateItemDuplicates(s.Key, s.Count())).ToList();
	}

	public void FixDuplicateMapStateItems()
	{
		List<MapStateItemDuplicates> itemDuplicateGuids = GetItemDuplicateGuids();
		List<CItem> checkMerchantStock = HeadquartersState.CheckMerchantStock;
		List<CItem> checkUnboundItems = MapParty.CheckUnboundItems;
		Dictionary<string, List<CItem>> dictionary = new Dictionary<string, List<CItem>>();
		Dictionary<string, List<CItem>> dictionary2 = new Dictionary<string, List<CItem>>();
		foreach (CMapCharacter checkCharacter in MapParty.CheckCharacters)
		{
			string key = (IsCampaign ? checkCharacter.CharacterName : checkCharacter.CharacterID);
			dictionary.Add(key, checkCharacter.CheckBoundItems);
			dictionary2.Add(key, checkCharacter.CheckEquippedItems);
		}
		foreach (MapStateItemDuplicates duplicate in itemDuplicateGuids)
		{
			int num = duplicate.NumberOfDuplicates;
			while (num > 1)
			{
				CItem cItem = checkMerchantStock.FirstOrDefault((CItem e) => e.ItemGuid == duplicate.ItemGuid);
				if (cItem != null)
				{
					HeadquartersState.RemoveItemFromMerchantStock(cItem);
					checkMerchantStock.Remove(cItem);
					num--;
					continue;
				}
				cItem = checkUnboundItems.FirstOrDefault((CItem e) => e.ItemGuid == duplicate.ItemGuid);
				if (cItem != null)
				{
					MapParty.RemoveItemFromUnboundItems(cItem);
					checkUnboundItems.Remove(cItem);
					num--;
					continue;
				}
				bool flag = false;
				foreach (CMapCharacter checkCharacter2 in MapParty.CheckCharacters)
				{
					string key2 = (IsCampaign ? checkCharacter2.CharacterName : checkCharacter2.CharacterID);
					cItem = dictionary[key2].FirstOrDefault((CItem e) => e.ItemGuid == duplicate.ItemGuid);
					if (cItem != null)
					{
						checkCharacter2.RemoveItemFromBoundItems(cItem);
						dictionary[key2].Remove(cItem);
						num--;
						flag = true;
						break;
					}
					cItem = dictionary2[key2].FirstOrDefault((CItem e) => e.ItemGuid == duplicate.ItemGuid);
					if (cItem != null)
					{
						checkCharacter2.RemoveEquippedItem(cItem);
						dictionary2[key2].Remove(cItem);
						num--;
						flag = true;
						break;
					}
				}
			}
		}
	}

	public void ValidateQuests()
	{
		foreach (CQuestState quest in (from i in AllQuests
			group i by new { i.ID } into j
			where j.Count() > 1
			select j).SelectMany(group => group).ToList())
		{
			CLocationState cLocationState = VillageStates.Find((CVillageState x) => x.QuestStates.Contains(quest));
			if (cLocationState == null && HeadquartersState.QuestStates.Contains(quest))
			{
				cLocationState = HeadquartersState;
			}
			DLLDebug.LogError("A quest with a duplicate ID was found for Quest ID: " + quest.ID + " Starting Village: " + cLocationState.ID);
		}
		ValidateQuestStates();
	}

	public void ValidateAddedQuest(CQuestState addedQuestState)
	{
		foreach (CQuestState questState in HeadquartersState.QuestStates)
		{
			_ = questState;
			for (int num = HeadquartersState.QuestStates.Count - 1; num >= 0; num--)
			{
				CQuestState cQuestState = HeadquartersState.QuestStates[num];
				if (cQuestState.ID == addedQuestState.ID)
				{
					DLLDebug.LogWarning("A quest with a duplicate ID was found for Quest ID: " + addedQuestState.ID + " at Location ID: " + HeadquartersState.ID + " and was removed");
					HeadquartersState.QuestStates.Remove(cQuestState);
				}
			}
		}
		foreach (CVillageState villageState in VillageStates)
		{
			for (int num2 = villageState.QuestStates.Count - 1; num2 >= 0; num2--)
			{
				CQuestState cQuestState2 = villageState.QuestStates[num2];
				if (cQuestState2.ID == addedQuestState.ID)
				{
					DLLDebug.LogWarning("A quest with a duplicate ID was found for Quest ID: " + addedQuestState.ID + " at Location ID: " + villageState.ID + " and was removed");
					villageState.QuestStates.Remove(cQuestState2);
				}
			}
		}
	}

	public void CheckForDuplicateItems()
	{
		List<CItem> list = new List<CItem>();
		List<CItem> checkMerchantStock = HeadquartersState.CheckMerchantStock;
		list.AddRange(checkMerchantStock);
		List<CItem> checkUnboundItems = MapParty.CheckUnboundItems;
		list.AddRange(checkUnboundItems);
		Dictionary<string, List<CItem>> dictionary = new Dictionary<string, List<CItem>>();
		Dictionary<string, List<CItem>> dictionary2 = new Dictionary<string, List<CItem>>();
		foreach (CMapCharacter checkCharacter in MapParty.CheckCharacters)
		{
			string key = (IsCampaign ? checkCharacter.CharacterName : checkCharacter.CharacterID);
			dictionary.Add(key, checkCharacter.CheckBoundItems);
			list.AddRange(dictionary[key]);
			dictionary2.Add(key, checkCharacter.CheckEquippedItems);
			list.AddRange(dictionary2[key]);
		}
		if (list.Select((CItem s) => s.ItemGuid).Distinct().Count() == list.Count)
		{
			return;
		}
		string text = "Duplicate items found.  Distinct Count: " + list.Select((CItem s) => s.ItemGuid).Distinct().Count() + "  Total Count: " + list.Count;
		foreach (CItem item in list)
		{
			if (list.Where((CItem w) => w.ItemGuid == item.ItemGuid).Count() <= 1)
			{
				continue;
			}
			text = text + "\nName: " + item.Name + "  Item Guid: " + item.ItemGuid + "  Network ID: " + item.NetworkID + " - exists in";
			if (checkMerchantStock.Exists((CItem e) => e.ItemGuid == item.ItemGuid))
			{
				text += " | Merchant Stock |";
			}
			if (checkUnboundItems.Exists((CItem e) => e.ItemGuid == item.ItemGuid))
			{
				text += " | Unbound Items |";
			}
			foreach (CMapCharacter checkCharacter2 in MapParty.CheckCharacters)
			{
				string text2 = (IsCampaign ? checkCharacter2.CharacterName : checkCharacter2.CharacterID);
				if (dictionary[text2].Exists((CItem e) => e.ItemGuid == item.ItemGuid))
				{
					text = text + " | " + text2 + " Bound Items |";
				}
				if (dictionary2[text2].Exists((CItem e) => e.ItemGuid == item.ItemGuid))
				{
					text = text + " | " + text2 + " Equipped Items |";
				}
			}
		}
		DLLDebug.LogError(text);
	}

	public void CheckLockedContent(bool loadedMap = false)
	{
		if (Thread.CurrentThread != MapRuleLibraryClient.Instance.MessageThreadHandler)
		{
			DLLDebug.Log("Queue CheckLockedContent");
			MapRuleLibraryClient.Instance.AddQueueMessage(new CMapDLLMessage(EMapDLLMessageType.CheckLockedContent), processImmediately: false);
		}
		else
		{
			CheckLockedContentInternal(loadedMap);
		}
	}

	public void CheckLockedContentInternal(bool loadedMap = false)
	{
		DLLDebug.Log("CheckLockedContentInternal");
		if (!HeadquartersState.MultiplayerUnlocked && IntroCompleted)
		{
			HeadquartersState.UnlockMultiplayer();
		}
		CheckAllLockedVillages();
		CheckAllLockedQuests();
		if (IsCampaign || !loadedMap)
		{
			CheckNonTrophyAchievements();
		}
		if (SoloID != "")
		{
			foreach (CMapCharacter checkCharacter in MapParty.CheckCharacters)
			{
				if (checkCharacter.CharacterID == SoloID && checkCharacter.PlayerStats.IsClear())
				{
					SoloID = "";
				}
			}
		}
		CheckPersonalQuests(SoloID);
		CheckAllLockedMessages();
		CheckAllLockedVisibilitySpheres();
		CheckForBlockedQuests();
		MapParty.HeroSummonsStats?.Clear();
		MapParty.MonstersStats?.Clear();
		foreach (CMapCharacter checkCharacter2 in MapParty.CheckCharacters)
		{
			checkCharacter2.PlayerStats?.Clear();
		}
		if (!IsCampaign && loadedMap)
		{
			CheckNonTrophyAchievements();
		}
		if (QueuedPlatformAchievementsToUnlock.Count > 0)
		{
			CPostTrophyAchievement_MapClientMessage message = new CPostTrophyAchievement_MapClientMessage(QueuedPlatformAchievementsToUnlock.ToList());
			if (MapRuleLibraryClient.Instance?.MessageHandler != null)
			{
				MapRuleLibraryClient.Instance.MessageHandler(message);
			}
			else
			{
				DLLDebug.LogWarning("Message handler not set");
			}
		}
		QueuedPlatformAchievementsToUnlock.Clear();
		if (MapRuleLibraryClient.Instance?.MessageHandler != null)
		{
			MapRuleLibraryClient.Instance.MessageHandler(new CFinishCheckLockedContent_MapClientMessage());
		}
	}

	public void CheckAutoCompleteQuestsAndAchievements()
	{
		if (!MapRuleLibraryClient.MRLYML.AutoComplete.IsLoaded)
		{
			return;
		}
		foreach (string questID in MapRuleLibraryClient.MRLYML.AutoComplete.AutoCompleteQuestIDs)
		{
			CQuestState cQuestState = AllQuests.SingleOrDefault((CQuestState q) => q.ID == questID);
			if (cQuestState != null && cQuestState.QuestState != CQuestState.EQuestState.Completed)
			{
				cQuestState.AutoComplete();
			}
		}
		foreach (string achievementID in MapRuleLibraryClient.MRLYML.AutoComplete.AutoCompleteAchievementIDs)
		{
			CPartyAchievement cPartyAchievement = MapParty.Achievements.SingleOrDefault((CPartyAchievement a) => a.ID == achievementID);
			if (cPartyAchievement != null && cPartyAchievement.State != EAchievementState.Completed)
			{
				cPartyAchievement.CompleteAchievement();
			}
		}
		CheckLockedContent();
		if (!IntroCompleted)
		{
			CheckFTUECompletion();
		}
	}

	public void StartTutorial()
	{
		CurrentMapPhase = new CMapPhaseInScenario(HeadquartersState.Headquarters.StartingScenarios[HeadquartersState.CurrentStartingScenarioIndex].ID);
		m_PlayingTutorial = true;
	}

	public void InitMapPhase()
	{
		CurrentMapPhase = new CMapPhase(EMapPhaseType.InHQ);
	}

	public void CompleteTutorial()
	{
		if (HeadquartersState.UpdateStartingScenariosCompletion())
		{
			m_PlayingTutorial = false;
			TutorialCompleted = true;
		}
	}

	public void CheckFTUECompletion(bool applyRewards = true)
	{
		bool flag = true;
		foreach (string questID in HeadquartersState.Headquarters.TutorialQuestNames)
		{
			CQuestState cQuestState = AllQuests.SingleOrDefault((CQuestState q) => q.ID == questID);
			if (cQuestState != null && cQuestState.QuestState != CQuestState.EQuestState.Completed)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			if (applyRewards)
			{
				StartupTreasureTablesRewardGenerationRNGState = MapRNG.Save();
				SharedLibrary.Random rng = StartupTreasureTablesRewardGenerationRNGState.Restore();
				StartupRewardGroup = new RewardGroup(TreasureTableProcessing.RollTreasureTables(rng, MapYMLShared.ValidateTreasureTableRewards(HeadquartersState.Headquarters.StartupTreasureTables, HighestUnlockedChapter, GetHighestUnlockedSubChapter(HighestUnlockedChapter)), MapParty.ScenarioLevel, HighestUnlockedChapter, GetHighestUnlockedSubChapter(HighestUnlockedChapter)));
				ApplyRewards(StartupRewardGroup.Rewards, "party");
			}
			IntroCompleted = true;
			SimpleLog.AddToSimpleLog("MapRNG (Startup Treasure Tables rolled): " + AdventureState.MapState.PeekMapRNG);
		}
	}

	private void CheckForStartupTreasureTableUpdates()
	{
		if (StartupTreasureTablesRewardGenerationRNGState == null || StartupRewardGroup == null)
		{
			StartupTreasureTablesRewardGenerationRNGState = MapRNG.Save();
			SharedLibrary.Random rng = StartupTreasureTablesRewardGenerationRNGState.Restore();
			StartupRewardGroup = new RewardGroup(TreasureTableProcessing.RollTreasureTables(rng, MapYMLShared.ValidateTreasureTableRewards(HeadquartersState.Headquarters.StartupTreasureTables, HighestUnlockedChapter, GetHighestUnlockedSubChapter(HighestUnlockedChapter)), MapParty.ScenarioLevel, HighestUnlockedChapter, GetHighestUnlockedSubChapter(HighestUnlockedChapter)));
			List<Reward> list = new List<Reward>();
			foreach (Reward reward in StartupRewardGroup.Rewards)
			{
				if (reward.Type == ETreasureType.UnlockCharacter || (reward.Type == ETreasureType.UnlockProsperityItemStock && MapParty.GetCurrentGlobalItemCount(reward.Item) <= 0))
				{
					list.Add(reward);
				}
			}
			ApplyRewards(list, "party", battleGoal: false, changeItemUnlockToGold: false);
			return;
		}
		RewardGroup rewardGroup = new RewardGroup(TreasureTableProcessing.RollTreasureTables(StartupTreasureTablesRewardGenerationRNGState.Restore(), MapYMLShared.ValidateTreasureTableRewards(HeadquartersState.Headquarters.StartupTreasureTables, 1, 0), 0, 1, 0));
		if (rewardGroup.Equals(StartupRewardGroup))
		{
			return;
		}
		List<Reward> list2 = new List<Reward>();
		foreach (Reward reward2 in rewardGroup.Rewards)
		{
			if (reward2.Type == ETreasureType.UnlockCharacter || (reward2.Type == ETreasureType.UnlockProsperityItemStock && MapParty.GetCurrentGlobalItemCount(reward2.Item) <= 0))
			{
				list2.Add(reward2);
			}
		}
		ApplyRewards(list2, "party", battleGoal: false, changeItemUnlockToGold: false);
		StartupRewardGroup = rewardGroup;
	}

	public void RemoveOldFTUEScenarioStates()
	{
		if (TutorialCompleted && HeadquartersState != null && HeadquartersState.StartingScenarioStates != null)
		{
			for (int num = HeadquartersState.StartingScenarioStates.Count - 1; num >= 0; num--)
			{
				CMapScenarioState cMapScenarioState = HeadquartersState.StartingScenarioStates[num];
				if (cMapScenarioState != null)
				{
					cMapScenarioState.RemoveAssociatedDataFromMapState();
				}
				else
				{
					DLLDebug.LogWarning("Found a null entry in HeadquartersState.StartingScenarioStates, removing it as the tutorial is completed already anyway");
					HeadquartersState.StartingScenarioStates.Remove(cMapScenarioState);
				}
			}
		}
		if (!IntroCompleted || HeadquartersState == null || HeadquartersState.Headquarters.TutorialQuestNames == null)
		{
			return;
		}
		foreach (string questID in HeadquartersState.Headquarters.TutorialQuestNames)
		{
			CQuestState cQuestState = AllQuests.SingleOrDefault((CQuestState q) => q.ID == questID);
			if (cQuestState != null && cQuestState.QuestState == CQuestState.EQuestState.Completed)
			{
				if (cQuestState.ScenarioState != null)
				{
					cQuestState.ScenarioState.RemoveAssociatedDataFromMapState();
				}
				else
				{
					DLLDebug.LogError("Old FTUE scenario state is null");
				}
			}
		}
	}

	public void SkipFTUE(bool skipTutorial = false, bool skipIntro = false)
	{
		if (Thread.CurrentThread != MapRuleLibraryClient.Instance.MessageThreadHandler)
		{
			MapRuleLibraryClient.Instance.AddQueueMessage(new CSkipFTUE_MapDLLMessage(skipTutorial, skipIntro), processImmediately: false);
		}
		else
		{
			SkipFTUEInternal(skipTutorial, skipIntro);
		}
	}

	private void SkipFTUEInternal(bool skipTutorial = false, bool skipIntro = false)
	{
		if (HeadquartersState.Headquarters.StartingScenarios == null || skipTutorial)
		{
			m_PlayingTutorial = false;
			TutorialCompleted = true;
			foreach (CMapScenarioState startingScenarioState in HeadquartersState.StartingScenarioStates)
			{
				startingScenarioState.RemoveAssociatedDataFromMapState();
			}
		}
		if (!(skipTutorial && skipIntro))
		{
			return;
		}
		if (HeadquartersState.Headquarters.TutorialQuestNames.Count > 0)
		{
			foreach (string questID in HeadquartersState.Headquarters.TutorialQuestNames)
			{
				CQuestState cQuestState = AllQuests.SingleOrDefault((CQuestState q) => q.ID == questID);
				if (cQuestState != null && cQuestState.QuestState != CQuestState.EQuestState.Completed)
				{
					cQuestState.AutoComplete();
					cQuestState.ScenarioState.RemoveAssociatedDataFromMapState();
				}
			}
		}
		else
		{
			CheckFTUECompletion();
		}
		foreach (string messageID in HeadquartersState.Headquarters.TutorialMessages)
		{
			CMapMessageState cMapMessageState = MapMessageStates.SingleOrDefault((CMapMessageState m) => m.ID == messageID);
			if (cMapMessageState != null && cMapMessageState.MapMessageState != CMapMessageState.EMapMessageState.Shown)
			{
				cMapMessageState.MapMessageShown();
			}
		}
	}

	public void EnterScenario()
	{
		SimpleLog.AddToSimpleLog("Entering scenario. Diffculty: " + DifficultySetting);
		CurrentMapScenarioState.EnterScenario(GetGUIDBasedOnMapRNGState().ToString());
	}

	public void SetNextMapPhase(CMapPhase nextPhase)
	{
		if (CurrentMapPhase != null)
		{
			CurrentMapPhase.EndPhase();
		}
		CurrentMapPhase = nextPhase;
		CurrentMapPhase.NextStep();
	}

	public void ChangeDifficulty(EAdventureDifficulty difficulty)
	{
		if (difficulty != EAdventureDifficulty.None)
		{
			DifficultySetting = difficulty;
			m_Difficulty = GetDifficulty();
			return;
		}
		throw new Exception("Invalid difficulty supplied to ChangeDifficulty");
	}

	public void ChangeHouseRules(StateShared.EHouseRulesFlag houseRulesSetting)
	{
		HouseRulesSetting = houseRulesSetting;
		SimpleLog.AddToSimpleLog("[HOUSE RULES] - changed house rules settings in MapState to " + houseRulesSetting);
		ScenarioManager.SetHouseRules(HouseRulesSetting);
		ScenarioRuleClient.Reset();
	}

	private CAdventureDifficulty GetDifficulty()
	{
		List<CAdventureDifficulty> list = MapRuleLibraryClient.MRLYML.Headquarters.Difficulty.Where((CAdventureDifficulty w) => w.ActiveOn.Contains(DifficultySetting)).ToList();
		if (list.Count > 0)
		{
			return new CAdventureDifficulty("Combined Difficulty", null, positiveEffect: false, (list.Where((CAdventureDifficulty w) => w.HasThreatModifier).Count() > 0) ? list.Where((CAdventureDifficulty w) => w.HasThreatModifier).Max((CAdventureDifficulty m) => m.ThreatModifier) : 1f, (list.Where((CAdventureDifficulty w) => w.HasHeroHealthModifier).Count() > 0) ? list.Where((CAdventureDifficulty w) => w.HasHeroHealthModifier).Max((CAdventureDifficulty m) => m.HeroHealthModifier) : 1f, (list.Where((CAdventureDifficulty w) => w.HasXPModifier).Count() > 0) ? list.Where((CAdventureDifficulty w) => w.HasXPModifier).Max((CAdventureDifficulty m) => m.XPModifier) : 1f, (list.Where((CAdventureDifficulty w) => w.HasGoldModifier).Count() > 0) ? list.Where((CAdventureDifficulty w) => w.HasGoldModifier).Max((CAdventureDifficulty m) => m.GoldModifier) : 1f, (list.Where((CAdventureDifficulty w) => w.HasBlessCards).Count() > 0) ? list.Where((CAdventureDifficulty w) => w.HasBlessCards).Max((CAdventureDifficulty m) => m.BlessCards) : 0, (list.Where((CAdventureDifficulty w) => w.HasCurseCards).Count() > 0) ? list.Where((CAdventureDifficulty w) => w.HasCurseCards).Max((CAdventureDifficulty m) => m.CurseCards) : 0, (list.Where((CAdventureDifficulty w) => w.HasScenarioLevelModifier).Count() > 0) ? list.Where((CAdventureDifficulty w) => w.HasScenarioLevelModifier).Max((CAdventureDifficulty m) => m.ScenarioLevelModifier) : 0);
		}
		return new CAdventureDifficulty("Combined Difficulty", null, positiveEffect: false);
	}

	public List<Reward> RollTreasureChest(CObjectChest chestProp, string mapGUID, string characterName)
	{
		List<TreasureTable> list = null;
		List<string> list2 = null;
		CMap cMap = CurrentMapScenarioState.CurrentState.Maps.SingleOrDefault((CMap x) => x.MapGuid == mapGUID);
		if (cMap == null)
		{
			DLLDebug.LogError("Could not find treasure table for room " + mapGUID + " in map inputs for scenario " + CurrentMapScenarioState.CurrentState.ScenarioFileName + ".  Using Scenario TreasureTable.");
		}
		if (chestProp != null && chestProp.ChestTreasureTablesID != null && chestProp.ChestTreasureTablesID.Count > 0)
		{
			list2 = chestProp.ChestTreasureTablesID;
			list = CardProcessingShared.GetTreasureTables(chestProp.ChestTreasureTablesID, CurrentMapScenarioState.CurrentState.ScenarioFileName);
			AlreadyRewardedChestTreasureTableIDs.AddRange(list2);
			CheckTrophyAchievements(new CLootChest_AchievementTrigger());
		}
		else if (cMap != null && cMap.SelectedPossibleRoom.ChestTreasureTables != null && cMap.SelectedPossibleRoom.ChestTreasureTables.Count > 0)
		{
			list2 = cMap.SelectedPossibleRoom.ChestTreasureTables;
			list = CardProcessingShared.GetTreasureTables(cMap.SelectedPossibleRoom.ChestTreasureTables, CurrentMapScenarioState.CurrentState.ScenarioFileName);
		}
		else
		{
			list2 = CurrentMapScenarioState.CurrentState.ChestTreasureTables;
			list = CardProcessingShared.GetTreasureTables(CurrentMapScenarioState.CurrentState.ChestTreasureTables, CurrentMapScenarioState.CurrentState.ScenarioFileName);
		}
		if (chestProp.ObjectType != ScenarioManager.ObjectImportType.GoalChest)
		{
			list = MapYMLShared.ValidateTreasureTableRewards(list, HighestUnlockedChapter, GetHighestUnlockedSubChapter(HighestUnlockedChapter));
		}
		RewardGroup rewardGroup = new RewardGroup(TreasureTableProcessing.RollTreasureTables(CurrentMapScenarioState.CurrentState.ScenarioRNG, list, CurrentMapScenarioState.CurrentState.Level, HighestUnlockedChapter, GetHighestUnlockedSubChapter(HighestUnlockedChapter)), "Chest|" + string.Join(",", list.Select((TreasureTable x) => x.Name)));
		if (rewardGroup != null)
		{
			rewardGroup.PickedUpBy = characterName;
			foreach (Reward reward in rewardGroup.Rewards)
			{
				reward.GiveToCharacterType = ((GoldMode != EGoldMode.PartyGold) ? EGiveToCharacterType.Give : EGiveToCharacterType.None);
			}
			if (CurrentMapScenarioState.ChestRewards != null)
			{
				CurrentMapScenarioState.ChestRewards.Add(rewardGroup);
			}
			if (chestProp.ObjectType == ScenarioManager.ObjectImportType.GoalChest)
			{
				ScenarioManager.CurrentScenarioState.GoalChestRewards.Add(new Tuple<string, RewardGroup>("party", rewardGroup));
			}
			else
			{
				ScenarioManager.CurrentScenarioState.RoundChestRewards.Add(new Tuple<string, RewardGroup>(characterName, rewardGroup));
			}
			DLLDebug.Log(Reward.GenerateRewardsString(rewardGroup.Rewards, list2));
			return rewardGroup.Rewards;
		}
		return null;
	}

	public void ApplyRoundChestRewards()
	{
		if (ScenarioManager.CurrentScenarioState.RoundChestRewards == null)
		{
			return;
		}
		foreach (Tuple<string, RewardGroup> roundChestReward in ScenarioManager.CurrentScenarioState.RoundChestRewards)
		{
			ApplyRewards(roundChestReward.Item2.Rewards, roundChestReward.Item1);
		}
		ScenarioManager.CurrentScenarioState.RoundChestRewards.Clear();
	}

	public void ApplyRewards(List<RewardGroup> rewardGroups, bool battleGoal = false)
	{
		foreach (RewardGroup rewardGroup in rewardGroups)
		{
			ApplyRewards(rewardGroup.Rewards, rewardGroup.PickedUpBy, battleGoal);
		}
	}

	public string DetermineReward(Reward reward, int rewardAmount, string characterIDToUse, bool battleGoal = false, bool changeItemUnlockToGold = true, string characterName = null)
	{
		string text = string.Empty;
		switch (reward.Type)
		{
		case ETreasureType.Gold:
			switch (AdventureState.MapState.GoldMode)
			{
			case EGoldMode.PartyGold:
				MapParty.ModifyPartyGold(rewardAmount, useGoldModifier: true);
				break;
			case EGoldMode.CharacterGold:
				if (characterIDToUse == "party")
				{
					int num2 = MapParty.SelectedCharacters.Count();
					int num3 = rewardAmount;
					for (int num4 = 0; num4 < num2; num4++)
					{
						int num5 = num3 / (num2 - num4);
						int num6 = 0;
						num6 = ((num4 != num2 - 1) ? ((Math.Abs(num3) < Math.Abs(num5)) ? num3 : num5) : num3);
						CMapCharacter cMapCharacter3 = MapParty.SelectedCharacters.ToList()[num4];
						if (num6 < 0 && cMapCharacter3.CharacterGold < Math.Abs(num6))
						{
							num6 = -cMapCharacter3.CharacterGold;
						}
						cMapCharacter3.ModifyGold(num6, useGoldModifier: true);
						num3 -= num6;
					}
				}
				else
				{
					CMapCharacter cMapCharacter4 = null;
					cMapCharacter4 = ((characterName == null) ? MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter x) => x.CharacterID == characterIDToUse) : MapParty.CheckCharacters.SingleOrDefault((CMapCharacter x) => x.CharacterID == characterIDToUse && x.CharacterName == characterName));
					if (cMapCharacter4 != null)
					{
						cMapCharacter4.ModifyGold(rewardAmount, useGoldModifier: true);
					}
					else
					{
						DLLDebug.LogError("Character that earned gold was not in the Selected Characters");
					}
				}
				break;
			}
			text = text + characterIDToUse + "Earned Gold: " + rewardAmount + "\n";
			break;
		case ETreasureType.Item:
		case ETreasureType.UnlockProsperityItem:
		{
			if (rewardAmount <= 0)
			{
				break;
			}
			for (int num8 = 0; num8 < rewardAmount; num8++)
			{
				if (MapParty.GetCurrentGlobalItemCount(reward.Item) < AdventureState.MapState.HeadquartersState.Headquarters.GetMaxItemStock(reward.Item.YMLData.Rarity))
				{
					CMapCharacter cMapCharacter7 = null;
					if (characterName == null)
					{
						IEnumerable<CMapCharacter> source2;
						if (!IsCampaign)
						{
							IEnumerable<CMapCharacter> checkCharacters = MapParty.CheckCharacters;
							source2 = checkCharacters;
						}
						else
						{
							source2 = MapParty.SelectedCharacters;
						}
						cMapCharacter7 = source2.SingleOrDefault((CMapCharacter c) => c.CharacterID == characterIDToUse);
					}
					else
					{
						cMapCharacter7 = MapParty.CheckCharacters.SingleOrDefault((CMapCharacter c) => c.CharacterID == characterIDToUse && c.CharacterName == characterName);
					}
					if (!IsCampaign || cMapCharacter7 != null)
					{
						if (IsCampaign && cMapCharacter7.CheckBoundItems.FirstOrDefault((CItem i) => i.Name == reward.Item.Name) != null && reward.Item.YMLData.Slot != CItem.EItemSlot.SmallItem)
						{
							SimpleLog.AddToSimpleLog("(DetermineReward)" + ((reward.Type == ETreasureType.Item) ? "(Item) " : "(UnlockProsperityItem) ") + cMapCharacter7.CharacterName + " already had item " + reward.Item.Name + ", awarding " + reward.Item.SellPrice + " gold instead.");
							cMapCharacter7.ModifyGold(reward.Item.SellPrice, useGoldModifier: true);
							text = text + characterIDToUse + "Earned Gold: " + reward.Item.SellPrice + "\n";
							continue;
						}
						CItem cItem3 = reward.Item.Copy(GetGUIDBasedOnMapRNGState(), GetNextItemNetworkID());
						SimpleLog.AddToSimpleLog("(DetermineReward)" + ((reward.Type == ETreasureType.Item) ? "(Item)" : "(UnlockProsperityItem)") + " Adding new item " + cItem3.Name + " with NetworkID " + cItem3.NetworkID);
						MapParty.AddItem(cItem3);
						if (cMapCharacter7 != null && reward.GiveToCharacterType != EGiveToCharacterType.None)
						{
							MapParty.BindItem(cMapCharacter7.CharacterID, cMapCharacter7.CharacterName, cItem3);
							if (reward.GiveToCharacterType == EGiveToCharacterType.Equip && !IsInScenarioPhase)
							{
								SimpleLog.AddToSimpleLog("(DetermineReward)" + ((reward.Type == ETreasureType.Item) ? "(Item)" : "(UnlockProsperityItem)") + " equipping item " + cItem3.Name + " with NetworkID " + cItem3.NetworkID);
								cMapCharacter7.UnequipPreviouslyEquippedItems(cItem3, cItem3.SlotIndex);
								cMapCharacter7.EquipItem(cItem3);
							}
						}
						text = text + "Gained item: " + reward.Item.Name + "\n";
					}
					else if (!MapParty.CharactersRetiredThisSession.Exists((CMapCharacter x) => x.CharacterID == characterIDToUse && (characterName == null || x.CharacterName == characterName)))
					{
						DLLDebug.LogError("Attempted to give item to party instead of a specific character in Campaign mode. This is not supported");
					}
				}
				else if (AdventureState.MapState.HeadquartersState.CheckMerchantStock.Count((CItem a) => a.Name == reward.Item.Name) > 0)
				{
					IEnumerable<CMapCharacter> source3;
					if (!IsCampaign)
					{
						IEnumerable<CMapCharacter> checkCharacters = MapParty.CheckCharacters;
						source3 = checkCharacters;
					}
					else
					{
						source3 = MapParty.SelectedCharacters;
					}
					CMapCharacter cMapCharacter8 = source3.SingleOrDefault((CMapCharacter c) => c.CharacterID == characterIDToUse);
					if (!IsCampaign || cMapCharacter8 != null)
					{
						if (IsCampaign && cMapCharacter8.CheckBoundItems.FirstOrDefault((CItem i) => i.Name == reward.Item.Name) != null)
						{
							SimpleLog.AddToSimpleLog("(DetermineReward)" + ((reward.Type == ETreasureType.Item) ? "(Item) " : "(UnlockProsperityItem) ") + cMapCharacter8.CharacterName + " already had item " + reward.Item.Name + ", awarding " + reward.Item.SellPrice + " gold instead.");
							cMapCharacter8.ModifyGold(reward.Item.SellPrice, useGoldModifier: true);
							text = text + characterIDToUse + "Earned Gold: " + reward.Item.SellPrice + "\n";
							continue;
						}
						CItem cItem4 = AdventureState.MapState.HeadquartersState.CheckMerchantStock.FirstOrDefault((CItem a) => a.Name == reward.Item.Name);
						if (cItem4 == null)
						{
							continue;
						}
						AdventureState.MapState.HeadquartersState.RemoveItemFromMerchantStock(cItem4);
						SimpleLog.AddToSimpleLog("(DetermineReward)" + ((reward.Type == ETreasureType.Item) ? "(Item)" : "(UnlockProsperityItem)") + " moving item from merchant stock " + cItem4.Name + " with NetworkID " + cItem4.NetworkID);
						cItem4.IsNew = true;
						MapParty.AddItem(cItem4);
						if (cMapCharacter8 != null && reward.GiveToCharacterType != EGiveToCharacterType.None)
						{
							MapParty.BindItem(cMapCharacter8.CharacterID, cMapCharacter8.CharacterName, cItem4);
							if (reward.GiveToCharacterType == EGiveToCharacterType.Equip && !IsInScenarioPhase)
							{
								SimpleLog.AddToSimpleLog("(DetermineReward)" + ((reward.Type == ETreasureType.Item) ? "(Item)" : "(UnlockProsperityItem)") + " equipping item " + cItem4.Name + " with NetworkID " + cItem4.NetworkID);
								cMapCharacter8.UnequipPreviouslyEquippedItems(cItem4, cItem4.SlotIndex);
								cMapCharacter8.EquipItem(cItem4);
							}
						}
						text = text + "Gained item: " + reward.Item.Name + "\n";
					}
					else
					{
						DLLDebug.LogError("Attempted to give item to party instead of a specific character in Campaign mode. This is not supported");
					}
				}
				else if (IsCampaign)
				{
					IEnumerable<CMapCharacter> source4;
					if (!IsCampaign)
					{
						IEnumerable<CMapCharacter> checkCharacters = MapParty.CheckCharacters;
						source4 = checkCharacters;
					}
					else
					{
						source4 = MapParty.SelectedCharacters;
					}
					CMapCharacter cMapCharacter9 = source4.SingleOrDefault((CMapCharacter c) => c.CharacterID == characterIDToUse);
					if (cMapCharacter9 != null)
					{
						SimpleLog.AddToSimpleLog("(ApplyRewards)" + ((reward.Type == ETreasureType.Item) ? "(Item) " : "(UnlockProsperityItem) ") + cMapCharacter9.CharacterName + " already had item " + reward.Item.Name + ", awarding " + reward.Item.SellPrice + " gold instead.");
						cMapCharacter9.ModifyGold(reward.Item.SellPrice, useGoldModifier: true);
						text = text + characterIDToUse + "Earned Gold: " + reward.Item.SellPrice + "\n";
					}
				}
			}
			break;
		}
		case ETreasureType.ItemStock:
		case ETreasureType.UnlockProsperityItemStock:
		{
			if (rewardAmount <= 0)
			{
				break;
			}
			for (int num7 = 0; num7 < rewardAmount; num7++)
			{
				if (MapParty.GetCurrentGlobalItemCount(reward.Item) < AdventureState.MapState.HeadquartersState.Headquarters.GetMaxItemStock(reward.Item.YMLData.Rarity))
				{
					CItem cItem2 = reward.Item.Copy(GetGUIDBasedOnMapRNGState(), GetNextItemNetworkID());
					SimpleLog.AddToSimpleLog("(ApplyRewards) (UnlockProsperityItemStock) Adding new item " + cItem2.Name + " with NetworkID " + cItem2.NetworkID);
					HeadquartersState.AddItemToMerchantStock(cItem2);
					text = text + "Awarded " + rewardAmount + " stock of item " + reward.Item.Name + "\n";
					CContentUnlocked_MapClientMessage message4 = new CContentUnlocked_MapClientMessage(reward.Item.Name, "item_stock", null);
					if (MapRuleLibraryClient.Instance.MessageHandler != null)
					{
						MapRuleLibraryClient.Instance.MessageHandler(message4);
					}
					else
					{
						DLLDebug.LogWarning("Message handler not set");
					}
				}
				else if (changeItemUnlockToGold)
				{
					reward.Type = ETreasureType.Gold;
					reward.Amount = reward.Item.SellPrice;
					text += DetermineReward(reward, reward.Amount, characterIDToUse, battleGoal);
				}
			}
			break;
		}
		case ETreasureType.XP:
			if (characterIDToUse == "party")
			{
				foreach (CMapCharacter selectedCharacter in MapParty.SelectedCharacters)
				{
					selectedCharacter.GainEXP(rewardAmount, Difficulty.HasXPModifier ? Difficulty.XPModifier : 1f);
				}
			}
			else
			{
				CMapCharacter cMapCharacter11 = null;
				cMapCharacter11 = ((characterName == null) ? MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter x) => x.CharacterID == characterIDToUse) : MapParty.CheckCharacters.SingleOrDefault((CMapCharacter x) => x.CharacterID == characterIDToUse && x.CharacterName == characterName));
				if (cMapCharacter11 != null)
				{
					cMapCharacter11.GainEXP(rewardAmount, Difficulty.HasXPModifier ? Difficulty.XPModifier : 1f);
				}
				else if (!MapParty.CharactersRetiredThisSession.Exists((CMapCharacter x) => x.CharacterID == characterIDToUse && (characterName == null || x.CharacterName == characterName)))
				{
					DLLDebug.LogError("Character that earned xp was not in the Selected Characters");
				}
			}
			text = text + "Each selected character earned " + rewardAmount + " XP\n";
			break;
		case ETreasureType.Condition:
			foreach (RewardCondition condition in reward.Conditions)
			{
				switch (condition.MapDuration)
				{
				case RewardCondition.EConditionMapDuration.NextScenario:
				case RewardCondition.EConditionMapDuration.NextVillage:
				{
					CMapCharacter cMapCharacter10 = MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter c) => c.CharacterID == characterIDToUse);
					if (cMapCharacter10 != null)
					{
						if (condition.Type == RewardCondition.EConditionType.Negative)
						{
							if (CurrentMapPhaseType == EMapPhaseType.InHQ)
							{
								cMapCharacter10.GainNextScenarioCondition(new NegativeConditionPair(condition.NegativeCondition, condition.MapDuration, condition.RoundDuration, EConditionDecTrigger.Turns));
							}
							else
							{
								cMapCharacter10.GainTempCondition(new NegativeConditionPair(condition.NegativeCondition, condition.MapDuration, condition.RoundDuration, EConditionDecTrigger.Turns));
							}
							text = text + "Character " + cMapCharacter10.CharacterID + " has had the condition " + condition.NegativeCondition.ToString() + " applied until " + condition.MapDuration.ToString() + "\n";
						}
						else if (condition.Type == RewardCondition.EConditionType.Positive)
						{
							if (CurrentMapPhaseType == EMapPhaseType.InHQ)
							{
								cMapCharacter10.GainNextScenarioCondition(new PositiveConditionPair(condition.PositiveCondition, condition.MapDuration, condition.RoundDuration, EConditionDecTrigger.Turns));
							}
							else
							{
								cMapCharacter10.GainTempCondition(new PositiveConditionPair(condition.PositiveCondition, condition.MapDuration, condition.RoundDuration, EConditionDecTrigger.Turns));
							}
							text = text + "Character " + cMapCharacter10.CharacterID + " has had the condition " + condition.PositiveCondition.ToString() + " applied until " + condition.MapDuration.ToString() + "\n";
						}
						break;
					}
					foreach (CMapCharacter selectedCharacter2 in MapParty.SelectedCharacters)
					{
						if (condition.Type == RewardCondition.EConditionType.Negative)
						{
							if (CurrentMapPhaseType == EMapPhaseType.InHQ)
							{
								selectedCharacter2.GainNextScenarioCondition(new NegativeConditionPair(condition.NegativeCondition, condition.MapDuration, condition.RoundDuration, EConditionDecTrigger.Turns));
							}
							else
							{
								selectedCharacter2.GainTempCondition(new NegativeConditionPair(condition.NegativeCondition, condition.MapDuration, condition.RoundDuration, EConditionDecTrigger.Turns));
							}
							text = text + "Character " + selectedCharacter2.CharacterID + " has had the condition " + condition.NegativeCondition.ToString() + " applied until " + condition.MapDuration.ToString() + "\n";
						}
						else if (condition.Type == RewardCondition.EConditionType.Positive)
						{
							if (CurrentMapPhaseType == EMapPhaseType.InHQ)
							{
								selectedCharacter2.GainNextScenarioCondition(new PositiveConditionPair(condition.PositiveCondition, condition.MapDuration, condition.RoundDuration, EConditionDecTrigger.Turns));
							}
							else
							{
								selectedCharacter2.GainTempCondition(new PositiveConditionPair(condition.PositiveCondition, condition.MapDuration, condition.RoundDuration, EConditionDecTrigger.Turns));
							}
							text = text + "Character " + selectedCharacter2.CharacterID + " has had the condition " + condition.PositiveCondition.ToString() + " applied until " + condition.MapDuration.ToString() + "\n";
						}
					}
					break;
				}
				case RewardCondition.EConditionMapDuration.Now:
					DLLDebug.LogError("Unable to apply condition Now. This can only be done within a scenario.");
					break;
				default:
					DLLDebug.LogError("Unable to process condition with duration " + condition.MapDuration);
					break;
				}
			}
			break;
		case ETreasureType.EnemyCondition:
			if (MapParty.NextScenarioEffects == null)
			{
				break;
			}
			foreach (RewardCondition enemyCondition in reward.EnemyConditions)
			{
				if (enemyCondition.Type == RewardCondition.EConditionType.Negative)
				{
					if (MapParty.NextScenarioEffects.EnemyNegativeConditions == null)
					{
						MapParty.NextScenarioEffects.EnemyNegativeConditions = new List<NegativeConditionPair>();
					}
					MapParty.NextScenarioEffects.EnemyNegativeConditions.Add(new NegativeConditionPair(enemyCondition.NegativeCondition, enemyCondition.MapDuration, enemyCondition.RoundDuration, EConditionDecTrigger.Turns));
					text = text + "Enemy condition " + enemyCondition.NegativeCondition.ToString() + " added.\n";
				}
				else if (enemyCondition.Type == RewardCondition.EConditionType.Positive)
				{
					if (MapParty.NextScenarioEffects.EnemyPositiveConditions == null)
					{
						MapParty.NextScenarioEffects.EnemyPositiveConditions = new List<PositiveConditionPair>();
					}
					MapParty.NextScenarioEffects.EnemyPositiveConditions.Add(new PositiveConditionPair(enemyCondition.PositiveCondition, enemyCondition.MapDuration, enemyCondition.RoundDuration, EConditionDecTrigger.Turns));
					text = text + "Enemy condition " + enemyCondition.PositiveCondition.ToString() + " added.\n";
				}
			}
			break;
		case ETreasureType.Enhancement:
			if (reward.Enhancement != EEnhancement.NoEnhancement && !HeadquartersState.EnhancerStock.Contains(reward.Enhancement))
			{
				HeadquartersState.EnhancerStock.Add(reward.Enhancement);
				HeadquartersState.EnhancerHasNewStock = true;
				text = text + "Incremented stock of enhancement: " + reward.Enhancement.ToString() + " \n";
			}
			break;
		case ETreasureType.Perk:
		{
			IEnumerable<CMapCharacter> source;
			if (!IsCampaign)
			{
				IEnumerable<CMapCharacter> checkCharacters = MapParty.CheckCharacters;
				source = checkCharacters;
			}
			else
			{
				source = MapParty.SelectedCharacters;
			}
			CMapCharacter cMapCharacter5 = source.SingleOrDefault((CMapCharacter x) => x.CharacterID == reward.Perk.CharacterID);
			if (cMapCharacter5 != null)
			{
				CharacterPerk characterPerk = cMapCharacter5.Perks.SingleOrDefault((CharacterPerk x) => x.Perk.Name == reward.Perk.Name);
				if (characterPerk != null)
				{
					characterPerk.IsActive = true;
					text = text + "Added Perk named " + reward.Perk.Name + " for Character " + cMapCharacter5.CharacterID + " \n";
				}
				else
				{
					DLLDebug.LogError("No perk named " + reward.Perk.Name + " found for character " + cMapCharacter5.CharacterID);
				}
			}
			break;
		}
		case ETreasureType.Infuse:
			if (MapParty.NextScenarioEffects != null)
			{
				MapParty.NextScenarioEffects.Infusions.AddRange(reward.Infusions);
				text = text + "NextScenarioEffects Infusions: " + string.Join(",", reward.Infusions) + "\n";
			}
			break;
		case ETreasureType.Damage:
			if (rewardAmount > 0 && MapParty.NextScenarioEffects != null)
			{
				MapParty.NextScenarioEffects.Damage = rewardAmount;
				text = text + "Each character starts the next scenario suffering " + MapParty.NextScenarioEffects.Damage + " damage\n";
			}
			break;
		case ETreasureType.Discard:
			if (rewardAmount > 0 && MapParty.NextScenarioEffects != null)
			{
				MapParty.NextScenarioEffects.Discard += rewardAmount;
				text = text + "At the start of the next scenario, each character discards " + MapParty.NextScenarioEffects.Discard + " cards\n";
			}
			break;
		case ETreasureType.LoseItem:
		{
			if (rewardAmount <= 0)
			{
				break;
			}
			for (int num = 0; num < rewardAmount; num++)
			{
				if (characterIDToUse != "party")
				{
					CMapCharacter cMapCharacter2 = MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter c) => c.CharacterID == characterIDToUse);
					if (cMapCharacter2 != null)
					{
						CItem cItem = null;
						cItem = cMapCharacter2.CheckBoundItems.FirstOrDefault((CItem it) => it.ID == reward.ItemID);
						if (cItem == null)
						{
							cItem = cMapCharacter2.CheckEquippedItems.First((CItem it) => it.ID == reward.ItemID);
						}
						if (cItem != null)
						{
							MapParty.RemoveItem(cItem, cMapCharacter2);
							AdventureState.MapState.HeadquartersState.AddItemToMerchantStock(cItem);
						}
						else
						{
							DLLDebug.LogError("No item to lose named " + reward.ItemID + " found for character " + characterIDToUse);
						}
					}
				}
				else
				{
					DLLDebug.LogError("Losing items not implemented for party");
				}
				text = text + "Lost item: " + reward.Item.Name + "\n";
			}
			break;
		}
		case ETreasureType.ConsumeItem:
			if (reward.ConsumeSlot != null && MapParty.NextScenarioEffects != null)
			{
				if (MapParty.NextScenarioEffects.ConsumeItems == null)
				{
					MapParty.NextScenarioEffects.ConsumeItems = new List<Tuple<string, string>>();
				}
				MapParty.NextScenarioEffects.ConsumeItems.Add(new Tuple<string, string>(characterIDToUse, reward.ConsumeSlot));
				text = ((!(characterIDToUse == "party")) ? (text + "At the start of the next scenario, character: " + characterIDToUse + " consumes " + reward.ConsumeSlot + " item\n") : (text + "At the start of the next scenario, each character consumes " + reward.ConsumeSlot + " item\n"));
			}
			break;
		case ETreasureType.AddModifiers:
			if (reward.Modifiers != null && MapParty.NextScenarioEffects != null)
			{
				MapParty.NextScenarioEffects.AttackModifiers.Add(new Tuple<string, Dictionary<string, int>>(characterIDToUse, reward.Modifiers));
				text += "At the start of the next scenario, each character adds attack modifiers\n";
			}
			break;
		case ETreasureType.Prosperity:
		{
			int val = 0;
			if (AdventureState.MapState.HeadquartersState.Headquarters.LevelToProsperity != null)
			{
				foreach (int item in AdventureState.MapState.HeadquartersState.Headquarters.LevelToProsperity)
				{
					if (item <= MapParty.ProsperityXP)
					{
						val = item;
					}
				}
			}
			int prosperityLevel = MapParty.ProsperityLevel;
			MapParty.UpdateProsperityXP(Math.Max(val, MapParty.ProsperityXP + rewardAmount));
			reward.LevelUp = prosperityLevel != MapParty.ProsperityLevel;
			text = text + "Earned Prosperity: " + rewardAmount + "\n";
			break;
		}
		case ETreasureType.Reputation:
			MapParty.UpdateReputation(MapParty.Reputation + rewardAmount);
			text = text + ((rewardAmount < 0) ? "Lost " : "Earned ") + "Reputation: " + rewardAmount + "\n";
			break;
		case ETreasureType.UnlockLocation:
		{
			CLocationState cLocationState = AllLocations.SingleOrDefault((CLocationState x) => x.ID == reward.UnlockName);
			if (cLocationState != null)
			{
				cLocationState.UnlockLocation();
				text = text + "Unlocked Location: " + cLocationState.ID + "\n";
			}
			break;
		}
		case ETreasureType.UnlockMerchant:
			if (reward.Unlock)
			{
				HeadquartersState.UnlockMerchant();
				text += "Unlocked Marchant\n";
			}
			break;
		case ETreasureType.UnlockEnhancer:
			if (reward.Unlock)
			{
				HeadquartersState.UnlockEnhancer();
				text += "Unlocked Enhancer\n";
			}
			break;
		case ETreasureType.UnlockTrainer:
			if (reward.Unlock)
			{
				HeadquartersState.UnlockTrainer();
				text += "Unlocked Trainer\n";
			}
			break;
		case ETreasureType.UnlockTemple:
			if (reward.Unlock)
			{
				HeadquartersState.UnlockTemple();
				text += "Unlocked Temple\n";
			}
			break;
		case ETreasureType.UnlockQuest:
		{
			CQuestState cQuestState = AllQuests.SingleOrDefault((CQuestState x) => x.ID == reward.UnlockName);
			if (cQuestState != null && cQuestState.QuestState == CQuestState.EQuestState.Locked)
			{
				cQuestState.UnlockQuest();
				text = text + "Unlocked Quest: " + cQuestState.ID + "\n";
			}
			break;
		}
		case ETreasureType.UnlockAchievement:
		{
			CPartyAchievement cPartyAchievement = MapParty.Achievements.SingleOrDefault((CPartyAchievement x) => x.ID == reward.UnlockName);
			if (cPartyAchievement != null && cPartyAchievement.State == EAchievementState.Locked)
			{
				if (IsCampaign)
				{
					cPartyAchievement.CompleteAchievement();
					text = text + "Completed Achievement: " + cPartyAchievement.ID + "\n";
				}
				else
				{
					cPartyAchievement.UnlockAchievement();
					text = text + "Unlocked Achievement: " + cPartyAchievement.ID + "\n";
				}
			}
			break;
		}
		case ETreasureType.LockAchievement:
			MapParty.Achievements.SingleOrDefault((CPartyAchievement x) => x.ID == reward.UnlockName)?.LockAchievement();
			break;
		case ETreasureType.CityEvent:
			SimpleLog.AddToSimpleLog("[CITY EVENT DECK]: Adding unlocked event card: " + reward.UnlockName);
			MapParty.CityEventDeck.AddCard(reward.UnlockName);
			SimpleLog.AddToSimpleLog("[CITY EVENT DECK]: Shuffling - DeckOnly Mode");
			MapParty.CityEventDeck.Shuffle(CCardDeck.EShuffle.DeckOnly);
			SimpleLog.AddToSimpleLog("[CITY EVENT DECK]: City Event Deck Top 5 Cards: " + MapParty.CityEventDeck.GetEventCardsOnTop(5));
			break;
		case ETreasureType.RoadEvent:
			SimpleLog.AddToSimpleLog("[ROAD EVENT DECK]: Adding unlocked event card: " + reward.UnlockName);
			MapParty.RoadEventDeck.AddCard(reward.UnlockName);
			SimpleLog.AddToSimpleLog("[ROAD EVENT DECK]: Shuffling - DeckOnly Mode");
			MapParty.RoadEventDeck.Shuffle(CCardDeck.EShuffle.DeckOnly);
			SimpleLog.AddToSimpleLog("[ROAD EVENT DECK]: Road Event Deck Top 5 Cards: " + MapParty.RoadEventDeck.GetEventCardsOnTop(5));
			break;
		case ETreasureType.UnlockChapter:
		{
			CChapter newChapter = new CChapter(rewardAmount, reward.SubChapter);
			if (!UnlockedChapters.Any((CChapter c) => c.Chapter == newChapter.Chapter && c.SubChapter == newChapter.SubChapter))
			{
				UnlockedChapters.Add(new CChapter(rewardAmount, reward.SubChapter));
				text = text + "Unlock Chapter: " + newChapter.Chapter + " SubChapter: " + newChapter.SubChapter + "\n";
				CContentUnlocked_MapClientMessage message2 = new CContentUnlocked_MapClientMessage("Chapter: " + newChapter.Chapter + "." + newChapter.SubChapter, "chapter", null);
				if (MapRuleLibraryClient.Instance.MessageHandler != null)
				{
					MapRuleLibraryClient.Instance.MessageHandler(message2);
				}
				else
				{
					DLLDebug.LogWarning("Message handler not set");
				}
			}
			break;
		}
		case ETreasureType.UnlockCharacter:
			if (MapParty.UnlockedCharacterIDs.Contains(reward.CharacterID))
			{
				break;
			}
			MapParty.UnlockedCharacter(reward.CharacterID);
			text = text + "Unlocked Character: " + reward.CharacterID + "\n";
			if (!IsCampaign)
			{
				foreach (CCharacterClass @class in CharacterClassManager.Classes)
				{
					if (!(@class.CharacterID == reward.CharacterID))
					{
						continue;
					}
					CMapCharacter cMapCharacter12 = new CMapCharacter(@class.CharacterID, string.Empty, 0);
					MapParty.AddCharacterToCharactersList(cMapCharacter12);
					CUnlockCharacter_MapClientMessage message6 = new CUnlockCharacter_MapClientMessage(cMapCharacter12);
					if (MapRuleLibraryClient.Instance.MessageHandler != null)
					{
						MapRuleLibraryClient.Instance.MessageHandler(message6);
					}
					else
					{
						DLLDebug.LogWarning("Message handler not set");
					}
					if (MapParty.SelectedCharacters.Count() < 2 && !MapParty.SelectedCharacters.Contains(cMapCharacter12))
					{
						MapParty.AddSelectedCharacterToNextOpenSlot(cMapCharacter12);
						CRegenerateAllMapScenarios_MapClientMessage message7 = new CRegenerateAllMapScenarios_MapClientMessage();
						if (MapRuleLibraryClient.Instance.MessageHandler != null)
						{
							MapRuleLibraryClient.Instance.MessageHandler(message7);
						}
						else
						{
							DLLDebug.LogWarning("Message handler not set");
						}
					}
					break;
				}
			}
			CheckNonTrophyAchievements();
			break;
		case ETreasureType.EnhancementSlots:
		{
			HeadquartersState.EnhancementSlots += rewardAmount;
			text = text + "Unlock EnhancementSlots: " + rewardAmount + "\n";
			CContentUnlocked_MapClientMessage message5 = new CContentUnlocked_MapClientMessage("EnhancementSlot", "enhancement_slot", null);
			if (MapRuleLibraryClient.Instance.MessageHandler != null)
			{
				MapRuleLibraryClient.Instance.MessageHandler(message5);
			}
			else
			{
				DLLDebug.LogWarning("Message handler not set");
			}
			break;
		}
		case ETreasureType.UnlockPartyUI:
			if (reward.Unlock)
			{
				HeadquartersState.UnlockPartyUI();
				text += "Unlocked PartyUI\n";
			}
			break;
		case ETreasureType.UnlockMultiplayer:
			if (reward.Unlock)
			{
				HeadquartersState.UnlockMultiplayer();
				text += "Unlocked Multiplayer\n";
			}
			break;
		case ETreasureType.PerkCheck:
		{
			IEnumerable<CMapCharacter> enumerable3;
			if (!IsCampaign)
			{
				IEnumerable<CMapCharacter> checkCharacters = MapParty.CheckCharacters;
				enumerable3 = checkCharacters;
			}
			else
			{
				enumerable3 = MapParty.SelectedCharacters;
			}
			IEnumerable<CMapCharacter> enumerable4 = enumerable3;
			if (characterIDToUse != "party")
			{
				CMapCharacter cMapCharacter6 = enumerable4.SingleOrDefault((CMapCharacter c) => c.CharacterID == characterIDToUse);
				if (cMapCharacter6 != null)
				{
					cMapCharacter6.UpdatePerkChecks(Math.Max(0, cMapCharacter6.PerkChecks + rewardAmount), rewardAmount, battleGoal);
				}
				else
				{
					DLLDebug.LogError("Could not find character that earned perk check");
				}
			}
			else
			{
				foreach (CMapCharacter item2 in enumerable4)
				{
					item2.UpdatePerkChecks(Math.Max(0, item2.PerkChecks + rewardAmount), rewardAmount, battleGoal);
					text = text + "Unlock PerkCheck: " + rewardAmount + " for Character: " + item2.ToString() + "\n";
				}
			}
			CContentUnlocked_MapClientMessage message3 = new CContentUnlocked_MapClientMessage("PerkCheck", "perk_check", null);
			if (MapRuleLibraryClient.Instance.MessageHandler != null)
			{
				MapRuleLibraryClient.Instance.MessageHandler(message3);
			}
			else
			{
				DLLDebug.LogWarning("Message handler not set");
			}
			break;
		}
		case ETreasureType.PerkPoint:
		{
			IEnumerable<CMapCharacter> enumerable;
			if (!IsCampaign)
			{
				IEnumerable<CMapCharacter> checkCharacters = MapParty.CheckCharacters;
				enumerable = checkCharacters;
			}
			else
			{
				enumerable = MapParty.SelectedCharacters;
			}
			IEnumerable<CMapCharacter> enumerable2 = enumerable;
			if (characterIDToUse != "party")
			{
				CMapCharacter cMapCharacter = enumerable2.SingleOrDefault((CMapCharacter c) => c.CharacterID == characterIDToUse);
				if (cMapCharacter != null)
				{
					cMapCharacter.UpdatePerkPoints(Math.Max(0, cMapCharacter.PerkPoints + rewardAmount));
				}
				else
				{
					DLLDebug.LogError("Could not find character that earned perk point");
				}
			}
			else
			{
				foreach (CMapCharacter item3 in enumerable2)
				{
					item3.UpdatePerkPoints(Math.Max(0, item3.PerkPoints + rewardAmount));
					text = text + "Unlock PerkPoints: " + rewardAmount + " for Character: " + item3.ToString() + "\n";
				}
			}
			CContentUnlocked_MapClientMessage message = new CContentUnlocked_MapClientMessage("PerkPoint", "perk_point", null);
			if (MapRuleLibraryClient.Instance.MessageHandler != null)
			{
				MapRuleLibraryClient.Instance.MessageHandler(message);
			}
			else
			{
				DLLDebug.LogWarning("Message handler not set");
			}
			break;
		}
		default:
			DLLDebug.LogError("Unsupported Treasure Type: " + reward.Type);
			break;
		case ETreasureType.ItemDesign:
			break;
		}
		return text;
	}

	public void ApplyRewards(List<Reward> rewards, string characterID, bool battleGoal = false, bool changeItemUnlockToGold = true, string characterName = null)
	{
		string text = string.Empty;
		foreach (Reward reward in rewards)
		{
			int num = reward.Amount;
			if (reward.TreasureDistributionType == ETreasureDistributionType.PerMercenaryInParty && reward.Type != ETreasureType.XP)
			{
				num *= Math.Max(1, MapParty.SelectedCharacters.Count());
			}
			string characterIDToUse = ((characterID != null) ? characterID : "party");
			if (reward.GiveToCharacterID != null && reward.GiveToCharacterID != "NoneID")
			{
				characterIDToUse = reward.GiveToCharacterID;
			}
			else if (reward.GiveToCharacterRequirement != EGiveToCharacterRequirement.None && !string.IsNullOrEmpty(reward.GiveToCharacterRequirementCheckID) && reward.GiveToCharacterRequirement == EGiveToCharacterRequirement.CharacterHasPersonalQuest)
			{
				CMapCharacter cMapCharacter = MapParty.SelectedCharacters.FirstOrDefault((CMapCharacter x) => x.PersonalQuest.ID == reward.GiveToCharacterRequirementCheckID);
				if (cMapCharacter == null)
				{
					DLLDebug.LogError("Unable to find a character that fits the reward requirement in selected characters.");
					return;
				}
				characterIDToUse = cMapCharacter.CharacterID;
			}
			text += DetermineReward(reward, num, characterIDToUse, battleGoal, changeItemUnlockToGold, characterName);
		}
		if (text != string.Empty)
		{
			DLLDebug.Log(text);
		}
		if (IsCampaign && !IsInScenarioPhase)
		{
			CheckNonTrophyAchievements();
		}
	}

	public void CheckAllLockedVillages()
	{
		foreach (CLocationState allVillage in AllVillages)
		{
			if (allVillage.LocationState == ELocationState.Locked && allVillage.UnlockConditionState != null && allVillage.UnlockConditionState.IsUnlocked())
			{
				allVillage.UnlockLocation();
				if (allVillage is CVillageState cVillageState)
				{
					cVillageState.ConnectVillage(AdventureState.MapState.HeadquartersState.ID);
				}
			}
		}
	}

	public void CheckAllLockedQuests()
	{
		foreach (CQuestState allLockedQuest in AllLockedQuests)
		{
			if (allLockedQuest.UnlockConditionState.IsUnlocked())
			{
				allLockedQuest.UnlockQuest();
			}
		}
	}

	public void CheckForBlockedQuests()
	{
		foreach (CQuestState allBlockedQuest in AllBlockedQuests)
		{
			if (allBlockedQuest.HasBlockedCondition && !allBlockedQuest.BlockedConditionState.IsUnlocked())
			{
				DLLDebug.LogWarning("Quest " + allBlockedQuest.ID + " is no longer blocked.");
				allBlockedQuest.LockQuest();
				if (allBlockedQuest.UnlockConditionState.IsUnlocked())
				{
					allBlockedQuest.UnlockQuest();
				}
			}
		}
		foreach (CQuestState allLockedQuest in AllLockedQuests)
		{
			if (allLockedQuest.HasBlockedCondition && allLockedQuest.BlockedConditionState.IsUnlocked())
			{
				DLLDebug.LogWarning("Quest " + allLockedQuest.ID + " is blocked.");
				allLockedQuest.BlockQuest();
			}
		}
		foreach (CQuestState allUnlockedQuest in AllUnlockedQuests)
		{
			if (allUnlockedQuest.HasBlockedCondition && allUnlockedQuest.BlockedConditionState.IsUnlocked())
			{
				DLLDebug.LogWarning("Quest " + allUnlockedQuest.ID + " is blocked.");
				allUnlockedQuest.BlockQuest();
				QueuedUnlockedQuestIDs.Remove(allUnlockedQuest.ID);
			}
		}
	}

	public void ResetAchievementsProgress()
	{
		foreach (CPartyAchievement item in MapParty.Achievements.Where(delegate(CPartyAchievement a)
		{
			AchievementYMLData achievement = a.Achievement;
			return achievement != null && achievement.AchievementType == EAchievementType.Trophy && a.State == EAchievementState.Completed;
		}).ToList())
		{
			item.AchievementConditionState.ResetProgress();
			item.LockAchievement();
		}
	}

	public void CheckTrophyAchievements(CAchievementTrigger trigger)
	{
		if (MapParty.Achievements == null)
		{
			return;
		}
		if (HeadquartersState?.Headquarters?.StartingScenarios != null)
		{
			CHeadquartersState headquartersState = HeadquartersState;
			if ((headquartersState == null || headquartersState.Headquarters?.StartingScenarios.Count != 0) && HeadquartersState?.Headquarters?.TutorialQuestNames != null)
			{
				CHeadquartersState headquartersState2 = HeadquartersState;
				if ((headquartersState2 == null || headquartersState2.Headquarters?.TutorialQuestNames.Count != 0) && (!TutorialCompleted || !IntroCompleted))
				{
					return;
				}
			}
		}
		foreach (CPartyAchievement item in MapParty.Achievements.Where((CPartyAchievement a) => a.IsTriggeredBy(trigger)))
		{
			item.CheckAchievement();
			if (item.State != EAchievementState.Completed || AdventureState.MapState.QueuedPlatformAchievementsToUnlock.Contains(item.ID))
			{
				continue;
			}
			if (AdventureState.MapState.IsInScenarioPhase)
			{
				AdventureState.MapState.QueuedPlatformAchievementsToUnlock.Add(item.ID);
				continue;
			}
			CPostTrophyAchievement_MapClientMessage message = new CPostTrophyAchievement_MapClientMessage(new List<string> { item.ID });
			if (MapRuleLibraryClient.Instance?.MessageHandler != null)
			{
				MapRuleLibraryClient.Instance.MessageHandler(message);
			}
			else
			{
				AdventureState.MapState.QueuedPlatformAchievementsToUnlock.Add(item.ID);
			}
		}
	}

	public void CheckNonTrophyAchievements(bool OverrideLock = false, string soloID = "")
	{
		if (MapParty.Achievements == null)
		{
			return;
		}
		if (HeadquartersState?.Headquarters?.StartingScenarios != null)
		{
			CHeadquartersState headquartersState = HeadquartersState;
			if ((headquartersState == null || headquartersState.Headquarters?.StartingScenarios.Count != 0) && HeadquartersState?.Headquarters?.TutorialQuestNames != null)
			{
				CHeadquartersState headquartersState2 = HeadquartersState;
				if ((headquartersState2 == null || headquartersState2.Headquarters?.TutorialQuestNames.Count != 0) && (!TutorialCompleted || !IntroCompleted))
				{
					return;
				}
			}
		}
		foreach (CPartyAchievement item in MapParty.Achievements.Where((CPartyAchievement a) => a.Achievement.AchievementType != EAchievementType.Trophy))
		{
			item.CheckAchievement(OverrideLock);
		}
	}

	public void CheckAchievementsForPlatformTrophies()
	{
		if (Thread.CurrentThread != MapRuleLibraryClient.Instance.MessageThreadHandler)
		{
			MapRuleLibraryClient.Instance.AddQueueMessage(new CMapDLLMessage(EMapDLLMessageType.CheckAchievementsForPlatformTrophies), processImmediately: false);
			return;
		}
		List<string> list = new List<string>();
		if (MapParty.Achievements != null)
		{
			foreach (CPartyAchievement achievement in MapParty.Achievements)
			{
				if (achievement.Achievement.AchievementType == EAchievementType.Trophy && achievement.State == EAchievementState.Completed)
				{
					list.Add(achievement.ID);
				}
			}
		}
		if (list.Count > 0)
		{
			CPostTrophyAchievement_MapClientMessage message = new CPostTrophyAchievement_MapClientMessage(list);
			if (MapRuleLibraryClient.Instance?.MessageHandler != null)
			{
				MapRuleLibraryClient.Instance.MessageHandler(message);
			}
			else
			{
				DLLDebug.LogWarning("Message handler not set");
			}
		}
	}

	public void CheckPersonalQuests(string soloID = "")
	{
		if (!IsCampaign)
		{
			return;
		}
		foreach (CMapCharacter selectedCharacter in MapParty.SelectedCharacters)
		{
			if (soloID == "" || selectedCharacter.CharacterID == soloID)
			{
				selectedCharacter.PersonalQuest?.CheckPersonalQuest(selectedCharacter.CharacterID);
			}
		}
	}

	public void AutoCompletePersonalQuests()
	{
		if (!IsCampaign)
		{
			return;
		}
		foreach (CMapCharacter selectedCharacter in MapParty.SelectedCharacters)
		{
			CPersonalQuestState personalQuest;
			do
			{
				personalQuest = selectedCharacter.PersonalQuest;
			}
			while (personalQuest != null && personalQuest.NextPersonalQuestStep());
			selectedCharacter.PersonalQuest?.CompletePersonalQuest(selectedCharacter.CharacterID);
		}
	}

	public void CheckAllLockedMessages()
	{
		foreach (CMapMessageState mapMessageState in MapMessageStates)
		{
			if (mapMessageState.MapMessageState != CMapMessageState.EMapMessageState.Locked)
			{
				continue;
			}
			if (mapMessageState.UnlockConditionState.IsUnlocked())
			{
				mapMessageState.UnlockMapMessage();
			}
			if (MapRuleLibraryClient.MRLYML.AutoComplete == null || !MapRuleLibraryClient.MRLYML.AutoComplete.IsLoaded || mapMessageState.MapMessage.UnlockCondition == null)
			{
				continue;
			}
			List<Tuple<EUnlockConditionType, string>> requiredConditions = mapMessageState.MapMessage.UnlockCondition.RequiredConditions;
			if (requiredConditions == null || requiredConditions.Count <= 0)
			{
				continue;
			}
			foreach (Tuple<EUnlockConditionType, string> item in mapMessageState.MapMessage.UnlockCondition.RequiredConditions.FindAll((Tuple<EUnlockConditionType, string> s) => s.Item1 == EUnlockConditionType.CompletedQuest).ToList())
			{
				if (MapRuleLibraryClient.MRLYML.AutoComplete.AutoCompleteQuestIDs.Contains(item.Item2))
				{
					mapMessageState.MapMessageShown();
					break;
				}
			}
			if (mapMessageState.MapMessageState == CMapMessageState.EMapMessageState.Shown)
			{
				continue;
			}
			foreach (Tuple<EUnlockConditionType, string> item2 in mapMessageState.MapMessage.UnlockCondition.RequiredConditions.FindAll((Tuple<EUnlockConditionType, string> s) => s.Item1 == EUnlockConditionType.CompletedAchievement).ToList())
			{
				if (MapRuleLibraryClient.MRLYML.AutoComplete.AutoCompleteAchievementIDs.Contains(item2.Item2))
				{
					mapMessageState.MapMessageShown();
					break;
				}
			}
		}
	}

	public void CheckAllLockedVisibilitySpheres()
	{
		foreach (CVisibilitySphereState visibilitySphereState in VisibilitySphereStates)
		{
			if (visibilitySphereState.VisibilitySphereState == CVisibilitySphereState.EVisibilitySphereState.Locked && visibilitySphereState.UnlockConditionState.IsUnlocked())
			{
				visibilitySphereState.UnlockVisibilitySphere();
			}
		}
	}

	public void CheckCurrentJobQuests(bool checkTimeout = false)
	{
		if (checkTimeout)
		{
			for (int num = CurrentJobQuestStates.Count - 1; num >= 0; num--)
			{
				CurrentJobQuestStates[num].CheckQuestTimeout();
			}
		}
		if (TutorialCompleted && IntroCompleted && CurrentJobQuestStates.Count < 3)
		{
			int num2 = Math.Min(3, Math.Min(AllUnlockedVillagesWithNoJob.Count, AllAvailableUnlockedJobQuestStates.Count)) - CurrentJobQuestStates.Count;
			if (CurrentJobQuestStates.Count + num2 < 3)
			{
				PreviousJobQuestStates.Clear();
				num2 = Math.Min(3, Math.Min(AllUnlockedVillagesWithNoJob.Count, AllAvailableUnlockedJobQuestStates.Count)) - CurrentJobQuestStates.Count;
			}
			for (int i = 0; i < num2; i++)
			{
				List<CLocationState> list = new List<CLocationState>();
				list.AddRange(AllUnlockedVillagesWithNoJob);
				for (int num3 = list.Count - 1; num3 >= 0; num3--)
				{
					CLocationState cLocationState = list[num3];
					if (cLocationState is CVillageState cVillageState)
					{
						if (cVillageState.AvailableJobQuests().Count <= 0)
						{
							list.Remove(cLocationState);
						}
					}
					else if (cLocationState is CHeadquartersState cHeadquartersState && cHeadquartersState.AvailableJobQuests().Count <= 0)
					{
						list.Remove(cLocationState);
					}
				}
				if (list.Count > 0)
				{
					CLocationState cLocationState2 = list[MapRNG.Next(list.Count)];
					if (cLocationState2 is CVillageState cVillageState2)
					{
						cVillageState2.RollForJobQuest();
					}
					else if (cLocationState2 is CHeadquartersState cHeadquartersState2)
					{
						cHeadquartersState2.RollForJobQuest();
					}
					continue;
				}
				MapRuleLibraryClient.AnalyticsLogError("ERROR_MRL_CHOREOGRAPHER_00002", "No villages with a viable job quest found");
				string text = "";
				foreach (CLocationState item in AllUnlockedVillagesWithNoJob)
				{
					text = text + item.ID + ", ";
				}
				DLLDebug.LogError("All unlocked villages with no job:" + text);
				string text2 = "";
				foreach (CJobQuestState allAvailableUnlockedJobQuestState in AllAvailableUnlockedJobQuestStates)
				{
					text2 = text2 + allAvailableUnlockedJobQuestState.ID + ", ";
				}
				DLLDebug.LogError("All unlocked and available job quest states:" + text2);
				break;
			}
		}
		SimpleLog.AddToSimpleLog("MapRNG (Check Current Job Quests): " + PeekMapRNG);
	}

	public void DebugAddJobQuest(string jobID)
	{
		CJobQuestState cJobQuestState = JobQuestStates.SingleOrDefault((CJobQuestState q) => q.ID == jobID);
		if (cJobQuestState != null)
		{
			CVector3 cVector = HeadquartersState.Headquarters.JobMapLocations[0];
			if (cVector != null && !AdventureState.MapState.CurrentJobQuestStates.Contains(cJobQuestState))
			{
				cJobQuestState.ResetQuest();
				cJobQuestState.SetJobLocationAndTimeout(cVector, HeadquartersState.Headquarters.ID);
				cJobQuestState.Init();
				CurrentJobQuestStates.Add(cJobQuestState);
			}
		}
	}

	public void RegenerateAllMapScenarios(bool excludeInProgressQuest = false, bool rerollQuestRewards = true)
	{
		foreach (CQuestState allQuest in AllQuests)
		{
			if (!excludeInProgressQuest || allQuest != InProgressQuestState)
			{
				if (rerollQuestRewards || allQuest.RewardGenerationRNGState == null)
				{
					allQuest.RollQuestRewards(AdventureState.MapState.MapRNG);
				}
				else
				{
					SharedLibrary.Random rng = allQuest.RewardGenerationRNGState.Restore();
					allQuest.RollQuestRewards(rng);
				}
				if (allQuest.ScenarioState.LocationState != ELocationState.Locked && (!allQuest.IsIntroQuest || allQuest.QuestState != CQuestState.EQuestState.Completed))
				{
					allQuest.ScenarioState.RegenerateMapScenario(allQuest.Quest.Chapter);
				}
			}
		}
		SimpleLog.AddToSimpleLog("MapRNG (Regenerate All Map Scenarios): " + AdventureState.MapState.PeekMapRNG + "\n" + Environment.StackTrace);
	}

	public Guid GetGUIDBasedOnMapRNGState()
	{
		byte[] array = new byte[16];
		MapRNG.NextBytes(array);
		SimpleLog.AddToSimpleLog("MapRNG (get GUID): " + AdventureState.MapState.PeekMapRNG);
		return new Guid(array);
	}

	public uint GetNextItemNetworkID()
	{
		m_LastItemNetworkID++;
		return m_LastItemNetworkID;
	}

	public int FindAppliedPerkPointRewardsForCharacter(string characterID)
	{
		int num = 0;
		foreach (CQuestState allCompletedQuest in AllCompletedQuests)
		{
			foreach (Reward reward in allCompletedQuest.QuestCompletionRewardGroup.Rewards)
			{
				if (reward.Type == ETreasureType.PerkPoint && (reward.GiveToCharacterID == "NoneID" || reward.GiveToCharacterID == "party" || reward.GiveToCharacterID == characterID))
				{
					num += reward.Amount;
				}
			}
		}
		foreach (CPartyAchievement claimedAchievement in MapParty.ClaimedAchievements)
		{
			foreach (RewardGroup reward2 in claimedAchievement.Rewards)
			{
				foreach (Reward reward3 in reward2.Rewards)
				{
					if (reward3.Type == ETreasureType.PerkPoint && (reward3.GiveToCharacterID == null || reward3.GiveToCharacterID == "party" || reward3.GiveToCharacterID == characterID))
					{
						num += reward3.Amount;
					}
				}
			}
		}
		return num;
	}

	public int FindAppliedEnhancementSlotRewards()
	{
		int num = 0;
		foreach (CQuestState allCompletedQuest in AllCompletedQuests)
		{
			foreach (Reward reward in allCompletedQuest.QuestCompletionRewardGroup.Rewards)
			{
				if (reward.Type == ETreasureType.EnhancementSlots)
				{
					num += reward.Amount;
				}
			}
		}
		foreach (CPartyAchievement claimedAchievement in MapParty.ClaimedAchievements)
		{
			foreach (RewardGroup reward2 in claimedAchievement.Rewards)
			{
				foreach (Reward reward3 in reward2.Rewards)
				{
					if (reward3.Type == ETreasureType.EnhancementSlots)
					{
						num += reward3.Amount;
					}
				}
			}
		}
		return num;
	}

	public CMapCharacter GetMapCharacterWithCharacterNameHash(int hashCode)
	{
		if (IsCampaign)
		{
			foreach (CMapCharacter selectedCharacter in MapParty.SelectedCharacters)
			{
				if (selectedCharacter.CharacterName.GetHashCode() == hashCode)
				{
					return selectedCharacter;
				}
			}
			foreach (CMapCharacter checkCharacter in MapParty.CheckCharacters)
			{
				if (checkCharacter.CharacterName.GetHashCode() == hashCode)
				{
					return checkCharacter;
				}
			}
			foreach (CMapCharacter item in MapParty.CharactersRetiredThisSession)
			{
				if (item.CharacterName.GetHashCode() == hashCode)
				{
					return item;
				}
			}
			DLLDebug.LogError("Unable to find character name for hash code");
			return null;
		}
		DLLDebug.LogError("Attempted to get character name hash in a non campaign mode adventure state - so characters won't have any names to hash");
		return null;
	}

	public string GetMapCharacterIDWithCharacterNameHash(int hashCode, bool suppressErrors = false)
	{
		if (IsCampaign)
		{
			foreach (CMapCharacter selectedCharacter in MapParty.SelectedCharacters)
			{
				if (selectedCharacter.CharacterName.GetHashCode() == hashCode)
				{
					return selectedCharacter.CharacterID;
				}
			}
			foreach (CMapCharacter checkCharacter in MapParty.CheckCharacters)
			{
				if (checkCharacter.CharacterName.GetHashCode() == hashCode)
				{
					return checkCharacter.CharacterID;
				}
			}
			foreach (CMapCharacter item in MapParty.CharactersRetiredThisSession)
			{
				if (item.CharacterName.GetHashCode() == hashCode)
				{
					return item.CharacterID;
				}
			}
			if (!suppressErrors)
			{
				DLLDebug.LogError("Unable to find character name for hash code");
			}
			return null;
		}
		DLLDebug.LogError("Attempted to get character name hash in a non campaign mode adventure state - so characters won't have any names to hash");
		return null;
	}

	public void SetDLCEnabled(DLCRegistry.EDLCKey dlcToEnable)
	{
		DLCEnabled |= dlcToEnable;
	}
}
