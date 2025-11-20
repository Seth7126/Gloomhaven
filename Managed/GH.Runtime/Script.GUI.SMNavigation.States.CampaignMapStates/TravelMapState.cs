using Code.State;
using SM.Gamepad;
using Script.GUI.SMNavigation.States.MainMenuStates;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public abstract class TravelMapState : CampaignMapState
{
	protected override bool SelectedFirst => true;

	protected abstract bool NeedPartyUI { get; }

	protected abstract bool NeedQuestPopup { get; }

	protected TravelMapState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		Hotkeys.Instance.SetState(Hotkeys.HotkeyPositionState.WorldMap);
		InputManager.RegisterToOnPressed(KeyAction.UI_SUBMIT, TravelLongConfirm);
		InputManager.RegisterToOnPressed(KeyAction.UI_CANCEL, CancelTravel);
		FirstOrCurrentNavigation();
		if (NeedQuestPopup)
		{
			Singleton<UIQuestPopupManager>.Instance.TryFocusQuest();
		}
		if (!NeedPartyUI)
		{
			NewPartyDisplayUI.PartyDisplay.TabInput.UnRegister();
		}
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		if (Singleton<UINavigation>.Instance.StateMachine.CurrentState != Singleton<UINavigation>.Instance.StateMachine.GetState(MainStateTag.GamepadDisconnectionBox))
		{
			UnregisterCancelTravel();
			UnregisterTravel();
			if (NeedQuestPopup)
			{
				Singleton<UIQuestPopupManager>.Instance.TryUnfocusQuest();
			}
			NewPartyDisplayUI.PartyDisplay?.CloseTooltip();
		}
	}

	private void TravelLongConfirm()
	{
		Singleton<AdventureMapUIManager>.Instance.OnConfirmHotkeyPressed(delegate
		{
			Singleton<AdventureMapUIManager>.Instance.ConfirmTravel();
		});
	}

	private void CancelTravel()
	{
		Singleton<AdventureMapUIManager>.Instance.OnCancelHotkeyPressed(delegate
		{
			Singleton<AdventureMapUIManager>.Instance.OnCancelTravelButtonClick();
		});
	}

	private void UnregisterCancelTravel()
	{
		if (Singleton<AdventureMapUIManager>.Instance != null)
		{
			InputManager.UnregisterToOnPressed(KeyAction.UI_CANCEL, CancelTravel);
		}
	}

	private void UnregisterTravel()
	{
		if (Singleton<AdventureMapUIManager>.Instance != null)
		{
			InputManager.UnregisterToOnPressed(KeyAction.UI_SUBMIT, TravelLongConfirm);
		}
	}
}
