using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_ReachRound : CObjective
{
	public int ReachRoundNumber { get; private set; }

	public bool CountFromActivationRound { get; private set; }

	public CObjective_ReachRound()
	{
	}

	public CObjective_ReachRound(CObjective_ReachRound state, ReferenceDictionary references)
		: base(state, references)
	{
		ReachRoundNumber = state.ReachRoundNumber;
		CountFromActivationRound = state.CountFromActivationRound;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ReachRoundNumber", ReachRoundNumber);
		info.AddValue("CountFromActivationRound", CountFromActivationRound);
	}

	public CObjective_ReachRound(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "ReachRoundNumber"))
				{
					if (name == "CountFromActivationRound")
					{
						CountFromActivationRound = info.GetBoolean("CountFromActivationRound");
					}
				}
				else
				{
					ReachRoundNumber = info.GetInt32("ReachRoundNumber");
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CObjective_ReachRound entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjective_ReachRound(EObjectiveResult result, CObjectiveFilter objectiveFilter, int reachRoundNumber, bool countFromActivationRound, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.ReachRound, result, objectiveFilter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
		ReachRoundNumber = reachRoundNumber;
		CountFromActivationRound = countFromActivationRound;
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		int num = (isEndOfRound ? ScenarioManager.CurrentScenarioState.RoundNumber : (ScenarioManager.CurrentScenarioState.RoundNumber - 1));
		int reachRoundNumber = ReachRoundNumber;
		reachRoundNumber += (CountFromActivationRound ? base.RoundActivatedOn : 0);
		base.IsComplete = num >= reachRoundNumber && CheckOtherObjectiveStatesRequirement();
		return base.IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		total = ReachRoundNumber;
		int num = ((!CountFromActivationRound) ? 1 : base.RoundActivatedOn);
		int num2 = ScenarioManager.CurrentScenarioState.RoundNumber - num;
		current = Math.Min(num2, total);
		return Math.Min(1f, (float)num2 / (float)ReachRoundNumber);
	}

	public override int GetObjectiveCompletionValue(int partySize)
	{
		return ReachRoundNumber;
	}
}
