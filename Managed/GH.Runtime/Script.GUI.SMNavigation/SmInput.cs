using System;
using System.Collections.Generic;
using SM.Gamepad;
using UnityEngine;

namespace Script.GUI.SMNavigation;

public class SmInput : IUIActionsInput, IHotkeyActionInput
{
	private readonly Dictionary<KeyAction, string> _bindings = new Dictionary<KeyAction, string>
	{
		{
			KeyAction.UI_SUBMIT,
			"Submit"
		},
		{
			KeyAction.UI_NEXT_TAB,
			"NextTab"
		},
		{
			KeyAction.UI_PREVIOUS_TAB,
			"PreviousTab"
		},
		{
			KeyAction.UI_R_LEFT,
			"Switch_Left"
		},
		{
			KeyAction.UI_R_RIGHT,
			"Switch_Right"
		},
		{
			KeyAction.UI_INFO,
			"Info"
		},
		{
			KeyAction.UI_RETRY,
			"Retry"
		},
		{
			KeyAction.UI_RENAME,
			"Rename"
		},
		{
			KeyAction.TOGGLE_VOICE_CHAT_CONTROL,
			"ToggleVoiceChatControl"
		},
		{
			KeyAction.UI_TIPS,
			"Tips"
		},
		{
			KeyAction.UI_STARTING_PERKS,
			"StartingPerks"
		},
		{
			KeyAction.UI_MERCENARY_INFO,
			"MercenaryInfo"
		},
		{
			KeyAction.UI_Reset_Mercenary,
			"ResetMercenary"
		}
	};

	private readonly Dictionary<KeyAction, Action> _bindingsActions = new Dictionary<KeyAction, Action>();

	public bool GamePadInUse => InputManager.GamePadInUse;

	public InputDisplayData.InputDeviceType CurrentDeviceType
	{
		get
		{
			if (Singleton<InputManager>.Instance == null)
			{
				return InputDisplayData.InputDeviceType.KeyboardAndMouse;
			}
			return GetDeviceTypeBasedOnGhController(Singleton<InputManager>.Instance.CurrentControllerType);
		}
		set
		{
		}
	}

	public event Action<UIActionBaseEventData> MoveSelectionEvent;

	public event Action<string> OnInputEvent;

	public event Action<InputDisplayData.InputDeviceType> OnInputDeviceTypeChanged;

	public SmInput()
	{
		InputManager.OnKeybindingsInitialized = (Action)Delegate.Combine(InputManager.OnKeybindingsInitialized, new Action(InitializeBindings));
		InputManager.OnControllerTypeChangedEvent = (Action<ControllerType>)Delegate.Combine(InputManager.OnControllerTypeChangedEvent, new Action<ControllerType>(OnGHInputDeviceChanged));
	}

	public void Destroy()
	{
		RemoveBindings();
		InputManager.OnKeybindingsInitialized = (Action)Delegate.Remove(InputManager.OnKeybindingsInitialized, new Action(InitializeBindings));
		InputManager.OnControllerTypeChangedEvent = (Action<ControllerType>)Delegate.Remove(InputManager.OnControllerTypeChangedEvent, new Action<ControllerType>(OnGHInputDeviceChanged));
	}

	private void OnGHInputDeviceChanged(ControllerType controllerType)
	{
		this.OnInputDeviceTypeChanged?.Invoke(GetDeviceTypeBasedOnGhController(controllerType));
	}

	private InputDisplayData.InputDeviceType GetDeviceTypeBasedOnGhController(ControllerType controllerType)
	{
		if (IsWindowsPlatform() && controllerType != ControllerType.Generic)
		{
			return InputDisplayData.InputDeviceType.XBox;
		}
		return controllerType switch
		{
			ControllerType.Generic => InputDisplayData.InputDeviceType.Generic, 
			ControllerType.XboxOne => InputDisplayData.InputDeviceType.XBoxOne, 
			ControllerType.MouseKeyboard => InputDisplayData.InputDeviceType.KeyboardAndMouse, 
			ControllerType.PS4 => InputDisplayData.InputDeviceType.PlayStation4, 
			ControllerType.PS5 => InputDisplayData.InputDeviceType.PlayStation5, 
			ControllerType.Switch => InputDisplayData.InputDeviceType.Switch, 
			_ => throw new ArgumentException($"[SmInput] Cant convert GH ControllerType to SM InputDeviceType {controllerType}"), 
		};
	}

	private bool IsWindowsPlatform()
	{
		RuntimePlatform platform = Application.platform;
		return platform == RuntimePlatform.WindowsEditor || platform == RuntimePlatform.WindowsPlayer;
	}

	private void InitializeBindings()
	{
		foreach (KeyValuePair<KeyAction, string> binding in _bindings)
		{
			KeyAction key = binding.Key;
			string inputData = binding.Value;
			_bindingsActions.Add(key, TriggerReleaseOnInput);
			InputManager.RegisterToOnPressed(key, TriggerReleaseOnInput);
			void TriggerReleaseOnInput()
			{
				ReleaseOnInputEvent(inputData);
			}
		}
	}

	private void RemoveBindings()
	{
		foreach (KeyValuePair<KeyAction, Action> bindingsAction in _bindingsActions)
		{
			KeyAction key = bindingsAction.Key;
			Action value = bindingsAction.Value;
			InputManager.UnregisterToOnPressed(key, value);
		}
	}

	private void ReleaseOnInputEvent(string inputData)
	{
		this.OnInputEvent?.Invoke(inputData);
	}
}
