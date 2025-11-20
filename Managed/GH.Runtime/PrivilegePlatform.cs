using System;

[Flags]
public enum PrivilegePlatform
{
	Switch = 1,
	Xbox = 2,
	PS = 4,
	AllExceptSwitch = 6,
	All = 7
}
