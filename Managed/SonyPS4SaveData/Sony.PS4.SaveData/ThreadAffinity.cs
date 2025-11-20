using System;

namespace Sony.PS4.SaveData;

[Flags]
public enum ThreadAffinity
{
	Core2 = 4,
	Core3 = 8,
	Core4 = 0x10,
	Core5 = 0x20,
	AllCores = 0x3C
}
