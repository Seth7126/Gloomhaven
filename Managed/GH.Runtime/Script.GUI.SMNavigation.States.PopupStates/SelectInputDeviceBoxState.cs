using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.PopupStates;

public class SelectInputDeviceBoxState : PopupState
{
	public override PopupStateTag StateTag => PopupStateTag.SelectInputDeviceBox;

	protected override string RootName => "SelectInputDeviceBox";

	public SelectInputDeviceBoxState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}
}
