using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.PopupStates;

public abstract class PopupState : NavigationState<PopupStateTag>
{
	private readonly UiNavigationManager _navigationManager;

	protected virtual string RootName => string.Empty;

	protected PopupState(UiNavigationManager navigationManager)
	{
		_navigationManager = navigationManager;
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		if (payload is MainStateData data)
		{
			NavigationWithMainStateData(data);
		}
		else
		{
			DefaultNavigation();
		}
	}

	private void NavigationWithMainStateData(MainStateData data)
	{
		_navigationManager.SetCurrentRoot(data.NavigationRoot);
	}

	private void DefaultNavigation()
	{
		_navigationManager.SetCurrentRoot(RootName);
	}

	protected void DeselectAll()
	{
		_navigationManager.DeselectAll();
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
	}
}
