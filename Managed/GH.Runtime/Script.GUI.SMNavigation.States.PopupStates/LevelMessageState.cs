using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.PopupStates;

public class LevelMessageState : PopupState
{
	private readonly KeyAction[] _actionsToDisable = new KeyAction[8]
	{
		KeyAction.UI_PREVIOUS_TAB,
		KeyAction.UI_NEXT_TAB,
		KeyAction.NEXT_MERCENARY_OPTION,
		KeyAction.UI_NEXT_TAB_MERCENARY,
		KeyAction.NEXT_SHIELD_TAB,
		KeyAction.PREVIOUS_SHIELD_TAB,
		KeyAction.CONTROL_LOCAL_OPTIONS_LEFT,
		KeyAction.CONTROL_LOCAL_OPTIONS_RIGHT
	};

	public override PopupStateTag StateTag => PopupStateTag.LevelMessage;

	protected override string RootName => "LevelMessage";

	public LevelMessageState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		InputManager.RequestDisableInput(this, _actionsToDisable);
		if (Singleton<UIGuildmasterHUD>.IsInitialized)
		{
			Singleton<UIGuildmasterHUD>.Instance.EnableShieldInput(active: false);
		}
		CameraController.s_CameraController?.RequestDisableCameraInput(this);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		InputManager.RequestEnableInput(this, _actionsToDisable);
		if (Singleton<UIGuildmasterHUD>.IsInitialized)
		{
			Singleton<UIGuildmasterHUD>.Instance.EnableShieldInput(active: true);
		}
		CameraController.s_CameraController?.FreeDisableCameraInput(this);
	}
}
