using System;
using ScenarioRuleLibrary.CustomLevels;
using TMPro;
using UnityEngine;

public class LevelEditorEventItem : MonoBehaviour
{
	public TextMeshProUGUI EventText;

	[HideInInspector]
	public int ItemIndex;

	[HideInInspector]
	public CLevelEvent ItemEvent;

	public Action<LevelEditorEventItem> ButtonClickedAction { get; private set; }

	public void Init(CLevelEvent itemEvent, int index, Action<LevelEditorEventItem> buttonClicked)
	{
		ItemIndex = index;
		ItemEvent = itemEvent;
		ButtonClickedAction = buttonClicked;
		EventText.text = "Event " + index;
	}

	public void OnButtonClicked()
	{
		ButtonClickedAction?.Invoke(this);
	}
}
