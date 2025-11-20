using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class LevelEditorScenarioModifierListItem : MonoBehaviour
{
	[HideInInspector]
	public int modifierIndex;

	public TextMeshProUGUI ModifierNameText;

	public UnityAction<LevelEditorScenarioModifierListItem> ButtonPressedAction;

	public CScenarioModifier Modifier => ScenarioManager.CurrentScenarioState.ScenarioModifiers[modifierIndex];

	public void InitForScenarioModifier(CScenarioModifier scenarioModifier, int indexToUse)
	{
		modifierIndex = indexToUse;
		ModifierNameText.text = "Modifier #" + indexToUse;
	}

	public void OnButtonPressItemSelected()
	{
		ButtonPressedAction?.Invoke(this);
	}
}
