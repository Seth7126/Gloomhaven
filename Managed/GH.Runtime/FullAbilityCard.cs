using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FFSNet;
using GLOOM;
using JetBrains.Annotations;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using Script.GUI.Ability_Card;
using Script.GUI.SMNavigation.Utils;
using SharedLibrary.SimpleLog;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FullAbilityCard : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI initiativeText;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private TextMeshProUGUI levelText;

	[SerializeField]
	private Image headerImage;

	[SerializeField]
	private Image buttonsHolderImage;

	[SerializeField]
	public CardEffects cardEffects;

	[SerializeField]
	public FullAbilityCardAction topActionButton;

	[SerializeField]
	public FullAbilityCardAction bottomActionButton;

	[SerializeField]
	private AttackValueBreakdown attackValueBreakdown;

	[SerializeField]
	private CardEnhancementElements _enhancementElements;

	[SerializeField]
	private string audioItemSelectCard = "PlaySound_CardUI_SelectCard";

	[SerializeField]
	private Image unfocusedMask;

	public TrackedButton DefaultMoveButton;

	public TrackedButton DefaultAttackButton;

	private int id;

	private CAbilityCard abilityCard;

	private Action<FullAbilityCard, CBaseCard.ActionType> onActionCallback;

	private Action onActionCompleteCallback;

	private CBaseCard.ECardPile cardPile = CBaseCard.ECardPile.Hand;

	public CreateLayout topLayout;

	public CreateLayout bottomLayout;

	[SerializeField]
	private bool isLongRestCard;

	private CPlayerActor playerActor;

	private CanvasGroup canvasGroup;

	private bool enableSelectCard;

	public UnityEvent onFullCardSelected = new UnityEvent();

	private bool isValid = true;

	private Action onClickedInvalidCard;

	private AbilityCardUISkin _skin;

	private RectTransform _rectTransform;

	[SerializeField]
	private ImageAddressableLoader _imageLoader;

	public Canvas Canvas;

	public static bool IsHover { get; private set; }

	private bool IsInstanceHover { get; set; }

	public FullCardViewSettings ViewSettings { get; set; } = UISettings.DefaultFullCardViewSettings;

	public RectTransform RectTransform => _rectTransform ?? (_rectTransform = GetComponent<RectTransform>());

	public CardEnhancementElements EnhancementElements => _enhancementElements;

	public string Title => titleText.text;

	public bool IsActionSelection { get; set; }

	public CAbilityCard AbilityCard => abilityCard;

	public static event Action<bool> FullCardHoveringStateChanged;

	public static event Action OnEnterForView;

	public void SetInteractable(bool active, CBaseCard.ActionType actionType)
	{
		SimpleLog.AddToSimpleLog("Set Interaction card: " + Title + " to " + active + " action " + actionType);
		if (actionType == CBaseCard.ActionType.BottomAction || actionType == CBaseCard.ActionType.DefaultMoveAction)
		{
			bottomActionButton.SetInteractable(active, actionType == CBaseCard.ActionType.DefaultMoveAction);
		}
		else
		{
			topActionButton.SetInteractable(active, actionType == CBaseCard.ActionType.DefaultAttackAction);
		}
	}

	public void SetInteractable(bool active)
	{
		bottomActionButton.SetInteractable(active);
		topActionButton.SetInteractable(active);
	}

	public void ResetInteractable()
	{
		bottomActionButton.ResetInteractable();
		topActionButton.ResetInteractable();
	}

	public void BlockRaycasts(bool block)
	{
		if (canvasGroup == null)
		{
			canvasGroup = GetComponent<CanvasGroup>();
			if (canvasGroup == null)
			{
				canvasGroup = base.gameObject.AddComponent<CanvasGroup>();
			}
		}
		canvasGroup.blocksRaycasts = block;
	}

	public bool IsInteractable(CBaseCard.ActionType actionType, bool considerSelection = true)
	{
		if (actionType == CBaseCard.ActionType.BottomAction || actionType == CBaseCard.ActionType.DefaultMoveAction)
		{
			return bottomActionButton.IsInteractable(actionType, considerSelection);
		}
		return topActionButton.IsInteractable(actionType, considerSelection);
	}

	public bool HasActionInteractable(bool considerSelection = true)
	{
		if (!bottomActionButton.IsInteractable(considerSelection))
		{
			return topActionButton.IsInteractable(considerSelection);
		}
		return true;
	}

	public void ToggleHover(bool active, bool isTopSide)
	{
		if (isTopSide)
		{
			topActionButton.ToggleAnimateBurnIcon(active);
			topActionButton.ToggleHover(active, isDefaultAbility: true);
			topActionButton.ToggleHover(active, isDefaultAbility: false);
			topActionButton.ToggleHoverInfusion(active);
		}
		else
		{
			bottomActionButton.ToggleAnimateBurnIcon(active);
			bottomActionButton.ToggleHover(active, isDefaultAbility: true);
			bottomActionButton.ToggleHover(active, isDefaultAbility: false);
			bottomActionButton.ToggleHoverInfusion(active);
		}
	}

	public void ToggleHighlightHover(bool active, bool isTopSide, bool isDefault)
	{
		if (isTopSide)
		{
			topActionButton.ToggleHighlightHover(active, isDefault);
		}
		else
		{
			bottomActionButton.ToggleHighlightHover(active, isDefault);
		}
	}

	public void UntoggleHighlightHover(bool isTopSide)
	{
		if (isTopSide)
		{
			topActionButton.ToggleHighlightHover(active: false, isDefaultAbility: false);
			topActionButton.ToggleHighlightHover(active: false, isDefaultAbility: true);
		}
		else
		{
			bottomActionButton.ToggleHighlightHover(active: false, isDefaultAbility: false);
			bottomActionButton.ToggleHighlightHover(active: false, isDefaultAbility: true);
		}
	}

	public void Deselect()
	{
		bottomActionButton.ToggleSelect(active: false, isDefaultAction: true);
		bottomActionButton.ToggleSelect(active: false);
		topActionButton.ToggleSelect(active: false, isDefaultAction: true);
		topActionButton.ToggleSelect(active: false);
	}

	public void ToggleSelect(bool active, CBaseCard.ActionType actionType)
	{
		if (actionType == CBaseCard.ActionType.BottomAction || actionType == CBaseCard.ActionType.DefaultMoveAction)
		{
			bottomActionButton.ToggleSelect(active, actionType == CBaseCard.ActionType.DefaultMoveAction);
		}
		else
		{
			topActionButton.ToggleSelect(active, actionType == CBaseCard.ActionType.DefaultAttackAction);
		}
	}

	public void SetSkin(AbilityCardUISkin skin)
	{
		if (!isLongRestCard)
		{
			if (!cardEffects.HasEffect(CardEffects.FXTask.BurnCard))
			{
				initiativeText.color = skin.initiativeColor;
			}
			_skin = skin;
			buttonsHolderImage.sprite = skin.buttonsHolderSprite;
		}
		topActionButton.SetSkin(skin, topAction: true, isLongRestCard);
		bottomActionButton.SetSkin(skin, topAction: false, isLongRestCard);
	}

	public void UpdateView(FullCardViewSettings viewSettings)
	{
		ViewSettings = viewSettings;
		UpdateView();
	}

	private void UpdateView()
	{
		if (IsActionSelection)
		{
			UpdateScale();
			UpdatePosition();
		}
	}

	private void UpdateScale()
	{
		base.transform.localScale = (IsInstanceHover ? ViewSettings.HoverScale : (IsHover ? ViewSettings.AnotherCardHoveredScale : ViewSettings.DefaultScale));
	}

	private void UpdatePosition()
	{
		if (ViewSettings.OverridePosition)
		{
			RectTransform.anchoredPosition = (IsInstanceHover ? Vector3.zero : (IsHover ? ViewSettings.AnotherCardHoveredPosition : ViewSettings.Position));
		}
	}

	public void Highlight(bool active)
	{
		if (IsInstanceHover == active)
		{
			return;
		}
		IsHover = active;
		IsInstanceHover = active;
		FullAbilityCard.FullCardHoveringStateChanged?.Invoke(IsHover);
		if (!(!topActionButton.IsInteractable() && !bottomActionButton.IsInteractable() && active))
		{
			if (active && abilityCard != null && DebugMenu.displayCardsId)
			{
				UIManager.Instance.DisplayDebugMessage(abilityCard.Name + " card ID: " + abilityCard.ID + " card instance ID: " + abilityCard.CardInstanceID);
			}
			UpdateView();
		}
	}

	public void HighlightAvailableConsumes(List<ElementInfusionBoardManager.EElement> availableElements)
	{
		topActionButton.HighlightAvailableConsumes(availableElements);
		bottomActionButton.HighlightAvailableConsumes(availableElements);
	}

	public void ToggleBurningAnimation(bool active)
	{
		topActionButton.ToggleAnimateBurnIcon(active);
		bottomActionButton.ToggleAnimateBurnIcon(active);
	}

	public void SetPile(CBaseCard.ECardPile newCardPile)
	{
		if (cardPile != newCardPile && cardEffects != null)
		{
			if (newCardPile == CBaseCard.ECardPile.Discarded)
			{
				cardEffects.ToggleEffect(active: true, CardEffects.FXTask.DiscardMode);
			}
			if (newCardPile == CBaseCard.ECardPile.Lost || newCardPile == CBaseCard.ECardPile.PermanentlyLost)
			{
				cardEffects.ToggleEffect(active: true, CardEffects.FXTask.LostMode, playOnDisabled: true);
			}
			if (newCardPile == CBaseCard.ECardPile.Hand || newCardPile == CBaseCard.ECardPile.Activated)
			{
				cardEffects.RestoreCard();
			}
		}
		cardPile = newCardPile;
		if (bottomActionButton != null)
		{
			bottomActionButton.UpdateView(cardPile);
		}
		if (topActionButton != null)
		{
			topActionButton.UpdateView(cardPile);
		}
	}

	public void RefreshPile()
	{
		if (cardEffects != null)
		{
			if (cardPile == CBaseCard.ECardPile.Discarded)
			{
				cardEffects.ToggleEffect(active: false, CardEffects.FXTask.DiscardMode);
			}
			if (cardPile == CBaseCard.ECardPile.Lost || cardPile == CBaseCard.ECardPile.PermanentlyLost)
			{
				cardEffects.ToggleEffect(active: false, CardEffects.FXTask.LostMode);
			}
			if (cardPile == CBaseCard.ECardPile.Hand)
			{
				cardEffects.RestoreCard();
			}
		}
	}

	public List<InfuseElement> UnselectedInfusions(CBaseCard.ActionType abilityType)
	{
		List<InfuseElement> list = new List<InfuseElement>();
		switch (abilityType)
		{
		case CBaseCard.ActionType.TopAction:
			list.AddRange(topActionButton.UnselectedInfusions());
			break;
		case CBaseCard.ActionType.BottomAction:
			list.AddRange(bottomActionButton.UnselectedInfusions());
			break;
		}
		return list;
	}

	public bool AreThereInfusionsToPick(CBaseCard.ActionType abilityType)
	{
		return UnselectedInfusions(abilityType).Count > 0;
	}

	private List<ElementInfusionBoardManager.EElement> SelectedAnyInfusionElemenents(CBaseCard.ActionType abilityType)
	{
		List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>();
		switch (abilityType)
		{
		case CBaseCard.ActionType.TopAction:
			list.AddRange(topActionButton.SelectedAnyInfusionElements());
			break;
		case CBaseCard.ActionType.BottomAction:
			list.AddRange(bottomActionButton.SelectedAnyInfusionElements());
			break;
		}
		return list;
	}

	public void Init(CPlayerActor playerActor, CAbilityCard abilityCard, int id, Action<FullAbilityCard, CBaseCard.ActionType> onActionCallback, Action onActionCompleteCallback)
	{
		this.playerActor = playerActor;
		this.onActionCallback = onActionCallback;
		this.onActionCompleteCallback = onActionCompleteCallback;
		this.abilityCard = abilityCard;
		this.id = id;
		SetPile(CBaseCard.ECardPile.Hand);
		FullCardHoveringStateChanged -= OnFullCardHoveringChanged;
		FullCardHoveringStateChanged += OnFullCardHoveringChanged;
	}

	public void DeInit()
	{
		onActionCallback = null;
		onActionCompleteCallback = null;
		FullCardHoveringStateChanged -= OnFullCardHoveringChanged;
	}

	private void OnFullCardHoveringChanged(bool isHover)
	{
		UpdateView();
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		if (SceneController.Instance != null)
		{
			SceneController.Instance.UnloadSpecialMemory -= UnloadAll;
			UnloadAll();
		}
	}

	[UsedImplicitly]
	protected void OnEnable()
	{
		ShowCard();
		UpdateView();
		SceneController.Instance.UnloadSpecialMemory -= UnloadAll;
		SceneController.Instance.UnloadSpecialMemory += UnloadAll;
	}

	public void ShowCard()
	{
		if (_skin != null)
		{
			_imageLoader.LoadAsync(this, headerImage, _skin.TitleSprite.SpriteReference);
			_imageLoader.LoadAsync(this, unfocusedMask, _skin.TitleSprite.SpriteReference);
		}
		bottomActionButton.Show();
		topActionButton.Show();
	}

	private void UnloadAll()
	{
		topActionButton.UnloadAll();
		bottomActionButton.UnloadAll();
		_imageLoader.UnloadAll();
	}

	public void MakeFullCard(string cardName, AbilityCardYMLData cardData, Vector2 contentPadding, bool isLongRest = false)
	{
		SetCardName(cardName);
		initiativeText.text = cardData.Initiative.ToString();
		if (levelText != null)
		{
			levelText.text = cardData.Level;
		}
		topLayout = topActionButton.MakeAbilityAction(cardData.TopActionFullLayout?.ParentGroup, cardData.ID, contentPadding, cardData.TopIsLost, cardData.TopIsPermLost, isLongRest, FullAbilityCardAction.CardHalf.Top);
		_enhancementElements.Add(topActionButton.EnhancementElements);
		isLongRestCard = isLongRest;
		if (!isLongRest)
		{
			bottomLayout = bottomActionButton.MakeAbilityAction(cardData.BottomActionFullLayout.ParentGroup, cardData.ID, contentPadding, cardData.BottomIsLost, cardData.BottomIsPermLost, isLongRest, FullAbilityCardAction.CardHalf.Bottom);
			_enhancementElements.Add(bottomActionButton.EnhancementElements);
		}
		else if (levelText != null)
		{
			levelText.gameObject.SetActive(value: false);
		}
	}

	public void MakeFullCardContinued()
	{
		if (topLayout != null)
		{
			topActionButton.MakeAbilityActionContinued(topLayout);
			topActionButton.AddEventPusher(FullCardEventPusher.EventType.Top);
		}
		if (!isLongRestCard && bottomLayout != null)
		{
			bottomActionButton.MakeAbilityActionContinued(bottomLayout);
			bottomActionButton.AddEventPusher(FullCardEventPusher.EventType.Bottom);
		}
		base.gameObject.AddFullCardEventPusher();
		topLayout = null;
		bottomLayout = null;
	}

	public void MoveAbilityClick()
	{
		if (bottomActionButton.IsInteractable(CBaseCard.ActionType.DefaultMoveAction))
		{
			OnAbilityClick(CBaseCard.ActionType.DefaultMoveAction, isProxyAction: false);
		}
	}

	public void AttackAbilityClick()
	{
		if (topActionButton.IsInteractable(CBaseCard.ActionType.DefaultAttackAction))
		{
			OnAbilityClick(CBaseCard.ActionType.DefaultAttackAction, isProxyAction: false);
		}
	}

	public void OnAbilityClick(bool isTopAbility)
	{
		OnAbilityClick((!isTopAbility) ? CBaseCard.ActionType.BottomAction : CBaseCard.ActionType.TopAction, isProxyAction: false);
	}

	public void OnPointerEnter(bool isTopAbility)
	{
		FullAbilityCard.OnEnterForView?.Invoke();
		ToggleHighlightButtonInFullCardViewer(active: true, isTopAbility, isDefaultAbility: false);
		if (isValid && !(isTopAbility ? (topActionButton == null) : (bottomActionButton == null)) && (isTopAbility ? topActionButton.IsInteractable(considerSelection: false) : bottomActionButton.IsInteractable(considerSelection: false)) && abilityCard != null && playerActor != null && !(CardsHandManager.Instance == null))
		{
			if (isTopAbility)
			{
				topActionButton.ToggleAnimateBurnIcon(active: true);
				topActionButton.ToggleHoverInfusion(active: true);
			}
			else
			{
				bottomActionButton.ToggleHoverInfusion(active: true);
				bottomActionButton.ToggleAnimateBurnIcon(active: true);
			}
			CardsHandManager.Instance.cardsActionController.OnCardHighlight(isActive: true, this, isTopAbility, isDefaultAbility: false);
			CAction cAction = (isTopAbility ? abilityCard.TopAction : abilityCard.BottomAction);
			if (cAction.FindType(CAbility.EAbilityType.Attack) != null)
			{
				attackValueBreakdown.Init(playerActor.FindApplicableActiveBonuses(CAbility.EAbilityType.Attack), GameState.CurrentActionSelectedAugmentations, cAction);
				attackValueBreakdown.Show();
			}
		}
	}

	public void OnPointerExit(bool isTopAbility)
	{
		ToggleHighlightButtonInFullCardViewer(active: false, isTopAbility, isDefaultAbility: false);
		CardsHandManager.Instance?.cardsActionController.OnCardHighlight(isActive: false, this, isTopAbility, isDefaultAbility: false);
		if (isTopAbility)
		{
			topActionButton.ToggleHoverInfusion(active: false);
			topActionButton.ToggleAnimateBurnIcon(active: false);
		}
		else
		{
			bottomActionButton.ToggleHoverInfusion(active: false);
			bottomActionButton.ToggleAnimateBurnIcon(active: false);
		}
		if (attackValueBreakdown != null)
		{
			attackValueBreakdown.Hide();
		}
	}

	public void OnDefaultPointerEnter(bool isTopAbility)
	{
		ToggleHighlightButtonInFullCardViewer(active: true, isTopAbility, isDefaultAbility: true);
		if (isValid && (isTopAbility ? topActionButton.IsInteractable(considerSelection: false) : bottomActionButton.IsInteractable(considerSelection: false)) && abilityCard != null && playerActor != null && !(CardsHandManager.Instance == null))
		{
			CardsHandManager.Instance.cardsActionController.OnCardHighlight(isActive: true, this, isTopAbility, isDefaultAbility: true);
		}
	}

	public void OnDefaultPointerExit(bool isTopAbility)
	{
		ToggleHighlightButtonInFullCardViewer(active: false, isTopAbility, isDefaultAbility: true);
		CardsHandManager.Instance?.cardsActionController.OnCardHighlight(isActive: false, this, isTopAbility, isDefaultAbility: true);
	}

	public void TryPlayBurnAnimation(CBaseCard.ActionType abilityType)
	{
		if (cardPile == CBaseCard.ECardPile.Lost || cardPile == CBaseCard.ECardPile.PermanentlyLost)
		{
			return;
		}
		if ((abilityType == CBaseCard.ActionType.BottomAction && (abilityCard.BottomAction.CardPile == CBaseCard.ECardPile.Lost || abilityCard.BottomAction.CardPile == CBaseCard.ECardPile.PermanentlyLost)) || (abilityType == CBaseCard.ActionType.TopAction && (abilityCard.TopAction.CardPile == CBaseCard.ECardPile.Lost || abilityCard.TopAction.CardPile == CBaseCard.ECardPile.PermanentlyLost)))
		{
			if (abilityCard.ActionHasHappened)
			{
				if (!cardEffects.HasEffect(CardEffects.FXTask.BurnCard))
				{
					cardEffects.ToggleEffect(active: true, CardEffects.FXTask.BurnCard);
					AudioController.Play("PlaySound_CardUI_BurnedCard", base.transform, null, attachToParent: false);
				}
			}
			else if (!cardEffects.HasEffect(CardEffects.FXTask.DiscardMode) && !cardEffects.HasEffect(CardEffects.FXTask.BurnCard))
			{
				cardEffects.ToggleEffect(active: true, CardEffects.FXTask.DiscardMode);
				AudioController.Play("PlaySound_CardUI_BurnedCard", base.transform, null, attachToParent: false);
			}
		}
		else if (!cardEffects.HasEffect(CardEffects.FXTask.DiscardMode) && !cardEffects.HasEffect(CardEffects.FXTask.BurnCard))
		{
			cardEffects.ToggleEffect(active: true, CardEffects.FXTask.DiscardMode);
			AudioController.Play("PlaySound_CardUI_DiscardedCard", base.transform, null, attachToParent: false);
		}
	}

	public void OnAbilityClick(CBaseCard.ActionType abilityType, bool isProxyAction, bool checkValid = true)
	{
		if (InfusionBoardUI.Instance == null)
		{
			SimpleLog.AddToSimpleLog("Returning from OnAbilityClick for card: " + titleText.text + " early due to InfusionBoardUI instance == null");
			return;
		}
		if (CardsHandManager.Instance.IsFullCardPreviewShowing && !isProxyAction)
		{
			SimpleLog.AddToSimpleLog("Returning from OnAbilityClick for card: " + titleText.text + " early due to CardsHandManager IsFullCardPreviewShowing and !isProxyAction");
			return;
		}
		if (!IsInteractable(abilityType, !isLongRestCard))
		{
			SimpleLog.AddToSimpleLog("Returning from OnAbilityClick for card: " + titleText.text + " early due to card not being interactable action type" + abilityType);
			return;
		}
		if (checkValid && !isValid)
		{
			AudioControllerUtils.PlaySound(UIInfoTools.Instance.InvalidOptionAudioItem);
			onClickedInvalidCard?.Invoke();
			SimpleLog.AddToSimpleLog("Returning from OnAbilityClick for card: " + titleText.text + " early due to card not being valid");
			return;
		}
		if (enableSelectCard)
		{
			AudioControllerUtils.PlaySound(audioItemSelectCard);
			onFullCardSelected.Invoke();
		}
		if (PhaseManager.PhaseType == CPhase.PhaseType.Action || (CardsHandManager.Instance != null && !CardsHandManager.Instance.cardsActionController.IsActionAvailable) || (PhaseManager.PhaseType == CPhase.PhaseType.ActionSelection && Choreographer.s_Choreographer.CurrentActor != playerActor))
		{
			SimpleLog.AddToSimpleLog("Returning from OnAbilityClick for card: " + titleText.text + " early due to phase type");
			return;
		}
		if (cardPile != CBaseCard.ECardPile.Round && PhaseManager.PhaseType == CPhase.PhaseType.ActionSelection && !isProxyAction)
		{
			SimpleLog.AddToSimpleLog("Returning from OnAbilityClick for card: " + titleText.text + " early due to UI card not being in the round pile at action selection phase");
			return;
		}
		if (!enableSelectCard)
		{
			AudioControllerUtils.PlaySound(audioItemSelectCard);
		}
		CBaseCard.ActionType actionType;
		UIEvent.EUIEventType eventType;
		switch (abilityType)
		{
		case CBaseCard.ActionType.TopAction:
			actionType = CBaseCard.ActionType.DefaultAttackAction;
			eventType = UIEvent.EUIEventType.CardTopHalfSelected;
			break;
		case CBaseCard.ActionType.DefaultAttackAction:
			actionType = CBaseCard.ActionType.TopAction;
			eventType = UIEvent.EUIEventType.DefaultAttackAbilityPressed;
			break;
		case CBaseCard.ActionType.BottomAction:
			actionType = CBaseCard.ActionType.DefaultMoveAction;
			eventType = UIEvent.EUIEventType.CardBottomHalfSelected;
			break;
		case CBaseCard.ActionType.DefaultMoveAction:
			actionType = CBaseCard.ActionType.BottomAction;
			eventType = UIEvent.EUIEventType.DefaultMoveAbilityPressed;
			break;
		default:
			Debug.LogError("Invalid ability type set in OnAbilityClick().");
			return;
		}
		SimpleLog.AddToSimpleLog("ToggleActionInteractivity to false for card: " + titleText.text + " after ability click for " + actionType);
		ToggleActionInteractivity(active: false, actionType);
		ElementInfusionBoardManager.SaveState();
		InfusionBoardUI.Instance.SaveState();
		Singleton<UIUseItemsBar>.Instance.Hide();
		Singleton<UIActiveBonusBar>.Instance.Hide(toggle: true);
		Singleton<UIUseAbilitiesBar>.Instance.Hide(clearSelection: true);
		Singleton<UIUseAugmentationsBar>.Instance.Hide();
		UIEventManager.LogUIEvent(new UIEvent(eventType, playerActor.GetPrefabName(), abilityCard.Name));
		Choreographer.s_Choreographer.onCharacterAbilityComplete = delegate
		{
			OnActionMade(abilityType);
		};
		onActionCallback(this, abilityType);
		GameState.PlayerSelectedAbilityCardAction(abilityCard, abilityType);
		Choreographer.s_Choreographer.Pass();
		if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl)
		{
			ActionProcessor.SetState(ActionProcessorStateType.Halted);
			int targetPhaseType = ((GameState.CurrentActionSelectionSequence == GameState.ActionSelectionSequenceType.FirstAction) ? 7 : 8);
			int iD = playerActor.ID;
			int cardInstanceID = abilityCard.CardInstanceID;
			CBaseCard.ActionType supplementaryDataIDMin = abilityType;
			byte[] bytes = Encoding.ASCII.GetBytes("Ability: " + abilityCard.Name);
			Synchronizer.SendGameAction(GameActionType.SelectCardAbility, (ActionPhaseType)targetPhaseType, validateOnServerBeforeExecuting: false, disableAutoReplication: false, iD, (int)supplementaryDataIDMin, 0, cardInstanceID, supplementaryDataBoolean: false, default(Guid), null, null, null, null, bytes, binaryDataIncludesLoggingDetails: true);
		}
	}

	public void UpdateElements()
	{
		if (abilityCard == null)
		{
			return;
		}
		List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>();
		if (abilityCard.SelectedAction?.Infusions != null)
		{
			foreach (ElementInfusionBoardManager.EElement infusion in abilityCard.SelectedAction.Infusions)
			{
				if (infusion != ElementInfusionBoardManager.EElement.Any && abilityCard.SelectedAction.Abilities.Any((CAbility a) => a.AbilityHasHappened))
				{
					list.Add(infusion);
					if (abilityCard.SelectedAction?.ID == abilityCard.TopAction.ID)
					{
						topActionButton.AnimateGenerateInfusions();
					}
					else if (abilityCard.SelectedAction?.ID == abilityCard.BottomAction.ID)
					{
						bottomActionButton.AnimateGenerateInfusions();
					}
				}
			}
		}
		InfusionBoardUI.Instance.UpdateBoard(list);
	}

	private void OnActionMade(CBaseCard.ActionType abilityType)
	{
		onActionCompleteCallback();
	}

	private void ToggleHighlightButtonInFullCardViewer(bool active, bool isTopAbility, bool isDefaultAbility)
	{
		FullAbilityCardAction fullAbilityCardAction = (isTopAbility ? topActionButton : bottomActionButton);
		if (fullAbilityCardAction != null && Singleton<FullCardHandViewer>.Instance != null && Singleton<FullCardHandViewer>.Instance.IsActive)
		{
			fullAbilityCardAction.ToggleHighlightHover(active, isDefaultAbility);
		}
	}

	public void ToggleSideInteractivity(bool active, CBaseCard.ActionType actionType)
	{
		if (actionType == CBaseCard.ActionType.TopAction || actionType == CBaseCard.ActionType.DefaultAttackAction)
		{
			topActionButton.SetInteractable(active);
		}
		else
		{
			bottomActionButton.SetInteractable(active);
		}
	}

	public void ToggleActionInteractivity(bool active, CBaseCard.ActionType actionType)
	{
		switch (actionType)
		{
		case CBaseCard.ActionType.TopAction:
			topActionButton.SetInteractable(active, defaultAction: false);
			break;
		case CBaseCard.ActionType.DefaultAttackAction:
			topActionButton.SetInteractable(active, defaultAction: true);
			break;
		default:
			bottomActionButton.SetInteractable(active, actionType == CBaseCard.ActionType.DefaultMoveAction);
			break;
		}
	}

	public void ClearEffects()
	{
		ToggleBurningAnimation(active: false);
		cardEffects?.RestoreCard();
		ResetHighlightInfusions();
	}

	public void HighlightInfusions(bool highlight, CBaseCard.ActionType actionType)
	{
		switch (actionType)
		{
		case CBaseCard.ActionType.BottomAction:
			bottomActionButton.ToggleSelectedHighlightInfusion(highlight);
			break;
		case CBaseCard.ActionType.TopAction:
			topActionButton.ToggleSelectedHighlightInfusion(highlight);
			break;
		default:
			topActionButton.ToggleSelectedHighlightInfusion(active: false);
			bottomActionButton.ToggleSelectedHighlightInfusion(active: false);
			break;
		}
	}

	public void ResetHighlightInfusions()
	{
		topActionButton.ToggleSelectedHighlightInfusion(active: false);
		bottomActionButton.ToggleSelectedHighlightInfusion(active: false);
	}

	public void PickUnselectedInfusions(CBaseCard.ActionType abilityType, Action<bool> onFinish, CActor actorPicking)
	{
		List<InfuseElement> infusions = UnselectedInfusions(abilityType);
		if (infusions.Count == 0)
		{
			onFinish?.Invoke(obj: false);
			return;
		}
		((Action)delegate
		{
			ElementInfusionBoardManager.Infuse(SelectedAnyInfusionElemenents(abilityType), actorPicking);
			InfusionBoardUI.Instance.UpdateBoard(SelectedAnyInfusionElemenents(abilityType).ToList());
			if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
			{
				foreach (InfuseElement item in infusions)
				{
					_ = item;
					((CPhaseAction)PhaseManager.CurrentPhase).ElementsToInfuse.Remove(ElementInfusionBoardManager.EElement.Any);
				}
			}
			onFinish?.Invoke(obj: true);
		})();
	}

	public void EnableSelectCard(bool enabled)
	{
		enableSelectCard = enabled;
	}

	public void DisplaySelected(bool isSelected)
	{
		topActionButton.DisplaySelected(enableSelectCard && isSelected, topAction: true, isLongRestCard);
		bottomActionButton.DisplaySelected(enableSelectCard && isSelected, topAction: false, isLongRestCard);
	}

	public void ProxyToggleAugment(bool toggleOn, AbilityAugmentToken dataToken)
	{
		FullAbilityCardAction fullAbilityCardAction = ((dataToken.ActionTypeID == 0) ? topActionButton : bottomActionButton);
		if (dataToken.ConsumeIndex < fullAbilityCardAction.consumeButtons.Count && dataToken.ConsumeIndex >= 0)
		{
			Singleton<UIUseAugmentationsBar>.Instance.ProxyToggleAugment(fullAbilityCardAction.consumeButtons[dataToken.ConsumeIndex], toggleOn, dataToken);
			return;
		}
		throw new Exception("Error trying to augment an ability. The provided consume index is out of bounds.");
	}

	public void SetCardName(string cardName)
	{
		titleText.text = LocalizationManager.GetTranslation(cardName);
	}

	public void SetValid(bool isValid, Action onClickedInvalidCard = null)
	{
		this.isValid = isValid;
		this.onClickedInvalidCard = onClickedInvalidCard;
		bottomActionButton.DisableHoverHighlight(!isValid);
		topActionButton.DisableHoverHighlight(!isValid);
	}

	public void ResetConsumes(bool clearselection = true)
	{
		foreach (ConsumeButton consumeButton in bottomActionButton.consumeButtons)
		{
			consumeButton.ResetState(clearselection);
		}
		foreach (ConsumeButton consumeButton2 in topActionButton.consumeButtons)
		{
			consumeButton2.ResetState(clearselection);
		}
	}

	public ConsumeButton FindConsumeButton(CAbility ability)
	{
		AbilityCardYMLData getAbilityCardYML = abilityCard.GetAbilityCardYML;
		ConsumeButton consumeButton = FindAbilityConsumeButton(getAbilityCardYML.TopConsumes, topActionButton.consumeButtons, ability);
		if (consumeButton != null)
		{
			return consumeButton;
		}
		return FindAbilityConsumeButton(getAbilityCardYML.BottomConsumes, bottomActionButton.consumeButtons, ability);
	}

	private ConsumeButton FindAbilityConsumeButton(List<AbilityConsume> consumes, List<ConsumeButton> buttons, CAbility ability)
	{
		AbilityConsume consume = consumes.FirstOrDefault((AbilityConsume it) => it.ConsumeData != null && it.ConsumeData.AugmentationOps.Exists((CActionAugmentationOp cActionAugmentationOp) => cActionAugmentationOp.AbilityOverride != null && cActionAugmentationOp.AbilityOverride.ChooseAbilities.Contains(ability)));
		if (consume != null)
		{
			return buttons.FirstOrDefault((ConsumeButton it) => it.ConsumeName == consume.Name);
		}
		return null;
	}

	public void HighlightInCanvas(int order)
	{
	}

	public void ResetHighlightInCanvas()
	{
	}

	public void SetUnfocused(bool unfocused)
	{
		unfocusedMask.gameObject.SetActive(unfocused);
	}

	public void EnableNavigation(Selectable up = null, Selectable down = null, Selectable left = null)
	{
		if (!isLongRestCard)
		{
			if (topActionButton.IsInteractable())
			{
				Button actionButton = topActionButton.actionButton;
				Selectable up2 = up;
				Selectable defaultAttackButton = DefaultAttackButton;
				actionButton.SetNavigation(null, left, up2, defaultAttackButton);
				TrackedButton defaultAttackButton2 = DefaultAttackButton;
				defaultAttackButton = topActionButton.actionButton;
				up2 = (bottomActionButton.IsInteractable() ? DefaultMoveButton : down);
				defaultAttackButton2.SetNavigation(null, left, defaultAttackButton, up2);
			}
			if (bottomActionButton.IsInteractable())
			{
				TrackedButton defaultMoveButton = DefaultMoveButton;
				Selectable up2 = (topActionButton.IsInteractable() ? DefaultAttackButton : up);
				Selectable defaultAttackButton = bottomActionButton.actionButton;
				defaultMoveButton.SetNavigation(null, left, up2, defaultAttackButton);
				Button actionButton2 = bottomActionButton.actionButton;
				defaultAttackButton = DefaultMoveButton;
				up2 = down;
				actionButton2.SetNavigation(null, left, defaultAttackButton, up2);
			}
		}
	}

	public Selectable GetFirstSelectable()
	{
		if (topActionButton.IsInteractable(CBaseCard.ActionType.TopAction))
		{
			return topActionButton.actionButton;
		}
		if (topActionButton.IsInteractable(CBaseCard.ActionType.DefaultAttackAction))
		{
			return DefaultAttackButton;
		}
		if (bottomActionButton.IsInteractable(CBaseCard.ActionType.BottomAction))
		{
			return bottomActionButton.actionButton;
		}
		if (bottomActionButton.IsInteractable(CBaseCard.ActionType.DefaultMoveAction))
		{
			return DefaultMoveButton;
		}
		return null;
	}

	public void DisableNavigation()
	{
		DefaultAttackButton?.DisableNavigation(clear: true);
		DefaultMoveButton?.DisableNavigation(clear: true);
		bottomActionButton?.actionButton?.DisableNavigation(clear: true);
		topActionButton?.actionButton?.DisableNavigation(clear: true);
	}
}
