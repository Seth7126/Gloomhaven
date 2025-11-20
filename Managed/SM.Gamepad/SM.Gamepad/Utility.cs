#define ENABLE_LOGS
using SM.Utils;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

namespace SM.Gamepad;

public static class Utility
{
	public static ButtonControl GetButtonControlByName(this UnityEngine.InputSystem.Gamepad gamepad, string buttonName)
	{
		if (gamepad == null)
		{
			LogUtils.LogError("[GamepadButtonsComboController] There is no gamepad");
			return null;
		}
		if (TryGetButtonTypeByName(gamepad, buttonName, out var _, out var button))
		{
			return button;
		}
		return null;
	}

	public static bool TryGetButtonTypeByName(UnityEngine.InputSystem.Gamepad gamepad, string buttonName, out GamepadButton gamepadButtonType, out ButtonControl button)
	{
		GamepadButton? buttonTypeByName = GetButtonTypeByName(buttonName);
		gamepadButtonType = buttonTypeByName.GetValueOrDefault();
		button = null;
		if (buttonTypeByName.HasValue)
		{
			button = GetButtonControlByType(gamepad, gamepadButtonType);
		}
		return buttonTypeByName.HasValue;
	}

	public static ButtonControl GetButtonControlByType(UnityEngine.InputSystem.Gamepad gamepad, GamepadButton type)
	{
		return gamepad[type];
	}

	public static GamepadButton? GetButtonTypeByName(string buttonName)
	{
		switch (buttonName)
		{
		case "buttonSouth":
		case "aButton":
		case "A":
		case "crossButton":
		case "cross":
			return GamepadButton.South;
		case "buttonNorth":
		case "yButton":
		case "Y":
		case "triangleButton":
		case "triangle":
			return GamepadButton.North;
		case "buttonWest":
		case "xButton":
		case "X":
		case "squareButton":
		case "square":
			return GamepadButton.West;
		case "buttonEast":
		case "bButton":
		case "B":
		case "circleButton":
		case "circle":
			return GamepadButton.East;
		case "leftShoulder":
		case "leftBumper":
		case "L1":
			return GamepadButton.LeftShoulder;
		case "rightShoulder":
		case "rightBumper":
		case "R1":
			return GamepadButton.RightShoulder;
		case "leftTrigger":
		case "L2":
			return GamepadButton.LeftTrigger;
		case "rightTrigger":
		case "R2":
			return GamepadButton.RightTrigger;
		case "leftStickButton":
		case "L3":
			return GamepadButton.LeftStick;
		case "rightStickButton":
		case "R3":
			return GamepadButton.RightStick;
		case "dpadDown":
		case "down":
			return GamepadButton.DpadDown;
		case "dpadUp":
		case "up":
			return GamepadButton.DpadUp;
		case "dpadLeft":
		case "left":
			return GamepadButton.DpadLeft;
		case "dpadRight":
		case "right":
			return GamepadButton.DpadRight;
		case "start":
		case "menu":
		case "options":
			return GamepadButton.Start;
		case "select":
		case "view":
		case "share":
			return GamepadButton.Select;
		default:
			LogUtils.LogWarning("[GamepadButtonsComboController] There is no button with name " + buttonName + ";");
			return null;
		}
	}
}
