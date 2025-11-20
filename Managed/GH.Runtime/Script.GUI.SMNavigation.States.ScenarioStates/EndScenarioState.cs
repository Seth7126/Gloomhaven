using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.ScenarioStates;

public class EndScenarioState : ScenarioState
{
	public override ScenarioStateTag StateTag => ScenarioStateTag.End;

	public EndScenarioState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}
}
