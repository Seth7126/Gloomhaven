using System;

namespace UnityEngine.UI;

[Serializable]
internal class UIWindowSegmentWrapper : ListWrapper<UIWindowSegment>
{
	public WindowSegmentTransitionType groupTransitionType;

	public bool manuallyTransitionGroup;

	public bool useGroupTransitionInterval;

	public float groupTransitionInterval;
}
