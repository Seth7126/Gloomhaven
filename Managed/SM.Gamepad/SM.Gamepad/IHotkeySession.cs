using System;
using System.Collections.Generic;

namespace SM.Gamepad;

public interface IHotkeySession : IDisposable
{
	bool IsDispose { get; }

	IHotkeySession AddOrReplaceHotkey(string hotkey, Action action);

	bool RemoveHotkey(string hotkey);

	void RemoveAllHotkeys();

	bool ContainsHotkey(string hotkey);

	IEnumerable<KeyValuePair<string, Action>> GetHotkeys();

	IHotkeySession CopyHotkeys(IHotkeySession session);

	void ChangeContainer(IHotkeyContainer container);
}
