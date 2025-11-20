using UnityEngine;

namespace Script.GUI;

public class MonoHotkeySessionOnFocusLocal : MonoHotkeySession
{
	[SerializeField]
	private ControllerInputAreaLocal _controllerArea;

	private void Awake()
	{
		_controllerArea.OnFocusedArea.AddListener(base.Show);
		_controllerArea.OnUnfocusedArea.AddListener(base.Hide);
	}
}
