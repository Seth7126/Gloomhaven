#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chronos;
using I2.Loc;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using SM.Gamepad;
using Script.GUI;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;
using UnityEngine;
using UnityEngine.UI;

public class AdventureMapUIManager : Singleton<AdventureMapUIManager>, IEscapable
{
	private const string GUIHotkeyEnterTravel = "Consoles/GUI_HOTKEY_CONFIRM_TRAVEL";

	private const string GUIHotkeyCancelTravel = "GUI_CANCEL";

	[SerializeField]
	private ExtendedButton travelButton;

	[SerializeField]
	private Button cancelTravelButton;

	[SerializeField]
	private GameObject travelOptions;

	[SerializeField]
	private GameObject warningTravel;

	[SerializeField]
	private float helpBoxHideTime = 1.5f;

	[SerializeField]
	private GameObject lockMapInteractionMask;

	[SerializeField]
	private GameObject lockMapInteractionBlur;

	[SerializeField]
	private string confirmTravelAudiItem;

	[SerializeField]
	private ControllerInputAreaLocal inputAreaLocal;

	[SerializeField]
	private LongConfirmHandler _longConfirmHandler;

	[SerializeField]
	private Hotkey _confirmHotkey;

	[SerializeField]
	private bool m_IsAllowedToEscapeDuringSave;

	private Coroutine hideHelpboxCorutine;

	private Action<MapLocation> onConfirmTravelCallback;

	private MapLocation locationToTravel;

	private Dictionary<Component, bool> lockInteractionRequests = new Dictionary<Component, bool>();

	private IHotkeySession hotkeySession;

	public MapLocation LocationToTravel => locationToTravel;

	public bool IsLocked => !lockInteractionRequests.IsNullOrEmpty();

	public bool IsAllowedToEscapeDuringSave => m_IsAllowedToEscapeDuringSave;

	public event Action LockChanged;

	private new void Awake()
	{
		base.Awake();
		if (InputManager.GamePadInUse)
		{
			InitGamepadInput();
		}
		else
		{
			travelButton.onClick.AddListener(OnTravelButtonClick);
			cancelTravelButton.onClick.AddListener(OnCancelTravelButtonClick);
		}
		if (FFSNetwork.IsOnline)
		{
			OnSwitchedToMultiplayer();
		}
		else
		{
			OnSwitchedToSinglePlayer();
		}
	}

	protected override void OnDestroy()
	{
		if (InputManager.GamePadInUse)
		{
			DisableGamepadInput();
		}
		UIWindowManager.UnregisterEscapable(this);
		FFSNetwork.Manager.HostingStartedEvent.RemoveListener(OnSwitchedToMultiplayer);
		FFSNetwork.Manager.HostingEndedEvent.RemoveListener(OnSwitchedToSinglePlayer);
		travelButton.onClick.RemoveAllListeners();
		cancelTravelButton.onClick.RemoveAllListeners();
		this.LockChanged = null;
		base.OnDestroy();
	}

	private void Start()
	{
		travelButton.interactable = !FFSNetwork.IsClient;
		lockMapInteractionMask.gameObject.SetActive(value: false);
		EnableTravelOptions(show: false);
	}

	private void SetMultiplayerButtonLabels()
	{
		string translation = LocalizationManager.GetTranslation("Consoles/GUI_HOTKEY_CONFIRM_TRAVEL");
		string translation2 = LocalizationManager.GetTranslation("GUI_CANCEL");
		Singleton<UIReadyToggle>.Instance.SetLabelTextGamepad(translation);
		Singleton<UIReadyToggle>.Instance.SetCancelLabelTextGamepad(translation2);
	}

	private void InitGamepadInput()
	{
		_confirmHotkey.Initialize(Singleton<UINavigation>.Instance.Input);
		hotkeySession = Hotkeys.Instance.GetSessionOrEmpty();
	}

