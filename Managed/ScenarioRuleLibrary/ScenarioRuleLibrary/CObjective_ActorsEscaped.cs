using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_ActorsEscaped : CObjective
{
	public List<int> TargetNumberOfEscapees { get; private set; }

	public List<TileIndex> EscapePositions { get; private set; }

	public int CurrentEscapedAmount { get; private set; }

	public override bool RemovesFromUIOnComplete => false;

	public CObjective_ActorsEscaped()
	{
	}

	public CObjective_ActorsEscaped(CObjective_ActorsEscaped state, ReferenceDictionary references)
		: base(state, references)
	{
		TargetNumberOfEscapees = references.Get(state.TargetNumberOfEscapees);
		if (TargetNumberOfEscapees == null && state.TargetNumberOfEscapees != null)
		{
			TargetNumberOfEscapees = new List<int>();
			for (int i = 0; i < state.TargetNumberOfEscapees.Count; i++)
			{
				int item = state.TargetNumberOfEscapees[i];
				TargetNumberOfEscapees.Add(item);
			}
			references.Add(state.TargetNumberOfEscapees, TargetNumberOfEscapees);
		}
		EscapePositions = references.Get(state.EscapePositions);
		if (EscapePositions == null && state.EscapePositions != null)
		{
			EscapePositions = new List<TileIndex>();
			for (int j = 0; j < state.EscapePositions.Count; j++)
			{
				TileIndex tileIndex = state.EscapePositions[j];
				TileIndex tileIndex2 = references.Get(tileIndex);
				if (tileIndex2 == null && tileIndex != null)
				{
					tileIndex2 = new TileIndex(tileIndex, references);
					references.Add(tileIndex, tileIndex2);
				}
				EscapePositions.Add(tileIndex2);
			}
			references.Add(state.EscapePositions, EscapePositions);
		}
		CurrentEscapedAmount = state.CurrentEscapedAmount;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("TargetNumberOfEscapees", TargetNumberOfEscapees);
		info.AddValue("EscapePositions", EscapePositions);
		info.AddValue("CurrentEscapedAmount", CurrentEscapedAmount);
	}

	public CObjective_ActorsEscaped(SerializationInfo info, StreamingContext context)
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
				case "TargetNumberOfEscapees":
					TargetNumberOfEscapees = (List<int>)info.GetValue("TargetNumberOfEscapees", typeof(List<int>));
					break;
				case "EscapePositions":
					EscapePositions = (List<TileIndex>)info.GetValue("EscapePositions", typeof(List<TileIndex>));
					break;
				case "CurrentEscapedAmount":
					CurrentEscapedAmount = info.GetInt32("CurrentEscapedAmount");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CObjective_ActorsEscaped entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjective_ActorsEscaped(EObjectiveResult result, CObjectiveFilter objectiveFilter, List<int> targetNumberOfEscapees, List<TileIndex> actorEscapePositions, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.ActorsEscaped, result, objectiveFilter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
		TargetNumberOfEscapees = targetNumberOfEscapees;
		EscapePositions = actorEscapePositions;
		CurrentEscapedAmount = 0;
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		int num = 0;
		foreach (ActorState actorState in ScenarioManager.CurrentScenarioState.ActorStates)
		{
			CActor cActor = ScenarioManager.Scenario.FindActor(actorState.ActorGuid);
			if (cActor != null)
			{
				if (base.ObjectiveFilter.IsValidTarget(actorState) && cActor.CauseOfDeath == CActor.ECauseOfDeath.ActorRemovedFromMap)
				{
					num++;
				}
			}
			else if (actorState.CauseOfDeath == CActor.ECauseOfDeath.ActorRemovedFromMap)
			{
				num++;
			}
		}
		CurrentEscapedAmount = num;
		int index = Math.Max(0, partySize - 1);
		base.IsComplete = num >= TargetNumberOfEscapees[index] && CheckOtherObjectiveStatesRequirement();
		return base.IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		int index = Math.Max(0, partySize - 1);
		total = TargetNumberOfEscapees[index];
		current = Math.Min(CurrentEscapedAmount, TargetNumberOfEscapees[index]);
		return Math.Min(1f, (float)CurrentEscapedAmount / (float)TargetNumberOfEscapees[index]);
	}
}
