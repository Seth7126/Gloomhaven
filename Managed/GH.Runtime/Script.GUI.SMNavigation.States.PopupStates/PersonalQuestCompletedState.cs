using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.PopupStates;

public class PersonalQuestCompletedState : PopupState
{
	public override PopupStateTag StateTag => PopupStateTag.PersonalQuestCompleted;

	protected override string RootName => "CompletedPQ";

	public PersonalQuestCompletedState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}
}
