using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_AllCharactersMustLoot : CObjective
{
	public int CurrentCharactersLooted { get; private set; }

	public override string LocKey
	{
		get
		{
			if (string.IsNullOrEmpty(base.CustomLocKey))
			{
				if (base.Result == EObjectiveResult.Win)
				{
					return "GUI_OBJECTIVE_ALL_CHARACTERS_MUST_LOOT_WIN";
				}
				if (base.Result == EObjectiveResult.Lose)
				{
					return "GUI_OBJECTIVE_ALL_CHARACTERS_MUST_LOOT_LOSE";
				}
				return string.Empty;
			}
			return base.CustomLocKey;
		}
	}

	public CObjective_AllCharactersMustLoot()
	{
	}

	public CObjective_AllCharactersMustLoot(CObjective_AllCharactersMustLoot state, ReferenceDictionary references)
		: base(state, references)
	{
		CurrentCharactersLooted = state.CurrentCharactersLooted;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("CurrentCharactersLooted", CurrentCharactersLooted);
	}

	public CObjective_AllCharactersMustLoot(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				if (current.Name == "CurrentCharactersLooted")
				{
					CurrentCharactersLooted = info.GetInt32("CurrentCharactersLooted");
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CObjective_AllCharactersMustLoot entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjective_AllCharactersMustLoot(EObjectiveResult result, CObjectiveFilter objectiveFilter, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.AllCharactersMustLoot, result, objectiveFilter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		List<CObjectProp> list = ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp p) => p.IsLootable && p.GetConfigForPartySize(partySize) != ScenarioManager.EPerPartySizeConfig.Hidden && base.ObjectiveFilter.IsValidLootTarget(p.ObjectType)).ToList();
		List<CObjectProp> source = ScenarioManager.CurrentScenarioState.ActivatedProps.Where((CObjectProp p) => p.IsLootable && base.ObjectiveFilter.IsValidLootTarget(p.ObjectType)).ToList();
		int num = 0;
		bool flag = true;
		foreach (PlayerState player in ScenarioManager.CurrentScenarioState.Players)
		{
			bool flag2 = false;
			flag2 = ScenarioManager.Scenario.AllPlayers.FirstOrDefault((CPlayerActor p) => p.ActorGuid == player.ActorGuid)?.IsDeadForObjectives ?? player.IsDeadForObjectives;
			if (!source.Any((CObjectProp p) => p.ActorActivated == player.ActorGuid))
			{
				flag = false;
				if (flag2)
				{
					base.UnableToComplete = true;
					break;
				}
			}
			else
			{
				num++;
			}
		}
		CurrentCharactersLooted = num;
		if (!flag && list.Count == 0)
		{
			base.UnableToComplete = true;
		}
		base.IsComplete = flag && CheckOtherObjectiveStatesRequirement();
		return base.IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		current = CurrentCharactersLooted;
		total = ScenarioManager.CurrentScenarioState.Players.Count;
		return Math.Min(1f, (float)current / (float)total);
	}

	public override int GetObjectiveCompletionValue(int partySize)
	{
		if (ScenarioManager.CurrentScenarioState == null)
		{
			return 0;
		}
		return ScenarioManager.CurrentScenarioState.Players.Count;
	}
}
