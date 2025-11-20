using Platforms.Utils;

namespace Platforms.PS5;

public class GamepadPS5 : IPlatformJoystick
{
	public int JoystickIndex { get; }

	public DeviceModel Model => DeviceModel.PlayStationDualSense;

	public bool IsValid()
	{
		return JoystickIndex >= 0;
	}

	public GamepadPS5(int index)
	{
		JoystickIndex = index;
	}
}
