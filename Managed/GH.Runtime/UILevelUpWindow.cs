using System;
using System.Collections;
using System.Collections.Generic;
using AsmodeeNet.Foundation;
using Chronos;
using FFSNet;
using GLOOM;
using JetBrains.Annotations;
using MapRuleLibrary.Adventure;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UILevelUpWindow : Singleton<UILevelUpWindow>, IEscapable
{
	[SerializeField]
	private TMP_Text characterName;

	[SerializeField]
	private TMP_Text characterLevel;

	[SerializeField]
	private UILevelUpCardInventory inventory;

	[SerializeField]
	private RectTransform header;

	[SerializeField]
	private ClickTrackerExtended nextCardTracker;

	[SerializeField]
	private UILevelUpCardHighlighter cardHolder;

	[SerializeField]
	private TMP_Text cardsReceivedText;

	[SerializeField]
	private UIIntroduceLevelUp introduction;

	[Header("Controller")]
	[SerializeField]
	private ControllerInputArea controllerArea;

	[SerializeField]
	private GameObject nextCardControllerTip;

	[SerializeField]
	private GameObject focusOnCardsTip;

	[SerializeField]
	private GameObject focusOnLevelUpTip;

	[Header("Animations")]
	[SerializeField]
	private Animation headerAnimator;

	[SerializeField]
	private float delayOpenWindows = 1f;

	[SerializeField]
	private TMP_Text textLevelBump;

	[SerializeField]
	private TMP_Text textLevelGlow;

	[SerializeField]
	private float delayHighlight = 0.3f;

	[SerializeField]
	private string animationHeaderOpen = "HeroLevelUpAnimationOpen";

	[SerializeField]
	private string animationHeaderMove = "HeroLevelUpAnimationMove";

	[SerializeField]
	private string audioItemShowNewCard = "PlaySound_UIReceivedItem";

	[SerializeField]
	private bool m_IsAllowedToEscapeDuringSave;

	private UIWindow myWindow;

	private ICharacterService character;

	private UnityAction onLeveledUpCallback;

	public static Action FinishedShowCards;

	private List<CAbilityCard> cardsAddedInLevel;

	private int currentCard;

	private bool isOpenConfirmationBox;

	private bool isPlayingOpenAnimation;

	private bool enableTracker;

	public bool IsShowing { get; private set; }

	public bool IsAllowedToEscapeDuringSave => m_IsAllowedToEscapeDuringSave;

	[UsedImplicitly]
	protected override void Awake()
	{
		base.Awake();
		myWindow = GetComponent<UIWindow>();
		myWindow.onTransitionComplete.AddListener(delegate(UIWindow window, UIWindow.VisualState state)
		{
			if (state == UIWindow.VisualState.Hidden)
			{
				OnHidden();
			}
		});
		myWindow.onShown.AddListener(OnOpen);
		nextCardTracker.onClick.AddListener(OnCardShown);
		controllerArea.OnFocused.AddListener(OnControllerFocused);
		controllerArea.OnUnfocused.AddListener(OnControllerUnfocused);
		controllerArea.OnEnabledArea.AddListener(OnControllerEnabled);
		controllerArea.OnDisabledArea.AddListener(OnControllerDisabled);
	}

	[UsedImplicitly]
	protected override void OnDestroy()
	{
		FinishedShowCards = null;
		base.OnDestroy();
	}

	private void ConfirmSelectCard(CAbilityCard card, bool isNew)
	{
		if ((!FFSNetwork.IsOnline || !FFSNetwork.IsHost || PlayerRegistry.JoiningPlayers.Count <= 0) && (!FFSNetwork.IsClient || !PlayerRegistry.OtherClientsAreJoining))
		{
			isOpenConfirmationBox = true;
			UIConfirmationBoxManager instance = Singleton<UIConfirmationBoxManager>.Instance;
			string translation = LocalizationManager.GetTranslation("GUI_CONFIRMATION_LEVELUP_CARD");
			UnityAction onActionConfirmed = delegate
			{
				isOpenConfirmationBox = false;
				SelectCard(card);
			};
			UnityAction onActionCancelled = delegate
			{
				isOpenConfirmationBox = false;
			};
			INavigationOperation toPreviousNonMenuState = NavigationOperation.ToPreviousNonMenuState;
			instance.ShowGenericConfirmation(translation, null, onActionConfirmed, onActionCancelled, null, null, null, showHeader: true, enableSoftlockReport: false, toPreviousNonMenuState);
		}
	}

	private void HighlightCard(CAbilityCard card, bool isNew)
	{
		cardsReceivedText.gameObject.SetActive(value: false);
		cardHolder.HighlightCard(card, isNew);
	}

	private void UnhighlightCard()
	{
		cardHolder.UnhighlightCard();
	}

	public void Show(ICharacterService characterData, UnityAction onLeveledUpCallback = null)
	{
		ClearInventoryEvents();
		if (!InputManager.GamePadInUse)
		{
			inventory.OnCardHovered.AddListener(HighlightCard);
			inventory.OnCardUnhovered.AddListener(UnhighlightCard);
		}
		inventory.OnCardSelected.AddListener(ConfirmSelectCard);
		Singleton<APartyDisplayUI>.Instance.EnableInteraction = false;
		Singleton<UIResetLevelUpWindow>.Instance.EnableShow(enableShow: false, this);
		Singleton<UINotificationManager>.Instance.Hide(this);
		characterName.text = string.Format(LocalizationManager.GetTranslation("GUI_LEVELUP_CHARACTER"), characterData.CharacterName.IsNullOrEmpty() ? LocalizationManager.GetTranslation(characterData.Class.LocKey) : characterData.CharacterName);
		TMP_Text tMP_Text = characterLevel;
		TMP_Text tMP_Text2 = textLevelBump;
		string text = (textLevelGlow.text = string.Format(LocalizationManager.GetTranslation("GUI_LEVELUP"), characterData.Level + 1));
		string text3 = (tMP_Text2.text = text);
		tMP_Text.text = text3;
		character = characterData;
		this.onLeveledUpCallback = onLeveledUpCallback;
		cardsAddedInLevel = characterData.GetUnownedAbilityCards(characterData.Level + 1, characterData.Level + 1);
		currentCard = 0;
		Clear();
		isPlayingOpenAnimation = true;
		myWindow.Show();
		UIWindowManager.RegisterEscapable(this);
	}

	private void OnOpen()
	{
		IsShowing = true;
		controllerArea.enabled = true;
		controllerArea.Focus();
		Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.ShowingCardLevelUp);
		introduction.Show(delegate
		{
			StartCoroutine(AnimateOpen());
		});
	}

	private IEnumerator AnimateOpen()
	{
		header.gameObject.SetActive(value: false);
		yield return Timekeeper.instance.WaitForSeconds(myWindow.transitionDuration);
		header.gameObject.SetActive(value: true);
		headerAnimator.Play(animationHeaderOpen);
		yield return Timekeeper.instance.WaitForSeconds(headerAnimator.GetClip(animationHeaderOpen).length);
		yield return Timekeeper.instance.WaitForSeconds(delayOpenWindows);
		float length = headerAnimator.GetClip(animationHeaderMove).length;
		headerAnimator.Play(animationHeaderMove);
		inventory.Show(character, length);
		inventory.EnableInteraction(enabled: false);
		if (!AdventureState.MapState.IsCampaign)
		{
			Singleton<APartyDisplayUI>.Instance.OpenCardsPanel(character.CharacterID, length);
		}
		ControllerInputAreaManager.Instance.UnfocusArea(EControllerInputAreaType.CharacterAbilityCards);
		yield return Timekeeper.instance.WaitForSeconds(length);
		isPlayingOpenAnimation = false;
		if (controllerArea.IsFocused)
		{
			focusOnCardsTip.SetActive(value: true);
		}
		ShowCard();
	}

	protected void ShowCard()
	{
		if (cardsAddedInLevel.IsNullOrEmpty() || currentCard == cardsAddedInLevel.Count)
		{
			OnFinishedShowCards();
			return;
		}
		AudioControllerUtils.PlaySound(audioItemShowNewCard);
		nextCardTracker.enabled = false;
		enableTracker = false;
		nextCardControllerTip.SetActive(value: false);
		cardHolder.HighlightWonCard(cardsAddedInLevel[currentCard], delegate
		{
			enableTracker = true;
			nextCardTracker.enabled = true;
			if (controllerArea.IsFocused)
			{
				nextCardControllerTip.SetActive(value: true);
			}
		});
	}

	private void OnCardShown()
	{
		nextCardTracker.enabled = false;
		enableTracker = false;
		nextCardControllerTip.SetActive(value: false);
		cardHolder.UnhighlightWonCard(delegate
		{
			inventory.AddNewCard(cardsAddedInLevel[currentCard]);
			currentCard++;
			ShowCard();
		});
	}

	private void SelectCard(CAbilityCard card)
	{
		if ((!FFSNetwork.IsOnline || !FFSNetwork.IsHost || PlayerRegistry.JoiningPlayers.Count <= 0) && (!FFSNetwork.IsClient || !PlayerRegistry.OtherClientsAreJoining))
		{
			if (!InputManager.GamePadInUse)
			{
				inventory.EnableInteraction(enabled: false);
			}
			StartCoroutine(MoveCardToCharacterInventory(card));
		}
	}

	private IEnumerator MoveCardToCharacterInventory(CAbilityCard card)
	{
		yield return Timekeeper.instance.WaitForSeconds(0.1f);
		inventory.RemoveCard(card);
		yield return Timekeeper.instance.WaitForSeconds(delayHighlight);
		character.GainCard(card);
		Close();
		yield return Timekeeper.instance.WaitForSeconds(0.1f);
		introduction.ShowAfterCardSelected(delegate
		{
		});
	}

	private void OnFinishedShowCards()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.LevelUp);
		nextCardTracker.enabled = false;
		enableTracker = false;
		inventory.EnableInteraction(enabled: true);
		cardsReceivedText.text = string.Format(LocalizationManager.GetTranslation("GUI_LEVELUP_RECEIVED_CARDS"), cardsAddedInLevel.Count);
		cardsReceivedText.gameObject.SetActive(value: true);
		nextCardControllerTip.SetActive(value: false);
		StartCoroutine(SkipAFrameAndSetIsShownToFalse());
		FinishedShowCards?.Invoke();
		controllerArea.Focus();
	}

	private void OnHidden()
	{
		inventory.OnCardSelected.RemoveListener(ConfirmSelectCard);
		StopAnimations();
		Singleton<APartyDisplayUI>.Instance.EnableInteraction = true;
		Singleton<UIResetLevelUpWindow>.Instance.EnableShow(enableShow: true, this);
		Singleton<UINotificationManager>.Instance.Show(this);
		if (isOpenConfirmationBox)
		{
			Singleton<UIConfirmationBoxManager>.Instance.Hide();
			isOpenConfirmationBox = false;
		}
		controllerArea.Unfocus();
		controllerArea.enabled = false;
		UIWindowManager.UnregisterEscapable(this);
		onLeveledUpCallback?.Invoke();
		ClearInventoryEvents();
	}

	private void ClearInventoryEvents()
	{
		if (!InputManager.GamePadInUse)
		{
			inventory.OnCardHovered.RemoveListener(HighlightCard);
			inventory.OnCardUnhovered.RemoveListener(UnhighlightCard);
		}
		inventory.OnCardSelected.RemoveListener(ConfirmSelectCard);
	}

	private void Clear()
	{
		cardsReceivedText.gameObject.SetActive(value: false);
		header.gameObject.SetActive(value: false);
		headerAnimator.ResetToStart(animationHeaderMove);
		inventory.EnableInteraction(enabled: false);
		nextCardTracker.enabled = false;
		enableTracker = false;
		nextCardControllerTip.SetActive(value: false);
		StopAnimations();
	}

	private void StopAnimations()
	{
		headerAnimator.Stop();
		StopAllCoroutines();
		inventory.Hide();
		cardHolder.UnhighlightCard();
	}

	public void ForceClose()
	{
		onLeveledUpCallback = null;
		inventory.Hide();
		ClearInventoryEvents();
		myWindow.Hide();
		IsShowing = false;
	}

	public void Close()
	{
		inventory.Hide(myWindow.transitionDuration);
		ClearInventoryEvents();
		myWindow.Hide();
	}

	public bool IsLevelingUp(string characterID)
	{
		if (IsLevelingUp())
		{
			return character.CharacterID == characterID;
		}
		return false;
	}

	public bool IsLevelingUp()
	{
		return myWindow.IsOpen;
	}

	public void Focus()
	{
		controllerArea.Focus();
	}

	private IEnumerator SkipAFrameAndSetIsShownToFalse()
	{
		yield return new WaitForEndOfFrame();
		IsShowing = false;
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			OnControllerDisabled();
			UIWindowManager.UnregisterEscapable(this);
			Singleton<UINotificationManager>.Instance.Show(this);
		}
	}

	private void OnControllerEnabled()
	{
		focusOnLevelUpTip.SetActive(value: false);
		focusOnCardsTip.SetActive(value: false);
		InputManager.RegisterToOnPressed(KeyAction.CONTROL_LOCAL_OPTIONS_LEFT, FocusOnCards);
		InputManager.RegisterToOnPressed(KeyAction.CONTROL_LOCAL_OPTIONS_RIGHT, controllerArea.Focus);
	}

	private void OnControllerDisabled()
	{
		focusOnLevelUpTip.SetActive(value: false);
		focusOnCardsTip.SetActive(value: false);
		InputManager.UnregisterToOnPressed(KeyAction.CONTROL_LOCAL_OPTIONS_LEFT, FocusOnCards);
		InputManager.UnregisterToOnPressed(KeyAction.CONTROL_LOCAL_OPTIONS_RIGHT, controllerArea.Focus);
	}

	private void FocusOnCards()
	{
		ControllerInputAreaManager.Instance.FocusArea(EControllerInputAreaType.CharacterAbilityCards, stack: true);
	}

	private void OnControllerFocused()
	{
		if (enableTracker && !nextCardTracker.enabled)
		{
			nextCardTracker.enabled = true;
		}
		else if (!enableTracker && nextCardTracker.enabled)
		{
			nextCardTracker.enabled = false;
		}
		nextCardControllerTip.SetActive(nextCardTracker.enabled && !isPlayingOpenAnimation);
		inventory.EnableNavigation();
		focusOnCardsTip.SetActive(!isPlayingOpenAnimation);
		focusOnLevelUpTip.SetActive(value: false);
	}

	private void OnControllerUnfocused()
	{
		nextCardTracker.enabled = false;
		nextCardControllerTip.SetActive(value: false);
		inventory.DisableNavigation();
		focusOnCardsTip.SetActive(value: false);
		focusOnLevelUpTip.SetActive(controllerArea.IsEnabled);
	}

	public bool Escape()
	{
		return true;
	}

	public int Order()
	{
		return 0;
	}
}
