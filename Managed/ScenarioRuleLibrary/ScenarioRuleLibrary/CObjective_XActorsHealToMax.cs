using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_XActorsHealToMax : CObjective
{
	public List<int> ActorAmount { get; private set; }

	public int CurrentActorsFullHealth { get; private set; }

	public override string LocKey
	{
		get
		{
			if (string.IsNullOrEmpty(base.CustomLocKey))
			{
				return string.Empty;
			}
			return base.CustomLocKey;
		}
	}

	public CObjective_XActorsHealToMax()
	{
	}

	public CObjective_XActorsHealToMax(CObjective_XActorsHealToMax state, ReferenceDictionary references)
		: base(state, references)
	{
		ActorAmount = references.Get(state.ActorAmount);
		if (ActorAmount == null && state.ActorAmount != null)
		{
			ActorAmount = new List<int>();
			for (int i = 0; i < state.ActorAmount.Count; i++)
			{
				int item = state.ActorAmount[i];
				ActorAmount.Add(item);
			}
			references.Add(state.ActorAmount, ActorAmount);
		}
		CurrentActorsFullHealth = state.CurrentActorsFullHealth;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ActorAmount", ActorAmount);
	}

	public CObjective_XActorsHealToMax(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				if (current.Name == "ActorAmount")
				{
					ActorAmount = (List<int>)info.GetValue("ActorAmount", typeof(List<int>));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CObjective_XActorsHealToMax entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjective_XActorsHealToMax(EObjectiveResult result, CObjectiveFilter objectiveFilter, List<int> actorAmount, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.XActorsHealToMax, result, objectiveFilter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
		ActorAmount = actorAmount;
		CurrentActorsFullHealth = 0;
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		int num = 0;
		foreach (HeroSummonState heroSummon2 in ScenarioManager.CurrentScenarioState.HeroSummons)
		{
			CActor heroSummon = heroSummon2.HeroSummon;
			if (heroSummon != null && heroSummon.Health >= heroSummon.OriginalMaxHealth && base.ObjectiveFilter.IsValidTarget(heroSummon2))
			{
				num++;
			}
		}
		foreach (PlayerState player2 in ScenarioManager.CurrentScenarioState.Players)
		{
			CActor player = player2.Player;
			if (player != null && player.Health >= player.OriginalMaxHealth && base.ObjectiveFilter.IsValidTarget(player2))
			{
				num++;
			}
		}
		foreach (EnemyState allEnemyState in ScenarioManager.CurrentScenarioState.AllEnemyStates)
		{
			if (!allEnemyState.IsHiddenForCurrentPartySize)
			{
				CActor enemy = allEnemyState.Enemy;
				if (enemy != null && enemy.Health >= enemy.OriginalMaxHealth && base.ObjectiveFilter.IsValidTarget(allEnemyState))
				{
					num++;
				}
			}
		}
		CurrentActorsFullHealth = num;
		int index = Math.Max(0, partySize - 1);
		base.IsComplete = num >= ActorAmount[index] && CheckOtherObjectiveStatesRequirement();
		return base.IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		int index = Math.Max(0, partySize - 1);
		total = ActorAmount[index];
		current = Math.Min(CurrentActorsFullHealth, ActorAmount[index]);
		return Math.Min(1f, (float)CurrentActorsFullHealth / (float)ActorAmount[index]);
	}

	public override int GetObjectiveCompletionValue(int partySize)
	{
		int index = Math.Max(0, partySize - 1);
		return ActorAmount[index];
	}
}
