using System;
using System.Collections.Generic;

namespace SM.Gamepad;

public class EmptyHotkeySession : IHotkeySession, IDisposable
{
	public string Id => string.Empty;

	public bool IsDispose { get; private set; }

	public void Dispose()
	{
	}

	public IHotkeySession CopyHotkeys(IHotkeySession session)
	{
		return this;
	}

	public void ChangeContainer(IHotkeyContainer container)
	{
	}

	public IHotkeySession AddOrReplaceHotkey(string hotkey, Action action)
	{
		return this;
	}

	public bool RemoveHotkey(string hotkey)
	{
		return true;
	}

	public void RemoveAllHotkeys()
	{
	}

	public bool ContainsHotkey(string hotkey)
	{
		return false;
	}

	public IEnumerable<KeyValuePair<string, Action>> GetHotkeys()
	{
		yield break;
	}
}
