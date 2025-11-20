using System.Collections.Generic;
using System.Linq;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using UnityEngine;

public class BattleGoalContainer : MonoBehaviour
{
	[SerializeField]
	private List<UIScenarioPlayerBattleGoal> playerBattlePool = new List<UIScenarioPlayerBattleGoal>();

	private Dictionary<CPlayerActor, UIScenarioPlayerBattleGoal> assignedGoals = new Dictionary<CPlayerActor, UIScenarioPlayerBattleGoal>();

	public void Init(List<CBattleGoalState> battleGoals)
	{
		assignedGoals.Clear();
		if (battleGoals == null)
		{
			HelperTools.NormalizePool(ref playerBattlePool, playerBattlePool[0].gameObject, playerBattlePool[0].transform.parent, 0);
			return;
		}
		HelperTools.NormalizePool(ref playerBattlePool, playerBattlePool[0].gameObject, playerBattlePool[0].transform.parent, battleGoals.Count, delegate(UIScenarioPlayerBattleGoal ui)
		{
			ui.gameObject.SetActive(value: false);
		});
		if (ScenarioManager.Scenario.AllPlayers.Count <= 0)
		{
			return;
		}
		int i;
		for (i = 0; i < battleGoals.Count; i++)
		{
			CPlayerActor cPlayerActor = ScenarioManager.Scenario.AllPlayers.First((CPlayerActor it) => it.CharacterClass.CharacterID == battleGoals[i].BattleGoalConditionState.OverrideCharacterID);
			playerBattlePool[i].Init(cPlayerActor.CharacterClass.CharacterModel, battleGoals[i], cPlayerActor.CharacterClass.CharacterYML.CustomCharacterConfig);
			assignedGoals[cPlayerActor] = playerBattlePool[i];
			battleGoals[i].BattleGoalConditionState.UpdateGoal(battleGoals[i].BattleGoalConditionState.OverrideCharacterID);
		}
	}

	public void Show(CPlayerActor playerActor)
	{
		foreach (KeyValuePair<CPlayerActor, UIScenarioPlayerBattleGoal> assignedGoal in assignedGoals)
		{
			if (assignedGoal.Key != playerActor || (FFSNetwork.IsOnline && !playerActor.IsUnderMyControl))
			{
				Hide(assignedGoal.Key);
			}
			else
			{
				assignedGoals[playerActor].Show();
			}
		}
	}

	public void Hide(CPlayerActor playerActor)
	{
		if (playerActor != null && assignedGoals.ContainsKey(playerActor))
		{
			assignedGoals[playerActor].Hide();
		}
	}

	public void Hide()
	{
		foreach (UIScenarioPlayerBattleGoal value in assignedGoals.Values)
		{
			value.Hide();
		}
	}

	public void UpdateProgress(CPlayerActor playerActor)
	{
		if (assignedGoals.ContainsKey(playerActor) && (!FFSNetwork.IsOnline || playerActor.IsUnderMyControl))
		{
			assignedGoals[playerActor].UpdateProgress();
		}
	}

	public void UpdateProgress()
	{
		foreach (KeyValuePair<CPlayerActor, UIScenarioPlayerBattleGoal> item in assignedGoals.Where((KeyValuePair<CPlayerActor, UIScenarioPlayerBattleGoal> it) => !FFSNetwork.IsOnline || it.Key.IsUnderMyControl))
		{
			item.Value.UpdateProgress();
		}
	}
}
