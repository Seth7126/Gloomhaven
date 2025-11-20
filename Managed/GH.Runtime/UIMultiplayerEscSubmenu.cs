using System;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Adventure;
using Platforms.Social;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI;
using Script.GUI.Multiplayer;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.MainMenuStates;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VoiceChat;

[RequireComponent(typeof(UIWindow))]
public class UIMultiplayerEscSubmenu : Singleton<UIMultiplayerEscSubmenu>
{
	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	[SerializeField]
	private ScrollRect contentScroll;

	[SerializeField]
	private GraphicsListEnabler focusMasks;

	[SerializeField]
	private GraphicsListEnabler focusVoiceChatHostMask;

	[SerializeField]
	private GraphicsListEnabler focusVoiceChatClientMask;

	[SerializeField]
	private GraphicsListEnabler focusFriendlistMasks;

	[Header("Session")]
	[SerializeField]
	private Button startSessionButton;

	[SerializeField]
	private Button endSessionButton;

	[SerializeField]
	private CanvasGroup onlineOptionsGroup;

	[SerializeField]
	private CanvasGroup clientSessionGroup;

	[SerializeField]
	private CanvasGroup hostSessionGroup;

	[Header("Invite")]
	[SerializeField]
	private Button copyCodeButton;

	[SerializeField]
	private Button generateCodeButton;

	[SerializeField]
	private TextMeshProUGUI inviteCodeText;

	[SerializeField]
	private UIMenuOptionButton openFriendListButton;

	[SerializeField]
	private MultiplayerFriendList friendListWindow;

	[SerializeField]
	private UIMultiplayerInvitePlayersWindow invitePlayersWindow;

	[SerializeField]
	public UIMultiplayerSignInConfirmationBox signInConfirmationBox;

	[Header("Steam")]
	[SerializeField]
	private GameObject steamPanel;

	[SerializeField]
	private Button openSTEAMButton;

	[Header("Stadia")]
	[SerializeField]
	private Button openStadiaButton;

	[Header("Epic")]
	[SerializeField]
	private GameObject epicPanel;

	[SerializeField]
	private Button openEpicButton;

	[SerializeField]
	private Graphic epicMask;

	[SerializeField]
	private GraphicsListEnabler focusEpicMasks;

	[SerializeField]
	private UIMenuOptionButton[] voiceChatButtons;

	[SerializeField]
	private UIMenuOptionButton clientVoiceChatButtons;

	[Header("Assign characters")]
	[SerializeField]
	private UIMultiplayerMerchantRoster heroSlotsController;

	[SerializeField]
	private UIMultiplayerSelectPlayerScreen selectPlayerPopup;

	[Header("Misc")]
	[SerializeField]
	private LocalHotkeys _hotkeys;

	private INetworkSessionService sessionService;

	private INetworkHeroAssignService heroAssignService;

	private INetworkLoginService epicLoginService;

	private UIWindow window;

	private IHotkeySession _hotkeySession;

	private bool _voiceChatWasOpen;

	private GraphicsListEnabler CurrentVoiceChatMask
	{
		get
		{
			if (!FFSNetwork.IsClient)
			{
				return focusVoiceChatHostMask;
			}
			return focusVoiceChatClientMask;
		}
	}

	public UnityEvent OnHide => Window.onHidden;

	public INetworkSessionService SessionService => sessionService;

	public UIWindow Window => window;

