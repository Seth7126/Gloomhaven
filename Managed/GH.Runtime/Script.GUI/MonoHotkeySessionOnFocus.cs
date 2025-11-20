using UnityEngine;

namespace Script.GUI;

public class MonoHotkeySessionOnFocus : MonoHotkeySession
{
	[SerializeField]
	private ControllerInputArea _controllerArea;

	private void Awake()
	{
		_controllerArea.OnFocused.AddListener(base.Show);
		_controllerArea.OnUnfocused.AddListener(base.Hide);
	}
}
