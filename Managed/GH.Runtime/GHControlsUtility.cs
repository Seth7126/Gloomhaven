using System.ComponentModel;
using InControl;
using UnityEngine.InputSystem.LowLevel;

public static class GHControlsUtility
{
	public static InputControlType ToInputControlType(this GamepadButton gamepadButton)
	{
		return gamepadButton switch
		{
			GamepadButton.North => InputControlType.Action4, 
			GamepadButton.South => InputControlType.Action1, 
			GamepadButton.East => InputControlType.Action2, 
			GamepadButton.West => InputControlType.Action3, 
			GamepadButton.Start => InputControlType.Start, 
			GamepadButton.Select => InputControlType.Select, 
			GamepadButton.LeftShoulder => InputControlType.LeftBumper, 
			GamepadButton.RightShoulder => InputControlType.RightBumper, 
			GamepadButton.LeftTrigger => InputControlType.LeftTrigger, 
			GamepadButton.RightTrigger => InputControlType.RightTrigger, 
			GamepadButton.LeftStick => InputControlType.LeftStickButton, 
			GamepadButton.RightStick => InputControlType.RightStickButton, 
			GamepadButton.DpadUp => InputControlType.DPadUp, 
			GamepadButton.DpadDown => InputControlType.DPadDown, 
			GamepadButton.DpadLeft => InputControlType.DPadLeft, 
			GamepadButton.DpadRight => InputControlType.DPadRight, 
			_ => throw new InvalidEnumArgumentException("gamepadButton", (int)gamepadButton, typeof(GamepadButton)), 
		};
	}
}
