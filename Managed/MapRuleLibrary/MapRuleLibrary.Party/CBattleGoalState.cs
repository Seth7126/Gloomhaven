using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Client;
using MapRuleLibrary.MapState;
using MapRuleLibrary.YML.BattleGoals;
using MapRuleLibrary.YML.Shared;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;
using StateCodeGenerator;

namespace MapRuleLibrary.Party;

[Serializable]
public class CBattleGoalState : ISerializable
{
	public string ID { get; private set; }

	public List<RewardGroup> Rewards { get; private set; }

	public Extensions.RandomState RewardGenerationRNGState { get; private set; }

	public bool RolledRewards { get; private set; }

	public CUnlockConditionState BattleGoalConditionState { get; private set; }

	public BattleGoalYMLData BattleGoal => MapRuleLibraryClient.MRLYML.BattleGoals.SingleOrDefault((BattleGoalYMLData s) => s.ID == ID);

	public List<TreasureTable> BattleGoalTreasureTables => ScenarioRuleClient.SRLYML.TreasureTables.Where((TreasureTable w) => BattleGoal.TreasureTables.Contains(w.Name)).ToList();

	public CBattleGoalState()
	{
	}

	public CBattleGoalState(CBattleGoalState state, ReferenceDictionary references)
	{
		ID = state.ID;
		Rewards = references.Get(state.Rewards);
		if (Rewards == null && state.Rewards != null)
		{
			Rewards = new List<RewardGroup>();
			for (int i = 0; i < state.Rewards.Count; i++)
			{
				RewardGroup rewardGroup = state.Rewards[i];
				RewardGroup rewardGroup2 = references.Get(rewardGroup);
				if (rewardGroup2 == null && rewardGroup != null)
				{
					rewardGroup2 = new RewardGroup(rewardGroup, references);
					references.Add(rewardGroup, rewardGroup2);
				}
				Rewards.Add(rewardGroup2);
			}
			references.Add(state.Rewards, Rewards);
		}
		RewardGenerationRNGState = references.Get(state.RewardGenerationRNGState);
		if (RewardGenerationRNGState == null && state.RewardGenerationRNGState != null)
		{
			RewardGenerationRNGState = new Extensions.RandomState(state.RewardGenerationRNGState, references);
			references.Add(state.RewardGenerationRNGState, RewardGenerationRNGState);
		}
		RolledRewards = state.RolledRewards;
		BattleGoalConditionState = references.Get(state.BattleGoalConditionState);
		if (BattleGoalConditionState == null && state.BattleGoalConditionState != null)
		{
			BattleGoalConditionState = new CUnlockConditionState(state.BattleGoalConditionState, references);
			references.Add(state.BattleGoalConditionState, BattleGoalConditionState);
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("ID", ID);
		info.AddValue("Rewards", Rewards);
		info.AddValue("RewardGenerationRNGState", RewardGenerationRNGState);
		info.AddValue("RolledRewards", RolledRewards);
		info.AddValue("BattleGoalConditionState", BattleGoalConditionState);
	}

	public CBattleGoalState(SerializationInfo info, StreamingContext context)
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
				case "Rewards":
					Rewards = (List<RewardGroup>)info.GetValue("Rewards", typeof(List<RewardGroup>));
					break;
				case "RewardGenerationRNGState":
					RewardGenerationRNGState = (Extensions.RandomState)info.GetValue("RewardGenerationRNGState", typeof(Extensions.RandomState));
					break;
				case "RolledRewards":
					RolledRewards = info.GetBoolean("RolledRewards");
					break;
				case "BattleGoalConditionState":
					BattleGoalConditionState = (CUnlockConditionState)info.GetValue("BattleGoalConditionState", typeof(CUnlockConditionState));
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CPartyAchievement entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
	}

	public CBattleGoalState(BattleGoalYMLData battleGoalData, CMapCharacter mapCharacter)
	{
		string characterID = mapCharacter.CharacterID;
		ID = battleGoalData.ID;
		Rewards = new List<RewardGroup>();
		RolledRewards = false;
		BattleGoalConditionState = new CUnlockConditionState(BattleGoal.BattleGoalCondition, BattleGoal.BattleGoalFailCondition);
		BattleGoalConditionState.OverrideCharacterID = characterID;
		BattleGoalConditionState.OverrideCharacterName = mapCharacter.CharacterName;
	}

	public void OnMapStateAdventureStarted()
	{
		if (BattleGoal != null)
		{
			BattleGoalConditionState.CacheUnlockCondition(BattleGoal.BattleGoalCondition, BattleGoal.BattleGoalFailCondition);
		}
		if (!RolledRewards)
		{
			return;
		}
		if (RewardGenerationRNGState != null)
		{
			List<RewardGroup> list = TreasureTableProcessing.RollTreasureTables(RewardGenerationRNGState.Restore(), MapYMLShared.ValidateTreasureTableRewards(BattleGoalTreasureTables, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter)), AdventureState.MapState.MapParty.ScenarioLevel, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter));
			bool flag = true;
			if (list.Count != Rewards.Count)
			{
				flag = false;
			}
			List<RewardGroup> list2 = new List<RewardGroup>();
			for (int i = 0; i < list.Count; i++)
			{
				if (i < Rewards.Count)
				{
					if (!list[i].Equals(Rewards[i]))
					{
						flag = false;
						list2.Add(list[i]);
					}
				}
				else
				{
					list2.Add(list[i]);
				}
			}
			if (!flag)
			{
				Rewards = list;
			}
		}
		else
		{
			RollBattleGoalRewards();
		}
	}

	public void CheckBattleGoal(string characterID, bool useCurrentScenarioStats = false)
	{
		if (BattleGoal.BattleGoalCondition == null)
		{
			return;
		}
		BattleGoalConditionState.ResetProgress();
		if (!useCurrentScenarioStats || !BattleGoal.CheckAtEndOfScenario)
		{
			if (BattleGoalConditionState.IsUnlocked(useCurrentScenarioStats) && !useCurrentScenarioStats)
			{
				CompleteBattleGoal(characterID);
			}
		}
		else if (useCurrentScenarioStats && BattleGoalConditionState.FailCondition != null)
		{
			BattleGoalConditionState.Failed = BattleGoalConditionState.CheckUnlocked(useCurrentScenarioStats, BattleGoalConditionState.FailCondition, BattleGoalConditionState.FailConditionTargetStates, checkingFailCondition: true);
		}
	}

	public void CompleteBattleGoal(string characterID)
	{
		if (!RolledRewards)
		{
			RollBattleGoalRewards();
		}
		foreach (RewardGroup reward in Rewards)
		{
			reward.PickedUpBy = characterID;
		}
		AdventureState.MapState.ApplyRewards(Rewards, battleGoal: true);
	}

	public void RollBattleGoalRewards()
	{
		RewardGenerationRNGState = AdventureState.MapState.MapRNG.Save();
		List<RewardGroup> rewards = TreasureTableProcessing.RollTreasureTables(AdventureState.MapState.MapRNG, MapYMLShared.ValidateTreasureTableRewards(BattleGoalTreasureTables, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter)), AdventureState.MapState.MapParty.ScenarioLevel, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter));
		Rewards = rewards;
		RolledRewards = true;
		SimpleLog.AddToSimpleLog("MapRNG (roll BG rewards): " + AdventureState.MapState.PeekMapRNG);
	}

	public void ResetBattleGoalProgress()
	{
		if (BattleGoal.BattleGoalCondition != null)
		{
			BattleGoalConditionState.ResetProgress();
		}
	}
}
