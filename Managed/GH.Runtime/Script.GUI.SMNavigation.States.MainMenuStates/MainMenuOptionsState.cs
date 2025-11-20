using Code.State;
using GLOOM.MainMenu;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class MainMenuOptionsState : MainMenuState
{
	protected override bool SelectedFirst => false;

	public override MainStateTag StateTag => MainStateTag.MainOptions;

	protected override string RootName => "MainMenuOptions";

	public MainMenuOptionsState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		base.Enter(stateProvider, payload);
		if (Singleton<PromotionDLCManager>.Instance != null)
		{
			Singleton<PromotionDLCManager>.Instance.Show();
		}
		if (global::PlatformLayer.Instance.IsDelayedInit && MainMenuUIManager.Instance != null)
		{
			MainMenuUIManager.Instance.PrimaryUserData.Show();
		}
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		base.Exit(stateProvider, payload);
		if (Singleton<PromotionDLCManager>.Instance != null)
		{
			Singleton<PromotionDLCManager>.Instance.Hide();
		}
		if (global::PlatformLayer.Instance.IsDelayedInit && MainMenuUIManager.Instance != null)
		{
			MainMenuUIManager.Instance.PrimaryUserData.Hide();
		}
	}
}
