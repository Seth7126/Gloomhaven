using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Client;
using MapRuleLibrary.MapState;
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
public class CPersonalQuestState : ISerializable
{
	private PersonalQuestYMLData _parentPersonalQuestData;

	public string ID { get; private set; }

	public Extensions.RandomState RewardGenerationRNGState { get; private set; }

	public bool RolledRewards { get; private set; }

	public CUnlockConditionState PersonalQuestConditionState { get; private set; }

	public bool IsConcealed { get; private set; }

	public EPersonalQuestState State { get; private set; }

	public int LastProgressShown { get; set; }

	public int CurrentPersonalQuestStep { get; set; }

	public List<List<RewardGroup>> RewardsByStep { get; private set; }

	public PersonalQuestYMLData ParentPersonalQuestData
	{
		get
		{
			if (_parentPersonalQuestData == null)
			{
				_parentPersonalQuestData = MapRuleLibraryClient.MRLYML.PersonalQuests.SingleOrDefault((PersonalQuestYMLData s) => s.ID == ID);
			}
			return _parentPersonalQuestData;
		}
	}

	public List<RewardGroup> CurrentRewards => RewardsByStep[CurrentPersonalQuestStep];

	public List<RewardGroup> FinalRewards => RewardsByStep.Last();

	public int PersonalQuestSteps
	{
		get
		{
			int result = 0;
			if (ParentPersonalQuestData.PersonalQuestSteps != null)
			{
				result = ParentPersonalQuestData.PersonalQuestSteps.Count;
			}
			return result;
		}
	}

	public PersonalQuestYMLData CurrentPersonalQuestStepData
	{
		get
		{
			PersonalQuestYMLData personalQuestYMLData = ParentPersonalQuestData;
			if (personalQuestYMLData?.PersonalQuestSteps != null)
			{
				personalQuestYMLData = personalQuestYMLData.PersonalQuestSteps[CurrentPersonalQuestStep];
			}
			return personalQuestYMLData;
		}
	}

	public string LocalisedCompletedProgressStepStory
	{
		get
		{
			if (CurrentPersonalQuestStep >= PersonalQuestSteps - 1)
			{
				return CurrentPersonalQuestStepData.LocalisedCompletedStory;
			}
			if (CurrentPersonalQuestStepData.IsPersonalQuestStep && CurrentPersonalQuestStep == 0)
			{
				return ID + "_PROGRESS_COMPLETED_STEP_0";
			}
			return null;
		}
	}

	public string AudioIdCompletedProgressStepStory
	{
		get
		{
			if (CurrentPersonalQuestStep >= PersonalQuestSteps - 1)
			{
				return CurrentPersonalQuestStepData.AudioIdCompletedStory;
			}
			if (CurrentPersonalQuestStepData.IsPersonalQuestStep && CurrentPersonalQuestStep == 0)
			{
				return CurrentPersonalQuestStepData.AudioIdProgressFirstStepStory;
			}
			return null;
		}
	}

	public List<TreasureTable> PersonalQuestTreasureTables => ScenarioRuleClient.SRLYML.TreasureTables.Where((TreasureTable w) => CurrentPersonalQuestStepData.TreasureTables.Contains(w.Name)).ToList();

	public bool IsFinished
	{
		get
		{
			if (State == EPersonalQuestState.Completed)
			{
				return CurrentPersonalQuestStep >= PersonalQuestSteps - 1;
			}
			return false;
		}
	}

	public CPersonalQuestState()
	{
	}

