using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_ActorReachPosition : CObjective
{
	public List<int> ReachPositionAmounts { get; private set; }

	public List<TileIndex> ActorTargetPositions { get; private set; }

	public int CurrentReachedPosition { get; private set; }

	public override bool RemovesFromUIOnComplete => false;

	public CObjective_ActorReachPosition()
	{
	}

	public CObjective_ActorReachPosition(CObjective_ActorReachPosition state, ReferenceDictionary references)
		: base(state, references)
	{
		ReachPositionAmounts = references.Get(state.ReachPositionAmounts);
		if (ReachPositionAmounts == null && state.ReachPositionAmounts != null)
		{
			ReachPositionAmounts = new List<int>();
			for (int i = 0; i < state.ReachPositionAmounts.Count; i++)
			{
				int item = state.ReachPositionAmounts[i];
				ReachPositionAmounts.Add(item);
			}
			references.Add(state.ReachPositionAmounts, ReachPositionAmounts);
		}
		ActorTargetPositions = references.Get(state.ActorTargetPositions);
		if (ActorTargetPositions == null && state.ActorTargetPositions != null)
		{
			ActorTargetPositions = new List<TileIndex>();
			for (int j = 0; j < state.ActorTargetPositions.Count; j++)
			{
				TileIndex tileIndex = state.ActorTargetPositions[j];
				TileIndex tileIndex2 = references.Get(tileIndex);
				if (tileIndex2 == null && tileIndex != null)
				{
					tileIndex2 = new TileIndex(tileIndex, references);
					references.Add(tileIndex, tileIndex2);
				}
				ActorTargetPositions.Add(tileIndex2);
			}
			references.Add(state.ActorTargetPositions, ActorTargetPositions);
		}
		CurrentReachedPosition = state.CurrentReachedPosition;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ReachPositionAmounts", ReachPositionAmounts);
		info.AddValue("ActorTargetPositions", ActorTargetPositions);
		info.AddValue("CurrentReachedPosition", CurrentReachedPosition);
	}

	public CObjective_ActorReachPosition(SerializationInfo info, StreamingContext context)
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
				case "ReachPositionAmounts":
					ReachPositionAmounts = (List<int>)info.GetValue("ReachPositionAmounts", typeof(List<int>));
					break;
				case "ActorTargetPositions":
					ActorTargetPositions = (List<TileIndex>)info.GetValue("ActorTargetPositions", typeof(List<TileIndex>));
					break;
				case "CurrentReachedPosition":
					CurrentReachedPosition = info.GetInt32("CurrentReachedPosition");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CObjective_ActorReachPosition entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjective_ActorReachPosition(EObjectiveResult result, CObjectiveFilter objectiveFilter, List<int> reachPositionAmounts, List<TileIndex> actorTargetPositions, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.ActorReachPosition, result, objectiveFilter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
		ReachPositionAmounts = reachPositionAmounts;
		ActorTargetPositions = actorTargetPositions;
		CurrentReachedPosition = 0;
	}

	public void AddExitDungeonPositions(List<TileIndex> exitDungeonIndexes)
	{
		ActorTargetPositions.AddRange(exitDungeonIndexes);
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		int num = 0;
		foreach (PlayerState player in ScenarioManager.CurrentScenarioState.Players)
		{
			if (player.Player != null && base.ObjectiveFilter.IsValidTarget(player))
			{
				if (ActorTargetPositions.Any((TileIndex t) => t.X == player.Player.ArrayIndex.X && t.Y == player.Player.ArrayIndex.Y))
				{
					num++;
				}
				else if (player.Player.IsDeadForObjectives)
				{
					base.UnableToComplete = true;
				}
			}
		}
		CurrentReachedPosition = num;
		int index = Math.Max(0, partySize - 1);
		base.IsComplete = num >= ReachPositionAmounts[index] && CheckOtherObjectiveStatesRequirement();
		return base.IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		int index = Math.Max(0, partySize - 1);
		total = ReachPositionAmounts[index];
		current = Math.Min(CurrentReachedPosition, ReachPositionAmounts[index]);
		return Math.Min(1f, (float)CurrentReachedPosition / (float)ReachPositionAmounts[index]);
	}
}
