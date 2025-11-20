using UnityEngine;

namespace SRDebugger.Gamepad;

public interface IGamepad
{
	bool IsValid();

	IGamepadButton GetGamepadButton(string name);

	Vector2 GetLeftStickValue();

	Vector2 GetRightStickValue();
}
