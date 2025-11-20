using UnityEngine;

namespace GLOO.Introduction;

[CreateAssetMenu(menuName = "UI Config/FTUE/Completed Step Condition")]
public class MapFTUECompletedStepConditionConfigUI : IntroductionConditionConfigUI
{
	[SerializeField]
	private EMapFTUEStep step;

	public override bool IsValid()
	{
		return Singleton<MapFTUEManager>.Instance.HasCompletedStep(step);
	}
}
