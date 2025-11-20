using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.PopupStates;

public class RetirementConfirmationState : PopupState
{
	public override PopupStateTag StateTag => PopupStateTag.RetirementConfirmation;

	protected override string RootName => "RetirementConfirmation";

	public RetirementConfirmationState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}
}
