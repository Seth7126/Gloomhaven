using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.PopupStates;

public class ErrorMessageState : PopupState
{
	public override PopupStateTag StateTag => PopupStateTag.ErrorMessage;

	public ErrorMessageState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		DeselectAll();
	}
}
