using Code.State;
using SM.Gamepad;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;

namespace Script.GUI.SMNavigation.States.ScenarioStates;

public class ChooseCardsScenarioState : ScenarioState
{
	private IHotkeySession _hotkeySession;

	private CardSelectHotkeys _cardSelectHotkeys = new CardSelectHotkeys();

	public override ScenarioStateTag StateTag => ScenarioStateTag.ChooseCards;

	public ChooseCardsScenarioState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		bool isOpen = Singleton<TakeDamagePanel>.Instance.IsOpen;
		_hotkeySession = Hotkeys.Instance.GetSession().SetHotkeyAdded("Back", isOpen).AddOrReplaceHotkeys("AllCards");
		_cardSelectHotkeys.Enter(_hotkeySession);
		CardsHandManager.Instance.EnableAllCardsCombo(value: true);
		CardsHandManager.Instance.EnableAllDeckSelection(isEnabled: true);
		CameraController.s_CameraController.RequestDisableCameraInput(this);
		if (!(CardsHandManager.Instance.CurrentHand == null))
		{
			if (isOpen)
			{
				Singleton<InputManager>.Instance.PlayerControl.UICancel.OnPressed += ReturnToDamageState;
			}
			CardsHandUI currentHand = CardsHandManager.Instance.CurrentHand;
			currentHand.ControllerFocus();
			_cardSelectHotkeys.SetSelectHotkeys(currentHand.CurrentCard);
			InitiativeTrack.Instance.DisplayControllerTips(doShow: false);
		}
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		CardsHandManager.Instance.EnableAllCardsCombo(value: false);
		CardsHandManager.Instance.EnableAllDeckSelection(isEnabled: false);
		CameraController.s_CameraController.FreeDisableCameraInput(this);
		Singleton<InputManager>.Instance.PlayerControl.UICancel.OnPressed -= ReturnToDamageState;
		_cardSelectHotkeys.Exit();
		_hotkeySession.Dispose();
		if (CardsHandManager.Instance != null && CardsHandManager.Instance.CurrentHand != null)
		{
			CardsHandManager.Instance.CurrentHand.ControllerUnfocus();
			_navigationManager.DeselectAll();
		}
	}

	private void ReturnToDamageState()
	{
		if (!Singleton<InputManager>.Instance.PlayerControl.ActionIsHandled(Singleton<InputManager>.Instance.PlayerControl.UICancel))
		{
			Singleton<InputManager>.Instance.PlayerControl.MarkActionAsHandled(Singleton<InputManager>.Instance.PlayerControl.UICancel, "ReturnToDamageState from ChooseCardScenarioState");
			_stateMachine.Enter(ScenarioStateTag.Damage);
			Singleton<TakeDamagePanel>.Instance.UnToggleBurnAvailableCard();
			Singleton<TakeDamagePanel>.Instance.UnToggleBurnDiscardedCards();
		}
	}
}
