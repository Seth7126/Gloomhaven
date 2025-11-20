using System;
using System.Collections.Generic;
using AsmodeeNet.Foundation;
using InControl;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Gloomhaven;

public class ControlsSettings : MonoBehaviour, IEscapable
{
	[SerializeField]
	private KeyActionControlButton[] keyBindingButtons;

	[SerializeField]
	private ExtendedButton resetButton;

	[SerializeField]
	private ButtonSwitch enableSecondClickHexToConfirmToggle;

	[SerializeField]
	private ButtonSwitch switchSkipAndUndoButtonsToggle;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	[SerializeField]
	private ExtendedScrollRect scrollRect;

	[SerializeField]
	private bool m_IsAllowedToEscapeDuringSave;

	[SerializeField]
	private List<UIPlatformControllerActivator> controllers;

	[SerializeField]
	private GameObject _inputDeviceObject;

	[SerializeField]
	private ExtendedButton _inputDeviceButton;

	[SerializeField]
	private GameObject[] _gamepadSettingObjects;

	[SerializeField]
	private GameObject _controllerObject;

	[SerializeField]
	private ButtonSwitch _inverseAxisToggle;

	[SerializeField]
	private ButtonSwitch _vibrationToggle;

	[SerializeField]
	private UISliderController _stickSensitivitySlider;

	private bool recording;

	private KeyAction keyAction;

	private const string _mouseKeyboardLocKey = "Consoles/GUI_PC_Gamepad_Mouse_Keyboard";

	private const string _gamepadLocKey = "Consoles/GUI_PC_Gamepad_Gamepad";

	private int _minSensitivityValue = 10;

	public bool isRecording => recording;

	public bool IsInitialised { get; private set; }

	public bool IsAllowedToEscapeDuringSave => m_IsAllowedToEscapeDuringSave;

	private void Awake()
	{
	}

	private void OnDestroy()
	{
		enableSecondClickHexToConfirmToggle.OnValueChanged.RemoveAllListeners();
		switchSkipAndUndoButtonsToggle.OnValueChanged.RemoveAllListeners();
		resetButton.onClick.RemoveAllListeners();
		_inputDeviceButton.onClick.RemoveAllListeners();
		_inverseAxisToggle.OnValueChanged.RemoveAllListeners();
		_vibrationToggle.OnValueChanged.RemoveAllListeners();
		_stickSensitivitySlider.ClearOnSliderValueChanged();
		InputManager.DeviceChangedEvent = (Action)Delegate.Remove(InputManager.DeviceChangedEvent, new Action(ChangedDevice));
		keyBindingButtons = null;
	}

	private void OnSelected(Component row)
	{
		if (InputManager.GamePadInUse)
		{
			if (row == resetButton)
			{
				scrollRect.ScrollToTop();
			}
			else
			{
				scrollRect.ScrollToFit(row.transform as RectTransform);
			}
		}
	}

	private void OnSelectedDeviceButton(Component row)
	{
		scrollRect.ScrollToTop();
	}

	private void DisableNavigation()
	{
		for (int i = 0; i < keyBindingButtons.Length; i++)
		{
			keyBindingButtons[i].DisableNavigation();
		}
		resetButton.DisableNavigation();
		enableSecondClickHexToConfirmToggle.DisableNavigation();
		_inputDeviceButton.DisableNavigation();
		_inverseAxisToggle.DisableNavigation();
		_vibrationToggle.DisableNavigation();
		_stickSensitivitySlider.DisableNavigation();
	}

	private void EnableNavigation()
	{
		for (int i = 0; i < keyBindingButtons.Length; i++)
		{
			keyBindingButtons[i].EnableNavigation();
		}
		resetButton.SetNavigation(Navigation.Mode.Vertical);
		enableSecondClickHexToConfirmToggle.SetNavigation(Navigation.Mode.Vertical);
		_inputDeviceButton.SetNavigation(Navigation.Mode.Vertical);
		_inverseAxisToggle.SetNavigation(Navigation.Mode.Vertical);
		_vibrationToggle.SetNavigation(Navigation.Mode.Vertical);
		_stickSensitivitySlider.EnableNavigation();
		resetButton.Select();
	}

