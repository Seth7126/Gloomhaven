namespace SRDebugger.Gamepad;

public interface IGamepadButton
{
	string GetName();

	bool IsPressed();

	bool IsWasPressed();

	bool IsWasReleased();
}
