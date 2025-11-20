using GLOOM;
using InControl;
using UnityEngine;

[CreateAssetMenu(menuName = "UI Config/Gamepad Mapping")]
public class GamepadDeviceMappingConfig : ScriptableObject
{
	private const float DefaultSize = 26f;

	private const string L3 = "L3";

	private const string R3 = "R3";

	private const string LS = "LS";

	private const string RS = "RS";

	private const string DPAD = "<sprite name=\"DPad\">";

	public ControllerType DeviceTypeCompatible;

	public string Action1;

	public string Action2;

	public string Action3;

	public string Action4;

	[Space]
	public string LeftCommand = "<sprite name=\"Options\">";

	public string RightCommand = "<sprite name=\"Pause\">";

	[Space]
	public string LeftBumper = "LB";

	public string RightBumper = "RB";

	public string LeftTrigger = "LT";

	public string RightTrigger = "RT";

	[Header("Control type icons")]
	public Sprite LeftOption;

	public Sprite RightOption;

	public Sprite LeftBumperIcon;

	public Sprite RightBumperIcon;

	public Sprite LeftTriggerIcon;

	public Sprite RightTriggerIcon;

	public Sprite SouthButton;

	public Sprite NorthButton;

	public Sprite EastButton;

	public Sprite WestButton;

	public Sprite RightStickUp;

	public Sprite RightStickDown;

	public Sprite LeftStickMove;

	public Sprite RightStickButton;

	public Sprite GetActionTypeIcon(InputControlType inputControlType)
	{
		switch (inputControlType)
		{
		case InputControlType.Action1:
			return SouthButton;
		case InputControlType.Action2:
			return EastButton;
		case InputControlType.Action3:
			return WestButton;
		case InputControlType.Action4:
			return NorthButton;
		case InputControlType.LeftBumper:
			return LeftBumperIcon;
		case InputControlType.RightBumper:
			return RightBumperIcon;
		case InputControlType.LeftTrigger:
			return LeftTriggerIcon;
		case InputControlType.RightTrigger:
			return RightTriggerIcon;
		case InputControlType.LeftCommand:
			return LeftOption;
		case InputControlType.Options:
		case InputControlType.Pause:
		case InputControlType.Command:
		case InputControlType.RightCommand:
			return RightOption;
		case InputControlType.RightStickUp:
			return RightStickUp;
		case InputControlType.RightStickDown:
			return RightStickDown;
		case InputControlType.LeftStickUp:
		case InputControlType.LeftStickDown:
		case InputControlType.LeftStickLeft:
		case InputControlType.LeftStickRight:
		case InputControlType.LeftStickX:
		case InputControlType.LeftStickY:
			return LeftStickMove;
		case InputControlType.RightStickButton:
			return RightStickButton;
		default:
			return null;
		}
	}

	public string GetActionName(InputControlType inputControlType)
	{
		return inputControlType switch
		{
			InputControlType.Action1 => Action1, 
			InputControlType.Action2 => Action2, 
			InputControlType.Action3 => Action3, 
			InputControlType.Action4 => Action4, 
			InputControlType.RightCommand => RightCommand, 
			InputControlType.LeftCommand => LeftCommand, 
			_ => LocalizationManager.GetTranslation(inputControlType.ToString()), 
		};
	}

	public string GetShortActionName(InputControlType inputControlType)
	{
		switch (inputControlType)
		{
		case InputControlType.LeftBumper:
			return LeftBumper;
		case InputControlType.RightBumper:
			return RightBumper;
		case InputControlType.LeftTrigger:
			return LeftTrigger;
		case InputControlType.RightTrigger:
			return RightTrigger;
		case InputControlType.RightStickButton:
			return "R3";
		case InputControlType.LeftStickButton:
			return "L3";
		case InputControlType.LeftStickUp:
		case InputControlType.LeftStickDown:
		case InputControlType.LeftStickLeft:
		case InputControlType.LeftStickRight:
		case InputControlType.LeftStickX:
		case InputControlType.LeftStickY:
			return "LS";
		case InputControlType.RightStickUp:
		case InputControlType.RightStickDown:
		case InputControlType.RightStickLeft:
		case InputControlType.RightStickRight:
		case InputControlType.RightStickX:
		case InputControlType.RightStickY:
			return "RS";
		case InputControlType.DPadUp:
		case InputControlType.DPadDown:
		case InputControlType.DPadLeft:
		case InputControlType.DPadRight:
		case InputControlType.DPadX:
		case InputControlType.DPadY:
			return "<sprite name=\"DPad\">";
		default:
			return GetActionName(inputControlType);
		}
	}

	public string GetActionIconByName(string name)
	{
		string text = ((!(name == "UIMove")) ? string.Empty : LeftStickMove.name);
		return GetActionIcon(text);
	}

	public string GetActionIcon(InputControlType bindingControl, bool useSize = true)
	{
		return GetActionIcon(GetActionTypeIcon(bindingControl).name, useSize);
	}

	private string GetActionIcon(string name, bool useSize = true)
	{
		if (useSize)
		{
			return $"<font=\"Sarala-Regular SDF\" material=\"Sarala-Regular-Shadow SDF\"><size={26f}><sprite name=\"{name}\" color=#FFFFFFFF></size></font>";
		}
		return "<font=\"Sarala-Regular SDF\" material=\"Sarala-Regular-Shadow SDF\"><sprite name=\"" + name + "\" color=#FFFFFFFF></font>";
	}
}
