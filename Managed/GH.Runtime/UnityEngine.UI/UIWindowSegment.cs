#define ENABLE_LOGS
using System;
using SM.Utils;

namespace UnityEngine.UI;

[Serializable]
public class UIWindowSegment : MonoBehaviour
{
	public WindowSegmentTransitionType individualTransitionType;

	public void StartTransition(WindowSegmentTransitionType groupTransitionType)
	{
		switch ((individualTransitionType == WindowSegmentTransitionType.None) ? groupTransitionType : individualTransitionType)
		{
		case WindowSegmentTransitionType.Show:
			base.gameObject.SetActive(value: true);
			break;
		case WindowSegmentTransitionType.Hide:
			base.gameObject.SetActive(value: false);
			break;
		default:
			LogUtils.LogWarning("Using unknown transition type for this window segment.");
			break;
		case WindowSegmentTransitionType.None:
			break;
		}
	}
}
