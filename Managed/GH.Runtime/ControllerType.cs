using System;

[Flags]
public enum ControllerType
{
	None = 0,
	MouseKeyboard = 2,
	XboxOne = 4,
	Xbox360 = 8,
	PS4 = 0x10,
	PS3 = 0x20,
	Generic = 0x40,
	PS5 = 0x80,
	Switch = 0x100
}
