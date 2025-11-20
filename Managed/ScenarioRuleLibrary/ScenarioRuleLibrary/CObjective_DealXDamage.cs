using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_DealXDamage : CObjective
{
	public List<int> TargetDamage { get; private set; }

	public int CurrentDamageDealt { get; private set; }

	public override bool RemovesFromUIOnComplete => false;

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
						return "GUI_OBJECTIVE_DEAL_X_DAMAGE_WIN_FILTERED";
					}
					if (base.Result == EObjectiveResult.Lose)
					{
						return "GUI_OBJECTIVE_DEAL_X_DAMAGE_LOSE_FILTERED";
					}
				}
				else
				{
					if (base.Result == EObjectiveResult.Win)
					{
						return "GUI_OBJECTIVE_DEAL_X_DAMAGE_WIN";
					}
					if (base.Result == EObjectiveResult.Lose)
					{
						return "GUI_OBJECTIVE_DEAL_X_DAMAGE_LOSE";
					}
				}
				return string.Empty;
			}
			return base.CustomLocKey;
		}
	}

	public CObjective_DealXDamage()
	{
	}

	public CObjective_DealXDamage(CObjective_DealXDamage state, ReferenceDictionary references)
		: base(state, references)
	{
		TargetDamage = references.Get(state.TargetDamage);
		if (TargetDamage == null && state.TargetDamage != null)
		{
			TargetDamage = new List<int>();
			for (int i = 0; i < state.TargetDamage.Count; i++)
			{
				int item = state.TargetDamage[i];
				TargetDamage.Add(item);
			}
			references.Add(state.TargetDamage, TargetDamage);
		}
		CurrentDamageDealt = state.CurrentDamageDealt;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("TargetDamage", TargetDamage);
		info.AddValue("CurrentDamageDealt", CurrentDamageDealt);
	}

	public CObjective_DealXDamage(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "TargetDamage"))
				{
					if (name == "CurrentDamageDealt")
					{
						CurrentDamageDealt = info.GetInt32("CurrentDamageDealt");
					}
				}
				else
				{
					TargetDamage = (List<int>)info.GetValue("TargetDamage", typeof(List<int>));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CObjective_DealXDamage entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjective_DealXDamage(EObjectiveResult result, CObjectiveFilter objectiveFilter, List<int> targetDamage, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.DealXDamage, result, objectiveFilter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
		TargetDamage = targetDamage;
		CurrentDamageDealt = 0;
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		int index = Math.Max(0, partySize - 1);
		base.IsComplete = CurrentDamageDealt >= TargetDamage[index] && CheckOtherObjectiveStatesRequirement();
		return base.IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		int index = Math.Max(0, partySize - 1);
		total = TargetDamage[index];
		current = Math.Min(CurrentDamageDealt, TargetDamage[index]);
		return Math.Min(1f, (float)CurrentDamageDealt / (float)TargetDamage[index]);
	}

	public void SetActorDamaged(ActorState actorBeingDamaged, int damageReceived)
	{
		if (base.ObjectiveFilter.IsValidTarget(actorBeingDamaged))
		{
			CurrentDamageDealt += damageReceived;
		}
	}
}
