using UnityEngine.InputSystem.Controls;

namespace SRDebugger.Gamepad;

public class DefaultGamepadButton : IGamepadButton
{
	private string _nameButton;

	private ButtonControl _button;

	public DefaultGamepadButton(string name, ButtonControl button)
	{
		_nameButton = name;
		_button = button;
	}

	public string GetName()
	{
		return _nameButton;
	}

	public bool IsPressed()
	{
		return _button.isPressed;
	}

	public bool IsWasPressed()
	{
		return _button.wasPressedThisFrame;
	}

	public bool IsWasReleased()
	{
		return _button.wasReleasedThisFrame;
	}
}
