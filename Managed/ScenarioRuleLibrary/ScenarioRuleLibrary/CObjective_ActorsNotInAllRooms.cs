using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_ActorsNotInAllRooms : CObjective
{
	private bool m_LastCheckedCompletionValue;

	public List<string> RoomMapGUIDs { get; private set; }

	public override bool RemovesFromUIOnComplete => false;

	public CObjective_ActorsNotInAllRooms()
	{
	}

	public CObjective_ActorsNotInAllRooms(CObjective_ActorsNotInAllRooms state, ReferenceDictionary references)
		: base(state, references)
	{
		RoomMapGUIDs = references.Get(state.RoomMapGUIDs);
		if (RoomMapGUIDs == null && state.RoomMapGUIDs != null)
		{
			RoomMapGUIDs = new List<string>();
			for (int i = 0; i < state.RoomMapGUIDs.Count; i++)
			{
				string item = state.RoomMapGUIDs[i];
				RoomMapGUIDs.Add(item);
			}
			references.Add(state.RoomMapGUIDs, RoomMapGUIDs);
		}
		m_LastCheckedCompletionValue = state.m_LastCheckedCompletionValue;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("RoomMapGUIDs", RoomMapGUIDs);
	}

	public CObjective_ActorsNotInAllRooms(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				if (current.Name == "RoomMapGUIDs")
				{
					RoomMapGUIDs = (List<string>)info.GetValue("RoomMapGUIDs", typeof(List<string>));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CObjective_ActorsNotInAllRooms entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjective_ActorsNotInAllRooms(EObjectiveResult result, CObjectiveFilter objectiveFilter, List<string> roomMapGUIDs, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.ActorsNotInAllRooms, result, objectiveFilter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
		RoomMapGUIDs = roomMapGUIDs;
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		List<CMap> list = new List<CMap>();
		foreach (ActorState actorState in ScenarioManager.CurrentScenarioState.ActorStates)
		{
			if (!base.ObjectiveFilter.IsValidTarget(actorState))
			{
				continue;
			}
			CActor cActor = ScenarioManager.Scenario.FindActor(actorState.ActorGuid);
			if (cActor != null)
			{
				CTile cTile = ScenarioManager.Tiles[cActor.ArrayIndex.X, cActor.ArrayIndex.Y];
				if (cTile.m_HexMap != null && RoomMapGUIDs.Contains(cTile.m_HexMap.MapGuid) && !list.Contains(cTile.m_HexMap))
				{
					list.Add(cTile.m_HexMap);
				}
			}
		}
		m_LastCheckedCompletionValue = RoomMapGUIDs.Count != list.Count && CheckOtherObjectiveStatesRequirement();
		base.IsComplete = m_LastCheckedCompletionValue;
		return base.IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		total = 1;
		current = (m_LastCheckedCompletionValue ? 1 : 0);
		return m_LastCheckedCompletionValue ? 1 : 0;
	}
}
