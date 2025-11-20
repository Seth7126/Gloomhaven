using System;
using System.Collections.Generic;

namespace SM.Gamepad;

public static class HotkeySessionUtils
{
	public static IHotkeySession GetHotkey(this IHotkeySession session, out SessionHotkey sessionHotkey, string hotkey, bool setShown, Action action = null)
	{
		sessionHotkey = session.GetHotkey(hotkey, action).SetShown(setShown);
		return session;
	}

	public static IHotkeySession GetHotkey(this IHotkeySession session, out SessionHotkey sessionHotkey, string hotkey, Action action = null)
	{
		sessionHotkey = session.GetHotkey(hotkey, action);
		return session;
	}

	public static SessionHotkey GetHotkey(this IHotkeySession session, string hotkey, Action action = null)
	{
		return new SessionHotkey(hotkey, action, session);
	}

	public static IHotkeySession Save(this SessionHotkey sessionHotkey, out SessionHotkey outSessionHotkey)
	{
		outSessionHotkey = sessionHotkey;
		return sessionHotkey.Session;
	}

	public static IHotkeySession ChangeSession(this IHotkeySession session, SessionHotkey sessionHotkey)
	{
		sessionHotkey.ChangeSession(session);
		return session;
	}

	public static IHotkeySession SetHotkeyAdded(this IHotkeySession session, string hotkey, bool added, Action action = null)
	{
		if (added)
		{
			session.AddOrReplaceHotkey(hotkey, action);
		}
		else
		{
			session.RemoveHotkey(hotkey);
		}
		return session;
	}

	public static IHotkeySession AddOrReplaceHotkeys(this IHotkeySession session, params string[] hotkeys)
	{
		return session.AddOrReplaceHotkeys((IEnumerable<string>)hotkeys);
	}

	public static IHotkeySession AddOrReplaceHotkeys(this IHotkeySession session, params (string key, Action action)[] hotkeys)
	{
		return session.AddOrReplaceHotkeys((IEnumerable<(string key, Action action)>)hotkeys);
	}

	public static IHotkeySession AddOrReplaceHotkeys(this IHotkeySession session, IEnumerable<string> hotkeys)
	{
		foreach (string hotkey in hotkeys)
		{
			session.AddOrReplaceHotkey(hotkey, null);
		}
		return session;
	}

	public static IHotkeySession AddOrReplaceHotkeys(this IHotkeySession session, IEnumerable<(string key, Action action)> hotkeys)
	{
		foreach (var hotkey in hotkeys)
		{
			session.AddOrReplaceHotkey(hotkey.key, hotkey.action);
		}
		return session;
	}

	public static IHotkeySession RemoveHotkeys(this IHotkeySession session, params string[] hotkeys)
	{
		return session.RemoveHotkeys((IEnumerable<string>)hotkeys);
	}

	public static IHotkeySession RemoveHotkeys(this IHotkeySession session, IEnumerable<string> hotkeys)
	{
		foreach (string hotkey in hotkeys)
		{
			session.RemoveHotkey(hotkey);
		}
		return session;
	}
}
