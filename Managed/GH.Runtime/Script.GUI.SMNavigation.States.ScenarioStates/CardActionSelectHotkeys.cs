using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.ScenarioStates;

public class CardActionSelectHotkeys
{
	private SelectHotkeys _selectHotkeys = new SelectHotkeys();

	public void Enter(IHotkeySession session)
	{
		_selectHotkeys.Enter(session);
		FullAbilityCard.FullCardHoveringStateChanged += SetSelectHotkeys;
	}

	public void Exit()
	{
		FullAbilityCard.FullCardHoveringStateChanged -= SetSelectHotkeys;
	}

	public void SetSelectHotkeys(bool hover)
	{
		_selectHotkeys.SetShown(hover, canUnselect: false);
	}
}
