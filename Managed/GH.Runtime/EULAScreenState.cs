using Code.State;
using Script.GUI.SMNavigation.States.MainMenuStates;

public class EULAScreenState : MainMenuState
{
	public override MainStateTag StateTag => MainStateTag.EULAScreen;

	protected override bool SelectedFirst => true;

	protected override string RootName => "";

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
	}
}
