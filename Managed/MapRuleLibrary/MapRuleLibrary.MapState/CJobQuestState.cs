using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.YML.Quest;
using ScenarioRuleLibrary;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;
using StateCodeGenerator;

namespace MapRuleLibrary.MapState;

[Serializable]
public class CJobQuestState : CQuestState
{
	public int QuestsRemainingUntilTimeout { get; private set; }

	public CVector3 JobMapLocation { get; private set; }

	public string JobVillageID { get; private set; }

	public CJobQuestState()
	{
	}

	public CJobQuestState(CJobQuestState state, ReferenceDictionary references)
		: base(state, references)
	{
		QuestsRemainingUntilTimeout = state.QuestsRemainingUntilTimeout;
		JobMapLocation = references.Get(state.JobMapLocation);
		if (JobMapLocation == null && state.JobMapLocation != null)
		{
			JobMapLocation = new CVector3(state.JobMapLocation, references);
			references.Add(state.JobMapLocation, JobMapLocation);
		}
		JobVillageID = state.JobVillageID;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("QuestsRemainingUntilTimeout", QuestsRemainingUntilTimeout);
		info.AddValue("JobMapLocation", JobMapLocation);
		info.AddValue("JobVillageID", JobVillageID);
	}

	public CJobQuestState(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "QuestsRemainingUntilTimeout":
					QuestsRemainingUntilTimeout = info.GetInt32("QuestsRemainingUntilTimeout");
					break;
				case "JobMapLocation":
					JobMapLocation = (CVector3)info.GetValue("JobMapLocation", typeof(CVector3));
					break;
				case "JobVillageID":
					JobVillageID = info.GetString("JobVillageID");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CJobQuestState entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	protected override void onDeserialized(StreamingContext context)
	{
		base.onDeserialized(context);
		if (base.QuestState == EQuestState.Completed)
		{
			base.QuestState = EQuestState.Unlocked;
		}
	}

	public CJobQuestState(CQuest quest, CVector3 jobMapLocation)
		: base(quest)
	{
		JobMapLocation = jobMapLocation;
		QuestsRemainingUntilTimeout = 1;
		base.QuestState = EQuestState.Locked;
	}

	public override void Init()
	{
		base.Init();
	}

	protected override void UpdateQuestCompletionInternal(bool autoComplete = false)
	{
		base.UpdateQuestCompletionInternal(autoComplete);
		if (base.QuestState == EQuestState.Completed)
		{
			AdventureState.MapState.CurrentJobQuestStates.Remove(this);
			AdventureState.MapState.PreviousJobQuestStates.Add(this);
			if (AdventureState.MapState.PreviousJobQuestStates.Count > 3)
			{
				AdventureState.MapState.PreviousJobQuestStates.RemoveAt(0);
			}
			if (base.ScenarioState != null)
			{
				base.ScenarioState.RemoveAssociatedDataFromMapState();
			}
			ResetQuest();
			RollQuestRewards(AdventureState.MapState.MapRNG);
		}
		SimpleLog.AddToSimpleLog("MapRNG (update quest completion): " + AdventureState.MapState.PeekMapRNG);
	}

	public void CheckQuestTimeout()
	{
		if (base.InProgress)
		{
			return;
		}
		QuestsRemainingUntilTimeout--;
		if (QuestsRemainingUntilTimeout <= 0)
		{
			AdventureState.MapState.CurrentJobQuestStates.Remove(this);
			AdventureState.MapState.PreviousJobQuestStates.Add(this);
			if (AdventureState.MapState.PreviousJobQuestStates.Count > 3)
			{
				AdventureState.MapState.PreviousJobQuestStates.RemoveAt(0);
			}
			if (base.ScenarioState != null)
			{
				base.ScenarioState.RemoveAssociatedDataFromMapState();
			}
			ResetQuest();
			RollQuestRewards(AdventureState.MapState.MapRNG);
		}
		SimpleLog.AddToSimpleLog("MapRNG (Check Quest timeout): " + AdventureState.MapState.PeekMapRNG);
	}

	public void SetJobLocationAndTimeout(CVector3 jobMapLocation, string villageID)
	{
		JobMapLocation = jobMapLocation;
		JobVillageID = villageID;
		QuestsRemainingUntilTimeout = 1 + AdventureState.MapState.MapRNG.Next(4);
		SimpleLog.AddToSimpleLog("MapRNG (set job location): " + AdventureState.MapState.PeekMapRNG);
	}
}
