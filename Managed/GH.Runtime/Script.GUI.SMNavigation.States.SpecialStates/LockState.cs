using Code.State;

namespace Script.GUI.SMNavigation.States.SpecialStates;

public class LockState : NavigationState<SpecialStateTag>
{
	public override SpecialStateTag StateTag => SpecialStateTag.Lock;

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		Singleton<UINavigation>.Instance.NavigationManager.DeselectAll();
		InputManager.RequestDisableInput(this, EKeyActionTag.All);
		if (Singleton<UILevelUpWindow>.Instance != null && Singleton<UILevelUpWindow>.Instance.IsLevelingUp())
		{
			Singleton<UILevelUpWindow>.Instance.Close();
		}
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		Singleton<UINavigation>.Instance.StateMachine.RemovePreviousState();
		InputManager.RequestEnableInput(this, EKeyActionTag.All);
	}
}
