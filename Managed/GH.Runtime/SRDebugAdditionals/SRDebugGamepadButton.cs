using InControl;
using SRDebugger.Gamepad;

namespace SRDebugAdditionals;

public class SRDebugGamepadButton : IGamepadButton
{
	private string _name;

	private PlayerAction _input;

	public SRDebugGamepadButton(string name, PlayerAction input)
	{
		_name = name;
		_input = input;
	}

	public string GetName()
	{
		return _name;
	}

	public bool IsPressed()
	{
		return _input.IsPressed;
	}

	public bool IsWasPressed()
	{
		return _input.WasPressed;
	}

	public bool IsWasReleased()
	{
		return _input.WasReleased;
	}
}
