using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.ScenarioStates;

public class EnemyActionScenarioState : ScenarioState
{
	private bool _selectButtonClicked;

	public override ScenarioStateTag StateTag => ScenarioStateTag.EnemyAction;

	public EnemyActionScenarioState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		_selectButtonClicked = false;
		InitiativeTrack.Instance.DisplayControllerTips(doShow: false);
		InputManager.RequestDisableInput(this, KeyAction.HIGHLIGHT);
		if (!FFSNetwork.IsOnline || !FFSNetwork.IsClient)
		{
			if (Choreographer.s_Choreographer.ClientMonsterObjects.Count > 0)
			{
				InputManager.RegisterToOnPressed(KeyAction.UI_SUBMIT, SelectButtonClick);
			}
			Singleton<CombatLogHandler>.Instance.ToggleHotkey(activity: false);
			WorldspaceStarHexDisplay.Instance.Hide(this);
		}
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		InputManager.RequestEnableInput(this, KeyAction.HIGHLIGHT);
		if (!FFSNetwork.IsOnline || !FFSNetwork.IsClient)
		{
			InputManager.UnregisterToOnPressed(KeyAction.UI_SUBMIT, SelectButtonClick);
			Singleton<CombatLogHandler>.Instance.ToggleHotkey(activity: true);
			WorldspaceStarHexDisplay.Instance.CancelHide(this);
		}
	}

	private void SelectButtonClick()
	{
		if (!_selectButtonClicked)
		{
			Choreographer.s_Choreographer.m_selectButton.PlayAnimation(ReadyButtonClick);
			_selectButtonClicked = true;
		}
	}

	private void ReadyButtonClick()
	{
		Choreographer.s_Choreographer.readyButton.OnClickInternal();
	}
}
