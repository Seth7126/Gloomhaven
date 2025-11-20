using JetBrains.Annotations;
using Script.LogicalOperations;
using TMPro;
using UnityEngine;

namespace Script.GUI.Conditions;

public class InputFieldHasContentCondition : Condition
{
	[SerializeField]
	private TMP_InputField _inputField;

	protected override bool ConditionValue => HasContent();

	[UsedImplicitly]
	private void OnDestroy()
	{
		_inputField.onValueChanged.RemoveListener(OnValueChangedHandler);
	}

	protected override void Subscribe()
	{
		_inputField.onValueChanged.AddListener(OnValueChangedHandler);
	}

	protected override void Unsubscribe()
	{
		_inputField.onValueChanged.RemoveListener(OnValueChangedHandler);
	}

	private void OnValueChangedHandler(string value)
	{
		InvokeOnValueChanged();
	}

	private bool HasContent()
	{
		return !_inputField.text.IsNullOrEmpty();
	}
}
