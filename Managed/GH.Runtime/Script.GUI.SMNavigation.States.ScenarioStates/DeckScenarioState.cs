using Code.State;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;

namespace Script.GUI.SMNavigation.States.ScenarioStates;

public class DeckScenarioState : ScenarioState
{
	private IHotkeySession _hotkeySession;

	private SessionHotkey _selectHotkey;

	private SessionHotkey _unselectHotkey;

	private bool _isSelectCardPhase;

	private CardSelectHotkeys _selectHotkeys = new CardSelectHotkeys();

	public override ScenarioStateTag StateTag => ScenarioStateTag.Deck;

	public DeckScenarioState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		_hotkeySession = Hotkeys.Instance.GetSession();
		_hotkeySession.AddOrReplaceHotkeys(("Back", null), ("Tips", ToggleTooltip));
		if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest && CardsHandManager.Instance.m_PushPopCardHandMode == CardHandMode.CardsSelection)
		{
			_selectHotkeys.Enter(_hotkeySession);
			_isSelectCardPhase = true;
		}
		if (CardsHandManager.Instance.CurrentHand != null)
		{
			CardsHandManager.Instance.OnControllerAreaUnfocused();
		}
		if (Singleton<CardsHandPreviewWindow>.Instance != null)
		{
			Singleton<CardsHandPreviewWindow>.Instance.EnableInput();
		}
		CameraController.s_CameraController?.RequestDisableCameraInput(this);
		if (!FFSNetwork.IsOnline)
		{
			InputManager.RequestDisableInput(this, KeyAction.CONFIRM_ACTION_BUTTON);
		}
		else
		{
			Singleton<UIReadyToggle>.Instance.SetSendDefaultSubmitEvent(enable: true);
		}
		Singleton<ActorStatPanel>.Instance.HideTemporary(hide: true);
		Singleton<UITextInfoPanel>.Instance.HideTemporary(hide: true);
		InteractabilityHighlightCanvas.s_Instance.ToggleHighlights(isActive: false);
		InitiativeTrack.Instance.DisplayControllerTips(doShow: false);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		if (_isSelectCardPhase)
		{
			_selectHotkeys.Exit();
			_isSelectCardPhase = false;
		}
		_hotkeySession.Dispose();
		_hotkeySession = null;
		if (Singleton<CardsHandPreviewWindow>.Instance != null)
		{
			Singleton<CardsHandPreviewWindow>.Instance.DisableInput();
		}
		CameraController.s_CameraController?.FreeDisableCameraInput(this);
		if (!FFSNetwork.IsOnline)
		{
			InputManager.RequestEnableInput(this, KeyAction.CONFIRM_ACTION_BUTTON);
		}
		else
		{
			Singleton<UIReadyToggle>.Instance.SetSendDefaultSubmitEvent(enable: false);
		}
		Singleton<ActorStatPanel>.Instance.HideTemporary(hide: false);
		Singleton<UITextInfoPanel>.Instance.HideTemporary(hide: false);
		InteractabilityHighlightCanvas.s_Instance.ToggleHighlights(isActive: true);
		TooltipsVisibilityHelper.Instance.RemoveTooltipRequest(this);
	}

	public void ToggleTooltip()
	{
		TooltipsVisibilityHelper.Instance.ToggleTooltips(this);
	}
}
