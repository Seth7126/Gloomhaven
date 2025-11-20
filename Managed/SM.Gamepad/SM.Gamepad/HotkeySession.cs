using System;
using System.Collections.Generic;

namespace SM.Gamepad;

public class HotkeySession : IHotkeySession, IDisposable
{
	private IHotkeyContainer _hotkeyContainer;

	private Dictionary<string, Action> _hotkeys = new Dictionary<string, Action>();

	private string _id;

	private bool IsEmpty => _hotkeys.Count == 0;

	public bool IsDispose => _hotkeys == null;

	public HotkeySession(IHotkeyContainer hotkeyContainer, IHotkeySession session)
		: this(hotkeyContainer)
	{
		CopyHotkeys(session);
	}

	public HotkeySession(IHotkeyContainer hotkeyContainer)
	{
		SetContainer(hotkeyContainer);
	}

	public void Dispose()
	{
		if (!IsDispose)
		{
			RemoveAllHotkeys();
			_hotkeys = null;
		}
	}

	public void ChangeContainer(IHotkeyContainer container)
	{
		RemoveHotkeysFromContainer();
		SetContainer(container);
		AddOrReplaceHotkeysToContainer();
	}

	private void SetContainer(IHotkeyContainer hotkeyContainer)
	{
		_id = hotkeyContainer.RequestSessionId();
		_hotkeyContainer = hotkeyContainer;
	}

	public IHotkeySession CopyHotkeys(IHotkeySession session)
	{
		foreach (KeyValuePair<string, Action> hotkey in session.GetHotkeys())
		{
			AddOrReplaceHotkey(hotkey.Key, hotkey.Value);
		}
		return this;
	}

	public IHotkeySession AddOrReplaceHotkey(string hotkey, Action action)
	{
		if (ContainsHotkey(hotkey))
		{
			_hotkeys[hotkey] = action;
		}
		else
		{
			_hotkeys.TryAdd(hotkey, action);
		}
		AddOrReplaceHotkeysToContainer();
		return this;
	}

	public bool RemoveHotkey(string hotkey)
	{
		if (!ContainsHotkey(hotkey))
		{
			return false;
		}
		RemoveHotkeysFromContainer();
		_hotkeys.Remove(hotkey);
		AddOrReplaceHotkeysToContainer();
		return true;
	}

	public void RemoveAllHotkeys()
	{
		RemoveHotkeysFromContainer();
		_hotkeys.Clear();
	}

	public bool ContainsHotkey(string hotkey)
	{
		return _hotkeys.ContainsKey(hotkey);
	}

	public IEnumerable<KeyValuePair<string, Action>> GetHotkeys()
	{
		foreach (KeyValuePair<string, Action> hotkey in _hotkeys)
		{
			yield return hotkey;
		}
	}

	private void AddOrReplaceHotkeysToContainer()
	{
		if (!IsEmpty)
		{
			_hotkeyContainer?.AddOrReplaceHotkeysForObject(_id, _hotkeys);
		}
	}

	private void RemoveHotkeysFromContainer()
	{
		if (!IsEmpty)
		{
			_hotkeyContainer?.RemoveHotkeysForObject(_id);
		}
	}
}
