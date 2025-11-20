using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Client;
using MapRuleLibrary.MapState;
using MapRuleLibrary.YML.Achievements;
using MapRuleLibrary.YML.Shared;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;
using StateCodeGenerator;

namespace MapRuleLibrary.Party;

[Serializable]
public class CPartyAchievement : ISerializable
{
	private AchievementYMLData m_CachedAchievementData;

	public string ID { get; private set; }

	public EAchievementState State { get; private set; }

	public List<RewardGroup> Rewards { get; private set; }

	public Extensions.RandomState RewardGenerationRNGState { get; private set; }

	public DateTime UnlockedTimeStamp { get; private set; }

	public bool RolledRewards { get; private set; }

	public DateTime CompletedTimeStamp { get; private set; }

	public CUnlockConditionState UnlockConditionState { get; private set; }

	public CUnlockConditionState AchievementConditionState { get; private set; }

	public bool IsNew { get; set; }

	public AchievementYMLData Achievement
	{
		get
		{
			if (m_CachedAchievementData == null)
			{
				m_CachedAchievementData = MapRuleLibraryClient.MRLYML.Achievements.SingleOrDefault((AchievementYMLData s) => s.ID == ID);
			}
			_ = m_CachedAchievementData;
			return m_CachedAchievementData;
		}
	}

	public List<TreasureTable> AchievementTreasureTables => ScenarioRuleClient.SRLYML.TreasureTables.Where((TreasureTable w) => Achievement.TreasureTables.Contains(w.Name)).ToList();

	public CPartyAchievement()
	{
	}

	public CPartyAchievement(CPartyAchievement state, ReferenceDictionary references)
	{
		ID = state.ID;
		State = state.State;
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
		UnlockedTimeStamp = state.UnlockedTimeStamp;
		RolledRewards = state.RolledRewards;
		CompletedTimeStamp = state.CompletedTimeStamp;
		UnlockConditionState = references.Get(state.UnlockConditionState);
		if (UnlockConditionState == null && state.UnlockConditionState != null)
		{
			UnlockConditionState = new CUnlockConditionState(state.UnlockConditionState, references);
			references.Add(state.UnlockConditionState, UnlockConditionState);
		}
		AchievementConditionState = references.Get(state.AchievementConditionState);
		if (AchievementConditionState == null && state.AchievementConditionState != null)
		{
			AchievementConditionState = new CUnlockConditionState(state.AchievementConditionState, references);
			references.Add(state.AchievementConditionState, AchievementConditionState);
		}
		IsNew = state.IsNew;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("ID", ID);
		info.AddValue("State", State);
		info.AddValue("Rewards", Rewards);
		info.AddValue("RewardGenerationRNGState", RewardGenerationRNGState);
		info.AddValue("UnlockedTimeStamp", UnlockedTimeStamp);
		info.AddValue("RolledRewards", RolledRewards);
		info.AddValue("CompletedTimeStamp", CompletedTimeStamp);
		info.AddValue("UnlockConditionState", UnlockConditionState);
		info.AddValue("AchievementConditionState", AchievementConditionState);
	}

