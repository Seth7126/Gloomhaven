using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_DestroyXObjects : CObjective
{
	public List<int> DestroyAmount { get; private set; }

	public int CurrentObjectsDestroyed { get; private set; }

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
						return "GUI_OBJECTIVE_DESTROY_X_OBJECTS_WIN_FILTERED";
					}
					if (base.Result == EObjectiveResult.Lose)
					{
						return "GUI_OBJECTIVE_DESTROY_X_OBJECTS_LOSE_FILTERED";
					}
				}
				else
				{
					if (base.Result == EObjectiveResult.Win)
					{
						return "GUI_OBJECTIVE_DESTROY_X_OBJECTS_WIN";
					}
					if (base.Result == EObjectiveResult.Lose)
					{
						return "GUI_OBJECTIVE_DESTROY_X_OBJECTS_LOSE";
					}
				}
				return string.Empty;
			}
			return base.CustomLocKey;
		}
	}

	public CObjective_DestroyXObjects()
	{
	}

	public CObjective_DestroyXObjects(CObjective_DestroyXObjects state, ReferenceDictionary references)
		: base(state, references)
	{
		DestroyAmount = references.Get(state.DestroyAmount);
		if (DestroyAmount == null && state.DestroyAmount != null)
		{
			DestroyAmount = new List<int>();
			for (int i = 0; i < state.DestroyAmount.Count; i++)
			{
				int item = state.DestroyAmount[i];
				DestroyAmount.Add(item);
			}
			references.Add(state.DestroyAmount, DestroyAmount);
		}
		CurrentObjectsDestroyed = state.CurrentObjectsDestroyed;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("DestroyAmount", DestroyAmount);
	}

	public CObjective_DestroyXObjects(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "DestroyAmount"))
				{
					if (name == "CurrentObjectsDestroyed")
					{
						CurrentObjectsDestroyed = info.GetInt32("CurrentObjectsDestroyed");
					}
				}
				else
				{
					DestroyAmount = (List<int>)info.GetValue("DestroyAmount", typeof(List<int>));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CObjective_DestroyXObjects entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjective_DestroyXObjects(EObjectiveResult result, CObjectiveFilter objectiveFilter, List<int> destroyAmount, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.DestroyXObjects, result, objectiveFilter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
		DestroyAmount = destroyAmount;
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		int num = 0;
		foreach (ObjectState @object in ScenarioManager.CurrentScenarioState.Objects)
		{
			if (!@object.IsHiddenForCurrentPartySize && @object.Object != null && @object.Object.IsDeadForObjectives && base.ObjectiveFilter.IsValidTarget(@object))
			{
				num++;
			}
		}
		int index = Math.Max(0, partySize - 1);
		CurrentObjectsDestroyed = num;
		base.IsComplete = num >= DestroyAmount[index] && CheckOtherObjectiveStatesRequirement();
		return base.IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		int index = Math.Max(0, partySize - 1);
		total = DestroyAmount[index];
		current = Math.Min(CurrentObjectsDestroyed, DestroyAmount[index]);
		return Math.Min(1f, (float)CurrentObjectsDestroyed / (float)DestroyAmount[index]);
	}

	public override int GetObjectiveCompletionValue(int partySize)
	{
		int index = Math.Max(0, partySize - 1);
		return DestroyAmount[index];
	}
}
