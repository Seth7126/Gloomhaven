using UnityEngine;

namespace GLOO.Introduction;

[CreateAssetMenu(menuName = "UI Config/FTUE/Tag Condition")]
public class MapFTUETagConditionConfigUI : IntroductionConditionConfigUI
{
	[SerializeField]
	private string tag;

	public override bool IsValid()
	{
		return Singleton<MapFTUEManager>.Instance.HasProcessedTag(tag);
	}
}
