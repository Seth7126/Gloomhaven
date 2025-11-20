using UnityEngine;
using UnityEngine.InputSystem;

namespace SRDebugger.Gamepad;

public class DefaultGamepad : IGamepad
{
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
			result = new DefaultGamepadButton(name, UnityEngine.InputSystem.Gamepad.current.buttonSouth);
			break;
		case "buttonNorth":
		case "yButton":
		case "Y":
		case "triangleButton":
		case "triangle":
			result = new DefaultGamepadButton(name, UnityEngine.InputSystem.Gamepad.current.buttonNorth);
			break;
		case "buttonWest":
		case "xButton":
		case "X":
		case "squareButton":
		case "square":
			result = new DefaultGamepadButton(name, UnityEngine.InputSystem.Gamepad.current.buttonWest);
			break;
		case "buttonEast":
		case "bButton":
		case "B":
		case "circleButton":
		case "circle":
			result = new DefaultGamepadButton(name, UnityEngine.InputSystem.Gamepad.current.buttonEast);
			break;
		case "leftShoulder":
		case "leftBumper":
		case "L1":
			result = new DefaultGamepadButton(name, UnityEngine.InputSystem.Gamepad.current.leftShoulder);
			break;
		case "rightShoulder":
		case "rightBumper":
		case "R1":
			result = new DefaultGamepadButton(name, UnityEngine.InputSystem.Gamepad.current.rightShoulder);
			break;
		case "leftTrigger":
		case "L2":
			result = new DefaultGamepadButton(name, UnityEngine.InputSystem.Gamepad.current.leftTrigger);
			break;
		case "rightTrigger":
		case "R2":
			result = new DefaultGamepadButton(name, UnityEngine.InputSystem.Gamepad.current.rightTrigger);
			break;
		case "leftStickButton":
		case "L3":
			result = new DefaultGamepadButton(name, UnityEngine.InputSystem.Gamepad.current.leftStickButton);
			break;
		case "rightStickButton":
		case "R3":
			result = new DefaultGamepadButton(name, UnityEngine.InputSystem.Gamepad.current.rightStickButton);
			break;
		case "dpadDown":
		case "down":
			result = new DefaultGamepadButton(name, UnityEngine.InputSystem.Gamepad.current.dpad.down);
			break;
		case "dpadUp":
		case "up":
			result = new DefaultGamepadButton(name, UnityEngine.InputSystem.Gamepad.current.dpad.up);
			break;
		case "dpadLeft":
		case "left":
			result = new DefaultGamepadButton(name, UnityEngine.InputSystem.Gamepad.current.dpad.left);
			break;
		case "dpadRight":
		case "right":
			result = new DefaultGamepadButton(name, UnityEngine.InputSystem.Gamepad.current.dpad.right);
			break;
		}
		return result;
	}

	public Vector2 GetLeftStickValue()
	{
		return UnityEngine.InputSystem.Gamepad.current.leftStick.ReadValue();
	}

	public Vector2 GetRightStickValue()
	{
		return UnityEngine.InputSystem.Gamepad.current.rightStick.ReadValue();
	}

	public bool IsValid()
	{
		return UnityEngine.InputSystem.Gamepad.current != null;
	}
}
