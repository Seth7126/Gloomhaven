using System;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using UnityEngine;

namespace Script.GUI.Popups;

public class InputButton : MonoBehaviour
{
	[SerializeField]
	private Hotkey _hotkey;

	private KeyActionHandler _keyActionHandler;

	[field: SerializeField]
	public ExtendedButton ExtendedButton { get; private set; }

	[field: SerializeField]
	public ControllerInputElement ControllerInputElement { get; private set; }

	public event Action ButtonPressed;

	public void InitializeInputGamepad(KeyAction keyAction, IKeyActionHandlerBlocker[] blockers)
	{
		if (InputManager.GamePadInUse)
		{
			_hotkey.Initialize(Singleton<UINavigation>.Instance.Input);
			_keyActionHandler = new KeyActionHandler(keyAction, OnPressed);
			foreach (IKeyActionHandlerBlocker keyActionHandlerBlocker in blockers)
			{
				_keyActionHandler.AddBlocker(keyActionHandlerBlocker);
			}
			Singleton<KeyActionHandlerController>.Instance.AddHandler(_keyActionHandler);
		}
	}

	public void Deinitialize()
	{
		if (_keyActionHandler != null)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(_keyActionHandler);
		}
		this.ButtonPressed = null;
	}

	private void OnPressed()
	{
		this.ButtonPressed?.Invoke();
	}
}
