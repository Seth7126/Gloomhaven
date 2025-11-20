using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.ScenarioStates;

public class CheckForInitiativeAdjustmentsState : ScenarioState
{
	public override ScenarioStateTag StateTag => ScenarioStateTag.CheckForInitiativeAdjustments;

	public CheckForInitiativeAdjustmentsState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}
}
