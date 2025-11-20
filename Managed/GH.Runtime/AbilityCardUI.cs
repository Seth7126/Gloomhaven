#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using FFSNet;
using JetBrains.Annotations;
using MapRuleLibrary.Party;
using SM.Gamepad;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.PopupStates;
using SharedLibrary.SimpleLog;
using UnityEngine;
using UnityEngine.UI;

public class AbilityCardUI : LocalizedListener, IComparable, IPooleable
{
	public int CardID;

	public int CardInstanceID;

	public string CardName;

	[Header("Internal references")]
	[SerializeField]
	public FullAbilityCard fullAbilityCard;

	[SerializeField]
	private MiniAbilityCard miniAbilityCard;

	public CanvasGroup canvasGroup;

	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private UINavigationSelectable _uiNavigationSelectable;

	[SerializeField]
	private UIFX_MaterialFX_Control fxControls;

	[SerializeField]
	private AudioButtonProfile interactionAudioProfile;

	[Header("Display settings")]
	[SerializeField]
	private Vector2 fullCardDescriptionPadding;

	[SerializeField]
	private float previewCardHeight;

	[SerializeField]
	private float fullCardSpacing;

	[SerializeField]
	private float defaultHighlightScale = 1.2f;

	[SerializeField]
	private Vector3 defaultHoverMovement = new Vector3(10f, 0f, 0f);

	[SerializeField]
	private float unselectedCardOppacity = 0.2f;

	[SerializeField]
	private float unselectedDiscardedCardOppacity = 0.5f;

	private static AbilityCardUI _hoveredCard;

	private Transform fullCardHolder;

	private Func<RectTransform, Vector2> overrideFullPreviewCardPosition;

	private bool alwaysShowFullCard;

	private bool isSelectable;

	private bool isHovered;

	private bool isTransitMarked;

	private bool isHighlighted;

	private bool isFaded;

	private bool isInFocus;

	private bool isInFurtherAbilityPanel;

	private IndicatorType indicatorType;

	private InitiativeTrackActorAvatar.InitiativeEffects effect;

	private CardPileType cardType = CardPileType.Any;

	private CardPileType unselectedCardType = CardPileType.Any;

	private Action<AbilityCardUI, bool> onSelectedCallback;

	private Action<AbilityCardUI, bool> onDeselectedCallback;

	private Action<FullAbilityCard, CBaseCard.ActionType> onActionCallback;

	private Action<AbilityCardUI> onCancelActiveAbilityCallback;

	private Action onActionCompleteCallback;

	private CardHandMode mode;

	private CPlayerActor playerActor;

	private Action<bool, AbilityCardUI> onHoverAction;

	private Action<AbilityCardUI> onUnableToSelectAction;

	private bool disableFullCard;

	private List<InteractabilityIsolatedUIControl> m_IsolatableCardControls = new List<InteractabilityIsolatedUIControl>();

	private bool isValid = true;

	private Action onClickedInvalidCard;

	private float currentFullCardSpacing;

	private UiNavigationBase _navigationBase;

	private RectTransform _rectTransform;

	private CAbilityCard abilityCard;

	private bool isInteractable;

	private Coroutine _waitCoroutine;

	private const float Delay = 0.2f;

	private bool displayDebugMessage;

	private bool previewingFullCard;

	private Func<bool> _canToggleCard;

	public float PreviewCardHeight => previewCardHeight;

	public CCharacterClass PlayerClass { get; private set; }

	public string CharacterID { get; private set; }

	public string PrefabPath { get; set; }

	public MiniAbilityCard MiniAbilityCard => miniAbilityCard;

	public bool IsLongRest { get; private set; }

	public RectTransform RectTransform => _rectTransform ?? (_rectTransform = GetComponent<RectTransform>());

	public CPlayerActor PlayerActor => playerActor;

	public bool LockFullCard { get; set; }

	public bool IsTheMainLongRestCard { get; set; }

	public bool IsHovered => isHovered;

