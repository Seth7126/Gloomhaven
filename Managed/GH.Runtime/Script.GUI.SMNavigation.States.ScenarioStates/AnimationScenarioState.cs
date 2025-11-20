using Code.State;
using MapRuleLibrary.Adventure;
using SM.Gamepad;
using Script.GUI.GameScreen;

namespace Script.GUI.SMNavigation.States.ScenarioStates;

public class AnimationScenarioState : ScenarioState
{
	private bool _wasPressed;

	public override ScenarioStateTag StateTag => ScenarioStateTag.Animation;

	public AnimationScenarioState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		if (AdventureState.MapState != null && !AdventureState.MapState.IsPlayingTutorial && !SaveData.Instance.Global.SpeedUpToggle)
		{
			if (CameraController.s_CameraController.CameraFollowOn)
			{
				CameraController.s_CameraController.RequestDisableCameraInput(this);
			}
			WorldspaceStarHexDisplay.Instance.Hide(this);
			Singleton<FastForwardButton>.Instance.Toggle(active: true);
			InputManager.RegisterToOnPressed(KeyAction.SKIP_ATTACK, OnPressed);
			InputManager.RegisterToOnReleased(KeyAction.SKIP_ATTACK, OnReleased);
		}
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		WorldspaceStarHexDisplay.Instance.CancelHide(this);
		CameraController.s_CameraController.FreeDisableCameraInput(this);
		Singleton<FastForwardButton>.Instance.Toggle(active: false);
		if (_wasPressed)
		{
			OnReleased();
		}
		InputManager.UnregisterToOnPressed(KeyAction.SKIP_ATTACK, OnPressed);
		InputManager.UnregisterToOnReleased(KeyAction.SKIP_ATTACK, OnReleased);
	}

	private void OnPressed()
	{
		_wasPressed = true;
		SaveData.Instance.Global.SpeedUpToggle = true;
		SaveData.Instance.Global.StartSpeedUp();
	}

	private void OnReleased()
	{
		_wasPressed = false;
		SaveData.Instance.Global.SpeedUpToggle = false;
		SaveData.Instance.Global.StopSpeedUp();
	}
}
