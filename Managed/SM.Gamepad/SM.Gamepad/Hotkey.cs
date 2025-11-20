using System;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SM.Gamepad;

public class Hotkey : MonoBehaviour
{
	[SerializeField]
	private bool _hideIfKeyboardAndMouse;

	[SerializeField]
	private InputDisplayData _displayData;

	[SerializeField]
	private TMP_Text _label;

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private bool _overridenIconSize;

	public string ExpectedEvent;

	private Action _hotkeyAction;

	private IHotkeyActionInput _hotkeyActionInput;

	private static ILocalizationClient _localizationClient;

	[UsedImplicitly]
	private void OnDestroy()
	{
		Deinitialize();
	}

	public static void SetLocalizationClient(ILocalizationClient client)
	{
		_localizationClient = client;
	}

	private void OnLanguageChanged()
	{
		UpdateHotkeyLabel();
	}

	public void Initialize(IHotkeyActionInput hotkeyActionInput, InputDisplayData displayData = null, Action hotkeyAction = null, bool activate = true)
	{
		_hotkeyActionInput = hotkeyActionInput;
		if (displayData != null)
		{
			_displayData = displayData;
		}
		if (_localizationClient != null)
		{
			_localizationClient.OnLanguageChanged -= OnLanguageChanged;
			_localizationClient.OnLanguageChanged += OnLanguageChanged;
		}
		_hotkeyAction = hotkeyAction;
		UpdateHotkeyDisplay();
		DisplayHotkey(activate);
		_hotkeyActionInput.OnInputDeviceTypeChanged -= OnInputDeviceTypeChanged;
		_hotkeyActionInput.OnInputDeviceTypeChanged += OnInputDeviceTypeChanged;
		_hotkeyActionInput.OnInputEvent -= OnInputEvent;
		_hotkeyActionInput.OnInputEvent += OnInputEvent;
	}

	public void Deinitialize()
	{
		if (_localizationClient != null)
		{
			_localizationClient.OnLanguageChanged -= OnLanguageChanged;
		}
		_hotkeyAction = null;
		if (_hotkeyActionInput != null)
		{
			_hotkeyActionInput.OnInputDeviceTypeChanged -= OnInputDeviceTypeChanged;
			_hotkeyActionInput.OnInputEvent -= OnInputEvent;
		}
	}

	private void OnInputDeviceTypeChanged(InputDisplayData.InputDeviceType newInputDeviceType)
	{
		if (newInputDeviceType == InputDisplayData.InputDeviceType.KeyboardAndMouse && _hideIfKeyboardAndMouse)
		{
			DisplayHotkey(active: false);
		}
		else
		{
			DisplayHotkey(active: true);
		}
		UpdateHotkeyIcon(newInputDeviceType);
	}

	protected virtual void OnInputEvent(string eventName)
	{
		if (eventName == ExpectedEvent)
		{
			_hotkeyAction?.Invoke();
		}
	}

	public void UpdateHotkeyDisplay()
	{
		UpdateHotkeyIcon();
		UpdateHotkeyLabel();
	}

	public void DisplayHotkey(bool active)
	{
		if (_hotkeyActionInput == null || !(_hotkeyActionInput.CurrentDeviceType == InputDisplayData.InputDeviceType.KeyboardAndMouse && _hideIfKeyboardAndMouse && active))
		{
			if (_icon != null)
			{
				_icon.enabled = active;
			}
			if (_label != null)
			{
				_label.enabled = active;
			}
		}
	}

	public void UpdateHotkeyIcon()
	{
		if (!(_icon == null) && !(_displayData == null) && _hotkeyActionInput != null)
		{
			Sprite icon = _displayData.GetIcon(ExpectedEvent, _hotkeyActionInput.CurrentDeviceType);
			_icon.sprite = icon;
			if (_overridenIconSize)
			{
				UpdateIconSize(icon);
			}
		}
	}

	public void UpdateIconSize(Sprite sprite)
	{
		LayoutElement component = _icon.GetComponent<LayoutElement>();
		if (component != null)
		{
			float preferredWidth = (component.minWidth = ImageExtensions.GetMatchedWidth(component.minHeight, sprite));
			component.preferredWidth = preferredWidth;
		}
	}

	public void UpdateHotkeyIcon(InputDisplayData.InputDeviceType inputDeviceType)
	{
		if (!(_icon == null) && !(_displayData == null))
		{
			_icon.sprite = _displayData.GetIcon(ExpectedEvent, inputDeviceType);
		}
	}

	public void UpdateHotkeyLabel()
	{
		if (!(_label == null) && !(_displayData == null))
		{
			if (_localizationClient != null)
			{
				_label.text = _localizationClient.GetTranslation(_displayData.GetLocalizationKey(ExpectedEvent));
			}
			else
			{
				_label.text = _displayData.GetLocalizationKey(ExpectedEvent);
			}
		}
	}
}
