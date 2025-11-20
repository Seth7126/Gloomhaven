#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using GLOOM;
using I2.Loc;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using MapRuleLibrary.State;
using Photon.Bolt;
using SM.Gamepad;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using Script.GUI.SMNavigation.Tabs;
using SharedLibrary.SimpleLog;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class NewPartyCharacterUI : APartyCharacterUI, IControllable, IGoldEventListener, IEscapable, INavigationTab
{
	[Serializable]
	public class PartyCharacterEvent : UnityEvent<NewPartyCharacterUI>
	{
	}

	[SerializeField]
	private UINavigationTabComponent _uiNavigationTabComponent;

	[SerializeField]
	private Image characterPortrait;

	[SerializeField]
	private Image characterPortraitHighlight;

	[SerializeField]
	private Image characterUnselectedImage;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private CanvasGroup slotInteraction;

	[SerializeField]
	private Color selectedColor = new Color(1f, 1f, 1f, 0.9f);

	[SerializeField]
	private Color highlightedColor = new Color(1f, 1f, 1f, 0.2f);

	[SerializeField]
	private Color selectedHighlightedColor = new Color(1f, 1f, 1f, 1f);

	[SerializeField]
	private CanvasGroup buttonsCanvasGroup;

	[SerializeField]
	private CanvasGroup characterSlot;

	[SerializeField]
	private MultiplayerUserState multiplayerInfo;

	[SerializeField]
	private Button multiplayerAssignRoleButton;

	public int SlotIndex;

	[SerializeField]
	private ControllerInputArea controllerArea;

	[SerializeField]
	private List<GameObject> characterInformation;

	[Header("Gold")]
	[SerializeField]
	private GoldCounter individualGoldCounter;

	[SerializeField]
	private GUIAnimator warningGoldAnimator;

	[SerializeField]
	private GameObject individualGoldContainer;

	[Header("Free slot")]
	[SerializeField]
	private Sprite emptySlotSelectedSprite;

	[SerializeField]
	private CanvasGroup emptySlot;

	[SerializeField]
	private ExtendedToggle assignToggle;

	[SerializeField]
	private GameObject assignWarning;

	[SerializeField]
	private GUIAnimator assignWarningHighlight;

	private Image assignIcon;

	[SerializeField]
	private GameObject newAssignNotification;

	[Header("Level")]
	[SerializeField]
	private TextMeshProUGUI levelText;

	[SerializeField]
	private GameObject xpPanel;

	[SerializeField]
	private ImageProgressBar xpBar;

	[Header("Class")]
	[SerializeField]
	private ExtendedToggle classToggle;

	[SerializeField]
	private GameObject classContainer;

	[SerializeField]
	private Image classIcon;

	[SerializeField]
	private GameObject classWarning;

	[SerializeField]
	private GUIAnimator classWarningHighlight;

	[SerializeField]
	private GameObject newClassNotification;

	[Header("Cards")]
	[SerializeField]
	private ExtendedToggle cardsToggle;

	[SerializeField]
	private GameObject cardsWarning;

	[SerializeField]
	private GUIAnimator cardsWarningHighlight;

	private Image cardsIcon;

	[Header("Perks")]
	[SerializeField]
	private ExtendedToggle perksToggle;

	[SerializeField]
	private GameObject perkPointsHighlight;

	[SerializeField]
	private GameObject perkPointsNotification;

	[SerializeField]
	private GUIAnimator perksWarningHighlight;

	private Image perkIcon;

	[Header("Item modifiers")]
	[SerializeField]
	private ExtendedToggle itemsToggle;

	[SerializeField]
	private UINavigationTabElement _itemTabComponent;

	[SerializeField]
	private GUIAnimator itemModifiersHighlight;

	[SerializeField]
	private GameObject itemModifiersNotification;

	[SerializeField]
	private UINewNotificationTip newItemsNotification;

	private Image itemsIcon;

	[Header("Battle goal")]
	[SerializeField]
	private ExtendedToggle battleGoalToggle;

	[SerializeField]
	private GameObject battleGoalContainer;

	[SerializeField]
	private GameObject battleGoalWarning;

	[SerializeField]
	private GUIAnimator battleGoalWarningHighlight;

	private Image battleGoalIcon;

	[SerializeField]
	private UITextTooltipTarget battleGoalTooltip;

	[Header("Mode custom stats")]
	[SerializeField]
	private UICampaignPartyCharacterStats campaignStats;

	[SerializeField]
	private UIGuildmasterPartyCharacterStats guildmasterStats;

	[Header("Navigation Tab")]
	[SerializeField]
	private TabComponentInputListener _tabInput;

	[SerializeField]
	private UINavigationTabComponent _tab;

	[SerializeField]
	private Hotkey _nextOptionHotkey;

	[SerializeField]
	private GameObject _optionSeparator;

	[SerializeField]
	private bool m_IsAllowedToEscapeDuringSave;

	private UICustomPartyCharacterStats customStats;

	private PartySlotState state;

	private Action<NewPartyCharacterUI, bool> onItemsClickAction;

	private Action<NewPartyCharacterUI, bool> onCardsClickAction;

	private Action<NewPartyCharacterUI, bool> onPerksClickAction;

	private Action<bool, NewPartyCharacterUI> onCharacterSelect;

	private Action<NewPartyCharacterUI, bool, RectTransform> onClickAssignAction;

	private Action<NewPartyCharacterUI> onClickLevelupAction;

	private Action<NewPartyCharacterUI, bool> onBattleGoalClickAction;

	private CMapCharacter characterData;

	private CMapCharacterService service;

	private bool initialised;

	private bool autoOpenDefaultPanel = true;

	public PartyCharacterEvent OnClickedAssignRole;

	private bool isSelected;

	private bool isHighlighted;

	private bool isDimmed;

	private bool isOtherHovered;

	private bool disabledCharacterSelection;

	public RectTransform ClassPointReference => classToggle.transform as RectTransform;

	public RectTransform CardPointReference => cardsToggle.transform as RectTransform;

	public bool CardWindowSelected => cardsToggle.isOn;

	public RectTransform PerksPointReference => perksToggle.transform as RectTransform;

	public RectTransform ItemPointReference => itemsToggle.transform as RectTransform;

	public bool IsAllowedToEscapeDuringSave => m_IsAllowedToEscapeDuringSave;

	public PartySlotState State => state;

	public CMapCharacter Data => characterData;

	public override ICharacterService Service => service;

	public UnityEvent OnHovered => button.onMouseEnter;

	public UnityEvent OnUnhovered => button.onMouseExit;

	public int InternalTabIndex => _tab.CurrentIndex.GetValueOrDefault();

	public bool IsHidden { get; private set; }

	public bool IsInteractable
	{
		get
		{
			if (button.IsInteractable())
			{
				return AreButtonsInteractable;
			}
			return false;
		}
	}

	private bool AreButtonsInteractable
	{
		get
		{
			if (slotInteraction.interactable)
			{
				return buttonsCanvasGroup.interactable;
			}
			return false;
		}
	}

	public bool IsParticipant => true;

	public bool IsAlive => true;

	public NetworkPlayer Player { get; private set; }

	public event Action<int, int> OnTabIndexUpdated;

	private void Awake()
	{
		perksToggle.onValueChanged.AddListener(OnClickPerks);
		itemsToggle.onValueChanged.AddListener(OnClickItem);
		cardsToggle.onValueChanged.AddListener(OnClickCards);
		assignToggle.onValueChanged.AddListener(OnClickAssign);
		classToggle.onValueChanged.AddListener(OnClickAssign);
		battleGoalToggle.onValueChanged.AddListener(OnClickBattleGoal);
		button.onClick.AddListener(delegate
		{
			if (!InputManager.GamePadInUse || !isSelected)
			{
				OnClick();
			}
		});
		perkIcon = perksToggle.GetComponent<Image>();
		itemsIcon = itemsToggle.GetComponent<Image>();
		assignIcon = assignToggle.GetComponent<Image>();
		cardsIcon = cardsToggle.GetComponent<Image>();
		battleGoalIcon = battleGoalToggle.GetComponent<Image>();
		initialised = false;
		multiplayerAssignRoleButton.onClick.AddListener(OnClickAssignRole);
		if (AdventureState.MapState.IsCampaign)
		{
			customStats = campaignStats;
			guildmasterStats.Hide();
		}
		else
		{
			customStats = guildmasterStats;
			guildmasterStats.OnClickLevelUp.AddListener(OnClickLevelup);
			campaignStats.Hide();
		}
		customStats.Show();
		if (InputManager.GamePadInUse)
		{
			DisplayHotkey(state: false);
			_tabInput.SetTabChangeConditions(TabChangeCondition, TabChangeCondition);
		}
		else
		{
			if (_optionSeparator != null)
			{
				_optionSeparator.SetActive(value: false);
			}
			_nextOptionHotkey.gameObject.SetActive(value: false);
		}
		if (InputManager.GamePadInUse)
		{
			_uiNavigationTabComponent.Initialize();
		}
	}

	private void OnEquippedItemsChanged(List<CItem> item, CMapCharacter character)
	{
		OnEquippedItemsChanged(character);
	}

	private void OnEquippedItemsChanged(CItem item, CMapCharacter character)
	{
		OnEquippedItemsChanged(character);
	}

	private void OnEquippedItemsChanged(CMapCharacter character)
	{
		if (characterData != null && character.CharacterID == characterData.CharacterID)
		{
			RefreshEquippedItemModifiers();
		}
	}

	private bool TabChangeCondition()
	{
		if (!Singleton<ESCMenu>.Instance.IsOpen)
		{
			return !Singleton<UINavigation>.Instance.StateMachine.IsCurrentState<MerchantState>();
		}
		return false;
	}

	private void OnDestroy()
	{
		perksToggle.onValueChanged.RemoveAllListeners();
		itemsToggle.onValueChanged.RemoveAllListeners();
		cardsToggle.onValueChanged.RemoveAllListeners();
		assignToggle.onValueChanged.RemoveAllListeners();
		classToggle.onValueChanged.RemoveAllListeners();
		if (Singleton<MapChoreographer>.Instance != null)
		{
			Singleton<MapChoreographer>.Instance.EventBuss.UnregisterToOnCharacterItemUnequipped(OnEquippedItemsChanged);
			Singleton<MapChoreographer>.Instance.EventBuss.UnregisterToOnCharacterItemEquipped(OnEquippedItemsChanged);
		}
		battleGoalToggle.onValueChanged.RemoveAllListeners();
		button.onClick.RemoveAllListeners();
		multiplayerAssignRoleButton.onClick.RemoveAllListeners();
		guildmasterStats.OnClickLevelUp.RemoveAllListeners();
		OnClickedAssignRole.RemoveAllListeners();
		SetHotkeysActive(value: false);
	}

	public void Init(CMapParty party, int slot, CMapCharacter characterData, Action<NewPartyCharacterUI, bool> onCardsClickAction, Action<NewPartyCharacterUI, bool> onItemsClickAction, Action<bool, NewPartyCharacterUI> onCharacterSelect, Action<NewPartyCharacterUI, bool> onPerksClickAction, ToggleGroup toggleGroup, int newItemsNotification, Action<NewPartyCharacterUI, bool, RectTransform> onClickSwitchCharacter = null, Action<NewPartyCharacterUI> onClickLevelupAction = null, NetworkPlayer initialController = null, Action<NewPartyCharacterUI, bool> onBattleGoalClickAction = null, bool updateController = false)
	{
		Debug.Log("Initializing PartyCharacterUI");
		ClearEvents();
		if (party != null)
		{
			party.SelectedCharactersArray[slot] = characterData;
		}
		bool forceUpdate = updateController && this.characterData != characterData;
		if (this.characterData != characterData)
		{
			CloseCharacterWindows();
		}
		this.characterData = characterData;
		service = new CMapCharacterService(characterData);
		this.onCharacterSelect = onCharacterSelect;
		this.onCardsClickAction = onCardsClickAction;
		this.onItemsClickAction = onItemsClickAction;
		this.onPerksClickAction = onPerksClickAction;
		onClickAssignAction = onClickSwitchCharacter;
		this.onClickLevelupAction = onClickLevelupAction;
		this.onBattleGoalClickAction = onBattleGoalClickAction;
		CharacterYMLData characterYMLData = characterData.CharacterYMLData;
		if (FFSNetwork.IsClient)
		{
			characterData.DisplayCharacterName = (FFSNetwork.IsUGCEnabled ? characterData.CharacterName : GLOOM.LocalizationManager.GetTranslation(characterData.CharacterYMLData.LocKey));
		}
		else
		{
			characterData.DisplayCharacterName = characterData.CharacterName;
		}
		if (characterData.DisplayCharacterName.IsNullOrEmpty())
		{
			nameText.text = GLOOM.LocalizationManager.GetTranslation(characterYMLData.LocKey);
		}
		else
		{
			nameText.SetTextCensored(characterData.DisplayCharacterName);
		}
		characterPortrait.sprite = UIInfoTools.Instance.GetNewAdventureCharacterPortrait(characterYMLData.Model, highlighted: false, characterData.CharacterYMLData.CustomCharacterConfig);
		characterPortraitHighlight.sprite = UIInfoTools.Instance.GetNewAdventureCharacterPortrait(characterYMLData.Model, highlighted: true, characterData.CharacterYMLData.CustomCharacterConfig);
		if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold)
		{
			individualGoldContainer.SetActive(value: true);
			individualGoldCounter.SetCount(characterData.CharacterGold);
			service.AddCallbackOnGoldChanged(this);
		}
		else
		{
			individualGoldContainer.SetActive(value: false);
		}
		service.AddCallbackOnPerkPointsChanged(UpdatePerkPoints);
		Singleton<MapChoreographer>.Instance.EventBuss.RegisterToOnCharacterItemEquipped(OnEquippedItemsChanged);
		Singleton<MapChoreographer>.Instance.EventBuss.RegisterToOnCharacterItemUnequipped(OnEquippedItemsChanged);
		customStats?.Setup(service);
		cardsToggle.group = toggleGroup;
		perksToggle.group = toggleGroup;
		itemsToggle.group = toggleGroup;
		cardsToggle.group = toggleGroup;
		battleGoalToggle.group = toggleGroup;
		assignToggle.group = toggleGroup;
		classToggle.group = toggleGroup;
		initialised = true;
		IsHidden = false;
		UpdateXPTexts();
		UpdatePerkPoints();
		RefreshEquippedItemModifiers();
		CreateControllable(initialController, updateController, updateController, forceUpdate);
		ShowCanvasGroup(emptySlot, show: false);
		ShowCanvasGroup(characterSlot, show: true);
		state = PartySlotState.Assigned;
		EnableSelection();
		ShowNewItemsNotification(newItemsNotification > 0);
		ClearState();
		RefreshMultiplayerState();
		ShowNewClassNotification(party != null && party.NewUnlockedCharacterIDs.Count > 0);
	}

	public void CreateControllable(NetworkPlayer initialController, bool overrideBenched, bool updateIfExists = false, bool forceUpdate = false)
	{
		if ((SaveData.Instance.Global.GameMode == EGameMode.Guildmaster && AdventureState.MapState.HeadquartersState.MultiplayerUnlocked) || SaveData.Instance.Global.GameMode == EGameMode.Campaign)
		{
			NetworkControllable networkControllable = null;
			networkControllable = ((SaveData.Instance.Global.GameMode != EGameMode.Campaign) ? ControllableRegistry.GetControllable(CharacterClassManager.GetModelInstanceIDFromCharacterID(characterData.CharacterID)) : ControllableRegistry.GetControllable(characterData.CharacterName.GetHashCode()));
			if (networkControllable == null || forceUpdate || (updateIfExists && networkControllable.Controller != initialController) || (overrideBenched && NewPartyDisplayUI.PartyDisplay.CharacterAlreadyBenched(characterData)) || (networkControllable.ControllableObject != null && networkControllable.ControllableObject is CharacterManager))
			{
				if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
				{
					ControllableRegistry.CreateControllable(characterData.CharacterName.GetHashCode(), this, initialController, changeObjectIfControllableExists: true, releaseOldControllableObject: false);
				}
				else
				{
					ControllableRegistry.CreateControllable(CharacterClassManager.GetModelInstanceIDFromCharacterID(characterData.CharacterID), this, initialController, changeObjectIfControllableExists: true, releaseOldControllableObject: false);
				}
			}
		}
		OnControlAssigned(initialController);
	}

	private void ClearState()
	{
		HideAssignWarning();
		HideCardsWarning();
		HideClassWarning();
		HideBattleGoalWarning();
		ShowPerksWarning(show: false);
		DisableCardButton(disable: false);
		ShowWarningGold(show: false);
		ShowNewClassNotification(show: false);
	}

	private void ShowCanvasGroup(CanvasGroup canvasGroup, bool show)
	{
		canvasGroup.alpha = (show ? 1 : 0);
		bool interactable = (canvasGroup.blocksRaycasts = show);
		canvasGroup.interactable = interactable;
	}

	public void InitEmpty(CMapParty party, int slot)
	{
		ClearEvents();
		if (party != null)
		{
			party.SelectedCharactersArray[slot] = null;
		}
		ShowCanvasGroup(characterSlot, show: false);
		ShowCanvasGroup(emptySlot, show: false);
		state = PartySlotState.Empty;
		characterData = null;
		service = null;
		DisableSelection();
		customStats?.Clear();
		initialised = true;
		HideNewItemsNotification();
		ClearState();
	}

	public void InitAvailable(CMapParty party, int slot, ToggleGroup toggleGroup, Action<bool, NewPartyCharacterUI> onCharacterSelect, Action<NewPartyCharacterUI, bool, RectTransform> onClickSwitchCharacter = null)
	{
		if (characterData != null)
		{
			int controllableID = (AdventureState.MapState.IsCampaign ? characterData.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(characterData.CharacterID));
			NetworkControllable networkControllable = ControllableRegistry.AllControllables.SingleOrDefault((NetworkControllable e) => e.ID == controllableID);
			if (networkControllable != null && networkControllable.ControllableObject is NewPartyCharacterUI && !NewPartyDisplayUI.PartyDisplay.CharacterAlreadyBenched(characterData))
			{
				NewPartyDisplayUI.PartyDisplay.AddBenchedCharacter(characterData);
			}
		}
		ClearEvents();
		if (party != null)
		{
			party.SelectedCharactersArray[slot] = null;
		}
		assignToggle.group = toggleGroup;
		cardsToggle.group = toggleGroup;
		perksToggle.group = toggleGroup;
		itemsToggle.group = toggleGroup;
		battleGoalToggle.group = toggleGroup;
		cardsToggle.group = toggleGroup;
		classToggle.group = toggleGroup;
		DetermineInteractability();
		ShowCanvasGroup(characterSlot, show: false);
		ShowCanvasGroup(emptySlot, show: true);
		characterData = null;
		service = null;
		characterPortraitHighlight.sprite = emptySlotSelectedSprite;
		this.onCharacterSelect = onCharacterSelect;
		onClickAssignAction = onClickSwitchCharacter;
		Singleton<MapChoreographer>.Instance.EventBuss.UnregisterToOnCharacterItemUnequipped(OnEquippedItemsChanged);
		Singleton<MapChoreographer>.Instance.EventBuss.UnregisterToOnCharacterItemEquipped(OnEquippedItemsChanged);
		customStats?.Clear();
		state = PartySlotState.Available;
		EnableSelection();
		initialised = true;
		HideNewItemsNotification();
		ClearState();
		RefreshMultiplayerState();
		ShowNewClassNotification(party != null && party.NewUnlockedCharacterIDs.Count > 0);
	}

	private void OnEnable()
	{
		I2.Loc.LocalizationManager.OnLocalizeEvent += OnLanguageChanged;
	}

	private void OnDisable()
	{
		ClearEvents();
		initialised = false;
		I2.Loc.LocalizationManager.OnLocalizeEvent -= OnLanguageChanged;
	}

	private void ClearEvents()
	{
		if (service != null)
		{
			service.RemoveCallbackOnPerkPointsChanged(UpdatePerkPoints);
			service.RemoveCallbackOnGoldChanged(this);
		}
	}

	public void OnUpdatedGold(int gold)
	{
		if (individualGoldContainer.activeSelf)
		{
			individualGoldCounter.CountTo(gold);
		}
	}

	public void OnXpChanged()
	{
		UpdateXPTexts(playProgressAnimation: true);
	}

	public void UpdateXPTexts(bool playProgressAnimation = false)
	{
		if (!initialised)
		{
			return;
		}
		levelText.text = string.Format("{0} {1}", GLOOM.LocalizationManager.GetTranslation("GUI_LEVEL"), Data.Level);
		if (Data.Level + 1 >= Data.XPTable.Count)
		{
			xpPanel.SetActive(value: false);
			return;
		}
		int num = Data.EXP - Data.GetXPThreshold(Data.Level);
		int num2 = Data.GetXPThreshold(Data.Level + 1) - Data.GetXPThreshold(Data.Level);
		xpPanel.SetActive(value: true);
		if (playProgressAnimation)
		{
			xpBar.PlayProgressTo(num, num2);
		}
		else
		{
			xpBar.SetAmount(num, num2);
		}
	}

	public void UpdatePerkPoints()
	{
		if (initialised)
		{
			perkPointsHighlight.SetActive(characterData.PerkPoints > 0);
			perkPointsNotification.SetActive(characterData.PerkPoints > 0);
		}
	}

	public void UpdateDisplay()
	{
	}

	public void OnClickItem(bool isSelected)
	{
		if (isSelected)
		{
			OnTabClicked(selected: true);
		}
		onItemsClickAction(this, isSelected);
	}

	private void OnClickAssign(bool isSelected)
	{
		if (buttonsCanvasGroup.interactable)
		{
			if (isSelected)
			{
				OnTabClicked(selected: true);
			}
			if (!InputManager.GamePadInUse || !Singleton<MapFTUEManager>.IsInitialized || Singleton<MapFTUEManager>.Instance.CurrentStep != EMapFTUEStep.SelectSecondSlot || SlotIndex == 1)
			{
				onClickAssignAction(this, isSelected, assignToggle.transform as RectTransform);
			}
		}
	}

	private void OnClickBattleGoal(bool isSelected)
	{
		if (buttonsCanvasGroup.interactable)
		{
			if (isSelected)
			{
				OnTabClicked(selected: true);
			}
			onBattleGoalClickAction(this, isSelected);
		}
	}

	private void OnClickAssignRole()
	{
		if (buttonsCanvasGroup.interactable || !multiplayerAssignRoleButton.interactable)
		{
			cardsToggle.group.SetAllTogglesOff();
			OnTabClicked(selected: true);
			OnClickedAssignRole.Invoke(this);
		}
	}

	public void ShowClassToggled()
	{
		assignToggle.SetValue(value: false);
		classToggle.SetValue(value: true);
	}

	public void ShowAssignToggled()
	{
		classToggle.SetValue(value: false);
		assignToggle.SetValue(value: true);
	}

	public void ActivateTabsAndSetElement(int index)
	{
		_tabInput.Register();
		_tab.Initialize();
		_tab.SetCurrentElement(index);
		SetHotkeysActive(value: true);
	}

	public void CloseAssigWindow()
	{
		if (classToggle.isOn || assignToggle.isOn)
		{
			bool num = InputManager.GamePadInUse && (classToggle.isOn || assignToggle.isOn);
			classToggle.isOn = false;
			assignToggle.isOn = false;
			if (num)
			{
				Escape();
			}
		}
	}

	public void OnClickCards(bool isSelected)
	{
		if (buttonsCanvasGroup.interactable)
		{
			if (isSelected)
			{
				OnTabClicked(selected: true);
			}
			onCardsClickAction(this, isSelected);
		}
	}

	public void OnClickPerks(bool isSelected)
	{
		if (isSelected)
		{
			OnTabClicked(selected: true);
		}
		onPerksClickAction(this, isSelected);
	}

	private void OnClickLevelup()
	{
		CoroutineHelper.RunNextFrame(delegate
		{
			if (!Singleton<UILoadoutManager>.Instance.InLoadout())
			{
				cardsToggle.group.SetAllTogglesOff();
				OnTabClicked(selected: true);
				onClickLevelupAction?.Invoke(this);
			}
		});
	}

	public void ToggleDim(bool active)
	{
		isDimmed = active;
		UpdateSprite();
	}

	public void ToggleHoverDim(bool active)
	{
		isOtherHovered = active;
		UpdateSprite();
	}

	public void OnClick()
	{
		if ((!InputManager.GamePadInUse && Singleton<MapFTUEManager>.IsInitialized && Singleton<MapFTUEManager>.Instance.CurrentStep == EMapFTUEStep.VisitMerchant) || !button.interactable || (FFSNetwork.IsOnline && PlayerRegistry.IsSwitchingCharacter))
		{
			return;
		}
		bool flag = !isSelected;
		if (characterData != null)
		{
			ToggleSelect(flag);
			InvokeOnCharacterSelected(isSelected);
			if (!isSelected)
			{
				return;
			}
			if (autoOpenDefaultPanel && AreButtonsInteractable && !InputManager.GamePadInUse)
			{
				if (classToggle.gameObject.activeSelf && classToggle.interactable)
				{
					classToggle.isOn = true;
				}
				else if (battleGoalToggle.gameObject.activeSelf && battleGoalToggle.IsInteractable())
				{
					battleGoalToggle.isOn = true;
				}
				else
				{
					classToggle.group.SetAllTogglesOff();
				}
			}
			else
			{
				classToggle.group.SetAllTogglesOff();
			}
		}
		else if (flag)
		{
			if (assignToggle.gameObject.activeSelf)
			{
				if (assignToggle.interactable && AreButtonsInteractable)
				{
					ToggleSelect(active: true);
					InvokeOnCharacterSelected(isSelected);
					assignToggle.isOn = true;
				}
				else
				{
					assignToggle.group.SetAllTogglesOff();
				}
			}
			else
			{
				AudioControllerUtils.PlaySound(UIInfoTools.Instance.InvalidOptionAudioItem);
			}
		}
		else
		{
			ToggleSelect(active: false);
			InvokeOnCharacterSelected(isSelected);
		}
	}

	public void EnableAutoOpenDefaultPanel(bool isEnabled)
	{
		autoOpenDefaultPanel = isEnabled;
	}

	public void ToggleSelect(bool active)
	{
		isSelected = active;
		if (isSelected)
		{
			ToggleDim(active: false);
		}
		UpdateSprite();
	}

	public void OnHighlight(bool active)
	{
		isHighlighted = active;
		UpdateSprite();
	}

	private void UpdateSprite()
	{
		characterUnselectedImage.enabled = !isHighlighted && (isDimmed || (isSelected && isOtherHovered));
		characterUnselectedImage.color = (isSelected ? highlightedColor : UIInfoTools.Instance.White);
		characterPortraitHighlight.color = ((!isHighlighted) ? (isSelected ? selectedColor : new Color(1f, 1f, 1f, 0f)) : (isSelected ? selectedHighlightedColor : highlightedColor));
	}

	public void LevelUp(Action onLevelUp = null)
	{
		if (service.CanLevelup())
		{
			bool flag = !FFSNetwork.IsOnline || Data.IsUnderMyControl;
			service.LevelUp();
			Singleton<APartyDisplayUI>.Instance?.UpdatePartyLevel();
			UpdateXPTexts();
			if (flag)
			{
				onLevelUp?.Invoke();
			}
			customStats.RefreshLevelUp();
			if (FFSNetwork.IsOnline && Data.IsUnderMyControl)
			{
				int controllableID = (AdventureState.MapState.IsCampaign ? Data.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(Data.CharacterID));
				ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
				IProtocolToken supplementaryDataToken = new CardInventoryToken(Data.HandAbilityCardIDs, Data.OwnedAbilityCardIDs);
				IProtocolToken supplementaryDataToken2 = new LevelToken(Data.Level);
				IProtocolToken supplementaryDataToken3 = new PerkPointsToken(Data.PerkPoints);
				Synchronizer.ReplicateControllableStateChange(GameActionType.LevelUp, currentPhase, controllableID, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken, supplementaryDataToken2, supplementaryDataToken3);
			}
			if (Singleton<MapChoreographer>.Instance != null && flag)
			{
				Singleton<MapChoreographer>.Instance.RequestRegenerateAllMapScenarios();
			}
			SimpleLog.AddToSimpleLog("MapCharacter: " + characterData.CharacterID + " levelled up");
		}
		else
		{
			customStats.RefreshLevelUp();
		}
	}

	public void ShowNewItemsNotification(bool show)
	{
		if (show)
		{
			newItemsNotification.Show();
		}
		else
		{
			HideNewItemsNotification();
		}
	}

	public void HideNewItemsNotification()
	{
		newItemsNotification.Hide();
	}

	public void ShowNewClassNotification(bool show)
	{
		bool active = show && !InputManager.GamePadInUse;
		newClassNotification.SetActive(active);
		newAssignNotification.SetActive(active);
	}

	public void CloseWindows()
	{
		Debug.Log("CloseWindows");
		cardsToggle.isOn = false;
		perksToggle.isOn = false;
		assignToggle.isOn = false;
		classToggle.isOn = false;
		itemsToggle.isOn = false;
		battleGoalToggle.isOn = false;
	}

	private void CloseCharacterWindows()
	{
		cardsToggle.isOn = false;
		perksToggle.isOn = false;
		itemsToggle.isOn = false;
		battleGoalToggle.isOn = false;
	}

	public void EnableSelection()
	{
		button.interactable = true;
	}

	public void DisableSelection()
	{
		button.interactable = false;
	}

	public void EnableAssignPlayer(bool enable)
	{
		multiplayerAssignRoleButton.interactable = enable;
	}

	public void EnableSelectCharacter(bool enable)
	{
		disabledCharacterSelection = !enable;
		DetermineInteractability();
	}

	public void DisableButtons(bool ignoreCardButton)
	{
		buttonsCanvasGroup.interactable = false;
		buttonsCanvasGroup.blocksRaycasts = false;
		RefreshButtonsState();
		if (ignoreCardButton)
		{
			cardsIcon.material = (cardsToggle.interactable ? null : UIInfoTools.Instance.greyedOutMaterial);
		}
	}

	private void DisableCardButton(bool disable)
	{
		cardsToggle.interactable = !disable;
		cardsIcon.material = ((cardsToggle.interactable && AreButtonsInteractable) ? null : UIInfoTools.Instance.greyedOutMaterial);
	}

	public void EnableButtons(bool ignoreCardButton)
	{
		buttonsCanvasGroup.interactable = true;
		buttonsCanvasGroup.blocksRaycasts = true;
		if (!ignoreCardButton)
		{
			DisableCardButton(disable: false);
		}
		RefreshButtonsState();
	}

	private void RefreshStateClassToggle()
	{
		classIcon.material = ((classToggle.interactable && AreButtonsInteractable) ? null : UIInfoTools.Instance.greyedOutMaterial);
		assignIcon.material = ((assignToggle.interactable && AreButtonsInteractable) ? null : UIInfoTools.Instance.greyedOutMaterial);
	}

	private void RefreshButtonsState()
	{
		RefreshStateClassToggle();
		itemsIcon.material = ((itemsToggle.interactable && AreButtonsInteractable) ? null : UIInfoTools.Instance.greyedOutMaterial);
		perkIcon.material = ((perksToggle.interactable && AreButtonsInteractable) ? null : UIInfoTools.Instance.greyedOutMaterial);
		cardsIcon.material = ((cardsToggle.interactable && AreButtonsInteractable) ? null : UIInfoTools.Instance.greyedOutMaterial);
		battleGoalIcon.material = ((battleGoalToggle.interactable && AreButtonsInteractable) ? null : UIInfoTools.Instance.greyedOutMaterial);
	}

	public void AddEquippedItemModifiers(CItem item)
	{
		if (item.YMLData.Data.AdditionalModifiers.Count != 0)
		{
			itemModifiersHighlight.Stop();
			itemModifiersHighlight.Play();
			RefreshEquippedItemModifiers();
		}
	}

	public void RefreshEquippedItemModifiers()
	{
		int num = characterData.NewEquippedItemsWithModifiers?.Count ?? 0;
		itemModifiersNotification.SetActive(num > 0);
	}

	public void ToggleCardsPanel(bool toggledOn)
	{
		cardsToggle.isOn = toggledOn;
	}

	public void ToggleEquipmentPanel(bool toggledOn)
	{
		if (InputManager.GamePadInUse)
		{
			_tab.Select(_itemTabComponent);
		}
		else
		{
			itemsToggle.isOn = toggledOn;
		}
	}

	public void ToggleBatteGoal(bool toggledOn)
	{
		if (battleGoalToggle.interactable || !toggledOn)
		{
			battleGoalToggle.isOn = toggledOn;
		}
	}

	public void ShowAssignWarning(bool glow)
	{
		if (glow)
		{
			assignWarningHighlight.Play();
		}
		else
		{
			assignWarning.SetActive(value: true);
		}
	}

	public void HideAssignWarning()
	{
		assignWarning.SetActive(value: false);
		assignWarningHighlight.Stop();
	}

	public void ShowClassWarning(bool glow)
	{
		if (glow)
		{
			classWarningHighlight.Play();
		}
		else
		{
			classWarning.SetActive(value: true);
		}
	}

	public void HideClassWarning()
	{
		classWarning.SetActive(value: false);
		classWarningHighlight.Stop();
	}

	public void ShowCardsWarning(bool glow)
	{
		cardsWarning.SetActive(value: true);
		if (glow)
		{
			cardsWarningHighlight.Play();
		}
	}

	public void HideCardsWarning()
	{
		cardsWarning.SetActive(value: false);
		cardsWarningHighlight.Stop();
	}

	public void ShowPerksWarning(bool show)
	{
		if (show)
		{
			perksWarningHighlight.Play();
		}
		else
		{
			perksWarningHighlight.Stop(goToEnd: true);
		}
	}

	private void DetermineInteractability(bool updateOpenWindows = true)
	{
		NetworkControllable networkControllable = null;
		if (characterData != null)
		{
			networkControllable = ControllableRegistry.GetControllable(AdventureState.MapState.IsCampaign ? characterData.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(characterData.CharacterID));
		}
		slotInteraction.interactable = !FFSNetwork.IsOnline || characterData == null || networkControllable != null;
		if (assignToggle != null && classToggle != null)
		{
			assignToggle.interactable = !disabledCharacterSelection && (!FFSNetwork.IsOnline || (PlayerRegistry.MyPlayer != null && PlayerRegistry.MyPlayer.AssignedSlots.Contains(SlotIndex)));
			classToggle.interactable = !disabledCharacterSelection && (!FFSNetwork.IsOnline || characterData == null || characterData.IsUnderMyControl);
			if (!PlayerRegistry.IsSwitchingCharacter && !UIAdventurePartyAssemblyWindow.IsChangingCharacter && (!(PlayerRegistry.MyPlayer != null) || !PlayerRegistry.MyPlayer.IsCreatingCharacter))
			{
				if (assignToggle.isOn && updateOpenWindows && (!assignToggle.interactable || !slotInteraction.interactable))
				{
					CloseAssigWindow();
				}
				if (classToggle.isOn && updateOpenWindows && (!classToggle.interactable || !slotInteraction.interactable))
				{
					CloseAssigWindow();
				}
			}
		}
		battleGoalToggle.interactable = !FFSNetwork.IsOnline || characterData == null || characterData.IsUnderMyControl;
		multiplayerAssignRoleButton.interactable = FFSNetwork.IsHost && PlayerRegistry.AllPlayers.All((NetworkPlayer a) => !a.IsCreatingCharacter);
		RefreshButtonsState();
	}

	public void RefreshMultiplayerState(bool updateOpenWindows = true)
	{
		DetermineInteractability(updateOpenWindows);
		if (State == PartySlotState.Empty)
		{
			return;
		}
		if (FFSNetwork.IsOnline)
		{
			NetworkPlayer networkPlayer = PlayerRegistry.AllPlayers.SingleOrDefault((NetworkPlayer s) => s.AssignedSlots.Contains(SlotIndex));
			if (networkPlayer == null)
			{
				networkPlayer = PlayerRegistry.HostPlayer;
			}
			RefreshMultiplayerOwner(networkPlayer);
		}
		else
		{
			Player = null;
			multiplayerAssignRoleButton.gameObject.SetActive(value: false);
			multiplayerInfo.Hide();
		}
	}

	private void RefreshMultiplayerOwner(NetworkPlayer player)
	{
		if (State != PartySlotState.Empty)
		{
			Player = player;
			multiplayerInfo.Show(player, !PlayerRegistry.IsJoining(player));
			multiplayerAssignRoleButton.gameObject.SetActive(value: true);
		}
	}

	public void OnControlAssigned(NetworkPlayer player)
	{
		if (player == null || PlayerRegistry.MyPlayer == null)
		{
			return;
		}
		try
		{
			bool flag = true;
			Debug.LogGUI("OnControlAssigned " + characterData?.CharacterID + " " + PlayerRegistry.MyPlayer.PlayerID);
			if (characterData != null && PlayerRegistry.MyPlayer.PlayerID == player.PlayerID)
			{
				NetworkControllable networkControllable = ((ControllableRegistry.AllControllables.Where((NetworkControllable w) => w.GetCharacter == characterData.CharacterID).Count() > 1) ? null : ControllableRegistry.AllControllables.SingleOrDefault((NetworkControllable s) => s.GetCharacter == characterData.CharacterID));
				Debug.LogGUI("Is benched " + (networkControllable != null && networkControllable.ControllableObject is BenchedCharacter));
				if (networkControllable != null && networkControllable.ControllableObject is BenchedCharacter)
				{
					flag = false;
				}
				else if (!characterData.IsUnderMyControl)
				{
					FFSNet.Console.LogInfo(characterData.CharacterID + "(MAP) is now under my control.");
					characterData.IsUnderMyControl = true;
				}
			}
			if (flag)
			{
				RefreshMultiplayerOwner(player);
				CloseCharacterWindows();
			}
			DetermineInteractability();
			EnableSelection();
		}
		catch
		{
		}
	}

	public void OnControlReleased()
	{
		if (!(PlayerRegistry.MyPlayer == null))
		{
			if (characterData != null && characterData.IsUnderMyControl)
			{
				Debug.LogGUI("Relese " + characterData?.CharacterID);
				characterData.IsUnderMyControl = false;
				FFSNet.Console.LogInfo(characterData.CharacterID + "(MAP) is no longer under my control.");
			}
			if (SaveData.Instance.Global.CurrentGameState == EGameState.Map)
			{
				RefreshMultiplayerState(updateOpenWindows: false);
			}
		}
	}

	public PrefabId GetNetworkEntityPrefabID()
	{
		return BoltPrefabs.GHControllableState;
	}

	public string GetName()
	{
		if (characterData == null)
		{
			return "UNINITIALIZED";
		}
		return characterData.CharacterID;
	}

	public void DisableUIForLevelingUp()
	{
		customStats.DisableUIForLevelingUp();
	}

	public void EnableBattleGoalsSelection(bool enable, bool interactable = true, string tooltip = null)
	{
		if (InputManager.GamePadInUse)
		{
			battleGoalContainer.gameObject.SetActive(enable);
			classContainer.gameObject.SetActive(!enable);
		}
		else
		{
			battleGoalToggle.gameObject.SetActive(enable);
			classToggle.gameObject.SetActive(!enable);
		}
		if (InputManager.GamePadInUse)
		{
			_uiNavigationTabComponent.Initialize();
		}
		battleGoalToggle.interactable = interactable;
		RefreshButtonsState();
		if (tooltip.IsNOTNullOrEmpty())
		{
			battleGoalTooltip.SetText(tooltip);
			battleGoalTooltip.enabled = true;
		}
		else
		{
			battleGoalTooltip.enabled = false;
		}
	}

	public void HideBattleGoalWarning()
	{
		battleGoalWarning.SetActive(value: false);
		battleGoalWarningHighlight.Stop();
	}

	public void ShowBattleGoalWarning(bool glow)
	{
		battleGoalWarning.SetActive(value: true);
		if (glow)
		{
			battleGoalWarningHighlight.Play();
		}
	}

	public void ShowWarningGold(bool show)
	{
		if (show)
		{
			warningGoldAnimator.Play();
		}
		else
		{
			warningGoldAnimator.Stop();
		}
	}

	public void DisableAssign()
	{
		assignToggle.gameObject.SetActive(value: false);
	}

	public void CloseCardsWindow()
	{
		if (!AdventureState.MapState.IsCampaign && !cardsToggle.gameObject.activeSelf)
		{
			cardsToggle.SetIsOnWithoutNotify(guildmasterStats.levelUpButton.gameObject.activeSelf);
		}
		if (cardsToggle.isOn)
		{
			bool num = InputManager.GamePadInUse && cardsToggle.isOn;
			cardsToggle.SetIsOnWithoutNotify(value: false);
			if (num)
			{
				Escape();
			}
		}
	}

	public void CloseItemsWindow()
	{
		if (itemsToggle.isOn)
		{
			bool num = InputManager.GamePadInUse && itemsToggle.isOn;
			itemsToggle.isOn = false;
			if (num)
			{
				Escape();
			}
		}
	}

	public void ClosePerksWindow()
	{
		if (perksToggle.isOn)
		{
			bool num = InputManager.GamePadInUse && perksToggle.isOn;
			perksToggle.isOn = false;
			if (num)
			{
				Escape();
			}
		}
	}

	public void CloseBattleGoal()
	{
		if (battleGoalToggle.isOn)
		{
			bool num = InputManager.GamePadInUse && battleGoalToggle.isOn;
			battleGoalToggle.isOn = false;
			if (num)
			{
				Escape();
			}
		}
	}

	public void HideAssignedCharacter()
	{
		IsHidden = true;
		characterPortraitHighlight.sprite = emptySlotSelectedSprite;
		characterPortrait.SetAlpha(0f);
		for (int i = 0; i < characterInformation.Count; i++)
		{
			characterInformation[i].SetActive(value: false);
		}
		DisableButtons(ignoreCardButton: false);
		DisableSelection();
		EnableBattleGoalsSelection(enable: false);
	}

	private void OnControllerAreaFocused()
	{
		if (state == PartySlotState.Available)
		{
			controllerArea.Unfocus();
		}
		if (isSelected)
		{
			UIWindowManager.RegisterEscapable(this);
		}
	}

	private void OnControllerAreaUnfocused()
	{
		UIWindowManager.UnregisterEscapable(this);
	}

	public void SetHotkeysActive(bool value)
	{
		if (InputManager.GamePadInUse)
		{
			if (value)
			{
				_nextOptionHotkey.Initialize(Singleton<UINavigation>.Instance.Input);
				DisplayHotkey(state: true);
			}
			else
			{
				_nextOptionHotkey.Deinitialize();
				DisplayHotkey(state: false);
			}
		}
	}

	private void DisplayHotkey(bool state)
	{
		if (_optionSeparator != null)
		{
			_optionSeparator.SetActive(state);
		}
		HandleDisplayNextOptionHotkey(state);
	}

	private void HandleDisplayNextOptionHotkey(bool state)
	{
		bool active = state && Singleton<UIGuildmasterHUD>.Instance.CurrentMode != EGuildmasterMode.Enchantress;
		_nextOptionHotkey.DisplayHotkey(active);
	}

	public bool Escape()
	{
		UIWindowManager.UnregisterEscapable(this);
		OnClick();
		_tabInput.UnRegister();
		_tab.RemoveCurrent(deselectFirst: false);
		ResetTabIndex();
		SetHotkeysActive(value: false);
		return true;
	}

	public int Order()
	{
		return 0;
	}

	public void Select()
	{
		if (InputManager.GamePadInUse && Singleton<UINavigation>.Instance.StateMachine.CurrentState is LoadoutState && characterData == null)
		{
			int count = NewPartyDisplayUI.PartyDisplay.CharacterSlots.Count;
			int? currentCharacterTabIndex = NewPartyDisplayUI.PartyDisplay.CurrentCharacterTabIndex;
			if (!currentCharacterTabIndex.HasValue || currentCharacterTabIndex == -1)
			{
				NewPartyDisplayUI.PartyDisplay.SelectFirstCharacterBattleGoal();
				return;
			}
			for (int i = currentCharacterTabIndex.Value; i < count; i++)
			{
				int num = i + 1;
				if (num >= count)
				{
					break;
				}
				NewPartyCharacterUI newPartyCharacterUI = NewPartyDisplayUI.PartyDisplay.CharacterSlots[num];
				if (newPartyCharacterUI.Data != null)
				{
					string characterID = newPartyCharacterUI.Data.CharacterID;
					NewPartyDisplayUI.PartyDisplay.SelectCharacterById(characterID);
					NewPartyDisplayUI.PartyDisplay.OpenBattleGoalPanel(characterID);
					return;
				}
			}
			if (NewPartyDisplayUI.PartyDisplay.SelectFirstCharacterBattleGoal())
			{
				return;
			}
		}
		if (MapFTUEManager.IsPlaying && Singleton<MapFTUEManager>.Instance.CurrentStep == EMapFTUEStep.SelectSecondSlot && characterData != null)
		{
			NewPartyDisplayUI.PartyDisplay.NextCharacter();
			return;
		}
		if ((MapFTUEManager.IsPlaying && Singleton<UIGuildmasterHUD>.Instance.CurrentMode != EGuildmasterMode.Merchant) || characterData == null)
		{
			ExecuteEvents.Execute(button.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
			return;
		}
		InvokeOnCharacterSelected(selected: true);
		_tabInput.Register();
		_tab.Initialize();
		int safeCharacterInternalTabIndex = GetSafeCharacterInternalTabIndex();
		_tab.Select(safeCharacterInternalTabIndex);
		SetHotkeysActive(value: true);
	}

	public void Deselect()
	{
		_tabInput.UnRegister();
		_tab.RemoveCurrent();
		InvokeOnCharacterSelected(selected: false);
		SetHotkeysActive(value: false);
	}

	private int GetSafeCharacterInternalTabIndex()
	{
		int index = NewPartyDisplayUI.PartyDisplay.CharacterInternalTabIndex;
		int characterLastTabElementsCount = NewPartyDisplayUI.PartyDisplay.CharacterLastTabElementsCount;
		RemapTabIndex(ref index, characterLastTabElementsCount);
		return Math.Clamp(index, 0, _tab.ElementCount - 1);
	}

	private void RemapTabIndex(ref int index, int characterLastTabTotalElements)
	{
		if (characterLastTabTotalElements != 0)
		{
			if (_tab.ElementCount < characterLastTabTotalElements)
			{
				index--;
			}
			else if (_tab.ElementCount > characterLastTabTotalElements)
			{
				index++;
			}
		}
	}

	private void OnTabClicked(bool selected)
	{
		if (!InputManager.GamePadInUse)
		{
			InvokeOnCharacterSelected(selected);
		}
		UpdateTabIndex();
	}

	private void InvokeOnCharacterSelected(bool selected)
	{
		onCharacterSelect?.Invoke(selected, this);
	}

	private void UpdateTabIndex()
	{
		InvokeOnTabIndexUpdated(InternalTabIndex, _tab.ElementCount);
	}

	private void ResetTabIndex()
	{
		InvokeOnTabIndexUpdated(0, _tab.ElementCount);
	}

	private void InvokeOnTabIndexUpdated(int index, int totalElements)
	{
		this.OnTabIndexUpdated?.Invoke(index, totalElements);
	}

	private void OnLanguageChanged()
	{
		if (characterData != null)
		{
			levelText.text = string.Format("{0} {1}", GLOOM.LocalizationManager.GetTranslation("GUI_LEVEL"), Data.Level);
			if (characterData.DisplayCharacterName.IsNullOrEmpty())
			{
				nameText.text = GLOOM.LocalizationManager.GetTranslation(characterData.CharacterYMLData.LocKey);
			}
			else
			{
				nameText.SetTextCensored(characterData.DisplayCharacterName);
			}
		}
	}
}
