using UnityEngine;

public class UIMapFTUECompleteStepListener : UIMapFTUEListener
{
	[SerializeField]
	private EMapFTUEStep step;

	public void CompleteStep()
	{
		Singleton<MapFTUEManager>.Instance.CompleteStep(step);
		Clear();
	}
}
