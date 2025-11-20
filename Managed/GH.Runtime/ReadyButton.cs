#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI.GameScreen;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.ScenarioStates;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReadyButton : ButtonOnBlockingPanel
{
	public enum EButtonState
	{
		EREADYBUTTONENDSELECTION,
		EREADYBUTTONENDROUND,
		EREADYBUTTONENDTURN,
		EREADYBUTTONPASS,
		EREADYBUTTONCONTINUE,
		EREADYBUTTONLONGREST,
		EREADYBUTTONCONFIRMTARGETS,
		EREADYBUTTONFINISHTARGETSELECT,
		EREADYBUTTONCONFIRM,
		EREADYBUTTONCONFIRMMOVEMENT,
		EREADYBUTTONOPENDOOR,
		EREADYBUTTONCONFIRMDISABLED,
		EREADYBUTTONNA,
		EREADYBUTTONRECOVERCARD,
		EREADYBUTTONIMPROVEDSHORTREST,
		EREADYBUTTONCONFIRMITEM
	}

	[SerializeField]
	private ExtendedButton readyButton;

	[SerializeField]
	private TextMeshProUGUI buttonText;

	[SerializeField]
	private Hotkey _hotkey;

	[SerializeField]
	private UIFX_MaterialFX_Control effectControl;

	[SerializeField]
	private GameObject highlight;

	[SerializeField]
	private Button warningMask;

	[SerializeField]
	private string audioItemEndTurn = "PlaySound_UIEnd_Turn";

	[SerializeField]
	private LeanTweenGUIAnimator _guiAnimator;

	private List<Action> actionsQueue = new List<Action>();

	private bool hideOnClick;

	private string originalText;

	private bool m_Interactable;

	private EButtonState buttonState = EButtonState.EREADYBUTTONNA;

	private bool showEffects;

	private bool disregardTurnControlForInteractability;

	private Action onClickWarning;

	private Coroutine m_WaitOnReadyCoroutine;

	public LongConfirmHandler LongConfirmHandler;

	private bool _shortPressed;

	private bool _networkActionIfOnline;

	public Button ButtonComponent => readyButton;

	public new bool IsInteractable => readyButton.interactable;

	public List<Action> ActionsQueue => actionsQueue;

	public bool IsInteractablePreviously => m_Interactable;

	public bool IsVisibility => canvasGroup.alpha > 0f;

	public bool Pressing { get; private set; }

	public bool ShortPressed
	{
		get
		{
			bool shortPressed = _shortPressed;
			_shortPressed = false;
			return shortPressed;
		}
		private set
		{
			_shortPressed = value;
		}
	}

	public event Action OnShortPress;

	private void Start()
	{
		canvasGroup = GetComponent<CanvasGroup>();
		originalText = buttonText.text;
		effectControl.ToggleEnable(showEffects);
		if (highlight != null)
		{
			highlight.SetActive(showEffects);
		}
		warningMask.onClick.AddListener(delegate
		{
			onClickWarning?.Invoke();
		});
		HideWarning();
		if (InputManager.GamePadInUse)
		{
			_hotkey.Initialize(Singleton<UINavigation>.Instance.Input);
			ExtendedButton extendedButton = readyButton;
			extendedButton.OnAnimationCancelled = (Action)Delegate.Combine(extendedButton.OnAnimationCancelled, new Action(EnableInput));
			if (_guiAnimator != null)
			{
				_guiAnimator.OnAnimationFinished.AddListener(HandleFinishAnimation);
				_guiAnimator.OnAnimationStopped.AddListener(EnableInput);
				_guiAnimator.OnAnimationStarted.AddListener(DisableInput);
			}
		}
		else
		{
			_hotkey.gameObject.SetActive(value: false);
			LongConfirmHandler.gameObject.SetActive(value: false);
		}
		base.gameObject.SetActive(value: false);
	}

	private void OnDestroy()
	{
		warningMask.onClick.RemoveAllListeners();
		if (InputManager.GamePadInUse)
		{
			_hotkey.Deinitialize();
			ExtendedButton extendedButton = readyButton;
			extendedButton.OnAnimationCancelled = (Action)Delegate.Remove(extendedButton.OnAnimationCancelled, new Action(EnableInput));
			if (_guiAnimator != null)
			{
				_guiAnimator.OnAnimationFinished.RemoveListener(HandleFinishAnimation);
				_guiAnimator.OnAnimationStopped.RemoveListener(EnableInput);
				_guiAnimator.OnAnimationStarted.RemoveListener(DisableInput);
			}
		}
	}

	private void OnEnable()
	{
		Debug.Log("Enabled ready button");
	}

	private void Update()
	{
		if (!SceneController.Instance.GlobalErrorMessage.ShowingMessage && buttonState != EButtonState.EREADYBUTTONCONFIRMDISABLED)
		{
			CheckButtonInteractability();
		}
	}

	public void SetSendDefaultSubmitEvent(bool enable)
	{
		LongConfirmHandler.SetSendSubmitEventOnShort(enable);
	}

	public void SendSubmitPlayerActionOnShort(bool enable)
	{
		LongConfirmHandler.SetSendSubmitPlayerActionOnShort(enable);
	}

	public void OnClick(bool networkActionIfOnline = true)
	{
		_networkActionIfOnline = networkActionIfOnline;
		if (ButtonComponent.enabled && !warningMask.gameObject.activeSelf && readyButton.interactable)
		{
			if (InputManager.GamePadInUse)
			{
				ShortPressed = false;
				Pressing = true;
				LongConfirmHandler.Pressed(PlayGUIAnimation, HandleShortPressed);
			}
			else
			{
				OnClickInternal(networkActionIfOnline);
			}
		}
	}

	private void PlayGUIAnimation()
	{
		Pressing = false;
		if (_guiAnimator != null)
		{
			if (_guiAnimator.IsPlaying)
			{
				_guiAnimator.Stop();
			}
			_guiAnimator.Play();
		}
		else
		{
			OnClickInternal(_networkActionIfOnline);
		}
	}

	private void HandleFinishAnimation()
	{
		OnClickInternal(_networkActionIfOnline);
		EnableInput();
	}

	private void DisableInput()
	{
		InputManager.RequestDisableInput(this, EKeyActionTag.All);
	}

	private void EnableInput()
	{
		InputManager.RequestEnableInput(this, EKeyActionTag.All);
	}

	private void HandleShortPressed()
	{
		Pressing = false;
		this.OnShortPress?.Invoke();
		ShortPressed = true;
	}

	public void OnClickInternal(bool networkActionIfOnline = true)
	{
		if (Singleton<UIConfirmationBoxManager>.Instance.IsRequested)
		{
			return;
		}
		if (FFSNetwork.IsOnline && !buttonState.In(EButtonState.EREADYBUTTONIMPROVEDSHORTREST))
		{
			if (networkActionIfOnline)
			{
				if (FFSNetwork.IsClient && PhaseManager.CurrentPhase.Type == CPhase.PhaseType.MonsterClassesSelectAbilityCards)
				{
					return;
				}
				if (buttonState != EButtonState.EREADYBUTTONCONFIRMITEM && buttonState != EButtonState.EREADYBUTTONRECOVERCARD && !(PhaseManager.CurrentPhase is CPhasePlayerExhausted))
				{
					Synchronizer.SendGameAction(GameActionType.ConfirmAction, ActionProcessor.CurrentPhase);
				}
			}
			if (Singleton<UIResultsManager>.Instance.IsShown)
			{
				return;
			}
			ActionProcessor.SetState(ActionProcessorStateType.Halted);
		}
		EButtonState eButtonState = buttonState;
		UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.ConfirmButtonPressed));
		TileBehaviour.SetCallback(Choreographer.s_Choreographer.TileHandler);
		Choreographer.s_Choreographer.DisableTileSelection(active: true);
		Choreographer.s_Choreographer.m_SkipButton.Toggle(active: false);
		Choreographer.s_Choreographer.m_UndoButton.Toggle(active: false);
		Choreographer.s_Choreographer.SetActiveSelectButton(activate: false);
		Singleton<UIUseAugmentationsBar>.Instance.Hide();
		Singleton<UIUseItemsBar>.Instance.Hide(resetSlots: false);
		Singleton<UIActiveBonusBar>.Instance.LockToggledActiveBonuses();
		Singleton<UIActiveBonusBar>.Instance.Hide(toggle: true);
		ControllerInputAreaManager.Instance.FocusArea(EControllerInputAreaType.WorldMap);
		Action actionDelayed = null;
		Debug.Log("READY BUTTON - ON CLICK: \nActions Queue Count: " + actionsQueue.Count + " ButtonState: " + buttonState.ToString() + " Last Choreographer MessageType: " + Choreographer.s_Choreographer.LastMessage.m_Type.ToString() + " HideOnClick: " + hideOnClick);
		if (actionsQueue.Count > 0)
		{
			Action extraActions = null;
			Action queueAction = actionsQueue[0];
			if (actionsQueue.Count > 0)
			{
				actionsQueue.RemoveAt(0);
			}
			switch (eButtonState)
			{
			case EButtonState.EREADYBUTTONCONFIRMMOVEMENT:
				extraActions = delegate
				{
					Choreographer.s_Choreographer.DisableTileSelection(active: true);
				};
				break;
			case EButtonState.EREADYBUTTONOPENDOOR:
				extraActions = delegate
				{
					Choreographer.s_Choreographer.DisableTileSelection(active: true);
				};
				break;
			}
			actionDelayed = delegate
			{
				queueAction();
				extraActions?.Invoke();
				Choreographer.s_Choreographer.m_SkipButton.Toggle(active: false);
			};
		}
		else
		{
			Choreographer.s_Choreographer.m_UndoButton.gameObject.SetActive(value: false);
			if (eButtonState >= EButtonState.EREADYBUTTONCONTINUE)
			{
				Debug.Log("STEP COMPLETE " + actionsQueue.Count + " = " + buttonState.ToString() + " = " + Choreographer.s_Choreographer.LastMessage.m_Type);
				actionDelayed = delegate
				{
					ScenarioRuleClient.StepComplete();
				};
			}
			else
			{
				UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.EndTurnPressed));
				if (Choreographer.s_Choreographer.LastMessage.m_Type != CMessageData.MessageType.EndAbilityAnimSync)
				{
					actionDelayed = Choreographer.s_Choreographer.Pass;
				}
			}
			WorldspaceStarHexDisplay.Instance.SetAOELocked(locked: false);
		}
		Choreographer.s_Choreographer.m_SkipButton.Toggle(active: false);
		if (hideOnClick)
		{
			if (showEffects)
			{
				ButtonComponent.enabled = false;
				UIManager.Instance.RequestToggleLockUI(active: true, base.gameObject);
				effectControl.ButtonFXActive(delegate
				{
					HideDelayed(actionDelayed);
				});
			}
			else
			{
				Debug.Log("READY BUTTON - invoked delayed action");
				actionDelayed?.Invoke();
				ToggleVisibility(active: false);
			}
		}
		else
		{
			if (showEffects)
			{
				effectControl.ButtonFXActive();
			}
			Debug.Log("READY BUTTON - invoked delayed action");
			actionDelayed?.Invoke();
		}
	}

	private void HideDelayed(Action afterHideAction)
	{
		try
		{
			ButtonComponent.enabled = true;
			UIManager.Instance.RequestToggleLockUI(active: false, base.gameObject);
			afterHideAction?.Invoke();
			ToggleVisibility(active: false);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the ReadyButton.HideDelayed().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00050", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void QueueAlternativeAction(Action extraAction)
	{
		Debug.Log("QUEUE ALTERNATIVE ACTION");
		actionsQueue.Add(extraAction);
	}

	public void ResetAlternativeAction(string text, EButtonState state)
	{
		Debug.Log("RESET ALTERNATIVE ACTION " + state.ToString() + " = " + ((text == null) ? "" : text));
		actionsQueue.Clear();
		SetButtonText(text);
		SetInteractable(interactable: true);
		buttonState = state;
		readyButton.mouseDownAudioItem = ((state == EButtonState.EREADYBUTTONENDTURN) ? audioItemEndTurn : null);
		RefreshInteractableStyle();
	}

	private void RefreshInteractableStyle()
	{
		buttonText.color = (readyButton.interactable ? UIInfoTools.Instance.basicTextColor : UIInfoTools.Instance.greyedOutTextColor);
		if (InputManager.GamePadInUse)
		{
			_hotkey.DisplayHotkey(readyButton.interactable);
			_hotkey.UpdateHotkeyIcon();
		}
		RefreshHighlight();
	}

	private void RefreshHighlight()
	{
		if (!(highlight == null))
		{
			highlight.SetActive(showEffects && readyButton.interactable && readyButton.gameObject.activeSelf && !warningMask.gameObject.activeSelf);
		}
	}

	public void ClearAlternativeAction()
	{
		actionsQueue.Clear();
	}

	public void AlternativeAction(Action extraAction, EButtonState state = EButtonState.EREADYBUTTONNA, string text = null, bool active = true)
	{
		Debug.Log("ALTERNATIVE ACTION " + state.ToString() + " = " + ((text == null) ? "" : text));
		actionsQueue.Clear();
		actionsQueue.Add(extraAction);
		if (!string.IsNullOrEmpty(text))
		{
			SetButtonText(text);
		}
		if (state != EButtonState.EREADYBUTTONNA)
		{
			buttonState = state;
		}
		SetInteractable(state != EButtonState.EREADYBUTTONCONFIRMDISABLED && active);
		readyButton.mouseDownAudioItem = ((state == EButtonState.EREADYBUTTONENDTURN) ? audioItemEndTurn : null);
	}

	public void SetButtonText(string text)
	{
		buttonText.text = text;
	}

	public void Toggle(bool active, EButtonState state = EButtonState.EREADYBUTTONNA, string text = null, bool hideOnClick = true, bool glowingEffect = false, bool interactable = true, bool disregardTurnControlForInteractability = false, bool haltActionProcessorIfDeactivated = true, bool hideSelectionOnEndTurn = true)
	{
		Debug.Log("TOGGLE " + actionsQueue.Count + " = " + active + " = " + state.ToString() + " = " + ((text == null) ? "" : text) + " = " + hideOnClick);
		if (FFSNetwork.IsOnline && !active && haltActionProcessorIfDeactivated && ActionProcessor.CurrentPhase != ActionPhaseType.ScenarioEnded)
		{
			ActionProcessor.SetState(ActionProcessorStateType.Halted);
		}
		this.hideOnClick = hideOnClick;
		ToggleVisibility(active);
		if (!string.IsNullOrEmpty(text))
		{
			SetButtonText(text);
		}
		if (state != EButtonState.EREADYBUTTONNA)
		{
			buttonState = state;
			showEffects = glowingEffect;
		}
		SetInteractable(active && interactable && state != EButtonState.EREADYBUTTONCONFIRMDISABLED, disregardTurnControlForInteractability);
		if (hideSelectionOnEndTurn && state == EButtonState.EREADYBUTTONENDTURN && Choreographer.s_Choreographer != null)
		{
			Choreographer.s_Choreographer.HideSelectionUI();
		}
		readyButton.mouseDownAudioItem = ((state == EButtonState.EREADYBUTTONENDTURN) ? audioItemEndTurn : null);
		if (active)
		{
			effectControl.ToggleEnable(showEffects);
			RefreshHighlight();
			if (showEffects)
			{
				effectControl.ButtonFXEndTurn();
			}
		}
		HideWarning();
	}

	public void SetInteractable(bool interactable, bool disregardTurnControl = false)
	{
		disregardTurnControlForInteractability = disregardTurnControl;
		bool interactable2 = m_Interactable;
		m_Interactable = interactable;
		CheckButtonInteractability();
		if (InputManager.GamePadInUse)
		{
			ToggleVisibility(interactable);
		}
		if (m_Interactable != interactable2)
		{
			FFSNet.Console.LogInfo("SP Ready Button set to " + (m_Interactable ? " INTERACTABLE. " : " UNINTERACTABLE."));
		}
	}

	private void CheckButtonInteractability()
	{
		GameObject currentActorGameObject = ((Choreographer.s_Choreographer.m_CurrentActor != null) ? Choreographer.s_Choreographer.FindClientActorGameObject(Choreographer.s_Choreographer.m_CurrentActor) : null);
		bool flag = !InputManager.GamePadInUse || !Singleton<UINavigation>.Instance.StateMachine.IsCurrentState<AnimationScenarioState>();
		bool flag2 = !FFSNetwork.IsOnline || buttonState.In(EButtonState.EREADYBUTTONIMPROVEDSHORTREST) || !FFSNetwork.IsClient || PhaseManager.CurrentPhase.Type != CPhase.PhaseType.MonsterClassesSelectAbilityCards;
		readyButton.interactable = m_Interactable && flag2 && (Choreographer.s_Choreographer.ThisPlayerHasTurnControl || disregardTurnControlForInteractability) && (!ScenarioRuleClient.IsProcessingOrMessagesQueued || GameState.WaitingForPlayerToSelectDamageResponse || GameState.WaitingForPlayerActorToAvoidDamageResponse) && (Choreographer.s_Choreographer.m_MessageQueue.Count <= 0 || AutoTestController.s_AutoTestCurrentlyLoaded) && (Choreographer.s_Choreographer.m_CurrentActor == null || currentActorGameObject == null || (Choreographer.s_Choreographer.IdleStates.Any((string x) => MF.GameObjectAnimatorControllerIsCurrentState(currentActorGameObject, x)) && flag));
		ChangeCanvasAlpha(readyButton.interactable && canvasGroup.interactable);
		RefreshInteractableStyle();
		RefreshHighlight();
	}

	public void ShowWarning(Action onClickWarningCallback = null)
	{
		onClickWarning = onClickWarningCallback;
		warningMask.gameObject.SetActive(value: true);
		RefreshHighlight();
	}

	public void HideWarning()
	{
		onClickWarning = null;
		warningMask.gameObject.SetActive(value: false);
		RefreshHighlight();
	}

	private void OnDisable()
	{
		HideWarning();
	}
}
