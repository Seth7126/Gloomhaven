#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using FFSNet;
using GLOOM;
using JetBrains.Annotations;
using Photon.Bolt;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.ScenarioStates;
using SharedLibrary.SimpleLog;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CardsHandManager : MonoBehaviour
{
	private const string LocalizationEndSelectionKey = "Consoles/GUI_HOTKEY_END_SELECTION";

	public CPlayerActor m_PushPopCardHandManagerPlayer;

	public CardHandMode m_PushPopCardHandMode;

	public CardPileType m_PushPopCardPileType;

	[SerializeField]
	private GameObject cardsHandPrefab;

	[SerializeField]
	private Transform holderTransform;

	[SerializeField]
	private Transform highlightTransform;

	[SerializeField]
	private CardsHandTabs handTabs;

	[SerializeField]
	private Button viewAllDecksButton;

	[SerializeField]
	private UIWindow window;

	public CardsActionControlller cardsActionController;

	[SerializeField]
	private Image characterPreview;

	[SerializeField]
	private UICharacterStats characterStats;

	[SerializeField]
	[FormerlySerializedAs("previewCardsWindow")]
	private CardsHandPreviewWindow previewDecksWindow;

	[SerializeField]
	private LongRestConfirmationButton longRestConfirmationButton;

	[NonSerialized]
	public List<CardsHandUI> cardHandsUI = new List<CardsHandUI>();

	[Header("Controller")]
	[SerializeField]
	private ControllerInputArea controllerInputArea;

	[SerializeField]
	private List<GameObject> controllerIndicators;

	[SerializeField]
	private Hotkey _previousTabHotkey;

	[SerializeField]
	private Hotkey _nextTabHotkey;

	[SerializeField]
	private string _showAllDecksCombo;

	[SerializeField]
	private string _showAllCardsCombo;

	[SerializeField]
	private MonoHotkeySession _returnToActionSelectHotkeys;

	private CardsHandUI currentHand;

	private readonly Color32 _endSelectionTextColor = new Color32(243, 221, 171, byte.MaxValue);

	private bool isPreviewing;

	private bool isFullCardPreviewAlllowed;

	private CPlayerActor activePlayer;

	private CPlayerActor previewPlayer;

	private bool enableCancelActiveAbilities = true;

	private List<CardPileType> selectableCardTypes;

	private int maxCardsSelected;

	private bool fadeUnselectableCards;

	private bool highlightSelectableCards;

	private CardsHandUI.CardActionsCommand resetCardActionsPhase;

	private bool isFullDeckPreviewAllowed;

	private bool isShown;

	private bool restoreVisibility;

	private KeyActionHandler _keyActionHandler;

	private SimpleKeyActionHandlerBlocker _simpleKeyActionHandlerBlocker;

	private SkipFrameKeyActionHandlerBlocker _skipFrameKeyActionHandlerBlocker;

	public static CardsHandManager Instance { get; private set; }

	public ControllerInputArea ControllerInputArea => controllerInputArea;

	public List<CardsHandUI> CardHandsUI => cardHandsUI;

	public LongRestConfirmationButton LongRestConfirmationButton => longRestConfirmationButton;

	public CharacterSymbolTab CurrentCharacterSymbolTab => handTabs.CurrentCharacterTab;

	public CardsHandUI CurrentHand => currentHand;

	public MonoHotkeySession ReturnToActionSelectHotkeys => _returnToActionSelectHotkeys;

	public bool IsFullCardPreviewShowing => cardHandsUI.Any((CardsHandUI a) => a.IsFullCardPreviewShowing);

	public CPlayerActor ActivePlayer => activePlayer;

	private bool IsAllDecksInteractable
	{
		get
		{
			if (m_PushPopCardHandMode != CardHandMode.CardsSelection && m_PushPopCardHandMode != CardHandMode.ActionSelection)
			{
				return m_PushPopCardHandMode == CardHandMode.LoseCard;
			}
			return true;
		}
	}

	public bool IsShown => isShown;

	public bool EnableCancelActiveAbilities
	{
		get
		{
			return enableCancelActiveAbilities;
		}
		set
		{
			enableCancelActiveAbilities = value;
			InitiativeTrack.Instance.SetActiveBonusInteractable(enableCancelActiveAbilities);
		}
	}

	public bool IsFullCardPreviewAllowed
	{
		set
		{
			isFullCardPreviewAlllowed = value;
			if (!isFullCardPreviewAlllowed)
			{
				CloseViewAllCards();
			}
		}
	}

	public bool IsFullDeckPreviewAllowed
	{
		get
		{
			return isFullDeckPreviewAllowed;
		}
		set
		{
			isFullDeckPreviewAllowed = value;
			viewAllDecksButton.gameObject.SetActive(value);
			if (!isFullDeckPreviewAllowed)
			{
				previewDecksWindow.Cancel();
			}
		}
	}

	[UsedImplicitly]
	private void Awake()
	{
		Instance = this;
		characterPreview.gameObject.SetActive(value: false);
		if (SaveData.Instance.Global.GameMode != EGameMode.LevelEditor)
		{
			if (InputManager.GamePadInUse)
			{
				InitGamepadInput();
				Singleton<InputManager>.Instance.ButtonsComboController.AddCombo(_showAllCardsCombo, OnShowAllCardsComboPressed);
				EnableAllCardsCombo(value: false);
			}
			viewAllDecksButton.onClick.AddListener(delegate
			{
				ShowAllDecks();
			});
		}
		if (FFSNetwork.IsOnline && PlayerRegistry.MyPlayer != null)
		{
			SubscribeToControllableChanges();
		}
		else
		{
			SubscribeToMyPlayerInitialized();
		}
		if (SaveData.Instance.Global.GameMode != EGameMode.LevelEditor)
		{
			longRestConfirmationButton.Init();
		}
		SetControllerIndicatorsActive(active: true);
		if (InputManager.GamePadInUse)
		{
			_previousTabHotkey.Initialize(Singleton<UINavigation>.Instance.Input);
			_nextTabHotkey.Initialize(Singleton<UINavigation>.Instance.Input);
		}
		else
		{
			if (_previousTabHotkey != null)
			{
				_previousTabHotkey.gameObject.SetActive(value: false);
			}
			if (_nextTabHotkey != null)
			{
				_nextTabHotkey.gameObject.SetActive(value: false);
			}
		}
		if (controllerInputArea != null)
		{
			controllerInputArea.OnFocused.AddListener(delegate
			{
				if (window.IsOpen && currentHand != null && currentHand.IsInteractable)
				{
					OnControllerAreaFocused();
				}
			});
			controllerInputArea.OnUnfocused.AddListener(OnControllerAreaUnfocused);
			window.onShown.AddListener(OnWindowShown);
		}
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.DISPLAY_CARDS_HERO_1, delegate
		{
			ShowAllCards(0);
		}));
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.DISPLAY_CARDS_HERO_1, CloseViewAllCards, null, null, isPersistent: false, KeyActionHandler.RegisterType.Release));
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.DISPLAY_CARDS_HERO_2, delegate
		{
			ShowAllCards(1);
		}));
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.DISPLAY_CARDS_HERO_2, CloseViewAllCards, null, null, isPersistent: false, KeyActionHandler.RegisterType.Release));
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.DISPLAY_CARDS_HERO_3, delegate
		{
			ShowAllCards(2);
		}));
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.DISPLAY_CARDS_HERO_3, CloseViewAllCards, null, null, isPersistent: false, KeyActionHandler.RegisterType.Release));
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.DISPLAY_CARDS_HERO_4, delegate
		{
			ShowAllCards(3);
		}));
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.DISPLAY_CARDS_HERO_4, CloseViewAllCards, null, null, isPersistent: false, KeyActionHandler.RegisterType.Release));
	}

	private void ShowAllCards(int index)
	{
		if (ScenarioManager.Scenario.PlayerActors.Count > index)
		{
			ToggleViewAllCards(ScenarioManager.Scenario.PlayerActors[index], openByKey: true);
		}
	}

	public void SetControllerIndicatorsActive(bool active)
	{
		for (int i = 0; i < controllerIndicators.Count; i++)
		{
			controllerIndicators[i].SetActive(active);
		}
	}

	private void OnWindowShown()
	{
		if (controllerInputArea != null && controllerInputArea.IsFocused)
		{
			OnControllerAreaFocused();
		}
		string translation = LocalizationManager.GetTranslation("Consoles/GUI_HOTKEY_END_SELECTION");
		Singleton<UIReadyToggle>.Instance.InitializeLabelGamepad(translation, _endSelectionTextColor);
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		if (previewDecksWindow != null)
		{
			previewDecksWindow.ReleasePreviews();
		}
		cardHandsUI.Clear();
		if (InputManager.GamePadInUse)
		{
			DeinitGamepadInput();
			if (Singleton<InputManager>.Instance != null)
			{
				Singleton<InputManager>.Instance.ButtonsComboController.RemoveCombo(_showAllDecksCombo);
				Singleton<InputManager>.Instance.ButtonsComboController.RemoveCombo(_showAllCardsCombo);
			}
		}
		if (FFSNetwork.IsOnline && PlayerRegistry.MyPlayer != null)
		{
			UnsubscribeFromControllableChanges();
		}
		else
		{
			UnsubscribeFromMyPlayerInitialized();
		}
		if (controllerInputArea != null)
		{
			controllerInputArea.OnFocused.RemoveListener(OnControllerAreaFocused);
			controllerInputArea.OnUnfocused.RemoveListener(OnControllerAreaUnfocused);
		}
		if (InputManager.GamePadInUse)
		{
			_previousTabHotkey.Deinitialize();
			_nextTabHotkey.Deinitialize();
		}
		if (PlayerRegistry.MyPlayer != null)
		{
			PlayerRegistry.MyPlayer.MyControllables.CollectionChanged -= OnMyControllableOwnershipChanged;
		}
		if (viewAllDecksButton != null)
		{
			viewAllDecksButton.onClick.RemoveAllListeners();
		}
		if (TooltipsVisibilityHelper.Instance != null)
		{
			TooltipsVisibilityHelper.Instance.RemoveTooltipRequest(this);
		}
		Instance = null;
	}

	public void InitGamepadInput()
	{
		if (InputManager.GamePadInUse)
		{
			_simpleKeyActionHandlerBlocker = new SimpleKeyActionHandlerBlocker(isBlock: true);
			_skipFrameKeyActionHandlerBlocker = new SkipFrameKeyActionHandlerBlocker(this);
			Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_ALL_CARDS, OnAllDeckButtonPressed).AddBlocker(_simpleKeyActionHandlerBlocker).AddBlocker(_skipFrameKeyActionHandlerBlocker));
		}
	}

	public void DeinitGamepadInput()
	{
		if (InputManager.GamePadInUse)
		{
			_simpleKeyActionHandlerBlocker = null;
			_skipFrameKeyActionHandlerBlocker = null;
			if (Singleton<KeyActionHandlerController>.Instance != null)
			{
				Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_ALL_CARDS, OnAllDeckButtonPressed);
			}
		}
	}

	public void EnableInput()
	{
		_simpleKeyActionHandlerBlocker?.SetBlock(value: false);
		_skipFrameKeyActionHandlerBlocker?.Run();
	}

	public void DisableInput()
	{
		_simpleKeyActionHandlerBlocker?.SetBlock(value: true);
	}

	public void EnableAllDeckSelection(bool isEnabled)
	{
		_simpleKeyActionHandlerBlocker?.SetBlock(!isEnabled);
		_skipFrameKeyActionHandlerBlocker?.Run();
	}

	private void OnAllDeckButtonPressed()
	{
		if (isFullDeckPreviewAllowed && TransitionManager.s_Instance.TransitionDone && !ScenarioRuleClient.IsProcessingOrMessagesQueued && PhaseManager.Phase.Type != CPhase.PhaseType.EndTurn)
		{
			ShowAllDecks();
		}
	}

	public void EnableAllCardsCombo(bool value)
	{
		Singleton<InputManager>.Instance.ButtonsComboController.SetEnabledCombo(_showAllCardsCombo, value);
	}

	public void RefreshCardHandTabs()
	{
		handTabs.RefreshTabs();
	}

	private void Update()
	{
		LevelEditorController s_Instance = LevelEditorController.s_Instance;
		if ((object)s_Instance != null && s_Instance.IsEditing)
		{
			return;
		}
		if (isFullDeckPreviewAllowed && InputManager.GetIsPressed(KeyAction.DISPLAY_CARDS) && !previewDecksWindow.IsOpen && !previewDecksWindow.IsOpenByKey)
		{
			restoreVisibility = isShown;
			ShowAllDecks(openByKey: true);
		}
		if (InputManager.GamePadInUse)
		{
			_previousTabHotkey.UpdateHotkeyIcon();
			_nextTabHotkey.UpdateHotkeyIcon();
		}
		else if (InputManager.GetWasReleased(KeyAction.DISPLAY_CARDS))
		{
			previewDecksWindow.Hide();
			if (restoreVisibility)
			{
				ShowHands();
			}
		}
	}

	private void ShowAllDecks(bool openByKey = false)
	{
		CloseViewAllCards();
		window.Hide();
		previewDecksWindow.Show(IsAllDecksInteractable, delegate
		{
			if (isShown)
			{
				window.Show();
			}
		}, openByKey);
		Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.Deck);
	}

	public bool IsShowingPlayerHand(CPlayerActor playerActor)
	{
		if (window.IsOpen && currentHand != null && currentHand.IsShown)
		{
			return currentHand.PlayerActor == playerActor;
		}
		return false;
	}

	public void AddPlayer(CPlayerActor playerData)
	{
		bool flag = false;
		switch (SaveData.Instance.Global.GameMode)
		{
		case EGameMode.Campaign:
			flag = SaveData.Instance.Global.CampaignData.AdventureMapState.InProgressQuestState?.IsSoloScenario ?? false;
			break;
		case EGameMode.Guildmaster:
			flag = SaveData.Instance.Global.AdventureData.AdventureMapState.InProgressQuestState?.IsSoloScenario ?? false;
			break;
		}
		if (flag && playerData.IsDead)
		{
			return;
		}
		CardsHandUI cardsHand = UnityEngine.Object.Instantiate(cardsHandPrefab, holderTransform).GetComponent<CardsHandUI>();
		cardsHand.gameObject.name = cardsHandPrefab.name + playerData.Class.DefaultModel;
		cardsHand.Init(playerData, highlightTransform, cardsActionController);
		cardsHand.OnHide.AddListener(delegate
		{
			if (CurrentHand == cardsHand)
			{
				HideHands();
			}
		});
		cardsHand.OnShow.AddListener(delegate
		{
			window.Show();
			UIManager.Instance.BattleGoalContainer.Show(playerData);
		});
		cardHandsUI.Add(cardsHand);
		List<CPlayerActor> list = new List<CPlayerActor>();
		foreach (CardsHandUI item in cardHandsUI)
		{
			list.Add(item.PlayerActor);
		}
		handTabs.Init(list);
		cardsHand.ToggleFullCardsPreview(active: false);
		previewDecksWindow.CreatePreview(cardsHand);
	}

	public IEnumerator AddPlayerCoroutine(CPlayerActor playerData)
	{
		bool flag = false;
		switch (SaveData.Instance.Global.GameMode)
		{
		case EGameMode.Campaign:
			flag = SaveData.Instance.Global.CampaignData.AdventureMapState.InProgressQuestState?.IsSoloScenario ?? false;
			break;
		case EGameMode.Guildmaster:
			flag = SaveData.Instance.Global.AdventureData.AdventureMapState.InProgressQuestState?.IsSoloScenario ?? false;
			break;
		}
		if (flag && playerData.IsDead)
		{
			yield break;
		}
		CardsHandUI cardsHand = UnityEngine.Object.Instantiate(cardsHandPrefab, holderTransform).GetComponent<CardsHandUI>();
		cardsHand.gameObject.name = cardsHandPrefab.name + playerData.Class.DefaultModel;
		cardsHand.Init(playerData, highlightTransform, cardsActionController);
		yield return null;
		cardsHand.OnHide.AddListener(delegate
		{
			if (CurrentHand == cardsHand)
			{
				HideHands();
			}
		});
		cardsHand.OnShow.AddListener(delegate
		{
			window.Show();
			UIManager.Instance.BattleGoalContainer.Show(playerData);
		});
		cardHandsUI.Add(cardsHand);
		List<CPlayerActor> list = new List<CPlayerActor>();
		foreach (CardsHandUI item in cardHandsUI)
		{
			list.Add(item.PlayerActor);
		}
		handTabs.Init(list);
		cardsHand.ToggleFullCardsPreview(active: false);
		yield return previewDecksWindow.CreatePreviewCoroutine(cardsHand);
	}

	public void UpdateElements(CPlayerActor playerActor)
	{
		CardsHandUI cardsHandUI = cardHandsUI.SingleOrDefault((CardsHandUI s) => s.PlayerActor == playerActor);
		if (cardsHandUI != null)
		{
			cardsHandUI.UpdateElements();
		}
	}

	[UsedImplicitly]
	public void Reset()
	{
		foreach (CardsHandUI item in cardHandsUI)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		cardHandsUI.Clear();
	}

	public CardsHandUI GetHand(int actorID)
	{
		foreach (CardsHandUI item in cardHandsUI)
		{
			if (item.PlayerActor.ID == actorID)
			{
				return item;
			}
		}
		Debug.LogWarning("Requested card hand could not be found. Actor ID: " + actorID);
		return null;
	}

	public CardsHandUI GetHand(CPlayerActor cPlayer)
	{
		foreach (CardsHandUI item in cardHandsUI)
		{
			if (item.PlayerActor == cPlayer)
			{
				return item;
			}
		}
		return null;
	}

	public CardsHandUI GetActiveHand()
	{
		foreach (CardsHandUI item in cardHandsUI)
		{
			if (item.PlayerActor == activePlayer)
			{
				return item;
			}
		}
		return null;
	}

	public void UpdateCharText()
	{
		characterStats.UpdateCharText();
		previewDecksWindow.UpdateCharText();
	}

	public void SwitchHand(CPlayerActor cPlayer)
	{
		StopPreview();
		activePlayer = cPlayer;
		foreach (CardsHandUI item in cardHandsUI.OrderBy((CardsHandUI it) => (it.PlayerActor != cPlayer) ? 1 : 0))
		{
			if (item.PlayerActor == cPlayer)
			{
				Debug.LogFormat("Switch hand {0} from {1}", cPlayer.CharacterClass.DefaultModel, currentHand?.PlayerActor?.CharacterClass?.DefaultModel);
				if (currentHand != null && controllerInputArea != null && controllerInputArea.IsFocused)
				{
					currentHand.ControllerUnfocus();
				}
				currentHand = item;
				item.Show();
				if (controllerInputArea != null && controllerInputArea.IsFocused)
				{
					item.ControllerFocus();
				}
			}
			else
			{
				item.Hide();
			}
		}
		handTabs.SelectTab(cPlayer);
		characterStats.Init(cPlayer);
		Choreographer.s_Choreographer.OnSwitchHand(cPlayer);
		longRestConfirmationButton.Hide();
		ShowHands();
	}

	private void ShowHands()
	{
		Singleton<FullCardHandViewer>.Instance.SetCardsInteractable(interactable: true);
		previewDecksWindow.ClearLastDeckState();
		previewDecksWindow.Cancel();
		window.Show();
		isShown = true;
		if (activePlayer != null)
		{
			UIManager.Instance.BattleGoalContainer.Show(activePlayer);
		}
		if (controllerInputArea != null && controllerInputArea.IsFocused)
		{
			currentHand?.ControllerFocus();
		}
	}

	public void Undo(CPlayerActor cPlayer)
	{
		GetHand(cPlayer).Undo();
		cardsActionController.OnActionUndo();
		controllerInputArea.Focus();
	}

	public void Reset(CPlayerActor cPlayer)
	{
		cardsActionController.Reset();
	}

	public void CacheCardsActionControllerPhase()
	{
		cardsActionController.CachePhase();
	}

	public void RestoreCardsActionControllerPhase()
	{
		cardsActionController.RestorePhase();
	}

	public void RefreshCardsActionControllerPhase()
	{
		cardsActionController.RefreshPhase();
	}

	public void ToggleViewAllCards(CPlayerActor player, bool openByKey = false)
	{
		if (isFullCardPreviewAlllowed)
		{
			StopPreview();
		}
		Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.AllCards);
		foreach (CardsHandUI item in cardHandsUI)
		{
			item.ToggleFullCardsPreview(item.PlayerActor == player && isFullCardPreviewAlllowed, openByKey);
		}
	}

	private void CloseViewAllCards()
	{
		foreach (CardsHandUI item in cardHandsUI)
		{
			item.ToggleFullCardsPreview(active: false);
		}
	}

	public void ShowLongRestConfirmation(CPlayerActor playerActor, Action<bool> onToggledConfirmLongRest)
	{
		Show(playerActor, CardHandMode.ActionSelection, CardPileType.None, CardPileType.None);
		longRestConfirmationButton.ShowConfirm(playerActor, onToggledConfirmLongRest);
		handTabs.UpdateTabsInteraction(interactable: false);
		Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.LongRest);
	}

	public void ShowTabs(bool show)
	{
		bool flag = !FFSNetwork.IsOnline || (PlayerRegistry.MyPlayer != null && PlayerRegistry.MyPlayer.IsParticipant);
		handTabs.gameObject.SetActive(show && PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest && flag);
		if (handTabs.gameObject.activeSelf)
		{
			handTabs.UpdateTabsInteraction();
		}
	}

	public void ShowLongRested(CPlayerActor playerActor)
	{
		Show(playerActor, CardHandMode.Preview, CardPileType.None, CardPileType.None);
		longRestConfirmationButton.ShowUsed(playerActor);
	}

	public void Show(CPlayerActor playerActor, CardHandMode mode, CardPileType filterType = CardPileType.Any, CardPileType selectableCardType = CardPileType.Any, int maxCardsSelected = 0, bool fadeUnselectableCards = false, bool highlightSelectableCards = false, bool allowFullCardPreview = true, CardsHandUI.CardActionsCommand resetCardActions = CardsHandUI.CardActionsCommand.RESET, bool forceUseCurrentRoundCards = false, bool allowFullDeckPreview = true, Action<AbilityCardUI> overrideCardCallback = null, Func<CAbilityCard, bool> cardFilter = null)
	{
		Show(playerActor, mode, filterType, new List<CardPileType> { selectableCardType }, maxCardsSelected, fadeUnselectableCards, highlightSelectableCards, allowFullCardPreview, resetCardActions, forceUseCurrentRoundCards, allowFullDeckPreview, overrideCardCallback, cardFilter);
	}

	public void Show(CPlayerActor playerActor, CardHandMode mode, CardPileType filterType, List<CardPileType> selectableCardTypes, int maxCardsSelected = 0, bool fadeUnselectableCards = false, bool highlightSelectableCards = false, bool allowFullCardPreview = true, CardsHandUI.CardActionsCommand resetCardActionsPhase = CardsHandUI.CardActionsCommand.RESET, bool forceUseCurrentRoundCards = false, bool allowFullDeckPreview = true, Action<AbilityCardUI> overrideCardCallback = null, Func<CAbilityCard, bool> cardFilter = null)
	{
		m_PushPopCardHandManagerPlayer = playerActor;
		m_PushPopCardHandMode = mode;
		m_PushPopCardPileType = filterType;
		this.selectableCardTypes = selectableCardTypes;
		this.maxCardsSelected = maxCardsSelected;
		this.fadeUnselectableCards = fadeUnselectableCards;
		this.highlightSelectableCards = highlightSelectableCards;
		this.resetCardActionsPhase = resetCardActionsPhase;
		IsFullCardPreviewAllowed = allowFullCardPreview;
		IsFullDeckPreviewAllowed = allowFullDeckPreview;
		ShowSelectionElements(show: true);
		previewDecksWindow.SetCardsSelectable(mode == CardHandMode.CardsSelection);
		Singleton<FullCardHandViewer>.Instance.SetCardsInteractable(interactable: true);
		longRestConfirmationButton.Hide();
		SwitchHand(playerActor);
		ShowPreview(playerActor, mode);
		foreach (CardsHandUI item in cardHandsUI)
		{
			if (playerActor == item.PlayerActor)
			{
				item.UpdateView(mode, filterType, selectableCardTypes, maxCardsSelected, fadeUnselectableCards, highlightSelectableCards, resetCardActionsPhase, forceUseCurrentRoundCards, overrideCardCallback, cardFilter);
				return;
			}
		}
		ShowHands();
	}

	private void ShowPreview(CPlayerActor actor, CardHandMode mode)
	{
		characterPreview.sprite = null;
		switch (mode)
		{
		case CardHandMode.CardsSelection:
		case CardHandMode.LoseCard:
		case CardHandMode.DiscardCard:
			characterPreview.sprite = UIInfoTools.Instance.GetNewAdventureCharacterPortrait(activePlayer.CharacterClass.CharacterModel);
			characterPreview.gameObject.SetActive(value: true);
			break;
		default:
			characterPreview.gameObject.SetActive(value: false);
			break;
		}
	}

	public void HideCharacterPreview()
	{
		characterPreview.sprite = null;
		characterPreview.gameObject.SetActive(value: false);
	}

	public void UpdateOriginalExtraTurnCards(CPlayerActor playerActor)
	{
		foreach (CardsHandUI item in cardHandsUI)
		{
			if (playerActor == item.PlayerActor)
			{
				item.UpdateOriginalExtraTurnCards(playerActor);
			}
		}
	}

	private void ShowSelectionElements(bool show)
	{
		bool flag = !FFSNetwork.IsOnline || (PlayerRegistry.MyPlayer != null && PlayerRegistry.MyPlayer.IsParticipant);
		handTabs.gameObject.SetActive(show && PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest && flag);
		if (handTabs.gameObject.activeSelf)
		{
			handTabs.UpdateTabsInteraction();
		}
		characterStats.Show(show && flag && (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest || (m_PushPopCardHandMode == CardHandMode.CardsSelection && m_PushPopCardHandManagerPlayer != null && m_PushPopCardHandManagerPlayer.SelectingCardsForExtraTurnOfType != CAbilityExtraTurn.EExtraTurnType.None) || m_PushPopCardHandMode == CardHandMode.LoseCard || m_PushPopCardHandMode == CardHandMode.DiscardCard || m_PushPopCardHandMode == CardHandMode.RecoverDiscardedCard || m_PushPopCardHandMode == CardHandMode.RecoverDiscardedCard));
	}

	public void Show(CardHandMode mode, CardPileType filterType, List<CardPileType> selectableCardTypes, int maxCardsSelected = 0, bool fadeUnselectableCards = false, bool allowFullCardPreview = true, bool allowFullDeckPreview = true)
	{
		m_PushPopCardHandMode = mode;
		m_PushPopCardPileType = filterType;
		IsFullCardPreviewAllowed = allowFullCardPreview;
		IsFullDeckPreviewAllowed = allowFullDeckPreview;
		this.selectableCardTypes = selectableCardTypes;
		this.maxCardsSelected = maxCardsSelected;
		this.fadeUnselectableCards = fadeUnselectableCards;
		longRestConfirmationButton.Hide();
		ShowSelectionElements(show: true);
		if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			handTabs.Reset();
		}
		Debug.Log("ShowHandManager");
		foreach (CardsHandUI item in cardHandsUI)
		{
			item.UpdateView(mode, filterType, selectableCardTypes, maxCardsSelected, fadeUnselectableCards);
			item.SetInteractable(!item.PlayerActor.IsDead, !item.PlayerActor.IsDead);
		}
		previewDecksWindow.SetCardsSelectable(mode == CardHandMode.CardsSelection);
		ShowHands();
	}

	public IEnumerator ShowCoroutine(CardHandMode mode, CardPileType filterType, List<CardPileType> selectableCardTypes, int maxCardsSelected = 0, bool fadeUnselectableCards = false, bool allowFullCardPreview = true, bool allowFullDeckPreview = true, Action onShow = null)
	{
		m_PushPopCardHandMode = mode;
		m_PushPopCardPileType = filterType;
		IsFullCardPreviewAllowed = allowFullCardPreview;
		IsFullDeckPreviewAllowed = allowFullDeckPreview;
		this.selectableCardTypes = selectableCardTypes;
		this.maxCardsSelected = maxCardsSelected;
		this.fadeUnselectableCards = fadeUnselectableCards;
		longRestConfirmationButton.Hide();
		ShowSelectionElements(show: true);
		if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			handTabs.Reset();
		}
		Debug.Log("ShowHandManager");
		foreach (CardsHandUI item in cardHandsUI)
		{
			item.UpdateView(mode, filterType, selectableCardTypes, maxCardsSelected, fadeUnselectableCards);
			item.SetInteractable(!item.PlayerActor.IsDead, !item.PlayerActor.IsDead);
			yield return null;
		}
		previewDecksWindow.SetCardsSelectable(mode == CardHandMode.CardsSelection);
		ShowHands();
		yield return null;
		onShow?.Invoke();
	}

	public void SetHandInteractable(bool interactable)
	{
		previewDecksWindow.SetCardsSelectable(interactable && m_PushPopCardHandMode == CardHandMode.CardsSelection);
		Singleton<FullCardHandViewer>.Instance.SetCardsInteractable(interactable);
		cardHandsUI.ForEach(delegate(CardsHandUI x)
		{
			x.RefreshValidCards();
		});
	}

	public void DisableHands()
	{
		previewDecksWindow.SetCardsSelectable(selectable: false);
		Singleton<FullCardHandViewer>.Instance.SetCardsInteractable(interactable: false);
		cardHandsUI.ForEach(delegate(CardsHandUI x)
		{
			x.SetInteractable(interactable: false, allowHover: false);
		});
	}

	public void ResetShortRestFlags()
	{
		cardHandsUI.ForEach(delegate(CardsHandUI x)
		{
			x.ResetShortRestUIFlag();
		});
	}

	public bool IsActive()
	{
		return handTabs.gameObject.activeSelf;
	}

	public void Hide(CPlayerActor playerActor = null)
	{
		if (playerActor == null)
		{
			ShowSelectionElements(show: false);
		}
		foreach (CardsHandUI item in cardHandsUI)
		{
			if (playerActor != null && item.PlayerActor == playerActor)
			{
				item.Hide();
			}
		}
		HideHands();
	}

	private void HideHands()
	{
		isShown = false;
		Singleton<FullCardHandViewer>.Instance.SetCardsInteractable(interactable: false);
		previewDecksWindow.Hide();
		window.HideOrUpdateStartingState();
		UIManager.Instance.BattleGoalContainer.Hide(currentHand?.PlayerActor);
		controllerInputArea?.Unfocus();
	}

	public void Preview(CPlayerActor playerActor, Transform cardsHighlightHolder)
	{
		foreach (CardsHandUI item in cardHandsUI)
		{
			if (playerActor == item.PlayerActor)
			{
				if (m_PushPopCardHandMode != CardHandMode.IncreaseCardLimit)
				{
					isPreviewing = true;
					previewPlayer = playerActor;
					item.PreviewActionCards(cardsHighlightHolder);
				}
				break;
			}
		}
	}

	public void StopPreview()
	{
		if (!isPreviewing)
		{
			return;
		}
		isPreviewing = false;
		foreach (CardsHandUI item in cardHandsUI)
		{
			if (previewPlayer == item.PlayerActor)
			{
				if (m_PushPopCardHandMode != CardHandMode.IncreaseCardLimit)
				{
					item.HidePreviewActionCards(m_PushPopCardPileType, m_PushPopCardHandMode);
				}
				break;
			}
		}
	}

	public List<AbilityCardUI> GetSelectedCards(int actorID)
	{
		return GetHand(actorID)?.SelectedCards;
	}

	public void OnSelectedCardsNumberChanged(CPlayerActor playerActor, int cardsNumber)
	{
		if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest && m_PushPopCardHandMode != CardHandMode.LoseCard)
		{
			handTabs.UpdateSelectedCardsNum(playerActor, cardsNumber);
		}
	}

	public void UpdateDeadPlayer(CActor player)
	{
		bool flag = false;
		handTabs.UpdateTabsInteraction();
		foreach (CardsHandUI item in cardHandsUI)
		{
			if (player == item.PlayerActor)
			{
				flag = item.gameObject.activeSelf;
				item.SetInteractable(interactable: false, allowHover: false);
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		flag = isShown;
		foreach (CardsHandUI item2 in cardHandsUI)
		{
			if (!item2.PlayerActor.IsDead)
			{
				SwitchHand(item2.PlayerActor);
				break;
			}
		}
		if (!flag && isShown)
		{
			HideHands();
		}
	}

	public void DiscardActiveAbilityCard(CPlayerActor playerActor, CAbilityCard card, UnityAction onSuccess = null, UnityAction onCancel = null)
	{
		if (EnableCancelActiveAbilities)
		{
			LocalizationManager.TryGetTranslation("GUI_CONFIRMATION_CANCEL_" + card.Name + "_DESCR", out var Translation);
			Singleton<UIConfirmationBoxManager>.Instance.ShowCancelActiveAbility(LocalizationManager.GetTranslation(card.Name), card.SelectedAction.CardPile != CBaseCard.ECardPile.Discarded, delegate
			{
				CancelActiveAbility(playerActor, card);
				Choreographer.s_Choreographer.ForceEnterToCardSelection = true;
				NetworkCancelingActiveBonuses(playerActor, card);
				onSuccess?.Invoke();
			}, Translation, onCancel);
		}
	}

	private void CancelActiveAbility(CPlayerActor playerActor, CAbilityCard card)
	{
		List<CPlayerActor> list = new List<CPlayerActor>();
		bool flag = false;
		foreach (CActiveBonus item in card.ActiveBonuses.ToList())
		{
			if (item.IsAura)
			{
				flag = true;
			}
			if (item.Actor.Type == CActor.EType.Player)
			{
				list.Add(item.Actor as CPlayerActor);
			}
			if (item.Caster.Type == CActor.EType.Player)
			{
				list.Add(item.Caster as CPlayerActor);
			}
			else if (item.Actor.Type == CActor.EType.HeroSummon)
			{
				list.Add((item.Actor as CHeroSummonActor).Summoner);
			}
			CClass.CancelActiveBonus(item);
		}
		SimpleLog.AddToSimpleLog(LocalizationManager.GetTranslation(playerActor.ActorLocKey()) + " removed active ability " + LocalizationManager.GetTranslation(card.Name));
		if (flag)
		{
			ScenarioManager.Scenario.PlayerActors.ForEach(delegate(CPlayerActor f)
			{
				RefreshCancelledActiveCardUI(f, card);
			});
		}
		else
		{
			list.ForEach(delegate(CPlayerActor f)
			{
				RefreshCancelledActiveCardUI(f, card);
			});
		}
	}

	private void RefreshCancelledActiveCardUI(CPlayerActor playerActor, CAbilityCard card)
	{
		InitiativeTrackActorBehaviour initiativeTrackActorBehaviour = InitiativeTrack.Instance.FindInitiativeTrackActor(playerActor);
		if (!(initiativeTrackActorBehaviour != null))
		{
			return;
		}
		initiativeTrackActorBehaviour.Avatar.RefreshAbilities();
		foreach (CardsHandUI item in cardHandsUI)
		{
			if (playerActor == item.PlayerActor)
			{
				bool animateDisabling = IsShowingPlayerHand(playerActor) && currentHand == item && m_PushPopCardHandMode == CardHandMode.CardsSelection;
				item.RefreshCancelActiveAbility(m_PushPopCardPileType, card, animateDisabling);
				break;
			}
		}
	}

	public void RefreshPendingActiveBonus(CPlayerActor playerActor)
	{
		AbilityCardUI abilityCardUI = GetHand(playerActor).GetActiveAbilityCards().FirstOrDefault((AbilityCardUI it) => !playerActor.CharacterClass.ActivatedAbilityCards.Contains(it.AbilityCard));
		if (abilityCardUI != null)
		{
			RefreshCancelledActiveCardUI(playerActor, abilityCardUI.AbilityCard);
		}
	}

	public void CancelImprovedShortRest(CPlayerActor playerActor)
	{
		playerActor.CharacterClass.ImprovedShortRest = false;
		InitiativeTrack.Instance.FindInitiativeTrackActor(playerActor).RefreshAbilities();
	}

	public void OnControllerAreaFocused()
	{
		if (longRestConfirmationButton.IsVisible)
		{
			CurrentHand.ControllerFocus(longRestConfirmationButton.Selectable.IsInteractable() ? longRestConfirmationButton.Selectable : null);
			if (!InputManager.GamePadInUse)
			{
				longRestConfirmationButton.Selectable.SetNavigation(null, CurrentHand.modifiersSelectable);
			}
		}
		else
		{
			CurrentHand.ControllerFocus();
		}
	}

	public void OnControllerAreaUnfocused()
	{
		if (controllerInputArea.IsFocused)
		{
			controllerInputArea.Unfocus();
		}
		CurrentHand.ControllerUnfocus();
		longRestConfirmationButton.DisableNavigation();
	}

	public void SwitchToNextMercenary()
	{
		if (m_PushPopCardHandMode == CardHandMode.CardsSelection || m_PushPopCardHandMode == CardHandMode.ActionSelection)
		{
			handTabs.SelectNextTab();
		}
	}

	public void SwitchToPreviousMercenary()
	{
		if (m_PushPopCardHandMode == CardHandMode.CardsSelection || m_PushPopCardHandMode == CardHandMode.ActionSelection)
		{
			handTabs.SelectPreviousTab();
		}
	}

	public void GoToNextHand()
	{
		if ((m_PushPopCardHandMode == CardHandMode.CardsSelection || m_PushPopCardHandMode == CardHandMode.ActionSelection) && handTabs.gameObject.activeSelf)
		{
			if (!controllerInputArea.IsFocused)
			{
				controllerInputArea.Focus();
			}
			handTabs.SelectNextTab();
			CurrentHand.ControllerFocus();
		}
	}

	public void GoToPreviousHand()
	{
		if ((m_PushPopCardHandMode == CardHandMode.CardsSelection || m_PushPopCardHandMode == CardHandMode.ActionSelection) && handTabs.gameObject.activeSelf)
		{
			if (!controllerInputArea.IsFocused)
			{
				controllerInputArea.Focus();
			}
			handTabs.SelectPreviousTab();
			CurrentHand.ControllerFocus();
		}
	}

	private void OnShowAllCardsComboPressed(Gamepad gamepad)
	{
		if (TransitionManager.s_Instance.TransitionDone && !ScenarioRuleClient.IsProcessingOrMessagesQueued && PhaseManager.Phase.Type != CPhase.PhaseType.EndTurn)
		{
			ToggleViewAllCards(InitiativeTrack.Instance.SelectedActor().Actor as CPlayerActor);
		}
	}

	private void OnShowAllDeckComboPressed(Gamepad gamepad)
	{
		if (isFullDeckPreviewAllowed)
		{
			ShowAllDecks();
		}
	}

	private void SubscribeToMyPlayerInitialized()
	{
		FFSNet.Console.LogInfo("Card Manager: Subscribing to MyPlayerInitialized.");
		PlayerRegistry.MyPlayerInitialized.AddListener(SubscribeToControllableChanges);
	}

	private void UnsubscribeFromMyPlayerInitialized()
	{
		FFSNet.Console.LogInfo("Card Manager: Unsubscribing from MyPlayerInitialized.");
		PlayerRegistry.MyPlayerInitialized.RemoveListener(SubscribeToControllableChanges);
	}

	private void SubscribeToControllableChanges()
	{
		FFSNet.Console.LogInfo("Card Manager: Subscribing to controllable changes.");
		PlayerRegistry.MyPlayer.MyControllables.CollectionChanged += OnMyControllableOwnershipChanged;
		FFSNetwork.Manager.HostingEndedEvent.AddListener(SubscribeToMyPlayerInitialized);
	}

	private void UnsubscribeFromControllableChanges()
	{
		FFSNet.Console.LogInfo("Card Manager: Unsubscribing from controllable changes.");
		PlayerRegistry.MyPlayer.MyControllables.CollectionChanged -= OnMyControllableOwnershipChanged;
		FFSNetwork.Manager.HostingEndedEvent.RemoveListener(SubscribeToMyPlayerInitialized);
	}

	private void OnMyControllableOwnershipChanged(object sender, NotifyCollectionChangedEventArgs args)
	{
		if (FFSNetwork.IsStartingUp || FFSNetwork.IsShuttingDown)
		{
			return;
		}
		RefreshConnectionState();
		if (Singleton<UIResultsManager>.Instance != null && Singleton<UIResultsManager>.Instance.IsShown)
		{
			return;
		}
		if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			CardsHandUI cardsHandUI = currentHand;
			FFSNet.Console.LogInfo("OnControllableOwnershipChanged");
			ShowSelectionElements(PlayerRegistry.MyPlayer.IsParticipant);
			InitiativeTrack.Instance.SelectedActor()?.Deselect();
			Choreographer.s_Choreographer.ClearHilightedActors();
			if (PlayerRegistry.MyPlayer.MyControllables.Count == 0)
			{
				WorldspaceStarHexDisplay.Instance.ClearStars();
			}
			if (cardsHandUI.PlayerActor.IsUnderMyControl)
			{
				SwitchHand(cardsHandUI.PlayerActor);
			}
			else
			{
				foreach (CardsHandUI item in cardHandsUI)
				{
					if (!item.PlayerActor.IsDead && InitiativeTrack.Instance.Select(item.PlayerActor))
					{
						SwitchHand(item.PlayerActor);
						break;
					}
				}
			}
			foreach (CardsHandUI item2 in cardHandsUI)
			{
				FFSNet.Console.LogInfo("Updating hand view for " + item2.PlayerActor.CharacterClass.ID);
				item2.ResetViewToCachedSettings();
				item2.SetInteractable(!item2.PlayerActor.IsDead, !item2.PlayerActor.IsDead);
			}
			ControllableRegistry.AllControllables.ForEach(delegate(NetworkControllable x)
			{
				x.NetworkEntity.GetComponent<GHNetworkControllable>().OnStartRoundCardsChanged();
			});
			return;
		}
		Debug.Log("OnMyControllableOwnershipChanged.UpdateView");
		foreach (CardsHandUI item3 in cardHandsUI)
		{
			if (m_PushPopCardHandManagerPlayer == item3.PlayerActor)
			{
				RefreshItemsBar();
				item3.UpdateView(m_PushPopCardHandMode, m_PushPopCardPileType, selectableCardTypes, maxCardsSelected, fadeUnselectableCards, highlightSelectableCards, resetCardActionsPhase);
				break;
			}
		}
	}

	private void RefreshItemsBar()
	{
		if (PhaseManager.PhaseType != CPhase.PhaseType.ActionSelection)
		{
			return;
		}
		if (m_PushPopCardHandManagerPlayer.Class is CMonsterClass || m_PushPopCardHandManagerPlayer.Class is CHeroSummonClass || (m_PushPopCardHandManagerPlayer.Class is CCharacterClass && m_PushPopCardHandManagerPlayer.Type != CActor.EType.Player))
		{
			return;
		}
		CPlayerActor playerActor = m_PushPopCardHandManagerPlayer;
		if (playerActor.CharacterClass.LongRest)
		{
			Singleton<UIUseItemsBar>.Instance.ShowUsableItems(playerActor, null, null, "GUI_USE_ITEM_TITLE", hideIfEmpty: true);
		}
		else if (!playerActor.CharacterClass.HasLongRested || playerActor.IsTakingExtraTurn)
		{
			if (GameState.CurrentActionSelectionSequence == GameState.ActionSelectionSequenceType.FirstAction)
			{
				Singleton<UIUseItemsBar>.Instance.ShowUsableItems(playerActor, (CItem item) => !playerActor.IsTakingExtraTurn || item.SlotState != CItem.EItemSlotState.Locked || !Singleton<AbilityEffectManager>.Instance.IsItemAffectingActor(playerActor, item), null, "GUI_USE_ITEM_TITLE", hideIfEmpty: true);
			}
			else if (GameState.CurrentActionSelectionSequence == GameState.ActionSelectionSequenceType.SecondAction)
			{
				Singleton<UIUseItemsBar>.Instance.ShowUsableItems(playerActor, (CItem item) => !playerActor.IsTakingExtraTurn || item.SlotState != CItem.EItemSlotState.Locked || !Singleton<AbilityEffectManager>.Instance.IsItemAffectingActor(playerActor, item), null, "GUI_USE_ITEM_TITLE", hideIfEmpty: true);
			}
			else
			{
				Singleton<UIUseItemsBar>.Instance.ShowUsableItems(playerActor, (CItem item) => item.YMLData.Data.CompareAbility == null || item.YMLData.Data.CompareAbility.CompareActor(playerActor), null, "GUI_USE_ITEM_TITLE", hideIfEmpty: true);
			}
		}
		else
		{
			Singleton<UIUseItemsBar>.Instance.ShowUsableItems(playerActor, (CItem item) => item.YMLData.Data.CompareAbility == null || item.YMLData.Data.CompareAbility.CompareActor(playerActor), null, "GUI_USE_ITEM_TITLE", hideIfEmpty: true);
			Singleton<UIUseItemsBar>.Instance.ShowUsableItems(playerActor, null, null, "GUI_USE_ITEM_TITLE", hideIfEmpty: true);
		}
	}

	public void ProxySelectRoundCards(GameAction action)
	{
		StartRoundCardsToken startRoundCardsToken = (StartRoundCardsToken)action.SupplementaryDataToken;
		ProxySelectRoundCards(action.ActorID, startRoundCardsToken);
	}

	public void ProxySelectRoundCards(int controllableID, StartRoundCardsToken startRoundCardsToken, bool isHandUnderMyControl = true)
	{
		if (startRoundCardsToken.LongRestSelected)
		{
			ProxySelectRoundCards(controllableID, new int[1], isHandUnderMyControl);
		}
		else
		{
			ProxySelectRoundCards(controllableID, startRoundCardsToken.RoundCardIDs, isHandUnderMyControl);
		}
	}

	private void ProxySelectRoundCards(int controllableID, int[] selectedCardIDs, bool isHandUnderMyControl = true)
	{
		CActor cActor = ((CharacterManager)(ControllableRegistry.GetControllable(controllableID)?.ControllableObject))?.CharacterActor;
		if (cActor != null)
		{
			CardsHandUI hand = GetHand(cActor.ID);
			if (hand != null)
			{
				CPlayerActor cPlayerActor = (CPlayerActor)cActor;
				FFSNet.Console.Log("[PROXY SELECT ROUND CARDS]: About to select round cards for " + cPlayerActor.CharacterClass.ID);
				hand.DeselectAllCards(networkAction: false, isHandUnderMyControl);
				FFSNet.Console.Log("[PROXY SELECT ROUND CARDS]: Deselected all previously selected cards.");
				foreach (int cardInstanceID in selectedCardIDs)
				{
					hand.ProxySelectCard(cardInstanceID, isHandUnderMyControl);
				}
				return;
			}
			throw new Exception("Error selecting round cards for proxy playerActor. Card Hand returns null for the actor (ActorID: " + cActor.ID + ").");
		}
		throw new Exception("Error selecting round cards for proxy playerActor. Cannot find any corresponding actor (ControllableID: " + controllableID + ").");
	}

	public void ProxySelectExtraTurnCards(GameAction action)
	{
		CardsHandUI hand = GetHand(action.ActorID);
		if (hand != null)
		{
			hand.DeselectAllCards(networkAction: false);
			CardsToken cardsToken = (CardsToken)action.SupplementaryDataToken;
			for (int num = cardsToken.CardIDs.Length - 1; num >= 0; num--)
			{
				hand.ProxySelectCard(cardsToken.CardIDs[num]);
			}
			return;
		}
		throw new Exception("Error selecting extra turn cards proxy playerActor. Card Hand returns null for the actor (ActorID: " + action.ActorID + ").");
	}

	public void ProxySelectCardAbility(GameAction action)
	{
		CardsHandUI hand = GetHand(action.ActorID);
		if (hand != null)
		{
			ActionProcessor.SetState(ActionProcessorStateType.Halted);
			hand.ProxySelectCardAction(action.SupplementaryDataIDMax, (CBaseCard.ActionType)action.SupplementaryDataIDMin);
			return;
		}
		throw new Exception("Error selecting card action for proxy playerActor. Card Hand returns null for the actor (ActorID: " + action.ActorID + ").");
	}

	public void ProxyToggleAugment(GameAction action)
	{
		if (Choreographer.s_Choreographer == null)
		{
			FFSNet.Console.LogError("ERROR_MULTIPLAYER_00002", "Cannot toggle the enhancement. Choreographer not yet initialized.");
			return;
		}
		if (Choreographer.s_Choreographer.m_CurrentActor == null)
		{
			FFSNet.Console.LogError("ERROR_MULTIPLAYER_00003", "Cannot toggle the enhancement. Current actor not yet initialized.");
			return;
		}
		CActor currentActor = Choreographer.s_Choreographer.CurrentActor;
		if (currentActor != null)
		{
			CPlayerActor cPlayerActor = null;
			if (GameState.OverridenActionActorStack.Count > 0 && GameState.OverridenActionActorStack.Peek().ControllingActor is CPlayerActor cPlayerActor2)
			{
				cPlayerActor = cPlayerActor2;
			}
			else if (currentActor is CPlayerActor cPlayerActor3)
			{
				cPlayerActor = cPlayerActor3;
			}
			if (cPlayerActor != null)
			{
				AbilityAugmentToken abilityAugmentToken = (AbilityAugmentToken)action.SupplementaryDataToken;
				AbilityCardUI abilityCardUI = GetHand(cPlayerActor.ID)?.GetCard(abilityAugmentToken.CardID);
				if (abilityCardUI != null)
				{
					abilityCardUI.fullAbilityCard.ProxyToggleAugment(action.ActionTypeID == 34, abilityAugmentToken);
					return;
				}
				throw new Exception("Error trying to toggle an ability augment for proxy character. Card returns null (CardID: " + abilityAugmentToken.CardID + ").");
			}
			throw new Exception("Error trying to toggle an ability augment for proxy character. CPlayerActor returns null (ActorID: " + action.ActorID + ").");
		}
		throw new Exception("Error trying to toggle an ability augment for proxy character. CPlayerActor returns null (ActorID: " + action.ActorID + ").");
	}

	public void ProxyShortRest(GameAction action)
	{
		ProxyShortRest(action.ActorID, (StartRoundCardsToken)action.SupplementaryDataToken);
	}

	private void ProxyShortRest(int controllableID, StartRoundCardsToken startRoundCardToken)
	{
		CActor cActor = ((CharacterManager)(ControllableRegistry.GetControllable(controllableID)?.ControllableObject))?.CharacterActor;
		if (cActor != null)
		{
			CardsHandUI hand = GetHand(cActor.ID);
			if (hand != null)
			{
				hand.ProxyShortRest(startRoundCardToken);
				return;
			}
			throw new Exception("Error using short rest for proxy playerActor. Card Hand returns null for the actor (ActorID: " + cActor.ID + ").");
		}
		throw new Exception("Error using short rest for proxy playerActor. Cannot find any corresponding actor (ControllableID: " + controllableID + ").");
	}

	public void ProxyLongRest(GameAction action)
	{
		CardsHandUI hand = GetHand(action.ActorID);
		if (hand != null)
		{
			hand.ProxyLongRest(action.SupplementaryDataIDMax);
			return;
		}
		throw new Exception("Error using long rest for proxy playerActor. Card Hand returns null for the actor (ActorID: " + action.ActorID + ").");
	}

	public void ProxyRecoverCards(GameAction action)
	{
		CardsHandUI hand = GetHand(action.ActorID);
		if (hand != null)
		{
			hand.ProxyRecoverCards((CardsToken)action.SupplementaryDataToken, action.SupplementaryDataBoolean);
			return;
		}
		throw new Exception("Error recovering cards for proxy playerActor. Card Hand returns null for the actor (ActorID: " + action.ActorID + ").");
	}

	public void NetworkCancelingActiveBonuses(CPlayerActor playerActor, CBaseCard baseCard)
	{
		if (!FFSNetwork.IsOnline || playerActor.IsUnderMyControl)
		{
			if (PhaseManager.CurrentPhase.Type == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
			{
				ScenarioRuleClient.GetAndReplicateStartRoundDeckState(playerActor, 57);
			}
			else if (FFSNetwork.IsOnline)
			{
				int actorID = ((SaveData.Instance.Global.GameMode == EGameMode.Campaign) ? playerActor.CharacterName.GetHashCode() : playerActor.CharacterClass.ModelInstanceID);
				ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
				IProtocolToken supplementaryDataToken = new CardsToken(baseCard);
				Synchronizer.SendGameAction(GameActionType.CancelActiveBonus, currentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, actorID, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
			}
		}
	}

	public void ProxyCancelActiveBonuses(GameAction action)
	{
		if (action.SupplementaryDataToken is StartRoundCardsToken startRoundCardsToken)
		{
			if (!(((CharacterManager)(ControllableRegistry.GetControllable(action.ActorID)?.ControllableObject))?.CharacterActor is CPlayerActor cPlayerActor))
			{
				return;
			}
			for (int num = cPlayerActor.CharacterClass.ActivatedCards.Count - 1; num >= 0; num--)
			{
				CBaseCard cBaseCard = cPlayerActor.CharacterClass.ActivatedCards[num];
				if (!startRoundCardsToken.ActivatedCardIDs.Contains(cBaseCard.ID))
				{
					ProxyCancelActiveBonus(action.ActorID, cBaseCard.ID, cBaseCard.CardType);
				}
			}
		}
		else if (action.SupplementaryDataToken is CardsToken cardsToken && ((CharacterManager)(ControllableRegistry.GetControllable(action.ActorID)?.ControllableObject))?.CharacterActor is CPlayerActor)
		{
			int[] cardIDs = cardsToken.CardIDs;
			foreach (int cardID in cardIDs)
			{
				ProxyCancelActiveBonus(action.ActorID, cardID, cardsToken.CardTypeEnum);
			}
		}
	}

	private void ProxyCancelActiveBonus(int controllableID, int cardID, CBaseCard.ECardType cardType)
	{
		CActor cActor = ((CharacterManager)(ControllableRegistry.GetControllable(controllableID)?.ControllableObject))?.CharacterActor;
		if (cActor != null)
		{
			List<CActiveBonus> list = CharacterClassManager.FindAllActiveBonuses(cActor);
			if (cActor.DoomTarget != null)
			{
				list.AddRange(from x in CharacterClassManager.FindAllActiveBonuses(cActor.DoomTarget)
					where x.IsDoom
					select x);
			}
			FFSNet.Console.Log("About to cancel an active bonus (Character: " + (cActor as CPlayerActor).CharacterClass.CharacterID + ", Base Card ID: " + cardID + " CardType: " + cardType.ToString() + "). Listing all active bonuses on the character:");
			foreach (CActiveBonus item in list)
			{
				FFSNet.Console.Log("Active Bonus. Base Card Name: " + item.BaseCard.Name + ", Base Card ID: " + item.BaseCard.ID + ", Ability Name:" + item.Ability.Name);
			}
			List<CActiveBonus> list2 = list.Where((CActiveBonus x) => x.BaseCard.ID == cardID && x.BaseCard.CardType == cardType).ToList();
			if (list2.Count > 0)
			{
				foreach (CActiveBonus item2 in list2)
				{
					switch (item2.BaseCard.CardType)
					{
					case CBaseCard.ECardType.Item:
						CClass.CancelActiveBonus(item2);
						InitiativeTrack.Instance.FindInitiativeTrackActor(item2.Actor)?.Avatar.RefreshAbilities();
						break;
					case CBaseCard.ECardType.CharacterAbility:
					{
						AbilityCardUI abilityCardUI = GetHand(item2.Caster.ID)?.GetCard(cardID);
						if (!(abilityCardUI != null))
						{
							throw new Exception("Error canceling an ability card base active bonus for proxy playerActor. No such card exists (ActorID: " + cActor.ID + ", CardID: " + cardID + ").");
						}
						CancelActiveAbility((CPlayerActor)cActor, abilityCardUI.AbilityCard);
						break;
					}
					case CBaseCard.ECardType.HeroSummonAbility:
					case CBaseCard.ECardType.MonsterAbility:
					case CBaseCard.ECardType.AttackModifier:
					case CBaseCard.ECardType.ScenarioModifier:
						throw new Exception("Error canceling an active ability bonus for proxy playerActor. Missing implementation for the card type: " + item2.BaseCard.CardType);
					default:
						throw new Exception("Error canceling an active ability bonus for proxy playerActor. Unidentified card type: " + item2.BaseCard.CardType);
					}
					FFSNet.Console.Log("Successfully canceled the active bonus (Base Card Name: " + item2.BaseCard.Name + ", Base Card ID: " + item2.BaseCard.ID + ", Ability Name: " + item2.Ability.Name + ") from " + ((CPlayerActor)cActor).CharacterClass.ID);
				}
				return;
			}
			FFSNet.Console.Log("No active bonuses were found to cancel");
			return;
		}
		throw new Exception("Error canceling an active ability bonus for proxy playerActor. Cannot find any corresponding actor (ControllableID: " + controllableID + ").");
	}

	public void ProxyCancelImprovedShortRest(GameAction action)
	{
		ProxyCancelImprovedShortRest(action.ActorID);
	}

	private void ProxyCancelImprovedShortRest(int controllableID)
	{
		CActor cActor = ((CharacterManager)(ControllableRegistry.GetControllable(controllableID)?.ControllableObject))?.CharacterActor;
		if (cActor != null)
		{
			CancelImprovedShortRest((CPlayerActor)cActor);
			return;
		}
		throw new Exception("Error canceling an active ability bonus for proxy playerActor. Cannot find any corresponding actor (ControllableID: " + controllableID + ").");
	}

	public void ProxyToggleSelectLongRestAbility(GameAction gameAction)
	{
		longRestConfirmationButton.ProxyToggleSelectLongRestAbility(gameAction);
	}

	public void ClearStartRoundCardsTokenCache()
	{
		cardHandsUI.ForEach(delegate(CardsHandUI x)
		{
			x.CachedStartRoundToken = null;
		});
	}

	public void RefreshConnectionState()
	{
		handTabs.RefreshConnectionState();
		longRestConfirmationButton.RefreshConnectionState();
		foreach (CardsHandUI item in cardHandsUI)
		{
			item.RefreshConnectionState();
		}
	}

	public void ProxyAbilityDiscardCard(GameAction action)
	{
		CardsHandUI hand = GetHand(action.ActorID);
		if (hand != null)
		{
			hand.ProxyAbilityDiscardCard((CardsToken)action.SupplementaryDataToken);
			return;
		}
		throw new Exception("Error discarding cards for proxy playerActor. Card Hand returns null for the actor (ActorID: " + action.ActorID + ").");
	}

	public void ProxyAbilityLoseCard(GameAction action)
	{
		CardsHandUI hand = GetHand(action.ActorID);
		if (hand != null)
		{
			hand.ProxyAbilityLoseCard((CardsToken)action.SupplementaryDataToken);
			return;
		}
		throw new Exception("Error losing cards for proxy playerActor. Card Hand returns null for the actor (ActorID: " + action.ActorID + ").");
	}
}
