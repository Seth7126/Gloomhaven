using System;

namespace Script.GUI;

public readonly struct HotkeyAction
{
	public string KeyName { get; }

	public Action Action { get; }

	public HotkeyAction(string keyName, Action action)
	{
		KeyName = keyName;
		Action = action;
	}
}
