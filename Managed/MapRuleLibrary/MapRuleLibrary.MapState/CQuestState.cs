using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Client;
using MapRuleLibrary.Party;
using MapRuleLibrary.YML.BattleGoals;
using MapRuleLibrary.YML.Quest;
using MapRuleLibrary.YML.Shared;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SharedLibrary;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;
using StateCodeGenerator;

namespace MapRuleLibrary.MapState;

[Serializable]
[DebuggerDisplay("ID: {ID} State: {QuestState}")]
public class CQuestState : ISerializable
{
	[Serializable]
	public enum EQuestState
	{
		None,
		Locked,
		Unlocked,
		InProgress,
		Completed,
		Blocked,
		InProgressCasual
	}

	public delegate void QuestEventHandler(string characterId);

	private List<CMapScenarioState> ScenarioStates;

	public int SoloScenarioGoldGained;

	public int SoloScenarioXPGained;

	[NonSerialized]
	private Delegate[] onBattleGoalsUpdatedDelegates;

	private CQuest m_CachedQuest;

	public bool IsInitialised { get; private set; }

	public string ID { get; private set; }

	public EQuestState QuestState { get; protected set; }

	public CMapScenarioState ScenarioState { get; private set; }

	public RewardGroup QuestCompletionRewardGroup { get; protected set; }

	public Extensions.RandomState RewardGenerationRNGState { get; private set; }

	public CUnlockConditionState UnlockConditionState { get; private set; }

	public CUnlockConditionState BlockedConditionState { get; private set; }

	public List<Tuple<string, CBattleGoalState>> AssignedBattleGoalStates { get; private set; }

	public List<Tuple<string, CBattleGoalState>> ChosenBattleGoalStates { get; private set; }

	public List<string> CompleteCharacterNames { get; private set; }

	public bool IsNew { get; set; }

	public CQuest Quest
	{
		get
		{
			if (m_CachedQuest == null)
			{
				m_CachedQuest = MapRuleLibraryClient.MRLYML.Quests.SingleOrDefault((CQuest s) => s.ID == ID);
			}
			return m_CachedQuest;
		}
	}

	public bool HasBlockedCondition => BlockedConditionState != null;

	public bool InProgress
	{
		get
		{
			if (QuestState != EQuestState.InProgress)
			{
				return QuestState == EQuestState.InProgressCasual;
			}
			return true;
		}
	}

	public CQuestState LinkedQuestState => AdventureState.MapState.AllQuests.SingleOrDefault((CQuestState x) => x.ID == Quest.LinkedQuestID);

	public bool IsIntroQuest => MapRuleLibraryClient.MRLYML.Headquarters.TutorialQuestNames.SingleOrDefault((string s) => s == ID) != null;

	public bool IsSoloScenario => Quest.QuestCharacterRequirements.Any((QuestYML.CQuestCharacterRequirement x) => x.RequiredCharacterCount.HasValue && x.RequiredCharacterCount.Value == 1 && !string.IsNullOrEmpty(x.RequiredCharacterID));

