using System.Collections.Generic;
using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public abstract class CampaignMapState : NavigationState<CampaignMapStateTag>
{
	protected readonly UiNavigationManager _navigationManager;

	protected abstract bool SelectedFirst { get; }

	protected abstract string RootName { get; }

	protected CampaignMapState(UiNavigationManager navigationManager)
	{
		_navigationManager = navigationManager;
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		FirstOrCurrentNavigation();
	}

	protected void DefaultNavigation()
	{
		_navigationManager.SetCurrentRoot(RootName, SelectedFirst);
	}

	protected void FirstOrCurrentNavigation()
	{
		_navigationManager.SetCurrentRoot(RootName, selectFirst: false);
	}

	protected void FirstNavigation()
	{
		_navigationManager.SetCurrentRoot(RootName);
	}

	protected void LastNavigation()
	{
		IUiNavigationRoot uiNavigationRoot = _navigationManager.RootByName(RootName);
		if (uiNavigationRoot == null)
		{
			return;
		}
		List<IUiNavigationElement> elements = uiNavigationRoot.Elements;
		IUiNavigationSelectable selectConcrete = null;
		for (int num = elements.Count - 1; num >= 0; num--)
		{
			IUiNavigationElement uiNavigationElement = elements[num];
			if (uiNavigationElement.GameObject.activeInHierarchy && uiNavigationElement is IUiNavigationSelectable uiNavigationSelectable && uiNavigationSelectable.ControlledSelectable.interactable)
			{
				selectConcrete = uiNavigationSelectable;
				break;
			}
		}
		_navigationManager.SetCurrentRoot(RootName, selectFirst: false, selectConcrete);
	}

	protected void ConcreteNavigation(IUiNavigationSelectable selectable)
	{
		_navigationManager.SetCurrentRoot(RootName, selectFirst: false, selectable);
	}

	protected void NavigationWithSelectionStateData(SelectionStateData data)
	{
		if (data.SelectPrevious)
		{
			FirstOrCurrentNavigation();
		}
		else if (data.SelectFirst)
		{
			FirstNavigation();
		}
		else if (data.ConcreteSelectable != null)
		{
			ConcreteNavigation(data.ConcreteSelectable);
		}
	}

	protected void SetActiveReturnToMap(bool value)
	{
		if (value)
		{
			InputManager.RegisterToOnPressed(KeyAction.UI_CANCEL, ReturnToMap);
		}
		else
		{
			InputManager.UnregisterToOnPressed(KeyAction.UI_CANCEL, ReturnToMap);
		}
		static void ReturnToMap()
		{
			Singleton<UIGuildmasterHUD>.Instance.UpdateCurrentMode(EGuildmasterMode.WorldMap);
		}
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
	}
}
