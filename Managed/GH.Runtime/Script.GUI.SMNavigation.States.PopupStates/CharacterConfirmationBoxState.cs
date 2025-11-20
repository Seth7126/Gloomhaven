using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.PopupStates;

public class CharacterConfirmationBoxState : PopupState
{
	public override PopupStateTag StateTag => PopupStateTag.CharacterConfirmationBox;

	public CharacterConfirmationBoxState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		DeselectAll();
	}
}
