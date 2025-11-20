using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_XCharactersDie : CObjective
{
	public List<int> KillAmount { get; private set; }

	public int CurrentKills { get; private set; }

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
						return "GUI_OBJECTIVE_KILL_X_WIN_FILTERED";
					}
					if (base.Result == EObjectiveResult.Lose)
					{
						return "GUI_OBJECTIVE_KILL_X_LOSE_FILTERED";
					}
				}
				else
				{
					if (base.Result == EObjectiveResult.Win)
					{
						return "GUI_OBJECTIVE_KILL_X_WIN";
					}
					if (base.Result == EObjectiveResult.Lose)
					{
						return "GUI_OBJECTIVE_KILL_X_LOSE";
					}
				}
				return string.Empty;
			}
			return base.CustomLocKey;
		}
	}

	public CObjective_XCharactersDie()
	{
	}

	public CObjective_XCharactersDie(CObjective_XCharactersDie state, ReferenceDictionary references)
		: base(state, references)
	{
		KillAmount = references.Get(state.KillAmount);
		if (KillAmount == null && state.KillAmount != null)
		{
			KillAmount = new List<int>();
			for (int i = 0; i < state.KillAmount.Count; i++)
			{
				int item = state.KillAmount[i];
				KillAmount.Add(item);
			}
			references.Add(state.KillAmount, KillAmount);
		}
		CurrentKills = state.CurrentKills;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("KillAmount", KillAmount);
	}

	public CObjective_XCharactersDie(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				if (current.Name == "KillAmount")
				{
					try
					{
						KillAmount = (List<int>)info.GetValue("KillAmount", typeof(List<int>));
					}
					catch
					{
						int item = (int)info.GetValue("KillAmount", typeof(int));
						KillAmount = new List<int> { item, item, item, item };
					}
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CObjective_XCharactersDie entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjective_XCharactersDie(EObjectiveResult result, CObjectiveFilter objectiveFilter, List<int> killAmount, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.XCharactersDie, result, objectiveFilter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
		KillAmount = killAmount;
		CurrentKills = 0;
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		int num = 0;
		foreach (HeroSummonState heroSummon2 in ScenarioManager.CurrentScenarioState.HeroSummons)
		{
			CActor heroSummon = heroSummon2.HeroSummon;
			if (heroSummon != null && heroSummon.IsDeadForObjectives && base.ObjectiveFilter.IsValidTarget(heroSummon2))
			{
				num++;
			}
		}
		foreach (PlayerState player2 in ScenarioManager.CurrentScenarioState.Players)
		{
			CActor player = player2.Player;
			if (player != null && player.IsDeadForObjectives && base.ObjectiveFilter.IsValidTarget(player2))
			{
				num++;
			}
		}
		foreach (EnemyState allEnemyState in ScenarioManager.CurrentScenarioState.AllEnemyStates)
		{
			if (!allEnemyState.IsHiddenForCurrentPartySize)
			{
				CActor enemy = allEnemyState.Enemy;
				if (enemy != null && enemy.IsDeadForObjectives && base.ObjectiveFilter.IsValidTarget(allEnemyState))
				{
					num++;
				}
			}
		}
		CurrentKills = num;
		int index = Math.Max(0, partySize - 1);
		base.IsComplete = num >= KillAmount[index] && CheckOtherObjectiveStatesRequirement();
		return base.IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		int index = Math.Max(0, partySize - 1);
		total = KillAmount[index];
		current = Math.Min(CurrentKills, KillAmount[index]);
		return Math.Min(1f, (float)CurrentKills / (float)KillAmount[index]);
	}

	public override int GetObjectiveCompletionValue(int partySize)
	{
		int index = Math.Max(0, partySize - 1);
		return KillAmount[index];
	}
}
