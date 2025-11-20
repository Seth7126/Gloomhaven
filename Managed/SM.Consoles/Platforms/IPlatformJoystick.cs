using Platforms.Utils;

namespace Platforms;

public interface IPlatformJoystick
{
	int JoystickIndex { get; }

	DeviceModel Model { get; }

	bool IsValid();
}
