using System.Collections;
using System.Linq;
using InControl;
using UnityEngine;
using UnityEngine.UI;

public class GamepadConnectionBox : Singleton<GamepadConnectionBox>
{
	[SerializeField]
	private UIWindow _uiWindow;

	[SerializeField]
	private UIButtonExtended _mouseKeyboard;

	[SerializeField]
	private TextLocalizedListener _warning;

	private const string warningOffline = "Consoles/GUI_PC_GAMEPAD_ADDED_WARNING";

	private const string warningClient = "Consoles/GUI_PC_GAMEPAD_ADDED_WARNING_CLIENT";

	private const string warningHost = "Consoles/GUI_PC_GAMEPAD_ADDED_WARNING_HOST";

	protected override void Awake()
	{
		base.Awake();
		_mouseKeyboard.onClick.AddListener(OnMouseKeyboardClicked);
	}

	public void Activate()
	{
		StartCoroutine(ActivateWindowCoroutine());
	}

	private IEnumerator ActivateWindowCoroutine()
	{
		while (SceneController.Instance.IsLoading)
		{
			yield return null;
		}
		if (!_uiWindow.IsOpen)
		{
			_uiWindow.Show();
			_warning.SetTextKey((!FFSNetwork.IsOnline) ? "Consoles/GUI_PC_GAMEPAD_ADDED_WARNING" : (FFSNetwork.IsHost ? "Consoles/GUI_PC_GAMEPAD_ADDED_WARNING_HOST" : "Consoles/GUI_PC_GAMEPAD_ADDED_WARNING_CLIENT"));
		}
	}

	private void Update()
	{
		if (_uiWindow.IsOpen && InControl.InputManager.Devices.Any((InputDevice x) => x.DeviceClass == InputDeviceClass.Controller && x.AnyControlIsPressed))
		{
			Deactivate();
			SwitchToGamepad();
		}
	}

	public void Deactivate()
	{
		_uiWindow.Hide();
	}

	private void OnMouseKeyboardClicked()
	{
		Deactivate();
	}

	private void SwitchToGamepad()
	{
		if (FFSNetwork.IsOnline)
		{
			UIManager.LoadMainMenu(null, delegate
			{
				PersistentData.s_Instance.ClearCardPools();
				SelectInputDeviceBox.ChangeDevice(isUseGamepad: true);
			});
		}
		else
		{
			SelectInputDeviceBox.ReloadSceneWithChangeDevice(isUseGamepad: true);
		}
	}
}
