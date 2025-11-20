using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.ScenarioStates;

public class SelectHotkeys
{
	private SessionHotkey _selectHotkey;

	private SessionHotkey _unselectHotkey;

	private IHotkeySession _hotkeySession;

	public void Enter(IHotkeySession session)
	{
		_hotkeySession = session;
		_selectHotkey = _hotkeySession.GetHotkey("Select");
		_unselectHotkey = _hotkeySession.GetHotkey("Unselect");
	}

	public void SetSelect(bool canSelect)
	{
		_selectHotkey.SetShown(canSelect);
		_unselectHotkey.SetShown(!canSelect);
	}

	public void SetShown(bool canSelect, bool canUnselect)
	{
		_selectHotkey.SetShown(canSelect);
		_unselectHotkey.SetShown(canUnselect);
	}

	public void Hide()
	{
		_selectHotkey.SetShown(shown: false);
		_unselectHotkey.SetShown(shown: false);
	}
}