	private void OnSwitchedToSinglePlayer()
	{
		FFSNetwork.Manager.HostingEndedEvent.RemoveListener(OnSwitchedToSinglePlayer);
		FFSNetwork.Manager.HostingStartedEvent.AddListener(OnSwitchedToMultiplayer);
		UnlockSinglePlayerConfirmButton();
	}

	private void OnSwitchedToMultiplayer()
	{
		FFSNetwork.Manager.HostingStartedEvent.RemoveListener(OnSwitchedToMultiplayer);
		FFSNetwork.Manager.HostingEndedEvent.AddListener(OnSwitchedToSinglePlayer);
		LockSinglePlayerConfirmButton();
	}

	private void DisableGamepadInput()
	{
		_confirmHotkey.Deinitialize();
		hotkeySession?.Dispose();
		hotkeySession = null;
	}

	public void OnConfirmHotkeyPressed(Action longPressedCallback, Action shortPressedCallback = null)
	{
		if (_longConfirmHandler.IsActive)
		{
			_longConfirmHandler.Pressed(longPressedCallback, shortPressedCallback);
		}
	}

	public void OnCancelHotkeyPressed(Action callback)
	{
		if (travelOptions.gameObject.activeInHierarchy)
		{
			callback?.Invoke();
		}
	}

	public void UnlockSinglePlayerConfirmButton()
	{
		if (!FFSNetwork.IsClient && !FFSNetwork.IsHost)
		{
			if (!InputManager.GamePadInUse)
			{
				travelButton.gameObject.SetActive(value: true);
			}
			if (_longConfirmHandler != null)
			{
				_longConfirmHandler.SetActiveLongConfirmButton(isActive: true);
			}
		}
	}

	public void LockSinglePlayerConfirmButton()
	{
		if (!InputManager.GamePadInUse)
		{
			travelButton.gameObject.SetActive(value: false);
		}
		if (_longConfirmHandler != null)
		{
			_longConfirmHandler.SetActiveLongConfirmButton(isActive: false);
		}
	}

	public void OnTravelButtonClick()
	{
		if (travelButton.interactable && lockInteractionRequests.IsNullOrEmpty())
		{
			ConfirmTravel();
		}
	}