	private void OnDeselectedKeyRecord(KeyAction action)
	{
		if (action == keyAction)
		{
			StopRecording();
		}
	}

	private void OnValidate()
	{
		keyBindingButtons = GetComponentsInChildren<KeyActionControlButton>(includeInactive: true);
	}

	private void RefreshKeyButtons()
	{
		for (int i = 0; i < keyBindingButtons.Length; i++)
		{
			keyBindingButtons[i].Refresh();
		}
	}

	public void Initialize()
	{
		IsInitialised = false;
		foreach (UIPlatformControllerActivator controller in controllers)
		{
			if (controller != null)
			{
				controller.CheckPlatform();
			}
		}
		for (int i = 0; i < keyBindingButtons.Length; i++)
		{
			keyBindingButtons[i].Init(RecordNewKey, OnDeselectedKeyRecord, OnSelected);
		}
		Singleton<InputManager>.Instance.RegisterCallbackOnInitialized(RefreshKeyButtons);
		InputManager.DeviceChangedEvent = (Action)Delegate.Combine(InputManager.DeviceChangedEvent, new Action(ChangedDevice));
		InControl.InputManager.OnActiveDeviceChanged += OnActiveDeviceChanged;
		enableSecondClickHexToConfirmToggle.OnValueChanged.AddListener(ToggleSecondClickHexToConfirm);
		enableSecondClickHexToConfirmToggle.OnSelected.AddListener(delegate
		{
			OnSelected(enableSecondClickHexToConfirmToggle);
		});
		enableSecondClickHexToConfirmToggle.SetValue(SaveData.Instance.Global.EnableSecondClickHexToConfirm);
		switchSkipAndUndoButtonsToggle.OnValueChanged.AddListener(ToggleSwitchSkipAndUndoButtons);
		switchSkipAndUndoButtonsToggle.SetValue(SaveData.Instance.Global.SwitchSkipAndUndoButtons);
		resetButton.onClick.AddListener(Reset);
		resetButton.onSelected.AddListener(delegate
		{
			OnSelected(resetButton);
		});
		SetResetInteraction(Singleton<InputManager>.Instance.HasCustomKeybinds);
		_inputDeviceButton.onClick.AddListener(OpenSelectInputDevice);
		_inputDeviceButton.onSelected.AddListener(delegate
		{
			OnSelectedDeviceButton(_inputDeviceButton);
		});
		ChangedDevice();
		_vibrationToggle.OnValueChanged.AddListener(ToggleVibration);
		_vibrationToggle.OnSelected.AddListener(delegate
		{
			OnSelected(_vibrationToggle);
		});
		_vibrationToggle.SetValue(SaveData.Instance.Global.HasVibration);
		_inverseAxisToggle.OnValueChanged.AddListener(ToggleInverseAxis);
		_inverseAxisToggle.OnSelected.AddListener(delegate
		{
			OnSelected(_inverseAxisToggle);
		});
		_inverseAxisToggle.SetValue(SaveData.Instance.Global.InvertYAxis);
		_stickSensitivitySlider.SubscribeOnSliderValueChanged(AdjustStickSensitivity);
		_stickSensitivitySlider.OnSelected.AddListener(delegate
		{
			OnSelected(_stickSensitivitySlider);
		});
		_stickSensitivitySlider.SetAmount((int)(SaveData.Instance.Global.StickSensitivity * 100f));
		IsInitialised = true;
	}

	private void OnActiveDeviceChanged(InControl.InputDevice obj)
	{
		ChangedDevice();
	}

