using UnityEngine;
using UnityEngine.EventSystems;

public class StandaloneInputModuleExtended : StandaloneInputModule, IInputModulePointer
{
	public GameObject GameObjectUnderPointer(int pointerId = -1)
	{
		return GetLastPointerEventData(pointerId)?.pointerCurrentRaycast.gameObject;
	}
}
