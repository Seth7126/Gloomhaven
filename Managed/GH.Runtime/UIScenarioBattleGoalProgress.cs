using System;
using System.Collections.Generic;
using MapRuleLibrary.Party;
using SharedLibrary.SimpleLog;
using TMPro;
using UnityEngine;

public class UIScenarioBattleGoalProgress : UIBattleGoalProgress
{
	private enum EState
	{
		Incompleted,
		Completed,
		Failed
	}

	[SerializeField]
	private List<GameObject> progressObjects;

	[Header("Incompleted")]
	[SerializeField]
	private GameObject incompletedCheck;

	[SerializeField]
	private Color incompletedColor;

	[SerializeField]
	private Color incompletedText;

	[Header("Completed")]
	[SerializeField]
	private GameObject completedCheck;

	[SerializeField]
	private GameObject failedCheck;

	[SerializeField]
	private Color completedColor;

	[SerializeField]
	private Color completedText;

	private EState state;

	private void Awake()
	{
		progressBar.OnCompletedProgress.AddListener(delegate
		{
			if (!m_BattleGoal.BattleGoalConditionState.NegativeCondition && !m_BattleGoal.BattleGoalConditionState.Failed)
			{
				RefreshState(EState.Completed);
				ShowProgressBar(show: false);
			}
		});
	}

	public override void SetBattleGoal(CBattleGoalState battleGoal)
	{
		state = EState.Incompleted;
		base.SetBattleGoal(battleGoal);
	}

	private void ShowProgressBar(bool show)
	{
		for (int i = 0; i < progressObjects.Count; i++)
		{
			progressObjects[i].SetActive(show);
		}
	}

	private void RefreshState(EState state)
	{
		if (this.state != state)
		{
			SimpleLog.AddToSimpleLog("Battle Goal " + m_BattleGoal.ID + " progress updated from " + this.state.ToString() + " to " + state.ToString() + ". Target=" + m_BattleGoal.BattleGoalConditionState.TotalConditionsAndTargets + ", progress=" + m_BattleGoal.BattleGoalConditionState.CurrentProgress);
		}
		this.state = state;
		incompletedCheck.SetActive(state == EState.Incompleted);
		completedCheck.SetActive(state == EState.Completed);
		failedCheck.SetActive(state == EState.Failed);
		description.Text.color = ((state == EState.Incompleted) ? incompletedColor : completedColor);
		foreach (TextMeshProUGUI amountText in progressBar.AmountTexts)
		{
			amountText.color = ((state == EState.Incompleted) ? incompletedText : completedText);
		}
	}

	public override void UpdateProgress(bool playAnimation = true)
	{
		int totalConditionsAndTargets = m_BattleGoal.BattleGoalConditionState.TotalConditionsAndTargets;
		int currentProgress = m_BattleGoal.BattleGoalConditionState.CurrentProgress;
		if (m_BattleGoal.BattleGoalConditionState.Failed)
		{
			ShowProgressBar(show: false);
			RefreshState(EState.Failed);
			UpdateProgress(totalConditionsAndTargets, currentProgress, playAnimation);
		}
		else if (totalConditionsAndTargets <= 1)
		{
			ShowProgressBar(show: false);
			if (totalConditionsAndTargets != currentProgress && state != EState.Incompleted)
			{
				SimpleLog.AddToSimpleLog("Error: Battle Goal " + m_BattleGoal.ID + " changing from " + state.ToString() + " to " + EState.Incompleted.ToString() + ". Target=" + m_BattleGoal.BattleGoalConditionState.TotalConditionsAndTargets + ", progress=" + m_BattleGoal.BattleGoalConditionState.CurrentProgress + "\n" + Environment.StackTrace);
			}
			RefreshState((totalConditionsAndTargets == currentProgress) ? EState.Completed : EState.Incompleted);
			UpdateProgress(totalConditionsAndTargets, currentProgress, playAnimation);
		}
		else if (totalConditionsAndTargets != currentProgress || state != EState.Completed)
		{
			ShowProgressBar(show: true);
			RefreshState(EState.Incompleted);
			UpdateProgress(totalConditionsAndTargets, currentProgress, playAnimation);
		}
	}
}
