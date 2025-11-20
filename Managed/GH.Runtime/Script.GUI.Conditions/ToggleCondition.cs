using JetBrains.Annotations;
using Script.LogicalOperations;
using UnityEngine;
using UnityEngine.UI;

namespace Script.GUI.Conditions;

public class ToggleCondition : Condition
{
	[SerializeField]
	private Toggle _tab;

	protected override bool ConditionValue => _tab.isOn;

	[UsedImplicitly]
	private void OnDestroy()
	{
		_tab.onValueChanged.RemoveListener(OnValueChangedHandler);
	}

	protected override void Subscribe()
	{
		_tab.onValueChanged.AddListener(OnValueChangedHandler);
	}

	protected override void Unsubscribe()
	{
		_tab.onValueChanged.RemoveListener(OnValueChangedHandler);
	}

	private void OnValueChangedHandler(bool value)
	{
		InvokeOnValueChanged();
	}
}