	public bool IsSelected
	{
		get
		{
			if (SaveData.Instance.Global.CurrentGameState == EGameState.Scenario)
			{
				if (IsLongRest)
				{
					if (CardsHandManager.Instance?.CurrentHand == null)
					{
						return false;
					}
					if (PhaseManager.CurrentPhase.Type == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
					{
						return playerActor.IsLongRestSelected;
					}
					return playerActor.IsLongRestActionSelected;
				}
				if (CardsHandManager.Instance.CurrentHand.currentMode == CardHandMode.CardsSelection)
				{
					if (playerActor.SelectingCardsForExtraTurnOfType != CAbilityExtraTurn.EExtraTurnType.None)
					{
						if (CardsHandManager.Instance?.CurrentHand == null)
						{
							return false;
						}
						return CardsHandManager.Instance.CurrentHand.IsCardSelected(CardID);
					}
					return playerActor.CharacterClass.RoundAbilityCards.Exists((CAbilityCard e) => e.ID == CardID && e.CardInstanceID == CardInstanceID);
				}
				if (CardsHandManager.Instance?.CurrentHand == null)
				{
					return false;
				}
				if (CardsHandManager.Instance.CurrentHand.PlayerActor.Class.ID != playerActor.Class.ID)
				{
					return playerActor.CharacterClass.RoundAbilityCards.Exists((CAbilityCard e) => e.ID == CardID && e.CardInstanceID == CardInstanceID);
				}
				return CardsHandManager.Instance.CurrentHand.IsCardSelected(CardID);
			}
			if (SaveData.Instance.Global.CurrentGameState == EGameState.Map)
			{
				CMapCharacter cMapCharacter = SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter s) => s.CharacterID == CharacterID);
				if (cMapCharacter != null)
				{
					return cMapCharacter.HandAbilityCardIDs.Contains(CardID);
				}
				Debug.LogError("Unable to find Map Party for AbilityCardUI");
				return false;
			}
			if (CustomPartySetup.Instance != null)
			{
				CMapCharacter cMapCharacter2 = CustomPartySetup.Instance.CurrentCustomParty.AdventureMapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter s) => s.CharacterID == CharacterID);
				if (cMapCharacter2 != null)
				{
					return cMapCharacter2.HandAbilityCardIDs.Contains(CardID);
				}
				Debug.LogError("Unable to find Map Party for AbilityCardUI");
				return false;
			}
			Debug.LogError("Unable to find Map Party for AbilityCardUI");
			return false;
		}
	}

	public CardHandMode Mode => mode;

	public CardPileType CardType => cardType;

	public CAbilityCard AbilityCard => abilityCard;

	public bool IsInteractable
	{
		get
		{
			return isInteractable;
		}
		set
		{
			SimpleLog.AddToSimpleLog("Set IsInteractable card: " + abilityCard?.Name + " to " + value);
			isInteractable = value;
			fullAbilityCard.SetInteractable(isInteractable);
		}
	}

	public CardEnhancementElements EnhancementElements => fullAbilityCard.EnhancementElements;

	public bool IsSelectable => isSelectable;

	public bool IsHoverable => button.scaleNonInteractable;

	public Selectable Selectable => button;

	public static event Action<AbilityCardUI, bool> CardHoveringStateChanged;

	public static event Action<AbilityCardUI, bool> CardSelectionStateChanged;

	public event Action CardSelected;

	public event Action CardDeselected;

	[UsedImplicitly]
	private void Awake()
	{
		fullAbilityCard.onFullCardSelected.AddListener(OnSelectCardFromAction);
	}

	[UsedImplicitly]
	protected void OnDestroy()
	{
		if (!CoreApplication.IsQuitting)
		{
			if (_uiNavigationSelectable != null)
			{
				_uiNavigationSelectable.OnNavigationSelectedEvent -= OnUINavigationSelected;
			}
			fullAbilityCard.onFullCardSelected.RemoveAllListeners();
			button.onClick.RemoveListener(delegate
			{
				OnClick(ignoreHiglight: true);
			});
		}
	}

	private void Start()
	{
		OnLanguageChanged();
		PlayEffect(InitiativeTrackActorAvatar.InitiativeEffects.None);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		ObjectPool.instance.RegisterAbilityCardUI(this);
		SubscribeOnEventsGamepad();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (ObjectPool.instance != null)
		{
			ObjectPool.instance.RemoveAbilityCardUI(this);
		}
		UnSubscribeOnEventsGamepad();
	}

	public void OnSelectCardFromAction()
	{
		if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest || mode == CardHandMode.LoseCard || mode == CardHandMode.DiscardCard || (PhaseManager.PhaseType == CPhase.PhaseType.ActionSelection && IsLongRest))
		{
			OnClick();
		}
	}

	public void OnClick()
	{
		OnClick(ignoreHiglight: false);
	}

	public void OnClick(bool ignoreHiglight)
	{
		if ((isSelectable || isHighlighted || ignoreHiglight) && InteractabilityManager.ShouldAllowClickForExtendedButton(button))
		{
			if (!isValid)
			{
				AudioControllerUtils.PlaySound(interactionAudioProfile.nonInteractableMouseDownAudioItem);
				miniAbilityCard.ShowWarning(show: true);
				onClickedInvalidCard?.Invoke();
			}
			else if (isSelectable && cardType == CardPileType.Active && onCancelActiveAbilityCallback != null)
			{
				AudioControllerUtils.PlaySound(interactionAudioProfile.mouseDownAudioItem);
				onCancelActiveAbilityCallback(this);
			}
			else
			{
				ToggleSelect(!IsSelected, highlight: true);
			}
		}
	}

	private IEnumerator Wait(Action callback)
	{
		float currentTime = 0f;
		while (currentTime < 0.2f)
		{
			currentTime += Time.deltaTime;
			if (InputManager.GetWasReleased(KeyAction.UI_SUBMIT))
			{
				if (isSelectable && InteractabilityManager.ShouldAllowClickForExtendedButton(button))
				{
					callback();
				}
				break;
			}
			yield return null;
		}
	}

	public void UpdateElements()
	{
		fullAbilityCard.UpdateElements();
	}

	public void OnPointerEnter()
	{
		if (isInFurtherAbilityPanel)
		{
			return;
		}
		isHovered = true;
		if (mode != CardHandMode.ActionSelection && mode != CardHandMode.Preview && !alwaysShowFullCard)
		{
			if (abilityCard != null && DebugMenu.displayCardsId)
			{
				displayDebugMessage = true;
				UIManager.Instance.DisplayDebugMessage(abilityCard.Name + " card ID: " + abilityCard.ID + " card instance ID: " + abilityCard.CardInstanceID);
			}
			if (IsLongRest)
			{
				CardsHandManager.Instance?.CurrentHand?.ToggleHighlight(new List<CardPileType> { CardPileType.Discarded }, active: true, keepLongRest: true);
			}
			onHoverAction?.Invoke(arg1: true, this);
			ToggleFullCardPreview(isHighlighted: true, fullCardHolder);
			if (mode == CardHandMode.CardsSelection && playerActor.CharacterClass.RoundAbilityCards.Count == 0 && isSelectable && !playerActor.CharacterClass.LongRest && playerActor.CharacterClass.ExtraTurnCardsSelectedInCardSelectionStack.Count == 0)
			{
				PlayEffect(InitiativeTrackActorAvatar.InitiativeEffects.Hover);
			}
			button.ToggleFade(active: false);
			AbilityCardUI.CardHoveringStateChanged?.Invoke(this, arg2: true);
			if ((bool)_hoveredCard)
			{
				_hoveredCard.OnPointerExit();
			}
			_hoveredCard = this;
		}
	}

	[UsedImplicitly]
	public void OnTransitNavigationMarked()
	{
		isTransitMarked = true;
	}

	public void OnPointerExit()
	{
		if (isInFurtherAbilityPanel)
		{
			return;
		}
		isHovered = false;
		if (isTransitMarked)
		{
			return;
		}
		if (mode != CardHandMode.ActionSelection && mode != CardHandMode.Preview && !alwaysShowFullCard)
		{
			ToggleFullCardPreview(isHighlighted: false, fullCardHolder);
		}
		_hoveredCard = null;
		if (displayDebugMessage)
		{
			displayDebugMessage = false;
			UIManager.Instance.HideDebugMessage();
		}
		if (IsLongRest && CardsHandManager.Instance?.CurrentHand != null)
		{
			CardsHandManager.Instance.CurrentHand.ToggleHighlight(new List<CardPileType>
			{
				CardPileType.Hand,
				CardPileType.Round
			}, active: false);
		}
		onHoverAction?.Invoke(arg1: false, this);
		AbilityCardUI.CardHoveringStateChanged?.Invoke(this, arg2: false);
		CPlayerActor cPlayerActor = playerActor;
		if (cPlayerActor == null || cPlayerActor.TakingExtraTurnOfType != CAbilityExtraTurn.EExtraTurnType.BothActionsLater)
		{
			if (mode == CardHandMode.CardsSelection && playerActor.CharacterClass.RoundAbilityCards.Count == 0 && !playerActor.CharacterClass.LongRest)
			{
				PlayEffect(InitiativeTrackActorAvatar.InitiativeEffects.None);
			}
		}
		else
		{
			CPlayerActor cPlayerActor2 = playerActor;
			if (cPlayerActor2 != null && cPlayerActor2.CharacterClass.ExtraTurnCardsSelectedInCardSelectionStack.Count == 0)
			{
				PlayEffect(InitiativeTrackActorAvatar.InitiativeEffects.None);
			}
		}
		button.ToggleFade(isFaded, (cardType == CardPileType.Discarded || cardType == CardPileType.Permalost || cardType == CardPileType.Lost) ? unselectedDiscardedCardOppacity : unselectedCardOppacity);
	}

	public void OnTransitNavigationUnmarked()
	{
		isTransitMarked = false;
	}

	public void SetCanToggleFunctor(Func<bool> canBeToggled)
	{
		_canToggleCard = canBeToggled;
	}

	public void Init(CAbilityCard abilityCard, CPlayerActor playerActor, CardPileType cardType, Action<AbilityCardUI, bool> onSelectedCallback, Action<AbilityCardUI, bool> onDeselectedCallback, Action<AbilityCardUI> onUnableToSelectCallback, Action<bool, AbilityCardUI> onHoverAction, Action<FullAbilityCard, CBaseCard.ActionType> onActionCallback, Action onActionCompleteCallback, Action<AbilityCardUI> onCancelActiveAbility = null, bool isLongRest = false, Func<RectTransform, Vector2> overrideFullPreviewCardPosition = null, Func<bool> canToggleCard = null)
	{
		ResetDefaultParameters();
		this.playerActor = playerActor;
		this.onActionCallback = onActionCallback;
		this.abilityCard = abilityCard;
		this.onSelectedCallback = onSelectedCallback;
		this.onDeselectedCallback = onDeselectedCallback;
		onUnableToSelectAction = onUnableToSelectCallback;
		this.onHoverAction = onHoverAction;
		this.onActionCompleteCallback = onActionCompleteCallback;
		onCancelActiveAbilityCallback = onCancelActiveAbility;
		this.overrideFullPreviewCardPosition = overrideFullPreviewCardPosition;
		_canToggleCard = canToggleCard;
		IsLongRest = isLongRest;
		SetSkin(playerActor.CharacterClass.DefaultModel, playerActor.CharacterClass.CharacterYML.CustomCharacterConfig);
		SetType(cardType);
		SetDisplayMode(cardType);
		SetUpInteractabilityRelationship();
		disableFullCard = false;
		CharacterID = playerActor.Class.ID;
		if (abilityCard != null)
		{
			CardInstanceID = abilityCard.CardInstanceID;
		}
		if (!IsLongRest && !button.onClick.HasMethod("OnClick", this))
		{
			button.onClick.AddListener(OnClick);
		}
	}

	public void Init(CAbilityCard abilityCard, Transform fullCardHolder, string characterID, Action<AbilityCardUI, bool> onSelectedCallback, Action<AbilityCardUI, bool> onDeselectedCallback, Action<AbilityCardUI> onUnableToSelectCallback, Action<bool, AbilityCardUI> onHoverAction, float highlightScale, Vector3 highlightDisplacement, float fullCardSpacing, bool disableFullCard = false)
	{
		Init(abilityCard, fullCardHolder, characterID, onSelectedCallback, onDeselectedCallback, onUnableToSelectCallback, onHoverAction, disableFullCard);
		button.highlightScaleFactor = highlightScale;
		button.hoverMovement = highlightDisplacement;
		currentFullCardSpacing = fullCardSpacing;
	}

	public void Init(CAbilityCard abilityCard, Transform fullCardHolder, string characterID, Action<AbilityCardUI, bool> onSelectedCallback, Action<AbilityCardUI, bool> onDeselectedCallback, Action<AbilityCardUI> onUnableToSelectCallback, Action<bool, AbilityCardUI> onHoverAction = null, bool disableFullCard = false, Func<RectTransform, Vector2> overrideFullPreviewCardPosition = null)
	{
		ResetDefaultParameters();
		this.onHoverAction = onHoverAction;
		this.abilityCard = abilityCard;
		this.onSelectedCallback = onSelectedCallback;
		this.onDeselectedCallback = onDeselectedCallback;
		onUnableToSelectAction = onUnableToSelectCallback;
		this.fullCardHolder = fullCardHolder;
		this.overrideFullPreviewCardPosition = overrideFullPreviewCardPosition;
		CharacterID = characterID;
		if (abilityCard != null)
		{
			CardInstanceID = abilityCard.CardInstanceID;
		}
		CCharacterClass cCharacterClass = CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == characterID);
		SetSkin(cCharacterClass.DefaultModel, cCharacterClass.CharacterYML.CustomCharacterConfig);
		SetMode(CardHandMode.DeckSelection, new List<CardPileType> { CardPileType.Any });
		SetType(IsSelected ? CardPileType.Round : CardPileType.Hand);
		SetDisplayMode(IsSelected ? CardPileType.Round : CardPileType.Hand);
		SetUpInteractabilityRelationship();
		this.disableFullCard = disableFullCard;
		if (!button.onClick.HasMethod("OnClick", this))
		{
			button.onClick.AddListener(OnClick);
		}
	}

	private void ResetDefaultParameters()
	{
		currentFullCardSpacing = fullCardSpacing;
		fullCardHolder = null;
		overrideFullPreviewCardPosition = null;
		button.highlightScaleFactor = defaultHighlightScale;
		button.hoverMovement = defaultHoverMovement;
	}

	public void Init(CAbilityCard abilityCard, bool disableEventDetection = false)
	{
		ResetDefaultParameters();
		this.abilityCard = abilityCard;
		SetSkin(abilityCard.ClassModel, abilityCard.ClassCharacterConfig);
		SetMode(CardHandMode.Preview, null);
		SetUpInteractabilityRelationship();
		disableFullCard = false;
		DisableEventDetection(disableEventDetection);
	}

	public void DeInit()
	{
		button.onClick.RemoveListener(OnClick);
		_canToggleCard = null;
		onHoverAction = null;
		onSelectedCallback = null;
		onDeselectedCallback = null;
		onUnableToSelectAction = null;
		onCancelActiveAbilityCallback = null;
		onActionCompleteCallback = null;
		onActionCallback = null;
		fullCardHolder = null;
		overrideFullPreviewCardPosition = null;
		onClickedInvalidCard = null;
		playerActor = null;
		m_IsolatableCardControls.Clear();
		fullAbilityCard.DeInit();
	}

	public void Show()
	{
		fullAbilityCard.ShowCard();
	}

	private void DisableEventDetection(bool disable)
	{
		button.enabled = !disable;
		fullAbilityCard.bottomActionButton.actionButton.enabled = !disable;
		fullAbilityCard.topActionButton.actionButton.enabled = !disable;
	}

	public void EditorInit(CCharacterClass playerClass)
	{
		ResetDefaultParameters();
		SetSkin(playerClass.DefaultModel, playerClass.CharacterYML.CustomCharacterConfig);
		SetType(CardPileType.Active);
		SetDisplayMode(CardPileType.Active);
	}

	public void CreateCardsInit(string classModel)
	{
		ResetDefaultParameters();
		SetSkin(classModel);
		SetType(CardPileType.Active);
		SetDisplayMode(CardPileType.Active);
	}

	private void SetUpInteractabilityRelationship()
	{
		if (m_IsolatableCardControls.Count <= 0)
		{
			InteractabilityIsolatedUIControl interactabilityIsolatedUIControl = base.gameObject.AddComponent<InteractabilityIsolatedUIControl>();
			m_IsolatableCardControls.Add(interactabilityIsolatedUIControl);
			interactabilityIsolatedUIControl.PoolableObject = true;
			interactabilityIsolatedUIControl.ControlType = CLevelUIInteractionProfile.EIsolatedControlType.CardSelection;
			interactabilityIsolatedUIControl.ControlIdentifier = CardName;
			interactabilityIsolatedUIControl.ExtendedButtonsToAllow.Add(button);
			interactabilityIsolatedUIControl.SpecifiedRectTransformToFollow = ((miniAbilityCard.HighlightRect == null) ? miniAbilityCard.GetComponent<RectTransform>() : miniAbilityCard.HighlightRect);
			interactabilityIsolatedUIControl.Initialise();
			InteractabilityIsolatedUIControl interactabilityIsolatedUIControl2 = fullAbilityCard.topActionButton.gameObject.AddComponent<InteractabilityIsolatedUIControl>();
			m_IsolatableCardControls.Add(interactabilityIsolatedUIControl2);
			interactabilityIsolatedUIControl2.PoolableObject = true;
			interactabilityIsolatedUIControl2.ControlType = CLevelUIInteractionProfile.EIsolatedControlType.CardTopHalfSelection;
			interactabilityIsolatedUIControl2.ControlIdentifier = CardName;
			interactabilityIsolatedUIControl2.TrackedButtonsToAllow.Add(fullAbilityCard.topActionButton.actionButton.GetComponent<TrackedButton>());
			interactabilityIsolatedUIControl2.Initialise();
			InteractabilityIsolatedUIControl interactabilityIsolatedUIControl3 = fullAbilityCard.bottomActionButton.gameObject.AddComponent<InteractabilityIsolatedUIControl>();
			m_IsolatableCardControls.Add(interactabilityIsolatedUIControl3);
			interactabilityIsolatedUIControl3.PoolableObject = true;
			interactabilityIsolatedUIControl3.ControlType = CLevelUIInteractionProfile.EIsolatedControlType.CardBottomHalfSelection;
			interactabilityIsolatedUIControl3.ControlIdentifier = CardName;
			interactabilityIsolatedUIControl3.TrackedButtonsToAllow.Add(fullAbilityCard.bottomActionButton.actionButton.GetComponent<TrackedButton>());
			interactabilityIsolatedUIControl3.Initialise();
			if (fullAbilityCard.DefaultMoveButton != null)
			{
				InteractabilityIsolatedUIControl interactabilityIsolatedUIControl4 = fullAbilityCard.DefaultMoveButton.gameObject.AddComponent<InteractabilityIsolatedUIControl>();
				m_IsolatableCardControls.Add(interactabilityIsolatedUIControl4);
				interactabilityIsolatedUIControl4.PoolableObject = true;
				interactabilityIsolatedUIControl4.ControlType = CLevelUIInteractionProfile.EIsolatedControlType.DefaultMoveAbility;
				interactabilityIsolatedUIControl4.ControlIdentifier = CardName;
				interactabilityIsolatedUIControl4.TrackedButtonsToAllow.Add(fullAbilityCard.DefaultMoveButton);
				interactabilityIsolatedUIControl4.Initialise();
			}
			if (fullAbilityCard.DefaultAttackButton != null)
			{
				InteractabilityIsolatedUIControl interactabilityIsolatedUIControl5 = fullAbilityCard.DefaultAttackButton.gameObject.AddComponent<InteractabilityIsolatedUIControl>();
				m_IsolatableCardControls.Add(interactabilityIsolatedUIControl5);
				interactabilityIsolatedUIControl5.PoolableObject = true;
				interactabilityIsolatedUIControl5.ControlType = CLevelUIInteractionProfile.EIsolatedControlType.DefaultAttackAbility;
				interactabilityIsolatedUIControl5.ControlIdentifier = CardName;
				interactabilityIsolatedUIControl5.TrackedButtonsToAllow.Add(fullAbilityCard.DefaultAttackButton);
				interactabilityIsolatedUIControl5.Initialise();
			}
			for (int i = 0; i < fullAbilityCard.topActionButton.consumeButtons.Count; i++)
			{
				ConsumeButton consumeButton = fullAbilityCard.topActionButton.consumeButtons[i];
				consumeButton.SetupForAbilityCard(isTopAction: true, CardID, i);
				InteractabilityIsolatedUIControl interactabilityIsolatedUIControl6 = consumeButton.gameObject.AddComponent<InteractabilityIsolatedUIControl>();
				m_IsolatableCardControls.Add(interactabilityIsolatedUIControl6);
				interactabilityIsolatedUIControl6.PoolableObject = true;
				interactabilityIsolatedUIControl6.ControlType = CLevelUIInteractionProfile.EIsolatedControlType.ConsumeElementOnCardTop;
				interactabilityIsolatedUIControl6.ControlIdentifier = CardName;
				interactabilityIsolatedUIControl6.ControlIndex = i;
				interactabilityIsolatedUIControl6.SpecifiedRectTransformToFollow = consumeButton.transform.parent.GetComponent<RectTransform>();
				interactabilityIsolatedUIControl6.Initialise();
			}
			for (int j = 0; j < fullAbilityCard.bottomActionButton.consumeButtons.Count; j++)
			{
				ConsumeButton consumeButton2 = fullAbilityCard.bottomActionButton.consumeButtons[j];
				consumeButton2.SetupForAbilityCard(isTopAction: false, CardID, j);
				InteractabilityIsolatedUIControl interactabilityIsolatedUIControl7 = consumeButton2.gameObject.AddComponent<InteractabilityIsolatedUIControl>();
				m_IsolatableCardControls.Add(interactabilityIsolatedUIControl7);
				interactabilityIsolatedUIControl7.PoolableObject = true;
				interactabilityIsolatedUIControl7.ControlType = CLevelUIInteractionProfile.EIsolatedControlType.ConsumeElementOnCardBottom;
				interactabilityIsolatedUIControl7.ControlIdentifier = CardName;
				interactabilityIsolatedUIControl7.ControlIndex = j;
				interactabilityIsolatedUIControl7.SpecifiedRectTransformToFollow = consumeButton2.transform.parent.GetComponent<RectTransform>();
				interactabilityIsolatedUIControl7.Initialise();
			}
		}
	}

	public void RemoveInteractabilityRelationship()
	{
		foreach (InteractabilityIsolatedUIControl isolatableCardControl in m_IsolatableCardControls)
		{
			UnityEngine.Object.Destroy(isolatableCardControl);
		}
		m_IsolatableCardControls.Clear();
	}

	private void SubscribeOnEventsGamepad()
	{
		if (InputManager.GamePadInUse && _uiNavigationSelectable != null)
		{
			_uiNavigationSelectable.OnNavigationSelectedEvent -= OnUINavigationSelected;
			_uiNavigationSelectable.OnNavigationSelectedEvent += OnUINavigationSelected;
			_uiNavigationSelectable.OnNavigationDeselectedEvent += OnUINavigationDeselected;
		}
	}

	private void UnSubscribeOnEventsGamepad()
	{
		if (InputManager.GamePadInUse)
		{
			if (_uiNavigationSelectable != null)
			{
				_uiNavigationSelectable.OnNavigationSelectedEvent -= OnUINavigationSelected;
				_uiNavigationSelectable.OnNavigationDeselectedEvent -= OnUINavigationDeselected;
			}
			if (Singleton<KeyActionHandlerController>.Instance != null)
			{
				Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_SUBMIT, OnClick);
			}
		}
	}

	private void OnUINavigationSelected(IUiNavigationSelectable obj)
	{
		this.CardSelected?.Invoke();
	}

	private void OnUINavigationDeselected(IUiNavigationSelectable obj)
	{
		this.CardDeselected?.Invoke();
	}

	private void SetSkin(string playerName, string custom = null)
	{
		SetSkin(UIInfoTools.Instance.GetCardSkin(playerName, custom));
	}

	private void SetSkin(AbilityCardUISkin skin)
	{
		fullAbilityCard.SetSkin(skin);
		miniAbilityCard.SetSkin(skin);
	}

	public void SetType(CardPileType type)
	{
		unselectedCardType = ((cardType == CardPileType.Round) ? CardPileType.Hand : cardType);
		cardType = type;
		if (!disableFullCard || mode != CardHandMode.DeckSelection)
		{
			fullAbilityCard.SetPile(CardPileTypeToECardPile(type));
		}
	}

	public void HiglightFullCard(bool active)
	{
		Debug.Log($"[AbilityCardUI-{base.gameObject.name}] Highlight full card {active}");
		fullAbilityCard.Highlight(active);
		UpdateSize(mode);
		fullAbilityCard.SetPile(CardPileTypeToECardPile(cardType));
	}

	public void LostAnimation(bool active, float cardHighlightTime)
	{
		StartCoroutine(miniAbilityCard.LostAnimation(active, cardHighlightTime));
	}

	public void CancelLostAnimation()
	{
		StopAllCoroutines();
		miniAbilityCard.CancelLostAnimation();
	}

	public void UpdateCard()
	{
		SetDisplayMode(cardType);
		if (!disableFullCard || mode != CardHandMode.DeckSelection)
		{
			fullAbilityCard.SetPile(CardPileTypeToECardPile(cardType));
		}
	}

	public void Highlight(bool active, List<CardPileType> highlightCardTypes = null, bool showIndicator = false, bool keepLongRest = false, bool keepShortRest = false)
	{
		isHighlighted = active && ((!IsLongRest && highlightCardTypes != null && highlightCardTypes.Contains(cardType)) || (IsLongRest && isSelectable && keepLongRest));
		if (IsLongRest)
		{
			button.ToggleFade(!isSelectable || !keepLongRest);
		}
		else if (isHighlighted)
		{
			button.ToggleFade(active: false);
		}
		bool flag = keepLongRest || keepShortRest;
		bool flag2 = (isHighlighted || showIndicator) && !IsLongRest;
		SetDisplayMode(cardType, !flag2 && isHighlighted);
		if (flag2)
		{
			if (flag || indicatorType == IndicatorType.Burn)
			{
				miniAbilityCard.EnableGlowBurnSelected();
			}
			else
			{
				miniAbilityCard.EnableGlowSelected();
			}
		}
		else
		{
			miniAbilityCard.DisableGlowSelected();
		}
		miniAbilityCard.DisplayIndicator(flag2, flag ? IndicatorType.Burn : indicatorType);
	}

	public void SwapInitiative()
	{
		if (PhaseManager.PhaseType != CPhase.PhaseType.SelectAbilityCardsOrLongRest || playerActor.CharacterClass.RoundAbilityCards.Count != 2 || !playerActor.CharacterClass.RoundAbilityCards.Contains(abilityCard))
		{
			if (mode == CardHandMode.CardsSelection && playerActor.TakingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater)
			{
				miniAbilityCard.ShowWarning(show: true);
				Singleton<HelpBox>.Instance.HighlightWarning();
			}
		}
		else
		{
			if (!InteractabilityManager.ShouldAllowClickForExtendedButton(button))
			{
				return;
			}
			if (!isValid)
			{
				AudioControllerUtils.PlaySound(interactionAudioProfile.nonInteractableMouseDownAudioItem);
				miniAbilityCard.ShowWarning(show: true);
				onClickedInvalidCard?.Invoke();
				return;
			}
			if (miniAbilityCard.InitiativeButton != null && miniAbilityCard.InitiativeButton.interactable && AutoTestController.s_ShouldRecordUIActionsForAutoTest)
			{
				AutoTestController.s_Instance.LogButtonClick(miniAbilityCard.InitiativeButton.gameObject);
			}
			AudioControllerUtils.PlaySound(interactionAudioProfile.mouseDownAudioItem);
			if (playerActor.CharacterClass.InitiativeAbilityCard == playerActor.CharacterClass.RoundAbilityCards[0])
			{
				playerActor.CharacterClass.SetInitiativeAbilityCard(playerActor.CharacterClass.RoundAbilityCards[1]);
				playerActor.CharacterClass.SetSubInitiativeAbilityCard(playerActor.CharacterClass.RoundAbilityCards[0]);
			}
			else
			{
				playerActor.CharacterClass.SetInitiativeAbilityCard(playerActor.CharacterClass.RoundAbilityCards[0]);
				playerActor.CharacterClass.SetSubInitiativeAbilityCard(playerActor.CharacterClass.RoundAbilityCards[1]);
			}
			playerActor.CharacterClass.RoundAbilityCards.Reverse();
			InitiativeTrack.Instance.UpdateActors();
			CardsHandManager.Instance.GetHand(playerActor).NetworkSelectedRoundCards();
		}
	}

	public void SetSelectable(bool isOn, bool isHoverable = true)
	{
		isSelectable = isOn;
		GetComponent<Button>().interactable = isOn;
		GetComponent<ExtendedButton>().scaleNonInteractable = isHoverable;
	}

	public void DisableNavigation()
	{
		button.DisableNavigation(clear: true);
		fullAbilityCard.DisableNavigation();
	}

	public void SetValid(bool isValid, Action onClickedInvalidCard = null, Action onClickedInvalidFullCard = null)
	{
		this.isValid = isValid;
		this.onClickedInvalidCard = onClickedInvalidCard;
		if (isValid)
		{
			miniAbilityCard.ShowWarning(show: false);
		}
		fullAbilityCard.SetValid(isValid, onClickedInvalidFullCard);
	}

	public void PlayEffect(InitiativeTrackActorAvatar.InitiativeEffects effect)
	{
		if ((this.effect != effect || effect == InitiativeTrackActorAvatar.InitiativeEffects.None) && fxControls.gameObject.activeInHierarchy)
		{
			this.effect = effect;
			switch (effect)
			{
			case InitiativeTrackActorAvatar.InitiativeEffects.Active:
				fxControls.InitiativeActive();
				break;
			case InitiativeTrackActorAvatar.InitiativeEffects.Hover:
				fxControls.InitiativeHover();
				break;
			case InitiativeTrackActorAvatar.InitiativeEffects.None:
				fxControls.InitiativeNone();
				break;
			case InitiativeTrackActorAvatar.InitiativeEffects.Select:
				fxControls.InitiativeSelect();
				break;
			}
		}
	}

	public void SetMode(CardHandMode mode, List<CardPileType> selectableCardType, bool fadeUnselectableCards = false, bool highlightSelectableCard = false, bool longRestAvailable = false, Func<CAbilityCard, bool> cardSelectableFilter = null)
	{
		if (playerActor != null)
		{
			SetSkin(playerActor.CharacterClass.DefaultModel, playerActor.CharacterClass.CharacterYML.CustomCharacterConfig);
		}
		SetSelectable(selectableCardType != null && !selectableCardType.Contains(CardPileType.None) && (selectableCardType.Contains(CardPileType.Any) || selectableCardType.Contains(cardType)) && (!IsLongRest || longRestAvailable) && (cardSelectableFilter?.Invoke(abilityCard) ?? true));
		isFaded = (!isSelectable && fadeUnselectableCards) || (IsLongRest && !isSelectable);
		isInFocus = isSelectable && highlightSelectableCard && !IsLongRest;
		indicatorType = ((mode == CardHandMode.LoseCard) ? IndicatorType.Burn : IndicatorType.General);
		if (isInFocus)
		{
			Highlight(active: true, selectableCardType, showIndicator: true);
		}
		else
		{
			Highlight(active: false);
		}
		button.ToggleFade(isFaded, (cardType == CardPileType.Discarded || cardType == CardPileType.Permalost || cardType == CardPileType.Lost) ? unselectedDiscardedCardOppacity : unselectedCardOppacity);
		switch (mode)
		{
		case CardHandMode.CardsSelection:
		case CardHandMode.LoseCard:
		case CardHandMode.DeckSelection:
		case CardHandMode.RecoverLostCard:
		case CardHandMode.RecoverDiscardedCard:
		case CardHandMode.IncreaseCardLimit:
		case CardHandMode.DiscardCard:
			this.mode = mode;
			fullAbilityCard.SetInteractable(active: true);
			ToggleFullCard(active: false);
			break;
		case CardHandMode.ActionSelection:
		{
			bool num;
			if (!playerActor.IsTakingExtraTurn)
			{
				if (!playerActor.CharacterClass.RoundAbilityCards.Contains(abilityCard) && !playerActor.CharacterClass.RoundCardsSelectedInCardSelection.Contains(abilityCard))
				{
					goto IL_01e3;
				}
				num = this.mode == mode;
			}
			else
			{
				num = !playerActor.CharacterClass.ExtraTurnCardsSelectedInCardSelection.Contains(abilityCard);
			}
			if (num)
			{
				goto IL_01e3;
			}
			goto IL_01fe;
		}
		case CardHandMode.Preview:
			{
				this.mode = mode;
				ToggleFullCard(active: true);
				break;
			}
			IL_01e3:
			if (!playerActor.CharacterClass.LongRest || !IsLongRest)
			{
				break;
			}
			goto IL_01fe;
			IL_01fe:
			this.mode = mode;
			fullAbilityCard.Init(playerActor, abilityCard, playerActor.IsTakingExtraTurn ? playerActor.CharacterClass.ExtraTurnCardsSelectedInCardSelection.IndexOf(abilityCard) : playerActor.CharacterClass.RoundCardsSelectedInCardSelection.IndexOf(abilityCard), onActionCallback, onActionCompleteCallback);
			ToggleFullCard(active: true);
			break;
		}
	}

	public void ToggleFullSelectionCard(bool active)
	{
		if (!IsLongRest)
		{
			alwaysShowFullCard = active;
			fullAbilityCard.EnableSelectCard(active);
			bool flag = mode == CardHandMode.ActionSelection && playerActor.CharacterClass.RoundCardsSelectedInCardSelection.Contains(abilityCard);
			bool flag2 = FFSNetwork.IsOnline && SaveData.Instance.Global.CurrentGameState == EGameState.Scenario && cardType == CardPileType.Round && !playerActor.IsUnderMyControl && PhaseManager.CurrentPhase.Type == CPhase.PhaseType.SelectAbilityCardsOrLongRest;
			fullAbilityCard.DisplaySelected((IsSelected || flag) && active && !flag2);
		}
	}

	public void ToggleFullCardCanvasSorting(bool active)
	{
		if (fullAbilityCard.Canvas != null)
		{
			fullAbilityCard.Canvas.overrideSorting = active;
		}
	}

	public void ToggleFullCard(bool active)
	{
		RectTransform rectTransform = fullAbilityCard.RectTransform;
		if (active)
		{
			miniAbilityCard.gameObject.SetActive(value: false);
			fullAbilityCard.gameObject.SetActive(value: true);
			rectTransform.anchorMin = new Vector2(0f, 0.5f);
			rectTransform.anchorMax = new Vector2(0f, 0.5f);
			rectTransform.anchoredPosition = (IsTheMainLongRestCard ? ((Vector2)new Vector3(0f, -68f)) : new Vector2(0f, 0f));
			UpdateSize(CardHandMode.ActionSelection);
		}
		else
		{
			miniAbilityCard.gameObject.SetActive(value: true);
			fullAbilityCard.gameObject.SetActive(value: false);
			rectTransform.anchorMin = new Vector2(1f, 0.5f);
			rectTransform.anchorMax = new Vector2(1f, 0.5f);
			rectTransform.anchoredPosition = (IsTheMainLongRestCard ? ((Vector2)new Vector3(0f, -68f)) : new Vector2(currentFullCardSpacing, 0f));
			fullAbilityCard.Highlight(active: false);
			UpdateSize(mode);
		}
	}

	private void SetDisplayMode(CardPileType type)
	{
		SetDisplayMode(type, isHighlighted);
	}

	private void SetDisplayMode(CardPileType type, bool isHighlighted)
	{
		if (FFSNetwork.IsOnline && SaveData.Instance.Global.CurrentGameState == EGameState.Scenario && type == CardPileType.Round && !playerActor.IsUnderMyControl && PhaseManager.CurrentPhase.Type == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			type = unselectedCardType;
		}
		miniAbilityCard.SetDisplayMode(type, abilityCard, isHighlighted && isValid, isHovered && isValid);
	}

	public void ToggleFullCardPreview(bool isHighlighted, Transform holder = null)
	{
		if (LockFullCard)
		{
			return;
		}
		if (previewingFullCard == isHighlighted || (disableFullCard && mode == CardHandMode.DeckSelection))
		{
			ChangeFullCardPosition();
			return;
		}
		previewingFullCard = isHighlighted;
		SetDisplayMode(cardType, isHighlighted);
		ToggleFullCard(active: false);
		fullAbilityCard.Highlight(active: false);
		fullAbilityCard.gameObject.SetActive(isHighlighted);
		fullAbilityCard.ToggleBurningAnimation(isHighlighted);
		Canvas component;
		if (isHighlighted)
		{
			Canvas canvas = fullAbilityCard.gameObject.AddComponent<Canvas>();
			if (canvas != null)
			{
				canvas.overrideSorting = true;
				canvas.sortingOrder = 10;
			}
		}
		else if (fullAbilityCard.gameObject.TryGetComponent<Canvas>(out component))
		{
			UnityEngine.Object.Destroy(component);
		}
		fullAbilityCard.BlockRaycasts(holder == null || !isHighlighted);
		if (isHighlighted && !isInFurtherAbilityPanel)
		{
			ChangeFullCardPosition();
		}
	}

	private void ChangeFullCardPosition()
	{
		if (CardsHandManager.Instance?.CurrentHand.ShortRestedCard != abilityCard)
		{
			RectTransform rectTransform = fullAbilityCard.RectTransform;
			fullAbilityCard.transform.position = new Vector3(base.transform.position.x + 40f, base.transform.position.y - 45f);
			Vector3 vector = rectTransform.DeltaWorldPositionToFitTheScreen(UIManager.Instance.UICamera, 160f, 160f);
			rectTransform.position += vector;
			if ((miniAbilityCard.GetComponent<RectTransform>().anchoredPosition - rectTransform.anchoredPosition).x > 500f)
			{
				rectTransform.anchoredPosition = new Vector3(rectTransform.anchoredPosition.x * 1.4f, rectTransform.anchoredPosition.y, rectTransform.anchoredPosition.x);
			}
		}
	}

	private void UpdateSize(CardHandMode mode)
	{
		if (mode == CardHandMode.CardsSelection || mode == CardHandMode.LoseCard || mode == CardHandMode.DiscardCard || mode == CardHandMode.DeckSelection || mode == CardHandMode.RecoverDiscardedCard || mode == CardHandMode.RecoverLostCard || mode == CardHandMode.IncreaseCardLimit)
		{
			GetComponent<RectTransform>().sizeDelta = new Vector2(miniAbilityCard.GetComponent<RectTransform>().sizeDelta.x, previewCardHeight);
		}
		else
		{
			GetComponent<RectTransform>().sizeDelta = new Vector2(fullAbilityCard.GetComponent<RectTransform>().sizeDelta.x * fullAbilityCard.transform.localScale.x, fullAbilityCard.GetComponent<RectTransform>().sizeDelta.y * fullAbilityCard.transform.localScale.y);
		}
	}

	public void ToggleHighlight(bool active, bool highlight)
	{
		cardType = (active ? CardPileType.Round : unselectedCardType);
		SetDisplayMode(active ? CardPileType.Round : unselectedCardType, highlight);
		if (FFSNetwork.IsOnline && SaveData.Instance.Global.CurrentGameState == EGameState.Scenario && !playerActor.IsUnderMyControl && PhaseManager.CurrentPhase.Type == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			fullAbilityCard.DisplaySelected(isSelected: false);
		}
		else
		{
			fullAbilityCard.DisplaySelected(active);
		}
	}

	public void ToggleSelect(bool active, bool highlight = false, bool networkAction = true, bool isHandUnderMyControl = true)
	{
		if ((_canToggleCard != null && !_canToggleCard()) || Singleton<UINavigation>.Instance.StateMachine.CurrentState is LevelMessageState)
		{
			return;
		}
		if (!active && !IsSelected)
		{
			Debug.LogError("ToggleSelect. Trying to deselect a deselected card (" + CardName + "). Returning.\n" + Environment.StackTrace);
			return;
		}
		if (active && IsSelected)
		{
			Debug.LogError("ToggleSelect. Trying to select a selected card (" + CardName + "). Returning.\n" + Environment.StackTrace);
			return;
		}
		bool flag;
		if (!active && IsSelected)
		{
			flag = true;
		}
		else if (SaveData.Instance.Global.CurrentGameState == EGameState.Scenario)
		{
			flag = CanSelectInScenario();
		}
		else if (SaveData.Instance.Global.CurrentGameState == EGameState.Map)
		{
			CMapCharacter cMapCharacter = SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter s) => s.CharacterID == CharacterID);
			if (cMapCharacter != null)
			{
				flag = cMapCharacter.HandAbilityCardIDs.Count < cMapCharacter.MaxCards;
			}
			else
			{
				Debug.LogError("Unable to find Map Party for AbilityCardUI");
				flag = false;
			}
		}
		else if (CustomPartySetup.Instance != null)
		{
			CMapCharacter cMapCharacter2 = CustomPartySetup.Instance.CurrentCustomParty.AdventureMapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter s) => s.CharacterID == CharacterID);
			if (cMapCharacter2 != null)
			{
				flag = cMapCharacter2.HandAbilityCardIDs.Count < cMapCharacter2.MaxCards;
			}
			else
			{
				Debug.LogError("Unable to find Map Party for AbilityCardUI");
				flag = false;
			}
		}
		else
		{
			Debug.LogError("Unable to find Map Party for AbilityCardUI");
			flag = false;
		}
		if (!flag)
		{
			AudioControllerUtils.PlaySound(interactionAudioProfile.nonInteractableMouseDownAudioItem);
			onUnableToSelectAction?.Invoke(this);
			return;
		}
		int controllableID = 0;
		if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
		{
			CMapCharacter cMapCharacter3 = SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter s) => s.CharacterID == CharacterID);
			controllableID = cMapCharacter3.CharacterName.GetHashCode();
		}
		else
		{
			controllableID = CharacterClassManager.GetModelInstanceIDFromCharacterID(CharacterID);
		}
		if (!FFSNetwork.IsOnline || PlayerRegistry.MyPlayer.MyControllables.Any((NetworkControllable a) => a.ID == controllableID))
		{
			AudioControllerUtils.PlaySound(interactionAudioProfile.mouseDownAudioItem);
		}
		cardType = (active ? CardPileType.Round : unselectedCardType);
		SetDisplayMode(active ? CardPileType.Round : unselectedCardType, highlight);
		if (FFSNetwork.IsOnline && SaveData.Instance.Global.CurrentGameState == EGameState.Scenario && !playerActor.IsUnderMyControl && PhaseManager.CurrentPhase.Type == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			fullAbilityCard.DisplaySelected(isSelected: false);
		}
		else
		{
			fullAbilityCard.DisplaySelected(active);
		}
		if (IsLongRest)
		{
			if (PhaseManager.CurrentPhase.Type == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
			{
				playerActor.IsLongRestSelected = active;
			}
			else
			{
				playerActor.IsLongRestActionSelected = active;
			}
		}
		bool customFlag = playerActor != null && playerActor.CharacterClass != null && playerActor.CharacterClass.LongRest;
		if (active)
		{
			FFSNet.Console.Log("[ABILITY CARD UI]: Toggle Select, selecting card " + CardName, customFlag);
			if (onSelectedCallback != null)
			{
				onSelectedCallback(this, networkAction);
			}
		}
		else
		{
			FFSNet.Console.Log("[ABILITY CARD UI]: Toggle Select, deselecting a card " + CardName, customFlag);
			onDeselectedCallback(this, networkAction);
		}
		if (isHandUnderMyControl)
		{
			AbilityCardUI.CardSelectionStateChanged?.Invoke(this, IsSelected);
		}
	}

	public bool CanSelectInScenario()
	{
		if (CardsHandManager.Instance.CurrentHand.currentMode == CardHandMode.CardsSelection && playerActor.SelectingCardsForExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.None)
		{
			return IsLongRest || (playerActor.CharacterClass.RoundAbilityCards.Count < 2 && !IsGivenToOtherPlayer());
		}
		return true;
	}

	private bool IsGivenToOtherPlayer()
	{
		string characterID = playerActor.CharacterClass.CharacterID;
		string classID = abilityCard.ClassID;
		int cardID = abilityCard.ID;
		if (classID == characterID && characterID == "SawbonesID" && (cardID == 417 || cardID == 424))
		{
			return false;
		}
		if (classID == characterID)
		{
			return ScenarioManager.Scenario.PlayerActors.Exists((CPlayerActor it) => it.ID != playerActor.ID && (it.CharacterClass.GivenCards.Exists((CAbilityCard card) => card.ID == cardID) || it.CharacterClass.ActivatedAbilityCards.Exists((CAbilityCard card) => card.ID == cardID)));
		}
		return false;
	}

	public bool IsMatchingCard(int cardInstanceID)
	{
		if (!IsLongRest || cardInstanceID != 0)
		{
			CAbilityCard cAbilityCard = AbilityCard;
			if (cAbilityCard == null)
			{
				return false;
			}
			return cAbilityCard.CardInstanceID == cardInstanceID;
		}
		return true;
	}

	public int CompareTo(object obj)
	{
		AbilityCardUI abilityCardUI = (AbilityCardUI)obj;
		int num = ((cardType == CardPileType.Round && PhaseManager.CurrentPhase != null && PhaseManager.CurrentPhase.Type == CPhase.PhaseType.SelectAbilityCardsOrLongRest) ? CardPileType.Hand.ToSortedValue() : cardType.ToSortedValue());
		int num2 = ((abilityCardUI.cardType == CardPileType.Round && PhaseManager.CurrentPhase != null && PhaseManager.CurrentPhase.Type == CPhase.PhaseType.SelectAbilityCardsOrLongRest) ? CardPileType.Hand.ToSortedValue() : abilityCardUI.cardType.ToSortedValue());
		if (abilityCardUI.IsLongRest && !IsLongRest)
		{
			if (num < CardPileType.Discarded.ToSortedValue())
			{
				return -1;
			}
			return 1;
		}
		if (!abilityCardUI.IsLongRest && IsLongRest)
		{
			if (num2 < CardPileType.Discarded.ToSortedValue())
			{
				return 1;
			}
			return -1;
		}
		if (num2 < num)
		{
			return 1;
		}
		if (num2 > num)
		{
			return -1;
		}
		return abilityCard.Initiative.CompareTo(abilityCardUI.abilityCard.Initiative);
	}

	public void MakeAbilityCard(AbilityCardYMLData cardData, bool alwaysShow = false, bool isLongRest = false)
	{
		CardID = cardData.ID;
		CardName = cardData.Name;
		alwaysShowFullCard = alwaysShow;
		miniAbilityCard.MakeMiniCard(CardName, cardData, isLongRest);
		fullAbilityCard.MakeFullCard(CardName, cardData, fullCardDescriptionPadding, isLongRest);
	}

	public void MakeAbilityCardContinued()
	{
		fullAbilityCard.MakeFullCardContinued();
	}

	public void OnReturnedToPool()
	{
		SetUnfocused(unfocused: false);
		previewingFullCard = false;
		fullAbilityCard.BlockRaycasts(block: true);
		fullAbilityCard.gameObject.SetActive(value: true);
		fullAbilityCard.ClearEffects();
		fullAbilityCard.EnableSelectCard(enabled: false);
		fullAbilityCard.ResetHighlightInCanvas();
		cardType = CardPileType.Any;
		unselectedCardType = CardPileType.Any;
		SetValid(isValid: true);
		RemoveInteractabilityRelationship();
		fullAbilityCard.ResetInteractable();
		disableFullCard = false;
		fullAbilityCard.ResetConsumes();
		fullAbilityCard.HighlightAvailableConsumes(null);
		button.highlightScaleFactor = defaultHighlightScale;
		button.hoverMovement = defaultHoverMovement;
		miniAbilityCard.Clear();
		DisableNavigation();
		DisableEventDetection(disable: false);
		DeInit();
	}

	public void OnRemovedFromPool()
	{
	}

	public static CardPileType ECardPileToCardPileType(CBaseCard.ECardPile cardpile)
	{
		switch (cardpile)
		{
		case CBaseCard.ECardPile.Discarded:
			return CardPileType.Discarded;
		case CBaseCard.ECardPile.Round:
			return CardPileType.Round;
		case CBaseCard.ECardPile.Hand:
			return CardPileType.Hand;
		case CBaseCard.ECardPile.Activated:
			return CardPileType.Active;
		case CBaseCard.ECardPile.Lost:
			return CardPileType.Lost;
		case CBaseCard.ECardPile.PermanentlyLost:
			return CardPileType.Permalost;
		case CBaseCard.ECardPile.None:
			return CardPileType.Unselected;
		default:
			Debug.LogError("Invalid conversion for card pile " + cardpile.ToString() + " is not valid");
			return CardPileType.None;
		}
	}

	public static CBaseCard.ECardPile CardPileTypeToECardPile(CardPileType cardPileType)
	{
		switch (cardPileType)
		{
		case CardPileType.Discarded:
			return CBaseCard.ECardPile.Discarded;
		case CardPileType.Round:
			return CBaseCard.ECardPile.Round;
		case CardPileType.Hand:
			return CBaseCard.ECardPile.Hand;
		case CardPileType.Active:
			return CBaseCard.ECardPile.Activated;
		case CardPileType.Lost:
			return CBaseCard.ECardPile.Lost;
		case CardPileType.Permalost:
			return CBaseCard.ECardPile.PermanentlyLost;
		case CardPileType.Unselected:
			return CBaseCard.ECardPile.None;
		case CardPileType.ExtraTurn:
			return CBaseCard.ECardPile.Round;
		default:
			Debug.LogError("Invalid conversion for card pile " + cardPileType.ToString() + " is not valid");
			return CBaseCard.ECardPile.None;
		}
	}

	protected override void OnLanguageChanged()
	{
		miniAbilityCard.SetCardName(CardName);
		fullAbilityCard.SetCardName(CardName);
	}

	public void SetUnfocused(bool unfocused)
	{
		miniAbilityCard.SetUnfocused(unfocused);
		fullAbilityCard.SetUnfocused(unfocused);
	}

	public void SetParent(Transform newParentTransform)
	{
		if (!(base.transform == newParentTransform))
		{
			base.transform.SetParent(newParentTransform);
		}
	}

	public void ChangeDefaultHighlightScale(float value)
	{
		defaultHighlightScale = value;
	}

	public void ChangeDefaultHoverMovement(Vector3 vector)
	{
		defaultHoverMovement = vector;
	}

	public void MarkAsInFurtherAbilityPanel(bool value)
	{
		isInFurtherAbilityPanel = value;
	}
}
