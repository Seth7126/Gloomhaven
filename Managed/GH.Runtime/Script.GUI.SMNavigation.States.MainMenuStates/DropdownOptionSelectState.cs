using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class DropdownOptionSelectState : MainMenuState
{
	private IHotkeySession _hotkeySession;

	protected override bool SelectedFirst => false;

	public override MainStateTag StateTag => MainStateTag.DropdownOptionSelect;

	protected override string RootName => null;

	public DropdownOptionSelectState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		base.Enter(stateProvider, payload);
		_hotkeySession = Singleton<UIOptionsWindow>.Instance.TabHotkeys.GetSessionOrEmpty().AddOrReplaceHotkeys("Select");
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		base.Exit(stateProvider, payload);
		_hotkeySession.Dispose();
	}
}
