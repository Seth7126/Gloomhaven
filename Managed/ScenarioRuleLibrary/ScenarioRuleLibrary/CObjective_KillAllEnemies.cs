using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_KillAllEnemies : CObjective
{
	public bool CheckedEnemies { get; private set; }

	public int CurrentKills { get; private set; }

	public int ValidTargets { get; private set; }

	public override bool RemovesFromUIOnComplete => false;

	public CObjective_KillAllEnemies()
	{
	}

	public CObjective_KillAllEnemies(CObjective_KillAllEnemies state, ReferenceDictionary references)
		: base(state, references)
	{
		CheckedEnemies = state.CheckedEnemies;
		CurrentKills = state.CurrentKills;
		ValidTargets = state.ValidTargets;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
	}

	public CObjective_KillAllEnemies(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public CObjective_KillAllEnemies(EObjectiveResult result, CObjectiveFilter objectiveFilter, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.KillAllEnemies, result, objectiveFilter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		int num = 0;
		int num2 = 0;
		foreach (EnemyState allEnemyState in ScenarioManager.CurrentScenarioState.AllEnemyStates)
		{
			if (!allEnemyState.IsHiddenForCurrentPartySize && !CActor.AreActorsAllied(CActor.EType.Player, allEnemyState.Type) && base.ObjectiveFilter.IsValidTarget(allEnemyState))
			{
				num++;
				CActor enemy = allEnemyState.Enemy;
				if (enemy != null && enemy.IsDeadForObjectives)
				{
					num2++;
				}
			}
		}
		foreach (CSpawner spawner in ScenarioManager.CurrentScenarioState.Spawners)
		{
			if (!spawner.SpawnerData.ShouldCountTowardsKillAllEnemies || spawner.GetConfigForPartySize(ScenarioManager.CurrentScenarioState.Players?.Count ?? 0) == ScenarioManager.EPerPartySizeConfig.Hidden)
			{
				continue;
			}
			if (spawner.SpawnerData.LoopSpawnPattern)
			{
				if (!spawner.HasSpawned)
				{
					num++;
				}
			}
			else
			{
				num += spawner.TotalMonstersLeftToSpawn();
			}
		}
		CurrentKills = num2;
		ValidTargets = num;
		CheckedEnemies = true;
		base.IsComplete = num2 >= num && CheckOtherObjectiveStatesRequirement();
		return base.IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		if (!CheckedEnemies)
		{
			CheckObjectiveComplete(partySize);
		}
		total = ValidTargets;
		current = CurrentKills;
		return Math.Min(1f, (float)CurrentKills / (float)ValidTargets);
	}
}
