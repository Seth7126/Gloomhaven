using UnityEngine;

public class UIMapFTUEStepStartListener : UIMapFTUEListener
{
	[SerializeField]
	private EMapFTUEStep stepToContinue;

	[SerializeField]
	private UIMapFTUEStep stepToStart;

	private void OnStepCompleted(EMapFTUEStep step)
	{
		if (stepToContinue == step)
		{
			Singleton<MapFTUEManager>.Instance.OnFinishedStep.RemoveListener(OnStepCompleted);
			Singleton<MapFTUEManager>.Instance.StartStep(stepToStart);
		}
	}

	protected override void Clear()
	{
		base.Clear();
		Singleton<MapFTUEManager>.Instance.OnFinishedStep.RemoveListener(OnStepCompleted);
	}

	protected override void OnStartedFTUE()
	{
		base.OnStartedFTUE();
		Singleton<MapFTUEManager>.Instance.OnFinishedStep.AddListener(OnStepCompleted);
	}
}
