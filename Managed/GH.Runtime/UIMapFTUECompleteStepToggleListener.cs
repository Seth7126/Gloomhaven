using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class UIMapFTUECompleteStepToggleListener : UIMapFTUECompleteStepListener
{
	private Toggle toggle;

	[SerializeField]
	private bool completedOnToggledOn;

	protected override void OnStartedFTUE()
	{
		base.OnStartedFTUE();
		toggle = GetComponent<Toggle>();
		toggle.onValueChanged.AddListener(OnValueChanged);
	}

	private void OnValueChanged(bool isOn)
	{
		if (completedOnToggledOn == isOn)
		{
			CompleteStep();
		}
	}

	protected override void Clear()
	{
		base.Clear();
		toggle.onValueChanged.RemoveListener(OnValueChanged);
	}
}
