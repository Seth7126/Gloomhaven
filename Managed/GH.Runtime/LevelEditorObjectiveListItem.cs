using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class LevelEditorObjectiveListItem : MonoBehaviour
{
	[HideInInspector]
	public int objectiveIndex;

	public EObjectiveResult CurrentObjectiveResult;

	public TextMeshProUGUI ObjectiveNameText;

	public UnityAction<LevelEditorObjectiveListItem> ButtonPressedAction;

	public CObjective CurrentObjective
	{
		get
		{
			if (CurrentObjectiveResult != EObjectiveResult.Win)
			{
				return ScenarioManager.CurrentScenarioState.LoseObjectives[objectiveIndex];
			}
			return ScenarioManager.CurrentScenarioState.WinObjectives[objectiveIndex];
		}
	}

	public void InitForObjective(CObjective objectiveToInitFor, int indexToUse, EObjectiveResult objectiveResult)
	{
		objectiveIndex = indexToUse;
		ObjectiveNameText.text = "Objective #" + indexToUse;
		CurrentObjectiveResult = objectiveResult;
	}

	public void OnButtonPressItemSelected()
	{
		ButtonPressedAction?.Invoke(this);
	}
}
