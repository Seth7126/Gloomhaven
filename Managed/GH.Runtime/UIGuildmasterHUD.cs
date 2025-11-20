#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using Assets.Script.GUI.NewAdventureMode.Guildmaster;
using Assets.Script.Misc;
using Code.State;
using FFSNet;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.YML.Message;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIGuildmasterHUD : Singleton<UIGuildmasterHUD>, IEscapable
{
	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private TextLocalizedListener hoveredOptionTooltip;

	[SerializeField]
	private GUIAnimator hoverAnimation;

	[SerializeField]
	private UIGuildmasterBanner banner;

	[SerializeField]
	private GameObject bannerContainer;

	[SerializeField]
	private UICityEncounterButton cityEncounterButton;

	[SerializeField]
	private CanvasGroup buttonsCanvasGroup;

	[SerializeField]
	private ControllerInputArea controllerArea;

	[SerializeField]
	private UIGuildmasterConfirmActionPresenter confirmActionPresenter;

	[Header("Options")]
	[SerializeField]
	private GameObject optionsContainer;

	[SerializeField]
	private UIGuildmasterButton enhanceButton;

	[SerializeField]
	private UIGuildmasterButton shopButton;

	[SerializeField]
	private UIGuildmasterButton trainerButton;

	[SerializeField]
	private UIGuildmasterButton mapButton;

	[SerializeField]
	private UIGuildmasterButton templeButton;

	[SerializeField]
	private UIGuildmasterButton cityButton;

	[SerializeField]
	private UIGuildmasterButton townRecordsButton;

	[SerializeField]
	private UIGuildmasterButton mercenaryLogButton;

	[Header("Windows")]
	[SerializeField]
	private UINewEnhancementWindow enhancementWindow;

	[SerializeField]
	private UITrainerWindow trainerWindow;

	[SerializeField]
	private UIShopItemWindow shopWindow;

	[SerializeField]
	private UITempleWindow templeWindow;

	[SerializeField]
	private UITownRecordsWindow townRecordsWindow;

	[SerializeField]
	private bool m_IsAllowedToEscapeDuringSave;

	[SerializeField]
	private List<GuildmasterModeSelectable> _modeSelectableObjects;

	private Dictionary<EGuildmasterMode, GuildmasterMode> modes;

	private EGuildmasterMode currentMode;

	private bool isHovered;

	private HashSet<Component> lockedInteractionRequests = new HashSet<Component>();

	private ToggleGroup toggleGroup;

	private HashSet<Component> disableEditGoldModeRequests = new HashSet<Component>();

	private HashSet<object> hideRequests = new HashSet<object>();

	private HashSet<Component> disableOptionsRequests = new HashSet<Component>();

	private HashSet<object> hideBannerRequests = new HashSet<object>();

	private int _lastModeIndex;

	private SimpleKeyActionHandlerBlocker _simpleKeyActionHandlerBlocker;

	private IModePaginationStrategy _modePaginationStrategy;

	[HideInInspector]
	public EGuildmasterMode CurrentMode => currentMode;

	public UIGuildmasterConfirmActionPresenter ConfirmActionPresenter => confirmActionPresenter;

	public ControllerInputArea TempleInputArea => TempleWindow.TempleInputArea;

	public ControllerInputArea ShopInputArea => shopWindow.ItemInventory.ControllerArea;

	public UITempleWindow TempleWindow => templeWindow;

	public UINewEnhancementWindow EnhancementWindow => enhancementWindow;

	public UIAchievementInventory UIAchievementInventory => trainerWindow.UIAchievementInventory;

	public bool IsNavigationLocked => _simpleKeyActionHandlerBlocker.IsBlock;

	public bool IsAllowedToEscapeDuringSave => m_IsAllowedToEscapeDuringSave;

	public bool AreOptionsAvailable
	{
		get
		{
			if (window.IsOpen)
			{
				return disableOptionsRequests.Count == 0;
			}
			return false;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_modePaginationStrategy = new CycledModePaginationStrategy();
		toggleGroup = optionsContainer.GetComponentInChildren<ToggleGroup>();
		toggleGroup.allowSwitchOff = true;
		modes = new Dictionary<EGuildmasterMode, GuildmasterMode>();
		modes[EGuildmasterMode.Enchantress] = new EnchantressMode(enhanceButton, delegate
		{
			banner.Hide();
			enhancementWindow.EnterShop();
			enhancementWindow.SetEditMode(disableEditGoldModeRequests.Count == 0);
		}, enhancementWindow.ExitShop);
		InitButtonMode(enhanceButton);
		modes[EGuildmasterMode.Merchant] = new MerchantMode(shopButton, delegate
		{
			ResetBannerParent();
			banner.ShowMode(EGuildmasterMode.Merchant);
			shopWindow.EnterShop(AdventureState.MapState.HeadquartersState);
			shopWindow.SetEditMode(disableEditGoldModeRequests.Count == 0);
		}, shopWindow.Exit);
		UIShopItemWindow uIShopItemWindow = shopWindow;
		uIShopItemWindow.OnNewMerchantItemShown = (BasicEventHandler)Delegate.Combine(uIShopItemWindow.OnNewMerchantItemShown, new BasicEventHandler(modes[EGuildmasterMode.Merchant].RefreshNotifications));
		InitButtonMode(shopButton);
		modes[EGuildmasterMode.Trainer] = new TrainerMode(trainerButton, delegate
		{
			banner.Hide();
			Singleton<UIPartyCharacterEquipmentDisplay>.Instance.Hide(playHideAudio: false);
			NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.Hide(playHideAudio: false);
			NewPartyDisplayUI.PartyDisplay.Hide(this, instant: true);
			trainerWindow.Show(OnExitTrainer);
		}, delegate
		{
			NewPartyDisplayUI.PartyDisplay.Show(this);
			trainerWindow.Exit();
		});
		UITrainerWindow uITrainerWindow = trainerWindow;
		uITrainerWindow.OnNewNotificationShown = (BasicEventHandler)Delegate.Combine(uITrainerWindow.OnNewNotificationShown, new BasicEventHandler(modes[EGuildmasterMode.Trainer].RefreshNotifications));
		InitButtonMode(trainerButton);
		modes[EGuildmasterMode.Temple] = new TempleMode(templeButton, delegate
		{
			IGuildmasterBannerConfig config = null;
			if (Singleton<UINavigation>.Instance.StateMachine.GetState(CampaignMapStateTag.Temple) is IGuildmasterBannerConfigContainer guildmasterBannerConfigContainer)
			{
				config = guildmasterBannerConfigContainer.GuildmasterBannerConfig;
			}
			banner.ShowMode(EGuildmasterMode.Temple, config);
			banner.transform.SetParent(TempleWindow.transform);
			banner.transform.SetSiblingIndex(1);
			TempleWindow.EnterTemple();
			TempleWindow.SetEditMode(disableEditGoldModeRequests.Count == 0);
		}, TempleWindow.Exit);
		InitButtonMode(templeButton);
		modes[EGuildmasterMode.WorldMap] = new WorldMapMode(mapButton, OnReturnToWorldMap, OnLeaveWorldMap);
		if (AdventureState.MapState.IsCampaign)
		{
			mapButton.OnSelected.AddListener(delegate(EGuildmasterMode mode)
			{
				if (currentMode != EGuildmasterMode.City)
				{
					UpdateCurrentMode(mode);
				}
				else
				{
					Singleton<MapChoreographer>.Instance.OpenWorldMap();
				}
			});
			mapButton.OnHovered.AddListener(PreviewMode);
		}
		else
		{
			InitButtonMode(mapButton);
		}
		modes[EGuildmasterMode.City] = new CityMapMode(cityButton, OnReturnToCity, OnLeaveCityMap);
		cityButton.OnSelected.AddListener(delegate(EGuildmasterMode mode)
		{
			if (currentMode != EGuildmasterMode.WorldMap)
			{
				UpdateCurrentMode(mode);
			}
			else
			{
				Singleton<MapChoreographer>.Instance.OpenCityMap();
			}
		});
		cityButton.OnHovered.AddListener(PreviewMode);
		modes[EGuildmasterMode.TownRecords] = new TownRecordsMode(townRecordsButton, delegate
		{
			ResetBannerParent();
			banner.ShowMode(EGuildmasterMode.TownRecords);
			townRecordsWindow.OpenRecords().Done(delegate
			{
				UpdateCurrentMode(EGuildmasterMode.WorldMap);
			});
		}, townRecordsWindow.Exit);
		InitButtonMode(townRecordsButton);
		modes[EGuildmasterMode.MercenaryLog] = new MercenaryLogMode(mercenaryLogButton, delegate
		{
			ResetBannerParent();
			banner.ShowMode(EGuildmasterMode.MercenaryLog);
			townRecordsWindow.OpenLog();
		}, townRecordsWindow.Exit);
		InitButtonMode(mercenaryLogButton);
		if (AdventureState.MapState.IsCampaign)
		{
			cityEncounterButton.OnHovered.AddListener(delegate
			{
				OnHovered(hovered: true);
				Preview("GUI_CITY_ENCOUNTER");
			});
			cityEncounterButton.OnUnhovered.AddListener(FinishPreview);
			cityEncounterButton.gameObject.SetActive(value: true);
		}
		else
		{
			cityEncounterButton.gameObject.SetActive(value: false);
		}
		RefreshUnlockedOptions();
		OnHovered(hovered: false);
		controllerArea.OnEnabledArea.AddListener(OnControllerEnabled);
		controllerArea.OnDisabledArea.AddListener(OnControllerDisabled);
		_simpleKeyActionHandlerBlocker = new SimpleKeyActionHandlerBlocker(isBlock: true);
		KeyActionHandler.RegisterType registerType = KeyActionHandler.RegisterType.Release;
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.NEXT_SHIELD_TAB, NavigateNextSelectable, null, null, isPersistent: false, registerType).AddBlocker(_simpleKeyActionHandlerBlocker));
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.PREVIOUS_SHIELD_TAB, NavigatePreviousSelectable, null, null, isPersistent: false, registerType).AddBlocker(_simpleKeyActionHandlerBlocker));
	}

	private void OnControllerDisabled()
	{
		_simpleKeyActionHandlerBlocker.SetBlock(value: true);
		controllerArea.OnUnfocused.RemoveListener(OnControllerAreaUnfocused);
		controllerArea.OnFocused.RemoveListener(OnControllerAreaFocused);
	}

	private void OnControllerEnabled()
	{
		_simpleKeyActionHandlerBlocker.SetBlock(value: false);
		controllerArea.OnFocused.AddListener(OnControllerAreaFocused);
		controllerArea.OnUnfocused.AddListener(OnControllerAreaUnfocused);
	}

	public void EnableShieldInput(bool active)
	{
		_simpleKeyActionHandlerBlocker.SetBlock(!active);
	}

	private void OnControllerAreaFocused()
	{
		OnHovered(hovered: true);
		foreach (GuildmasterMode value in modes.Values)
		{
			value.EnableNavigation(NavigatePreviousSelectable, NavigateNextSelectable);
		}
		ExtendedButton[] componentsInChildren = mercenaryLogButton.transform.parent.GetComponentsInChildren<ExtendedButton>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].SetNavigation(new NavigationCalculator
			{
				left = GetPreviousSelectable,
				right = GetNextSelectable
			});
		}
	}

	private void OnControllerAreaUnfocused()
	{
		OnHovered(hovered: false);
		foreach (GuildmasterMode value in modes.Values)
		{
			value.DisableNavigation();
		}
		ExtendedButton[] componentsInChildren = mercenaryLogButton.transform.parent.GetComponentsInChildren<ExtendedButton>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].DisableNavigation();
		}
		EventSystem.current.SetSelectedGameObject(null);
	}

	private void NavigateNextSelectable()
	{
		NavigateToSelectable(GetNextSelectable());
	}

	private void NavigatePreviousSelectable()
	{
		NavigateToSelectable(GetPreviousSelectable());
	}

	private void NavigateToSelectable(Selectable selectable)
	{
		if (AreOptionsAvailable && optionsContainer.activeInHierarchy)
		{
			if (!controllerArea.IsFocused && (ControllerInputAreaManager.IsFocusedDefaultArea() || ControllerInputAreaManager.IsFocusedArea(EControllerInputAreaType.QuestLog) || ControllerInputAreaManager.IsFocusedArea(EControllerInputAreaType.CharacterActions)))
			{
				Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.GuildmasterHUD);
				controllerArea.Focus();
			}
			if (selectable != null)
			{
				selectable.Select();
				ExecuteEvents.Execute(selectable.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
			}
		}
	}

	private Selectable GetNextSelectable()
	{
		return GetSelectable(1);
	}

	private Selectable GetPreviousSelectable()
	{
		return GetSelectable(-1);
	}

	private Selectable GetSelectable(int paginateOffset)
	{
		if (!AreOptionsAvailable || !optionsContainer.activeInHierarchy)
		{
			return null;
		}
		Paginate(paginateOffset);
		return GetSelectableChild(_lastModeIndex);
	}

	private void Paginate(int offset)
	{
		int count = GetActiveButtons().Count;
		_lastModeIndex = _modePaginationStrategy.Paginate(count, offset, _lastModeIndex);
	}

	private Selectable GetSelectableChild(int index)
	{
		List<GuildmasterModeSelectable> activeButtons = GetActiveButtons();
		if (index >= activeButtons.Count || index < 0)
		{
			Debug.LogWarning("Wrong index!");
			return null;
		}
		return activeButtons[index].GetSelectable();
	}

	protected override void OnDestroy()
	{
		if (!CoreApplication.IsQuitting)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.NEXT_SHIELD_TAB, NavigateNextSelectable);
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.PREVIOUS_SHIELD_TAB, NavigatePreviousSelectable);
			NewPartyDisplayUI.PartyDisplay.OnOpenedPanel -= OnToggledPartyPanel;
			base.OnDestroy();
		}
	}

	public void HighlightWorldMode(bool show)
	{
		mapButton.ShowHovered(show);
	}

	public void LockInteraction(Component request, bool locked)
	{
		if (locked)
		{
			lockedInteractionRequests.Add(request);
		}
		else
		{
			lockedInteractionRequests.Remove(request);
		}
		buttonsCanvasGroup.interactable = lockedInteractionRequests.Count == 0;
	}

	public void RefreshNotifications(EGuildmasterMode mode)
	{
		modes[mode].RefreshNotifications();
	}

	private void InitButtonMode(UIGuildmasterButton button)
	{
		button.OnSelected.AddListener(UpdateCurrentMode);
		button.OnHovered.AddListener(PreviewMode);
	}

	public void UpdateCurrentMode(EGuildmasterMode newMode)
	{
		if (newMode == currentMode)
		{
			return;
		}
		toggleGroup.allowSwitchOff = newMode == EGuildmasterMode.None;
		if (currentMode != EGuildmasterMode.None)
		{
			modes[currentMode].Exit();
		}
		currentMode = newMode;
		if (newMode != EGuildmasterMode.None)
		{
			modes[newMode].Enter();
		}
		List<GuildmasterModeSelectable> activeButtons = GetActiveButtons();
		for (int i = 0; i < activeButtons.Count; i++)
		{
			if (activeButtons[i].GuildmasterMode == newMode)
			{
				_lastModeIndex = i;
				break;
			}
		}
	}

	private void InitializeButtons()
	{
		_modeSelectableObjects.Clear();
		mercenaryLogButton.transform.parent.GetComponentsInChildren(includeInactive: true, _modeSelectableObjects);
	}

	private List<GuildmasterModeSelectable> GetActiveButtons()
	{
		List<GuildmasterModeSelectable> list = new List<GuildmasterModeSelectable>(10);
		foreach (GuildmasterModeSelectable modeSelectableObject in _modeSelectableObjects)
		{
			if (modeSelectableObject.gameObject.activeInHierarchy)
			{
				list.Add(modeSelectableObject);
			}
		}
		return list;
	}

	private void OnExitTrainer(bool claimedRewards)
	{
		if (claimedRewards)
		{
			Singleton<MapChoreographer>.Instance.RefreshAllMapLocations();
		}
	}

	private void OnReturnToMap()
	{
		ControllerInputAreaManager.Instance.FocusArea(EControllerInputAreaType.WorldMap);
		banner.transform.SetParent(Singleton<QuestManager>.Instance.transform);
		banner.transform.SetAsFirstSibling();
		CameraController.s_CameraController?.DisableCameraInput(disableInput: false);
		if (NewPartyDisplayUI.PartyDisplay.Initialised)
		{
			NewPartyDisplayUI.PartyDisplay.EnableMapOptions();
			NewPartyDisplayUI.PartyDisplay.OnOpenedPanel -= OnToggledPartyPanel;
			Show(this);
		}
	}

	private void OnReturnToWorldMap()
	{
		OnReturnToMap();
		banner.ShowMode(EGuildmasterMode.WorldMap);
		if (AdventureState.MapState.IsCampaign && controllerArea.IsFocused && EventSystem.current.currentSelectedGameObject == mapButton.gameObject)
		{
			EventSystem.current.SetSelectedGameObject(cityButton.gameObject);
		}
	}

	private void OnReturnToCity()
	{
		OnReturnToMap();
		banner.ShowMode(EGuildmasterMode.City);
		if (controllerArea.IsFocused && EventSystem.current.currentSelectedGameObject == cityButton.gameObject)
		{
			EventSystem.current.SetSelectedGameObject(mapButton.gameObject);
		}
	}

	private void OnLeaveMap()
	{
		CameraController.s_CameraController?.DisableCameraInput(disableInput: true);
		NewPartyDisplayUI.PartyDisplay.DisableMapOptions();
		NewPartyDisplayUI.PartyDisplay.OnOpenedPanel -= OnToggledPartyPanel;
		NewPartyDisplayUI.PartyDisplay.OnOpenedPanel += OnToggledPartyPanel;
	}

	private void OnLeaveCityMap()
	{
		OnLeaveMap();
		if (controllerArea.IsFocused && EventSystem.current.currentSelectedGameObject == mapButton.gameObject)
		{
			EventSystem.current.SetSelectedGameObject(cityButton.gameObject);
		}
	}

	private void OnLeaveWorldMap()
	{
		OnLeaveMap();
		if (AdventureState.MapState.IsCampaign && controllerArea.IsFocused && EventSystem.current.currentSelectedGameObject == cityButton.gameObject)
		{
			EventSystem.current.SetSelectedGameObject(mapButton.gameObject);
		}
	}

	private void OnToggledPartyPanel(bool visible, NewPartyDisplayUI.DisplayType display)
	{
		if (display == NewPartyDisplayUI.DisplayType.CHARACTER_SELECTOR || display == NewPartyDisplayUI.DisplayType.LEVELUP)
		{
			HandlePartialHiding(isEnablePartialHide: false);
			if (visible)
			{
				Hide(this);
			}
			else
			{
				Show(this);
			}
		}
	}

	private void HandlePartialHiding(bool isEnablePartialHide)
	{
		NavigationStateMachine stateMachine = Singleton<UINavigation>.Instance.StateMachine;
		IState state = stateMachine.GetState(CampaignMapStateTag.Temple);
		if (stateMachine.PreviousState == state || stateMachine.CurrentState == state)
		{
			HandleTempleState(isEnablePartialHide);
			return;
		}
		IState state2 = stateMachine.GetState(CampaignMapStateTag.Merchant);
		if (stateMachine.PreviousState == state2 || stateMachine.CurrentState == state2)
		{
			HandleMerchantState(isEnablePartialHide);
		}
	}

	private void HandleTempleState(bool isEnablePartialHide)
	{
		if (isEnablePartialHide)
		{
			TempleWindow.EnablePartialHide();
		}
		else
		{
			TempleWindow.DisablePartialHide();
		}
		HideBanner(this, !isEnablePartialHide);
		optionsContainer.SetActive(isEnablePartialHide);
	}

	private void HandleMerchantState(bool isEnablePartialHide)
	{
		if (isEnablePartialHide)
		{
			shopWindow.ItemInventory.EnablePartialHide();
		}
		else
		{
			shopWindow.ItemInventory.DisablePartialHide();
		}
		optionsContainer.SetActive(isEnablePartialHide);
	}

	public void Show(object request, bool instant = true)
	{
		hideRequests.Remove(request);
		HideBanner(request, hide: false);
		if (hideRequests.Count <= 0)
		{
			window.ShowOrUpdateStartingState(instant);
			RefreshUnlockedOptions();
			controllerArea.enabled = true;
		}
	}

	public void Hide(object request, bool instant = true)
	{
		hideRequests.Add(request);
		window.HideOrUpdateStartingState(instant);
		HideBanner(request, hide: true);
		controllerArea.Unfocus();
		controllerArea.enabled = false;
	}

	[ContextMenu("PrintHideRequests")]
	private void PrintHideRequests()
	{
		Debug.LogGUI(string.Join(", ", hideRequests));
	}

	public void RefreshUnlockedOptions()
	{
		if (AdventureState.MapState.IsCampaign)
		{
			cityEncounterButton.ToggleGreyOut(!AdventureState.MapState.CanDrawCityEvent);
			bool flag = AdventureState.MapState.MapParty.SelectedCharacters.Count() > 1 && (!MapFTUEManager.IsPlaying || Singleton<MapFTUEManager>.Instance.HasCompletedStep(EMapFTUEStep.BuyItem));
			if (InputManager.GamePadInUse)
			{
				cityEncounterButton.gameObject.SetActive(value: false);
				bool flag2 = FFSNetwork.IsOnline && FFSNetwork.IsClient;
				if (flag && AdventureState.MapState.CanDrawCityEvent && !flag2)
				{
					Singleton<QuestManager>.Instance.AddCityEventQuest(OnGoToGloomhavenClicked);
				}
				else
				{
					Singleton<QuestManager>.Instance.HideCityEventQuest();
				}
			}
			else
			{
				cityEncounterButton.gameObject.SetActive(flag);
			}
		}
		if (modes == null)
		{
			return;
		}
		foreach (GuildmasterMode value in modes.Values)
		{
			value.RefreshUnlocked();
		}
		RefreshVisibilityHeadquartersOptions();
	}

	private void OnGoToGloomhavenClicked()
	{
		if (Singleton<MapChoreographer>.Instance.PartyAtHQ)
		{
			cityEncounterButton.OpenCityEvent();
			return;
		}
		Singleton<MapChoreographer>.Instance.AutoPlayCityEvent = true;
		Singleton<MapChoreographer>.Instance.HeadquartersLocation.Select();
		Singleton<AdventureMapUIManager>.Instance.OnTravelButtonClick();
	}

	public void OpenCityEvent()
	{
		cityEncounterButton.OpenCityEvent();
	}

	public void OpenCityEncounter()
	{
		Singleton<MapChoreographer>.Instance.OpenCityEvent();
		RefreshUnlockedOptions();
	}

	public void EnableHeadquartersOptions(Component request, bool enableOptions)
	{
		if (enableOptions)
		{
			disableOptionsRequests.Remove(request);
			if (disableOptionsRequests.Count == 0)
			{
				RefreshUnlockedOptions();
			}
			else
			{
				RefreshVisibilityHeadquartersOptions();
			}
		}
		else
		{
			disableOptionsRequests.Add(request);
			RefreshVisibilityHeadquartersOptions();
		}
	}

	public void HideBanner(object request, bool hide)
	{
		if (!hide)
		{
			hideBannerRequests.Remove(request);
		}
		else
		{
			hideBannerRequests.Add(request);
		}
		if (hideBannerRequests.Count > 0)
		{
			banner.Hide();
		}
		else if (currentMode != EGuildmasterMode.Enchantress && currentMode != EGuildmasterMode.Trainer)
		{
			banner.Show();
		}
	}

	private void ResetBannerParent()
	{
		banner.transform.SetParent(bannerContainer.transform);
	}

	private void RefreshVisibilityHeadquartersOptions()
	{
		bool flag = disableOptionsRequests.Count == 0;
		optionsContainer.SetActive(flag && (cityEncounterButton.gameObject.activeSelf || modes.Count((KeyValuePair<EGuildmasterMode, GuildmasterMode> it) => it.Key != EGuildmasterMode.WorldMap && it.Value.IsUnlocked) >= 1));
		if (flag)
		{
			confirmActionPresenter.ToggleOn();
		}
		else
		{
			confirmActionPresenter.ToggleOff();
		}
	}

	public bool IsAvailable(EGuildmasterMode mode)
	{
		if (window.IsVisible && modes[mode].IsUnlocked)
		{
			return disableOptionsRequests.Count == 0;
		}
		return false;
	}

	public void OnHovered(bool hovered)
	{
		isHovered = hovered;
		if (hovered)
		{
			if (AdventureState.MapState.IsCampaign)
			{
				hoveredOptionTooltip.SetTextKey(null);
				hoveredOptionTooltip.Text.text = string.Empty;
			}
			else
			{
				hoveredOptionTooltip.SetTextKey("GUI_GUILD");
			}
			hoverAnimation.Play(fromStart: false);
		}
		else
		{
			hoverAnimation.Stop(goToEnd: false);
			hoverAnimation.GoInitState();
		}
	}

	private void PreviewMode(EGuildmasterMode mode, bool preview)
	{
		OnHovered(hovered: true);
		if (preview)
		{
			Preview($"GUI_GUILD_{mode}");
		}
		else
		{
			FinishPreview();
		}
	}

	private void Preview(string mode)
	{
		hoveredOptionTooltip.SetTextKey(mode);
	}

	private void FinishPreview()
	{
		OnHovered(isHovered);
	}

	public void ToggleNavigationLock(bool isNavigationLocked)
	{
		_simpleKeyActionHandlerBlocker.SetBlock(isNavigationLocked);
	}

	public void OpenTrainer()
	{
		UpdateCurrentMode(EGuildmasterMode.Trainer);
	}

	public void EnableEditGoldMode(Component request)
	{
		disableEditGoldModeRequests.Remove(request);
		cityEncounterButton.ToggleGreyOut(greyedOut: false, request);
		((TownRecordsMode)modes[EGuildmasterMode.TownRecords]).RequestEnable(request);
		if (disableEditGoldModeRequests.Count == 0)
		{
			switch (currentMode)
			{
			case EGuildmasterMode.Temple:
				TempleWindow.SetEditMode(isEnabled: true);
				break;
			case EGuildmasterMode.Merchant:
				shopWindow.SetEditMode(canEdit: true);
				break;
			case EGuildmasterMode.Enchantress:
				enhancementWindow.SetEditMode(canEdit: true);
				break;
			}
		}
	}

	public ICallbackPromise ShowUnlockTownRecords()
	{
		if (AdventureState.MapState.HeadquartersState.HasShownIntroTownRecords || AdventureState.MapState.MapParty.RetiredCharacterRecords.Count == 0)
		{
			return CallbackPromise.Resolved();
		}
		CallbackPromise promise = new CallbackPromise();
		EGuildmasterMode oldMode = CurrentMode;
		Singleton<MapStoryController>.Instance.Show(EMapMessageTrigger.TownRecords, new MapStoryController.MapDialogInfo(new List<DialogLineDTO>
		{
			new DialogLineDTO("GUI_FIRST_RETIRED_CHARACTERS_STORY_1", "Narrator", EExpression.Default, null, null, "GUI_FIRST_RETIRED_CHARACTERS_STORY_1"),
			new DialogLineDTO("GUI_FIRST_RETIRED_CHARACTERS_STORY_2", "Narrator", EExpression.Default, null, null, "GUI_FIRST_RETIRED_CHARACTERS_STORY_2")
		}, delegate
		{
			townRecordsWindow.OpenLog(force: true).Done(promise.Resolve);
			UpdateCurrentMode(EGuildmasterMode.MercenaryLog);
		}));
		return promise.Then(delegate
		{
			if (!IsAvailable(EGuildmasterMode.MercenaryLog))
			{
				UpdateCurrentMode(oldMode);
			}
		});
	}

	public void DisableEditGoldMode(Component request)
	{
		disableEditGoldModeRequests.Add(request);
		cityEncounterButton.ToggleGreyOut(greyedOut: true, request);
		((TownRecordsMode)modes[EGuildmasterMode.TownRecords]).RequestDisable(request);
		switch (currentMode)
		{
		case EGuildmasterMode.Temple:
			TempleWindow.SetEditMode(isEnabled: false);
			break;
		case EGuildmasterMode.Merchant:
			shopWindow.SetEditMode(canEdit: false);
			break;
		case EGuildmasterMode.Enchantress:
			enhancementWindow.SetEditMode(canEdit: false);
			break;
		}
	}

	public void EnableCityEncounter(Component request, bool enable)
	{
		cityEncounterButton.ToggleGreyOut(!enable, request);
	}

	public void DisableCityEncounter(Component request, string tooltip = null)
	{
		cityEncounterButton.ToggleGreyOut(greyedOut: true, request, tooltip);
	}

	public void ProxyBuyEnhancement(GameAction action, ref bool actionValid)
	{
		if ((FFSNetwork.IsHost && PlayerRegistry.JoiningPlayers.Count > 0) || (FFSNetwork.IsClient && PlayerRegistry.OtherClientsAreJoining))
		{
			actionValid = false;
		}
		else
		{
			enhancementWindow.ProxyBuyEnhancement(action, ref actionValid);
		}
	}

	public void ProxySellEnhancement(GameAction action, ref bool actionValid)
	{
		if ((FFSNetwork.IsHost && PlayerRegistry.JoiningPlayers.Count > 0) || (FFSNetwork.IsClient && PlayerRegistry.OtherClientsAreJoining))
		{
			actionValid = false;
		}
		else
		{
			enhancementWindow.ProxySellEnhancement(action, ref actionValid);
		}
	}

	public void ClientClaimAchievementReward(GameAction action)
	{
		trainerWindow.ClientClaimAchievementReward(action);
	}

	public void ProxyBuyBlessing(GameAction action, ref bool actionValid)
	{
		if ((FFSNetwork.IsHost && PlayerRegistry.JoiningPlayers.Count > 0) || (FFSNetwork.IsClient && PlayerRegistry.OtherClientsAreJoining))
		{
			actionValid = false;
		}
		else
		{
			TempleWindow.ProxyBuyBlessing(action, ref actionValid);
		}
	}

	public bool Escape()
	{
		if (!InputManager.GamePadInUse)
		{
			return false;
		}
		if (currentMode != EGuildmasterMode.WorldMap && IsAvailable(EGuildmasterMode.WorldMap) && mapButton.IsShown)
		{
			mapButton.Select();
			return true;
		}
		if (currentMode != EGuildmasterMode.City && IsAvailable(EGuildmasterMode.City) && cityButton.IsShown)
		{
			cityButton.Select();
			return true;
		}
		return false;
	}

	public int Order()
	{
		return 0;
	}
}
