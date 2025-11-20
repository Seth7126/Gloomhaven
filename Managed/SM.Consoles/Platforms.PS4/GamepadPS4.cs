using Platforms.Utils;

namespace Platforms.PS4;

public class GamepadPS4 : IPlatformJoystick
{
	public int JoystickIndex { get; }

	public DeviceModel Model => DeviceModel.PlayStation;

	public bool IsValid()
	{
		return JoystickIndex >= 0;
	}

	public GamepadPS4(int index)
	{
		JoystickIndex = index;
	}
}
