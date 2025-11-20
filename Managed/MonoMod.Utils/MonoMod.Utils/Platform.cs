using System;

namespace MonoMod.Utils;

[Flags]
public enum Platform
{
	OS = 1,
	Bits64 = 2,
	NT = 4,
	Unix = 8,
	ARM = 0x10000,
	Unknown = 0x11,
	Windows = 0x25,
	MacOS = 0x49,
	Linux = 0x89,
	Android = 0x189,
	iOS = 0x249
}