	public CPersonalQuestState(CPersonalQuestState state, ReferenceDictionary references)
	{
		ID = state.ID;
		RewardGenerationRNGState = references.Get(state.RewardGenerationRNGState);
		if (RewardGenerationRNGState == null && state.RewardGenerationRNGState != null)
		{
			RewardGenerationRNGState = new Extensions.RandomState(state.RewardGenerationRNGState, references);
			references.Add(state.RewardGenerationRNGState, RewardGenerationRNGState);
		}
		RolledRewards = state.RolledRewards;
		PersonalQuestConditionState = references.Get(state.PersonalQuestConditionState);
		if (PersonalQuestConditionState == null && state.PersonalQuestConditionState != null)
		{
			PersonalQuestConditionState = new CUnlockConditionState(state.PersonalQuestConditionState, references);
			references.Add(state.PersonalQuestConditionState, PersonalQuestConditionState);
		}
		IsConcealed = state.IsConcealed;
		State = state.State;
		LastProgressShown = state.LastProgressShown;
		CurrentPersonalQuestStep = state.CurrentPersonalQuestStep;
		RewardsByStep = references.Get(state.RewardsByStep);
		if (RewardsByStep != null || state.RewardsByStep == null)
		{
			return;
		}
		RewardsByStep = new List<List<RewardGroup>>();
		for (int i = 0; i < state.RewardsByStep.Count; i++)
		{
			List<RewardGroup> list = state.RewardsByStep[i];
			List<RewardGroup> list2 = references.Get(list);
			if (list2 == null && list != null)
			{
				list2 = new List<RewardGroup>();
				for (int j = 0; j < list.Count; j++)
				{
					RewardGroup rewardGroup = list[j];
					RewardGroup rewardGroup2 = references.Get(rewardGroup);
					if (rewardGroup2 == null && rewardGroup != null)
					{
						rewardGroup2 = new RewardGroup(rewardGroup, references);
						references.Add(rewardGroup, rewardGroup2);
					}
					list2.Add(rewardGroup2);
				}
				references.Add(list, list2);
			}
			RewardsByStep.Add(list2);
		}
		references.Add(state.RewardsByStep, RewardsByStep);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("ID", ID);
		info.AddValue("RewardGenerationRNGState", RewardGenerationRNGState);
		info.AddValue("RolledRewards", RolledRewards);
		info.AddValue("PersonalQuestConditionState", PersonalQuestConditionState);
		info.AddValue("IsConcealed", IsConcealed);
		info.AddValue("State", State);
		info.AddValue("LastProgressShown", LastProgressShown);
		info.AddValue("CurrentPersonalQuestStep", CurrentPersonalQuestStep);
		info.AddValue("RewardsByStep", RewardsByStep);
	}

