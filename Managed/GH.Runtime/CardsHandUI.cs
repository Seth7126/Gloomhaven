#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AsmodeeNet.Foundation;
using Code.State;
using FFSNet;
using GLOOM;
using JetBrains.Annotations;
using Photon.Bolt;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.ScenarioStates;
using Script.GUI.SMNavigation.States.SpecialStates;
using Script.GUI.SMNavigation.Utils;
using SharedLibrary.SimpleLog;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class CardsHandUI : MonoBehaviour
{
	public enum CardActionsCommand
	{
		NONE,
		RESET,
		FORCE_RESET
	}

	private class CardState
	{
		public bool isActive;

		public int order;

		public Transform parent;

		public Vector3 position;

		public CardState(AbilityCardUI abilityCard)
		{
			Transform transform = abilityCard.transform;
			isActive = abilityCard.gameObject.activeSelf;
			order = transform.GetSiblingIndex();
			parent = transform.parent;
			position = transform.position;
		}
	}

	private enum EUpdateCardViewMode
	{
		None,
		OnyActive,
		Refresh
	}

	[SerializeField]
	private Transform abilityCardsHolder;

	[SerializeField]
	private UIGivenCardsGroup givenCardsGroup;

	[SerializeField]
	private CardHandHeader burntHeader;

	[SerializeField]
	private CardHandHeader discardedHeader;

	[SerializeField]
	private GameObject shortRestPrefab;

	[SerializeField]
	private GameObject longRestCardPrefab;

	[SerializeField]
	private VerticalLayoutGroup verticalLayoutGroup;

	[SerializeField]
	private LayoutElementExtended layoutElement;

	[SerializeField]
	private ItemsUI itemsUI;

	[SerializeField]
	private ModifiersDisplayController modifiersController;

	public ExtendedButton modifiersSelectable;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[Header("Display settings")]
	[SerializeField]
	private Vector2 separatorOffset;

	[Header("Shuffle animation settings")]
	[SerializeField]
	private LeanTweenType easeType;

	[SerializeField]
	private float cardHighlightTime;

	[SerializeField]
	private float lostCardMoveTime;

	[SerializeField]
	private float cardsReorderingOffsetTime;

	[SerializeField]
	private float discardedCardsTransitionTime;

	[SerializeField]
	private float discardedCardsMoveTime;

	[SerializeField]
	private float postAnimationWaitTime;

	[SerializeField]
	private ScrollRect scrollRect;

	private CPlayerActor playerActor;

	private List<AbilityCardUI> cardsUI = new List<AbilityCardUI>();

	private List<AbilityCardUI> selectedCardsUI = new List<AbilityCardUI>();

	private List<AbilityCardUI> cardsInPopUp = new List<AbilityCardUI>();

	private bool cardsSpawned;

	private bool useCurrentRoundCards;

	private CardHandMode previousMode;

	private ShortRest shortRest;

	private bool shortRested;

	private Transform highlightTransform;

	private int shortRestLostCardID = -1;

	private int shortRestAlternateLostCardID = -1;

	private Dictionary<AbilityCardUI, CardState> cardsStateBeforePreview = new Dictionary<AbilityCardUI, CardState>();

	private List<IUiNavigationElement> _navigationElementsBuffer = new List<IUiNavigationElement>();

	private List<AbilityCardUI> _lockedCards = new List<AbilityCardUI>();

	private AbilityCardUI _currentCard;

	private CardsActionControlller cardsActionController;

	private bool dialogPopupShown;

	private bool isPreviewingCards;

	private CardHandMode cardHandMode;

	private bool enableLongCardSelection;

	private CardPileType filterType;

	private List<CardPileType> selectableCardTypes;

	private int maxCardsSelected;

	private bool fadeUnselectableCards;

	private bool highlightSelectableCards;

	private CardActionsCommand resetCardActionsPhase;

	private bool forceUseCurrentRoundCards;

	private Action<AbilityCardUI> overrideCardCallback;

	private Func<CAbilityCard, bool> cardFilter;

	private UiNavigationGroup _navigationGroup;

	private IStateFilter _stateFilter = new StateFilterByType(typeof(DeckScenarioState)).InverseFilter();

	public UnityEvent OnShow;

	public UnityEvent OnHide;

	private CAbilityCard _shortRestedCard;

	private bool animatedLosingCard;

	private Action onAnimateCardLostCallback;

	public float animationSpeed;

	private List<LTDescr> animations = new List<LTDescr>();

	private const string DebugCancel = "CancelAnimateCardLost";

	private Coroutine selectFirstElementCoroutine;

	public CPlayerActor PlayerActor => playerActor;

	public bool MaxCardSelected => SelectedCards.Count >= MaxSelectedCards;

	public ItemsUI ItemsUI => itemsUI;

	public CardHandMode currentMode { get; private set; }

	public bool IsFullCardPreviewShowing { get; private set; }

	public AbilityCardUI CurrentCard => _currentCard;

	public StartRoundCardsToken CachedStartRoundToken { get; set; }

	public List<AbilityCardUI> SelectedCards => selectedCardsUI;

	public int MaxSelectedCards => maxCardsSelected;

	public bool AnimatingLostCards { get; private set; }

	public UiNavigationGroup NavigationGroup
	{
		get
		{
			if (_navigationGroup == null)
			{
				_navigationGroup = GetComponent<UiNavigationGroup>();
			}
			return _navigationGroup;
		}
	}

	public bool IsPreviewingCards => isPreviewingCards;

	public bool IsShown => base.gameObject.gameObject.activeSelf;

	public bool IsImprovedLongResting
	{
		get
		{
			if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest && playerActor.CharacterClass.ImprovedShortRest && playerActor.CharacterClass.LongRest)
			{
				if (currentMode != CardHandMode.LoseCard)
				{
					return currentMode == CardHandMode.ActionSelection;
				}
				return true;
			}
			return false;
		}
	}

	public CAbilityCard ShortRestedCard => _shortRestedCard;

	public bool IsInteractable => canvasGroup.interactable;

	public void UpdateElements()
	{
		try
		{
			foreach (AbilityCardUI item in selectedCardsUI)
			{
				item.UpdateElements();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.UpdateElements().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void ToggleAttackModifiers(bool active)
	{
		try
		{
			if (active)
			{
				if (PlayerActor == null || !modifiersController.isActiveAndEnabled)
				{
					Debug.LogError("Trying to show modifiers for inactive actor", modifiersController.gameObject);
				}
				else
				{
					modifiersController.Display(PlayerActor.CharacterClass, !FFSNetwork.IsOnline || PlayerActor.IsUnderMyControl);
				}
			}
			else
			{
				modifiersController.Hide();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.ToggleAttackModifiers().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00002", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void ToggleAttackModifiers()
	{
		ToggleAttackModifiers(!modifiersController.IsOpen());
	}

	public void ToggleFullCardsPreview(bool active, bool openByKey = false)
	{
		try
		{
			if (animatedLosingCard)
			{
				active = false;
			}
			if (isPreviewingCards == active)
			{
				return;
			}
			isPreviewingCards = active;
			if (active)
			{
				IsFullCardPreviewShowing = true;
				dialogPopupShown = UIManager.Instance.dialogPopup.IsOpen();
				UIManager.Instance.dialogPopup.Hide(cleanContent: false);
				cardsStateBeforePreview.Clear();
				foreach (AbilityCardUI item in cardsUI)
				{
					cardsStateBeforePreview[item] = new CardState(item);
					item.ToggleFullCardPreview(isHighlighted: false);
				}
				foreach (AbilityCardUI item2 in cardsUI)
				{
					if (!item2.IsLongRest && item2.CardType != CardPileType.Unselected)
					{
						item2.fullAbilityCard.HighlightAvailableConsumes(InfusionBoardUI.Instance?.GetAvailableElements());
						item2.ToggleFullCard(active: true);
						item2.ToggleFullSelectionCard(active: true);
						item2.ToggleFullCardCanvasSorting(active: false);
						item2.gameObject.SetActive(value: true);
						item2.SetParent(Singleton<FullCardHandViewer>.Instance.CardContainer);
					}
				}
				Singleton<FullCardHandViewer>.Instance.Show(delegate
				{
					ToggleFullCardsPreview(active: false);
				}, openByKey);
				InteractabilityHighlightCanvas.s_Instance?.ClearHighlights();
				return;
			}
			Singleton<FullCardHandViewer>.Instance.Hide();
			IsFullCardPreviewShowing = false;
			int num = 0;
			foreach (AbilityCardUI item3 in cardsUI)
			{
				if (!item3.IsLongRest && item3.CardType != CardPileType.Unselected)
				{
					item3.gameObject.SetActive(cardsStateBeforePreview[item3].isActive);
					item3.SetParent(cardsStateBeforePreview[item3].parent);
					item3.transform.position = cardsStateBeforePreview[item3].position;
					item3.ToggleFullSelectionCard(active: false);
					item3.ToggleFullCardCanvasSorting(active: true);
					if (item3.Mode != CardHandMode.ActionSelection && item3.Mode != CardHandMode.Preview)
					{
						item3.ToggleFullCard(active: false);
					}
					num++;
				}
			}
			num = 0;
			foreach (AbilityCardUI item4 in cardsUI)
			{
				item4.transform.SetSiblingIndex(cardsStateBeforePreview[item4].order);
				num++;
			}
			if (NavigationGroup != null)
			{
				NavigationGroup.UpdateElementsSortingOrder();
			}
			cardsStateBeforePreview.Clear();
			UpdateEffects();
			if (dialogPopupShown)
			{
				UIManager.Instance.dialogPopup.ShowLoadedContent();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.ToggleFullCardsPreview().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00003", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		if (CoreApplication.IsQuitting)
		{
			return;
		}
		foreach (AbilityCardUI item in cardsUI)
		{
			ObjectPool.RecycleCard(item.CardID, ObjectPool.ECardType.Ability, item.gameObject);
		}
		if (UIManager.Instance != null)
		{
			DialogPopup dialogPopup = UIManager.Instance.dialogPopup;
			dialogPopup.OnHiding = (Action)Delegate.Remove(dialogPopup.OnHiding, new Action(OnHidingFullCardsPopup));
		}
		InputManager.RequestEnableInput(this, new KeyAction[4]
		{
			KeyAction.DISPLAY_CARDS_HERO_1,
			KeyAction.DISPLAY_CARDS_HERO_2,
			KeyAction.DISPLAY_CARDS_HERO_3,
			KeyAction.DISPLAY_CARDS_HERO_4
		});
		OnClearLeanTweens();
	}

	public void UpdateOriginalExtraTurnCards(CPlayerActor playerActor)
	{
		foreach (AbilityCardUI item in cardsUI)
		{
			if (playerActor.CharacterClass.RoundCardsSelectedInCardSelection.Contains(item.AbilityCard))
			{
				item.fullAbilityCard.TryPlayBurnAnimation(CBaseCard.ActionType.DefaultAttackAction);
			}
		}
	}

	public void Init(CPlayerActor playerActor, Transform highlightTransform, CardsActionControlller cardsActionController)
	{
		try
		{
			this.cardsActionController = cardsActionController;
			this.playerActor = playerActor;
			this.highlightTransform = highlightTransform;
			IsFullCardPreviewShowing = false;
			if (!cardsSpawned)
			{
				CoroutineHelper.instance.StartCoroutine(SpawnCards(playerActor.CharacterClass.SelectedAbilityCards.Concat(playerActor.CharacterClass.ActivatedAbilityCards).Distinct().ToList(), playerActor.CharacterClass.UnselectedAbilityCards.FindAll((CAbilityCard it) => it.Level <= playerActor.Level), playerActor.CharacterClass.GivenCards, longRestCardPrefab));
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.Init().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00004", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void Toggle(bool active)
	{
		scrollRect.gameObject.SetActive(active);
	}

	public void Hide()
	{
		try
		{
			modifiersController.Hide(immendiately: true);
			ToggleFullCardsPreview(active: false);
			foreach (AbilityCardUI item in cardsUI)
			{
				item.fullAbilityCard.ToggleSelect(active: false, CBaseCard.ActionType.BottomAction);
				item.fullAbilityCard.ToggleSelect(active: false, CBaseCard.ActionType.TopAction);
				item.SetParent(givenCardsGroup.IsGivenCard(item) ? givenCardsGroup.Container : abilityCardsHolder);
			}
			ShowOrHideInternal(show: false);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.Hide().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00005", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void Show(Transform holder = null)
	{
		try
		{
			ToggleFullCardsPreview(active: false);
			if (holder != null)
			{
				foreach (AbilityCardUI item in cardsUI)
				{
					item.SetParent(holder);
				}
			}
			ShowOrHideInternal(show: true);
			itemsUI.gameObject.SetActive(value: true);
			UpdateShortRest();
			UpdateEffects();
			if (FFSNetwork.IsOnline && !playerActor.IsUnderMyControl && shortRested && PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
			{
				Debug.Log("Show hand");
				UpdateView(currentMode, CardPileType.Any, new List<CardPileType>
				{
					CardPileType.Hand,
					CardPileType.Round
				}, 2);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.Show().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00006", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void ShowOrHideInternal(bool show)
	{
		base.gameObject.SetActive(show);
		if (show)
		{
			OnShow.Invoke();
		}
		else
		{
			OnHide.Invoke();
		}
	}

	public void ToggleHighlight(CardPileType highlightCardType, bool active, bool keepLongRest = false, bool keepShortRest = false)
	{
		ToggleHighlight(new List<CardPileType> { highlightCardType }, active, keepLongRest, keepShortRest);
	}

	public void ToggleHighlight(List<CardPileType> highlightCardTypes, bool active, bool keepLongRest = false, bool keepShortRest = false)
	{
		try
		{
			if (active)
			{
				shortRest.Button.ToggleFade(!keepShortRest || shortRest.Button.IsFaded);
			}
			else
			{
				UpdateShortRest();
			}
			bool flag = PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest && playerActor.CharacterClass.LongRest && !playerActor.CharacterClass.HasLongRested;
			foreach (AbilityCardUI item in cardsUI)
			{
				item.Highlight(active, highlightCardTypes, flag && item.CardType == CardPileType.Discarded, keepLongRest || (flag && item.CardType == CardPileType.Discarded), keepShortRest);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.ToggleHighlight().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00007", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void ResetShortRestUIFlag()
	{
		shortRested = false;
	}

	public void ResetViewToCachedSettings()
	{
		UpdateView(cardHandMode, enableLongCardSelection, filterType, selectableCardTypes, maxCardsSelected, fadeUnselectableCards, highlightSelectableCards, resetCardActionsPhase, forceUseCurrentRoundCards, overrideCardCallback, cardFilter);
	}

	public void UpdateView(CardHandMode mode, CardPileType filterType = CardPileType.Any, List<CardPileType> selectableCardTypes = null, int maxCardsSelected = 0, bool fadeUnselectableCards = false, bool highlightSelectableCards = false, CardActionsCommand resetCardActions = CardActionsCommand.RESET, bool forceUseCurrentRoundCards = false, Action<AbilityCardUI> overrideCardCallback = null, Func<CAbilityCard, bool> cardFilter = null)
	{
		UpdateView(mode, !playerActor.CharacterClass.LongRest, filterType, selectableCardTypes, maxCardsSelected, fadeUnselectableCards, highlightSelectableCards, resetCardActions, forceUseCurrentRoundCards, overrideCardCallback, cardFilter);
	}

	public void UpdateView(CardHandMode mode, bool enableLongCardSelection, CardPileType filterType = CardPileType.Any, List<CardPileType> selectableCardTypes = null, int maxCardsSelected = 0, bool fadeUnselectableCards = false, bool highlightSelectableCards = false, CardActionsCommand resetCardActions = CardActionsCommand.RESET, bool forceUseCurrentRoundCards = false, Action<AbilityCardUI> overrideCardCallback = null, Func<CAbilityCard, bool> cardFilter = null)
	{
		try
		{
			selectedCardsUI.Clear();
			cardHandMode = mode;
			this.enableLongCardSelection = enableLongCardSelection;
			this.filterType = filterType;
			this.selectableCardTypes = selectableCardTypes;
			this.maxCardsSelected = maxCardsSelected;
			this.fadeUnselectableCards = fadeUnselectableCards;
			this.highlightSelectableCards = highlightSelectableCards;
			resetCardActionsPhase = resetCardActions;
			this.forceUseCurrentRoundCards = forceUseCurrentRoundCards;
			this.overrideCardCallback = overrideCardCallback;
			this.cardFilter = cardFilter;
			useCurrentRoundCards = mode == CardHandMode.RecoverLostCard || mode == CardHandMode.RecoverDiscardedCard || mode == CardHandMode.DiscardCard || PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest || forceUseCurrentRoundCards;
			UpdateCards(filterType, EUpdateCardViewMode.None);
			SetMode(mode, filterType, selectableCardTypes, fadeUnselectableCards, highlightSelectableCards, playerActor.CharacterClass.DiscardedAbilityCards.Count > 1 && PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest && enableLongCardSelection, resetCardActions, cardFilter);
			UpdateCards(filterType, EUpdateCardViewMode.Refresh);
			if (mode == CardHandMode.ActionSelection)
			{
				burntHeader.Hide();
				discardedHeader.Hide();
				SortCards(reorderPositions: false);
				foreach (AbilityCardUI item in cardsUI)
				{
					overrideCardCallback?.Invoke(item);
					item.SetParent(highlightTransform);
					item.fullAbilityCard.IsActionSelection = true;
					item.fullAbilityCard.UpdateView(UISettings.FullCardViewInActionSelect);
					if (cardsStateBeforePreview.ContainsKey(item))
					{
						cardsStateBeforePreview[item].order = item.transform.GetSiblingIndex();
						cardsStateBeforePreview[item].parent = highlightTransform;
					}
				}
			}
			else
			{
				foreach (AbilityCardUI item2 in cardsUI)
				{
					overrideCardCallback?.Invoke(item2);
					Transform parent = (givenCardsGroup.IsGivenCard(item2) ? givenCardsGroup.Container : abilityCardsHolder);
					item2.SetParent(parent);
					item2.fullAbilityCard.UpdateView(UISettings.DefaultFullCardViewSettings);
					item2.fullAbilityCard.IsActionSelection = false;
					if (cardsStateBeforePreview.ContainsKey(item2))
					{
						cardsStateBeforePreview[item2].parent = parent;
					}
				}
				SortCards();
			}
			UpdateEffects();
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.UpdateView().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00008", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void UpdateInitiative()
	{
		try
		{
			if (playerActor.SelectingCardsForExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.None)
			{
				playerActor.CharacterClass.SetInitiativeAbilityCard((selectedCardsUI.Count > 0) ? selectedCardsUI[selectedCardsUI.Count - 1].AbilityCard : null);
				playerActor.CharacterClass.SetSubInitiativeAbilityCard((selectedCardsUI.Count > 0) ? selectedCardsUI[0].AbilityCard : null);
			}
			InitiativeTrack.Instance.UpdateActors();
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.UpdateInitiative().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00009", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void UpdateEffects()
	{
		try
		{
			bool flag = !FFSNetwork.IsOnline || playerActor.IsUnderMyControl || PhaseManager.PhaseType != CPhase.PhaseType.SelectAbilityCardsOrLongRest;
			if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest && playerActor.CharacterClass.RoundAbilityCards.Count >= 1 && flag)
			{
				foreach (AbilityCardUI item in cardsUI)
				{
					item.PlayEffect((playerActor.CharacterClass.RoundAbilityCards[0] == item.AbilityCard) ? InitiativeTrackActorAvatar.InitiativeEffects.Active : InitiativeTrackActorAvatar.InitiativeEffects.None);
				}
				return;
			}
			if (playerActor.CharacterClass.LongRest && flag)
			{
				foreach (AbilityCardUI item2 in cardsUI)
				{
					item2.PlayEffect(item2.IsLongRest ? InitiativeTrackActorAvatar.InitiativeEffects.Active : InitiativeTrackActorAvatar.InitiativeEffects.None);
				}
				return;
			}
			if (playerActor.TakingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater && flag)
			{
				foreach (AbilityCardUI item3 in cardsUI)
				{
					item3.PlayEffect((playerActor.CharacterClass.ExtraTurnCardsSelectedInCardSelectionStack.Count > 0 && playerActor.CharacterClass.ExtraTurnInitiativeAbilityCard == item3.AbilityCard) ? InitiativeTrackActorAvatar.InitiativeEffects.Active : InitiativeTrackActorAvatar.InitiativeEffects.None);
				}
				return;
			}
			foreach (AbilityCardUI item4 in cardsUI)
			{
				item4.PlayEffect(InitiativeTrackActorAvatar.InitiativeEffects.None);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.UpdateEffects().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00010", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void UpdateShortRest()
	{
		try
		{
			if (!(shortRest == null))
			{
				if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest && !IsImprovedLongResting)
				{
					shortRest.Show(playerActor, shortRest.IsSelected);
				}
				else
				{
					shortRest.ResetSelection();
					shortRest.Hide();
				}
				shortRest.SetInteractable(!shortRested && playerActor.CharacterClass.DiscardedAbilityCards.Count > 1);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.UpdateShortRest().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00011", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void PerformShortRest(CPlayerActor playerActor)
	{
		try
		{
			AbilityCardUI longRestCard = GetCard(-1);
			if (longRestCard.IsSelected)
			{
				longRestCard.ToggleSelect(active: false);
			}
			if (playerActor.CharacterClass.ImprovedShortRest)
			{
				playerActor.CharacterClass.LongRest = true;
				Singleton<HelpBox>.Instance.ShowTranslated(LocalizationManager.GetTranslation("GUI_TOOLTIP_LONG_REST"), LocalizationManager.GetTranslation("GUI_TOOLTIP_HELPBOX IMPROVED_SHORT_REST_TITLE"));
				if (FFSNetwork.IsOnline)
				{
					Singleton<UIReadyToggle>.Instance?.ToggleVisibility(visible: false);
				}
				CardsHandManager.Instance.SetControllerIndicatorsActive(active: false);
				CardsHandManager.Instance.ShowLongRestConfirmation(playerActor, delegate(bool confirmed)
				{
					Choreographer.s_Choreographer.readyButton.Toggle(confirmed, ReadyButton.EButtonState.EREADYBUTTONNA, null, hideOnClick: true, glowingEffect: false, interactable: true, disregardTurnControlForInteractability: true, haltActionProcessorIfDeactivated: false);
				});
				Choreographer.s_Choreographer.readyButton.Toggle(active: false, ReadyButton.EButtonState.EREADYBUTTONIMPROVEDSHORTREST, LocalizationManager.GetTranslation("GUI_PERFORM_LONG_REST"), hideOnClick: true, glowingEffect: false, interactable: true, disregardTurnControlForInteractability: false, haltActionProcessorIfDeactivated: false);
				Choreographer.s_Choreographer.readyButton.QueueAlternativeAction(delegate
				{
					ActorBehaviour.GetActorBehaviour(Choreographer.s_Choreographer.FindClientPlayer(playerActor)).m_WorldspacePanelUI.OnSelectingHealFocus(2);
					CardsHandManager.Instance.Show(playerActor, CardHandMode.LoseCard, CardPileType.Any, CardPileType.Discarded, 1, fadeUnselectableCards: true, highlightSelectableCards: true);
					Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.LoseCard);
				});
				return;
			}
			if (shortRestLostCardID == -1)
			{
				shortRestLostCardID = ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(playerActor.CharacterClass.DiscardedAbilityCards.Count);
			}
			CAbilityCard lostCard = playerActor.CharacterClass.DiscardedAbilityCards[shortRestLostCardID];
			if (LevelEventsController.s_EventsControllerActive)
			{
				Singleton<LevelEventsController>.Instance?.RunActionIfShortRestDataPending(delegate(string cardData)
				{
					if (playerActor.CharacterClass.DiscardedAbilityCards.Any((CAbilityCard ac) => ac.Name == cardData))
					{
						lostCard = playerActor.CharacterClass.DiscardedAbilityCards.Single((CAbilityCard ac) => ac.Name == cardData);
					}
				});
			}
			_shortRestedCard = lostCard;
			AbilityCardUI cardUI = GetCardUI(lostCard);
			cardUI.LostAnimation(active: true, 0.5f);
			string text = string.Format(LocalizationManager.GetTranslation("GUI_LOSE_CARD"), cardUI.fullAbilityCard.Title);
			string translation = LocalizationManager.GetTranslation("GUI_REDRAW_CARD");
			DeselectAllCards();
			bool wasSelectable = cardUI.IsSelectable;
			cardUI.SetSelectable(isOn: false, isHoverable: false);
			foreach (AbilityCardUI item in cardsUI.Where((AbilityCardUI it) => it != cardUI))
			{
				item.SetValid(isValid: false);
			}
			shortRest.Hide();
			longRestCard.gameObject.SetActive(value: false);
			CardsHandManager.Instance.ShowTabs(show: false);
			Choreographer.s_Choreographer.readyButton.ToggleVisibility(active: false);
			if (FFSNetwork.IsOnline)
			{
				Singleton<UIReadyToggle>.Instance.ToggleVisibility(visible: false);
			}
			bool canPreviewAllDecks = CardsHandManager.Instance.IsFullDeckPreviewAllowed;
			CardsHandManager.Instance.IsFullDeckPreviewAllowed = false;
			InputManager.RequestDisableInput(this, new KeyAction[4]
			{
				KeyAction.DISPLAY_CARDS_HERO_1,
				KeyAction.DISPLAY_CARDS_HERO_2,
				KeyAction.DISPLAY_CARDS_HERO_3,
				KeyAction.DISPLAY_CARDS_HERO_4
			});
			Action onSelectedOption = delegate
			{
				cardUI.SetSelectable(wasSelectable);
				foreach (AbilityCardUI item2 in cardsUI.Where((AbilityCardUI it) => it != cardUI))
				{
					item2.SetValid(isValid: true);
				}
			};
			Action onFinish = delegate
			{
				longRestCard.gameObject.SetActive(value: true);
				CardsHandManager.Instance.ShowTabs(show: true);
				CardsHandManager.Instance.IsFullDeckPreviewAllowed = canPreviewAllDecks;
				if (FFSNetwork.IsOnline)
				{
					Singleton<UIReadyToggle>.Instance.ToggleVisibility(visible: true);
				}
				else
				{
					Choreographer.s_Choreographer.readyButton.ToggleVisibility(!InputManager.GamePadInUse);
				}
				InputManager.RequestEnableInput(this, new KeyAction[4]
				{
					KeyAction.DISPLAY_CARDS_HERO_1,
					KeyAction.DISPLAY_CARDS_HERO_2,
					KeyAction.DISPLAY_CARDS_HERO_3,
					KeyAction.DISPLAY_CARDS_HERO_4
				});
			};
			if (playerActor.Health > 1)
			{
				Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.Burn);
				UIManager.Instance.dialogPopup.Show(GetCardUI(lostCard).fullAbilityCard.gameObject, new DialogOption[2]
				{
					new DialogOption(text, KeyAction.UI_SUBMIT, delegate
					{
						Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.RoundStart);
						onSelectedOption();
						onFinish();
						FinalizeShortRest(playerActor, cardUI, loseHp: false);
					}, delegate
					{
						cardUI.fullAbilityCard.cardEffects.ToggleEffect(active: false, CardEffects.FXTask.BurnCard);
					}, delegate
					{
						cardUI.fullAbilityCard.RefreshPile();
					}),
					new DialogOption(translation, KeyAction.UI_CANCEL, delegate
					{
						onSelectedOption();
						PerformFinalShortRest(playerActor, cardUI, onFinish);
					})
				}, allowHide: false, 1);
			}
			else
			{
				UIManager.Instance.dialogPopup.Show(GetCardUI(lostCard).fullAbilityCard.gameObject, new DialogOption[1]
				{
					new DialogOption(text, KeyAction.CONFIRM_ACTION_BUTTON, delegate
					{
						Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.RoundStart);
						onSelectedOption();
						onFinish();
						FinalizeShortRest(playerActor, cardUI, loseHp: false);
					}, delegate
					{
						cardUI.fullAbilityCard.cardEffects.ToggleEffect(active: false, CardEffects.FXTask.BurnCard);
					}, delegate
					{
						cardUI.fullAbilityCard.RefreshPile();
					})
				});
			}
			UIManager.Instance.dialogPopup.transform.SetParent(CardsHandManager.Instance.transform);
			UIManager.Instance.dialogPopup.transform.SetAsFirstSibling();
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.PerformShortRest().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00012", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void PerformFinalShortRest(CPlayerActor playerActor, AbilityCardUI firstCard, Action onConfirmed = null)
	{
		try
		{
			firstCard.LostAnimation(active: false, 0.5f);
			List<CAbilityCard> list = new List<CAbilityCard>(playerActor.CharacterClass.DiscardedAbilityCards);
			list.Remove(firstCard.AbilityCard);
			if (shortRestAlternateLostCardID == -1)
			{
				shortRestAlternateLostCardID = ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(list.Count);
			}
			CAbilityCard lostCard = list[shortRestAlternateLostCardID];
			_shortRestedCard = lostCard;
			if (LevelEventsController.s_EventsControllerActive)
			{
				Singleton<LevelEventsController>.Instance?.RunActionIfShortRestDataPending(delegate(string cardData)
				{
					if (playerActor.CharacterClass.DiscardedAbilityCards.Any((CAbilityCard ac) => ac.Name == cardData))
					{
						lostCard = playerActor.CharacterClass.DiscardedAbilityCards.Single((CAbilityCard ac) => ac.Name == cardData);
					}
				});
			}
			AbilityCardUI cardUI = GetCardUI(lostCard);
			cardUI.LostAnimation(active: true, 0.5f);
			string text = string.Format(LocalizationManager.GetTranslation("GUI_LOSE_CARD"), cardUI.fullAbilityCard.Title);
			bool wasSelectable = cardUI.IsSelectable;
			cardUI.SetSelectable(isOn: false, isHoverable: false);
			foreach (AbilityCardUI item in cardsUI.Where((AbilityCardUI it) => it != cardUI))
			{
				item.SetValid(isValid: false);
			}
			UIManager.Instance.dialogPopup.Show(cardUI.fullAbilityCard.gameObject, new DialogOption[1]
			{
				new DialogOption(text, KeyAction.CONFIRM_ACTION_BUTTON, delegate
				{
					Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.Burn);
					cardUI.SetSelectable(wasSelectable);
					foreach (AbilityCardUI item2 in cardsUI.Where((AbilityCardUI it) => it != cardUI))
					{
						item2.SetValid(isValid: true);
					}
					onConfirmed?.Invoke();
					FinalizeShortRest(playerActor, cardUI, loseHp: true);
					Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.RoundStart);
				}, delegate
				{
					cardUI.fullAbilityCard.cardEffects.ToggleEffect(active: false, CardEffects.FXTask.BurnCard);
				}, delegate
				{
					cardUI.fullAbilityCard.RefreshPile();
				})
			});
			UIManager.Instance.dialogPopup.transform.SetParent(CardsHandManager.Instance.transform);
			UIManager.Instance.dialogPopup.transform.SetAsFirstSibling();
			UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.ShortRestChoseToRedraw));
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.PerformFinalShortRest().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00013", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void FinalizeShortRest(CPlayerActor playerActor, AbilityCardUI cardUI, bool loseHp, bool networkActionIfOnline = true, bool fromStateUpdate = false)
	{
		try
		{
			shortRestLostCardID = -1;
			shortRestAlternateLostCardID = -1;
			if (!FFSNetwork.IsOnline || playerActor.IsUnderMyControl)
			{
				ShowOrHideInternal(show: true);
				cardUI.gameObject.SetActive(value: true);
				if (cardsStateBeforePreview.ContainsKey(cardUI))
				{
					cardsStateBeforePreview[cardUI].isActive = true;
				}
				cardUI.fullAbilityCard.cardEffects.ToggleEffect(active: true, CardEffects.FXTask.BurnCard);
			}
			ScenarioRuleClient.ShortRestPlayer(playerActor, cardUI.AbilityCard, loseHp, !networkActionIfOnline, fromStateUpdate);
			shortRested = true;
			UpdateShortRest();
			if (base.gameObject.activeSelf)
			{
				FFSNet.Console.LogInfo("Starting Short rest card animation.");
				StartCoroutine(AnimateCardsLost(new List<AbilityCardUI> { cardUI }, delegate
				{
					UpdateView(currentMode, CardPileType.Any, new List<CardPileType>
					{
						CardPileType.Hand,
						CardPileType.Round,
						CardPileType.Active
					}, 2);
				}));
			}
			_shortRestedCard = null;
			UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.ShortRestChoseToBurn));
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.FinalizeShortRest().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00014", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private AbilityCardUI GetCardUI(CAbilityCard card)
	{
		try
		{
			foreach (AbilityCardUI item in cardsUI)
			{
				if (item.AbilityCard == card)
				{
					return item;
				}
			}
			return null;
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.GetCardUI().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00015", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			return null;
		}
	}

	private IEnumerator AnimateCardsLost(List<AbilityCardUI> selectedCards, Action onCompleteCallback = null)
	{
		if (!base.gameObject.activeInHierarchy)
		{
			InstantCardLost(selectedCards);
			onCompleteCallback?.Invoke();
			yield break;
		}
		AnimatingLostCards = true;
		List<AbilityCardUI> tempSelectedCards = selectedCards.ToList();
		for (int i = 0; i < cardsUI.Count; i++)
		{
			cardsUI[i].MiniAbilityCard.DisableGlowSelected();
			cardsUI[i].MiniAbilityCard.DisplayIndicator(active: false);
		}
		onAnimateCardLostCallback = onCompleteCallback;
		animatedLosingCard = true;
		Singleton<UINavigation>.Instance.StateMachine.Enter(SpecialStateTag.Lock);
		UIManager.Instance.RequestToggleLockUI(active: true, base.gameObject);
		AudioController.Play("PlaySound_CardUI_BurnedCard", base.transform, null, attachToParent: false);
		if (isPreviewingCards)
		{
			ToggleFullCardsPreview(active: false);
			yield return new WaitForEndOfFrame();
		}
		UpdateCards(CardPileType.Any, EUpdateCardViewMode.None);
		foreach (AbilityCardUI item in tempSelectedCards)
		{
			CardHandHeader header = GetHeader(item);
			if (header != null)
			{
				header.Show();
			}
		}
		yield return new WaitForEndOfFrame();
		layoutElement.minHeight = (verticalLayoutGroup.transform as RectTransform).rect.height;
		verticalLayoutGroup.enabled = false;
		givenCardsGroup.PrepareLoseCard();
		SortCards(reorderPositions: true, toggleHeader: false);
		foreach (AbilityCardUI item2 in tempSelectedCards)
		{
			item2.LostAnimation(active: true, cardHighlightTime * animationSpeed);
		}
		yield return new WaitForSecondsRealtime(cardHighlightTime * animationSpeed);
		RefreshHeaders();
		List<RectTransform> scrollElements = (from it in GetOrderedCardScrollElements()
			where it.gameObject.activeSelf
			select it).ToList();
		float num = -verticalLayoutGroup.padding.top;
		List<float> positionY = new List<float>();
		for (int num2 = 0; num2 < scrollElements.Count; num2++)
		{
			positionY.Add(num - scrollElements[num2].sizeDelta.y * scrollElements[num2].pivot.y);
			num -= scrollElements[num2].sizeDelta.y + verticalLayoutGroup.spacing;
		}
		foreach (Tuple<RectTransform, float> item3 in givenCardsGroup.GetOrderedCardScrollElementsPosition())
		{
			scrollElements.Add(item3.Item1);
			positionY.Add(item3.Item2);
		}
		foreach (AbilityCardUI item4 in tempSelectedCards)
		{
			item4.UpdateCard();
			item4.LostAnimation(active: false, cardHighlightTime * animationSpeed);
			RectTransform rectTransform = item4.transform as RectTransform;
			LTDescr anim = LeanTween.move(rectTransform, new Vector3(rectTransform.anchoredPosition.x, positionY[scrollElements.IndexOf(rectTransform)]), lostCardMoveTime * animationSpeed).setEase(easeType).setIgnoreTimeScale(useUnScaledTime: true);
			anim.setOnComplete((Action)delegate
			{
				animations.Remove(anim);
			});
			animations.Add(anim);
		}
		yield return new WaitForSecondsRealtime(lostCardMoveTime * animationSpeed - cardsReorderingOffsetTime * animationSpeed - discardedCardsTransitionTime * animationSpeed);
		foreach (AbilityCardUI item5 in cardsUI)
		{
			item5.UpdateCard();
		}
		bool movementRequired = false;
		for (int num3 = 0; num3 < scrollElements.Count; num3++)
		{
			RectTransform rectTransform2 = scrollElements[num3];
			Vector3 vector = new Vector3(rectTransform2.anchoredPosition.x, positionY[num3]);
			if (!movementRequired && vector != rectTransform2.position)
			{
				movementRequired = true;
			}
			LTDescr anim2 = LeanTween.move(rectTransform2, vector, discardedCardsMoveTime * animationSpeed).setEase(easeType).setIgnoreTimeScale(useUnScaledTime: true);
			anim2.setOnComplete((Action)delegate
			{
				animations.Remove(anim2);
			});
			animations.Add(anim2);
		}
		if (movementRequired)
		{
			yield return new WaitForSecondsRealtime(discardedCardsMoveTime);
		}
		yield return new WaitForSecondsRealtime(movementRequired ? (postAnimationWaitTime * animationSpeed) : Mathf.Max(discardedCardsMoveTime * animationSpeed, postAnimationWaitTime * animationSpeed));
		yield return new WaitUntil(() => animations.Count == 0);
		SortCards();
		verticalLayoutGroup.enabled = true;
		layoutElement.minHeight = 0f;
		givenCardsGroup.ResetLoseCard();
		animatedLosingCard = false;
		UIManager.Instance.RequestToggleLockUI(active: false, base.gameObject);
		if (Singleton<UINavigation>.Instance.StateMachine.IsCurrentState<LockState>())
		{
			Singleton<UINavigation>.Instance.StateMachine.ToPreviousState();
		}
		onAnimateCardLostCallback = null;
		onCompleteCallback?.Invoke();
		AnimatingLostCards = false;
	}

	private void InstantCardLost(List<AbilityCardUI> selectedCards)
	{
		if (isPreviewingCards)
		{
			ToggleFullCardsPreview(active: false);
		}
		SortCards();
		foreach (AbilityCardUI item in cardsUI)
		{
			item.MiniAbilityCard.DisableGlowSelected();
			item.MiniAbilityCard.DisplayIndicator(active: false);
			item.UpdateCard();
		}
	}

	public void RefreshConnectionState()
	{
		bool unfocused = FFSNetwork.IsOnline && !playerActor.IsUnderMyControl;
		itemsUI.SetUnfocused(unfocused);
		modifiersController.SetUnfocused(unfocused);
	}

	private IEnumerable<RectTransform> GetOrderedCardScrollElements()
	{
		return from it in (from it in cardsUI
				where !givenCardsGroup.IsGivenCard(it)
				select it.transform as RectTransform).Append(burntHeader.transform as RectTransform).Append(discardedHeader.transform as RectTransform).Append(shortRest.transform as RectTransform)
			orderby it.GetSiblingIndex()
			select it;
	}

	private void OnDisable()
	{
		CancelAnimateCardLost();
	}

	private void CancelAnimateCardLost()
	{
		if (!animatedLosingCard)
		{
			return;
		}
		animatedLosingCard = false;
		OnClearLeanTweens();
		StopAllCoroutines();
		SortCards();
		foreach (AbilityCardUI item in cardsUI)
		{
			item.CancelLostAnimation();
			item.UpdateCard();
		}
		verticalLayoutGroup.enabled = true;
		layoutElement.minHeight = 0f;
		givenCardsGroup.ResetLoseCard();
		UIManager.Instance.RequestToggleLockUI(active: false, base.gameObject);
		if (Singleton<UINavigation>.Instance.StateMachine.IsCurrentState<LockState>())
		{
			Singleton<UINavigation>.Instance.StateMachine.ToPreviousState();
		}
		onAnimateCardLostCallback?.Invoke();
		onAnimateCardLostCallback = null;
	}

	private void OnClearLeanTweens()
	{
		animations.ForEach(delegate(LTDescr it)
		{
			LeanTween.cancel(it.id, "CancelAnimateCardLost");
		});
		animations.Clear();
	}

	private void SortCards(bool reorderPositions = true, bool toggleHeader = true)
	{
		try
		{
			cardsUI.Sort();
			if (reorderPositions)
			{
				if (toggleHeader)
				{
					discardedHeader.Hide();
					burntHeader.Hide();
				}
				List<CardHandHeader> list = new List<CardHandHeader>();
				int num = 0;
				foreach (AbilityCardUI item in cardsUI)
				{
					if (item.IsLongRest)
					{
						if (cardsStateBeforePreview.ContainsKey(item))
						{
							cardsStateBeforePreview[item].order = num;
						}
						item.transform.SetAsLastSibling();
						num++;
						shortRest.transform.SetAsLastSibling();
						num++;
						discardedHeader.transform.SetAsLastSibling();
						list.Add(discardedHeader);
						num++;
						continue;
					}
					if (item.CardType == CardPileType.Unselected && !list.Contains(burntHeader))
					{
						list.Add(burntHeader);
						burntHeader.transform.SetAsLastSibling();
						num++;
					}
					CardHandHeader header = GetHeader(item);
					if (header != null)
					{
						if (!list.Contains(header))
						{
							list.Add(header);
							header.transform.SetAsLastSibling();
							num++;
						}
						if (toggleHeader && item.gameObject.activeSelf)
						{
							header.Show();
						}
					}
					if (cardsStateBeforePreview.ContainsKey(item))
					{
						cardsStateBeforePreview[item].order = num;
					}
					item.transform.SetAsLastSibling();
					num++;
				}
				if (!list.Contains(burntHeader))
				{
					burntHeader.transform.SetAsLastSibling();
				}
			}
			if (NavigationGroup != null)
			{
				NavigationGroup.UpdateElementsSortingOrder();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.SortCards().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00017", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private CardHandHeader GetHeader(AbilityCardUI card)
	{
		if (givenCardsGroup.IsGivenCard(card))
		{
			return null;
		}
		switch (card.CardType)
		{
		case CardPileType.Active:
			return null;
		case CardPileType.Discarded:
			return discardedHeader;
		case CardPileType.Lost:
		case CardPileType.Permalost:
			return burntHeader;
		default:
			return null;
		}
	}

	public List<AbilityCardUI> GetActiveAbilityCards()
	{
		return cardsUI.FindAll((AbilityCardUI it) => it.CardType == CardPileType.Active);
	}

	private void UpdateCards(CardPileType filtertype, EUpdateCardViewMode updateView)
	{
		try
		{
			List<CAbilityCard> pile = ((playerActor.CharacterClass.RoundAbilityCards.Count == 2 || useCurrentRoundCards) ? playerActor.CharacterClass.RoundAbilityCards : playerActor.CharacterClass.RoundCardsSelectedInCardSelection);
			bool flag = updateView != EUpdateCardViewMode.None;
			if (flag)
			{
				discardedHeader.Hide();
				burntHeader.Hide();
			}
			foreach (AbilityCardUI item in cardsUI)
			{
				bool flag2 = givenCardsGroup.IsGivenCard(item);
				switch (filtertype)
				{
				case CardPileType.Any:
					if (item.IsLongRest && PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest && !IsImprovedLongResting)
					{
						item.SetType(playerActor.IsLongRestSelected ? CardPileType.Round : CardPileType.Hand);
						if (updateView != EUpdateCardViewMode.None)
						{
							item.UpdateCard();
						}
						item.gameObject.SetActive(value: true);
						if (cardsStateBeforePreview.ContainsKey(item))
						{
							cardsStateBeforePreview[item].isActive = true;
						}
					}
					else
					{
						if (UpdateCard(item, CardPileType.Round, pile, updateView) || UpdateCard(item, CardPileType.Active, playerActor.CharacterClass.ActivatedAbilityCards, updateView) || UpdateCard(item, CardPileType.Hand, playerActor.CharacterClass.HandAbilityCards, updateView))
						{
							break;
						}
						if (UpdateCard(item, CardPileType.Discarded, playerActor.CharacterClass.DiscardedAbilityCards, updateView))
						{
							if (flag && !flag2)
							{
								discardedHeader.Show();
							}
						}
						else if (UpdateCard(item, CardPileType.Lost, playerActor.CharacterClass.LostAbilityCards, updateView))
						{
							if (flag && !flag2)
							{
								burntHeader.Show();
							}
						}
						else if (UpdateCard(item, CardPileType.Permalost, playerActor.CharacterClass.PermanentlyLostAbilityCards, updateView))
						{
							if (flag && !flag2)
							{
								burntHeader.Show();
							}
						}
						else if (!UpdateCard(item, CardPileType.ExtraTurn, playerActor.CharacterClass.ExtraTurnCardsSelectedInCardSelection, updateView))
						{
						}
					}
					break;
				case CardPileType.Round:
					UpdateCard(item, filtertype, pile, updateView);
					break;
				case CardPileType.Active:
					UpdateCard(item, filtertype, playerActor.CharacterClass.ActivatedAbilityCards, updateView);
					break;
				case CardPileType.Hand:
					UpdateCard(item, filtertype, playerActor.CharacterClass.HandAbilityCards, updateView);
					break;
				case CardPileType.Discarded:
					if (UpdateCard(item, filtertype, playerActor.CharacterClass.DiscardedAbilityCards, updateView) && flag && !flag2)
					{
						discardedHeader.Show();
					}
					break;
				case CardPileType.Lost:
					if (UpdateCard(item, filtertype, playerActor.CharacterClass.LostAbilityCards, updateView) && flag && !flag2)
					{
						burntHeader.Show();
					}
					break;
				case CardPileType.Permalost:
					if (UpdateCard(item, filtertype, playerActor.CharacterClass.PermanentlyLostAbilityCards, updateView) && flag && !flag2)
					{
						burntHeader.Show();
					}
					break;
				case CardPileType.Unselected:
					UpdateCard(item, filtertype, playerActor.CharacterClass.UnselectedAbilityCards.FindAll((CAbilityCard c) => c.Level <= playerActor.Level), updateView);
					break;
				case CardPileType.ExtraTurn:
					UpdateCard(item, filtertype, playerActor.CharacterClass.ExtraTurnCardsSelectedInCardSelection, updateView);
					break;
				case CardPileType.None:
					UpdateCard(item, filtertype, null, updateView);
					break;
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.UpdateCards().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00018", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void RefreshHeaders()
	{
		bool flag = false;
		bool flag2 = false;
		foreach (AbilityCardUI item in cardsUI.Where((AbilityCardUI it) => it.gameObject.activeSelf && !givenCardsGroup.IsGivenCard(it)))
		{
			if (!flag && playerActor.CharacterClass.DiscardedAbilityCards.Contains(item.AbilityCard))
			{
				flag = true;
			}
			else if (!flag2 && playerActor.CharacterClass.LostAbilityCards.Contains(item.AbilityCard))
			{
				flag2 = true;
			}
			else if (!flag2 && playerActor.CharacterClass.PermanentlyLostAbilityCards.Contains(item.AbilityCard))
			{
				flag2 = true;
			}
		}
		if (!flag)
		{
			discardedHeader.Hide();
		}
		else
		{
			discardedHeader.Show();
		}
		if (!flag2)
		{
			burntHeader.Hide();
		}
		else
		{
			burntHeader.Show();
		}
	}

	private bool UpdateCard(AbilityCardUI cardUI, CardPileType displayMode, List<CAbilityCard> pile, EUpdateCardViewMode updateView)
	{
		if (pile != null && pile.Contains(cardUI.AbilityCard))
		{
			cardUI.SetType(displayMode);
			if (updateView != EUpdateCardViewMode.None)
			{
				if (updateView == EUpdateCardViewMode.Refresh)
				{
					cardUI.UpdateCard();
				}
				cardUI.gameObject.SetActive(value: true);
				if (cardsStateBeforePreview.ContainsKey(cardUI))
				{
					cardsStateBeforePreview[cardUI].isActive = true;
				}
			}
			return true;
		}
		if (updateView != EUpdateCardViewMode.None)
		{
			cardUI.gameObject.SetActive(value: false);
			if (cardsStateBeforePreview.ContainsKey(cardUI))
			{
				cardsStateBeforePreview[cardUI].isActive = false;
			}
		}
		return false;
	}

	public void PreviewActionCards(Transform holder)
	{
		foreach (AbilityCardUI item in cardsUI)
		{
			bool flag = playerActor.CharacterClass.RoundAbilityCards.Contains(item.AbilityCard) || playerActor.CharacterClass.RoundCardsSelectedInCardSelection.Contains(item.AbilityCard);
			item.gameObject.SetActive(flag);
			if (flag)
			{
				item.SetParent(holder);
				item.IsInteractable = true;
				item.ToggleFullCard(active: true);
				item.canvasGroup.alpha = 1f;
			}
		}
		UpdateShortRest();
	}

	public void HidePreviewActionCards(CardPileType filter, CardHandMode mode)
	{
		foreach (AbilityCardUI item in cardsUI)
		{
			if (item.IsLongRest)
			{
				item.gameObject.SetActive(item.CardType == CardPileType.Hand && PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest);
			}
			Transform transform = (givenCardsGroup.IsGivenCard(item) ? givenCardsGroup.Container : abilityCardsHolder);
			if (item.transform != transform)
			{
				item.SetParent(transform);
			}
			if (playerActor.CharacterClass.RoundAbilityCards.Contains(item.AbilityCard) || playerActor.CharacterClass.RoundCardsSelectedInCardSelection.Contains(item.AbilityCard))
			{
				item.ToggleFullCard(mode == CardHandMode.ActionSelection);
			}
		}
		UpdateCards(filter, EUpdateCardViewMode.OnyActive);
		shortRest.Hide();
	}

	private void SetMode(CardHandMode newMode, CardPileType filterType = CardPileType.Any, List<CardPileType> selectableCardTypes = null, bool fadeUnselectableCards = false, bool highlightSelectableCards = false, bool longRestAvailable = false, CardActionsCommand commandCardActions = CardActionsCommand.RESET, Func<CAbilityCard, bool> cardSelectableFilter = null)
	{
		try
		{
			selectedCardsUI.Clear();
			previousMode = currentMode;
			currentMode = newMode;
			if (!cardsSpawned)
			{
				Debug.LogError("Cards are not spawned.");
				return;
			}
			if (currentMode == CardHandMode.CardsSelection && (PhaseManager.CurrentPhase.Type == CPhase.PhaseType.SelectAbilityCardsOrLongRest || playerActor.SelectingCardsForExtraTurnOfType != CAbilityExtraTurn.EExtraTurnType.None))
			{
				givenCardsGroup.Show((AbilityCardUI abilityCard) => playerActor.CharacterClass.GivenCards.Contains(abilityCard.AbilityCard) || abilityCard.AbilityCard.ClassID != playerActor.CharacterClass.CharacterID);
			}
			else
			{
				givenCardsGroup.Hide();
			}
			if (currentMode == CardHandMode.ActionSelection)
			{
				bool flag = commandCardActions != CardActionsCommand.NONE;
				foreach (AbilityCardUI item in cardsUI)
				{
					if (playerActor.IsTakingExtraTurn)
					{
						if (playerActor.CharacterClass.ExtraTurnCardsSelectedInCardSelection.Contains(item.AbilityCard))
						{
							selectedCardsUI.Add(item);
						}
					}
					else if (playerActor.CharacterClass.RoundAbilityCards.Contains(item.AbilityCard) || playerActor.CharacterClass.RoundCardsSelectedInCardSelection.Contains(item.AbilityCard) || (playerActor.CharacterClass.LongRest && item.IsLongRest))
					{
						selectedCardsUI.Add(item);
					}
				}
				if ((previousMode != currentMode || (flag && cardsActionController.GetPhase() == CardsActionControlller.Phase.None)) && !IsLongRestSelected())
				{
					if (commandCardActions != CardActionsCommand.FORCE_RESET && (previousMode == CardHandMode.RecoverDiscardedCard || previousMode == CardHandMode.RecoverLostCard))
					{
						flag = false;
					}
					if (selectedCardsUI.Count < 2)
					{
						if (selectedCardsUI.Count < 1)
						{
							cardsActionController.Init(null, null, flag, playerActor.TakingExtraTurnOfType);
						}
						else
						{
							cardsActionController.Init(selectedCardsUI[0].fullAbilityCard, null, flag, playerActor.TakingExtraTurnOfType);
						}
					}
					else
					{
						cardsActionController.Init(selectedCardsUI[0].fullAbilityCard, selectedCardsUI[1].fullAbilityCard, flag, playerActor.TakingExtraTurnOfType);
					}
				}
			}
			else if (currentMode == CardHandMode.CardsSelection && PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
			{
				foreach (CAbilityCard roundAbilityCard in playerActor.CharacterClass.RoundAbilityCards)
				{
					AbilityCardUI cardUI = GetCardUI(roundAbilityCard);
					selectedCardsUI.Insert(0, cardUI);
				}
				if (playerActor.IsLongRestSelected)
				{
					selectedCardsUI.Add(GetCard(-1));
				}
			}
			bool flag2 = !FFSNetwork.IsOnline || playerActor == null || (playerActor.IsUnderMyControl && !Singleton<UIReadyToggle>.Instance.PlayersReady.Contains(PlayerRegistry.MyPlayer) && !Singleton<UIReadyToggle>.Instance.IsProgressingBar);
			if (currentMode == CardHandMode.ActionSelection && playerActor.TakingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater)
			{
				Func<CAbilityCard, bool> filter = cardSelectableFilter;
				cardSelectableFilter = (CAbilityCard card) => playerActor.Initiative() < card.Initiative && (filter == null || filter(card));
			}
			else if (currentMode == CardHandMode.LoseCard)
			{
				Func<CAbilityCard, bool> filter2 = cardSelectableFilter;
				cardSelectableFilter = (CAbilityCard card) => !card.SupplyCard && (filter2 == null || filter2(card));
			}
			foreach (AbilityCardUI item2 in cardsUI)
			{
				item2.SetMode(newMode, selectableCardTypes, item2.IsLongRest || fadeUnselectableCards, highlightSelectableCards, longRestAvailable && !IsImprovedLongResting, cardSelectableFilter);
				if (playerActor.IsTakingExtraTurn)
				{
					if (playerActor.TakingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BottomAction)
					{
						SimpleLog.AddToSimpleLog("ToggleSideInteractivity TopAction to false when taking extra tun " + item2.fullAbilityCard.AbilityCard?.Name);
						item2.fullAbilityCard.ToggleSideInteractivity(active: false, CBaseCard.ActionType.TopAction);
					}
					else if (playerActor.TakingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.TopAction)
					{
						SimpleLog.AddToSimpleLog("ToggleSideInteractivity BottomAction to false when taking extra tun " + item2.fullAbilityCard.AbilityCard?.Name);
						item2.fullAbilityCard.ToggleSideInteractivity(active: false, CBaseCard.ActionType.BottomAction);
					}
				}
				if (flag2)
				{
					item2.SetValid(isValid: true);
				}
				else if (playerActor.IsUnderMyControl)
				{
					item2.SetValid(isValid: false, UIMultiplayerNotifications.ShowCancelReadiedCards);
				}
				else
				{
					item2.SetValid(isValid: false, UIMultiplayerNotifications.ShowSelectedOtherPlayerCard, (currentMode == CardHandMode.ActionSelection) ? new Action(UIMultiplayerNotifications.ShowSelectedOtherPlayerCard) : null);
				}
				item2.SetUnfocused(FFSNetwork.IsOnline && !playerActor.IsUnderMyControl);
			}
			if (IsImprovedLongResting)
			{
				UpdateShortRest();
			}
			if (newMode == CardHandMode.CardsSelection)
			{
				UpdateInitiative();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.SetMode().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00019", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void RefreshValidCards()
	{
		bool flag = !FFSNetwork.IsOnline || playerActor == null || (playerActor.IsUnderMyControl && !Singleton<UIReadyToggle>.Instance.PlayersReady.Contains(PlayerRegistry.MyPlayer) && !Singleton<UIReadyToggle>.Instance.IsProgressingBar);
		foreach (AbilityCardUI item in cardsUI)
		{
			if (flag)
			{
				item.SetValid(isValid: true);
			}
			else if (playerActor.IsUnderMyControl)
			{
				item.SetValid(isValid: false, UIMultiplayerNotifications.ShowCancelReadiedCards);
			}
			else
			{
				item.SetValid(isValid: false, UIMultiplayerNotifications.ShowSelectedOtherPlayerCard, (currentMode == CardHandMode.ActionSelection) ? new Action(UIMultiplayerNotifications.ShowSelectedOtherPlayerCard) : null);
			}
		}
		if (shortRest.gameObject.activeInHierarchy)
		{
			if (flag)
			{
				shortRest.SetValid(isValid: true);
			}
			else if (playerActor.IsUnderMyControl)
			{
				shortRest.SetValid(isValid: false, UIMultiplayerNotifications.ShowCancelReadiedCards);
			}
			else
			{
				shortRest.SetValid(isValid: false, UIMultiplayerNotifications.ShowSelectedOtherPlayerCard);
			}
		}
	}

	public void SpawnGivenCardUI(List<CAbilityCard> newCards)
	{
		foreach (CAbilityCard newCard in newCards)
		{
			if (newCard.GetAbilityCardYML == null)
			{
				Debug.LogError("Ability card data is null! Check YML file for " + newCard.Name);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00020", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu);
				continue;
			}
			try
			{
				FFSNet.Console.Log("Spawning card " + newCard.Name + " (ID: " + newCard.ID + " CardInstanceID: " + newCard.CardInstanceID + ") for the actor " + playerActor.CharacterClass.ID + ", ActorID: " + playerActor.ID);
				AbilityCardUI abilityCardUI = ((!InputManager.GamePadInUse) ? givenCardsGroup.AddCard(newCard) : ObjectPool.SpawnCard(newCard.ID, ObjectPool.ECardType.Ability, abilityCardsHolder, resetLocalScale: true).GetComponent<AbilityCardUI>());
				abilityCardUI.Init(newCard, playerActor, CardPileType.Hand, OnCardSelected, OnCardDeselected, null, OnCardHover, OnCardAction, OnActionCompleted, OnCancelActiveAbility);
				abilityCardUI.transform.localPosition = Vector3.zero;
				cardsUI.Add(abilityCardUI);
				cardsStateBeforePreview[abilityCardUI] = new CardState(abilityCardUI);
			}
			catch (Exception ex)
			{
				Debug.LogError("Unable to initialise ability card " + newCard.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00021", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			}
		}
		if (!InputManager.GamePadInUse)
		{
			givenCardsGroup.Show();
		}
	}

	public void DestroyCardUI(CAbilityCard abilityCard)
	{
		AbilityCardUI abilityCardUI = cardsUI.SingleOrDefault((AbilityCardUI x) => x.AbilityCard == abilityCard);
		if (abilityCardUI != null)
		{
			givenCardsGroup.RemoveCard(abilityCardUI);
			cardsUI.Remove(abilityCardUI);
			cardsStateBeforePreview.Remove(abilityCardUI);
			ObjectPool.RecycleCard(abilityCardUI.CardID, ObjectPool.ECardType.Ability, abilityCardUI.gameObject);
		}
		else
		{
			Debug.LogError("Unable to find attached card UI to destroy for ability card " + abilityCard.Name + "\n" + Environment.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00048", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu);
		}
	}

	private IEnumerator SpawnCards(List<CAbilityCard> abilityCards, List<CAbilityCard> unselectedAbilityCards, List<CAbilityCard> givenCards, GameObject longRestPrefab)
	{
		try
		{
			Choreographer.s_Choreographer.AddUpdateBlocker();
			cardsSpawned = true;
			cardsUI.Clear();
			foreach (CAbilityCard abilityCard in abilityCards)
			{
				if (abilityCard.GetAbilityCardYML == null)
				{
					Debug.LogError("Ability card data is null! Check YML file for " + abilityCard.Name);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00020", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu);
					continue;
				}
				try
				{
					FFSNet.Console.Log("Spawning card " + abilityCard.Name + " (ID: " + abilityCard.ID + " CardInstanceID: " + abilityCard.CardInstanceID + ") for the actor " + playerActor.CharacterClass.ID + ", ActorID: " + playerActor.ID);
					AbilityCardUI abilityCardUI = ((!InputManager.GamePadInUse) ? ((abilityCard.ClassID == playerActor.CharacterClass.CharacterID) ? ObjectPool.SpawnCard(abilityCard.ID, ObjectPool.ECardType.Ability, abilityCardsHolder, resetLocalScale: true).GetComponent<AbilityCardUI>() : givenCardsGroup.AddCard(abilityCard)) : ObjectPool.SpawnCard(abilityCard.ID, ObjectPool.ECardType.Ability, abilityCardsHolder, resetLocalScale: true).GetComponent<AbilityCardUI>());
					abilityCardUI.Init(abilityCard, playerActor, CardPileType.Hand, OnCardSelected, OnCardDeselected, null, OnCardHover, OnCardAction, OnActionCompleted, OnCancelActiveAbility);
					abilityCardUI.transform.localPosition = Vector3.zero;
					cardsUI.Add(abilityCardUI);
				}
				catch (Exception ex)
				{
					Debug.LogError("Unable to initialise ability card " + abilityCard.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00021", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
				}
				yield return null;
			}
			foreach (CAbilityCard unselectedAbilityCard in unselectedAbilityCards)
			{
				if (unselectedAbilityCard.GetAbilityCardYML == null)
				{
					Debug.LogError("Ability card data is null! Check YML file for " + unselectedAbilityCard.Name);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00020", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu);
					continue;
				}
				try
				{
					AbilityCardUI component = ObjectPool.SpawnCard(unselectedAbilityCard.ID, ObjectPool.ECardType.Ability, abilityCardsHolder, resetLocalScale: true).GetComponent<AbilityCardUI>();
					component.Init(unselectedAbilityCard, playerActor, CardPileType.Unselected, OnCardSelected, OnCardDeselected, null, OnCardHover, OnCardAction, OnActionCompleted, OnCancelActiveAbility);
					component.transform.localPosition = Vector3.zero;
					cardsUI.Add(component);
				}
				catch (Exception ex2)
				{
					Debug.LogError("Unable to initialise ability card " + unselectedAbilityCard.Name + "\n" + ex2.Message + "\n" + ex2.StackTrace);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00021", "GUI_ERROR_MAIN_MENU_BUTTON", ex2.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex2.Message);
				}
				yield return null;
			}
			foreach (CAbilityCard givenCard in givenCards)
			{
				if (givenCard.GetAbilityCardYML == null)
				{
					Debug.LogError("Ability card data is null! Check YML file for " + givenCard.Name);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00020", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu);
					continue;
				}
				try
				{
					AbilityCardUI abilityCardUI2 = ((!InputManager.GamePadInUse) ? givenCardsGroup.AddCard(givenCard) : ObjectPool.SpawnCard(givenCard.ID, ObjectPool.ECardType.Ability, abilityCardsHolder, resetLocalScale: true).GetComponent<AbilityCardUI>());
					abilityCardUI2.Init(givenCard, playerActor, CardPileType.Unselected, OnCardSelected, OnCardDeselected, null, OnCardHover, OnCardAction, OnActionCompleted, OnCancelActiveAbility);
					abilityCardUI2.transform.localPosition = Vector3.zero;
					cardsUI.Add(abilityCardUI2);
				}
				catch (Exception ex3)
				{
					Debug.LogError("Unable to initialise ability card " + givenCard.Name + "\n" + ex3.Message + "\n" + ex3.StackTrace);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00021", "GUI_ERROR_MAIN_MENU_BUTTON", ex3.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex3.Message);
				}
				yield return null;
			}
			if (!InputManager.GamePadInUse)
			{
				givenCardsGroup.Show();
			}
			if (CharacterClassManager.LongRestLayout != null)
			{
				try
				{
					AbilityCardUI component2 = ObjectPool.SpawnCard(-1, ObjectPool.ECardType.Ability, abilityCardsHolder).GetComponent<AbilityCardUI>();
					component2.transform.localScale = Vector3.one;
					component2.Init(null, playerActor, CardPileType.Hand, OnCardSelected, OnCardDeselected, null, OnCardHover, OnCardAction, OnActionCompleted, null, isLongRest: true);
					cardsUI.Add(component2);
				}
				catch (Exception ex4)
				{
					Debug.LogError("Unable to initialise Long Rest card\n" + ex4.Message + "\n" + ex4.StackTrace);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00022", "GUI_ERROR_MAIN_MENU_BUTTON", ex4.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex4.Message);
				}
			}
			shortRest = UnityEngine.Object.Instantiate(shortRestPrefab, abilityCardsHolder).GetComponent<ShortRest>();
			shortRest.Init(playerActor, PerformShortRest);
			itemsUI.SetActor(playerActor);
		}
		finally
		{
			Choreographer.s_Choreographer.RemoveUpdateBlocker();
		}
	}

	private void OnCancelActiveAbility(AbilityCardUI abilityCardUi)
	{
		try
		{
			CardsHandManager.Instance.DiscardActiveAbilityCard(playerActor, abilityCardUi.AbilityCard, null, delegate
			{
				Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.CardSelection);
			});
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.OnCancelActiveAbility().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00023", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void OnActionCompleted()
	{
		try
		{
			if (GameState.CurrentActionSelectionSequence == GameState.ActionSelectionSequenceType.SecondAction)
			{
				selectedCardsUI.Clear();
			}
			cardsActionController.OnActionFinished();
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.OnActionCompleted().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00024", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void OnCardAction(FullAbilityCard cardActionUI, CBaseCard.ActionType actionType)
	{
		try
		{
			cardsActionController.OnCardClicked(cardActionUI, actionType);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.OnCardAction().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00025", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void Undo()
	{
		try
		{
			itemsUI.RefreshItemsState();
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.Undo().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00026", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void OnCardHover(bool isActive, AbilityCardUI cardUI)
	{
		try
		{
			InitiativeTrack.Instance?.OnCardHover(isActive, cardUI, playerActor);
			if (InputManager.GamePadInUse)
			{
				_currentCard = cardUI;
				ScrollTo(cardUI.gameObject);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.OnCardHover().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00027", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private int GetActiveChildren(Transform targetTransform)
	{
		int num = 0;
		foreach (Transform item in targetTransform)
		{
			if (item.gameObject.activeSelf)
			{
				num++;
			}
		}
		return num;
	}

	private void ScrollTo(GameObject item)
	{
		if (item.transform.GetSiblingIndex() == 0)
		{
			scrollRect.verticalNormalizedPosition = 1f;
			return;
		}
		int activeChildren = GetActiveChildren(item.transform.parent);
		if (item.transform.GetSiblingIndex() >= activeChildren - 2)
		{
			scrollRect.verticalNormalizedPosition = 0f;
		}
		else
		{
			scrollRect.ScrollToFit(item.transform as RectTransform);
		}
	}

	private void OnCardSelected(AbilityCardUI cardUI, bool networkAction = true)
	{
		try
		{
			if (currentMode == CardHandMode.ActionSelection || currentMode == CardHandMode.Preview)
			{
				return;
			}
			if (currentMode == CardHandMode.CardsSelection && playerActor.SelectingCardsForExtraTurnOfType != CAbilityExtraTurn.EExtraTurnType.None && selectedCardsUI.Count == maxCardsSelected)
			{
				selectedCardsUI.Insert(0, cardUI);
				cardUI.ToggleSelect(active: false, highlight: false, networkAction: false);
				cardUI.MiniAbilityCard.ShowWarning(show: true);
				Singleton<HelpBox>.Instance.HighlightWarning();
				return;
			}
			if (cardUI.IsLongRest)
			{
				selectedCardsUI.Insert(0, cardUI);
				for (int num = selectedCardsUI.Count - 1; num >= 1; num--)
				{
					selectedCardsUI[num].ToggleSelect(active: false, highlight: false, networkAction: false);
				}
				playerActor.CharacterClass.LongRest = true;
				UpdateInitiative();
				InitiativeTrack.Instance.CheckRoundAbilityCardsOrLongRestSelected();
				OnSelectedCardsNumberChanged(2);
				if (networkAction)
				{
					NetworkSelectedRoundCards();
				}
				UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.AbilityCardSelected, playerActor.GetPrefabName(), "Long Rest"));
				return;
			}
			if (selectedCardsUI.Count == 1 && selectedCardsUI[0].IsLongRest)
			{
				selectedCardsUI[0].ToggleSelect(active: false, highlight: false, networkAction: false);
			}
			selectedCardsUI.Insert(0, cardUI);
			for (int i = maxCardsSelected; i < selectedCardsUI.Count; i++)
			{
				selectedCardsUI[i].ToggleSelect(active: false, highlight: false, networkAction: false);
			}
			if (currentMode == CardHandMode.CardsSelection)
			{
				if (playerActor.SelectingCardsForExtraTurnOfType != CAbilityExtraTurn.EExtraTurnType.None)
				{
					int num2 = playerActor.OriginalInitiative();
					if (playerActor.SelectingCardsForExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater && selectedCardsUI.Count == 1 && selectedCardsUI[0].AbilityCard.Initiative <= num2)
					{
						InitiativeTrack.Instance.helpBox.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_EXTRA_TURN_BOTH_ACTIONS_LATER"), playerActor.OriginalInitiative()), Singleton<AbilityEffectManager>.Instance.ExtraTurnAbilityEffectTitle(playerActor));
						InitiativeTrack.Instance.helpBox.HighlightWarning();
						DeselectAllCards(networkAction: false);
						return;
					}
					if (maxCardsSelected == selectedCardsUI.Count && maxCardsSelected != 0)
					{
						Choreographer.s_Choreographer.readyButton.AlternativeAction(OnSelectExtraTurnCardsClick);
						if (!FFSNetwork.IsOnline || playerActor.IsUnderMyControl)
						{
							Choreographer.s_Choreographer.readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONENDSELECTION, null, hideOnClick: true, glowingEffect: true, interactable: true, disregardTurnControlForInteractability: true);
							Choreographer.s_Choreographer.m_SkipButton.SetInteractable(active: false);
						}
					}
					else if (playerActor.SelectingCardsForExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater && selectedCardsUI.Count == 1)
					{
						InitiativeTrack.Instance.helpBox.ShowTranslated(LocalizationManager.GetTranslation("GUI_TOOLTIP_EXTRA_TURN_BOTH_ACTIONS_LATER_SECOND_CARD"), Singleton<AbilityEffectManager>.Instance.ExtraTurnAbilityEffectTitle(playerActor));
						List<CAbilityCard> list = selectedCardsUI.Select((AbilityCardUI it) => it.AbilityCard).ToList();
						playerActor.CharacterClass.SetExtraTurnInitiativeAbilityCard(list[0]);
						UpdateInitiative();
					}
					if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl && networkAction)
					{
						int iD = playerActor.ID;
						IProtocolToken supplementaryDataToken = new CardsToken(selectedCardsUI);
						Synchronizer.SendGameAction(GameActionType.SelectExtraTurnCards, ActionPhaseType.ExtraTurnCardSelection, validateOnServerBeforeExecuting: false, disableAutoReplication: false, iD, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
					}
				}
				else
				{
					bool flag = playerActor.CharacterClass.RoundAbilityCards.Contains(cardUI.AbilityCard);
					if (!flag)
					{
						if (!playerActor.CharacterClass.HandAbilityCards.Contains(cardUI.AbilityCard))
						{
							FFSNet.Console.LogError("ERROR_MULTIPLAYER_00036", "Attempted to select a card that was not in the players hand!  Player: " + playerActor.CharacterClass.ID + "  Card: " + cardUI.AbilityCard.Name, Environment.StackTrace);
							return;
						}
						uint num3 = ScenarioRuleClient.MoveAbilityCard(playerActor.CharacterClass, cardUI.AbilityCard, playerActor.CharacterClass.HandAbilityCards, playerActor.CharacterClass.RoundAbilityCards, "HandAbilityCards", "RoundAbilityCards", networkAction);
						long num4 = DateTime.Now.Ticks / 10000;
						while (num3 > ScenarioRuleClient.s_SRLLastProcessedMessageID && DateTime.Now.Ticks / 10000 - num4 < 1000)
						{
							Thread.Sleep(10);
						}
					}
					InitiativeTrack.Instance.CheckRoundAbilityCardsOrLongRestSelected();
					Debug.LogFormat("[CARDS HAND UI]: OnCardSelected {0}. Is contained in round abilities: {1}. RoundAbilityCards {2}", cardUI.AbilityCard.Name, flag, playerActor.CharacterClass.RoundAbilityCards.Count);
					UpdateInitiative();
				}
			}
			else if (currentMode == CardHandMode.LoseCard || currentMode == CardHandMode.DiscardCard)
			{
				if (maxCardsSelected == selectedCardsUI.Count && maxCardsSelected != 0)
				{
					string text = "";
					if (currentMode == CardHandMode.LoseCard)
					{
						text = ((maxCardsSelected == 1) ? string.Format(LocalizationManager.GetTranslation("GUI_LOSE_CARD"), cardUI.fullAbilityCard.Title) : LocalizationManager.GetTranslation("GUI_LOSE_CARDS"));
					}
					else if (currentMode == CardHandMode.DiscardCard)
					{
						text = ((maxCardsSelected == 1) ? string.Format(LocalizationManager.GetTranslation("GUI_DISCARD_CARD"), cardUI.fullAbilityCard.Title) : LocalizationManager.GetTranslation("GUI_DISCARD_CARDS"));
					}
					foreach (AbilityCardUI item in selectedCardsUI)
					{
						item.OnPointerExit();
					}
					List<FullAbilityCard> fullCards = selectedCardsUI.Select((AbilityCardUI it) => it.fullAbilityCard).ToList();
					Dictionary<AbilityCardUI, bool> wasSelectable = new Dictionary<AbilityCardUI, bool>();
					foreach (AbilityCardUI item2 in cardsUI)
					{
						wasSelectable[item2] = item2.IsSelectable;
						if (!InputManager.GamePadInUse)
						{
							item2.SetSelectable(isOn: false, !selectedCardsUI.Contains(item2));
						}
					}
					Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.Burn);
					InputManager.RequestDisableInput(this, new KeyAction[4]
					{
						KeyAction.DISPLAY_CARDS_HERO_1,
						KeyAction.DISPLAY_CARDS_HERO_2,
						KeyAction.DISPLAY_CARDS_HERO_3,
						KeyAction.DISPLAY_CARDS_HERO_4
					});
					foreach (AbilityCardUI item3 in selectedCardsUI)
					{
						cardsInPopUp.Add(item3);
						LockCard(item3);
					}
					DialogPopup dialogPopup = UIManager.Instance.dialogPopup;
					dialogPopup.OnHiding = (Action)Delegate.Combine(dialogPopup.OnHiding, new Action(OnHidingFullCardsPopup));
					UIManager.Instance.dialogPopup.Show(fullCards.Select((FullAbilityCard it) => it.gameObject).ToList(), new DialogOption[2]
					{
						new DialogOption(text, KeyAction.CONFIRM_ACTION_BUTTON, delegate
						{
							Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.RoundStart);
							InputManager.RequestEnableInput(this, new KeyAction[4]
							{
								KeyAction.DISPLAY_CARDS_HERO_1,
								KeyAction.DISPLAY_CARDS_HERO_2,
								KeyAction.DISPLAY_CARDS_HERO_3,
								KeyAction.DISPLAY_CARDS_HERO_4
							});
							foreach (AbilityCardUI item4 in cardsUI)
							{
								item4.SetSelectable(wasSelectable[item4]);
							}
							OnLoseCardClick();
						}, delegate
						{
							fullCards.ForEach(delegate(FullAbilityCard it)
							{
								it.cardEffects.ToggleEffect(active: false, CardEffects.FXTask.BurnCard);
							});
						}, delegate
						{
							fullCards.ForEach(delegate(FullAbilityCard it)
							{
								it.RefreshPile();
							});
						}),
						new DialogOption(LocalizationManager.GetTranslation("GUI_CHOOSE_OTHER_CARD"), KeyAction.UI_CANCEL, delegate
						{
							Singleton<UINavigation>.Instance.StateMachine.ToNonMenuPreviousState(_stateFilter);
							InputManager.RequestEnableInput(this, new KeyAction[4]
							{
								KeyAction.DISPLAY_CARDS_HERO_1,
								KeyAction.DISPLAY_CARDS_HERO_2,
								KeyAction.DISPLAY_CARDS_HERO_3,
								KeyAction.DISPLAY_CARDS_HERO_4
							});
							foreach (AbilityCardUI item5 in cardsUI)
							{
								item5.SetSelectable(wasSelectable[item5]);
							}
							DeselectAllCards();
							Singleton<TakeDamagePanel>.Instance.ToggleVisibility(visible: true);
						})
					}, allowHide: false, 1);
					UIManager.Instance.dialogPopup.transform.SetParent(CardsHandManager.Instance.transform);
					UIManager.Instance.dialogPopup.transform.SetAsLastSibling();
					Singleton<TakeDamagePanel>.Instance.ToggleVisibility(visible: false);
					Choreographer.s_Choreographer.m_SkipButton.Toggle(active: false);
				}
			}
			else if ((currentMode == CardHandMode.RecoverDiscardedCard || currentMode == CardHandMode.RecoverLostCard || currentMode == CardHandMode.IncreaseCardLimit) && maxCardsSelected == selectedCardsUI.Count && maxCardsSelected != 0)
			{
				if (currentMode == CardHandMode.IncreaseCardLimit)
				{
					Choreographer.s_Choreographer.readyButton.AlternativeAction(OnInceaseLimitCardClick);
				}
				else
				{
					Choreographer.s_Choreographer.readyButton.AlternativeAction(OnRecoverCardClick);
				}
				Choreographer.s_Choreographer.readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONRECOVERCARD, LocalizationManager.GetTranslation("GUI_CONFIRM"), hideOnClick: true, glowingEffect: true, interactable: true, disregardTurnControlForInteractability: true);
				Choreographer.s_Choreographer.m_SkipButton.Toggle(active: false);
			}
			UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.AbilityCardSelected, playerActor.GetPrefabName(), cardUI.AbilityCard.Name));
			OnSelectedCardsNumberChanged(selectedCardsUI.Count);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.OnCardSelected().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00028", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void DeselectAllCards(bool networkAction = true, bool isHandUnderMyControl = true)
	{
		for (int num = selectedCardsUI.Count - 1; num >= 0; num--)
		{
			selectedCardsUI[num].ToggleSelect(active: false, highlight: false, networkAction, isHandUnderMyControl);
		}
		selectedCardsUI.Clear();
		UnlockAllLockedCards();
	}

	private void LockCard(AbilityCardUI abilityCard)
	{
		if (!(abilityCard == null))
		{
			abilityCard.LockFullCard = true;
			_lockedCards.Add(abilityCard);
		}
	}

	private void UnlockCard(AbilityCardUI abilityCard)
	{
		if (!(abilityCard == null))
		{
			abilityCard.LockFullCard = false;
			_lockedCards.Remove(abilityCard);
		}
	}

	private void UnlockAllLockedCards()
	{
		foreach (AbilityCardUI lockedCard in _lockedCards)
		{
			lockedCard.LockFullCard = false;
		}
		_lockedCards.Clear();
	}

	private void OnHidingFullCardsPopup()
	{
		UnlockAllLockedCards();
		DialogPopup dialogPopup = UIManager.Instance.dialogPopup;
		dialogPopup.OnHiding = (Action)Delegate.Remove(dialogPopup.OnHiding, new Action(OnHidingFullCardsPopup));
	}

	private void OnCardDeselected(AbilityCardUI cardUI, bool networkAction = true)
	{
		try
		{
			if (currentMode == CardHandMode.ActionSelection || currentMode == CardHandMode.Preview)
			{
				return;
			}
			selectedCardsUI.Remove(cardUI);
			if (cardUI.IsLongRest && PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
			{
				playerActor.CharacterClass.LongRest = false;
			}
			if (currentMode == CardHandMode.CardsSelection)
			{
				if (playerActor.SelectingCardsForExtraTurnOfType != CAbilityExtraTurn.EExtraTurnType.None)
				{
					if (playerActor.SelectingCardsForExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater && cardUI.AbilityCard.Initiative == playerActor.Initiative())
					{
						playerActor.CharacterClass.SetExtraTurnInitiativeAbilityCard(null);
						int num = playerActor.OriginalInitiative();
						if (selectedCardsUI.Count > 0 && selectedCardsUI[selectedCardsUI.Count - 1].AbilityCard.Initiative > num)
						{
							playerActor.CharacterClass.SetExtraTurnInitiativeAbilityCard(selectedCardsUI[selectedCardsUI.Count - 1].AbilityCard);
						}
						else
						{
							InitiativeTrack.Instance.helpBox.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_EXTRA_TURN_BOTH_ACTIONS_LATER"), playerActor.Initiative()), Singleton<AbilityEffectManager>.Instance.ExtraTurnAbilityEffectTitle(playerActor));
							if (selectedCardsUI.Count > 0)
							{
								InitiativeTrack.Instance.helpBox.HighlightWarning();
							}
							if (!FFSNetwork.IsOnline || playerActor.IsUnderMyControl)
							{
								DeselectAllCards(networkAction);
							}
						}
						UpdateInitiative();
						InitiativeTrack.Instance.CheckRoundAbilityCardsOrLongRestSelected();
					}
					if (selectedCardsUI.Count != maxCardsSelected)
					{
						Choreographer.s_Choreographer.readyButton.SetInteractable(interactable: false);
					}
					if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl && networkAction)
					{
						int iD = playerActor.ID;
						IProtocolToken supplementaryDataToken = new CardsToken(selectedCardsUI);
						Synchronizer.SendGameAction(GameActionType.SelectExtraTurnCards, ActionPhaseType.ExtraTurnCardSelection, validateOnServerBeforeExecuting: false, disableAutoReplication: false, iD, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
					}
				}
				else
				{
					if (!cardUI.IsLongRest)
					{
						if (!CardsHandManager.Instance.IsShown)
						{
							return;
						}
						FFSNet.Console.Log("Moving ability card " + cardUI.CardName + " from RoundCards to Hand.");
						uint num2 = ScenarioRuleClient.MoveAbilityCard(playerActor.CharacterClass, cardUI.AbilityCard, playerActor.CharacterClass.RoundAbilityCards, playerActor.CharacterClass.HandAbilityCards, "RoundAbilityCards", "HandAbilityCards", networkAction);
						long num3 = DateTime.Now.Ticks / 10000;
						while (num2 > ScenarioRuleClient.s_SRLLastProcessedMessageID && DateTime.Now.Ticks / 10000 - num3 < 1000)
						{
							Thread.Sleep(10);
						}
						FFSNet.Console.Log("[CARDS HAND UI]: OnCardDeselected: Ability card moved.");
						UpdateInitiative();
					}
					else if (networkAction)
					{
						NetworkSelectedRoundCards();
						UpdateInitiative();
					}
					InitiativeTrack.Instance.CheckRoundAbilityCardsOrLongRestSelected();
				}
			}
			else if (currentMode == CardHandMode.LoseCard || currentMode == CardHandMode.DiscardCard || currentMode == CardHandMode.RecoverDiscardedCard || currentMode == CardHandMode.RecoverLostCard || currentMode == CardHandMode.IncreaseCardLimit)
			{
				foreach (AbilityCardUI item in cardsInPopUp)
				{
					if (item == cardUI)
					{
						item.LockFullCard = false;
					}
				}
				cardsInPopUp.Remove(cardUI);
				Choreographer.s_Choreographer.readyButton.Toggle(active: false, ReadyButton.EButtonState.EREADYBUTTONNA, null, hideOnClick: true, glowingEffect: false, interactable: true, disregardTurnControlForInteractability: false, haltActionProcessorIfDeactivated: false);
				Choreographer.s_Choreographer.m_SkipButton.Toggle(active: false);
				if (FFSNetwork.IsOnline && playerActor.SelectingCardsForExtraTurnOfType != CAbilityExtraTurn.EExtraTurnType.None && playerActor.IsUnderMyControl && networkAction)
				{
					int iD2 = playerActor.ID;
					IProtocolToken supplementaryDataToken = new CardsToken(selectedCardsUI);
					Synchronizer.SendGameAction(GameActionType.SelectExtraTurnCards, ActionPhaseType.ExtraTurnCardSelection, validateOnServerBeforeExecuting: false, disableAutoReplication: false, iD2, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
				}
			}
			OnSelectedCardsNumberChanged((!cardUI.IsLongRest) ? selectedCardsUI.Count : 0);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.OnCardDeselected().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00029", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void OnLoseCardClick()
	{
		try
		{
			Debug.Log("OnLoseCardClick");
			if (PhaseManager.PhaseType == CPhase.PhaseType.StartTurn || PhaseManager.PhaseType == CPhase.PhaseType.Action || PhaseManager.PhaseType == CPhase.PhaseType.EndTurn || PhaseManager.PhaseType == CPhase.PhaseType.EndRound)
			{
				CAbility currentAbility = Choreographer.s_Choreographer.m_CurrentAbility;
				if (currentAbility != null && currentAbility.AbilityType == CAbility.EAbilityType.LoseCards)
				{
					List<CAbilityCard> losingAbilityCards = selectedCardsUI.Select((AbilityCardUI x) => x.AbilityCard).ToList();
					GameState.PlayerSelectedCardsToLose(playerActor, losingAbilityCards);
					StartCoroutine(AnimateCardsLost(selectedCardsUI, delegate
					{
						SetMode(currentMode, CardPileType.Any, new List<CardPileType> { CardPileType.None }, fadeUnselectableCards: true, highlightSelectableCards: true);
						GameState.PlayerFinishedMovingCards(playerActor);
					}));
					if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl)
					{
						int iD = playerActor.ID;
						IProtocolToken supplementaryDataToken = new CardsToken(selectedCardsUI);
						Synchronizer.SendGameAction(GameActionType.AbilityLoseCard, ActionPhaseType.CardRecovery, validateOnServerBeforeExecuting: false, disableAutoReplication: false, iD, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
					}
				}
				else
				{
					CAbility currentAbility2 = Choreographer.s_Choreographer.m_CurrentAbility;
					if (currentAbility2 != null && currentAbility2.AbilityType == CAbility.EAbilityType.DiscardCards)
					{
						List<CAbilityCard> losingAbilityCards2 = selectedCardsUI.Select((AbilityCardUI x) => x.AbilityCard).ToList();
						GameState.PlayerSelectedCardsToDiscard(playerActor, losingAbilityCards2);
						StartCoroutine(AnimateCardsLost(selectedCardsUI, delegate
						{
							SetMode(currentMode, CardPileType.Any, new List<CardPileType> { CardPileType.None }, fadeUnselectableCards: true, highlightSelectableCards: true);
							GameState.PlayerFinishedMovingCards(playerActor);
						}));
						if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl)
						{
							int iD2 = playerActor.ID;
							IProtocolToken supplementaryDataToken = new CardsToken(selectedCardsUI);
							Synchronizer.SendGameAction(GameActionType.AbilityDiscardCard, ActionPhaseType.CardRecovery, validateOnServerBeforeExecuting: false, disableAutoReplication: false, iD2, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
						}
					}
					else
					{
						GameState.EAvoidDamageOption avoidDamageOption = GameState.EAvoidDamageOption.None;
						Singleton<UIActiveBonusBar>.Instance.Hide(toggle: true);
						if (maxCardsSelected == 2)
						{
							GameState.Lose2DiscardCardsToAvoidAttack(playerActor, selectedCardsUI[0].AbilityCard, selectedCardsUI[1].AbilityCard);
							avoidDamageOption = GameState.EAvoidDamageOption.Lose2DiscardCards;
						}
						else if (maxCardsSelected == 1)
						{
							GameState.Lose1HandCardToAvoidAttack(playerActor, selectedCardsUI[0].AbilityCard);
							avoidDamageOption = GameState.EAvoidDamageOption.Lose1HandCard;
						}
						StartCoroutine(AnimateCardsLost(selectedCardsUI, delegate
						{
							GameState.PlayerAvoidingDamage(avoidDamageOption);
							Hide();
						}));
						if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl)
						{
							int actionType = ((avoidDamageOption == GameState.EAvoidDamageOption.Lose2DiscardCards) ? 49 : 48);
							int iD3 = playerActor.ID;
							IProtocolToken supplementaryDataToken = new CardsToken(selectedCardsUI);
							Synchronizer.SendGameAction((GameActionType)actionType, ActionPhaseType.TakeDamageConfirmation, validateOnServerBeforeExecuting: false, disableAutoReplication: false, iD3, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
						}
					}
				}
			}
			if (PhaseManager.PhaseType == CPhase.PhaseType.ActionSelection || PhaseManager.PhaseType == CPhase.PhaseType.EndTurnLoot || PhaseManager.PhaseType == CPhase.PhaseType.StartRoundEffects)
			{
				if (playerActor.CharacterClass.LongRest && GameState.InternalCurrentActor == playerActor)
				{
					HandleLongRest(selectedCardsUI[0].AbilityCard);
					if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl)
					{
						Synchronizer.SendGameAction(GameActionType.LongRest, ActionPhaseType.LongRest, validateOnServerBeforeExecuting: false, disableAutoReplication: false, playerActor.ID, 0, 0, selectedCardsUI[0].AbilityCard.ID);
					}
				}
				else
				{
					GameState.EAvoidDamageOption avoidDamageOption2 = GameState.EAvoidDamageOption.None;
					Singleton<UIActiveBonusBar>.Instance.Hide(toggle: true);
					if (maxCardsSelected == 2)
					{
						GameState.Lose2DiscardCardsToAvoidAttack(playerActor, selectedCardsUI[0].AbilityCard, selectedCardsUI[1].AbilityCard);
						avoidDamageOption2 = GameState.EAvoidDamageOption.Lose2DiscardCards;
					}
					else if (maxCardsSelected == 1)
					{
						GameState.Lose1HandCardToAvoidAttack(playerActor, selectedCardsUI[0].AbilityCard);
						avoidDamageOption2 = GameState.EAvoidDamageOption.Lose1HandCard;
					}
					StartCoroutine(AnimateCardsLost(selectedCardsUI, delegate
					{
						GameState.PlayerAvoidingDamage(avoidDamageOption2);
						Hide();
					}));
					if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl)
					{
						int actionType2 = ((avoidDamageOption2 == GameState.EAvoidDamageOption.Lose2DiscardCards) ? 49 : 48);
						int iD4 = playerActor.ID;
						IProtocolToken supplementaryDataToken = new CardsToken(selectedCardsUI);
						Synchronizer.SendGameAction((GameActionType)actionType2, ActionPhaseType.TakeDamageConfirmation, validateOnServerBeforeExecuting: false, disableAutoReplication: false, iD4, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
					}
				}
			}
			if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest && playerActor.CharacterClass.ImprovedShortRest && playerActor.CharacterClass.LongRest)
			{
				HandleImprovedShortRest(selectedCardsUI[0].AbilityCard, playerActor);
				if (!FFSNetwork.IsOnline || playerActor.IsUnderMyControl)
				{
					ScenarioRuleClient.GetAndReplicateStartRoundDeckState(playerActor, 51);
				}
			}
			UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.ChooseLoseCardConfirmed));
			Singleton<TakeDamagePanel>.Instance.ResetAndHide(stopActiveBonus: true);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.OnLoseCardClick().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00030", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void HandleLongRest(CAbilityCard cardToBurn)
	{
		GameState.PlayerLongRested(cardToBurn);
		WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.ShowNone;
		StartCoroutine(AnimateCardsLost(selectedCardsUI, delegate
		{
			if (IsShown && PhaseManager.PhaseType == CPhase.PhaseType.ActionSelection)
			{
				CardsHandManager.Instance.ShowLongRested(playerActor);
				CardsHandManager.Instance.EnableCancelActiveAbilities = !FFSNetwork.IsOnline || playerActor.IsUnderMyControl;
			}
		}));
	}

	private void HandleImprovedShortRest(CAbilityCard cardToBurn, CPlayerActor playerActor, bool fromStateUpdate = false)
	{
		GameState.PlayerLongRested(cardToBurn, playerActor, improvedShortRestUsed: true, fromStateUpdate);
		WorldspaceStarHexDisplay.Instance.CurrentDisplayState = WorldspaceStarHexDisplay.WorldSpaceStarDisplayState.ShowNone;
		shortRested = true;
		UpdateShortRest();
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(AnimateCardsLost(selectedCardsUI, delegate
			{
				Debug.Log("HandleImprovedShortRest");
				UpdateView(CardHandMode.CardsSelection, CardPileType.Any, new List<CardPileType>
				{
					CardPileType.Hand,
					CardPileType.Round
				}, 2);
				CardsHandManager.Instance.m_PushPopCardHandMode = CardHandMode.CardsSelection;
			}));
		}
	}

	private void OnRecoverCardClick()
	{
		try
		{
			if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
			{
				List<CAbilityCard> recoveredAbilityCards = selectedCardsUI.Select((AbilityCardUI x) => x.AbilityCard).ToList();
				if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl)
				{
					int iD = playerActor.ID;
					IProtocolToken supplementaryDataToken = new CardsToken(selectedCardsUI);
					Synchronizer.SendGameAction(GameActionType.RecoverCards, ActionPhaseType.CardRecovery, validateOnServerBeforeExecuting: false, disableAutoReplication: false, iD, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
				}
				Debug.Log("OnRecoverCardClick");
				GameState.PlayerSelectedCardsToRecover(playerActor, recoveredAbilityCards, currentMode.GetRecoverECardPile());
				StartCoroutine(AnimateCardsLost(selectedCardsUI, delegate
				{
					SetMode(currentMode, CardPileType.Any, new List<CardPileType> { CardPileType.None }, fadeUnselectableCards: true, highlightSelectableCards: true);
					GameState.PlayerFinishedMovingCards(playerActor);
				}));
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.OnRecoverCardClick().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00031", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void OnInceaseLimitCardClick()
	{
		try
		{
			if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
			{
				List<CAbilityCard> increasingLimitAbilityCards = selectedCardsUI.Select((AbilityCardUI x) => x.AbilityCard).ToList();
				if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl)
				{
					int iD = playerActor.ID;
					IProtocolToken supplementaryDataToken = new CardsToken(selectedCardsUI);
					Synchronizer.SendGameAction(GameActionType.RecoverCards, ActionPhaseType.CardRecovery, validateOnServerBeforeExecuting: false, disableAutoReplication: false, iD, 0, 0, 0, supplementaryDataBoolean: true, default(Guid), supplementaryDataToken);
				}
				Debug.Log("OnInceaseLimitCardClick");
				GameState.PlayerSelectedCardsToIncreaseLimit(playerActor, increasingLimitAbilityCards);
				StartCoroutine(AnimateCardsLost(selectedCardsUI, delegate
				{
					SetMode(currentMode, CardPileType.Any, new List<CardPileType> { CardPileType.None }, fadeUnselectableCards: true, highlightSelectableCards: true);
					GameState.PlayerFinishedMovingCards(playerActor);
				}));
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.OnInceaseLimitCardClick().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00031", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void OnSelectExtraTurnCardsClick()
	{
		try
		{
			if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
			{
				List<CAbilityCard> extraTurnCards = selectedCardsUI.Select((AbilityCardUI x) => x.AbilityCard).ToList();
				GameState.PlayerSelectedExtraTurnCards(playerActor, extraTurnCards);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.OnSelectExtraTurnCardsClick().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00031", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void OnSelectedCardsNumberChanged(int number)
	{
		try
		{
			CardsHandManager.Instance.OnSelectedCardsNumberChanged(playerActor, number);
			if (maxCardsSelected > 1)
			{
				for (int i = 0; i < selectedCardsUI.Count; i++)
				{
					selectedCardsUI[i].MiniAbilityCard.ShowIndicatorNumber(selectedCardsUI.Count - i);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.OnSelectedCardsNumberChanged().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00032", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void RefreshCancelActiveAbility(CardPileType type, CAbilityCard disabledAbilityCard, bool animateDisabling)
	{
		try
		{
			AbilityCardUI cardUI = GetCardUI(disabledAbilityCard);
			if (!(cardUI != null))
			{
				return;
			}
			if (animateDisabling && base.gameObject.activeSelf)
			{
				if (AnimatingLostCards || playerActor.CharacterClass.ActivatedCards.Contains(disabledAbilityCard))
				{
					return;
				}
				StartCoroutine(AnimateCardsLost(new List<AbilityCardUI> { cardUI }, delegate
				{
					cardUI.SetSelectable(isOn: false);
					if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
					{
						UpdateShortRest();
						ResetViewToCachedSettings();
					}
					Singleton<UINavigation>.Instance.NavigationManager.TrySelectFirstIn(Singleton<UINavigation>.Instance.NavigationManager.CurrentNavigationRoot);
				}));
			}
			else
			{
				UpdateCards(type, EUpdateCardViewMode.Refresh);
				SortCards();
				cardUI.SetSelectable(isOn: false);
				if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
				{
					UpdateShortRest();
					ResetViewToCachedSettings();
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred within the CardHandsUI.RefreshCancelActiveAbility().\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00033", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void SetInteractable(bool interactable, bool allowHover)
	{
		canvasGroup.interactable = interactable;
		canvasGroup.blocksRaycasts = allowHover;
	}

	public void SelectCard(CAbilityCard card)
	{
		foreach (AbilityCardUI item in cardsUI)
		{
			if (card == item.AbilityCard)
			{
				if (!item.IsSelected)
				{
					item.OnClick(ignoreHiglight: true);
				}
				break;
			}
		}
	}

	public void UnselectCard(CAbilityCard card)
	{
		foreach (AbilityCardUI item in cardsUI)
		{
			if (card == item.AbilityCard)
			{
				if (item.IsSelected)
				{
					item.OnClick(ignoreHiglight: true);
				}
				break;
			}
		}
	}

	public List<InfuseElement> PickUnselectedInfusionsForItem(CItem item)
	{
		return itemsUI.PickUnselectedInfusionsForItem(item);
	}

	public void ResetItemElements(CItem item)
	{
		itemsUI.ResetItemElements(item);
	}

	public AbilityCardUI GetCard(int cardID)
	{
		try
		{
			AbilityCardUI abilityCardUI = cardsUI.SingleOrDefault((AbilityCardUI s) => s != null && s.CardID == cardID);
			if (abilityCardUI == null)
			{
				Debug.LogError("Could not find a card with ID " + cardID + " from actor with ID: " + ((playerActor != null) ? playerActor.ID.ToString() : "null"));
			}
			return abilityCardUI;
		}
		catch (Exception ex)
		{
			Debug.LogError("Unable to get card with id " + cardID + "\n" + ex.Message + "\n" + ex.StackTrace);
			return null;
		}
	}

	public bool IsLongRestSelected()
	{
		if (selectedCardsUI != null && selectedCardsUI.Count == 1)
		{
			return selectedCardsUI[0].CardID == -1;
		}
		return false;
	}

	public bool IsShortRestSelected()
	{
		return shortRest.IsSelected;
	}

	public bool IsCardSelected(int cardID)
	{
		if (selectedCardsUI != null)
		{
			return selectedCardsUI.Exists((AbilityCardUI e) => e.CardID == cardID);
		}
		return false;
	}

	public void NetworkSelectedRoundCards()
	{
		if (FFSNetwork.IsOnline && !playerActor.IsUnderMyControl)
		{
			return;
		}
		CAbilityCard initiativeAbilityCard = playerActor.CharacterClass.InitiativeAbilityCard;
		List<AbilityCardUI> list = selectedCardsUI;
		if (initiativeAbilityCard != null && selectedCardsUI != null && selectedCardsUI.Count == 2)
		{
			FFSNet.Console.LogInfo("Checking initiative \n Initiative Card ID: " + initiativeAbilityCard.ID + " Initiative CardInstanceID: + " + initiativeAbilityCard.CardInstanceID + "\nSelectedCards[0] ID: " + (selectedCardsUI[0].IsLongRest ? "LongRest" : selectedCardsUI[0].AbilityCard.ID.ToString()) + " SelectedCards[0] CardInstanceID: " + (selectedCardsUI[0].IsLongRest ? "LongRest" : selectedCardsUI[0].AbilityCard.CardInstanceID.ToString()) + "\nSelectedCards[1] ID: " + (selectedCardsUI[1].IsLongRest ? "LongRest" : selectedCardsUI[1].AbilityCard.ID.ToString()) + " SelectedCards[1] CardInstanceID: " + (selectedCardsUI[1].IsLongRest ? "LongRest" : selectedCardsUI[1].AbilityCard.CardInstanceID.ToString()));
			if (!selectedCardsUI[1].IsLongRest && selectedCardsUI[1].AbilityCard.ID != initiativeAbilityCard.ID)
			{
				list.Reverse();
			}
		}
		FFSNet.Console.LogDebug("---> NEW BACKEND CARDS SELECTED FOR " + playerActor.CharacterClass.ID, customFlag: true);
		FFSNet.Console.LogDebug("ROUND: " + playerActor.CharacterClass.RoundAbilityCards.Select((CAbilityCard x) => x.StrictName + " CardInstanceID: " + x.CardInstanceID).ToStringPretty(), customFlag: true);
		FFSNet.Console.LogDebug("HAND: " + playerActor.CharacterClass.HandAbilityCards.Select((CAbilityCard x) => x.StrictName + " CardInstanceID: " + x.CardInstanceID).ToStringPretty(), customFlag: true);
		FFSNet.Console.LogDebug("DISCARDED: " + playerActor.CharacterClass.DiscardedAbilityCards.Select((CAbilityCard x) => x.StrictName + " CardInstanceID: " + x.CardInstanceID).ToStringPretty(), customFlag: true);
		FFSNet.Console.LogDebug("BURNED: " + playerActor.CharacterClass.LostAbilityCards.Select((CAbilityCard x) => x.StrictName + " CardInstanceID: " + x.CardInstanceID).ToStringPretty(), customFlag: true);
		FFSNet.Console.LogDebug("ACTIVE: " + playerActor.CharacterClass.ActivatedAbilityCards.Select((CAbilityCard x) => x.StrictName + " CardInstanceID: " + x.CardInstanceID).ToStringPretty(), customFlag: true);
		FFSNet.Console.LogDebug("<---", customFlag: true);
		if (!AutoTestController.s_AutoTestCurrentlyLoaded)
		{
			ScenarioRuleClient.GetAndReplicateStartRoundDeckState(playerActor, 32);
		}
	}

	public void ProxySelectCard(int cardInstanceID, bool isHandUnderMyControl = true)
	{
		AbilityCardUI abilityCardUI = cardsUI.Find((AbilityCardUI x) => x != null && x.IsMatchingCard(cardInstanceID));
		if (abilityCardUI != null)
		{
			abilityCardUI.ToggleSelect(active: true, highlight: false, networkAction: false, isHandUnderMyControl);
			return;
		}
		throw new Exception("Error with proxy card selection. Could not find a card with CardInstanceID " + cardInstanceID + " from actor with ID: " + playerActor.ID);
	}

	public void ProxySelectCardAction(int cardInstanceID, CBaseCard.ActionType cardActionType)
	{
		AbilityCardUI abilityCardUI = cardsUI.Find((AbilityCardUI x) => (object)x != null && x.AbilityCard?.CardInstanceID == cardInstanceID);
		if (abilityCardUI != null)
		{
			abilityCardUI.fullAbilityCard.OnAbilityClick(cardActionType, isProxyAction: true, checkValid: false);
			return;
		}
		throw new Exception("Error selecting an ability for a proxy playerActor. Could not find a card with CardInstanceID " + cardInstanceID + " from actor with ID: " + playerActor.ID);
	}

	public void ProxyBurnOneAvailableCard(int burnedCardID, bool animateCardLose = false)
	{
		AbilityCardUI abilityCardUI = cardsUI.Find((AbilityCardUI x) => (object)x != null && x.AbilityCard?.ID == burnedCardID);
		if (abilityCardUI != null)
		{
			selectedCardsUI.Insert(0, abilityCardUI);
			GameState.Lose1HandCardToAvoidAttack(playerActor, selectedCardsUI[0].AbilityCard);
			if (animateCardLose)
			{
				StartCoroutine(AnimateCardsLost(selectedCardsUI, delegate
				{
					GameState.PlayerAvoidingDamage(GameState.EAvoidDamageOption.Lose1HandCard);
					Hide();
				}));
			}
			else
			{
				CoroutineHelper.RunCoroutine(DelayCardLost(GameState.EAvoidDamageOption.Lose1HandCard));
			}
			return;
		}
		throw new Exception("Error trying to burn an available card for a proxy playerActor. Could not find a card with ID " + burnedCardID + " from actor with ID: " + playerActor.ID);
	}

	private IEnumerator DelayCardLost(GameState.EAvoidDamageOption option)
	{
		yield return null;
		InstantCardLost(selectedCardsUI);
		GameState.PlayerAvoidingDamage(option);
		Hide();
	}

	public void ProxyBurnTwoDiscardedCards(int burnedCard1ID, int burnedCard2ID, bool animateCardLose = false)
	{
		AbilityCardUI abilityCardUI = cardsUI.Find((AbilityCardUI x) => (object)x != null && x.AbilityCard?.ID == burnedCard1ID);
		if (abilityCardUI != null)
		{
			selectedCardsUI.Insert(0, abilityCardUI);
			AbilityCardUI abilityCardUI2 = cardsUI.Find((AbilityCardUI x) => (object)x != null && x.AbilityCard?.ID == burnedCard2ID);
			if (abilityCardUI2 != null)
			{
				selectedCardsUI.Insert(1, abilityCardUI2);
				GameState.Lose2DiscardCardsToAvoidAttack(playerActor, selectedCardsUI[0].AbilityCard, selectedCardsUI[1].AbilityCard);
				if (animateCardLose)
				{
					StartCoroutine(AnimateCardsLost(selectedCardsUI, delegate
					{
						GameState.PlayerAvoidingDamage(GameState.EAvoidDamageOption.Lose2DiscardCards);
						Hide();
					}));
				}
				else
				{
					CoroutineHelper.RunCoroutine(DelayCardLost(GameState.EAvoidDamageOption.Lose2DiscardCards));
				}
				return;
			}
			throw new Exception("Error trying to burn the second discarded card for a proxy playerActor. Could not find a card with ID " + burnedCard2ID + " from actor with ID: " + playerActor.ID);
		}
		throw new Exception("Error trying to burn the first discarded card for a proxy playerActor. Could not find a card with ID " + burnedCard1ID + " from actor with ID: " + playerActor.ID);
	}

	public void ProxyShortRest(StartRoundCardsToken startRoundCardToken, bool fromStateUpdate = false)
	{
		if (!playerActor.CharacterClass.HasShortRested && startRoundCardToken.HasShortRested)
		{
			AbilityCardUI abilityCardUI = cardsUI.Find((AbilityCardUI x) => x?.AbilityCard?.ID == startRoundCardToken.CardBurnedID);
			if (!(abilityCardUI != null))
			{
				throw new Exception("Error using short rest for a proxy playerActor. Could not find a card with ID " + startRoundCardToken.CardBurnedID + " from actor with ID: " + playerActor.ID);
			}
			if (startRoundCardToken.HasImprovedShortRested)
			{
				HandleImprovedShortRest(abilityCardUI.AbilityCard, playerActor, fromStateUpdate);
			}
			else
			{
				FinalizeShortRest(playerActor, abilityCardUI, startRoundCardToken.ShortRestCardRedrawn, networkActionIfOnline: false, fromStateUpdate);
			}
			FFSNet.Console.LogInfo("Short rest used for " + playerActor.CharacterClass.ID);
		}
	}

	public void ProxyLongRest(int burnedCardID)
	{
		AbilityCardUI abilityCardUI = cardsUI.Find((AbilityCardUI x) => (object)x != null && x.AbilityCard?.ID == burnedCardID);
		if (abilityCardUI != null)
		{
			selectedCardsUI.Insert(0, abilityCardUI);
			HandleLongRest(abilityCardUI.AbilityCard);
			return;
		}
		throw new Exception("Error using long rest for a proxy playerActor. Could not find a card with ID " + burnedCardID + " from actor with ID: " + playerActor.ID);
	}

	public void ProxyRecoverCards(CardsToken cardsToken, bool increaseCardHandSize)
	{
		int i;
		for (i = 0; i < cardsToken.CardIDs.Length; i++)
		{
			AbilityCardUI abilityCardUI = cardsUI.Find((AbilityCardUI x) => x?.AbilityCard?.ID == cardsToken.CardIDs[i]);
			if (abilityCardUI != null)
			{
				selectedCardsUI.Insert(0, abilityCardUI);
				continue;
			}
			throw new Exception("Error recovering cards for a proxy playerActor. Could not find a card with ID " + cardsToken.CardIDs[i] + " from actor with ID: " + playerActor.ID);
		}
		Choreographer.s_Choreographer.readyButton.AlternativeAction(increaseCardHandSize ? new Action(OnInceaseLimitCardClick) : new Action(OnRecoverCardClick));
		Choreographer.s_Choreographer.readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONRECOVERCARD, LocalizationManager.GetTranslation("GUI_CONFIRM"), hideOnClick: true, glowingEffect: true);
		Choreographer.s_Choreographer.m_SkipButton.Toggle(active: false);
		OnSelectedCardsNumberChanged(selectedCardsUI.Count);
		Choreographer.s_Choreographer.readyButton.OnClickInternal(networkActionIfOnline: false);
	}

	public void ProxyAbilityDiscardCard(CardsToken cardsToken)
	{
		List<CAbilityCard> list = new List<CAbilityCard>();
		int i;
		for (i = 0; i < cardsToken.CardIDs.Length; i++)
		{
			list.Add(cardsUI.Find((AbilityCardUI x) => x.AbilityCard.ID == cardsToken.CardIDs[i]).AbilityCard);
		}
		Debug.Log("ProxyAbilityDiscardCard");
		GameState.PlayerSelectedCardsToDiscard(playerActor, list);
		StartCoroutine(AnimateCardsLost(selectedCardsUI, delegate
		{
			SetMode(currentMode, CardPileType.Any, new List<CardPileType> { CardPileType.None }, fadeUnselectableCards: true, highlightSelectableCards: true);
			GameState.PlayerFinishedMovingCards(playerActor);
		}));
	}

	public void ProxyAbilityLoseCard(CardsToken cardsToken)
	{
		List<CAbilityCard> list = new List<CAbilityCard>();
		int i;
		for (i = 0; i < cardsToken.CardIDs.Length; i++)
		{
			list.Add(cardsUI.Find((AbilityCardUI x) => x.AbilityCard.ID == cardsToken.CardIDs[i]).AbilityCard);
		}
		Debug.Log("ProxyAbilityLoseCard");
		GameState.PlayerSelectedCardsToLose(playerActor, list);
		StartCoroutine(AnimateCardsLost(selectedCardsUI, delegate
		{
			SetMode(currentMode, CardPileType.Any, new List<CardPileType> { CardPileType.None }, fadeUnselectableCards: true, highlightSelectableCards: true);
			GameState.PlayerFinishedMovingCards(playerActor);
		}));
	}

	public void ControllerFocus(Selectable first = null)
	{
		Debug.LogController($"Focus Card Hand {playerActor?.CharacterClass?.CharacterModel}");
		if (!IsInteractable)
		{
			return;
		}
		if (InputManager.GamePadInUse)
		{
			CardHandMode cardHandMode = currentMode;
			if (cardHandMode == CardHandMode.LoseCard || cardHandMode == CardHandMode.DiscardCard)
			{
				UiNavigationManager navigationManager = Singleton<UINavigation>.Instance.NavigationManager;
				navigationManager.SetCurrentRoot("CardHands");
				IUiNavigationSelectable uiNavigationSelectable = UiNavigationUtils.FindFirstSelectableFiltered(navigationManager.CurrentNavigationRoot.Elements, ActiveAndInteractableFilter);
				navigationManager.TrySelect(uiNavigationSelectable);
			}
			if (currentMode != CardHandMode.ActionSelection)
			{
				return;
			}
			UiNavigationManager navigationManager2 = Singleton<UINavigation>.Instance.NavigationManager;
			navigationManager2.SetCurrentRoot("CardHands");
			if (UiNavigationUtils.FindFirstElementFiltered(navigationManager2.CurrentNavigationRoot.Elements, ActiveAndMainButtonFilter) == null)
			{
				navigationManager2.SetCurrentRoot("CardsHighlight");
			}
			IUiNavigationElement uiNavigationElement = null;
			navigationManager2.GetTaggedElements("AbilityCardTopButton", _navigationElementsBuffer, ActiveAndInteractableFilter);
			if (_navigationElementsBuffer.Count > 0)
			{
				uiNavigationElement = _navigationElementsBuffer[0];
			}
			if (uiNavigationElement == null)
			{
				navigationManager2.GetTaggedElements("AbilityCardBottomButton", _navigationElementsBuffer, ActiveAndInteractableFilter);
				if (_navigationElementsBuffer.Count > 0)
				{
					uiNavigationElement = _navigationElementsBuffer[0];
				}
			}
			bool flag = false;
			if (uiNavigationElement is IUiNavigationNode navigationNode)
			{
				flag = navigationManager2.TrySelectFirstIn(navigationNode);
			}
			else if (uiNavigationElement is IUiNavigationSelectable uiNavigationSelectable2)
			{
				flag = navigationManager2.TrySelect(uiNavigationSelectable2);
			}
			if (uiNavigationElement != null && !flag)
			{
				Debug.LogWarning("Object could not be selected properly - " + uiNavigationElement.GameObject.name + ";", uiNavigationElement.GameObject);
			}
			else if (uiNavigationElement == null)
			{
				Debug.LogWarning("Can't find active and interactable main ability button;");
			}
			return;
		}
		if (currentMode == CardHandMode.ActionSelection)
		{
			List<AbilityCardUI> list = (from it in cardsUI
				where it.gameObject.activeSelf && it.fullAbilityCard.HasActionInteractable()
				orderby it.transform.GetSiblingIndex()
				select it).ToList();
			for (int num = 0; num < list.Count; num++)
			{
				AbilityCardUI abilityCardUI = ((num == 0) ? list.Last() : list[num - 1]);
				AbilityCardUI abilityCardUI2 = ((num == list.Count - 1) ? list[0] : list[num + 1]);
				list[num].fullAbilityCard.EnableNavigation(left: modifiersSelectable, up: (abilityCardUI == list[num]) ? null : abilityCardUI.fullAbilityCard.bottomActionButton.actionButton, down: (abilityCardUI2 == list[num]) ? null : abilityCardUI2.fullAbilityCard.topActionButton.actionButton);
				if (num == 0)
				{
					first = list[num].fullAbilityCard.GetFirstSelectable();
				}
			}
		}
		else
		{
			List<Tuple<Selectable, int>> list2 = new List<Tuple<Selectable, int>>();
			List<Selectable> list3 = new List<Selectable>();
			for (int num2 = 0; num2 < cardsUI.Count; num2++)
			{
				if (cardsUI[num2].gameObject.activeSelf && cardsUI[num2].IsHoverable)
				{
					if (givenCardsGroup.IsGivenCard(cardsUI[num2]))
					{
						list3.Add(cardsUI[num2].GetComponent<Selectable>());
					}
					else
					{
						list2.Add(new Tuple<Selectable, int>(cardsUI[num2].GetComponent<Selectable>(), cardsUI[num2].transform.GetSiblingIndex()));
					}
				}
			}
			if (shortRest.gameObject.activeSelf)
			{
				list2.Add(new Tuple<Selectable, int>(shortRest.Button, shortRest.transform.GetSiblingIndex()));
			}
			List<Selectable> list4 = (from it in list2
				orderby it.Item2
				select it.Item1).ToList();
			for (int num3 = 0; num3 < list4.Count; num3++)
			{
				list4[num3].SetNavigation(left: modifiersSelectable, down: list4[(num3 + 1) % list4.Count], up: list4[(num3 == 0) ? (list4.Count - 1) : (num3 - 1)], right: list3.FirstOrDefault());
			}
			for (int num4 = 0; num4 < list3.Count; num4++)
			{
				list3[num4].SetNavigation(null, list4.FirstOrDefault(), down: list3[(num4 + 1) % list3.Count], up: list3[(num4 == 0) ? (list3.Count - 1) : (num4 - 1)]);
			}
			if ((EventSystem.current.currentSelectedGameObject == null || !list4.Exists((Selectable it) => it.gameObject == EventSystem.current.currentSelectedGameObject)) && list4.Count > 0)
			{
				first = list4[0];
			}
		}
		modifiersSelectable.SetNavigation(new NavigationCalculator
		{
			right = ((first == null) ? null : ((Func<Selectable>)(() => first))),
			down = itemsUI.GetFirstEquippedItem
		});
		itemsUI.EnableNavigation(modifiersSelectable, first);
		if (first != null)
		{
			CancelSelectFirstElementCoroutine();
			selectFirstElementCoroutine = StartCoroutine(SelectObject(first.gameObject));
		}
		else if (EventSystem.current.currentSelectedGameObject == null)
		{
			CancelSelectFirstElementCoroutine();
			modifiersSelectable.Select();
		}
		static bool ActiveAndInteractableFilter(IUiNavigationElement navigationElement)
		{
			if (!navigationElement.GameObject.activeInHierarchy)
			{
				return false;
			}
			if (navigationElement is IUiNavigationSelectable uiNavigationSelectable3)
			{
				return uiNavigationSelectable3.ControlledSelectable.IsInteractable();
			}
			if (navigationElement is IUiNavigationNode uiNavigationNode)
			{
				if (uiNavigationNode.Elements.Count > 0 && uiNavigationNode.Elements[0] is IUiNavigationSelectable uiNavigationSelectable4)
				{
					return uiNavigationSelectable4.ControlledSelectable.IsInteractable();
				}
				return false;
			}
			return false;
		}
		static bool ActiveAndMainButtonFilter(IUiNavigationElement navigationElement)
		{
			if (!navigationElement.GameObject.activeInHierarchy)
			{
				return false;
			}
			if (navigationElement.NavigationTags != null)
			{
				return navigationElement.NavigationTags.Contains("AbilityCardMainButton");
			}
			return false;
		}
	}

	public void ControllerUnfocus()
	{
		Debug.LogController($"Unfocused Card Hand {playerActor?.CharacterClass?.CharacterModel}");
		if (InputManager.GamePadInUse)
		{
			Singleton<UINavigation>.Instance.NavigationManager.DeselectCurrentSelectable();
			if (_currentCard != null)
			{
				_currentCard.OnPointerExit();
			}
			return;
		}
		CancelSelectFirstElementCoroutine();
		cardsUI.ForEach(delegate(AbilityCardUI it)
		{
			it.DisableNavigation();
		});
		shortRest.Button.DisableNavigation();
		itemsUI.DisableNavigation();
		modifiersSelectable.DisableNavigation();
		EventSystem.current.SetSelectedGameObject(null);
	}

	private void CancelSelectFirstElementCoroutine()
	{
		if (selectFirstElementCoroutine != null)
		{
			StopCoroutine(selectFirstElementCoroutine);
			selectFirstElementCoroutine = null;
		}
	}

	private IEnumerator SelectObject(GameObject selectable)
	{
		yield return null;
		selectFirstElementCoroutine = null;
		if (IsShown && InputManager.GamePadInUse)
		{
			EventSystem.current.SetSelectedGameObject(selectable);
		}
	}
}
