using UnityEngine;

namespace Script.GUI.Controller.Area;

public class MapCursorProxy : MonoBehaviour
{
	[SerializeField]
	private ControllerInputMapArea _mapAreaTarget;

	private static ControllerInputMapArea _mapArea;

	private void Awake()
	{
		_mapArea = _mapAreaTarget;
	}

	public static void EnableCursor()
	{
		if (_mapArea != null)
		{
			_mapArea.EnableCursor();
		}
	}
}
