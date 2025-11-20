using UnityEngine;
using UnityEngine.Events;

public static class UnityEventExtensions
{
	public static bool HasMethod(this UnityEvent unityEvent, string name, Object target = null)
	{
		for (int i = 0; i < unityEvent.GetPersistentEventCount(); i++)
		{
			if (unityEvent.GetPersistentMethodName(i).Contains("OnClick") && (target == null || unityEvent.GetPersistentTarget(i) == target))
			{
				return true;
			}
		}
		return false;
	}
}