	public CPersonalQuestState(SerializationInfo info, StreamingContext context)
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
				case "RewardGenerationRNGState":
					RewardGenerationRNGState = (Extensions.RandomState)info.GetValue("RewardGenerationRNGState", typeof(Extensions.RandomState));
					break;
				case "RolledRewards":
					RolledRewards = info.GetBoolean("RolledRewards");
					break;
				case "PersonalQuestConditionState":
					PersonalQuestConditionState = (CUnlockConditionState)info.GetValue("PersonalQuestConditionState", typeof(CUnlockConditionState));
					break;
				case "IsConcealed":
					IsConcealed = info.GetBoolean("IsConcealed");
					break;
				case "State":
					State = (EPersonalQuestState)info.GetValue("State", typeof(EPersonalQuestState));
					break;
				case "LastProgressShown":
					LastProgressShown = info.GetInt32("LastProgressShown");
					break;
				case "CurrentPersonalQuestStep":
					CurrentPersonalQuestStep = info.GetInt32("CurrentPersonalQuestStep");
					break;
				case "RewardsByStep":
					RewardsByStep = (List<List<RewardGroup>>)info.GetValue("RewardsByStep", typeof(List<List<RewardGroup>>));
					break;
				case "PersonalQuestStep":
					CurrentPersonalQuestStep = info.GetInt32("PersonalQuestStep");
					break;
				case "GivenRewardsByStep":
					RewardsByStep = (List<List<RewardGroup>>)info.GetValue("GivenRewardsByStep", typeof(List<List<RewardGroup>>));
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CPersonalQuestState entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
	}

	public CPersonalQuestState(PersonalQuestYMLData personalQuestData, string characterID, string characterName)
	{
		ID = personalQuestData.ID;
		RewardsByStep = new List<List<RewardGroup>>();
		ResetPersonalQuestState(characterID, characterName);
		RollPersonalQuestRewards();
		ValidateFinalReward();
		CurrentPersonalQuestStep = 0;
	}

	public void ResetPersonalQuestState(string characterID, string characterName)
	{
		RolledRewards = false;
		IsConcealed = false;
		State = EPersonalQuestState.None;
		LastProgressShown = 0;
		PersonalQuestConditionState = new CUnlockConditionState(CurrentPersonalQuestStepData.PersonalQuestCondition);
		PersonalQuestConditionState.OverrideCharacterID = characterID;
		PersonalQuestConditionState.OverrideCharacterName = characterName;
		PersonalQuestConditionState.IsUnlocked();
	}

	public bool NextPersonalQuestStep()
	{
		if (CurrentPersonalQuestStep < PersonalQuestSteps - 1)
		{
			CurrentPersonalQuestStep++;
			ResetPersonalQuestState(PersonalQuestConditionState.OverrideCharacterID, PersonalQuestConditionState.OverrideCharacterName);
			RolledRewards = true;
			return true;
		}
		return false;
	}

	public void OnMapStateAdventureStarted()
	{
		if (CurrentPersonalQuestStepData != null)
		{
			PersonalQuestConditionState.CacheUnlockCondition(CurrentPersonalQuestStepData.PersonalQuestCondition);
		}
		if (RewardsByStep == null || RewardsByStep.Count < 1 || RewardsByStep.Count < PersonalQuestSteps)
		{
			RolledRewards = false;
			RewardsByStep = new List<List<RewardGroup>>();
		}
		if (RolledRewards)
		{
			if (RewardGenerationRNGState != null)
			{
				if (CurrentPersonalQuestStep < PersonalQuestSteps)
				{
					List<RewardGroup> list = TreasureTableProcessing.RollTreasureTables(RewardGenerationRNGState.Restore(), MapYMLShared.ValidateTreasureTableRewards(PersonalQuestTreasureTables, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter)), AdventureState.MapState.MapParty.ScenarioLevel, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter));
					bool flag = true;
					if (list.Count != CurrentRewards.Count)
					{
						flag = false;
					}
					List<RewardGroup> list2 = new List<RewardGroup>();
					for (int i = 0; i < list.Count; i++)
					{
						if (i < CurrentRewards.Count)
						{
							if (!list[i].Equals(CurrentRewards[i]))
							{
								flag = false;
								list2.Add(list[i]);
							}
						}
						else
						{
							flag = false;
							list2.Add(list[i]);
						}
					}
					if (!flag)
					{
						RewardsByStep[CurrentPersonalQuestStep] = list;
					}
				}
				else
				{
					ValidateFinalReward();
				}
			}
			else
			{
				RewardsByStep.Clear();
				RollPersonalQuestRewards();
			}
		}
		else
		{
			RollPersonalQuestRewards();
		}
	}

	public void CheckPersonalQuest(string characterID)
	{
		ValidateFinalReward();
		if (CurrentPersonalQuestStepData.PersonalQuestCondition != null && PersonalQuestConditionState.IsUnlocked())
		{
			CompletePersonalQuest(characterID);
		}
	}

	public bool ValidateFinalReward()
	{
		if (State == EPersonalQuestState.Completed)
		{
			return false;
		}
		Reward reward = null;
		foreach (RewardGroup finalReward in FinalRewards)
		{
			foreach (Reward reward2 in finalReward.Rewards)
			{
				if (reward2.Type == ETreasureType.UnlockCharacter)
				{
					reward = reward2;
					break;
				}
			}
			if (reward != null)
			{
				break;
			}
		}
		if (reward != null)
		{
			if (AdventureState.MapState.MapParty.UnlockedCharacterIDs.Contains(reward.CharacterID))
			{
				SharedLibrary.Random rng = ((RewardGenerationRNGState == null) ? AdventureState.MapState.MapRNG : RewardGenerationRNGState.Restore());
				RewardsByStep[RewardsByStep.Count - 1] = TreasureTableProcessing.RollTreasureTables(rng, MapYMLShared.ValidateTreasureTableRewards(AdventureState.MapState.HeadquartersState.Headquarters.CharacterUnlockAlternateTreasureTables, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter)), AdventureState.MapState.MapParty.ScenarioLevel, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter));
				SimpleLog.AddToSimpleLog("MapRNG (check final reward for already unlocked characters): " + AdventureState.MapState.PeekMapRNG);
				return true;
			}
		}
		else
		{
			List<RewardGroup> list = TreasureTableProcessing.RollTreasureTables(RewardGenerationRNGState.Restore(), MapYMLShared.ValidateTreasureTableRewards(AdventureState.MapState.HeadquartersState.Headquarters.CharacterUnlockAlternateTreasureTables, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter)), AdventureState.MapState.MapParty.ScenarioLevel, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter));
			bool flag = true;
			if (list.Count != FinalRewards.Count)
			{
				flag = false;
			}
			if (flag)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (i < FinalRewards.Count && !list[i].Equals(FinalRewards[i]))
					{
						flag = false;
					}
				}
			}
			if (!flag)
			{
				RewardsByStep[RewardsByStep.Count - 1] = list;
			}
		}
		return false;
	}

	public void CompletePersonalQuest(string characterID)
	{
		if (State == EPersonalQuestState.Completed)
		{
			return;
		}
		if (!RolledRewards)
		{
			RollPersonalQuestRewards();
		}
		ValidateFinalReward();
		State = EPersonalQuestState.Completed;
		foreach (RewardGroup currentReward in CurrentRewards)
		{
			currentReward.PickedUpBy = characterID;
		}
		AdventureState.MapState.ApplyRewards(CurrentRewards.SelectMany((RewardGroup x) => x.Rewards.Where((Reward it) => it.Type == ETreasureType.UnlockQuest)).ToList(), "party");
		AdventureState.MapState.QueuedCompletionRewards.AddRange(CurrentRewards.SelectMany((RewardGroup x) => x.Rewards.Where((Reward it) => it.Type != ETreasureType.UnlockQuest)));
		if (PersonalQuestSteps == 0 || CurrentPersonalQuestStep >= PersonalQuestSteps - 1)
		{
			foreach (CMapCharacter selectedCharacter in AdventureState.MapState.MapParty.SelectedCharacters)
			{
				selectedCharacter.ExperiencePersonalGoal++;
			}
			AdventureState.MapState.CheckPersonalQuests();
		}
		CPersonalQuestCompleted_MapClientMessage message = new CPersonalQuestCompleted_MapClientMessage(characterID, ID, CurrentPersonalQuestStep);
		if (MapRuleLibraryClient.Instance.MessageHandler != null)
		{
			MapRuleLibraryClient.Instance.MessageHandler(message);
		}
	}

	public void RollPersonalQuestRewards()
	{
		RewardGenerationRNGState = AdventureState.MapState.MapRNG.Save();
		int currentPersonalQuestStep = CurrentPersonalQuestStep;
		if (PersonalQuestSteps > 0)
		{
			for (int i = 0; i < PersonalQuestSteps; i++)
			{
				CurrentPersonalQuestStep = i;
				List<RewardGroup> item = TreasureTableProcessing.RollTreasureTables(AdventureState.MapState.MapRNG, MapYMLShared.ValidateTreasureTableRewards(PersonalQuestTreasureTables, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter)), AdventureState.MapState.MapParty.ScenarioLevel, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter));
				RewardsByStep.Add(item);
				DLLDebug.LogInfo("RollPersonalQuestRewards " + CurrentRewards.Count + " " + PersonalQuestTreasureTables.Count);
			}
		}
		else
		{
			List<RewardGroup> item2 = TreasureTableProcessing.RollTreasureTables(AdventureState.MapState.MapRNG, MapYMLShared.ValidateTreasureTableRewards(PersonalQuestTreasureTables, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter)), AdventureState.MapState.MapParty.ScenarioLevel, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter));
			RewardsByStep.Add(item2);
		}
		CurrentPersonalQuestStep = currentPersonalQuestStep;
		RolledRewards = true;
		if (AdventureState.MapState != null)
		{
			DLLDebug.LogFromSimpleLog("MapRNG (Roll Personal Quest Rewards): " + AdventureState.MapState.PeekMapRNG + "\n" + Environment.StackTrace);
		}
	}

	public void Conceal(bool conceal)
	{
		IsConcealed = conceal;
	}
}
