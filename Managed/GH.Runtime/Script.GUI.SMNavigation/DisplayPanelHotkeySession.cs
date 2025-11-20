using SM.Gamepad;
using Script.LogicalOperations;

namespace Script.GUI.SMNavigation;

public class DisplayPanelHotkeySession : ConditionalSession
{
	private readonly string _hotkey;

	private IHotkeySession _hotkeySession;

	private readonly SessionHotkey _sessionHotkeyPresenter;

	public DisplayPanelHotkeySession(string hotkey, IHotkeyContainer hotkeyContainer, ICondition condition)
		: base(condition)
	{
		_hotkey = hotkey;
		_hotkeySession = hotkeyContainer.GetSessionOrEmpty();
		_sessionHotkeyPresenter = _hotkeySession.GetHotkey(_hotkey);
	}

	public override void Enter()
	{
		base.Enter();
		DisplayPanelHotkey(base.ConditionValue);
	}

	public override void Exit()
	{
		base.Exit();
		_hotkeySession?.Dispose();
	}

	public override void OnConditionValueChanged(bool value)
	{
		DisplayPanelHotkey(value);
	}

	private void DisplayPanelHotkey(bool value)
	{
		_sessionHotkeyPresenter.SetShown(value);
	}
}
