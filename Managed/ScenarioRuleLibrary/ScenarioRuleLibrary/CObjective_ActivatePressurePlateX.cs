using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_ActivatePressurePlateX : CObjective
{
	public List<int> NumberToActivate { get; private set; }

	public int CurrentActivatedAmount { get; private set; }

	public override string LocKey
	{
		get
		{
			if (string.IsNullOrEmpty(base.CustomLocKey))
			{
				if (base.Result == EObjectiveResult.Win)
				{
					return "GUI_OBJECTIVE_ACTIVATE_PRESSUREPLATE_X_WIN";
				}
				if (base.Result == EObjectiveResult.Lose)
				{
					return "GUI_OBJECTIVE_ACTIVATE_PRESSUREPLATE_X_LOSE";
				}
				return string.Empty;
			}
			return base.CustomLocKey;
		}
	}

	public CObjective_ActivatePressurePlateX()
	{
	}

	public CObjective_ActivatePressurePlateX(CObjective_ActivatePressurePlateX state, ReferenceDictionary references)
		: base(state, references)
	{
		NumberToActivate = references.Get(state.NumberToActivate);
		if (NumberToActivate == null && state.NumberToActivate != null)
		{
			NumberToActivate = new List<int>();
			for (int i = 0; i < state.NumberToActivate.Count; i++)
			{
				int item = state.NumberToActivate[i];
				NumberToActivate.Add(item);
			}
			references.Add(state.NumberToActivate, NumberToActivate);
		}
		CurrentActivatedAmount = state.CurrentActivatedAmount;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("NumberToActivate", NumberToActivate);
		info.AddValue("CurrentActivatedAmount", CurrentActivatedAmount);
	}

	public CObjective_ActivatePressurePlateX(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "NumberToActivate"))
				{
					if (name == "CurrentActivatedAmount")
					{
						CurrentActivatedAmount = info.GetInt32("CurrentActivatedAmount");
					}
					continue;
				}
				try
				{
					NumberToActivate = (List<int>)info.GetValue("NumberToActivate", typeof(List<int>));
				}
				catch
				{
					int item = (int)info.GetValue("NumberToActivate", typeof(int));
					NumberToActivate = new List<int> { item, item, item, item };
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CObjective_ActivatePressurePlateX entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjective_ActivatePressurePlateX(EObjectiveResult result, CObjectiveFilter objectiveFilter, List<int> numberToActivate, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.ActivateXPressurePlates, result, objectiveFilter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
		NumberToActivate = numberToActivate;
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		int num = 0;
		foreach (CObjectPressurePlate item in ScenarioManager.CurrentScenarioState.Props.OfType<CObjectPressurePlate>())
		{
			if (item.GetConfigForPartySize(ScenarioManager.CurrentScenarioState.Players.Count) != ScenarioManager.EPerPartySizeConfig.Hidden && item.Activated)
			{
				num++;
			}
		}
		CurrentActivatedAmount = num;
		int index = Math.Max(0, partySize - 1);
		base.IsComplete = num >= NumberToActivate[index] && CheckOtherObjectiveStatesRequirement();
		return base.IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		int index = Math.Max(0, partySize - 1);
		total = NumberToActivate[index];
		current = Math.Min(CurrentActivatedAmount, total);
		return Math.Min(1f, (float)CurrentActivatedAmount / (float)NumberToActivate[index]);
	}

	public override int GetObjectiveCompletionValue(int partySize)
	{
		int index = Math.Max(0, partySize - 1);
		return NumberToActivate[index];
	}
}
