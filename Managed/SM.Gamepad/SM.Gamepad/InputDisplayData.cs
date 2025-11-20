using System;
using System.Linq;
using UnityEngine;

namespace SM.Gamepad;

[CreateAssetMenu(menuName = "Data/Input Display")]
public class InputDisplayData : ScriptableObject
{
	[Flags]
	public enum InputDeviceType
	{
		KeyboardAndMouse = 1,
		Generic = 2,
		PlayStation4 = 4,
		PlayStation5 = 8,
		PlayStation = 0xC,
		XBoxOne = 0x10,
		XBoxSeries = 0x20,
		XBox = 0x30,
		Switch = 0x40,
		Stadia = 0x80
	}

	public enum GamepadKey
	{
		North,
		South,
		West,
		East,
		DPad,
		DPadUp,
		DPadDown,
		DPadUpDown,
		DPadLeft,
		DPadRight,
		DPadLeftRight,
		LStick,
		LStickPress,
		LStickUp,
		LStickDown,
		LStickUpDown,
		LStickLeft,
		LStickRight,
		LStickLeftRight,
		RStick,
		RStickPress,
		RStickUp,
		RStickDown,
		RStickUpDown,
		RStickLeft,
		RStickRight,
		RStickLeftRight,
		LB,
		RB,
		LT,
		RT,
		Share,
		Menu,
		LB_RB,
		LT_RT
	}

	[Serializable]
	public struct GamepadKeyIconMapper
	{
		public GamepadKey GamepadKey;

		public Sprite Icon;
	}

	[Serializable]
	public class GamepadIconMapper
	{
		public InputDeviceType InputDeviceType;

		public GamepadKeyIconMapper[] MappedGamepadKeyIcons;

		public bool Is(InputDeviceType inputDeviceType)
		{
			return InputDeviceType.HasFlag(inputDeviceType);
		}
	}

	[Serializable]
	public class InputKeyDisplay
	{
		public string KeyName;

		public string LocalizationKey;

		public GamepadKey GamepadKey;

		public Sprite KeyboardAndMouseIcon;
	}

	[Header("Defaults")]
	[SerializeField]
	private Sprite _defaultIcon;

	[SerializeField]
	private string _defaultLocalizationKey = "DEFAULT_LOCALIZATION_KEY";

	[SerializeField]
	private string _prefix = "";

	[Header("Keys")]
	[SerializeField]
	private GamepadIconMapper[] _gamepadIconMappers;

	[SerializeField]
	private InputKeyDisplay[] _keyDisplays;

	public Sprite GetIcon(string keyName, InputDeviceType inputDeviceType)
	{
		InputKeyDisplay inputKeyDisplay = _keyDisplays.FirstOrDefault((InputKeyDisplay keyDisplay) => keyDisplay.KeyName == keyName);
		if (inputKeyDisplay == null)
		{
			return _defaultIcon;
		}
		if (inputDeviceType == InputDeviceType.KeyboardAndMouse)
		{
			if (!(inputKeyDisplay.KeyboardAndMouseIcon == null))
			{
				return inputKeyDisplay.KeyboardAndMouseIcon;
			}
			return _defaultIcon;
		}
		foreach (GamepadIconMapper item in _gamepadIconMappers.Where((GamepadIconMapper gamepadIconMapper) => gamepadIconMapper.Is(inputDeviceType)))
		{
			GamepadKeyIconMapper[] mappedGamepadKeyIcons = item.MappedGamepadKeyIcons;
			for (int num = 0; num < mappedGamepadKeyIcons.Length; num++)
			{
				GamepadKeyIconMapper gamepadKeyIconMapper = mappedGamepadKeyIcons[num];
				if (gamepadKeyIconMapper.GamepadKey == inputKeyDisplay.GamepadKey)
				{
					return gamepadKeyIconMapper.Icon;
				}
			}
		}
		return _defaultIcon;
	}

	public Sprite GetIcon(string keyName, IHotkeyActionInput hotkeyActionInput)
	{
		return GetIcon(keyName, hotkeyActionInput.CurrentDeviceType);
	}

	public string GetLocalizationKey(string keyName)
	{
		InputKeyDisplay inputKeyDisplay = _keyDisplays.FirstOrDefault((InputKeyDisplay keyDisplay) => keyDisplay.KeyName == keyName);
		if (inputKeyDisplay != null && inputKeyDisplay.LocalizationKey != null)
		{
			return _prefix + inputKeyDisplay.LocalizationKey;
		}
		return _prefix + _defaultLocalizationKey;
	}
}
