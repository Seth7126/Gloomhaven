using System.Linq;
using Code.State;
using SM.Gamepad;
using Script.GUI.SMNavigation.States.ScenarioStates;

public class AbilityCardPickerState : ScenarioState
{
	private readonly string _rootName = "AbilityCardPicker";

	public override ScenarioStateTag StateTag => ScenarioStateTag.AbilityCardPicker;

	public AbilityCardPickerState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		CameraController.s_CameraController.RequestDisableCameraInput(this);
		IUiNavigationRoot uiNavigationRoot = _navigationManager.RootByName(_rootName);
		_navigationManager.SetCurrentRoot(uiNavigationRoot, selectFirst: false, (IUiNavigationSelectable)uiNavigationRoot.Elements.FirstOrDefault((IUiNavigationElement x) => x is IUiNavigationSelectable));
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		CameraController.s_CameraController.FreeDisableCameraInput(this);
	}
}