	private void ChangedDevice()
	{
		_inputDeviceButton.TextLanguageKey = (InputManager.GamePadInUse ? "Consoles/GUI_PC_Gamepad_Gamepad" : "Consoles/GUI_PC_Gamepad_Mouse_Keyboard");
		bool active = Singleton<InputManager>.Instance.IsPCAndGamepadVersion();
		_inputDeviceObject.SetActive(active);
		bool flag = Singleton<InputManager>.Instance.IsPCVersion();
		GameObject[] gamepadSettingObjects = _gamepadSettingObjects;
		for (int i = 0; i < gamepadSettingObjects.Length; i++)
		{
			gamepadSettingObjects[i].SetActive(flag && InputManager.GamePadInUse);
		}
		_controllerObject.SetActive(InputManager.GamePadInUse);
	}

	public void RecordNewKey(KeyAction keyAction)
	{
		if (recording)
		{
			if (keyAction == this.keyAction)
			{
				return;
			}
			RefreshKeyButtons();
		}
		recording = true;
		this.keyAction = keyAction;
		UIWindowManager.RegisterEscapable(this);
	}

	public void Reset()
	{
		StopRecording();
		Singleton<InputManager>.Instance.RestoreDefaultValues();
		RefreshKeyButtons();
		SetResetInteraction(interactable: false);
	}

	public void ToggleSecondClickHexToConfirm(bool enable)
	{
		SaveData.Instance.Global.EnableSecondClickHexToConfirm = !SaveData.Instance.Global.EnableSecondClickHexToConfirm;
		SaveData.Instance.SaveGlobalData();
	}

	public void ToggleSwitchSkipAndUndoButtons(bool enable)
	{
		SaveData.Instance.Global.SwitchSkipAndUndoButtons = !SaveData.Instance.Global.SwitchSkipAndUndoButtons;
		SaveData.Instance.SaveGlobalData();
	}

	public void ToggleScenarioSpeedUp(bool enable)
	{
		SaveData.Instance.Global.SpeedUpToggle = enable;
		SaveData.Instance.SaveGlobalData();
	}

	public void ToggleVibration(bool enable)
	{
		Singleton<InputManager>.Instance.SetVibration(enable);
	}

	public void ToggleInverseAxis(bool enable)
	{
		Singleton<InputManager>.Instance.SetInvertYAxis(enable);
	}

	public void AdjustStickSensitivity(float sensitivity)
	{
		if (sensitivity < (float)_minSensitivityValue)
		{
			_stickSensitivitySlider.SetAmount(_minSensitivityValue);
		}
		else
		{
			Singleton<InputManager>.Instance.SetStickSensitivity(sensitivity);
		}
	}

	private void OpenSelectInputDevice()
	{
		Singleton<SelectInputDeviceBox>.Instance.Active(SelectInputDeviceBox.ESelectInputReason.Options);
	}

	public void StopRecording()
	{
		if (recording)
		{
			RefreshKeyButtons();
			recording = false;
			SetResetInteraction(Singleton<InputManager>.Instance.HasCustomKeybinds);
			UIWindowManager.UnregisterEscapable(this);
		}
	}

	private void SetResetInteraction(bool interactable)
	{
		resetButton.interactable = interactable;
		resetButton.buttonText.color = (interactable ? UIInfoTools.Instance.basicTextColor : UIInfoTools.Instance.greyedOutTextColor);
	}

	private void Update()
	{
		if (!recording)
		{
			return;
		}
		foreach (UnityEngine.InputSystem.Key allowedKeyCode in Singleton<InputManager>.Instance.GetAllowedKeyCodes())
		{
			if (InputManager.GetKeyDown(allowedKeyCode) && Singleton<InputManager>.Instance.SetKey(keyAction, allowedKeyCode))
			{
				StopRecording();
				SetResetInteraction(Singleton<InputManager>.Instance.HasCustomKeybinds);
				RefreshKeyButtons();
			}
		}
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			StopRecording();
		}
	}

	public bool Escape()
	{
		StopRecording();
		return true;
	}

	public int Order()
	{
		return 0;
	}
}
