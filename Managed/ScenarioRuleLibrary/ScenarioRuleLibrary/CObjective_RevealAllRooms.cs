using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_RevealAllRooms : CObjective
{
	public override string LocKey
	{
		get
		{
			if (string.IsNullOrEmpty(base.CustomLocKey))
			{
				if (base.Result == EObjectiveResult.Win)
				{
					return "GUI_OBJECTIVE_REVEAL_ALL_ROOMS_WIN";
				}
				if (base.Result == EObjectiveResult.Lose)
				{
					return "GUI_OBJECTIVE_REVEAL_ALL_ROOMS_LOSE";
				}
				return string.Empty;
			}
			return base.CustomLocKey;
		}
	}

	public CObjective_RevealAllRooms()
	{
	}

	public CObjective_RevealAllRooms(CObjective_RevealAllRooms state, ReferenceDictionary references)
		: base(state, references)
	{
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
	}

	public CObjective_RevealAllRooms(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public CObjective_RevealAllRooms(EObjectiveResult result, CObjectiveFilter objectiveFilter, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.RevealAllRooms, result, objectiveFilter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		bool flag = true;
		foreach (CMap map in ScenarioManager.CurrentScenarioState.Maps)
		{
			if (!map.Revealed)
			{
				flag = false;
				break;
			}
		}
		base.IsComplete = flag && CheckOtherObjectiveStatesRequirement();
		return base.IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		current = ScenarioManager.CurrentScenarioState.Maps.Where((CMap m) => m.Revealed).Count();
		total = ScenarioManager.CurrentScenarioState.Maps.Count;
		return Math.Min(1f, (float)current / (float)total);
	}

	public override int GetObjectiveCompletionValue(int partySize)
	{
		return 1;
	}
}
