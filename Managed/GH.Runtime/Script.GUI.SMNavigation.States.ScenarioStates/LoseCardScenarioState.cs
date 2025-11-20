using Code.State;
using SM.Gamepad;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;

namespace Script.GUI.SMNavigation.States.ScenarioStates;

public class LoseCardScenarioState : ScenarioState
{
	private IHotkeySession _hotkeySession;

	private CardSelectHotkeys _cardSelectHotkeys = new CardSelectHotkeys();

	public override ScenarioStateTag StateTag => ScenarioStateTag.LoseCard;

	public LoseCardScenarioState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		_hotkeySession = Hotkeys.Instance.GetSession().AddOrReplaceHotkeys("Highlight");
		_cardSelectHotkeys.Enter(_hotkeySession);
		CameraController.s_CameraController?.RequestDisableCameraInput(this);
		UiNavigationManager navigationManager = Singleton<UINavigation>.Instance.NavigationManager;
		navigationManager.SetCurrentRoot("CardHands");
		IUiNavigationSelectable uiNavigationSelectable = UiNavigationUtils.FindFirstSelectableFiltered(navigationManager.CurrentNavigationRoot.Elements, Filter);
		navigationManager.TrySelect(uiNavigationSelectable);
		static bool Filter(IUiNavigationElement navigationElement)
		{
			if (!navigationElement.GameObject.activeInHierarchy)
			{
				return false;
			}
			if (navigationElement is IUiNavigationSelectable uiNavigationSelectable2)
			{
				return uiNavigationSelectable2.ControlledSelectable.IsInteractable();
			}
			if (navigationElement is IUiNavigationNode uiNavigationNode)
			{
				if (uiNavigationNode.Elements.Count > 0 && uiNavigationNode.Elements[0] is IUiNavigationSelectable uiNavigationSelectable3)
				{
					return uiNavigationSelectable3.ControlledSelectable.IsInteractable();
				}
				return false;
			}
			return false;
		}
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		Singleton<UINavigation>.Instance.NavigationManager.DeselectAll();
		CameraController.s_CameraController?.FreeDisableCameraInput(this);
		_cardSelectHotkeys.Exit();
		_hotkeySession.Dispose();
		if (CardsHandManager.Instance != null && CardsHandManager.Instance.CurrentHand != null)
		{
			CardsHandManager.Instance.OnControllerAreaUnfocused();
		}
		Singleton<InputManager>.Instance.PlayerControl.UIPreviousTab.OnReleased -= OnReleased;
		Singleton<InputManager>.Instance.PlayerControl.UINextTab.OnReleased -= OnReleased;
	}

	private void OnReleased()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.CheckOutRoundCards);
	}
}
