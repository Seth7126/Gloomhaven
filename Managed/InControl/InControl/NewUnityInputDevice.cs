using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem.XInput;

namespace InControl;

public class NewUnityInputDevice : InputDevice
{
	private const float LowerDeadZone = 0.2f;

	private const float UpperDeadZone = 0.9f;

	public Gamepad UnityGamepad;

	private InputControlType leftCommandControl;

	private InputControlType rightCommandControl;

	public NewUnityInputDevice(Gamepad unityGamepad)
	{
		Init(unityGamepad);
	}

	private protected virtual void Init(Gamepad unityGamepad)
	{
		UnityGamepad = unityGamepad;
		base.SortOrder = unityGamepad.deviceId;
		base.DeviceClass = InputDeviceClass.Controller;
		base.DeviceStyle = DetectDeviceStyle(unityGamepad);
		leftCommandControl = base.DeviceStyle.LeftCommandControl();
		rightCommandControl = base.DeviceStyle.RightCommandControl();
		base.Name = unityGamepad.displayName;
		base.Meta = unityGamepad.displayName;
		AddControl(InputControlType.LeftStickLeft, UnityGamepad.leftStick.left.displayName, 0.2f, 0.9f);
		AddControl(InputControlType.LeftStickRight, UnityGamepad.leftStick.right.displayName, 0.2f, 0.9f);
		AddControl(InputControlType.LeftStickUp, UnityGamepad.leftStick.up.displayName, 0.2f, 0.9f);
		AddControl(InputControlType.LeftStickDown, UnityGamepad.leftStick.down.displayName, 0.2f, 0.9f);
		AddControl(InputControlType.RightStickLeft, UnityGamepad.rightStick.left.displayName, 0.2f, 0.9f);
		AddControl(InputControlType.RightStickRight, UnityGamepad.rightStick.right.displayName, 0.2f, 0.9f);
		AddControl(InputControlType.RightStickUp, UnityGamepad.rightStick.up.displayName, 0.2f, 0.9f);
		AddControl(InputControlType.RightStickDown, UnityGamepad.rightStick.down.displayName, 0.2f, 0.9f);
		AddControl(InputControlType.LeftTrigger, UnityGamepad.leftTrigger.displayName);
		AddControl(InputControlType.RightTrigger, UnityGamepad.rightTrigger.displayName);
		AddControl(InputControlType.LeftBumper, UnityGamepad.leftShoulder.displayName);
		AddControl(InputControlType.RightBumper, UnityGamepad.rightShoulder.displayName);
		AddControl(InputControlType.DPadUp, UnityGamepad.dpad.up.displayName);
		AddControl(InputControlType.DPadDown, UnityGamepad.dpad.down.displayName);
		AddControl(InputControlType.DPadLeft, UnityGamepad.dpad.left.displayName);
		AddControl(InputControlType.DPadRight, UnityGamepad.dpad.right.displayName);
		AddControl(InputControlType.Action1, UnityGamepad.buttonSouth.displayName);
		AddControl(InputControlType.Action2, UnityGamepad.buttonEast.displayName);
		AddControl(InputControlType.Action3, UnityGamepad.buttonWest.displayName);
		AddControl(InputControlType.Action4, UnityGamepad.buttonNorth.displayName);
		AddControl(InputControlType.LeftStickButton, UnityGamepad.leftStickButton.displayName);
		AddControl(InputControlType.RightStickButton, UnityGamepad.rightStickButton.displayName);
		AddControl(leftCommandControl, UnityGamepad.selectButton.displayName);
		AddControl(rightCommandControl, UnityGamepad.startButton.displayName);
	}

	public override void Update(ulong updateTick, float deltaTime)
	{
		UpdateLeftStickWithValue(UnityGamepad.leftStick.ReadUnprocessedValue(), updateTick, deltaTime);
		UpdateRightStickWithValue(UnityGamepad.rightStick.ReadUnprocessedValue(), updateTick, deltaTime);
		UpdateWithValue(InputControlType.LeftTrigger, UnityGamepad.leftTrigger.ReadUnprocessedValue(), updateTick, deltaTime);
		UpdateWithValue(InputControlType.RightTrigger, UnityGamepad.rightTrigger.ReadUnprocessedValue(), updateTick, deltaTime);
		UpdateWithState(InputControlType.DPadUp, UnityGamepad.dpad.up.isPressed, updateTick, updateTick);
		UpdateWithState(InputControlType.DPadDown, UnityGamepad.dpad.down.isPressed, updateTick, updateTick);
		UpdateWithState(InputControlType.DPadLeft, UnityGamepad.dpad.left.isPressed, updateTick, updateTick);
		UpdateWithState(InputControlType.DPadRight, UnityGamepad.dpad.right.isPressed, updateTick, updateTick);
		UpdateWithState(InputControlType.Action1, UnityGamepad.buttonSouth.isPressed, updateTick, updateTick);
		UpdateWithState(InputControlType.Action2, UnityGamepad.buttonEast.isPressed, updateTick, updateTick);
		UpdateWithState(InputControlType.Action3, UnityGamepad.buttonWest.isPressed, updateTick, updateTick);
		UpdateWithState(InputControlType.Action4, UnityGamepad.buttonNorth.isPressed, updateTick, updateTick);
		UpdateWithState(InputControlType.LeftBumper, UnityGamepad.leftShoulder.isPressed, updateTick, updateTick);
		UpdateWithState(InputControlType.RightBumper, UnityGamepad.rightShoulder.isPressed, updateTick, updateTick);
		UpdateWithState(InputControlType.LeftStickButton, UnityGamepad.leftStickButton.isPressed, updateTick, updateTick);
		UpdateWithState(InputControlType.RightStickButton, UnityGamepad.rightStickButton.isPressed, updateTick, updateTick);
		UpdateWithState(leftCommandControl, UnityGamepad.selectButton.isPressed, updateTick, updateTick);
		UpdateWithState(rightCommandControl, UnityGamepad.startButton.isPressed, updateTick, updateTick);
	}

	public override void Vibrate(float leftMotor, float rightMotor)
	{
		if (base.IsAttached)
		{
			UnityGamepad.SetMotorSpeeds(leftMotor, rightMotor);
		}
	}

	private static InputDeviceStyle DetectDeviceStyle(UnityEngine.InputSystem.InputDevice unityDevice)
	{
		if (!(unityDevice is XInputController))
		{
			if (!(unityDevice is DualShockGamepad))
			{
				if (unityDevice is SwitchProControllerHID)
				{
					return InputDeviceStyle.NintendoSwitch;
				}
				return InputDeviceStyle.Unknown;
			}
			return InputDeviceStyle.PlayStation4;
		}
		return InputDeviceStyle.XboxOne;
	}
}
