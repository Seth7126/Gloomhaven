using System.Collections.Generic;
using ScenarioRuleLibrary;
using UnityEngine;

public class ScenarioModifierContainer : MonoBehaviour
{
	[SerializeField]
	private GameObject ScenarioModifierPrefab;

	private string ScenarioID;

	private List<ScenarioModifierUI> ScenarioModifierInstances = new List<ScenarioModifierUI>();

	public void Init(string scenarioID, List<CScenarioModifier> scenarioModifiers)
	{
		ScenarioID = scenarioID;
		foreach (ScenarioModifierUI scenarioModifierInstance in ScenarioModifierInstances)
		{
			Object.Destroy(scenarioModifierInstance);
		}
		ScenarioModifierInstances.Clear();
		foreach (CScenarioModifier scenarioModifier in scenarioModifiers)
		{
			AddModifier(scenarioModifier);
		}
	}

	public void ModifyUpdatedHiddenOrDeactivatedState(CScenarioModifier modifierUpdated)
	{
		if (modifierUpdated.IsHidden || modifierUpdated.Deactivated)
		{
			RemoveModifier(modifierUpdated);
		}
		else
		{
			AddModifier(modifierUpdated);
		}
	}

	private void AddModifier(CScenarioModifier scenarioModifier)
	{
		if (scenarioModifier != null && ScenarioModifierInstances.Find((ScenarioModifierUI o) => o.m_ScenarioModifier == scenarioModifier) == null)
		{
			InitialiseScenarioModifier(scenarioModifier);
		}
	}

	private void RemoveModifier(CScenarioModifier scenarioModifier)
	{
		if (scenarioModifier != null)
		{
			ScenarioModifierUI scenarioModifierUI = ScenarioModifierInstances.Find((ScenarioModifierUI o) => o.m_ScenarioModifier == scenarioModifier);
			if (scenarioModifierUI != null)
			{
				ScenarioModifierInstances.Remove(scenarioModifierUI);
				Object.Destroy(scenarioModifierUI.gameObject);
			}
		}
	}

	private void InitialiseScenarioModifier(CScenarioModifier scenarioModifier)
	{
		if (!scenarioModifier.IsHidden && !scenarioModifier.Deactivated)
		{
			string text = scenarioModifier.LocalizeText(ScenarioID);
			if (text != string.Empty && text != "")
			{
				ScenarioModifierUI component = Object.Instantiate(ScenarioModifierPrefab, base.transform).GetComponent<ScenarioModifierUI>();
				component.transform.SetAsFirstSibling();
				component.Init(scenarioModifier, text);
				ScenarioModifierInstances.Add(component);
			}
		}
	}
}
