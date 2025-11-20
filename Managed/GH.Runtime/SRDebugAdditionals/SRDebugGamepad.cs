using SRDebugger.Gamepad;
using UnityEngine;
using Utilities;

namespace SRDebugAdditionals;

public class SRDebugGamepad : IGamepad
{
	private BasicControl _basicControl;

	public SRDebugGamepad()
	{
		_basicControl = new BasicControl();
		_basicControl.Enable();
	}

	public IGamepadButton GetGamepadButton(string name)
	{
		IGamepadButton result = null;
		switch (name)
		{
		case "buttonSouth":
		case "aButton":
		case "A":
		case "crossButton":
		case "cross":
			result = new SRDebugGamepadButton(name, Singleton<InputManager>.Instance.PlayerControl.DebugButtonSouth);
			break;
		case "buttonEast":
		case "bButton":
		case "B":
		case "circleButton":
		case "circle":
			result = new SRDebugGamepadButton(name, Singleton<InputManager>.Instance.PlayerControl.DebugButtonEast);
			break;
		case "leftStickButton":
		case "L3":
			result = new SRDebugGamepadButton(name, Singleton<InputManager>.Instance.PlayerControl.DebugLeftStick);
			break;
		case "rightStickButton":
		case "R3":
			result = new SRDebugGamepadButton(name, Singleton<InputManager>.Instance.PlayerControl.DebugRightStick);
			break;
		}
		return result;
	}

	public Vector2 GetLeftStickValue()
	{
		return new Vector2(_basicControl.Gamepad.LeftStickX.ReadValue<float>(), _basicControl.Gamepad.LeftStickY.ReadValue<float>());
	}

	public Vector2 GetRightStickValue()
	{
		return new Vector2(_basicControl.Gamepad.RightStickX.ReadValue<float>(), _basicControl.Gamepad.RightStickY.ReadValue<float>());
	}

	public bool IsValid()
	{
		if (Singleton<InputManager>.Instance != null)
		{
			return Singleton<InputManager>.Instance.PlayerControl != null;
		}
		return false;
	}
}
