#define ENABLE_LOGS
using System;
using System.Linq;
using FFSNet;
using GLOOM;
using ScenarioRuleLibrary;
using Script.GUI.GameScreen;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkipButton : ButtonOnBlockingPanel
{
	[SerializeField]
	private Button skipButton;

	[SerializeField]
	private TextMeshProUGUI buttonText;

	[SerializeField]
	private UIControllerKeyTip controllerKeyTip;

	[SerializeField]
	private LeanTweenGUIAnimator _guiAnimator;

	private bool hideOnClick;

	private string originalText;

	private bool m_Interactable;

	private Action onSkipAction;

	public new bool IsInteractable => m_Interactable;

	private void Awake()
	{
		canvasGroup = GetComponent<CanvasGroup>();
		if (_guiAnimator != null)
		{
			_guiAnimator.OnAnimationFinished.AddListener(HandleFinishAnimation);
		}
	}

	private void Start()
	{
		buttonText.text = LocalizationManager.GetTranslation("GUI_SKIP_MOVEMENT");
		RefreshInteractionStyle();
		base.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		CheckButtonInteractability();
	}

	private void HandleFinishAnimation()
	{
		OnClick();
	}

	private void RefreshInteractionStyle()
	{
		buttonText.color = ((m_Interactable && skipButton.interactable) ? UIInfoTools.Instance.basicTextColor : UIInfoTools.Instance.greyedOutTextColor);
		controllerKeyTip.ShowInteractable(m_Interactable && skipButton.interactable);
	}

	public void OnClickFromButton()
	{
		if (InputManager.GamePadInUse && _guiAnimator != null)
		{
			_guiAnimator.Play();
		}
		else
		{
			OnClick();
		}
	}

	public void OnClick(bool networkActionIfOnline = true)
	{
		if (networkActionIfOnline && (!skipButton.interactable || !canvasGroup.interactable || !skipButton.gameObject.activeInHierarchy))
		{
			return;
		}
		if (FFSNetwork.IsOnline)
		{
			if (skipButton.interactable && networkActionIfOnline)
			{
				if (ActionProcessor.CurrentPhase == ActionPhaseType.NONE)
				{
					Debug.LogWarning("Skip action sent while current phase was NONE.  This action will not be performed.\n" + Environment.StackTrace);
					return;
				}
				Synchronizer.SendGameAction(GameActionType.SkipAction, ActionProcessor.CurrentPhase);
			}
			ActionProcessor.SetState(ActionProcessorStateType.Halted);
		}
		UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.SkipButtonPressed));
		TileBehaviour.SetCallback(Choreographer.s_Choreographer.TileHandler);
		Choreographer.s_Choreographer.DisableTileSelection(active: true);
		Choreographer.s_Choreographer.m_UndoButton.Toggle(active: false);
		Choreographer.s_Choreographer.readyButton.Toggle(active: false);
		Choreographer.s_Choreographer.readyButton.ClearAlternativeAction();
		Singleton<UIAbilityCardPicker>.Instance.Hide();
		Singleton<UIUseItemsBar>.Instance.Hide();
		Singleton<UIUseAbilitiesBar>.Instance.Hide(clearSelection: true);
		Singleton<UIUseAugmentationsBar>.Instance.Hide();
		Singleton<UIActiveBonusBar>.Instance.LockToggledActiveBonuses();
		Singleton<UIActiveBonusBar>.Instance.Hide(toggle: true);
		WorldspaceStarHexDisplay.Instance.SetAOELocked(locked: false);
		onSkipAction?.Invoke();
		ClearSkipAction();
		if (hideOnClick)
		{
			ToggleVisibility(active: false);
		}
		CAbility currentAbility = Choreographer.s_Choreographer.m_CurrentAbility;
		if (currentAbility == null || currentAbility.AbilityType != CAbility.EAbilityType.Push)
		{
			CAbility currentAbility2 = Choreographer.s_Choreographer.m_CurrentAbility;
			if (currentAbility2 == null || currentAbility2.AbilityType != CAbility.EAbilityType.Pull)
			{
				if (Choreographer.s_Choreographer.LastMessage.m_Type != CMessageData.MessageType.EndAbilityAnimSync)
				{
					Choreographer.s_Choreographer.Pass();
				}
				goto IL_01c6;
			}
		}
		ScenarioRuleClient.PassStep();
		goto IL_01c6;
		IL_01c6:
		Choreographer.s_Choreographer.m_CurrentAbility = null;
	}

	public void Toggle(bool active, string text = null, bool hideOnClick = true, bool? interactable = null, Action onSkipAction = null)
	{
		if (!interactable.HasValue)
		{
			interactable = active;
		}
		this.hideOnClick = hideOnClick;
		this.onSkipAction = onSkipAction;
		ToggleVisibility(active);
		SetInteractable(interactable.Value);
		if (!string.IsNullOrEmpty(text))
		{
			buttonText.text = text;
		}
	}

	public void SetInteractable(bool active)
	{
		m_Interactable = active;
		CheckButtonInteractability();
	}

	private void CheckButtonInteractability()
	{
		GameObject currentClientActorGameObject = ((Choreographer.s_Choreographer.m_CurrentActor != null) ? Choreographer.s_Choreographer.FindClientActorGameObject(Choreographer.s_Choreographer.m_CurrentActor) : null);
		skipButton.interactable = m_Interactable && (!ScenarioRuleClient.IsProcessingOrMessagesQueued || GameState.WaitingForPlayerToSelectDamageResponse || GameState.WaitingForPlayerActorToAvoidDamageResponse) && (Choreographer.s_Choreographer.m_MessageQueue.Count <= 0 || AutoTestController.s_AutoTestCurrentlyLoaded) && Choreographer.s_Choreographer.ThisPlayerHasTurnControl && (Choreographer.s_Choreographer.m_CurrentActor == null || currentClientActorGameObject == null || Choreographer.s_Choreographer.IdleStates.Any((string x) => MF.GameObjectAnimatorControllerIsCurrentState(currentClientActorGameObject, x)));
		ChangeCanvasAlpha(skipButton.interactable && canvasGroup.interactable);
		RefreshInteractionStyle();
	}

	private void OnDisable()
	{
		ClearSkipAction();
	}

	public void ClearSkipAction()
	{
		onSkipAction = null;
	}
}