	private void Start()
	{
		epicLoginService = new DummyNetworkLoginService(signedIn: false, 5f);
		SetInstance(this);
		heroSlotsController.OnSelectedSlot.AddListener(OnSelectedHeroToAssign);
		heroSlotsController.OnDeselectedSlot.AddListener(OnDeselectedHeroToAssign);
		window = GetComponent<UIWindow>();
		Window.onShown.AddListener(OnShown);
		Window.onHidden.AddListener(OnHidden);
		endSessionButton.onClick.AddListener(ShowConfirmationBoxToEndSession);
		startSessionButton.onClick.AddListener(StartSession);
		copyCodeButton.onClick.AddListener(CopyCode);
		generateCodeButton.onClick.AddListener(GenerateCode);
		sessionService = GetComponent<INetworkSessionService>();
		heroAssignService = GetComponent<INetworkHeroAssignService>();
		MultiplayerFriendList multiplayerFriendList = friendListWindow;
		multiplayerFriendList.OnHiddenCallback = (Action)Delegate.Combine(multiplayerFriendList.OnHiddenCallback, new Action(FriendsOnHiddenCallback));
		steamPanel.SetActive(PlatformLayer.Networking.PlatformInvitesSupported);
		openEpicButton.onClick.AddListener(OpenEpicInvite);
		InitializeInviteOption();
		PlatformLayer.Networking.RegisterNetworkSessionService(SessionService, heroAssignService);
		PlatformLayer.Networking.RegisterNetworkSessionServiceWithEpic(SessionService);
	}

	private void InitializeInviteOption()
	{
		bool flag = IsSteamInviteOptionAvailable();
		bool platformInvitesSupported = PlatformLayer.Networking.PlatformInvitesSupported;
		openFriendListButton.gameObject.SetActive(!flag && platformInvitesSupported);
		openSTEAMButton.gameObject.SetActive(flag && platformInvitesSupported);
	}

	private bool IsSteamInviteOptionAvailable()
	{
		return !PlatformLayer.Instance.IsConsole;
	}

	protected override void OnDestroy()
	{
		heroSlotsController.OnSelectedSlot.RemoveListener(OnSelectedHeroToAssign);
		heroSlotsController.OnDeselectedSlot.RemoveListener(OnDeselectedHeroToAssign);
		Window.onShown.RemoveListener(OnShown);
		Window.onHidden.RemoveListener(OnHidden);
		endSessionButton.onClick.RemoveListener(ShowConfirmationBoxToEndSession);
		startSessionButton.onClick.RemoveListener(StartSession);
		copyCodeButton.onClick.RemoveListener(CopyCode);
		generateCodeButton.onClick.RemoveListener(GenerateCode);
		if (_voiceChatWasOpen)
		{
			Singleton<SpecialUIProvider>.Instance.UIMultiplayerVoiceChat.OnHiddenCallback -= VoiceChatOnHidden;
		}
		MultiplayerFriendList multiplayerFriendList = friendListWindow;
		multiplayerFriendList.OnHiddenCallback = (Action)Delegate.Remove(multiplayerFriendList.OnHiddenCallback, new Action(FriendsOnHiddenCallback));
		INetworkSessionService networkSessionService = SessionService;
		networkSessionService.OnPlayerJoined = (PlayersChangedEvent)Delegate.Remove(networkSessionService.OnPlayerJoined, new PlayersChangedEvent(OnPlayerJoined));
		base.OnDestroy();
	}

	private void VoiceChatOnDestroy()
	{
		Singleton<SpecialUIProvider>.Instance.UIMultiplayerVoiceChat.OnHiddenCallback -= VoiceChatOnHidden;
		_voiceChatWasOpen = false;
	}

