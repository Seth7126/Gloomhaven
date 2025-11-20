using System;

namespace SM.Gamepad;

public struct SessionHotkey : IDisposable
{
	private readonly string _hotkey;

	private readonly Action _action;

	private IHotkeySession _session;

	public bool IsShown => Session.ContainsHotkey(_hotkey);

	public bool IsValid => Session != null;

	public IHotkeySession Session => _session;

	public SessionHotkey(string hotkey, Action action, IHotkeySession session)
	{
		_hotkey = hotkey;
		_action = action;
		_session = session;
	}

	public void Dispose()
	{
		ResetSession();
	}

	public void ChangeSession(IHotkeySession session)
	{
		bool shown = Session != null && IsShown;
		ResetSession();
		_session = session;
		SetShown(shown);
	}

	public SessionHotkey Show()
	{
		SetShown(shown: true);
		return this;
	}

	public SessionHotkey Hide()
	{
		SetShown(shown: false);
		return this;
	}

	public SessionHotkey SetShown(bool shown)
	{
		Session.SetHotkeyAdded(_hotkey, shown, _action);
		return this;
	}

	public bool TryShow()
	{
		return TrySetShown(shown: true);
	}

	public bool TryHide()
	{
		return TrySetShown(shown: false);
	}

	public bool TrySetShown(bool shown)
	{
		if (IsValid)
		{
			Session.SetHotkeyAdded(_hotkey, shown, _action);
		}
		return IsValid;
	}

	private void ResetSession()
	{
		IHotkeySession session = Session;
		if (session != null && !session.IsDispose)
		{
			Hide();
			_session = null;
		}
	}
}
