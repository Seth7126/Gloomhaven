using System.Collections.Generic;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;
using UnityEngine;

public class MissionObjectiveContainer : MonoBehaviour
{
	[SerializeField]
	private GameObject MissionObjectivePrefab;

	[SerializeField]
	private Transform objectiveContainer;

	[SerializeField]
	private GoldCounter goldCounter;

	[SerializeField]
	private GameObject goldContainer;

	[SerializeField]
	private UIScenarioQuest questHeader;

	private List<MissionObjectiveUI> ObjectiveInstances = new List<MissionObjectiveUI>();

	public void Init(List<CObjective> winObjectives, List<CObjective> loseObjectives)
	{
		foreach (MissionObjectiveUI objectiveInstance in ObjectiveInstances)
		{
			Object.Destroy(objectiveInstance.gameObject);
		}
		ObjectiveInstances.Clear();
		foreach (CObjective winObjective in winObjectives)
		{
			InitialiseObjective(winObjective);
		}
		foreach (CObjective loseObjective in loseObjectives)
		{
			InitialiseObjective(loseObjective);
		}
		if (SaveData.Instance.Global.GameMode == EGameMode.Guildmaster || SaveData.Instance.Global.GameMode == EGameMode.Campaign)
		{
			CQuestState cQuestState = AdventureState.MapState?.InProgressQuestState;
			if (cQuestState == null)
			{
				questHeader.gameObject.SetActive(value: false);
			}
			else
			{
				questHeader.SetQuest(cQuestState);
				questHeader.gameObject.SetActive(value: true);
			}
			goldContainer.SetActive(AdventureState.MapState.GoldMode == EGoldMode.PartyGold);
		}
		else
		{
			goldContainer.SetActive(value: false);
		}
	}

	public void AddObjective(CObjective objective)
	{
		if (objective != null && ObjectiveInstances.Find((MissionObjectiveUI o) => o.m_Objective == objective) == null)
		{
			InitialiseObjective(objective);
		}
	}

	public void CheckToRemoveObjectives()
	{
		for (int num = ObjectiveInstances.Count - 1; num >= 0; num--)
		{
			MissionObjectiveUI missionObjectiveUI = ObjectiveInstances[num];
			if (missionObjectiveUI.m_Objective.IsComplete && missionObjectiveUI.m_Objective.RemovesFromUIOnComplete)
			{
				RemoveObjective(missionObjectiveUI.m_Objective);
			}
		}
	}

	public void RemoveObjective(CObjective objective)
	{
		if (objective != null)
		{
			MissionObjectiveUI missionObjectiveUI = ObjectiveInstances.Find((MissionObjectiveUI o) => o.m_Objective == objective);
			if (missionObjectiveUI != null)
			{
				ObjectiveInstances.Remove(missionObjectiveUI);
				Object.Destroy(missionObjectiveUI.gameObject);
			}
		}
	}

	public void UpdateProgress()
	{
		foreach (MissionObjectiveUI objectiveInstance in ObjectiveInstances)
		{
			objectiveInstance.UpdateMissionProgress();
			objectiveInstance.UpdateMissionText();
		}
	}

	public void UpdateGold(int goldValue, bool animate)
	{
		if (goldContainer.activeSelf)
		{
			if (animate)
			{
				goldCounter.CountTo(goldValue);
			}
			else
			{
				goldCounter.SetCount(goldValue);
			}
		}
	}

	private void InitialiseObjective(CObjective objective)
	{
		if (objective.LocKey != string.Empty && objective.IsActive && !objective.IsHidden)
		{
			MissionObjectiveUI component = Object.Instantiate(MissionObjectivePrefab, objectiveContainer).GetComponent<MissionObjectiveUI>();
			component.transform.SetAsFirstSibling();
			component.Init(objective);
			ObjectiveInstances.Add(component);
		}
	}
}