	public CPartyAchievement(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "Name":
					ID = info.GetString("Name");
					break;
				case "ID":
					ID = info.GetString("ID");
					break;
				case "State":
					State = (EAchievementState)info.GetValue("State", typeof(EAchievementState));
					break;
				case "Rewards":
					Rewards = (List<RewardGroup>)info.GetValue("Rewards", typeof(List<RewardGroup>));
					break;
				case "RewardGenerationRNGState":
					RewardGenerationRNGState = (Extensions.RandomState)info.GetValue("RewardGenerationRNGState", typeof(Extensions.RandomState));
					break;
				case "UnlockedTimeStamp":
					UnlockedTimeStamp = (DateTime)info.GetValue("UnlockedTimeStamp", typeof(DateTime));
					break;
				case "RolledRewards":
					RolledRewards = info.GetBoolean("RolledRewards");
					break;
				case "CompletedTimeStamp":
					CompletedTimeStamp = (DateTime)info.GetValue("CompletedTimeStamp", typeof(DateTime));
					break;
				case "UnlockConditionState":
					UnlockConditionState = (CUnlockConditionState)info.GetValue("UnlockConditionState", typeof(CUnlockConditionState));
					break;
				case "AchievementConditionState":
					AchievementConditionState = (CUnlockConditionState)info.GetValue("AchievementConditionState", typeof(CUnlockConditionState));
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

	public CPartyAchievement(AchievementYMLData achievement)
	{
		ID = achievement.ID;
		State = EAchievementState.Locked;
		Rewards = new List<RewardGroup>();
		RolledRewards = false;
		IsNew = true;
		UnlockConditionState = new CUnlockConditionState(achievement.UnlockCondition);
		AchievementConditionState = new CUnlockConditionState(achievement.AchievementCondition);
	}

	public void RefreshAchievement()
	{
		UnlockConditionState = new CUnlockConditionState(Achievement.UnlockCondition);
		AchievementConditionState = new CUnlockConditionState(Achievement.AchievementCondition);
	}

	public void OnMapStateAdventureStarted()
	{
		if (Achievement != null)
		{
			if (UnlockConditionState == null)
			{
				UnlockConditionState = new CUnlockConditionState(Achievement.UnlockCondition);
			}
			if (AchievementConditionState == null)
			{
				AchievementConditionState = new CUnlockConditionState(Achievement.AchievementCondition);
			}
			UnlockConditionState.CacheUnlockCondition(Achievement.UnlockCondition);
			AchievementConditionState.CacheUnlockCondition(Achievement.AchievementCondition);
		}
		if (!RolledRewards)
		{
			return;
		}
		if (RewardGenerationRNGState != null)
		{
			List<RewardGroup> list = TreasureTableProcessing.RollTreasureTables(RewardGenerationRNGState.Restore(), MapYMLShared.ValidateTreasureTableRewards(AchievementTreasureTables, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter)), AdventureState.MapState.MapParty.ScenarioLevel, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter));
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
				if (State == EAchievementState.RewardsClaimed)
				{
					AdventureState.MapState.ApplyRewards(list2);
				}
			}
			if (State == EAchievementState.RewardsClaimed && flag && list.Any((RewardGroup r) => r.CharacterIDs.Count > 0))
			{
				List<RewardGroup> rewardGroups = list.FindAll((RewardGroup r) => r.CharacterIDs.Count > 0);
				AdventureState.MapState.ApplyRewards(rewardGroups);
			}
		}
		else
		{
			RollAchievementRewards();
		}
	}

	public void CheckAchievement(bool OverrideLock = false)
	{
		try
		{
			if (State == EAchievementState.Locked && (UnlockConditionState.IsUnlocked() || OverrideLock))
			{
				UnlockAchievement();
			}
			if (State == EAchievementState.Unlocked && Achievement.AchievementCondition != null && AchievementConditionState.IsUnlocked())
			{
				CompleteAchievement();
			}
		}
		catch (Exception ex)
		{
			DLLDebug.LogError("Exception checking achievement progress for Achievement ID: " + ID + "\n" + ex.Message);
		}
	}

	public void LockAchievement()
	{
		State = EAchievementState.Locked;
	}

	public void UnlockAchievement()
	{
		State = EAchievementState.Unlocked;
		if (!RolledRewards)
		{
			RollAchievementRewards();
		}
		UnlockedTimeStamp = DateTime.Now.AddSeconds(1.0);
		DLLDebug.LogInfo("Unlocked achievement for Achievement Name: " + ID);
		CContentUnlocked_MapClientMessage message = new CContentUnlocked_MapClientMessage(ID, "achievement", Achievement.UnlockCondition);
		if (MapRuleLibraryClient.Instance.MessageHandler != null)
		{
			MapRuleLibraryClient.Instance.MessageHandler(message);
		}
		else
		{
			DLLDebug.LogWarning("Message handler not set");
		}
	}

	public void CompleteAchievement()
	{
		if (State == EAchievementState.Completed)
		{
			return;
		}
		State = EAchievementState.Completed;
		CompletedTimeStamp = DateTime.Now;
		if (Achievement.AchievementType == EAchievementType.Trophy)
		{
			if (AdventureState.MapState.IsInScenarioPhase)
			{
				AdventureState.MapState.QueuedPlatformAchievementsToUnlock.Add(ID);
			}
			else
			{
				CPostTrophyAchievement_MapClientMessage message = new CPostTrophyAchievement_MapClientMessage(new List<string> { ID });
				if (MapRuleLibraryClient.Instance?.MessageHandler != null)
				{
					MapRuleLibraryClient.Instance.MessageHandler(message);
				}
				else
				{
					AdventureState.MapState.QueuedPlatformAchievementsToUnlock.Add(ID);
				}
			}
		}
		if (!RolledRewards)
		{
			RollAchievementRewards();
		}
		foreach (RewardGroup reward in Rewards)
		{
			reward.PickedUpBy = "party";
		}
		SimpleLog.AddToSimpleLog("Completed achievement for Achievement Type: " + Achievement.AchievementType.ToString() + ", Achievement Name: " + ID);
		if (Achievement.AchievementType == EAchievementType.Campaign)
		{
			AdventureState.MapState.QueuedCompletionRewards.AddRange(Rewards.SelectMany((RewardGroup x) => x.Rewards).ToList().FindAll((Reward it) => it.Type != ETreasureType.UnlockCharacter || !AdventureState.MapState.MapParty.UnlockedCharacterIDs.Contains(it.CharacterID)));
		}
		CAchievementCompleted_MapClientMessage message2 = new CAchievementCompleted_MapClientMessage(ID);
		if (MapRuleLibraryClient.Instance.MessageHandler != null)
		{
			MapRuleLibraryClient.Instance.MessageHandler(message2);
		}
		else
		{
			DLLDebug.LogWarning("Message handler not set");
		}
	}

	public void ClaimRewards(bool applyRewards = true)
	{
		State = EAchievementState.RewardsClaimed;
		if (applyRewards)
		{
			AdventureState.MapState.ApplyRewards(Rewards);
		}
		AdventureState.MapState.CheckLockedContent();
		DLLDebug.LogInfo("Claimed achievement rewards for Achievement Name: " + ID);
	}

	public void RollAchievementRewards()
	{
		RewardGenerationRNGState = AdventureState.MapState.MapRNG.Save();
		List<RewardGroup> list = TreasureTableProcessing.RollTreasureTables(AdventureState.MapState.MapRNG, MapYMLShared.ValidateTreasureTableRewards(AchievementTreasureTables, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter)), AdventureState.MapState.MapParty.ScenarioLevel, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter));
		if (State == EAchievementState.RewardsClaimed)
		{
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
				AdventureState.MapState.ApplyRewards(list2);
			}
		}
		Rewards = list;
		RolledRewards = true;
		SimpleLog.AddToSimpleLog("MapRNG (roll achievement reward): " + AdventureState.MapState.PeekMapRNG);
	}

	public bool IsTriggeredBy(CAchievementTrigger trigger)
	{
		AchievementYMLData achievement = Achievement;
		if (achievement == null || achievement.AchievementType != EAchievementType.Trophy)
		{
			return false;
		}
		switch (trigger.Type)
		{
		case CAchievementTrigger.EAchievementTriggerType.QuestCompleted:
		{
			CQuestCompleted_AchievementTrigger questCompletedTrigger = (CQuestCompleted_AchievementTrigger)trigger;
			CUnlockCondition achievementCondition2 = Achievement.AchievementCondition;
			List<Tuple<EUnlockConditionType, string>> list = achievementCondition2.RequiredConditions ?? new List<Tuple<EUnlockConditionType, string>>();
			List<CUnlockChoiceContainer> requiredChoiceContainer = achievementCondition2.RequiredChoiceContainer;
			if (requiredChoiceContainer != null && requiredChoiceContainer.Count > 0)
			{
				list = list.Concat(requiredChoiceContainer.SelectMany((CUnlockChoiceContainer c) => c.RequiredConditions)).ToList();
			}
			return list.Any((Tuple<EUnlockConditionType, string> condition) => condition.Item1 == EUnlockConditionType.CompletedQuest && condition.Item2 == questCompletedTrigger.QuestId);
		}
		case CAchievementTrigger.EAchievementTriggerType.CharacterRetired:
			if (!Achievement.AchievementCondition.RequiredConditions.Any((Tuple<EUnlockConditionType, string> condition) => condition.Item1 == EUnlockConditionType.Retirement))
			{
				return Achievement.AchievementCondition.RequiredConditionsCount.Any((Tuple<EUnlockConditionType, int> condition) => condition.Item1 == EUnlockConditionType.Retirement);
			}
			return true;
		case CAchievementTrigger.EAchievementTriggerType.EnhancementAdded:
		{
			bool flag = false;
			CUnlockCondition achievementCondition = Achievement.AchievementCondition;
			List<CUnlockConditionTarget> targets2 = achievementCondition.Targets;
			if (targets2 != null && targets2.Count > 0)
			{
				flag = targets2.Any((CUnlockConditionTarget c) => c.Filter == EUnlockConditionTargetFilter.Enhancement);
			}
			List<Tuple<EUnlockConditionType, int>> requiredConditionsCount = achievementCondition.RequiredConditionsCount;
			if (requiredConditionsCount != null && requiredConditionsCount.Count > 0)
			{
				flag |= requiredConditionsCount.Any((Tuple<EUnlockConditionType, int> condition) => condition.Item1 == EUnlockConditionType.EnhancedMercenaries);
			}
			return flag;
		}
		case CAchievementTrigger.EAchievementTriggerType.ScenarioEnded:
		{
			bool flag2 = false;
			List<CUnlockConditionTarget> targets3 = AchievementConditionState.UnlockCondition.Targets;
			if (targets3 != null && targets3.Count > 0)
			{
				flag2 |= targets3.Any((CUnlockConditionTarget t) => !string.IsNullOrEmpty(t.ScenarioResult));
				flag2 |= targets3.Any(delegate(CUnlockConditionTarget t)
				{
					EUnlockConditionTargetSubFilter subFilter = t.SubFilter;
					return subFilter == EUnlockConditionTargetSubFilter.Scenario || subFilter == EUnlockConditionTargetSubFilter.SingleScenario || subFilter == EUnlockConditionTargetSubFilter.Round || subFilter == EUnlockConditionTargetSubFilter.RoundNoReset;
				});
			}
			return flag2;
		}
		case CAchievementTrigger.EAchievementTriggerType.LevelUp:
		{
			List<Tuple<string, int>> requiredHeroes = AchievementConditionState.UnlockCondition.RequiredHeroes;
			if (requiredHeroes == null)
			{
				return false;
			}
			return requiredHeroes.Count > 0;
		}
		case CAchievementTrigger.EAchievementTriggerType.UnlockClass:
		{
			List<Tuple<EUnlockConditionType, string>> requiredConditions = AchievementConditionState.UnlockCondition.RequiredConditions;
			if (requiredConditions != null && requiredConditions.Count > 0)
			{
				return requiredConditions.Any((Tuple<EUnlockConditionType, string> condition) => condition.Item1 == EUnlockConditionType.UnlockClass);
			}
			return false;
		}
		case CAchievementTrigger.EAchievementTriggerType.LootChest:
		{
			List<CUnlockConditionTarget> targets = AchievementConditionState.UnlockCondition.Targets;
			if (targets != null && targets.Count > 0)
			{
				return targets.Any((CUnlockConditionTarget target) => target.Filter == EUnlockConditionTargetFilter.Chests);
			}
			return false;
		}
		default:
			return false;
		}
	}
}
