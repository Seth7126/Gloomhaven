using System;

[Flags]
public enum EKeyActionTag
{
	None = 0,
	All = 1,
	Scenario = 2,
	Map = 4,
	Camera = 8,
	UI = 0x10,
	ControllerExclusive = 0x20,
	AreaShortcuts = 0x40,
	LocalAreaControls = 0x80,
	MainMenu = 0x100
}