	private void VoiceChatOnHidden()
	{
		CurrentVoiceChatMask.SetEnable(enable: false);
		UIMenuOptionButton[] array = voiceChatButtons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetSelected(selected: false);
		}
		OnFocused();
	}

	private void FriendsOnHiddenCallback()
	{
		FocusFriends(focus: true);
		openFriendListButton.SetSelected(selected: false);
		OnFocused();
	}

	public void OpenVoiceChatOptions()
	{
		UIMenuOptionButton[] array = voiceChatButtons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetSelected(selected: false);
		}
		CurrentVoiceChatMask.SetEnable(enable: true);
		VoceChatOptions uIMultiplayerVoiceChat = Singleton<SpecialUIProvider>.Instance.UIMultiplayerVoiceChat;
		uIMultiplayerVoiceChat.Show();
		if (!_voiceChatWasOpen)
		{
			uIMultiplayerVoiceChat.OnHiddenCallback += VoiceChatOnHidden;
			uIMultiplayerVoiceChat.OnDestroyCallback += VoiceChatOnDestroy;
		}
		_voiceChatWasOpen = true;
		OnUnfocused();
	}

	public void OpenFriendList()
	{
		openFriendListButton.SetSelected(selected: false);
		FocusFriends(focus: false);
		friendListWindow.Show();
		OnUnfocused();
	}

	private void DisableNavigation()
	{
		startSessionButton.DisableNavigation();
		endSessionButton.DisableNavigation();
		copyCodeButton.DisableNavigation();
		generateCodeButton.DisableNavigation();
		openSTEAMButton.DisableNavigation();
		openStadiaButton.DisableNavigation();
		heroSlotsController.DisableNavigation();
		openEpicButton.DisableNavigation();
	}

	private void EnableNavigation()
	{
		startSessionButton.SetNavigation(Navigation.Mode.Vertical);
		endSessionButton.SetNavigation(Navigation.Mode.Vertical);
		copyCodeButton.SetNavigation(Navigation.Mode.Vertical);
		generateCodeButton.SetNavigation(Navigation.Mode.Vertical);
		openSTEAMButton.SetNavigation(Navigation.Mode.Vertical);
		openStadiaButton.SetNavigation(Navigation.Mode.Vertical);
		openEpicButton.SetNavigation(Navigation.Mode.Vertical);
		heroSlotsController.EnableNavigation();
		SelectFirstElement();
	}

	private void SelectFirstElement()
	{
		if (controllerArea.IsFocused)
		{
			if (startSessionButton.gameObject.activeInHierarchy)
			{
				startSessionButton.Select();
			}
			else if (heroSlotsController.CurrentSlot == null)
			{
				Singleton<UINavigation>.Instance.NavigationManager.TrySelectFirstIn(Singleton<UINavigation>.Instance.NavigationManager.CurrentNavigationRoot);
			}
			else
			{
				heroSlotsController.CurrentSlot.EnableNavigation(select: true);
			}
		}
	}

	public void Show()
	{
		epicPanel.SetActive(SaveData.Instance.Global.EpicLogin);
		if (PlatformLayer.Instance.IsConsole)
		{
			epicPanel.SetActive(value: false);
		}
		Window.Show();
		controllerArea.Enable();
	}

	public void Hide()
	{
		Window.Hide();
	}

	public void SetClientHostContainersVisibility(bool isClient)
	{
		clientSessionGroup.gameObject.SetActive(isClient);
		hostSessionGroup.gameObject.SetActive(!isClient);
	}

	private void Focus(bool focus)
	{
		focusMasks.SetEnable(!focus);
		heroSlotsController.Focus(focus);
		if (focus)
		{
			focusEpicMasks.SetEnable(enable: false);
		}
		epicMask?.SetAlpha(1f);
	}

	private void FocusFriends(bool focus)
	{
		focusFriendlistMasks.SetEnable(!focus);
	}

	private void OnClosedSelectPlayer()
	{
		heroSlotsController.ResetSelection();
		Focus(focus: true);
		OnFocused();
	}

	private void AssignHeroToPlayer(NetworkPlayer player)
	{
		heroSlotsController.AssignPlayerToSelectedSlot(player, SessionService.IsOnline(player));
	}

	private void OnDeselectedHeroToAssign()
	{
		selectPlayerPopup.Hide();
	}

	private void OnSelectedHeroToAssign(UIMultiplayerMerchantSlot slot)
	{
		if (FFSNetwork.IsHost)
		{
			Focus(focus: false);
			OnUnfocused();
			selectPlayerPopup.Show(base.name, heroSlotsController.CurrentSlot.CharacterID, heroSlotsController.CurrentSlot.CharacterName, slot.SlotIndex, slot.AssignedPlayer, slot.transform as RectTransform, OnClosedSelectPlayer, AssignHeroToPlayer);
		}
	}

	private void OnShown()
	{
		INetworkSessionService networkSessionService = SessionService;
		networkSessionService.OnPlayerJoined = (PlayersChangedEvent)Delegate.Combine(networkSessionService.OnPlayerJoined, new PlayersChangedEvent(OnPlayerJoined));
		ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Combine(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllableOwnersChanged));
		heroSlotsController.Init(AdventureState.MapState.MapParty.SelectedCharactersArray);
		if (selectPlayerPopup.IsOpen)
		{
			selectPlayerPopup.Hide();
		}
		Focus(focus: true);
		OnFocused();
		SetClientHostContainersVisibility(FFSNetwork.IsClient);
		ShowOnlineOptions(FFSNetwork.IsOnline && !FFSNetwork.IsStartingUp);
		Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.MultiplayerOnlineContainer);
	}

	private void OnHidden()
	{
		INetworkSessionService networkSessionService = SessionService;
		networkSessionService.OnPlayerJoined = (PlayersChangedEvent)Delegate.Remove(networkSessionService.OnPlayerJoined, new PlayersChangedEvent(OnPlayerJoined));
		ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Remove(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllableOwnersChanged));
		OnUnfocused();
		heroSlotsController.ResetSelection();
		selectPlayerPopup.Hide();
		invitePlayersWindow.Hide();
		signInConfirmationBox.Hide();
		if (Singleton<SpecialUIProvider>.Instance.UIMultiplayerVoiceChat != null)
		{
			Singleton<SpecialUIProvider>.Instance.UIMultiplayerVoiceChat.Hide();
		}
		friendListWindow.Hide();
		controllerArea.Destroy();
	}

	private void OnPlayerJoined(NetworkPlayer player)
	{
		heroSlotsController.UpdateConnectionStateSlot(player, isOnline: true);
	}

	private void OnControllableOwnersChanged(NetworkControllable controllable, NetworkPlayer oldController, NetworkPlayer newController)
	{
		string characterID = (AdventureState.MapState.IsCampaign ? AdventureState.MapState.GetMapCharacterIDWithCharacterNameHash(controllable.ID) : CharacterClassManager.GetCharacterIDFromModelInstanceID(controllable.ID));
		heroSlotsController.AssignPlayerTo(newController, characterID, SessionService.IsOnline(newController));
	}

	public void Refresh()
	{
		if (Window.IsOpen)
		{
			RefreshAssignedHeroes();
			selectPlayerPopup.RefreshPlayers();
		}
	}

	public void RefreshAssignInteraction()
	{
		if (Window.IsOpen)
		{
			heroSlotsController.RefreshInteractable();
		}
	}

	public void RefreshAssignedHeroes()
	{
		if (!FFSNetwork.IsOnline)
		{
			return;
		}
		foreach (Tuple<string, NetworkPlayer> characterAssignation in SessionService.GetCharacterAssignations())
		{
			if (characterAssignation.Item1 != null)
			{
				heroSlotsController.AssignPlayerTo(characterAssignation.Item2, characterAssignation.Item1, SessionService.IsOnline(characterAssignation.Item2));
			}
			else
			{
				heroSlotsController.AssignPlayerToEmptySlots(characterAssignation.Item2, SessionService.IsOnline(characterAssignation.Item2));
			}
		}
	}

	private void LoadCurrentCode()
	{
		if (SessionService.GetInviteCode().IsNullOrEmpty())
		{
			copyCodeButton.gameObject.SetActive(value: false);
			generateCodeButton.gameObject.SetActive(value: true);
		}
		else
		{
			generateCodeButton.gameObject.SetActive(value: false);
			inviteCodeText.text = LocalizationManager.GetTranslation("GUI_COPY") + ": <color=#F1DBAEFF>" + SessionService.GetInviteCode() + "</color>";
			copyCodeButton.gameObject.SetActive(value: true);
		}
	}

	private void GenerateCode()
	{
		SessionService.GenerateInviteCode();
		LoadCurrentCode();
		UIMultiplayerNotifications.ShowGenerateInviteCode();
	}

	private void CopyCode()
	{
		GUIUtility.systemCopyBuffer = SessionService.GetInviteCode();
		UIMultiplayerNotifications.ShowShareInviteCode();
	}

	public void OpenInviteOverlay()
	{
		PlatformLayer.Networking.OpenPlatformInviteOverlay();
	}

	private void OnFocused()
	{
		if (_hotkeySession == null)
		{
			_hotkeySession = _hotkeys.GetSessionOrEmpty().AddOrReplaceHotkeys("Back", "Select");
		}
	}

	private void OnUnfocused()
	{
		_hotkeySession?.Dispose();
		_hotkeySession = null;
	}

	private void OpenEpicInvite()
	{
		Focus(focus: false);
		epicMask?.SetAlpha(0f);
		heroSlotsController.Focus(focus: false, interactable: false);
		invitePlayersWindow.Show(delegate
		{
			Focus(focus: true);
		}, openEpicButton.transform as RectTransform);
	}

	private void ShowConfirmationBoxToEndSession()
	{
		if (FFSNetwork.IsOnline && !FFSNetwork.IsStartingUp)
		{
			UIConfirmationBoxManager.MainMenuInstance.ShowGenericConfirmation(LocalizationManager.GetTranslation("GUI_CONFIRMATION_END_SESSION"), null, EndSession, CancelEndSession, null, null, null, showHeader: true, enableSoftlockReport: true);
		}
	}

	public void EndSession()
	{
		Singleton<BoltVoiceChatService>.Instance.ShutdownVoiceConnection();
		SessionService.EndSession();
		Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.MultiplayerOnlineContainer);
	}

	private void CancelEndSession()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.MultiplayerOnlineContainer);
	}

	private void OnHostingEndedCallback()
	{
		selectPlayerPopup.Hide();
		friendListWindow.Hide();
		if (Singleton<SpecialUIProvider>.Instance.UIMultiplayerVoiceChat != null)
		{
			Singleton<SpecialUIProvider>.Instance.UIMultiplayerVoiceChat.Hide();
		}
		ShowOnlineOptions(show: false);
	}

	private void StartSession()
	{
		PlatformLayer.Networking.CheckNetworkAvailabilityAsync(delegate(bool isConnected)
		{
			if (isConnected)
			{
				PlatformLayer.Networking.CheckForPrivilegeValidityAsync(Privilege.Multiplayer, delegate(bool isMultiplayerValid)
				{
					if (isMultiplayerValid)
					{
						SessionService.StartSession(OnHostingStartedCallback, OnHostingEndedCallback);
					}
				}, PrivilegePlatform.AllExceptSwitch);
			}
		});
	}

	private void OnHostingStartedCallback()
	{
		ShowOnlineOptions(show: true);
		UIMultiplayerNotifications.ShowStartedSession();
		if (PlatformLayer.Instance.IsConsole)
		{
			PlatformLayer.Networking.JoinSession(null, null);
		}
	}

	private void ShowOnlineOptions(bool show)
	{
		if (show)
		{
			LoadCurrentCode();
			RefreshAssignedHeroes();
		}
		else
		{
			invitePlayersWindow.Hide();
			signInConfirmationBox.Hide();
		}
		contentScroll.verticalScrollbar.gameObject.SetActive(show);
		contentScroll.enabled = show;
		startSessionButton.gameObject.SetActive(!show);
		onlineOptionsGroup.gameObject.SetActive(show);
		onlineOptionsGroup.alpha = (show ? 1 : 0);
		onlineOptionsGroup.interactable = show;
		onlineOptionsGroup.blocksRaycasts = show;
		SelectFirstElement();
	}
}
