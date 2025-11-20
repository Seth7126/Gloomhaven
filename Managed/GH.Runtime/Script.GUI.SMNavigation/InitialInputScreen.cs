using System.Linq;
using InControl;
using Script.GUI.SMNavigation.States.MainMenuStates;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Script.GUI.SMNavigation;

[RequireComponent(typeof(UIWindow))]
public class InitialInputScreen : Singleton<InitialInputScreen>
{
	private UIWindow _uiWindow;

	[SerializeField]
	private MainStateTag _enteringTag;

	[SerializeField]
	private MainStateTag _exitTag;

	public UIWindow Window => _uiWindow;

	protected override void Awake()
	{
		base.Awake();
		_uiWindow = GetComponent<UIWindow>();
	}

	private void Update()
	{
		if (_uiWindow.IsOpen)
		{
			if (InControl.InputManager.Devices.Any((InputDevice x) => x.DeviceClass == InputDeviceClass.Controller && x.AnyControlIsPressed))
			{
				SelectInputDevice(isGamepad: true);
			}
			else if (InputSystemUtilities.AnyKeyDown() || InputSystemUtilities.AnyMouseButtonDown())
			{
				SelectInputDevice(isGamepad: false);
			}
		}
	}

	public void Show()
	{
		_uiWindow.Show();
		Singleton<UINavigation>.Instance.StateMachine.Enter(_enteringTag);
	}

	private void SelectInputDevice(bool isGamepad)
	{
		SelectInputDeviceBox.ChangeDevice(isGamepad);
		_uiWindow.Hide();
		Singleton<UINavigation>.Instance.StateMachine.Enter(_exitTag);
	}
}