	public bool IsDLCQuest
	{
		get
		{
			for (int i = 0; i < DLCRegistry.DLCNames.Length; i++)
			{
				if (ID.Contains(DLCRegistry.DLCNames[i], StringComparison.InvariantCultureIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}
	}

	public string SoloScenarioCharacterID
	{
		get
		{
			if (IsSoloScenario)
			{
				return Quest.QuestCharacterRequirements.FirstOrDefault((QuestYML.CQuestCharacterRequirement x) => !string.IsNullOrEmpty(x.RequiredCharacterID)).RequiredCharacterID;
			}
			return null;
		}
	}

	public int ScenarioLevelToUse
	{
		get
		{
			if (IsSoloScenario)
			{
				QuestYML.CQuestCharacterRequirement questRequirement = Quest.QuestCharacterRequirements.FirstOrDefault((QuestYML.CQuestCharacterRequirement x) => !string.IsNullOrEmpty(x.RequiredCharacterID));
				if (questRequirement != null)
				{
					CMapCharacter cMapCharacter = AdventureState.MapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter x) => x.CharacterID == questRequirement.RequiredCharacterID);
					if (cMapCharacter == null)
					{
						cMapCharacter = AdventureState.MapState.MapParty.CheckCharacters.FirstOrDefault((CMapCharacter x) => x.CharacterID == questRequirement.RequiredCharacterID);
					}
					if (cMapCharacter != null)
					{
						int num = (int)Math.Ceiling((float)cMapCharacter.Level / 2f);
						return Math.Min(7, num + ((AdventureState.MapState != null) ? AdventureState.MapState.Difficulty.ScenarioLevelModifier : 0));
					}
				}
			}
			return AdventureState.MapState.MapParty.ScenarioLevel;
		}
	}

	public event QuestEventHandler OnBattleGoalsUpdated;

	public CQuestState()
	{
	}

	public CQuestState(CQuestState state, ReferenceDictionary references)
	{
		IsInitialised = state.IsInitialised;
		ID = state.ID;
		QuestState = state.QuestState;
		ScenarioState = references.Get(state.ScenarioState);
		if (ScenarioState == null && state.ScenarioState != null)
		{
			ScenarioState = new CMapScenarioState(state.ScenarioState, references);
			references.Add(state.ScenarioState, ScenarioState);
		}
		QuestCompletionRewardGroup = references.Get(state.QuestCompletionRewardGroup);
		if (QuestCompletionRewardGroup == null && state.QuestCompletionRewardGroup != null)
		{
			QuestCompletionRewardGroup = new RewardGroup(state.QuestCompletionRewardGroup, references);
			references.Add(state.QuestCompletionRewardGroup, QuestCompletionRewardGroup);
		}
		RewardGenerationRNGState = references.Get(state.RewardGenerationRNGState);
		if (RewardGenerationRNGState == null && state.RewardGenerationRNGState != null)
		{
			RewardGenerationRNGState = new Extensions.RandomState(state.RewardGenerationRNGState, references);
			references.Add(state.RewardGenerationRNGState, RewardGenerationRNGState);
		}
		UnlockConditionState = references.Get(state.UnlockConditionState);
		if (UnlockConditionState == null && state.UnlockConditionState != null)
		{
			UnlockConditionState = new CUnlockConditionState(state.UnlockConditionState, references);
			references.Add(state.UnlockConditionState, UnlockConditionState);
		}
		BlockedConditionState = references.Get(state.BlockedConditionState);
		if (BlockedConditionState == null && state.BlockedConditionState != null)
		{
			BlockedConditionState = new CUnlockConditionState(state.BlockedConditionState, references);
			references.Add(state.BlockedConditionState, BlockedConditionState);
		}
		AssignedBattleGoalStates = references.Get(state.AssignedBattleGoalStates);
		if (AssignedBattleGoalStates == null && state.AssignedBattleGoalStates != null)
		{
			AssignedBattleGoalStates = new List<Tuple<string, CBattleGoalState>>();
			for (int i = 0; i < state.AssignedBattleGoalStates.Count; i++)
			{
				Tuple<string, CBattleGoalState> tuple = state.AssignedBattleGoalStates[i];
				string item = tuple.Item1;
				CBattleGoalState cBattleGoalState = references.Get(tuple.Item2);
				if (cBattleGoalState == null && tuple.Item2 != null)
				{
					cBattleGoalState = new CBattleGoalState(tuple.Item2, references);
					references.Add(tuple.Item2, cBattleGoalState);
				}
				Tuple<string, CBattleGoalState> item2 = new Tuple<string, CBattleGoalState>(item, cBattleGoalState);
				AssignedBattleGoalStates.Add(item2);
			}
			references.Add(state.AssignedBattleGoalStates, AssignedBattleGoalStates);
		}
		ChosenBattleGoalStates = references.Get(state.ChosenBattleGoalStates);
		if (ChosenBattleGoalStates == null && state.ChosenBattleGoalStates != null)
		{
			ChosenBattleGoalStates = new List<Tuple<string, CBattleGoalState>>();
			for (int j = 0; j < state.ChosenBattleGoalStates.Count; j++)
			{
				Tuple<string, CBattleGoalState> tuple2 = state.ChosenBattleGoalStates[j];
				string item3 = tuple2.Item1;
				CBattleGoalState cBattleGoalState2 = references.Get(tuple2.Item2);
				if (cBattleGoalState2 == null && tuple2.Item2 != null)
				{
					cBattleGoalState2 = new CBattleGoalState(tuple2.Item2, references);
					references.Add(tuple2.Item2, cBattleGoalState2);
				}
				Tuple<string, CBattleGoalState> item4 = new Tuple<string, CBattleGoalState>(item3, cBattleGoalState2);
				ChosenBattleGoalStates.Add(item4);
			}
			references.Add(state.ChosenBattleGoalStates, ChosenBattleGoalStates);
		}
		CompleteCharacterNames = references.Get(state.CompleteCharacterNames);
		if (CompleteCharacterNames == null && state.CompleteCharacterNames != null)
		{
			CompleteCharacterNames = new List<string>();
			for (int k = 0; k < state.CompleteCharacterNames.Count; k++)
			{
				string item5 = state.CompleteCharacterNames[k];
				CompleteCharacterNames.Add(item5);
			}
			references.Add(state.CompleteCharacterNames, CompleteCharacterNames);
		}
		IsNew = state.IsNew;
		ScenarioStates = references.Get(state.ScenarioStates);
		if (ScenarioStates == null && state.ScenarioStates != null)
		{
			ScenarioStates = new List<CMapScenarioState>();
			for (int l = 0; l < state.ScenarioStates.Count; l++)
			{
				CMapScenarioState cMapScenarioState = state.ScenarioStates[l];
				CMapScenarioState cMapScenarioState2 = references.Get(cMapScenarioState);
				if (cMapScenarioState2 == null && cMapScenarioState != null)
				{
					cMapScenarioState2 = new CMapScenarioState(cMapScenarioState, references);
					references.Add(cMapScenarioState, cMapScenarioState2);
				}
				ScenarioStates.Add(cMapScenarioState2);
			}
			references.Add(state.ScenarioStates, ScenarioStates);
		}
		SoloScenarioGoldGained = state.SoloScenarioGoldGained;
		SoloScenarioXPGained = state.SoloScenarioXPGained;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("ID", ID);
		info.AddValue("QuestState", QuestState);
		info.AddValue("ScenarioState", ScenarioState);
		info.AddValue("QuestCompletionRewardGroup", QuestCompletionRewardGroup);
		info.AddValue("RewardGenerationRNGState", RewardGenerationRNGState);
		info.AddValue("IsInitialised", IsInitialised);
		info.AddValue("UnlockConditionState", UnlockConditionState);
		info.AddValue("AssignedBattleGoalStates", AssignedBattleGoalStates);
		info.AddValue("ChosenBattleGoalStates", ChosenBattleGoalStates);
		info.AddValue("CompleteCharacterNames", CompleteCharacterNames);
	}

	public CQuestState(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "ID":
					ID = info.GetString("ID");
					break;
				case "QuestState":
					QuestState = (EQuestState)info.GetValue("QuestState", typeof(EQuestState));
					break;
				case "ScenarioState":
					ScenarioState = (CMapScenarioState)info.GetValue("ScenarioState", typeof(CMapScenarioState));
					break;
				case "QuestCompletionRewardGroup":
					QuestCompletionRewardGroup = (RewardGroup)info.GetValue("QuestCompletionRewardGroup", typeof(RewardGroup));
					break;
				case "RewardGenerationRNGState":
					RewardGenerationRNGState = (Extensions.RandomState)info.GetValue("RewardGenerationRNGState", typeof(Extensions.RandomState));
					break;
				case "IsInitialised":
					IsInitialised = info.GetBoolean("IsInitialised");
					break;
				case "UnlockConditionState":
					UnlockConditionState = (CUnlockConditionState)info.GetValue("UnlockConditionState", typeof(CUnlockConditionState));
					break;
				case "AssignedBattleGoalStates":
					AssignedBattleGoalStates = (List<Tuple<string, CBattleGoalState>>)info.GetValue("AssignedBattleGoalStates", typeof(List<Tuple<string, CBattleGoalState>>));
					break;
				case "ChosenBattleGoalStates":
					ChosenBattleGoalStates = (List<Tuple<string, CBattleGoalState>>)info.GetValue("ChosenBattleGoalStates", typeof(List<Tuple<string, CBattleGoalState>>));
					break;
				case "CompleteCharacterNames":
					CompleteCharacterNames = (List<string>)info.GetValue("CompleteCharacterNames", typeof(List<string>));
					break;
				case "ScenarioStates":
					ScenarioStates = (List<CMapScenarioState>)info.GetValue("ScenarioStates", typeof(List<CMapScenarioState>));
					break;
				case "BattleGoalStates":
					ChosenBattleGoalStates = (List<Tuple<string, CBattleGoalState>>)info.GetValue("BattleGoalStates", typeof(List<Tuple<string, CBattleGoalState>>));
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CQuestState entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	[OnSerializing]
	internal void OnSerializing(StreamingContext context)
	{
		if (this.OnBattleGoalsUpdated != null)
		{
			onBattleGoalsUpdatedDelegates = this.OnBattleGoalsUpdated.GetInvocationList();
			Delegate[] array = onBattleGoalsUpdatedDelegates;
			foreach (Delegate obj in array)
			{
				OnBattleGoalsUpdated -= obj as QuestEventHandler;
			}
		}
	}

	[OnSerialized]
	internal void OnSerialized(StreamingContext context)
	{
		if (onBattleGoalsUpdatedDelegates != null)
		{
			Delegate[] array = onBattleGoalsUpdatedDelegates;
			foreach (Delegate obj in array)
			{
				OnBattleGoalsUpdated += obj as QuestEventHandler;
			}
			onBattleGoalsUpdatedDelegates = null;
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		onDeserialized(context);
	}

	protected virtual void onDeserialized(StreamingContext context)
	{
		if (ScenarioStates != null && ScenarioStates.Count > 0)
		{
			ScenarioState = ScenarioStates[0];
		}
		if (ChosenBattleGoalStates == null)
		{
			ChosenBattleGoalStates = new List<Tuple<string, CBattleGoalState>>();
		}
		if (AssignedBattleGoalStates == null)
		{
			AssignedBattleGoalStates = new List<Tuple<string, CBattleGoalState>>();
		}
		if (CompleteCharacterNames == null)
		{
			CompleteCharacterNames = new List<string>();
		}
	}

	public CQuestState(CQuest quest)
	{
		ID = quest.ID;
		QuestState = EQuestState.Locked;
		ScenarioState = new CMapScenarioState(quest.MapScenario, ID);
		IsNew = true;
		IsInitialised = false;
		AssignedBattleGoalStates = new List<Tuple<string, CBattleGoalState>>();
		ChosenBattleGoalStates = new List<Tuple<string, CBattleGoalState>>();
		CompleteCharacterNames = new List<string>();
		UnlockConditionState = new CUnlockConditionState(quest.UnlockCondition);
		if (quest.BlockedCondition != null)
		{
			BlockedConditionState = new CUnlockConditionState(quest.BlockedCondition);
		}
	}

	public virtual void Init()
	{
		if (!IsInitialised)
		{
			ScenarioState.Init();
			RollQuestRewards(AdventureState.MapState.MapRNG);
			SimpleLog.AddToSimpleLog("MapRNG (Quest init roll rewards): " + AdventureState.MapState.PeekMapRNG);
			IsInitialised = true;
		}
	}

	public void OnMapStateAdventureStarted()
	{
		if (Quest == null)
		{
			return;
		}
		if (UnlockConditionState == null)
		{
			UnlockConditionState = new CUnlockConditionState(Quest.UnlockCondition);
		}
		UnlockConditionState.CacheUnlockCondition(Quest.UnlockCondition);
		if (Quest.BlockedCondition != null)
		{
			if (BlockedConditionState == null)
			{
				BlockedConditionState = new CUnlockConditionState(Quest.BlockedCondition);
			}
			BlockedConditionState.CacheUnlockCondition(Quest.BlockedCondition);
		}
		if (QuestState != EQuestState.Locked && QuestState != EQuestState.Blocked)
		{
			ScenarioState.OnMapStateAdventureStarted();
		}
		if (QuestState == EQuestState.Unlocked || InProgress)
		{
			if (RewardGenerationRNGState != null)
			{
				RewardGroup rewardGroup = new RewardGroup(TreasureTableProcessing.RollTreasureTables(RewardGenerationRNGState.Restore(), MapYMLShared.ValidateTreasureTableRewards(Quest.CompletionTreasureTables, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter)), AdventureState.MapState.MapParty.ScenarioLevel, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter)), "Chest|" + string.Join(",", Quest.CompletionTreasureTables.Select((TreasureTable x) => x.Name)));
				if (!IsIntroQuest)
				{
					ScenarioLevelTableEntry scenarioLevelTableEntry = AdventureState.MapState.HeadquartersState.Headquarters.SLT.Entries[ScenarioLevelToUse];
					Reward item = new Reward(ETreasureType.XP, scenarioLevelTableEntry.BonusXP, ETreasureDistributionType.Combined, null);
					rewardGroup.Rewards.Add(item);
				}
				if (Quest.Type == EQuestType.Travel)
				{
					rewardGroup.Rewards.Add(new Reward(ETreasureType.UnlockLocation, Quest.EndingVillage));
					rewardGroup.RefreshRewardValues();
				}
				if (!rewardGroup.Equals(QuestCompletionRewardGroup))
				{
					QuestCompletionRewardGroup = rewardGroup;
				}
			}
			else
			{
				RollQuestRewards(AdventureState.MapState.MapRNG);
			}
			SimpleLog.AddToSimpleLog("MapRNG (refresh Quest state from yml): " + AdventureState.MapState.PeekMapRNG);
		}
		ChosenBattleGoalStates.RemoveAll((Tuple<string, CBattleGoalState> x) => x == null);
		if (ChosenBattleGoalStates.Count <= 0)
		{
			return;
		}
		foreach (Tuple<string, CBattleGoalState> chosenBattleGoalState in ChosenBattleGoalStates)
		{
			chosenBattleGoalState.Item2.OnMapStateAdventureStarted();
		}
	}

	public void RollQuestRewards(SharedLibrary.Random rng, int? overrideLevel = null)
	{
		RewardGenerationRNGState = rng.Save();
		SharedLibrary.Random rng2 = RewardGenerationRNGState.Restore();
		int num = (overrideLevel.HasValue ? overrideLevel.Value : ScenarioLevelToUse);
		QuestCompletionRewardGroup = new RewardGroup(TreasureTableProcessing.RollTreasureTables(rng2, MapYMLShared.ValidateTreasureTableRewards(Quest.CompletionTreasureTables, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter)), num, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter)), "Chest|" + string.Join(",", Quest.CompletionTreasureTables.Select((TreasureTable x) => x.Name)));
		QuestCompletionRewardGroup.PickedUpBy = "party";
		if (!IsIntroQuest)
		{
			ScenarioLevelTableEntry scenarioLevelTableEntry = AdventureState.MapState.HeadquartersState.Headquarters.SLT.Entries[num];
			Reward item = new Reward(ETreasureType.XP, scenarioLevelTableEntry.BonusXP, ETreasureDistributionType.Combined, null);
			QuestCompletionRewardGroup.Rewards.Add(item);
		}
		if (Quest.Type == EQuestType.Travel)
		{
			QuestCompletionRewardGroup.Rewards.Add(new Reward(ETreasureType.UnlockLocation, Quest.EndingVillage));
		}
		QuestCompletionRewardGroup.RefreshRewardValues();
	}

	public void UnlockQuest(bool regenerate = true)
	{
		if (QuestState != EQuestState.Locked)
		{
			return;
		}
		QuestState = EQuestState.Unlocked;
		if (regenerate)
		{
			RollQuestRewards(AdventureState.MapState.MapRNG);
			SimpleLog.AddToSimpleLog("MapRNG (unlock quest): " + AdventureState.MapState.PeekMapRNG);
			if (AdventureState.MapState.MapParty.SelectedCharacters.Any())
			{
				ScenarioState.RegenerateMapScenario(Quest.Chapter);
			}
			ScenarioState.UnlockLocation();
			if (AdventureState.MapState.IsCampaign || Quest.UnlockDialogueLines.Count > 0)
			{
				AdventureState.MapState.QueuedUnlockedQuestIDs.Add(Quest.ID);
			}
		}
		CContentUnlocked_MapClientMessage message = new CContentUnlocked_MapClientMessage(ID, "quest", Quest.UnlockCondition);
		if (MapRuleLibraryClient.Instance.MessageHandler != null)
		{
			MapRuleLibraryClient.Instance.MessageHandler(message);
		}
		else
		{
			DLLDebug.LogWarning("Message handler not set");
		}
	}

	public void BlockQuest()
	{
		QuestState = EQuestState.Blocked;
	}

	public void LockQuest()
	{
		QuestState = EQuestState.Locked;
	}

	public void SetInProgressQuest()
	{
		QuestState = EQuestState.InProgress;
		AssignedBattleGoalStates.Clear();
		ChosenBattleGoalStates.Clear();
		foreach (CMapCharacter selectedCharacter in AdventureState.MapState.MapParty.SelectedCharacters)
		{
			RollAndAssignBattleGoals(selectedCharacter);
		}
		AdventureState.MapState.DebugNextBattleGoals.Clear();
	}

	public void SetInProgressCasualMode()
	{
		QuestState = EQuestState.InProgressCasual;
		AssignedBattleGoalStates.Clear();
		ChosenBattleGoalStates.Clear();
		foreach (CMapCharacter selectedCharacter in AdventureState.MapState.MapParty.SelectedCharacters)
		{
			RollAndAssignBattleGoals(selectedCharacter);
		}
		AdventureState.MapState.DebugNextBattleGoals.Clear();
	}

	public void ResetQuest(bool regenerate = true)
	{
		QuestState = ((QuestState == EQuestState.InProgressCasual) ? EQuestState.Completed : EQuestState.Unlocked);
		ScenarioState.ClearAutoSaveState();
		AssignedBattleGoalStates.Clear();
		ChosenBattleGoalStates.Clear();
		if (regenerate && ScenarioState != null && Quest != null)
		{
			ScenarioState.RegenerateMapScenario(Quest.Chapter);
		}
	}

	public void UpdateQuestCompletion(bool autoComplete = false)
	{
		if (Thread.CurrentThread != MapRuleLibraryClient.Instance.MessageThreadHandler)
		{
			MapRuleLibraryClient.Instance.AddQueueMessage(new CUpdateQuestCompletion_MapDLLMessage(ID, autoComplete), processImmediately: false);
		}
		else
		{
			UpdateQuestCompletionInternal(autoComplete);
		}
	}

	protected virtual void UpdateQuestCompletionInternal(bool autoComplete = false)
	{
		bool flag = QuestState == EQuestState.InProgressCasual;
		EQuestState questState = QuestState;
		QuestState = EQuestState.Completed;
		AdventureState.MapState.CheckTrophyAchievements(new CQuestCompleted_AchievementTrigger(ID));
		ScenarioState.CompleteLocation();
		foreach (CMapCharacter selectedCharacter in AdventureState.MapState.MapParty.SelectedCharacters)
		{
			if (!CompleteCharacterNames.Contains(selectedCharacter.CharacterName))
			{
				CompleteCharacterNames.Add(selectedCharacter.CharacterName);
			}
		}
		if (IsSoloScenario)
		{
			CMapCharacter cMapCharacter = AdventureState.MapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter x) => x.CharacterID == SoloScenarioCharacterID);
			if (cMapCharacter != null && cMapCharacter.CompletedSoloQuestData.Find((CCompletedSoloQuestData x) => x.QuestID == ID) == null)
			{
				foreach (Reward reward in QuestCompletionRewardGroup.Rewards)
				{
					reward.GiveToCharacterID = SoloScenarioCharacterID;
				}
				cMapCharacter.CompletedSoloQuestData.Add(new CCompletedSoloQuestData(ID, ScenarioLevelToUse, SoloScenarioXPGained, SoloScenarioGoldGained));
				AdventureState.MapState.ApplyRewards(QuestCompletionRewardGroup.Rewards.FindAll((Reward it) => it.Type == ETreasureType.UnlockQuest), SoloScenarioCharacterID);
				AdventureState.MapState.QueuedCompletionRewards.AddRange(QuestCompletionRewardGroup.Rewards.FindAll((Reward it) => it.Type != ETreasureType.UnlockCharacter || !AdventureState.MapState.MapParty.UnlockedCharacterIDs.Contains(it.CharacterID)));
			}
		}
		else if (questState < EQuestState.Completed)
		{
			if (!AdventureState.MapState.IsCampaign)
			{
				AdventureState.MapState.ApplyRewards(QuestCompletionRewardGroup.Rewards, "party");
			}
			else
			{
				AdventureState.MapState.ApplyRewards(QuestCompletionRewardGroup.Rewards.FindAll((Reward it) => it.Type == ETreasureType.UnlockQuest), "party");
			}
			AdventureState.MapState.QueuedCompletionRewards.AddRange(QuestCompletionRewardGroup.Rewards.FindAll((Reward it) => it.Type != ETreasureType.UnlockCharacter || !AdventureState.MapState.MapParty.UnlockedCharacterIDs.Contains(it.CharacterID)));
		}
		AssignedBattleGoalStates.Clear();
		ChosenBattleGoalStates.Clear();
		if (Quest.Type == EQuestType.Travel && AdventureState.MapState.AllVillages.SingleOrDefault((CLocationState v) => v.ID == Quest.EndingVillage) is CVillageState cVillageState)
		{
			cVillageState.UnlockLocation();
			cVillageState.ConnectVillage(Quest.StartingVillage);
		}
		if (!autoComplete && !flag && Quest.CompleteDialogueLines.Count > 0)
		{
			AdventureState.MapState.QueuedCompletedQuestIDs.Add(Quest.ID);
		}
		if (AdventureState.MapState.MapParty.SelectedCharacters.Count() > 0)
		{
			ScenarioState.RegenerateMapScenario(Quest.Chapter);
		}
		if (!AdventureState.MapState.IntroCompleted)
		{
			AdventureState.MapState.CheckFTUECompletion();
		}
	}

	public void AutoComplete()
	{
		UpdateQuestCompletion(autoComplete: true);
	}

	public void SoloScenarioImportCompletion(CMapCharacter characterToComplete, int levelToCompleteAt)
	{
		if (Thread.CurrentThread != MapRuleLibraryClient.Instance.MessageThreadHandler)
		{
			MapRuleLibraryClient.Instance.AddQueueMessage(new CSoloScenarioImportCompletion_MapDLLMessage(this, characterToComplete, levelToCompleteAt), processImmediately: false);
		}
		else
		{
			SoloScenarioImportCompletionInternal(characterToComplete, levelToCompleteAt);
		}
	}

	private void SoloScenarioImportCompletionInternal(CMapCharacter characterToComplete, int levelToCompleteAt)
	{
		RollQuestRewards(AdventureState.MapState.MapRNG, levelToCompleteAt);
		bool num = QuestState == EQuestState.InProgressCasual;
		_ = QuestState;
		QuestState = EQuestState.Completed;
		ScenarioState.CompleteLocation();
		if (!CompleteCharacterNames.Contains(characterToComplete.CharacterName))
		{
			CompleteCharacterNames.Add(characterToComplete.CharacterName);
		}
		if (characterToComplete != null && characterToComplete.CompletedSoloQuestData.Find((CCompletedSoloQuestData x) => x.QuestID == ID) == null)
		{
			characterToComplete.CompletedSoloQuestData.Add(new CCompletedSoloQuestData(ID, ScenarioLevelToUse, SoloScenarioXPGained, SoloScenarioGoldGained));
			AdventureState.MapState.ApplyRewards(QuestCompletionRewardGroup.Rewards.FindAll((Reward it) => it.Type != ETreasureType.UnlockCharacter || !AdventureState.MapState.MapParty.UnlockedCharacterIDs.Contains(it.CharacterID)), SoloScenarioCharacterID, battleGoal: false, changeItemUnlockToGold: true, characterToComplete.CharacterName);
			AdventureState.MapState.QueuedCompletionRewards.AddRange(QuestCompletionRewardGroup.Rewards.FindAll((Reward it) => it.Type != ETreasureType.UnlockCharacter || !AdventureState.MapState.MapParty.UnlockedCharacterIDs.Contains(it.CharacterID)));
		}
		AssignedBattleGoalStates.Clear();
		ChosenBattleGoalStates.Clear();
		if (Quest.Type == EQuestType.Travel && AdventureState.MapState.AllVillages.SingleOrDefault((CLocationState v) => v.ID == Quest.EndingVillage) is CVillageState cVillageState)
		{
			cVillageState.UnlockLocation();
			cVillageState.ConnectVillage(Quest.StartingVillage);
		}
		if (!num && Quest.CompleteDialogueLines.Count > 0)
		{
			AdventureState.MapState.QueuedCompletedQuestIDs.Add(Quest.ID);
		}
		if (AdventureState.MapState.MapParty.SelectedCharacters.Count() > 0)
		{
			ScenarioState.RegenerateMapScenario(Quest.Chapter);
		}
		if (!AdventureState.MapState.IntroCompleted)
		{
			AdventureState.MapState.CheckFTUECompletion();
		}
		if (MapRuleLibraryClient.Instance?.MessageHandler != null)
		{
			MapRuleLibraryClient.Instance.MessageHandler(new CMapClientMessage(EMapClientMessageType.FinishedSoloScenarioImport));
		}
	}

	public void RollAndAssignBattleGoals(CMapCharacter mapCharacter)
	{
		string characterID = mapCharacter.CharacterID;
		SimpleLog.AddToSimpleLog("Rolling and assigning battle goals for CharacterID: " + characterID);
		List<BattleGoalYMLData> list = MapRuleLibraryClient.MRLYML.BattleGoals.Where((BattleGoalYMLData it) => !AssignedBattleGoalStates.Any((Tuple<string, CBattleGoalState> x) => x.Item2.ID == it.ID)).ToList();
		if (list.Count <= 0)
		{
			SimpleLog.AddToSimpleLog("All Battle Goals currently assigned, reusing Battle Goal deck");
			list = MapRuleLibraryClient.MRLYML.BattleGoals;
		}
		int num = Math.Min(list.Count, 2);
		for (int num2 = 0; num2 < num; num2++)
		{
			CBattleGoalState cBattleGoalState = null;
			if (AdventureState.MapState.DebugNextBattleGoals.Count > 0)
			{
				cBattleGoalState = new CBattleGoalState(AdventureState.MapState.DebugNextBattleGoals.Last(), mapCharacter);
				AdventureState.MapState.DebugNextBattleGoals.RemoveAt(AdventureState.MapState.DebugNextBattleGoals.Count - 1);
			}
			else
			{
				int num3 = AdventureState.MapState.MapRNG.Next(list.Count);
				SimpleLog.AddToSimpleLog("MapRNG (roll battle goal): " + AdventureState.MapState.PeekMapRNG);
				if (!AdventureState.MapState.IsCampaign && list[num3].CampaignOnly)
				{
					SimpleLog.AddToSimpleLog("Skipped Plunderer Battle Goal in Guildmaster for CharacterID: " + characterID);
					num3 = ((num3 >= list.Count - 1) ? (num3 - 1) : (num3 + 1));
				}
				cBattleGoalState = new CBattleGoalState(list[num3], mapCharacter);
				list.RemoveAt(num3);
			}
			cBattleGoalState.RollBattleGoalRewards();
			AssignedBattleGoalStates.Add(new Tuple<string, CBattleGoalState>(characterID, cBattleGoalState));
		}
		if (IsSoloScenario && SoloScenarioCharacterID != characterID)
		{
			ChosenBattleGoalStates.Add(AssignedBattleGoalStates.First((Tuple<string, CBattleGoalState> it) => it.Item1 == characterID));
			this.OnBattleGoalsUpdated?.Invoke(characterID);
		}
	}

	public void ChooseBattleGoal(string characterID, CBattleGoalState battleGoal)
	{
		CBattleGoalState chosenBattleGoal = GetChosenBattleGoal(characterID);
		if (chosenBattleGoal != null)
		{
			if (chosenBattleGoal.ID == battleGoal.ID)
			{
				return;
			}
			RemoveBattleGoal(characterID, chosenBattleGoal.ID, triggerEvent: false);
		}
		SimpleLog.AddToSimpleLog("Battle Goal " + battleGoal.ID + " chosen by " + characterID);
		ChosenBattleGoalStates.Add(new Tuple<string, CBattleGoalState>(characterID, battleGoal));
		this.OnBattleGoalsUpdated?.Invoke(characterID);
	}

	public List<CBattleGoalState> GetAssignedBattleGoals(string characterID)
	{
		return (from x in AssignedBattleGoalStates.FindAll((Tuple<string, CBattleGoalState> it) => it.Item1 == characterID)
			select x.Item2).ToList();
	}

	public CBattleGoalState GetChosenBattleGoal(string characterID)
	{
		return ChosenBattleGoalStates.FirstOrDefault((Tuple<string, CBattleGoalState> it) => it.Item1 == characterID)?.Item2;
	}

	public void RemoveBattleGoal(string characterID, CBattleGoalState battleGoal)
	{
		RemoveBattleGoal(characterID, battleGoal.ID, triggerEvent: true);
	}

	public void RemoveBattleGoal(string characterID, string battleGoalID)
	{
		RemoveBattleGoal(characterID, battleGoalID, triggerEvent: true);
	}

	private void RemoveBattleGoal(string characterID, string battleGoalID, bool triggerEvent)
	{
		if (ChosenBattleGoalStates.RemoveAll((Tuple<string, CBattleGoalState> it) => it.Item1 == characterID && it.Item2.ID == battleGoalID) > 0 && triggerEvent)
		{
			this.OnBattleGoalsUpdated?.Invoke(characterID);
		}
	}

	public void CheckBattleGoals(bool useCurrentScenarioStats = false)
	{
		foreach (Tuple<string, CBattleGoalState> chosenBattleGoalState in ChosenBattleGoalStates)
		{
			CBattleGoalState item = chosenBattleGoalState.Item2;
			string item2 = chosenBattleGoalState.Item1;
			item.CheckBattleGoal(item2, useCurrentScenarioStats);
		}
	}
}
