using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Client;
using MapRuleLibrary.YML.Locations;
using MapRuleLibrary.YML.Quest;
using ScenarioRuleLibrary;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;
using StateCodeGenerator;

namespace MapRuleLibrary.MapState;

[Serializable]
public class CVillageState : CLocationState, ISerializable
{
	public List<CQuestState> QuestStates { get; private set; }

	public List<string> ConnectedVillageIDs { get; private set; }

	public CVillage Village => MapRuleLibraryClient.MRLYML.Villages.SingleOrDefault((CVillage s) => s.ID == base.ID);

	public CVillageState()
	{
	}

	public CVillageState(CVillageState state, ReferenceDictionary references)
		: base(state, references)
	{
		QuestStates = references.Get(state.QuestStates);
		if (QuestStates == null && state.QuestStates != null)
		{
			QuestStates = new List<CQuestState>();
			for (int i = 0; i < state.QuestStates.Count; i++)
			{
				CQuestState cQuestState = state.QuestStates[i];
				CQuestState cQuestState2 = references.Get(cQuestState);
				if (cQuestState2 == null && cQuestState != null)
				{
					CQuestState cQuestState3 = ((!(cQuestState is CJobQuestState state2)) ? new CQuestState(cQuestState, references) : new CJobQuestState(state2, references));
					cQuestState2 = cQuestState3;
					references.Add(cQuestState, cQuestState2);
				}
				QuestStates.Add(cQuestState2);
			}
			references.Add(state.QuestStates, QuestStates);
		}
		ConnectedVillageIDs = references.Get(state.ConnectedVillageIDs);
		if (ConnectedVillageIDs == null && state.ConnectedVillageIDs != null)
		{
			ConnectedVillageIDs = new List<string>();
			for (int j = 0; j < state.ConnectedVillageIDs.Count; j++)
			{
				string item = state.ConnectedVillageIDs[j];
				ConnectedVillageIDs.Add(item);
			}
			references.Add(state.ConnectedVillageIDs, ConnectedVillageIDs);
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("QuestStates", QuestStates);
		info.AddValue("ConnectedVillageIDs", ConnectedVillageIDs);
	}

	public CVillageState(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "QuestStates"))
				{
					if (name == "ConnectedVillageIDs")
					{
						ConnectedVillageIDs = (List<string>)info.GetValue("ConnectedVillageIDs", typeof(List<string>));
					}
				}
				else
				{
					QuestStates = (List<CQuestState>)info.GetValue("QuestStates", typeof(List<CQuestState>));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CVillageState entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		if (base.Location != null)
		{
			if (base.UnlockConditionState == null && base.Location.UnlockCondition != null)
			{
				base.UnlockConditionState = new CUnlockConditionState(base.Location.UnlockCondition);
			}
			if (base.UnlockConditionState != null && base.Location.UnlockCondition != null)
			{
				base.UnlockConditionState.CacheUnlockCondition(base.Location.UnlockCondition);
			}
		}
	}

	public CVillageState(CVillage village)
		: base(village)
	{
		QuestStates = new List<CQuestState>();
		foreach (CQuest quest in village.Quests)
		{
			if (quest.Type != EQuestType.Job)
			{
				QuestStates.Add(new CQuestState(quest));
			}
		}
		ConnectedVillageIDs = new List<string>();
		base.Mesh = village.Mesh;
	}

	public void OnMapStateAdventureStarted()
	{
		foreach (CQuestState questState in QuestStates)
		{
			questState.OnMapStateAdventureStarted();
		}
		foreach (CQuest quest in Village.Quests)
		{
			if (quest.Type != EQuestType.Job && !QuestStates.Any((CQuestState q) => q.Quest?.ID == quest.ID))
			{
				CQuestState cQuestState = new CQuestState(quest);
				AdventureState.MapState.ValidateAddedQuest(cQuestState);
				QuestStates.Add(cQuestState);
				cQuestState.Init();
			}
		}
		List<CQuestState> list = new List<CQuestState>();
		foreach (CQuestState questState2 in QuestStates)
		{
			if (questState2.Quest == null)
			{
				list.Add(questState2);
			}
		}
		foreach (CQuestState item in list)
		{
			QuestStates.Remove(item);
		}
	}

	public List<CJobQuestState> AvailableJobQuests()
	{
		List<CJobQuestState> list = new List<CJobQuestState>();
		list.AddRange(AdventureState.MapState.AllAvailableUnlockedJobQuestStates.Where((CJobQuestState q) => Village.JobQuests.Any((CQuest j) => q.Quest.ID == j.ID)));
		return list;
	}

	public void RollForJobQuest()
	{
		if (Village.JobMapLocations.Count > 0)
		{
			List<CJobQuestState> list = AvailableJobQuests();
			int index = AdventureState.MapState.MapRNG.Next(list.Count);
			CJobQuestState cJobQuestState = list[index];
			int index2 = AdventureState.MapState.MapRNG.Next(Village.JobMapLocations.Count);
			CVector3 cVector = Village.JobMapLocations[index2];
			if (cJobQuestState != null && cVector != null)
			{
				cJobQuestState.SetJobLocationAndTimeout(cVector, base.ID);
				cJobQuestState.Init();
				AdventureState.MapState.CurrentJobQuestStates.Add(cJobQuestState);
			}
			SimpleLog.AddToSimpleLog("MapRNG (roll for job quest): " + AdventureState.MapState.PeekMapRNG);
		}
	}

	public void ConnectVillage(string villageID)
	{
		if (!ConnectedVillageIDs.Contains(villageID))
		{
			ConnectedVillageIDs.Add(villageID);
		}
	}

	public override void UnlockLocation()
	{
		base.UnlockLocation();
		CContentUnlocked_MapClientMessage message = new CContentUnlocked_MapClientMessage(base.ID, "village", Village.UnlockCondition);
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
