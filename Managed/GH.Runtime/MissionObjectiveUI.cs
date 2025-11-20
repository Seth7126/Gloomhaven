using System.Collections.Generic;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

public class MissionObjectiveUI : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI objectiveText;

	[SerializeField]
	private ImageProgressBar progressBar;

	[SerializeField]
	private List<GameObject> progressObjects;

	[Header("Incompleted")]
	[SerializeField]
	private GameObject incompletedCheck;

	[SerializeField]
	private Color incompletedColor;

	[SerializeField]
	private Color incompletedColorText;

	[SerializeField]
	[Header("Completed")]
	private GameObject completedCheck;

	[SerializeField]
	private Color completedColor;

	[SerializeField]
	private Color completedColorText;

	[HideInInspector]
	public CObjective m_Objective;

	private bool isCompleted;

	private void Awake()
	{
		progressBar.OnCompletedProgress.AddListener(delegate
		{
			ShowCompleted(completed: true);
			ShowProgressBar(show: false);
		});
	}

	public void Init(CObjective objective)
	{
		isCompleted = false;
		m_Objective = objective;
		UpdateMissionProgress(playAnimation: false);
		UpdateMissionText(initialState: true);
	}

	private void ShowProgressBar(bool show)
	{
		for (int i = 0; i < progressObjects.Count; i++)
		{
			progressObjects[i].SetActive(show);
		}
	}

	private void ShowCompleted(bool completed)
	{
		isCompleted = completed;
		incompletedCheck.SetActive(!completed);
		completedCheck.SetActive(completed);
		objectiveText.color = (completed ? completedColor : incompletedColor);
		foreach (TextMeshProUGUI amountText in progressBar.AmountTexts)
		{
			amountText.color = (completed ? completedColorText : incompletedColorText);
		}
	}

	public void UpdateMissionProgress(bool playAnimation = true)
	{
		if (m_Objective == null)
		{
			return;
		}
		m_Objective.GetObjectiveProgress(ScenarioManager.CurrentScenarioState.Players.Count, out var total, out var current);
		if (total <= 1)
		{
			ShowProgressBar(show: false);
			ShowCompleted(total == current);
		}
		else if (total != current || !isCompleted)
		{
			ShowProgressBar(show: true);
			ShowCompleted(completed: false);
			progressBar.ShowProgressText(m_Objective.ObjectiveType != EObjectiveType.KillAllEnemies && m_Objective.ObjectiveType != EObjectiveType.KillAllBosses);
			if (playAnimation)
			{
				progressBar.PlayProgressTo(current, total);
			}
			else
			{
				progressBar.SetAmount(current, total);
			}
		}
	}

	public void UpdateMissionText(bool initialState = false)
	{
		if (m_Objective.LocKey != string.Empty && m_Objective.IsActive)
		{
			string text = m_Objective.LocalizeText(initialState);
			objectiveText.text = text;
		}
	}
}
