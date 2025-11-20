using System;
using InControl;

[Serializable]
public class BindingDetails
{
	public bool rebindable;

	public ControlBindingType BindingType;

	public InputControlType GamepadControl;

	public Mouse MouseControl;

	public Key KeyControl;

	[NonSerialized]
	public PlayerAction ActionBoundTo;

	public void AddBindingToControl(PlayerAction actionToAddBindingTo)
	{
		if (actionToAddBindingTo == null)
		{
			return;
		}
		ActionBoundTo = actionToAddBindingTo;
		switch (BindingType)
		{
		case ControlBindingType.Gamepad:
			if (GamepadControl != InputControlType.None)
			{
				actionToAddBindingTo.AddBinding(new DeviceBindingSource(GamepadControl));
			}
			break;
		case ControlBindingType.Mouse:
			if (MouseControl != Mouse.None)
			{
				actionToAddBindingTo.AddBinding(new MouseBindingSource(MouseControl));
			}
			break;
		case ControlBindingType.Keyboard:
			if (KeyControl != Key.None)
			{
				actionToAddBindingTo.AddBinding(new KeyBindingSource(KeyControl));
			}
			break;
		}
	}

	public void RemoveBindingFromControl(PlayerAction actionToAddBindingTo)
	{
		if (actionToAddBindingTo == null)
		{
			return;
		}
		ActionBoundTo = actionToAddBindingTo;
		switch (BindingType)
		{
		case ControlBindingType.Gamepad:
			if (GamepadControl != InputControlType.None)
			{
				actionToAddBindingTo.RemoveBinding(GamepadControl);
			}
			break;
		case ControlBindingType.Mouse:
			if (MouseControl != Mouse.None)
			{
				actionToAddBindingTo.RemoveBinding(MouseControl);
			}
			break;
		case ControlBindingType.Keyboard:
			if (KeyControl != Key.None)
			{
				actionToAddBindingTo.RemoveBinding(KeyControl);
			}
			break;
		}
	}
}
