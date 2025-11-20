using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIMapFTUECompleteStepButtonListener : UIMapFTUECompleteStepListener
{
	private Button button;

	protected override void OnStartedFTUE()
	{
		base.OnStartedFTUE();
		button = GetComponent<Button>();
		button.onClick.AddListener(base.CompleteStep);
	}

	protected override void Clear()
	{
		base.Clear();
		button.onClick.RemoveListener(base.CompleteStep);
	}
}