	public void OnCancelTravelButtonClick()
	{
		if (lockInteractionRequests.IsNullOrEmpty())
		{
			Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.WorldMap);
			CancelTravel();
		}
	}

	public void HideTravelOption()
	{
		UIWindowManager.UnregisterEscapable(this);
		ResetLocationToTravel();
		onConfirmTravelCallback = null;
		EnableTravelOptions(show: false);
		HideTravelWarning();
	}

	public void CancelTravel()
	{
		MapLocation mapLocation = ((locationToTravel != null && locationToTravel.IsSelected) ? locationToTravel : null);
		if (!(mapLocation != null) || Singleton<UIMapMultiplayerController>.Instance.CanCancelSelectedLocation(mapLocation))
		{
			HideTravelOption();
			locationToTravel = mapLocation;
			locationToTravel?.ForceHighlight(force: false);
			locationToTravel?.Deselect();
			ResetLocationToTravel();
			UIWindowManager.UnregisterEscapable(this);
		}
	}

	public void ConfirmTravel()
	{
		if (!(locationToTravel == null))
		{
			if (CheckTravel())
			{
				AudioControllerUtils.PlaySound(confirmTravelAudiItem);
				onConfirmTravelCallback?.Invoke(locationToTravel);
				HideTravelOption();
			}
			else
			{
				AudioControllerUtils.PlaySound(UIInfoTools.Instance.InvalidOptionAudioItem);
			}
		}
	}

	public bool CheckTravel()
	{
		return CheckTravelQuestConditions();
	}

	private bool CheckTravelQuestConditions()
	{
		if (locationToTravel == null || locationToTravel.LocationQuest == null)
		{
			ClearTravelWarning();
			return true;
		}
		RequirementCheckResult requirementCheckResult = locationToTravel.LocationQuest.CheckRequirements();
		if (requirementCheckResult.IsUnlocked() || Singleton<MapChoreographer>.Instance.ShowAllScenariosMode)
		{
			ClearTravelWarning();
			return true;
		}
		ShowWarning(requirementCheckResult);
		return false;
	}

	public void ShowWarning(RequirementCheckResult requirements, bool autoHide = false)
	{
		StopHideWarningAnimation();
		Singleton<HelpBox>.Instance.ShowTranslated(requirements.ToStringList(), useWarningFormat: true);
		NewPartyDisplayUI.PartyDisplay.ShowRequiredCharactersWarning(requirements.missingAmountCharacters - AdventureState.MapState.MapParty.SelectedCharacters.Count(), warningTravel.activeSelf);
		if (requirements.missingRequiredCharacters.Count == 0)
		{
			NewPartyDisplayUI.PartyDisplay.HideClassCharactersWarning();
		}
		else
		{
			NewPartyDisplayUI.PartyDisplay.ShowClassCharactersWarning(warningTravel.activeSelf);
		}
		if (warningTravel.activeSelf)
		{
			Singleton<HelpBox>.Instance.HighlightWarning();
		}
		else
		{
			warningTravel.SetActive(value: true);
		}
		if (autoHide)
		{
			hideHelpboxCorutine = StartCoroutine(DelayHideTravelWarning());
		}
	}

	private IEnumerator DelayHideTravelWarning()
	{
		Singleton<HelpBox>.Instance.HighlightWarning();
		yield return Timekeeper.instance.WaitForSeconds(helpBoxHideTime);
		hideHelpboxCorutine = null;
		HideTravelWarning();
	}

	private void StopHideWarningAnimation()
	{
		if (hideHelpboxCorutine != null)
		{
			StopCoroutine(hideHelpboxCorutine);
		}
		hideHelpboxCorutine = null;
	}

	public void HideTravelWarning()
	{
		ClearTravelWarning();
	}

	private void ClearTravelWarning()
	{
		StopHideWarningAnimation();
		if (Singleton<HelpBox>.Instance != null)
		{
			Singleton<HelpBox>.Instance.Hide();
		}
		warningTravel.SetActive(value: false);
		NewPartyDisplayUI.PartyDisplay.HideRequiredCharactersWarning();
		NewPartyDisplayUI.PartyDisplay.HideClassCharactersWarning();
	}

	public void OnSelectedMapLocation(MapLocation mapLocation, Action<MapLocation> onConfirmTravelCallback)
	{
		this.onConfirmTravelCallback = onConfirmTravelCallback;
		if (mapLocation == locationToTravel)
		{
			if (!FFSNetwork.IsOnline)
			{
				ConfirmTravel();
			}
			return;
		}
		DeselectCurrentMapLocation();
		this.onConfirmTravelCallback = onConfirmTravelCallback;
		SetLocationToTravel(mapLocation);
		Singleton<MapMarkersManager>.Instance.FadeMarkers();
		HideTravelWarning();
		travelButton.TextLanguageKey = (locationToTravel.IsCompleted() ? "GUI_REPLAY_LOCATION" : "GUI_TRAVEL");
		Singleton<UIGuildmasterHUD>.Instance.EnableHeadquartersOptions(this, enableOptions: false);
		Singleton<UIMapMultiplayerController>.Instance.OnSelectedLocation();
		UIWindowManager.RegisterEscapable(this);
		SetFocused(focused: true);
	}

	public void LockOptionsInteraction(bool locked, Component request, bool blur = false)
	{
		if (locked)
		{
			lockInteractionRequests[request] = blur;
			Debug.LogGUI("Locked options by " + request?.name + " " + lockInteractionRequests.Count);
		}
		else
		{
			lockInteractionRequests.Remove(request);
			Debug.LogGUI("Unlocked options by " + request?.name + " " + lockInteractionRequests.Count);
		}
		if (!PlatformLayer.Setting.SimplifiedUI || !PlatformLayer.Setting.SimplifiedUISettings.DisableUIBlur)
		{
			lockMapInteractionBlur.SetActive(lockInteractionRequests.Any((KeyValuePair<Component, bool> it) => it.Value));
		}
		else
		{
			lockMapInteractionBlur.SetActive(value: false);
		}
		lockMapInteractionMask.SetActive(lockInteractionRequests.Count > 0);
		this.LockChanged?.Invoke();
	}

	public void DeselectCurrentMapLocation()
	{
		if (locationToTravel != null && locationToTravel.IsSelected)
		{
			Debug.LogGUI("DeselectCurrentMapLocation");
			locationToTravel.Deselect();
		}
	}

	public void OnDeselectedMapLocation(MapLocation mapLocation)
	{
		if (mapLocation == locationToTravel)
		{
			Debug.LogGUI("OnDeselectedMapLocation");
			HideTravelOption();
			Singleton<MapMarkersManager>.Instance.ShowMarkers();
			Singleton<UIMapMultiplayerController>.Instance.OnDeselectedQuest();
		}
		Singleton<UIGuildmasterHUD>.Instance.EnableHeadquartersOptions(this, enableOptions: true);
	}

	public void EnableTravelOptions(bool show)
	{
		if (!InputManager.GamePadInUse)
		{
			travelButton.gameObject.SetActive(!FFSNetwork.IsOnline);
		}
		bool flag = show && (!FFSNetwork.IsOnline || (!Singleton<UIMapMultiplayerController>.Instance.IsShowingHostQuestToClient() && !Singleton<UIMapMultiplayerController>.Instance.HasConfirmedQuest()));
		travelOptions.SetActive(flag);
		hotkeySession?.SetHotkeyAdded("Back", flag);
		if (FFSNetwork.IsOnline && FFSNetwork.IsHost)
		{
			Singleton<UIMapMultiplayerController>.Instance.ToggleReadyUpUI(show, EReadyUpToggleStates.Quests);
		}
	}

	public void RefreshTravelOptions()
	{
		EnableTravelOptions(locationToTravel != null);
		if (locationToTravel != null)
		{
			CheckTravel();
		}
	}

	private void SetLocationToTravel(MapLocation location)
	{
		locationToTravel = location;
		NewPartyDisplayUI.PartyDisplay.OnOpenedPanel += OnPartyPanelToggled;
	}

	private void ResetLocationToTravel()
	{
		locationToTravel = null;
		NewPartyDisplayUI.PartyDisplay.OnOpenedPanel -= OnPartyPanelToggled;
	}

	private void Focus()
	{
		if (inputAreaLocal != null)
		{
			inputAreaLocal.Enable();
		}
	}

	private void OnPartyPanelToggled(bool visible, NewPartyDisplayUI.DisplayType display)
	{
		bool focused = !visible && NewPartyDisplayUI.PartyDisplay.ActiveDisplay == NewPartyDisplayUI.DisplayType.NONE;
		SetFocused(focused);
	}

	private void SetFocused(bool focused)
	{
		EnableTravelOptions(focused);
		if (focused)
		{
			TrySwitchToQuestState();
		}
	}

	private void TrySwitchToQuestState()
	{
		if (locationToTravel == null)
		{
			return;
		}
		if (locationToTravel.Location is CHeadquartersState)
		{
			Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.GloomhavenTravelCityState);
		}
		else
		{
			Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.TravelQuestState);
		}
		if (FFSNetwork.IsOnline)
		{
			if (FFSNetwork.IsClient)
			{
				LockSinglePlayerConfirmButton();
			}
			SetMultiplayerButtonLabels();
		}
	}

	public bool Escape()
	{
		if (!cancelTravelButton.IsInteractable() || !cancelTravelButton.gameObject.activeInHierarchy || lockInteractionRequests.Any() || !inputAreaLocal.IsFocused)
		{
			return false;
		}
		CancelTravel();
		return true;
	}

	public int Order()
	{
		return 0;
	}
}
