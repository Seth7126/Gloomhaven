#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using FFSNet;
using GLOOM;
using ScenarioRuleLibrary;
using Script.GUI.GameScreen;
using SharedLibrary.SimpleLog;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UndoButton : ButtonOnBlockingPanel, IEscapable
{
	public enum EButtonState
	{
		EUNDOBUTTONUNDO,
		EUNDOBUTTONCLEARTARGETS,
		EUNDOBUTTONUNDOWAYPOINT
	}

	private class ActionContainer
	{
		public Action m_ActionOverrider;

		public string m_PreviousText;

		public EButtonState m_PreviousButtonState;

		public bool m_PreviousEnabled;

		public bool m_PreviousInteractable;
	}

	[SerializeField]
	private ExtendedButton m_UndoButton;

	[SerializeField]
	private TextMeshProUGUI m_ButtonText;

	[SerializeField]
	private UIControllerKeyTip m_ControllerKeyTip;

	[SerializeField]
	private bool m_IsAllowedToEscapeDuringSave;

	[SerializeField]
	private LeanTweenGUIAnimator _guiAnimator;

	private Stack<ActionContainer> m_OnClickOverrides = new Stack<ActionContainer>();

	private EButtonState m_ButtonState;

	private bool m_Interactable;

	public bool IsAllowedToEscapeDuringSave => m_IsAllowedToEscapeDuringSave;

	public new bool IsInteractable => m_Interactable;

	public bool SkipNextNetworkAction { get; set; }

	public event Action OnClearTargets;

	private void Awake()
	{
		canvasGroup = GetComponent<CanvasGroup>();
		m_ButtonText.text = LocalizationManager.GetTranslation("GUI_UNDO");
		base.gameObject.SetActive(value: false);
	}

	private void OnEnable()
	{
		if (_guiAnimator != null)
		{
			_guiAnimator.OnAnimationFinished.AddListener(HandleFinishAnimation);
		}
	}

	private void Update()
	{
		CheckButtonInteractability();
	}

	private void OnDisable()
	{
		UIWindowManager.UnregisterEscapable(this);
		ClearOnClickOverriders();
		if (_guiAnimator != null)
		{
			_guiAnimator.OnAnimationFinished.RemoveListener(HandleFinishAnimation);
		}
	}

	public void OnClick(bool networkActionIfOnline = true)
	{
		if (m_UndoButton.interactable)
		{
			if (!InteractabilityManager.ShouldAllowClickForExtendedButton(m_UndoButton))
			{
				Debug.Log("Button press for button " + base.gameObject.name + " intercepted and prevented by InteractabilityManager");
				return;
			}
			AudioControllerUtils.PlaySound("PlaySound_ScenarioUIUndo");
			OnClickInternal(networkActionIfOnline);
		}
	}

	public bool OnClickInternal(bool networkActionIfOnline = true, GameAction action = null)
	{
		UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.UndoButtonPressed));
		CAbility cAbility = null;
		if (PhaseManager.Phase is CPhaseAction cPhaseAction)
		{
			cAbility = cPhaseAction.CurrentPhaseAbility?.m_Ability;
			if (cAbility != null && cAbility is CAbilityMerged { ActiveAbility: { } activeAbility })
			{
				cAbility = activeAbility;
			}
		}
		bool flag = false;
		flag = ((action == null) ? (m_ButtonState == EButtonState.EUNDOBUTTONCLEARTARGETS && (cAbility == null || !cAbility.AllTargets)) : (action.ActionTypeID == 115));
		if (FFSNetwork.IsOnline && networkActionIfOnline && !SkipNextNetworkAction)
		{
			if (flag)
			{
				Synchronizer.SendGameAction(GameActionType.ClearTargets, ActionProcessor.CurrentPhase);
			}
			else
			{
				Synchronizer.SendGameAction(GameActionType.UndoAction, ActionProcessor.CurrentPhase);
			}
		}
		if (m_OnClickOverrides.Count > 0)
		{
			ActionContainer actionContainer = m_OnClickOverrides.Pop();
			actionContainer.m_ActionOverrider();
			m_ButtonText.text = actionContainer.m_PreviousText;
			m_ButtonState = actionContainer.m_PreviousButtonState;
			HandleEmptyButtonOverrides();
			Toggle(actionContainer.m_PreviousEnabled, m_ButtonState);
			SetInteractable(actionContainer.m_PreviousInteractable);
			return true;
		}
		if (FFSNetwork.IsOnline && !Choreographer.s_Choreographer.m_CurrentActor.IsUnderMyControl && !SkipNextNetworkAction)
		{
			ActionProcessor.SetState(ActionProcessorStateType.Halted);
		}
		SkipNextNetworkAction = false;
		WorldspaceStarHexDisplay.Instance.SetAOELocked(locked: false);
		List<CActiveBonus> selectedActiveBonuses = Singleton<UIActiveBonusBar>.Instance.GetSelectedActiveBonuses();
		SetInteractable(active: false);
		Choreographer.s_Choreographer.readyButton.SetInteractable(interactable: false);
		Choreographer.s_Choreographer.m_SkipButton.SetInteractable(active: false);
		Choreographer.s_Choreographer.readyButton.ClearAlternativeAction();
		if (cAbility != null)
		{
			if (flag)
			{
				Singleton<UIUseItemsBar>.Instance.SetItemsInteractable(enable: false);
				Singleton<UIActiveBonusBar>.Instance.SetInteractionAvailableSlots(interactable: false);
				Singleton<UIUseAbilitiesBar>.Instance.Hide(clearSelection: true);
				Singleton<UIUseAugmentationsBar>.Instance.SetInteractionAvailableSlots(interactable: false);
				ScenarioRuleClient.ClearTargets();
				this.OnClearTargets?.Invoke();
			}
			else
			{
				Singleton<UIUseItemsBar>.Instance.Hide();
				Singleton<UIActiveBonusBar>.Instance.Hide();
				Singleton<UIUseAbilitiesBar>.Instance.Hide(clearSelection: true);
				Singleton<UIUseAugmentationsBar>.Instance.Hide();
				CAbility parentAbility = cAbility.ParentAbility;
				bool flag2 = parentAbility != null && parentAbility.AbilityType == CAbility.EAbilityType.ChooseAbility;
				if (!cAbility.IsItemAbility && !flag2 && GameState.CurrentActionInitiator == GameState.EActionInitiator.AbilityCard)
				{
					CardsHandManager.Instance.Undo((CPlayerActor)InitiativeTrack.Instance.SelectedActor().Actor);
				}
				ScenarioRuleClient.Undo(selectedActiveBonuses);
			}
			return true;
		}
		return false;
	}

	private void HandleEmptyButtonOverrides()
	{
		if (m_OnClickOverrides.Count <= 0)
		{
			m_ButtonText.text = LocalizationManager.GetTranslation("GUI_UNDO");
			m_ButtonState = EButtonState.EUNDOBUTTONUNDO;
			if (Choreographer.s_Choreographer.LastMessage is CActorIsSelectingMoveTile_MessageData)
			{
				CActorIsSelectingMoveTile_MessageData cActorIsSelectingMoveTile_MessageData = Choreographer.s_Choreographer.LastMessage as CActorIsSelectingMoveTile_MessageData;
				ToggleVisibility(cActorIsSelectingMoveTile_MessageData.m_MoveAbility.CanUndo);
			}
			else if (Choreographer.s_Choreographer.LastMessage is CActorIsSelectingPushTile_MessageData)
			{
				CActorIsSelectingPushTile_MessageData cActorIsSelectingPushTile_MessageData = Choreographer.s_Choreographer.LastMessage as CActorIsSelectingPushTile_MessageData;
				ToggleVisibility(cActorIsSelectingPushTile_MessageData.m_PushAbility.CanUndo);
			}
			else if (Choreographer.s_Choreographer.LastMessage is CActorIsSelectingPullTile_MessageData)
			{
				CActorIsSelectingPullTile_MessageData cActorIsSelectingPullTile_MessageData = Choreographer.s_Choreographer.LastMessage as CActorIsSelectingPullTile_MessageData;
				ToggleVisibility(cActorIsSelectingPullTile_MessageData.m_PullAbility.CanUndo);
			}
		}
	}

	public void SetOnClickOverrider(Action action, EButtonState buttonState, string text = null)
	{
		if (text != null)
		{
			m_ButtonText.text = text;
		}
		m_OnClickOverrides.Push(new ActionContainer
		{
			m_ActionOverrider = action,
			m_PreviousText = m_ButtonText.text,
			m_PreviousEnabled = m_UndoButton.gameObject.activeSelf,
			m_PreviousInteractable = m_UndoButton.interactable,
			m_PreviousButtonState = m_ButtonState
		});
		SimpleLog.AddToSimpleLog("Adding Undo override.  Overrides Count: " + m_OnClickOverrides.Count);
		Toggle(active: true, buttonState, text);
	}

	public void SetOnClickOverrider(Action action, EButtonState buttonState, bool previousEnabled, bool previousInteractable, string text = null)
	{
		if (text != null)
		{
			m_ButtonText.text = text;
		}
		m_OnClickOverrides.Push(new ActionContainer
		{
			m_ActionOverrider = action,
			m_PreviousText = m_ButtonText.text,
			m_PreviousEnabled = previousEnabled,
			m_PreviousInteractable = previousInteractable,
			m_PreviousButtonState = m_ButtonState
		});
		SimpleLog.AddToSimpleLog("Adding Undo override.  Overrides Count: " + m_OnClickOverrides.Count);
		Toggle(active: true, buttonState, text);
	}

	public void ClearOnClickOverriders()
	{
		m_OnClickOverrides.Clear();
		m_ButtonText.text = LocalizationManager.GetTranslation("GUI_UNDO");
		m_ButtonState = EButtonState.EUNDOBUTTONUNDO;
	}

	public void Toggle(bool active, EButtonState buttonState = EButtonState.EUNDOBUTTONUNDO, string text = null)
	{
		if (buttonState == EButtonState.EUNDOBUTTONCLEARTARGETS && (!(PhaseManager.Phase is CPhaseAction cPhaseAction) || (cPhaseAction.CurrentPhaseAbility != null && !cPhaseAction.CurrentPhaseAbility.m_Ability.CanClearTargets()) || (cPhaseAction.CurrentPhaseAbility != null && cPhaseAction.CurrentPhaseAbility.m_Ability.AllTargets && GameState.OverridingCurrentActor)))
		{
			return;
		}
		Debug.Log("TOGGLE UNDO = " + active + " = " + buttonState.ToString() + " = " + ((text == null) ? "" : text));
		if (GameState.OverridingCurrentActor && buttonState == EButtonState.EUNDOBUTTONUNDO && Waypoint.s_Waypoints.Count <= 0)
		{
			ToggleVisibility(active: false);
			SetInteractable(active: false);
			UIWindowManager.UnregisterEscapable(this);
			return;
		}
		if (text != null)
		{
			m_ButtonText.text = text;
		}
		m_ButtonState = buttonState;
		ToggleVisibility(active);
		SetInteractable(active);
		if (active)
		{
			UIWindowManager.RegisterEscapable(this);
		}
		else
		{
			UIWindowManager.UnregisterEscapable(this);
		}
	}

	public void SetInteractable(bool active)
	{
		active = active && Choreographer.s_Choreographer.ThisPlayerHasTurnControl;
		m_Interactable = active;
		m_ButtonText.color = (active ? UIInfoTools.Instance.basicTextColor : UIInfoTools.Instance.greyedOutTextColor);
		m_ControllerKeyTip.ShowInteractable(active);
		CheckButtonInteractability();
	}

	private void CheckButtonInteractability()
	{
		m_UndoButton.interactable = m_Interactable && (!ScenarioRuleClient.IsProcessingOrMessagesQueued || GameState.WaitingForPlayerToSelectDamageResponse || GameState.WaitingForPlayerActorToAvoidDamageResponse) && (Choreographer.s_Choreographer.m_MessageQueue.Count <= 0 || AutoTestController.s_AutoTestCurrentlyLoaded);
		ChangeCanvasAlpha(m_UndoButton.interactable && canvasGroup.interactable);
	}

	private void HandleFinishAnimation()
	{
		UIWindowManager.UnregisterEscapable(this);
		OnClick();
	}

	public bool Escape()
	{
		if ((!m_UndoButton.gameObject.activeSelf && !m_UndoButton.IsInteractable()) || !canvasGroup.interactable || !m_Interactable)
		{
			return false;
		}
		if (!InputManager.GamePadInUse)
		{
			return false;
		}
		if (_guiAnimator != null)
		{
			if (_guiAnimator.IsPlaying)
			{
				return false;
			}
			_guiAnimator.Play();
		}
		return true;
	}

	public int Order()
	{
		return 0;
	}
}
