using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_LootX : CObjective
{
	public List<int> LootAmount { get; private set; }

	public int CurrentLootedAmount { get; private set; }

	public override string LocKey
	{
		get
		{
			if (string.IsNullOrEmpty(base.CustomLocKey))
			{
				if (base.ObjectiveFilter.FilterHasValues)
				{
					if (base.Result == EObjectiveResult.Win)
					{
						return "GUI_OBJECTIVE_LOOT_X_WIN_FILTERED";
					}
					if (base.Result == EObjectiveResult.Lose)
					{
						return "GUI_OBJECTIVE_LOOT_X_LOSE_FILTERED";
					}
				}
				else
				{
					if (base.Result == EObjectiveResult.Win)
					{
						return "GUI_OBJECTIVE_LOOT_X_WIN";
					}
					if (base.Result == EObjectiveResult.Lose)
					{
						return "GUI_OBJECTIVE_LOOT_X_LOSE";
					}
				}
				return string.Empty;
			}
			return base.CustomLocKey;
		}
	}

	public CObjective_LootX()
	{
	}

	public CObjective_LootX(CObjective_LootX state, ReferenceDictionary references)
		: base(state, references)
	{
		LootAmount = references.Get(state.LootAmount);
		if (LootAmount == null && state.LootAmount != null)
		{
			LootAmount = new List<int>();
			for (int i = 0; i < state.LootAmount.Count; i++)
			{
				int item = state.LootAmount[i];
				LootAmount.Add(item);
			}
			references.Add(state.LootAmount, LootAmount);
		}
		CurrentLootedAmount = state.CurrentLootedAmount;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("LootAmount", LootAmount);
		info.AddValue("CurrentLootedAmount", CurrentLootedAmount);
	}

	public CObjective_LootX(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "LootAmount"))
				{
					if (name == "CurrentLootedAmount")
					{
						CurrentLootedAmount = info.GetInt32("CurrentLootedAmount");
					}
					continue;
				}
				try
				{
					LootAmount = (List<int>)info.GetValue("LootAmount", typeof(List<int>));
				}
				catch
				{
					int item = (int)info.GetValue("LootAmount", typeof(int));
					LootAmount = new List<int> { item, item, item, item };
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CObjective_LootX entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjective_LootX(EObjectiveResult result, CObjectiveFilter objectiveFilter, List<int> lootAmount, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.LootX, result, objectiveFilter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
		LootAmount = lootAmount;
		CurrentLootedAmount = 0;
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		int num = 0;
		foreach (CObjectProp activatedProp in ScenarioManager.CurrentScenarioState.ActivatedProps)
		{
			if (base.ObjectiveFilter.IsValidLootTarget(activatedProp.ObjectType) && base.ObjectiveFilter.IsValidProp(activatedProp))
			{
				num++;
			}
		}
		CurrentLootedAmount = num;
		int index = Math.Max(0, partySize - 1);
		base.IsComplete = CurrentLootedAmount >= LootAmount[index] && CheckOtherObjectiveStatesRequirement();
		return base.IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		int index = Math.Max(0, partySize - 1);
		total = LootAmount[index];
		current = Math.Min(CurrentLootedAmount, total);
		return Math.Min(1f, (float)CurrentLootedAmount / (float)LootAmount[index]);
	}

	public override int GetObjectiveCompletionValue(int partySize)
	{
		int index = Math.Max(0, partySize - 1);
		return LootAmount[index];
	}
}
