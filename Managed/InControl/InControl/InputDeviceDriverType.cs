using UnityEngine;

namespace InControl;

public enum InputDeviceDriverType : ushort
{
	Unknown,
	HID,
	USB,
	Bluetooth,
	[InspectorName("XInput")]
	XInput,
	[InspectorName("DirectInput")]
	DirectInput,
	[InspectorName("RawInput")]
	RawInput,
	[InspectorName("AppleGameController")]
	AppleGameController,
	[InspectorName("SDLJoystick")]
	SDLJoystick,
	[InspectorName("SDLController")]
	SDLController
}
