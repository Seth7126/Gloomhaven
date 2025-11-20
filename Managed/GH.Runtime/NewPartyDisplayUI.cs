#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using Chronos;
using FFSNet;
using GLOO.Introduction;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Client;
using MapRuleLibrary.Party;
using MapRuleLibrary.State;
using Photon.Bolt;
using SM.Gamepad;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using Script.GUI;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.Tabs;
using SharedLibrary.SimpleLog;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NewPartyDisplayUI : APartyDisplayUI
{
	public enum DisplayType
	{
		NONE,
		INVENTORY,
		CARDS,
		PERKS,
		CHARACTER_SELECTOR,
		LEVELUP,
		BATTLE_GOALS
	}

	[Flags]
	private enum FlagsInteraction
	{
		None = 0,
		DISABLE_DISPLAY_ITEMS_ON_CHARACTER_SELECT = 1,
		DISABLE_BUTTONS = 2,
		DISABLE_TOGGLE_OFF_CHARACTER = 4,
		DISABLE_EDIT_PARTY = 8,
		DISABLE_RESET_LEVELUP = 0x10
	}

	public delegate void SelectionEvent(bool selected, DisplayType display);

	[Header("References")]
	[SerializeField]
	private TextMeshProUGUI partyNameText;

	[SerializeField]
	private GoldCounter partyGoldCounter;

	[SerializeField]
	private GUIAnimator warningPartyGoldAnimator;

	[SerializeField]
	private GameObject partyGoldContainer;

	[SerializeField]
	private List<NewPartyCharacterUI> charactersList;

	[SerializeField]
	private UIPartyCharacterAbilityCardsDisplay abilityCardsDisplay;

	[SerializeField]
	private ToggleGroup slotsToggleGroup;

	[SerializeField]
	private UIPerksWindow perkManager;

	[SerializeField]
	private UIAdventurePartyAssemblyWindow characterSelector;

	[SerializeField]
	private UIPartyCharacterEquipmentDisplay itemInventoryDisplay;

	[SerializeField]
	private UIBattleGoalPickerWindow battleGoalSelector;

	[SerializeField]
	private ClickTrackerExtended clickTracker;

	[SerializeField]
	private UIPartyStats campaignPartyStats;

	[SerializeField]
	private UIPartyStats guildmasterPartyStats;

	[SerializeField]
	private UIPartyCharacterEnhancementAbilityCardsDisplay enhancementCardsDisplay;

	[Header("Controller")]
	[SerializeField]
	private ControllerInputEscapableArea controllerArea;

	[SerializeField]
	private UIControllerKeyTip controllerFocusTip;

	[SerializeField]
	private Hotkey nextTabHotkey;

	[SerializeField]
	private GameObject _mercenaryLabelText;

	[Header("Open/Close animation")]
	[SerializeField]
	private GUIAnimator showPanelAnimator;

	[SerializeField]
	private GUIAnimator hidePanelAnimator;

	[SerializeField]
	private CanvasGroup animLockInteraction;

	[SerializeField]
	private float openPanelDelay;

	[Header("Navigation Tab")]
	[SerializeField]
	private UINavigationTabComponent _tab;

	[SerializeField]
	private TabComponentInputListener _tabInput;

	[SerializeField]
	private ToggleElementsController _toggleElementsController;

	[SerializeField]
	private Hotkey[] _tooltipHotkeys;

	[SerializeField]
	private GameObject _tooltipHotkeysPanel;

	private Coroutine delayedShowCoroutine;

	private NewPartyCharacterUI selectedCharacter;

	private DisplayType displayActive;

	private FlagsInteraction flagsInteraction;

	private Action<CMapCharacter> onCharacterSelectedCallback;

	private IPartyItemEquipmentService itemService;

	private CMapParty mapParty;

	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private PanelHotkeyContainer _shopInventoryPanelHotkeyContainer;

	[SerializeField]
	private ShopInventoryPanelHotkeyContainerProxy _shopInventoryPanelHotkeyContainerProxy;

	private Action hideCallback;

	private bool initialised;

	private UIPartyStats partyStats;

	private HashSet<Component> disableClickTrackerRequests = new HashSet<Component>();

	private HashSet<object> hideRequests = new HashSet<object>();

	private HashSet<Component> disableAssignPlayerRequests = new HashSet<Component>();

	private HashSet<Component> disableCharacterSelectionRequests = new HashSet<Component>();

	private HashSet<Component> disableStatsRequests = new HashSet<Component>();

	private List<BenchedCharacter> m_BenchedCharacters = new List<BenchedCharacter>();

	private bool _isSoloQuest;

	private HashSet<DisplayType> _ignoreDisplayTypes;

	private HashSet<DisplayType> _ignoreDisplayTypesForPauseMenu = new HashSet<DisplayType>
	{
		DisplayType.BATTLE_GOALS,
		DisplayType.LEVELUP,
		DisplayType.CARDS,
		DisplayType.CHARACTER_SELECTOR,
		DisplayType.INVENTORY,
		DisplayType.PERKS
	};

	public int? CurrentCharacterTabIndex => _tab.CurrentIndex;

	public ShopInventoryPanelHotkeyContainerProxy ShopInventoryPanelHotkeyContainerProxy => _shopInventoryPanelHotkeyContainerProxy;

	public bool IsDisableFurtherAbilityPanel => flagsInteraction.HasFlag(FlagsInteraction.DISABLE_RESET_LEVELUP);

	public DisplayType ActiveDisplay => displayActive;

	public bool TabInputLocked { get; private set; }

	public bool MercenariesHotkeysActiveState { get; private set; }

	public UICampaignAdventurePartyAssemblyWindow CampaignCharacterSelector
	{
		get
		{
			if (CharacterSelector != null && CharacterSelector is UICampaignAdventurePartyAssemblyWindow result)
			{
				return result;
			}
			return null;
		}
	}

	public int SelectedSlotIndex => charactersList.IndexOf(SelectedUISlot);

	public bool Initialised => initialised;

	public PartyItemEventHandler OnUpdatedNewItems { get; set; }

	public NewPartyCharacterUI SelectedUISlot => selectedCharacter;

	public int SelectedUISlotIndex => charactersList.IndexOf(selectedCharacter);

	public List<NewPartyCharacterUI> CharacterSlots => charactersList;

	public int CharacterInternalTabIndex { get; private set; }

	public int CharacterLastTabElementsCount { get; private set; }

	public bool IsVisible => window.IsOpen;

	public UIBattleGoalPickerWindow BattleGoalWindow => battleGoalSelector;

	public UnityEvent OnShown => window.onShown;

	public UnityEvent OnHidden => window.onHidden;

	public override APartyCharacterUI SelectedCharacter => selectedCharacter;

	public static NewPartyDisplayUI PartyDisplay => Singleton<APartyDisplayUI>.Instance as NewPartyDisplayUI;

	public bool IsOpen => window.IsOpen;

	public UIPartyStats PartyStats => partyStats;

	public TabComponentInputListener TabInput => _tabInput;

	public ControllerInputEscapableArea ControllerArea => controllerArea;

	public UIAdventurePartyAssemblyWindow CharacterSelector => characterSelector;

	public UIPartyCharacterAbilityCardsDisplay AbilityCardsDisplay => abilityCardsDisplay;

	public UIPartyCharacterEquipmentDisplay ItemInventoryDisplay => itemInventoryDisplay;

	public UIPartyCharacterEnhancementAbilityCardsDisplay EnhancementCardsDisplay => enhancementCardsDisplay;

	public UIPerksWindow PerkManager => perkManager;

	public event SelectionEvent OnOpenedPanel;

	public event BasicEventHandler OnAbilityDeckUpdated;

	public event Action<bool, NewPartyCharacterUI> NewCharacterSelected;

	protected override void Awake()
	{
		base.Awake();
		if ((object)window == null)
		{
			window = GetComponent<UIWindow>();
		}
		initialised = false;
		TabInputLocked = false;
		for (int i = 0; i < charactersList.Count; i++)
		{
			charactersList[i].OnClickedAssignRole.AddListener(OnMultiplayerAssignRoleSelected);
			charactersList[i].OnTabIndexUpdated += SetCharacterInternalIndex;
			NewPartyCharacterUI slot = charactersList[i];
			slot.OnHovered.AddListener(delegate
			{
				OnHoveredSlot(slot, hovered: true);
			});
			slot.OnUnhovered.AddListener(delegate
			{
				OnHoveredSlot(slot, hovered: false);
			});
		}
		showPanelAnimator.OnAnimationFinished.AddListener(delegate
		{
			animLockInteraction.blocksRaycasts = true;
		});
		hidePanelAnimator.OnAnimationFinished.AddListener(delegate
		{
			Debug.LogGUI(string.Format("Finish hide animation {0} ({1})", string.Join(",", hideRequests), hideCallback == null));
			animLockInteraction.blocksRaycasts = true;
			window.Hide();
			hideCallback?.Invoke();
		});
		if (InputManager.GamePadInUse)
		{
			SetHotkeysActive(value: true);
		}
		else
		{
			nextTabHotkey.gameObject.SetActive(value: false);
		}
		Hotkey[] tooltipHotkeys = _tooltipHotkeys;
		for (int num = 0; num < tooltipHotkeys.Length; num++)
		{
			tooltipHotkeys[num].Initialize(Singleton<UINavigation>.Instance.Input);
		}
		if (_shopInventoryPanelHotkeyContainer != null)
		{
			_shopInventoryPanelHotkeyContainer.ToggleActiveAllHotkeys(value: false);
		}
		if (_shopInventoryPanelHotkeyContainerProxy != null)
		{
			_shopInventoryPanelHotkeyContainerProxy.SetDefaultHotkeyContainer(_shopInventoryPanelHotkeyContainer);
			_shopInventoryPanelHotkeyContainerProxy.SetCurrentHotkeyContainer(_shopInventoryPanelHotkeyContainer);
		}
		CharacterInternalTabIndex = 0;
		_ignoreDisplayTypes = new HashSet<DisplayType>
		{
			DisplayType.CARDS,
			DisplayType.LEVELUP
		};
	}

	public void ResetLastTabElementsCount()
	{
		CharacterLastTabElementsCount = 0;
	}

	protected override void OnDestroy()
	{
		if (!CoreApplication.IsQuitting)
		{
			ClearEvents();
			CancelShowAnimations();
			if (InputManager.GamePadInUse)
			{
				nextTabHotkey.Deinitialize();
			}
			Hotkey[] tooltipHotkeys = _tooltipHotkeys;
			for (int i = 0; i < tooltipHotkeys.Length; i++)
			{
				tooltipHotkeys[i].Deinitialize();
			}
			base.OnDestroy();
		}
	}

	public void SetHotkeysActive(bool value)
	{
		if (!InputManager.GamePadInUse)
		{
			return;
		}
		MercenariesHotkeysActiveState = value;
		if (value)
		{
			nextTabHotkey.Initialize(Singleton<UINavigation>.Instance.Input);
			DisplayNextPrevHotkey(value: true);
			if (selectedCharacter != null)
			{
				selectedCharacter.SetHotkeysActive(value: true);
			}
		}
		else
		{
			nextTabHotkey.Deinitialize();
			DisplayNextPrevHotkey(value: false);
			if (selectedCharacter != null)
			{
				selectedCharacter.SetHotkeysActive(value: false);
			}
		}
	}

	public void DisplayNextPrevHotkey(bool value)
	{
		nextTabHotkey.DisplayHotkey(value);
		if (_mercenaryLabelText != null)
		{
			_mercenaryLabelText.SetActive(value);
		}
	}

	private void OnHoveredSlot(NewPartyCharacterUI slot, bool hovered)
	{
		for (int i = 0; i < charactersList.Count; i++)
		{
			charactersList[i].ToggleHoverDim(hovered && charactersList[i] != slot);
		}
	}

	public void ToggleBackgroundElements(bool isActive)
	{
		if (_toggleElementsController != null)
		{
			_toggleElementsController.ToggleElements(isActive);
		}
	}

	public void Init(CMapParty party)
	{
		ClearEvents();
		mapParty = party;
		itemService = new AdventurePartyItemEquipmentService(party);
		onCharacterSelectedCallback = null;
		flagsInteraction = FlagsInteraction.None;
		CharacterSelector.Initialize(mapParty, OnSelectedNewCharacter, OnDeselectedCharacter);
		if (AdventureState.MapState.IsCampaign)
		{
			partyStats = campaignPartyStats;
			guildmasterPartyStats.gameObject.SetActive(value: false);
		}
		else
		{
			partyStats = guildmasterPartyStats;
			campaignPartyStats.gameObject.SetActive(value: false);
		}
		if (AdventureState.MapState.HeadquartersState.PartyUIUnlocked)
		{
			Open();
			initialised = true;
		}
		else
		{
			Hide(this, instant: true);
		}
	}

	public IEnumerator InitCoroutine(CMapParty party)
	{
		ClearEvents();
		mapParty = party;
		itemService = new AdventurePartyItemEquipmentService(party);
		onCharacterSelectedCallback = null;
		flagsInteraction = FlagsInteraction.None;
		CharacterSelector.Initialize(mapParty, OnSelectedNewCharacter, OnDeselectedCharacter);
		if (AdventureState.MapState.IsCampaign)
		{
			partyStats = campaignPartyStats;
			guildmasterPartyStats.gameObject.SetActive(value: false);
		}
		else
		{
			partyStats = guildmasterPartyStats;
			campaignPartyStats.gameObject.SetActive(value: false);
		}
		if (AdventureState.MapState.HeadquartersState.PartyUIUnlocked)
		{
			yield return OpenCoroutine();
			initialised = true;
		}
		else
		{
			Hide(this, instant: true);
		}
		Singleton<ESCMenu>.Instance.OnShown.AddListener(HideCurrentDisplayOnPauseMenuOpened);
		PerkManager.PerkDisplay.SubscribeEvents();
	}

	private IEnumerator OpenCoroutine()
	{
		if (Singleton<MapChoreographer>.Instance != null)
		{
			Singleton<MapChoreographer>.Instance.EventBuss.RegisterToOnPartyGoldChanged(OnPartyGoldChanged);
			Singleton<MapChoreographer>.Instance.EventBuss.RegisterToOnCharacterXpChanged(OnXpChanged);
			Singleton<MapChoreographer>.Instance.EventBuss.RegisterToOnCharacterCreated(OnUnlockedCharacter);
			Singleton<MapChoreographer>.Instance.EventBuss.RegisterToOnCharacterItemBound(OnItemBound);
		}
		itemService.RegisterOnRemovedNewItemFlag(RefreshNewItemsNotification);
		itemService.RegisterOnAddedItemParty(OnNewItemAdded);
		itemService.RegisterOnRemovedItemParty(RefreshNewItemsNotification);
		PartyStats.Setup(mapParty);
		partyNameText.SetTextCensored(SaveData.Instance.Global.CurrentAdventureData?.DisplayPartyName);
		if (AdventureState.MapState.GoldMode == EGoldMode.PartyGold)
		{
			partyGoldContainer.SetActive(value: true);
			RefreshPartyGold(mapParty.PartyGold, animate: false);
		}
		else
		{
			partyGoldContainer.SetActive(value: false);
		}
		if ((SaveData.Instance.Global.GameMode == EGameMode.Guildmaster && AdventureState.MapState.HeadquartersState.MultiplayerUnlocked) || SaveData.Instance.Global.GameMode == EGameMode.Campaign)
		{
			foreach (CMapCharacter unselectedCharacter in mapParty.UnselectedCharacters)
			{
				if (!CharacterAlreadyBenched(unselectedCharacter))
				{
					AddBenchedCharacter(unselectedCharacter);
				}
			}
		}
		int num = (AdventureState.MapState.IsCampaign ? 4 : Mathf.Clamp(1, mapParty.CheckCharacters.Count, charactersList.Count));
		for (int i = 0; i < mapParty.SelectedCharactersArray.Count(); i++)
		{
			CMapCharacter cMapCharacter = mapParty.SelectedCharactersArray[i];
			if (cMapCharacter != null)
			{
				int controllableID = (AdventureState.MapState.IsCampaign ? cMapCharacter.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(cMapCharacter.CharacterID));
				charactersList[i].Init(null, i, cMapCharacter, OnCardsSelected, OnItemsSelected, OnCharacterSelect, OnPerksSelected, slotsToggleGroup, itemService.CountNewItems(mapParty.SelectedCharactersArray[i]), OnCharacterPickerSelected, OnLevelUpSelected, ControllableRegistry.GetController(controllableID), OnBattleGoalSelected, updateController: true);
			}
			else if (i < num)
			{
				charactersList[i].InitAvailable(null, i, slotsToggleGroup, OnCharacterSelect, OnCharacterPickerSelected);
			}
			else
			{
				charactersList[i].InitEmpty(null, i);
			}
		}
		CMapCharacter[] selectedCharactersArray = mapParty.SelectedCharactersArray;
		foreach (CMapCharacter character in selectedCharactersArray)
		{
			yield return InitPooledAbilityCardsCoroutine(character);
		}
		ShowNewClassNotification(mapParty.NewUnlockedCharacterIDs.Count > 0);
		AbilityCardsDisplay.Hide();
		Singleton<UIResetLevelUpWindow>.Instance?.Hide();
		PerkManager.Hide();
		battleGoalSelector?.Hide();
		ShowDelayed();
	}

	private void Open()
	{
		if (Singleton<MapChoreographer>.Instance != null)
		{
			Singleton<MapChoreographer>.Instance.EventBuss.RegisterToOnPartyGoldChanged(OnPartyGoldChanged);
			Singleton<MapChoreographer>.Instance.EventBuss.RegisterToOnCharacterXpChanged(OnXpChanged);
			Singleton<MapChoreographer>.Instance.EventBuss.RegisterToOnCharacterCreated(OnUnlockedCharacter);
			Singleton<MapChoreographer>.Instance.EventBuss.RegisterToOnCharacterItemBound(OnItemBound);
		}
		itemService.RegisterOnRemovedNewItemFlag(RefreshNewItemsNotification);
		itemService.RegisterOnAddedItemParty(OnNewItemAdded);
		itemService.RegisterOnRemovedItemParty(RefreshNewItemsNotification);
		PartyStats.Setup(mapParty);
		partyNameText.text = ((SaveData.Instance.Global.CurrentAdventureData != null) ? SaveData.Instance.Global.CurrentAdventureData.DisplayPartyName : string.Empty);
		if (AdventureState.MapState.GoldMode == EGoldMode.PartyGold)
		{
			partyGoldContainer.SetActive(value: true);
			RefreshPartyGold(mapParty.PartyGold, animate: false);
		}
		else
		{
			partyGoldContainer.SetActive(value: false);
		}
		if ((SaveData.Instance.Global.GameMode == EGameMode.Guildmaster && AdventureState.MapState.HeadquartersState.MultiplayerUnlocked) || SaveData.Instance.Global.GameMode == EGameMode.Campaign)
		{
			foreach (CMapCharacter unselectedCharacter in mapParty.UnselectedCharacters)
			{
				if (!CharacterAlreadyBenched(unselectedCharacter))
				{
					AddBenchedCharacter(unselectedCharacter);
				}
			}
		}
		int num = (AdventureState.MapState.IsCampaign ? 4 : Mathf.Clamp(1, mapParty.CheckCharacters.Count, charactersList.Count));
		for (int i = 0; i < mapParty.SelectedCharactersArray.Count(); i++)
		{
			CMapCharacter cMapCharacter = mapParty.SelectedCharactersArray[i];
			if (cMapCharacter != null)
			{
				int controllableID = (AdventureState.MapState.IsCampaign ? cMapCharacter.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(cMapCharacter.CharacterID));
				charactersList[i].Init(null, i, cMapCharacter, OnCardsSelected, OnItemsSelected, OnCharacterSelect, OnPerksSelected, slotsToggleGroup, itemService.CountNewItems(mapParty.SelectedCharactersArray[i]), OnCharacterPickerSelected, OnLevelUpSelected, ControllableRegistry.GetController(controllableID), OnBattleGoalSelected, updateController: true);
				InitPooledAbilityCards(cMapCharacter);
			}
			else if (i < num)
			{
				charactersList[i].InitAvailable(null, i, slotsToggleGroup, OnCharacterSelect, OnCharacterPickerSelected);
			}
			else
			{
				charactersList[i].InitEmpty(null, i);
			}
		}
		ShowNewClassNotification(mapParty.NewUnlockedCharacterIDs.Count > 0);
		AbilityCardsDisplay.Hide();
		Singleton<UIResetLevelUpWindow>.Instance?.Hide();
		PerkManager.Hide();
		battleGoalSelector?.Hide();
		ShowDelayed();
	}

	private void InitPooledAbilityCards(CMapCharacter character)
	{
		foreach (CAbilityCard ownedAbilityCard in character.GetOwnedAbilityCards())
		{
			if (ownedAbilityCard.GetAbilityCardYML == null)
			{
				Debug.LogError("Ability card data is null! Check YML file for " + ownedAbilityCard.Name);
			}
			else
			{
				ObjectPool.CreatePooledAbilityCard(ownedAbilityCard.ID, 2);
			}
		}
	}

	private IEnumerator InitPooledAbilityCardsCoroutine(CMapCharacter character)
	{
		foreach (CAbilityCard ownedAbilityCard in character.GetOwnedAbilityCards())
		{
			if (ownedAbilityCard.GetAbilityCardYML == null)
			{
				Debug.LogError("Ability card data is null! Check YML file for " + ownedAbilityCard.Name);
				continue;
			}
			ObjectPool.CreatePooledAbilityCard(ownedAbilityCard.ID, 2);
			yield return null;
		}
	}

	private void ClearEvents()
	{
		if (Singleton<ESCMenu>.Instance != null)
		{
			Singleton<ESCMenu>.Instance.OnShown.RemoveListener(HideCurrentDisplayOnPauseMenuOpened);
		}
		if (Singleton<MapChoreographer>.Instance != null)
		{
			Singleton<MapChoreographer>.Instance.EventBuss.UnregisterToOnPartyGoldChanged(OnPartyGoldChanged);
			Singleton<MapChoreographer>.Instance.EventBuss.UnregisterToOnCharacterXpChanged(OnXpChanged);
			Singleton<MapChoreographer>.Instance.EventBuss.UnregisterToOnCharacterCreated(OnUnlockedCharacter);
			Singleton<MapChoreographer>.Instance.EventBuss.UnregisterToOnCharacterItemBound(OnItemBound);
		}
		if (itemService != null)
		{
			itemService.UnregisterOnRemovedNewItemFlag(RefreshNewItemsNotification);
			itemService.UnregisterOnAddedItemParty(OnNewItemAdded);
			itemService.UnregisterOnRemovedItemParty(RefreshNewItemsNotification);
		}
	}

	private void OnXpChanged(string characterId, string characterName)
	{
		GetCharacterUI(characterId, characterName)?.OnXpChanged();
	}

	[ContextMenu("Show")]
	public void Show(object request)
	{
		hideRequests.Remove(request);
		Debug.LogGUI(string.Format("Show party panel requested by {0} (current hidding request: {1}) isOpen {2} isHidding {3}", request, string.Join(",", hideRequests), window.IsOpen, hidePanelAnimator.IsPlaying));
		if ((!window.IsOpen || hidePanelAnimator.IsPlaying) && AdventureState.MapState.HeadquartersState.PartyUIUnlocked && hideRequests.Count <= 0)
		{
			CancelShowAnimations();
			window.ShowOrUpdateStartingState();
			if (!window.HasGoneToStartingState)
			{
				window.onShown.Invoke();
			}
			showPanelAnimator.Play();
			animLockInteraction.blocksRaycasts = false;
		}
	}

	private void ShowDelayed()
	{
		CancelShowAnimations();
		window.Hide();
		showPanelAnimator.GoInitState();
		delayedShowCoroutine = StartCoroutine(DelayedShow());
	}

	private IEnumerator DelayedShow()
	{
		yield return Timekeeper.instance.WaitForSeconds(openPanelDelay);
		delayedShowCoroutine = null;
		Show(this);
	}

	[ContextMenu("Hide")]
	public void Hide(object request)
	{
		Hide(request, instant: false);
	}

	public void Hide(object request, bool instant, Action callback = null, bool deselectCurrentCharacter = true)
	{
		hideRequests.Add(request);
		if (!window.IsOpen)
		{
			Debug.LogGUI($"Hide party panel already closed, requested by {request} ({hideCallback == null})");
			if (delayedShowCoroutine != null)
			{
				CancelShowAnimations();
			}
			return;
		}
		if (deselectCurrentCharacter)
		{
			UnselectCurrentCharacter();
		}
		CancelShowAnimations();
		if (instant)
		{
			Debug.LogGUI($"Hide party panel immediatly requested by {request} ({hidePanelAnimator.IsPlaying}) ({hideCallback == null})");
			hidePanelAnimator.GoToFinishState();
			window.Hide();
			callback?.Invoke();
		}
		else
		{
			Debug.LogGUI($"Hide party panel animation requested by {request} ({hidePanelAnimator.IsPlaying}) ({hideCallback == null})");
			hideCallback = callback;
			animLockInteraction.blocksRaycasts = false;
			hidePanelAnimator.Play();
		}
	}

	private void CancelShowAnimations()
	{
		if (delayedShowCoroutine != null)
		{
			StopCoroutine(delayedShowCoroutine);
		}
		delayedShowCoroutine = null;
		showPanelAnimator.Stop(goToEnd: false);
		hidePanelAnimator.Stop(goToEnd: false);
	}

	private void OnUnlockedCharacter(CMapCharacter character)
	{
		int count = mapParty.CheckCharacters.Count;
		int num = charactersList.Where((NewPartyCharacterUI w) => w.State != PartySlotState.Empty).Count();
		if (num != charactersList.Count && count > num)
		{
			NewPartyCharacterUI newPartyCharacterUI = charactersList.FirstOrDefault((NewPartyCharacterUI f) => f.State == PartySlotState.Empty);
			if (newPartyCharacterUI != null)
			{
				newPartyCharacterUI.InitAvailable(mapParty, charactersList.IndexOf(newPartyCharacterUI), slotsToggleGroup, OnCharacterSelect, OnCharacterPickerSelected);
			}
		}
		if (!AdventureState.MapState.IsCampaign && mapParty.SelectedCharacters.Count() < charactersList.Count && mapParty.SelectedCharacters.Count() < 2)
		{
			NewPartyCharacterUI newPartyCharacterUI2 = charactersList.FirstOrDefault((NewPartyCharacterUI f) => f.State == PartySlotState.Available);
			if (newPartyCharacterUI2 != null)
			{
				int controllableID = (AdventureState.MapState.IsCampaign ? character.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(character.CharacterID));
				newPartyCharacterUI2.Init(mapParty, charactersList.IndexOf(newPartyCharacterUI2), character, OnCardsSelected, OnItemsSelected, OnCharacterSelect, OnPerksSelected, slotsToggleGroup, itemService.CountNewItems(character), OnCharacterPickerSelected, OnLevelUpSelected, ControllableRegistry.GetController(controllableID), OnBattleGoalSelected);
			}
		}
	}

	public override void UpdatePartyLevel()
	{
		PartyStats.RefreshLevel();
	}

	private void OnDisable()
	{
		CancelShowAnimations();
	}

	private void OnNewItemAdded(CItem item)
	{
		foreach (NewPartyCharacterUI characters in charactersList)
		{
			characters.ShowNewItemsNotification(itemService.CountNewItems(characters.Data) > 0);
		}
	}

	private void OnItemBound(CItem item, CMapCharacter character)
	{
		if (item.IsNew)
		{
			RefreshNewItemsNotification();
		}
	}

	private void RefreshPartyGold(int gold, bool animate)
	{
		ShowWarningGold(show: false);
		if (animate)
		{
			partyGoldCounter.CountTo(gold);
		}
		else
		{
			partyGoldCounter.SetCount(gold);
		}
	}

	private void OnPartyGoldChanged(int money)
	{
		RefreshPartyGold(money, animate: true);
	}

	public void NextCharacter()
	{
		_tab.Next();
	}

	public void RefreshNewClassesNotification()
	{
		if (initialised)
		{
			try
			{
				ShowNewClassNotification(mapParty.NewUnlockedCharacterIDs.Count > 0);
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception while trying to refresh new classes notification. MapParty is null: " + (mapParty == null) + " NewUnlockedCharacterIDs is null: " + (mapParty?.NewUnlockedCharacterIDs == null) + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	private void RefreshNewItemsNotification(CItem item)
	{
		RefreshNewItemsNotification();
	}

	private void RefreshNewItemsNotification(CItem item, int slot)
	{
		RefreshNewItemsNotification();
	}

	public void UnselectCurrentCharacter()
	{
		if (selectedCharacter != null)
		{
			selectedCharacter.OnClick();
		}
	}

	public void EscapeCurrentCharacter()
	{
		if (selectedCharacter != null)
		{
			selectedCharacter.Escape();
		}
	}

	public void Cancel()
	{
		if (window.IsOpen && !Singleton<UIConfirmationBoxManager>.Instance.IsOpen)
		{
			UnselectCurrentCharacter();
		}
	}

	private void SelectCurrentCharacter(NewPartyCharacterUI character)
	{
		if (character == null)
		{
			_tab.RemoveCurrent(deselectFirst: false);
		}
		selectedCharacter = character;
	}

	private void SetCharacterInternalIndex(int index, int totalElements)
	{
		CharacterInternalTabIndex = index;
		CharacterLastTabElementsCount = totalElements;
	}

	private void OnCharacterSelect(bool isSelected, NewPartyCharacterUI characterUI)
	{
		bool flag = selectedCharacter == characterUI;
		if (isSelected)
		{
			characterUI.ToggleSelect(active: true);
			SelectCurrentCharacter(characterUI);
			foreach (NewPartyCharacterUI characters in charactersList)
			{
				if (characters != characterUI)
				{
					characters.ToggleSelect(active: false);
					characters.ToggleDim(active: true);
				}
			}
			onCharacterSelectedCallback?.Invoke(characterUI.Data);
		}
		else
		{
			if (flag && flagsInteraction.HasFlag(FlagsInteraction.DISABLE_TOGGLE_OFF_CHARACTER))
			{
				selectedCharacter.ToggleSelect(active: true);
				return;
			}
			SelectCurrentCharacter(null);
			TryHideCurrentDisplay();
			Singleton<UIResetLevelUpWindow>.Instance.Hide();
			CloseQuickAccessAssignRole();
			if (slotsToggleGroup.AnyTogglesOn())
			{
				foreach (Toggle item in slotsToggleGroup.ActiveToggles())
				{
					if (item.isOn)
					{
						item.isOn = false;
					}
				}
			}
			foreach (NewPartyCharacterUI characters2 in charactersList)
			{
				characters2.ToggleDim(active: false);
			}
		}
		this.NewCharacterSelected?.Invoke(isSelected, characterUI);
	}

	private void OnAbilityCardSelected(bool isSelected)
	{
		this.OnAbilityDeckUpdated?.Invoke();
	}

	private void OnCardsSelected(NewPartyCharacterUI data, bool isSelected)
	{
		OnCardsSelected(data, isSelected, animate: true);
	}

	private void OnCardsSelected(NewPartyCharacterUI data, bool isSelected, bool animate, bool refreshLevelUp = true)
	{
		CloseQuickAccessAssignRole();
		if (isSelected)
		{
			if (AdventureState.MapState.IsCampaign)
			{
				TryHideCurrentDisplay(DisplayType.CARDS);
			}
			else
			{
				TryHideCurrentDisplay(_ignoreDisplayTypes);
			}
			displayActive = DisplayType.CARDS;
			if (!flagsInteraction.HasFlag(FlagsInteraction.DISABLE_RESET_LEVELUP))
			{
				Singleton<UIResetLevelUpWindow>.Instance.Show(data.Service);
			}
			bool enableLevelUp = AdventureState.MapState.IsCampaign && refreshLevelUp && disableStatsRequests.Count == 0 && data.Service.CanLevelup();
			bool isLevelUpValid = !Singleton<MapChoreographer>.Instance.IsChoosingLinkedQuestOption() && !Singleton<UILoadoutManager>.Instance.InLoadout();
			if (selectedCharacter != null)
			{
				AbilityCardsDisplay.Display(data.Data, OnAbilityCardSelected, selectedCharacter.CardPointReference, disableStatsRequests.Count == 0, animate, delegate
				{
					OnLevelUpSelected(data);
				}, delegate
				{
					data.CloseCardsWindow();
				}, enableLevelUp, isLevelUpValid);
			}
		}
		else
		{
			TryHideCurrentDisplay();
		}
		this.OnOpenedPanel?.Invoke(isSelected, DisplayType.CARDS);
	}

	private void TryHideCurrentDisplay(DisplayType ignoreType = DisplayType.NONE)
	{
		if (ignoreType != displayActive)
		{
			HideCurrentDisplay();
		}
	}

	private void TryHideCurrentDisplay(HashSet<DisplayType> displayTypesToIgnore)
	{
		if (!displayTypesToIgnore.Contains(displayActive))
		{
			HideCurrentDisplay();
		}
	}

	private void HideCurrentDisplayOnPauseMenuOpened()
	{
		if ((!AdventureState.MapState.IsCampaign || !SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.MapParty.IsCreatingCharacter) && (!MapFTUEManager.IsPlaying || Singleton<MapFTUEManager>.Instance.CurrentStep != EMapFTUEStep.CreateNewCharacter))
		{
			TryHideCurrentDisplay(_ignoreDisplayTypesForPauseMenu);
		}
	}

	private void HideCurrentDisplay()
	{
		switch (displayActive)
		{
		case DisplayType.INVENTORY:
			ItemInventoryDisplay.Hide(!slotsToggleGroup.AnyTogglesOn());
			break;
		case DisplayType.CARDS:
			AbilityCardsDisplay.Hide(!slotsToggleGroup.AnyTogglesOn());
			Singleton<UIResetLevelUpWindow>.Instance.Hide();
			break;
		case DisplayType.PERKS:
			PerkManager.Hide(!slotsToggleGroup.AnyTogglesOn());
			break;
		case DisplayType.CHARACTER_SELECTOR:
			CharacterSelector.Hide(instant: false, !slotsToggleGroup.AnyTogglesOn());
			break;
		case DisplayType.LEVELUP:
			Singleton<UIResetLevelUpWindow>.Instance.Hide();
			break;
		case DisplayType.BATTLE_GOALS:
			battleGoalSelector.Hide();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case DisplayType.NONE:
			break;
		}
		displayActive = DisplayType.NONE;
	}

	public void RefreshAbilityCardsLevelUp(ICharacterService character)
	{
		if (AbilityCardsDisplay.IsVisible && character != null && selectedCharacter?.Service == character)
		{
			AbilityCardsDisplay.EnableLevelUp(disableStatsRequests.Count == 0 && character.CanLevelup(), !Singleton<MapChoreographer>.Instance.IsChoosingLinkedQuestOption() && !Singleton<UILoadoutManager>.Instance.InLoadout());
		}
	}

	private void OnItemsSelected(NewPartyCharacterUI data, bool isSelected)
	{
		Singleton<UIResetLevelUpWindow>.Instance.Hide();
		CloseQuickAccessAssignRole();
		if (isSelected)
		{
			TryHideCurrentDisplay(DisplayType.INVENTORY);
			displayActive = DisplayType.INVENTORY;
			ItemInventoryDisplay.Display(itemService, data.Data, selectedCharacter.AddEquippedItemModifiers, delegate
			{
				selectedCharacter.RefreshEquippedItemModifiers();
			}, data.ItemPointReference, OnNewItemsDisplayed, disableStatsRequests.Count == 0, data.CloseItemsWindow);
		}
		else
		{
			TryHideCurrentDisplay();
		}
		this.OnOpenedPanel?.Invoke(isSelected, DisplayType.INVENTORY);
	}

	private void OnLevelUpSelected(NewPartyCharacterUI characterUI)
	{
		if ((FFSNetwork.IsHost && PlayerRegistry.JoiningPlayers.Count > 0) || (FFSNetwork.IsClient && PlayerRegistry.OtherClientsAreJoining) || Singleton<UILevelUpWindow>.Instance.IsLevelingUp(characterUI.Data.CharacterID))
		{
			return;
		}
		CloseQuickAccessAssignRole();
		if (!FFSNetwork.IsOnline || characterUI.Service.IsUnderMyControl)
		{
			Singleton<UIResetLevelUpWindow>.Instance.Hide();
			if (FFSNetwork.IsOnline && !characterUI.Service.IsUnderMyControl)
			{
				DisableEditStats(Singleton<UILevelUpWindow>.Instance, closeWindowsOnDisable: false);
			}
			AbilityCardsDisplay.EnableLevelUp(enable: false);
			Singleton<UILevelUpWindow>.Instance.Show(characterUI.Service, delegate
			{
				if (!FFSNetwork.IsOnline || characterUI.Data.IsUnderMyControl)
				{
					characterUI.LevelUp(delegate
					{
						if (FFSNetwork.IsOnline && !characterUI.Service.IsUnderMyControl)
						{
							EnableEditStats(Singleton<UILevelUpWindow>.Instance, closeWindowsOnEnable: false);
						}
						if (!flagsInteraction.HasFlag(FlagsInteraction.DISABLE_RESET_LEVELUP))
						{
							Singleton<UIResetLevelUpWindow>.Instance.Show(characterUI.Service);
						}
					});
				}
				else
				{
					EnableEditStats(Singleton<UILevelUpWindow>.Instance, closeWindowsOnEnable: false);
					if (!flagsInteraction.HasFlag(FlagsInteraction.DISABLE_RESET_LEVELUP))
					{
						Singleton<UIResetLevelUpWindow>.Instance.Show(characterUI.Service);
					}
				}
				displayActive = DisplayType.CARDS;
				this.OnOpenedPanel?.Invoke(selected: false, DisplayType.LEVELUP);
			});
			TryHideCurrentDisplay(_ignoreDisplayTypes);
			displayActive = DisplayType.LEVELUP;
			this.OnOpenedPanel?.Invoke(selected: true, DisplayType.LEVELUP);
		}
		else
		{
			characterUI.ToggleCardsPanel(!characterUI.CardWindowSelected);
		}
	}

	private void OnMultiplayerAssignRoleSelected(NewPartyCharacterUI characterUI)
	{
		Debug.Log("OnMultiplayerAssignRoleSelected");
		if (FFSNetwork.IsOnline && FFSNetwork.IsHost && characterUI.State != PartySlotState.Empty && disableAssignPlayerRequests.Count <= 0)
		{
			if (characterUI.State == PartySlotState.Assigned)
			{
				Singleton<UIMultiplayerSelectPlayerScreen>.Instance.Show(base.name, characterUI.Data.CharacterID, characterUI.Data.CharacterName, characterUI.SlotIndex, characterUI.Player, characterUI.ClassPointReference, null, null, (base.transform as RectTransform).rect.width);
			}
			else
			{
				Singleton<UIMultiplayerSelectPlayerScreen>.Instance.Show(base.name, null, null, characterUI.SlotIndex, characterUI.Player, characterUI.ClassPointReference, null, null, (base.transform as RectTransform).rect.width);
			}
		}
	}

	private void OnNewItemsDisplayed(CItem item)
	{
		RefreshNewItemsNotification();
		OnUpdatedNewItems?.Invoke(item);
	}

	private void RefreshNewItemsNotification()
	{
		foreach (NewPartyCharacterUI item in charactersList.Where((NewPartyCharacterUI it) => it.Data != null))
		{
			item.ShowNewItemsNotification(itemService.CountNewItems(item.Data) > 0);
		}
	}

	private void OnPerksSelected(NewPartyCharacterUI ui, bool isSelected)
	{
		AbilityCardsDisplay.Hide();
		Singleton<UIResetLevelUpWindow>.Instance.Hide();
		CloseQuickAccessAssignRole();
		if (isSelected)
		{
			TryHideCurrentDisplay(DisplayType.PERKS);
			displayActive = DisplayType.PERKS;
			bool flag = ui.Service.PerkPoints > 0 && !mapParty.HasIntroduced(EIntroductionConcept.PerksPanel.ToString());
			PerkManager.Show(ui.Service, ui.PerksPointReference, flag, OnHoveredPerk, disableStatsRequests.Count == 0, ui.ClosePerksWindow);
			if (flag)
			{
				mapParty.MarkIntroDone(EIntroductionConcept.PerksPanel.ToString());
				SaveData.Instance.SaveCurrentAdventureData();
			}
		}
		else
		{
			TryHideCurrentDisplay();
		}
		this.OnOpenedPanel?.Invoke(isSelected, DisplayType.PERKS);
	}

	private void OnHoveredPerk(AttackModifierYMLData modifier)
	{
		selectedCharacter.Data.NewEquippedItemsWithModifiers?.Remove(modifier.Name);
		selectedCharacter.RefreshEquippedItemModifiers();
		SaveData.Instance.SaveCurrentAdventureData();
	}

	public void ClearNewEquippedItemsWithModifiers()
	{
		if (!(selectedCharacter == null))
		{
			List<string> newEquippedItemsWithModifiers = selectedCharacter.Data.NewEquippedItemsWithModifiers;
			if (newEquippedItemsWithModifiers == null || newEquippedItemsWithModifiers.Count != 0)
			{
				selectedCharacter.Data.NewEquippedItemsWithModifiers?.Clear();
				selectedCharacter.RefreshEquippedItemModifiers();
				SaveData.Instance.SaveCurrentAdventureData();
			}
		}
	}

	private void OnBattleGoalSelected(NewPartyCharacterUI ui, bool isSelected)
	{
		Singleton<UIResetLevelUpWindow>.Instance.Hide();
		CloseQuickAccessAssignRole();
		if (isSelected)
		{
			TryHideCurrentDisplay(DisplayType.BATTLE_GOALS);
			displayActive = DisplayType.BATTLE_GOALS;
			battleGoalSelector.Display(AdventureState.MapState.InProgressQuestState, ui.Service, ui.ClassPointReference, delegate(bool selected)
			{
				if (selected)
				{
					IEnumerable<CMapCharacter> enumerable = mapParty.SelectedCharacters.Where((CMapCharacter it) => !FFSNetwork.IsOnline || it.IsUnderMyControl);
					CMapCharacter nextCharacter = enumerable.FirstOrDefault((CMapCharacter it) => AdventureState.MapState.InProgressQuestState.GetChosenBattleGoal(it.CharacterID) == null);
					if (nextCharacter != null && enumerable.FindIndex((CMapCharacter it) => it == ui.Data) == enumerable.FindIndex((CMapCharacter it) => it == nextCharacter) - 1)
					{
						if (!InputManager.GamePadInUse)
						{
							controllerArea.Focus();
							GetCharacterUI(nextCharacter.CharacterID).ToggleBatteGoal(toggledOn: true);
						}
					}
					else if (nextCharacter == null && enumerable.LastOrDefault() == ui.Data && !InputManager.GamePadInUse)
					{
						ui.ToggleBatteGoal(toggledOn: false);
					}
				}
			}, delegate
			{
				ui.CloseBattleGoal();
			});
		}
		else
		{
			TryHideCurrentDisplay();
		}
		this.OnOpenedPanel?.Invoke(isSelected, DisplayType.BATTLE_GOALS);
	}

	private void OnCharacterPickerSelected(NewPartyCharacterUI ui, bool isSelected, RectTransform point)
	{
		AbilityCardsDisplay.Hide();
		Singleton<UIResetLevelUpWindow>.Instance.Hide();
		CloseQuickAccessAssignRole();
		if (isSelected)
		{
			TryHideCurrentDisplay(DisplayType.CHARACTER_SELECTOR);
			displayActive = DisplayType.CHARACTER_SELECTOR;
			if (Singleton<AdventureMapUIManager>.Instance != null)
			{
				Singleton<AdventureMapUIManager>.Instance.HideTravelWarning();
			}
			CharacterSelector.Show(point, ui.Data, delegate
			{
				ui.CloseAssigWindow();
			}, !flagsInteraction.HasFlag(FlagsInteraction.DISABLE_EDIT_PARTY) && (!FFSNetwork.IsOnline || PlayerRegistry.MyPlayer.AssignedSlots.Contains(ui.SlotIndex)));
		}
		else
		{
			TryHideCurrentDisplay();
		}
		this.OnOpenedPanel?.Invoke(isSelected, DisplayType.CHARACTER_SELECTOR);
	}

	private void OnSelectedNewCharacter(CMapCharacter character, NewPartyCharacterUI slot)
	{
		if (slot != null)
		{
			AssignNewCharacterToUISlot(character, slot, PlayerRegistry.MyPlayer);
		}
		else
		{
			AssignNewCharacterToUISlot(character, selectedCharacter, PlayerRegistry.MyPlayer);
		}
	}

	public void AssignNewCharacterToUISlot(CMapCharacter character, NewPartyCharacterUI characterUISlot, NetworkPlayer initialController = null)
	{
		int newItemsNotification = itemService.CountNewItems(character);
		NewPartyCharacterUI newPartyCharacterUI = charactersList.FirstOrDefault((NewPartyCharacterUI it) => it.Data == character);
		if (newPartyCharacterUI != null)
		{
			if (characterUISlot.Data == null)
			{
				Debug.LogGUI("Init Available " + newPartyCharacterUI.Data.CharacterID);
				newPartyCharacterUI.InitAvailable(mapParty, charactersList.IndexOf(newPartyCharacterUI), slotsToggleGroup, OnCharacterSelect, OnCharacterPickerSelected);
			}
			else
			{
				int controllableID = (AdventureState.MapState.IsCampaign ? character.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(character.CharacterID));
				newPartyCharacterUI.Init(mapParty, charactersList.IndexOf(newPartyCharacterUI), characterUISlot.Data, OnCardsSelected, OnItemsSelected, OnCharacterSelect, OnPerksSelected, slotsToggleGroup, newItemsNotification, OnCharacterPickerSelected, OnLevelUpSelected, ControllableRegistry.GetController(controllableID), OnBattleGoalSelected, updateController: true);
			}
		}
		characterUISlot.Init(mapParty, charactersList.IndexOf(characterUISlot), character, OnCardsSelected, OnItemsSelected, OnCharacterSelect, OnPerksSelected, slotsToggleGroup, newItemsNotification, OnCharacterPickerSelected, OnLevelUpSelected, initialController, OnBattleGoalSelected, updateController: true);
		if (!FFSNetwork.IsOnline)
		{
			characterUISlot.ShowClassToggled();
			characterUISlot.ActivateTabsAndSetElement(0);
		}
		RemoveBenchedCharacter(character);
		if (characterUISlot == selectedCharacter)
		{
			onCharacterSelectedCallback?.Invoke(character);
		}
	}

	private void OnDeselectedCharacter(CMapCharacter character)
	{
		NewPartyCharacterUI newPartyCharacterUI = charactersList.FirstOrDefault((NewPartyCharacterUI x) => x.Data == character);
		if ((bool)newPartyCharacterUI)
		{
			newPartyCharacterUI.InitAvailable(mapParty, charactersList.IndexOf(newPartyCharacterUI), slotsToggleGroup, OnCharacterSelect, OnCharacterPickerSelected);
			if (!FFSNetwork.IsClient && slotsToggleGroup.AnyTogglesOn() && selectedCharacter == newPartyCharacterUI)
			{
				newPartyCharacterUI.ShowAssignToggled();
			}
			if (newPartyCharacterUI == selectedCharacter)
			{
				onCharacterSelectedCallback?.Invoke(null);
			}
		}
		else
		{
			Debug.LogWarning("Trying to remove the character but the character UI slot does not exist.");
		}
	}

	public void ShowNewAbilityCardWon(string characterId, string characterName, CAbilityCard card)
	{
		if (IsOpen && !(selectedCharacter == null) && !(selectedCharacter.Data.CharacterID != characterId))
		{
			OnCharacterSelect(isSelected: true, selectedCharacter);
			OnCardsSelected(selectedCharacter, isSelected: true, animate: false, refreshLevelUp: false);
			if (AdventureState.MapState.IsCampaign)
			{
				AbilityCardsDisplay.EnableLevelUp(enable: false);
			}
			AbilityCardsDisplay.HighlightCard(card);
		}
	}

	public override void CloseWindows()
	{
		TryHideCurrentDisplay();
		PerkManager.Hide();
		selectedCharacter?.CloseWindows();
		Singleton<UIResetLevelUpWindow>.Instance.Hide();
		CloseQuickAccessAssignRole();
		selectedCharacter?.Escape();
	}

	private void CloseQuickAccessAssignRole()
	{
		if (Singleton<UIMultiplayerSelectPlayerScreen>.Instance.IsOpen && Singleton<UIMultiplayerSelectPlayerScreen>.Instance.Id == base.name)
		{
			Singleton<UIMultiplayerSelectPlayerScreen>.Instance.Hide();
		}
	}

	public NewPartyCharacterUI GetCharacterUI(string characterID, string characterName = null)
	{
		if (characterName != null)
		{
			return charactersList.FirstOrDefault((NewPartyCharacterUI it) => it.Data != null && it.Data.CharacterID == characterID && it.Data.CharacterName == characterName);
		}
		return charactersList.FirstOrDefault((NewPartyCharacterUI it) => it.Data != null && it.Data.CharacterID == characterID);
	}

	public int GetCharacterSlot(NewPartyCharacterUI characterUI)
	{
		return charactersList.IndexOf(characterUI);
	}

	public override void OpenCardsPanel(string characterID, float durationOpen = -1f)
	{
		GetCharacterUI(characterID).ToggleCardsPanel(toggledOn: true);
		if (AdventureState.MapState.DLLMode != ScenarioManager.EDLLMode.Guildmaster)
		{
			AbilityCardsDisplay.Show();
		}
	}

	public void OpenBattleGoalPanel(string characterID)
	{
		_isSoloQuest = false;
		_tabInput.SetTabChangeConditions(null, null);
		GetCharacterUI(characterID)?.ToggleBatteGoal(toggledOn: true);
	}

	public void TryToggleEquipmentPanelForSelectedCharacter(bool isEnabled, bool focusIfToggled = false)
	{
		if (selectedCharacter == null)
		{
			UnityEngine.Debug.LogWarning("Selected char is null! Can't ToggleEquipmentPanelForSelectedCharacter");
		}
		else if (isEnabled && ActiveDisplay == DisplayType.INVENTORY)
		{
			if (focusIfToggled)
			{
				itemInventoryDisplay.FocusController();
			}
		}
		else
		{
			selectedCharacter.ToggleEquipmentPanel(isEnabled);
		}
	}

	public override void ToggleCardsPanel(string characterID)
	{
		NewPartyCharacterUI characterUI = GetCharacterUI(characterID);
		characterUI.ToggleCardsPanel(!characterUI.CardWindowSelected);
	}

	public void EnableEnhancementMode(Action<NewPartyCharacterUI> onSelectedCharacter)
	{
		EnableSelectionMode(onSelectedCharacter, disableButtons: true, disableButtonsIgnoreCards: true);
	}

	public void DisableEnhancementMode()
	{
		DisableSelectionMode();
	}

	public void DisableAssignToEmptySlot()
	{
		foreach (NewPartyCharacterUI item in charactersList.Where((NewPartyCharacterUI it) => it.State == PartySlotState.Available))
		{
			item.DisableAssign();
		}
	}

	public void EnableSelectionMode(Action<NewPartyCharacterUI> onSelectedCharacter, bool disableButtons = true, bool disableButtonsIgnoreCards = false)
	{
		DisableMapOptions();
		flagsInteraction = flagsInteraction | FlagsInteraction.DISABLE_TOGGLE_OFF_CHARACTER | FlagsInteraction.DISABLE_DISPLAY_ITEMS_ON_CHARACTER_SELECT;
		if (disableButtons)
		{
			flagsInteraction |= FlagsInteraction.DISABLE_BUTTONS;
		}
		charactersList.ForEach(delegate(NewPartyCharacterUI it)
		{
			it.EnableAutoOpenDefaultPanel(isEnabled: false);
			it.EnableSelectCharacter(enable: false);
			if (disableButtons || it.State != PartySlotState.Assigned)
			{
				it.DisableButtons(disableButtonsIgnoreCards);
			}
		});
		EnableAssignPlayer(enable: false, this);
		onCharacterSelectedCallback = delegate
		{
			if ((selectedCharacter == null || selectedCharacter.State != PartySlotState.Assigned) && mapParty.SelectedCharacters.Any())
			{
				OnCharacterSelect(isSelected: true, charactersList.First((NewPartyCharacterUI it) => it.State == PartySlotState.Assigned));
			}
			else
			{
				onSelectedCharacter(selectedCharacter);
			}
		};
		if (mapParty.SelectedCharacters.Any())
		{
			OnCharacterSelect(isSelected: true, (selectedCharacter == null || selectedCharacter.State != PartySlotState.Assigned) ? charactersList.First((NewPartyCharacterUI it) => it.State == PartySlotState.Assigned) : selectedCharacter);
		}
	}

	public bool SelectFirstCharacterBattleGoal()
	{
		CMapCharacter cMapCharacter = SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.MapParty.SelectedCharacters.FirstOrDefault();
		if (cMapCharacter != null)
		{
			string characterID = cMapCharacter.CharacterID;
			SelectCharacterById(characterID);
			OpenBattleGoalPanel(characterID);
			return true;
		}
		return false;
	}

	public void SelectCharacterById(string id)
	{
		_tabInput.Register();
		_tab.Initialize();
		NewPartyCharacterUI characterUI = GetCharacterUI(id);
		int characterSlot = GetCharacterSlot(characterUI);
		_tab.Select(characterSlot);
	}

	public void SelectSoloCharacter()
	{
		_tabInput.Register();
		_tab.Initialize();
		NewPartyCharacterUI characterUI = charactersList.FirstOrDefault((NewPartyCharacterUI it) => it.Data != null && !it.IsHidden);
		int characterSlot = GetCharacterSlot(characterUI);
		_tab.Select(characterSlot);
	}

	public void TrySelectCurrentCharacter()
	{
		if (selectedCharacter == null)
		{
			SelectCurrentCharacter();
		}
	}

	public void SelectCurrentCharacter()
	{
		_tabInput.Register();
		_tab.Initialize();
		_tab.CurrentOrFirst();
	}

	public void DisableSelectionMode()
	{
		flagsInteraction = flagsInteraction & ~FlagsInteraction.DISABLE_TOGGLE_OFF_CHARACTER & ~FlagsInteraction.DISABLE_BUTTONS & ~FlagsInteraction.DISABLE_DISPLAY_ITEMS_ON_CHARACTER_SELECT;
		charactersList.ForEach(delegate(NewPartyCharacterUI it)
		{
			it.EnableButtons(ignoreCardButton: false);
			it.EnableSelectCharacter(enable: true);
			it.EnableAutoOpenDefaultPanel(!flagsInteraction.HasFlag(FlagsInteraction.DISABLE_DISPLAY_ITEMS_ON_CHARACTER_SELECT));
		});
		onCharacterSelectedCallback = null;
		EnableAssignPlayer(enable: true, this);
	}

	public void DisableMapOptions()
	{
		EnableResetLevel(enableReset: false);
		if (selectedCharacter != null && selectedCharacter.State != PartySlotState.Assigned)
		{
			UnselectCurrentCharacter();
		}
		charactersList.ForEach(delegate(NewPartyCharacterUI it)
		{
			if (it.State == PartySlotState.Available)
			{
				it.DisableSelection();
			}
		});
		CloseWindows();
	}

	public void EnableMapOptions()
	{
		CloseWindows();
		EnableResetLevel(enableReset: true);
		charactersList.ForEach(delegate(NewPartyCharacterUI it)
		{
			if (it.State == PartySlotState.Available)
			{
				it.EnableSelection();
			}
		});
		_tab.Initialize();
		_tab.DeselectCurrent();
	}

	public void EnableResetLevel(bool enableReset)
	{
		if (enableReset)
		{
			flagsInteraction &= ~FlagsInteraction.DISABLE_RESET_LEVELUP;
		}
		else
		{
			flagsInteraction |= FlagsInteraction.DISABLE_RESET_LEVELUP;
		}
	}

	public void EnableClickTracker(bool enable, Component request)
	{
		if (enable)
		{
			disableClickTrackerRequests.Remove(request);
		}
		else
		{
			disableClickTrackerRequests.Add(request);
		}
		clickTracker.enabled = disableClickTrackerRequests.Count == 0;
	}

	public void ShowRequiredCharactersWarning(int requiredCharacters, bool glow)
	{
		int num = Math.Min(charactersList.Count, mapParty.CheckCharacters.Count);
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			if (charactersList[i].Data == null)
			{
				if (num2 < requiredCharacters)
				{
					charactersList[i].ShowAssignWarning(glow);
				}
				else
				{
					charactersList[i].HideAssignWarning();
				}
				num2++;
			}
		}
	}

	public void HideRequiredCharactersWarning()
	{
		for (int i = 0; i < charactersList.Count; i++)
		{
			charactersList[i].HideAssignWarning();
		}
	}

	public void ShowClassCharactersWarning(bool glow)
	{
		for (int i = 0; i < charactersList.Count; i++)
		{
			if (charactersList[i].State == PartySlotState.Assigned)
			{
				charactersList[i].ShowClassWarning(glow);
			}
		}
	}

	public void HideClassCharactersWarning()
	{
		for (int i = 0; i < charactersList.Count; i++)
		{
			charactersList[i].HideClassWarning();
		}
	}

	public void ShowCardsWarning(List<CMapCharacter> characters, bool glow)
	{
		for (int i = 0; i < charactersList.Count; i++)
		{
			if (charactersList[i].Data != null && characters.Contains(charactersList[i].Data))
			{
				charactersList[i].ShowCardsWarning(glow);
			}
		}
		for (int j = 0; j < charactersList.Count; j++)
		{
			if (charactersList[j].Data != null)
			{
				if (characters.Contains(charactersList[j].Data))
				{
					charactersList[j].ShowCardsWarning(glow);
				}
				else
				{
					charactersList[j].HideCardsWarning();
				}
			}
		}
	}

	public void HideCardsWarning()
	{
		for (int i = 0; i < charactersList.Count; i++)
		{
			charactersList[i].HideCardsWarning();
		}
	}

	public void HideLevelUpUI()
	{
		for (int i = 0; i < charactersList.Count; i++)
		{
			charactersList[i].DisableUIForLevelingUp();
		}
	}

	public void ToggleLockTabInput(bool locked)
	{
		TabInputLocked = locked;
	}

	public void ShowWarningGold(bool show, string characterId = null)
	{
		if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold)
		{
			if (!characterId.IsNOTNullOrEmpty())
			{
				foreach (NewPartyCharacterUI item in charactersList.Where((NewPartyCharacterUI it) => it.State == PartySlotState.Assigned))
				{
					item.ShowWarningGold(show);
				}
				return;
			}
			GetCharacterUI(characterId).ShowWarningGold(show);
		}
		else if (show)
		{
			warningPartyGoldAnimator.Play();
		}
		else
		{
			warningPartyGoldAnimator.Stop();
		}
	}

	public void ShowNewClassNotification(bool show)
	{
		foreach (NewPartyCharacterUI characters in charactersList)
		{
			characters.ShowNewClassNotification(show);
		}
	}

	public void ShowPerksWarning(string characterID, bool show)
	{
		GetCharacterUI(characterID).ShowPerksWarning(show);
	}

	public void EnableCharacterSelection(bool enable, Component request)
	{
		if (enable)
		{
			disableCharacterSelectionRequests.Remove(request);
		}
		else
		{
			disableCharacterSelectionRequests.Add(request);
		}
		if (disableCharacterSelectionRequests.Count > 0)
		{
			flagsInteraction |= FlagsInteraction.DISABLE_EDIT_PARTY;
		}
		else
		{
			flagsInteraction &= ~FlagsInteraction.DISABLE_EDIT_PARTY;
		}
	}

	public void EnableEditStats(Component request, bool closeWindowsOnEnable)
	{
		if (disableStatsRequests.Remove(request) && disableStatsRequests.Count == 0)
		{
			if (closeWindowsOnEnable)
			{
				CloseWindows();
			}
			else if (AbilityCardsDisplay.IsVisible)
			{
				AbilityCardsDisplay.SetSelectable(isEditAllowed: true);
			}
		}
	}

	public void DisableEditStats(Component request, bool closeWindowsOnDisable)
	{
		disableStatsRequests.Add(request);
		if (closeWindowsOnDisable && disableStatsRequests.Count == 1)
		{
			CloseWindows();
		}
	}

	public void EnableAssignPlayer(bool enable, Component request)
	{
		if (enable)
		{
			disableAssignPlayerRequests.Remove(request);
		}
		else
		{
			disableAssignPlayerRequests.Add(request);
			CloseQuickAccessAssignRole();
		}
		foreach (NewPartyCharacterUI characters in charactersList)
		{
			characters.EnableAssignPlayer(disableAssignPlayerRequests.Count == 0);
		}
	}

	public void UpdateConnectionStatus(List<string> characterIDs)
	{
		for (int i = 0; i < characterIDs.Count; i++)
		{
			GetCharacterUI(characterIDs[i]).RefreshMultiplayerState();
		}
	}

	public void UpdateConnectionStatus()
	{
		for (int i = 0; i < charactersList.Count; i++)
		{
			charactersList[i].RefreshMultiplayerState();
		}
		if (Singleton<UIMultiplayerEscSubmenu>.Instance != null)
		{
			Singleton<UIMultiplayerEscSubmenu>.Instance.RefreshAssignedHeroes();
		}
	}

	public void EnableBattleGoalsSelection(bool enable)
	{
		foreach (NewPartyCharacterUI characters in charactersList)
		{
			characters.EnableBattleGoalsSelection(enable);
		}
	}

	public void EnableBattleGoalsSelection(string characterId, bool interactable, string tooltip)
	{
		GetCharacterUI(characterId).EnableBattleGoalsSelection(enable: true, interactable, tooltip);
	}

	public void ShowBattleGoalWarning(List<string> charactersID, bool glow)
	{
		if (charactersID.IsNullOrEmpty())
		{
			return;
		}
		for (int i = 0; i < charactersList.Count; i++)
		{
			if (charactersList[i].Data != null)
			{
				if (charactersID.Contains(charactersList[i].Data.CharacterID))
				{
					charactersList[i].ShowBattleGoalWarning(glow);
				}
				else
				{
					charactersList[i].HideBattleGoalWarning();
				}
			}
		}
	}

	public void HideBattleGoalWarning()
	{
		for (int i = 0; i < charactersList.Count; i++)
		{
			charactersList[i].HideBattleGoalWarning();
		}
	}

	public bool RemoveCharacterFromSlot(string characterID, bool save)
	{
		bool executionFinished = false;
		return RemoveCharacterFromSlot(GetCharacterUI(characterID)?.Data, ref executionFinished, save);
	}

	private bool RemoveCharacterFromSlot(CMapCharacter character, ref bool executionFinished, bool save = true, bool isProxyCall = false, int switchingPlayerID = 0)
	{
		if (character == null)
		{
			executionFinished = true;
			return false;
		}
		return CharacterSelector.RemoveCharacterFromParty(character, ref executionFinished, networkIfOnline: false, save, isProxyCall, switchingPlayerID);
	}

	public void DisableSlots(List<CMapCharacter> characterSlots)
	{
		foreach (NewPartyCharacterUI characters in charactersList)
		{
			if (characters.State == PartySlotState.Assigned && characterSlots.Contains(characters.Data))
			{
				characters.DisableButtons(ignoreCardButton: false);
				characters.DisableSelection();
			}
		}
	}

	public void HideSlots(List<CMapCharacter> characterSlots)
	{
		_isSoloQuest = true;
		foreach (NewPartyCharacterUI characters in charactersList)
		{
			if (characters.State == PartySlotState.Assigned && characterSlots.Contains(characters.Data))
			{
				characters.HideAssignedCharacter();
				_tabInput.SetTabChangeConditions(IsSoloQuest, IsSoloQuest);
			}
		}
		bool IsSoloQuest()
		{
			return _isSoloQuest;
		}
	}

	public void MPRemoveCharacter(GameAction action, ref bool actionValid, ref bool executionFinished)
	{
		string targetCharacterID = (AdventureState.MapState.IsCampaign ? AdventureState.MapState.GetMapCharacterIDWithCharacterNameHash(action.ActorID) : CharacterClassManager.GetCharacterIDFromModelInstanceID(action.ActorID));
		CMapCharacter cMapCharacter = mapParty.SelectedCharacters.FirstOrDefault((CMapCharacter x) => x.CharacterID == targetCharacterID);
		if (cMapCharacter != null)
		{
			FFSNet.Console.LogInfo("Removing " + cMapCharacter.CharacterID + " from the party.");
			actionValid = RemoveCharacterFromSlot(cMapCharacter, ref executionFinished, save: true, isProxyCall: true, action.SupplementaryDataIDMax);
			return;
		}
		Debug.Log("[IsSwitchingCharacter] False - Unable to find character to remove");
		executionFinished = true;
		actionValid = false;
		PlayerRegistry.IsSwitchingCharacter = false;
		FFSNet.Console.LogWarning("Error trying to remove a character. Character returns null (ControllableID: " + action.ActorID + ").");
	}

	public void MPSwitchCharacter(GameAction action, ref bool actionValid, ref bool executionFinished)
	{
		CMapCharacter character = null;
		if (AdventureState.MapState.IsCampaign)
		{
			character = AdventureState.MapState.GetMapCharacterWithCharacterNameHash(action.ActorID);
		}
		else
		{
			string targetCharacterID = CharacterClassManager.GetCharacterIDFromModelInstanceID(action.ActorID);
			character = mapParty.CheckCharacters.FirstOrDefault((CMapCharacter x) => x.CharacterID == targetCharacterID);
		}
		if (character != null)
		{
			if (AdventureState.MapState.IsCampaign && mapParty.SelectedCharacters.Any((CMapCharacter it) => it.CharacterID == character.CharacterID && it.CharacterName != character.CharacterName && action.SupplementaryDataIDMed != mapParty.SelectedCharactersArray.IndexOf(it)))
			{
				Debug.Log("Character class already in party");
				actionValid = false;
				executionFinished = true;
				PlayerRegistry.IsSwitchingCharacter = false;
			}
			else
			{
				FFSNet.Console.LogInfo(PlayerRegistry.GetPlayer(action.SupplementaryDataIDMax).Username + " switching to " + character.CharacterID);
				executionFinished = false;
				actionValid = CharacterSelector.SelectCharacter(charactersList[action.SupplementaryDataIDMed], character, ref executionFinished, PlayerRegistry.GetPlayer(action.PlayerID), networkActionIfOnline: false, action.SupplementaryDataIDMax == PlayerRegistry.MyPlayer.PlayerID, isProxyCall: true, action.SupplementaryDataIDMax);
				CloseQuickAccessAssignRole();
			}
		}
		else
		{
			Debug.Log("[IsSwitchingCharacter] False - Unable to find character to switch");
			actionValid = false;
			executionFinished = true;
			PlayerRegistry.IsSwitchingCharacter = false;
			if (FFSNetwork.IsClient)
			{
				throw new Exception("Error switching character. Character returns null (ControllableID: " + action.ActorID + ").");
			}
		}
	}

	public void ProxyLevelUp(GameAction action)
	{
		ProxyIncreaseLevelNumber(action.ActorID, ((LevelToken)action.SupplementaryDataToken2).Level);
		ProxyUpdatePerkPoints(action.ActorID, ((PerkPointsToken)action.SupplementaryDataToken3).PerkPoints);
		ProxyModifyCardInventory(action.ActorID, (CardInventoryToken)action.SupplementaryDataToken);
		SaveData.Instance.SaveCurrentAdventureData();
	}

	public void ProxyIncreaseLevelNumber(IGHControllableState state, bool saveToFile)
	{
		ProxyIncreaseLevelNumber(state.ControllableID, ((LevelToken)state.Level).Level);
		if (saveToFile)
		{
			SaveData.Instance.SaveCurrentAdventureData();
		}
	}

	private void ProxyIncreaseLevelNumber(int controllableID, int newLevel)
	{
		CMapCharacter character = null;
		if (AdventureState.MapState.IsCampaign)
		{
			character = AdventureState.MapState.GetMapCharacterWithCharacterNameHash(controllableID);
		}
		else
		{
			string targetCharacterID = CharacterClassManager.GetCharacterIDFromModelInstanceID(controllableID);
			character = mapParty.CheckCharacters.FirstOrDefault((CMapCharacter x) => x.CharacterID == targetCharacterID);
		}
		if (character != null)
		{
			if (!mapParty.UnselectedCharacters.Contains(character))
			{
				FFSNet.Console.LogInfo("Modifying level for " + character.CharacterID);
				NewPartyCharacterUI newPartyCharacterUI = charactersList.First((NewPartyCharacterUI x) => x.Data == character);
				for (int num = newLevel - character.Level; num > 0; num--)
				{
					newPartyCharacterUI.LevelUp();
				}
			}
			return;
		}
		throw new Exception("Error increasing level number for a proxy character. Character returns null (ControllableID: " + controllableID + ").");
	}

	public void ProxyModifyCardInventory(GameAction action)
	{
		ProxyModifyCardInventory(action.ActorID, (CardInventoryToken)action.SupplementaryDataToken);
		SaveData.Instance.SaveCurrentAdventureData();
	}

	public void ProxyModifyCardInventory(IGHControllableState state, bool saveToFile)
	{
		ProxyModifyCardInventory(state.ControllableID, (CardInventoryToken)state.CardInventory);
		if (saveToFile)
		{
			SaveData.Instance.SaveCurrentAdventureData();
		}
	}

	private void ProxyModifyCardInventory(int controllableID, CardInventoryToken inventoryToken)
	{
		CMapCharacter character = null;
		if (AdventureState.MapState.IsCampaign)
		{
			character = AdventureState.MapState.GetMapCharacterWithCharacterNameHash(controllableID);
		}
		else
		{
			string targetCharacterID = CharacterClassManager.GetCharacterIDFromModelInstanceID(controllableID);
			character = mapParty.CheckCharacters.FirstOrDefault((CMapCharacter x) => x.CharacterID == targetCharacterID);
		}
		CCharacterClass cCharacterClass = CharacterClassManager.Classes.SingleOrDefault((CCharacterClass x) => x.ID == character.CharacterID);
		if (character != null)
		{
			FFSNet.Console.LogInfo("Modifying card inventory for " + character.CharacterID);
			List<int> list = character.HandAbilityCardIDs.ToList();
			character.HandAbilityCardIDs.Clear();
			character.OwnedAbilityCardIDs.Clear();
			int i;
			for (i = 0; i < inventoryToken.SelectedCardIDs.Length; i++)
			{
				if (cCharacterClass.AbilityCardsPool.Find((CAbilityCard x) => x.ID == inventoryToken.SelectedCardIDs[i]) != null)
				{
					character.OwnedAbilityCardIDs.Add(inventoryToken.SelectedCardIDs[i]);
					character.HandAbilityCardIDs.Add(inventoryToken.SelectedCardIDs[i]);
				}
			}
			int i2;
			for (i2 = 0; i2 < inventoryToken.UnselectedCardIDs.Length; i2++)
			{
				if (cCharacterClass.AbilityCardsPool.Find((CAbilityCard x) => x.ID == inventoryToken.UnselectedCardIDs[i2]) != null)
				{
					character.OwnedAbilityCardIDs.Add(inventoryToken.UnselectedCardIDs[i2]);
				}
			}
			if (AbilityCardsDisplay.IsVisible && displayActive == DisplayType.CARDS && selectedCharacter != null && selectedCharacter.Data == character)
			{
				OnCardsSelected(selectedCharacter, isSelected: true, animate: false);
			}
			NetworkPlayer controller = ControllableRegistry.GetController(controllableID);
			{
				foreach (int cardID in character.OwnedAbilityCardIDs)
				{
					CAbilityCard cAbilityCard = CharacterClassManager.AllAbilityCards.SingleOrDefault((CAbilityCard x) => x.ID == cardID);
					if (list.Contains(cardID))
					{
						if (!character.HandAbilityCardIDs.Contains(cardID))
						{
							SimpleLog.AddToSimpleLog(character.CharacterID + " " + PlayerRegistry.GetPlayerIdentifierString(controller) + " deselected card: " + cAbilityCard.StrictName + "(CardID: " + cAbilityCard.ID + ").");
						}
					}
					else if (character.HandAbilityCardIDs.Contains(cardID))
					{
						SimpleLog.AddToSimpleLog(character.CharacterID + " " + PlayerRegistry.GetPlayerIdentifierString(controller) + " selected card: " + cAbilityCard.StrictName + "(CardID: " + cAbilityCard.ID + ").");
					}
				}
				return;
			}
		}
		throw new Exception("Error modifying card inventory for proxy character. Character returns null (ControllableID: " + controllableID + ").");
	}

	public void ProxyModifyItemInventory(IGHControllableState state, bool saveToFile)
	{
		ProxyModifyItemInventory(state.ControllableID, (ItemInventoryToken)state.ItemInventory);
		if (saveToFile)
		{
			MapRuleLibraryClient.Instance.AddQueueMessage(new CMapDLLMessage(EMapDLLMessageType.CheckLockedContent), processImmediately: false);
			SaveData.Instance.SaveCurrentAdventureData();
		}
	}

	public void LogProxyModifyItemInventoryOnHost(int controllableID, ItemInventoryToken itemInventoryToken)
	{
		CMapCharacter cMapCharacter = null;
		if (AdventureState.MapState.IsCampaign)
		{
			cMapCharacter = AdventureState.MapState.GetMapCharacterWithCharacterNameHash(controllableID);
		}
		else
		{
			string targetCharacterID = CharacterClassManager.GetCharacterIDFromModelInstanceID(controllableID);
			cMapCharacter = mapParty.CheckCharacters.FirstOrDefault((CMapCharacter x) => x.CharacterID == targetCharacterID);
		}
		if (cMapCharacter == null)
		{
			return;
		}
		List<CItem> allUnlockedItems = mapParty.GetAllUnlockedItems(includeMultiplayerItemReserve: true);
		List<CItem> boundAndEquippedItems = cMapCharacter.GetBoundAndEquippedItems();
		List<CItem> list = cMapCharacter.CheckEquippedItems.ToList();
		List<CItem> list2 = new List<CItem>();
		List<CItem> list3 = new List<CItem>();
		List<CItem> list4 = new List<CItem>();
		List<CItem> list5 = new List<CItem>();
		FFSNet.Console.Log("[PROXY MODIFY ITEM INVENTORY]: Host logging proxy modify item inventory information. None of these changes are performed on the Host, this logging is just for comparing log files");
		FFSNet.Console.Log("[PROXY MODIFY ITEM INVENTORY]: " + cMapCharacter.CharacterID + ". Item Count: " + itemInventoryToken.Items.Length + ". Listing items:");
		int i;
		for (i = 0; i < itemInventoryToken.Items.Length; i++)
		{
			CItem cItem = null;
			cItem = allUnlockedItems.SingleOrDefault((CItem x) => x.NetworkID == itemInventoryToken.Items[i].ItemNetworkID);
			if (cItem != null)
			{
				switch (itemInventoryToken.Items[i].ItemState)
				{
				case ItemInventoryToken.ItemState.Unbound:
					list4.Add(cItem);
					break;
				case ItemInventoryToken.ItemState.BoundButNotEquipped:
					list3.Add(cItem);
					break;
				case ItemInventoryToken.ItemState.Equipped:
					cItem.SlotIndex = ((itemInventoryToken.Items[i].SlotIndex == byte.MaxValue) ? int.MaxValue : itemInventoryToken.Items[i].SlotIndex);
					list2.Add(cItem);
					break;
				case ItemInventoryToken.ItemState.Sold:
					list5.Add(cItem);
					break;
				}
				FFSNet.Console.Log("Name: " + cItem.Name + ". ItemState: " + itemInventoryToken.Items[i].ItemState.ToString() + ", SlotIndex: " + itemInventoryToken.Items[i].SlotIndex + ", NetworkID: " + itemInventoryToken.Items[i].ItemNetworkID + ".");
			}
			else
			{
				FFSNet.Console.LogWarning("No item found with NetworkID: " + itemInventoryToken.Items[i].ItemNetworkID + ", SlotIndex: " + itemInventoryToken.Items[i].SlotIndex + ", ItemState: " + itemInventoryToken.Items[i].ItemState, customFlag: true);
			}
		}
		List<CItem> list6 = list2.Concat(list3).Concat(list4).Concat(list5)
			.ToList();
		foreach (CItem item in boundAndEquippedItems)
		{
			if (!list6.Exists((CItem x) => x.NetworkID == item.NetworkID) && !mapParty.MultiplayerItemReserve.Exists((CItem x) => x.NetworkID == item.NetworkID))
			{
				FFSNet.Console.Log("[PROXY MODIFY ITEM INVENTORY]: Item (Name: " + item.Name + " NetworkID: " + item.NetworkID + ") added to Multiplayer item reserve.");
			}
		}
		foreach (CItem itemUpdatedInState in list6)
		{
			if (mapParty.MultiplayerItemReserve.Exists((CItem x) => x.NetworkID == itemUpdatedInState.NetworkID))
			{
				FFSNet.Console.Log("[PROXY MODIFY ITEM INVENTORY]: Item (Name: " + itemUpdatedInState.Name + " NetworkID: " + itemUpdatedInState.NetworkID + ") removed from Multiplayer item reserve.");
			}
		}
		NetworkPlayer controller = ControllableRegistry.GetController(controllableID);
		foreach (CItem item2 in list)
		{
			if (!list2.Exists((CItem x) => x.NetworkID == item2.NetworkID))
			{
				FFSNet.Console.Log("[PROXY MODIFY ITEM INVENTORY]: " + cMapCharacter.CharacterID + " " + PlayerRegistry.GetPlayerIdentifierString(controller) + " unequipped " + item2.YMLData.StringID + " (NetworkID: " + item2.NetworkID + ").", customFlag: true);
			}
		}
		foreach (CItem item3 in list2)
		{
			if (!list.Exists((CItem x) => x.NetworkID == item3.NetworkID))
			{
				FFSNet.Console.Log("[PROXY MODIFY ITEM INVENTORY]: " + cMapCharacter.CharacterID + " " + PlayerRegistry.GetPlayerIdentifierString(controller) + " equipped " + item3.YMLData.StringID + " (NetworkID: " + item3.NetworkID + ").", customFlag: true);
			}
		}
		FFSNet.Console.Log("[PROXY MODIFY ITEM INVENTORY]: Finished processing " + cMapCharacter.CharacterID + ". Item count: " + (cMapCharacter.CheckEquippedItems.Count + cMapCharacter.CheckBoundItems.Count + mapParty.CheckUnboundItems.Count) + " (Equipped: " + cMapCharacter.CheckEquippedItems.Count + ". Inventory [bound]: " + cMapCharacter.CheckBoundItems.Count + ". Inventory [unbound]: " + mapParty.CheckUnboundItems.Count + ").");
	}

	private void ProxyModifyItemInventory(int controllableID, ItemInventoryToken itemInventoryToken)
	{
		CMapCharacter cMapCharacter = null;
		if (AdventureState.MapState.IsCampaign)
		{
			cMapCharacter = AdventureState.MapState.GetMapCharacterWithCharacterNameHash(controllableID);
		}
		else
		{
			string targetCharacterID = CharacterClassManager.GetCharacterIDFromModelInstanceID(controllableID);
			cMapCharacter = mapParty.CheckCharacters.FirstOrDefault((CMapCharacter x) => x.CharacterID == targetCharacterID);
		}
		if (cMapCharacter != null)
		{
			List<CItem> allUnlockedItems = mapParty.GetAllUnlockedItems(includeMultiplayerItemReserve: true);
			List<CItem> boundAndEquippedItems = cMapCharacter.GetBoundAndEquippedItems();
			List<CItem> checkUnboundItems = mapParty.CheckUnboundItems;
			List<CItem> allBoundPartyItems = mapParty.GetAllBoundPartyItems();
			List<CItem> checkEquippedItems = cMapCharacter.CheckEquippedItems;
			List<CItem> list = new List<CItem>();
			List<CItem> list2 = new List<CItem>();
			List<CItem> list3 = new List<CItem>();
			List<CItem> list4 = new List<CItem>();
			FFSNet.Console.Log("[PROXY MODIFY ITEM INVENTORY]: " + cMapCharacter.CharacterID + ". Item Count: " + itemInventoryToken.Items.Length + ". Listing items:");
			int i;
			for (i = 0; i < itemInventoryToken.Items.Length; i++)
			{
				CItem cItem = null;
				cItem = allUnlockedItems.SingleOrDefault((CItem x) => x.NetworkID == itemInventoryToken.Items[i].ItemNetworkID);
				if (cItem != null)
				{
					switch (itemInventoryToken.Items[i].ItemState)
					{
					case ItemInventoryToken.ItemState.Unbound:
						list3.Add(cItem);
						break;
					case ItemInventoryToken.ItemState.BoundButNotEquipped:
						list2.Add(cItem);
						break;
					case ItemInventoryToken.ItemState.Equipped:
						cItem.SlotIndex = ((itemInventoryToken.Items[i].SlotIndex == byte.MaxValue) ? int.MaxValue : itemInventoryToken.Items[i].SlotIndex);
						list.Add(cItem);
						break;
					case ItemInventoryToken.ItemState.Sold:
						list4.Add(cItem);
						break;
					default:
						throw new Exception("[PROXY MODIFY ITEM INVENTORY]: Error modifying inventory. Uninitialized ItemState detected. ItemName: " + cItem.YMLData.StringID + ", ItemState: " + itemInventoryToken.Items[i].ItemState.ToString() + "NetworkID: " + itemInventoryToken.Items[i].ItemNetworkID + ").");
					}
					FFSNet.Console.Log("Name: " + cItem.Name + ". ItemState: " + itemInventoryToken.Items[i].ItemState.ToString() + ", SlotIndex: " + itemInventoryToken.Items[i].SlotIndex + ", NetworkID: " + itemInventoryToken.Items[i].ItemNetworkID + ".");
				}
				else
				{
					FFSNet.Console.LogWarning("No item found with NetworkID: " + itemInventoryToken.Items[i].ItemNetworkID + ", SlotIndex: " + itemInventoryToken.Items[i].SlotIndex + ", ItemState: " + itemInventoryToken.Items[i].ItemState, customFlag: true);
				}
			}
			List<CItem> list5 = list.Concat(list2).Concat(list3).Concat(list4)
				.ToList();
			foreach (CItem item in boundAndEquippedItems.Concat(checkUnboundItems))
			{
				if (!list5.Exists((CItem x) => x.NetworkID == item.NetworkID) && !mapParty.MultiplayerItemReserve.Exists((CItem x) => x.NetworkID == item.NetworkID))
				{
					item.SlotIndex = int.MaxValue;
					item.IsSlotIndexSet = false;
					item.SlotState = CItem.EItemSlotState.None;
					mapParty.MultiplayerItemReserve.Add(item);
					FFSNet.Console.Log("[PROXY MODIFY ITEM INVENTORY]: Item (Name: " + item.Name + " NetworkID: " + item.NetworkID + ") added to Multiplayer item reserve.");
				}
			}
			foreach (CItem itemUpdatedInState in list5)
			{
				if (mapParty.MultiplayerItemReserve.Exists((CItem x) => x.NetworkID == itemUpdatedInState.NetworkID))
				{
					mapParty.MultiplayerItemReserve.Remove(itemUpdatedInState);
					FFSNet.Console.Log("[PROXY MODIFY ITEM INVENTORY]: Item (Name: " + itemUpdatedInState.Name + " NetworkID: " + itemUpdatedInState.NetworkID + ") removed from Multiplayer item reserve.");
				}
			}
			for (int num = list3.Count - 1; num >= 0; num--)
			{
				CItem newUnboundItem = list3[num];
				if (allBoundPartyItems.Exists((CItem x) => x.NetworkID == newUnboundItem.NetworkID) && !boundAndEquippedItems.Exists((CItem x) => x.NetworkID == newUnboundItem.NetworkID))
				{
					list3.Remove(newUnboundItem);
				}
			}
			cMapCharacter.ClearEquippedItems();
			cMapCharacter.NewEquippedItemsWithModifiers?.Clear();
			cMapCharacter.ClearBoundItems();
			mapParty.ClearUnboundItems();
			foreach (CItem item5 in list2)
			{
				cMapCharacter.AddItemToBoundItems(item5);
			}
			foreach (CItem item6 in list3)
			{
				mapParty.AddItemToUnboundItems(item6);
			}
			List<CMapCharacter> checkCharacters = mapParty.CheckCharacters;
			foreach (CItem item2 in list)
			{
				foreach (CMapCharacter item7 in checkCharacters)
				{
					if (item7 != cMapCharacter)
					{
						CItem cItem2 = item7.CheckEquippedItems.SingleOrDefault((CItem x) => x.ItemGuid == item2.ItemGuid);
						if (cItem2 != null)
						{
							item7.RemoveEquippedItem(cItem2);
						}
						CItem cItem3 = item7.CheckBoundItems.SingleOrDefault((CItem x) => x.ItemGuid == item2.ItemGuid);
						if (cItem3 != null)
						{
							item7.RemoveItemFromBoundItems(cItem3);
						}
					}
				}
				CItem cItem4 = checkUnboundItems.SingleOrDefault((CItem x) => x.ItemGuid == item2.ItemGuid);
				if (cItem4 != null)
				{
					mapParty.RemoveItemFromUnboundItems(cItem4);
					checkUnboundItems.Remove(cItem4);
				}
				cMapCharacter.EquipItem(item2, item2.SlotIndex, forceEquip: true);
			}
			cMapCharacter.GetSmallItemMax();
			List<CItem> checkMerchantStock = AdventureState.MapState.HeadquartersState.CheckMerchantStock;
			foreach (CItem soldItem in list4)
			{
				if (!checkMerchantStock.Exists((CItem x) => x.NetworkID == soldItem.NetworkID))
				{
					SimpleLog.AddToSimpleLog("[PROXY MODIFY ITEM INVENTORY]: Local client user adding sold item back to merchant stock: " + soldItem.YMLData.StringID + " (NetworkID: " + soldItem.NetworkID + ").");
					AdventureState.MapState.HeadquartersState.AddItemToMerchantStock(soldItem);
					checkMerchantStock.Add(soldItem);
					if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold)
					{
						cMapCharacter.ModifyGold(soldItem.SellPrice, useGoldModifier: true);
					}
				}
			}
			List<CItem> checkUnboundItems2 = mapParty.CheckUnboundItems;
			List<CItem> boundAndEquippedItems2 = cMapCharacter.GetBoundAndEquippedItems();
			for (int num2 = checkMerchantStock.Count - 1; num2 >= 0; num2--)
			{
				CItem cItem5 = checkMerchantStock[num2];
				if (checkUnboundItems2.Contains(cItem5) || boundAndEquippedItems2.Contains(cItem5))
				{
					SimpleLog.AddToSimpleLog("[PROXY MODIFY ITEM INVENTORY]: Local client user removing bought item from merchant stock: " + cItem5.YMLData.StringID + " (NetworkID: " + cItem5.NetworkID + ")" + ((cMapCharacter != null) ? (" for CharacterID: " + cMapCharacter.CharacterID) : ""));
					AdventureState.MapState.HeadquartersState.RemoveItemFromMerchantStock(cItem5);
					checkMerchantStock.Remove(cItem5);
					if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold)
					{
						cMapCharacter.ModifyGold(-(cItem5.YMLData.Cost + AdventureState.MapState.MapParty.ShopDiscount), useGoldModifier: true);
					}
				}
			}
			if (ItemInventoryDisplay.IsVisible && selectedCharacter != null && selectedCharacter.Data == cMapCharacter)
			{
				ItemInventoryDisplay.Refresh(cMapCharacter);
			}
			if (Singleton<UIShopItemWindow>.Instance != null)
			{
				Singleton<UIShopItemWindow>.Instance.RefreshView();
			}
			if (AdventureState.MapState.GoldMode == EGoldMode.PartyGold)
			{
				mapParty.ModifyPartyGold(itemInventoryToken.PartyGold - mapParty.PartyGold, useGoldModifier: false);
			}
			NetworkPlayer controller = ControllableRegistry.GetController(controllableID);
			foreach (CItem item3 in checkEquippedItems)
			{
				if (!list.Exists((CItem x) => x.NetworkID == item3.NetworkID))
				{
					FFSNet.Console.Log("[PROXY MODIFY ITEM INVENTORY]: " + cMapCharacter.CharacterID + " " + PlayerRegistry.GetPlayerIdentifierString(controller) + " unequipped " + item3.YMLData.StringID + " (NetworkID: " + item3.NetworkID + ").", customFlag: true);
				}
			}
			foreach (CItem item4 in list)
			{
				if (!checkEquippedItems.Exists((CItem x) => x.NetworkID == item4.NetworkID))
				{
					FFSNet.Console.Log("[PROXY MODIFY ITEM INVENTORY]: " + cMapCharacter.CharacterID + " " + PlayerRegistry.GetPlayerIdentifierString(controller) + " equipped " + item4.YMLData.StringID + " (NetworkID: " + item4.NetworkID + ").", customFlag: true);
				}
			}
			FFSNet.Console.Log("[PROXY MODIFY ITEM INVENTORY]: Finished processing " + cMapCharacter.CharacterID + ". Item count: " + (cMapCharacter.CheckEquippedItems.Count + cMapCharacter.CheckBoundItems.Count + mapParty.CheckUnboundItems.Count) + " (Equipped: " + cMapCharacter.CheckEquippedItems.Count + ". Inventory [bound]: " + cMapCharacter.CheckBoundItems.Count + ". Inventory [unbound]: " + mapParty.CheckUnboundItems.Count + ").");
			AdventureState.MapState.CheckForDuplicateItems();
			return;
		}
		throw new Exception("[PROXY MODIFY ITEM INVENTORY]: Error modifying item inventory for proxy character. Character returns null (ControllableID: " + controllableID + ").");
	}

	public void ServerEquipItem(GameAction action)
	{
		CMapCharacter cMapCharacter = null;
		if (AdventureState.MapState.IsCampaign)
		{
			cMapCharacter = AdventureState.MapState.GetMapCharacterWithCharacterNameHash(action.ActorID);
		}
		else
		{
			string targetCharacterID = CharacterClassManager.GetCharacterIDFromModelInstanceID(action.ActorID);
			cMapCharacter = mapParty.CheckCharacters.FirstOrDefault((CMapCharacter x) => x.CharacterID == targetCharacterID);
		}
		if (cMapCharacter != null)
		{
			ItemToken itemToken = action.SupplementaryDataToken as ItemToken;
			CItem cItem = cMapCharacter.GetBoundAndEquippedItems().SingleOrDefault((CItem x) => x.NetworkID == itemToken.ItemNetworkID);
			if (cItem != null)
			{
				if (!cMapCharacter.CheckEquippedItems.Contains(cItem) || cItem.SlotIndex != itemToken.ItemSlotIndex)
				{
					cMapCharacter.UnequipPreviouslyEquippedItems(cItem, itemToken.ItemSlotIndex);
					cMapCharacter.EquipItem(cItem, itemToken.ItemSlotIndex);
					FinishInventoryModification((AdventureState.MapState.GoldMode == EGoldMode.CharacterGold) ? cMapCharacter : null);
					NetworkPlayer controller = ControllableRegistry.GetController(AdventureState.MapState.IsCampaign ? cMapCharacter.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(cMapCharacter.CharacterID));
					FFSNet.Console.Log(cMapCharacter.CharacterID + " " + PlayerRegistry.GetPlayerIdentifierString(controller) + " equipped " + cItem.YMLData.StringID + " (NetworkID: " + cItem.NetworkID + ").", customFlag: true);
				}
				return;
			}
			throw new Exception("Error equipping an item for proxy character. Item returns null (NetworkID: " + itemToken.ItemNetworkID + ").");
		}
		throw new Exception("Error equipping item for proxy character. Character returns null (ActorID: " + action.ActorID + ").");
	}

	public void ServerUnequipItem(GameAction action)
	{
		CMapCharacter cMapCharacter = null;
		if (AdventureState.MapState.IsCampaign)
		{
			cMapCharacter = AdventureState.MapState.GetMapCharacterWithCharacterNameHash(action.ActorID);
		}
		else
		{
			string targetCharacterID = CharacterClassManager.GetCharacterIDFromModelInstanceID(action.ActorID);
			cMapCharacter = mapParty.CheckCharacters.FirstOrDefault((CMapCharacter x) => x.CharacterID == targetCharacterID);
		}
		if (cMapCharacter != null)
		{
			ItemToken itemToken = action.SupplementaryDataToken as ItemToken;
			CItem cItem = cMapCharacter.CheckEquippedItems.SingleOrDefault((CItem x) => x.NetworkID == itemToken.ItemNetworkID);
			if (cItem == null)
			{
				cItem = cMapCharacter.CheckBoundItems.SingleOrDefault((CItem x) => x.NetworkID == itemToken.ItemNetworkID);
			}
			if (cItem != null)
			{
				if (cMapCharacter.CheckEquippedItems.Contains(cItem))
				{
					cMapCharacter.UnequipItems(new List<CItem> { cItem });
					FinishInventoryModification((AdventureState.MapState.GoldMode == EGoldMode.CharacterGold) ? cMapCharacter : null);
					NetworkPlayer controller = ControllableRegistry.GetController(AdventureState.MapState.IsCampaign ? cMapCharacter.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(cMapCharacter.CharacterID));
					FFSNet.Console.Log(cMapCharacter.CharacterID + " " + PlayerRegistry.GetPlayerIdentifierString(controller) + " unequipped " + cItem.YMLData.StringID + " (NetworkID: " + cItem.NetworkID + ").", customFlag: true);
				}
				return;
			}
			throw new Exception("Error unequipping an item for proxy character. Item returns null (NetworkID: " + itemToken.ItemNetworkID + ").");
		}
		throw new Exception("Error unequipping item for proxy character. Character returns null (ActorID: " + action.ActorID + ").");
	}

	public void ServerBindAndEquipItem(GameAction action)
	{
		CMapCharacter cMapCharacter = null;
		if (AdventureState.MapState.IsCampaign)
		{
			cMapCharacter = AdventureState.MapState.GetMapCharacterWithCharacterNameHash(action.ActorID);
		}
		else
		{
			string targetCharacterID = CharacterClassManager.GetCharacterIDFromModelInstanceID(action.ActorID);
			cMapCharacter = mapParty.CheckCharacters.FirstOrDefault((CMapCharacter x) => x.CharacterID == targetCharacterID);
		}
		if (cMapCharacter != null)
		{
			ItemToken itemToken = action.SupplementaryDataToken as ItemToken;
			CItem cItem = mapParty.CheckUnboundItems.SingleOrDefault((CItem x) => x.NetworkID == itemToken.ItemNetworkID);
			if (cItem != null)
			{
				mapParty.BindItem(cMapCharacter.CharacterID, cMapCharacter.CharacterName, cItem);
				cMapCharacter.UnequipPreviouslyEquippedItems(cItem, itemToken.ItemSlotIndex);
				cMapCharacter.EquipItem(cItem, itemToken.ItemSlotIndex);
				FinishInventoryModification((AdventureState.MapState.GoldMode == EGoldMode.CharacterGold) ? cMapCharacter : null);
				NetworkPlayer player = PlayerRegistry.GetPlayer(action.PlayerID);
				NetworkPlayer controller = ControllableRegistry.GetController(AdventureState.MapState.IsCampaign ? cMapCharacter.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(cMapCharacter.CharacterID));
				FFSNet.Console.Log(((player != null) ? player.Username : "NULL PLAYER") + " bound and equipped " + cItem.YMLData.StringID + " (NetworkID: " + cItem.NetworkID + ") to " + cMapCharacter.CharacterID + " (Controller: " + ((controller != null) ? controller.Username : "NULL PLAYER") + ")", customFlag: true);
			}
			else
			{
				FFSNet.Console.LogWarning("Error binding and equipping an item for proxy character. Item returns null (NetworkID: " + itemToken.ItemNetworkID + ").");
			}
		}
		else
		{
			FFSNet.Console.LogWarning("Error binding and equipping an item for proxy character. Character returns null (ActorID: " + action.ActorID + ").");
		}
	}

	public void ServerUnbindItem(GameAction action)
	{
		CMapCharacter cMapCharacter = null;
		if (AdventureState.MapState.IsCampaign)
		{
			cMapCharacter = AdventureState.MapState.GetMapCharacterWithCharacterNameHash(action.ActorID);
		}
		else
		{
			string targetCharacterID = CharacterClassManager.GetCharacterIDFromModelInstanceID(action.ActorID);
			cMapCharacter = mapParty.CheckCharacters.FirstOrDefault((CMapCharacter x) => x.CharacterID == targetCharacterID);
		}
		if (cMapCharacter != null)
		{
			ItemToken itemToken = action.SupplementaryDataToken as ItemToken;
			CItem cItem = cMapCharacter.GetBoundAndEquippedItems().SingleOrDefault((CItem x) => x.NetworkID == itemToken.ItemNetworkID);
			if (cItem != null)
			{
				switch (AdventureState.MapState.GoldMode)
				{
				case EGoldMode.PartyGold:
					if (mapParty.PartyGold < cItem.SellPrice)
					{
						return;
					}
					mapParty.ModifyPartyGold(-cItem.SellPrice, useGoldModifier: false);
					break;
				case EGoldMode.CharacterGold:
					if (cMapCharacter.CharacterGold < cItem.SellPrice)
					{
						return;
					}
					cMapCharacter.ModifyGold(-cItem.SellPrice, useGoldModifier: false);
					break;
				case EGoldMode.None:
					FFSNet.Console.LogError("ERROR_MULTIPLAYER_00004", "Error modifying item inventory. Gold mode not determined.");
					return;
				default:
					FFSNet.Console.LogError("ERROR_MULTIPLAYER_00005", "Error modifying item inventory. Unrecognized gold mode found.");
					return;
				}
				if (cMapCharacter.CheckEquippedItems.Contains(cItem))
				{
					cMapCharacter.UnequipItems(new List<CItem> { cItem });
				}
				cMapCharacter.UnbindItem(cItem);
				FinishInventoryModification((AdventureState.MapState.GoldMode == EGoldMode.CharacterGold) ? cMapCharacter : null);
			}
			else
			{
				FFSNet.Console.LogWarning("Error unbinding an item for proxy character. Item returns null (NetworkID: " + itemToken.ItemNetworkID + ").");
			}
		}
		else
		{
			FFSNet.Console.LogWarning("Error unbinding an item for proxy character. Character returns null (ActorID: " + action.ActorID + ").");
		}
	}

	private void FinishInventoryModification(CMapCharacter character)
	{
		if (ItemInventoryDisplay.IsVisible && selectedCharacter != null && selectedCharacter.Data == character)
		{
			ItemInventoryDisplay.Refresh(character);
		}
		foreach (CMapCharacter checkCharacter in SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.MapParty.CheckCharacters)
		{
			if (!AdventureState.MapState.IsCampaign || checkCharacter.PersonalQuest != null)
			{
				int controllableID = (AdventureState.MapState.IsCampaign ? checkCharacter.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(checkCharacter.CharacterID));
				ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
				IProtocolToken supplementaryDataToken = new ItemInventoryToken(checkCharacter, mapParty, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.GoldMode);
				Synchronizer.ReplicateControllableStateChange(GameActionType.ModifyItemInventory, currentPhase, controllableID, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
			}
		}
		SaveData.Instance.SaveCurrentAdventureData();
	}

	public void ProxyModifyPerks(GameAction action)
	{
		ProxyUpdatePerkPoints(action.ActorID, ((PerkPointsToken)action.SupplementaryDataToken).PerkPoints);
		ProxyModifyPerks(action.ActorID, (PerksToken)action.SupplementaryDataToken2);
		SaveData.Instance.SaveCurrentAdventureData();
	}

	public void ProxyModifyPerks(IGHControllableState state, bool saveToFile)
	{
		ProxyModifyPerks(state.ControllableID, (PerksToken)state.ActivePerks);
		if (saveToFile)
		{
			SaveData.Instance.SaveCurrentAdventureData();
		}
	}

	private void ProxyModifyPerks(int controllableID, PerksToken perksToken)
	{
		CMapCharacter cMapCharacter = null;
		if (AdventureState.MapState.IsCampaign)
		{
			cMapCharacter = AdventureState.MapState.GetMapCharacterWithCharacterNameHash(controllableID);
		}
		else
		{
			string targetCharacterID = CharacterClassManager.GetCharacterIDFromModelInstanceID(controllableID);
			cMapCharacter = mapParty.CheckCharacters.FirstOrDefault((CMapCharacter x) => x.CharacterID == targetCharacterID);
		}
		if (cMapCharacter != null)
		{
			FFSNet.Console.LogInfo("Modifying perks for " + cMapCharacter.CharacterID + " ( has perks: " + (cMapCharacter.Perks != null) + ")");
			List<CharacterPerk> list = cMapCharacter.Perks.FindAll((CharacterPerk x) => x.IsActive);
			FFSNet.Console.LogInfo("Already enabled perks " + list.Count);
			int[] activePerkIndexes = perksToken.ActivePerkIndexes;
			foreach (int num2 in activePerkIndexes)
			{
				if (cMapCharacter.Perks[num2] == null)
				{
					Debug.LogErrorFormat("Character perk index {0} is null", num2);
				}
				cMapCharacter.Perks[num2].IsActive = true;
				if (!list.Contains(cMapCharacter.Perks[num2]))
				{
					SimpleLog.AddToSimpleLog("MapCharacter: " + cMapCharacter.CharacterID + " enabled perk: " + cMapCharacter.Perks[num2].PerkID);
				}
			}
			if (PerkManager.IsVisible)
			{
				if (selectedCharacter == null)
				{
					Debug.LogErrorFormat("Selected character is null");
					PerkManager.Refresh(disableStatsRequests.Count == 0);
				}
				else if (selectedCharacter.Data == cMapCharacter)
				{
					PerkManager.PerkDisplay.Display(selectedCharacter.Service, selectedCharacter.PerksPointReference, disableStatsRequests.Count == 0);
					PerkManager.ModifierDisplay.Display(selectedCharacter.Service);
				}
			}
			return;
		}
		throw new Exception("Error modifying perks for proxy character. Character returns null (ControllableID: " + controllableID + ").");
	}

	public void ProxyUpdatePerkPoints(IGHControllableState state, bool saveToFile)
	{
		ProxyUpdatePerkPoints(state.ControllableID, ((PerkPointsToken)state.PerkPoints).PerkPoints);
		if (saveToFile)
		{
			SaveData.Instance.SaveCurrentAdventureData();
		}
	}

	private void ProxyUpdatePerkPoints(int controllableID, int perkPoints)
	{
		CMapCharacter cMapCharacter = null;
		if (AdventureState.MapState.IsCampaign)
		{
			cMapCharacter = AdventureState.MapState.GetMapCharacterWithCharacterNameHash(controllableID);
		}
		else
		{
			string targetCharacterID = CharacterClassManager.GetCharacterIDFromModelInstanceID(controllableID);
			cMapCharacter = mapParty.CheckCharacters.FirstOrDefault((CMapCharacter x) => x.CharacterID == targetCharacterID);
		}
		if (cMapCharacter != null)
		{
			FFSNet.Console.LogInfo("Updating perk points for " + cMapCharacter.CharacterID);
			cMapCharacter.UpdatePerkPoints(perkPoints);
			if (PerkManager.IsVisible && selectedCharacter != null && selectedCharacter.Data == cMapCharacter)
			{
				if (PerkManager.PerkDisplay == null)
				{
					Debug.LogError("PerkDisplay missing");
				}
				PerkManager.PerkDisplay.Display(selectedCharacter.Service, selectedCharacter.PerksPointReference, disableStatsRequests.Count == 0);
			}
			return;
		}
		throw new Exception("Error updating perk points proxy character. Character returns null (ControllableID: " + controllableID + ").");
	}

	public void RemoveBenchedCharacter(CMapCharacter characterData)
	{
		try
		{
			BenchedCharacter benchedCharacter = GetBenchedCharacter(characterData);
			if (benchedCharacter != null)
			{
				m_BenchedCharacters.Remove(benchedCharacter);
				FFSNet.Console.Log("Removed " + benchedCharacter.CharacterData.CharacterID + " from the list of benched characters.");
			}
		}
		catch (Exception)
		{
			Debug.LogError("Error attempting to remove benched character from NewPartyDisplayUI BenchedCharacters list. Looking for CharacterID: " + characterData.CharacterID + ((!string.IsNullOrEmpty(characterData.CharacterName)) ? (" CharacterName: " + characterData.CharacterName) : ""));
			Debug.Log("Benched Character List: " + m_BenchedCharacters.Select((BenchedCharacter x) => "ID: " + x.CharacterData.CharacterID + " Name: " + x.CharacterData.CharacterName).ToStringPretty());
		}
	}

	public BenchedCharacter GetBenchedCharacter(CMapCharacter character)
	{
		return m_BenchedCharacters.SingleOrDefault((BenchedCharacter x) => x.CharacterData.CharacterID == character.CharacterID && x.CharacterData.CharacterName == character.CharacterName);
	}

	public void AddBenchedCharacter(CMapCharacter character)
	{
		if (!CharacterAlreadyBenched(character))
		{
			m_BenchedCharacters.Add(new BenchedCharacter(character));
			Debug.Log("Added character to benched character list. ID: " + character.CharacterID + " Name: " + character.CharacterName);
		}
		else
		{
			Debug.LogError("Attempting to bench a character that is already benched. ID: " + character.CharacterID + " Name: " + character.CharacterName);
		}
	}

	public bool CharacterAlreadyBenched(CMapCharacter character)
	{
		return m_BenchedCharacters.Any((BenchedCharacter x) => x.CharacterData.CharacterID == character.CharacterID && x.CharacterData.CharacterName == character.CharacterName);
	}

	public void DestroyBenchedCharacters()
	{
		m_BenchedCharacters.ForEach(delegate(BenchedCharacter x)
		{
			x.Destroy();
		});
		m_BenchedCharacters.Clear();
	}

	public void PrintBenchedCharacters()
	{
		FFSNet.Console.LogDebug("Printing benched characters (" + m_BenchedCharacters.Count + "):");
		foreach (BenchedCharacter benchedCharacter in m_BenchedCharacters)
		{
			if (benchedCharacter != null)
			{
				FFSNet.Console.LogDebug(benchedCharacter.GetName());
			}
			else
			{
				FFSNet.Console.LogDebug("Null character.");
			}
		}
	}

	public void RefreshCharacterOwner(string characterID, string characterName, NetworkPlayer newController)
	{
		if (Singleton<UILevelUpWindow>.Instance.IsLevelingUp(characterID) && newController != PlayerRegistry.MyPlayer)
		{
			Singleton<UILevelUpWindow>.Instance.Close();
		}
		NewPartyCharacterUI characterUI = GetCharacterUI(characterID, characterName);
		if (!(characterUI == null))
		{
			characterUI.OnControlAssigned(newController);
			if (CharacterSelector.IsVisible && CharacterSelector is UICampaignAdventurePartyAssemblyWindow)
			{
				((UICampaignAdventurePartyAssemblyWindow)CharacterSelector).RefreshDisabledCharacterOptions();
			}
			if (characterUI == selectedCharacter && mapParty.SelectedCharacters.Contains(characterUI.Data))
			{
				onCharacterSelectedCallback?.Invoke(selectedCharacter.Data);
			}
		}
	}

	private void OnControllerAreaFocused()
	{
		if (selectedCharacter != null && selectedCharacter.State != PartySlotState.Assigned)
		{
			UnselectCurrentCharacter();
		}
	}

	public void CloseTooltip()
	{
		partyStats.CloseTooltip();
	}

	public void SetTooltipHotkeyPanelActive(bool value)
	{
		_tooltipHotkeysPanel.SetActive(value);
	}
}
