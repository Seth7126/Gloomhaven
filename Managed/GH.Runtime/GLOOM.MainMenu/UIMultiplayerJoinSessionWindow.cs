#define ENABLE_LOGS
using System.Threading.Tasks;
using FFSNet;
using I2.Loc;
using SM.Gamepad;
using SM.Utils;
using Script.GUI;
using Script.GUI.Controller;
using Script.GUI.Popups;
using Script.GUI.SMNavigation;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

[RequireComponent(typeof(UIWindow))]
public class UIMultiplayerJoinSessionWindow : UISubmenuGOWindow, IEscapable
{
	private const int MillisecondsMult = 1000;

	[SerializeField]
	private UIKeyboard controllerKeyboard;

	[SerializeField]
	private EscapableGameObject escapableGameObject;

	[SerializeField]
	private ControllerInputKeyboard keyboardBehaviour;

	[SerializeField]
	private GameObject controllerKeybaordTip;

	[Header("Invite code")]
	[SerializeField]
	private TMP_InputField inviteCodeInput;

	[SerializeField]
	private int minSizeSessionId = 1;

	[Header("Connection status")]
	[SerializeField]
	private GameObject statusPanel;

	[SerializeField]
	private TextLocalizedListener statusTitle;

	[SerializeField]
	private TextLocalizedListener statusText;

	[SerializeField]
	private LoopAnimator connectingAnimation;

	[SerializeField]
	private int joiningAbortTimer = 30;

	[Header("Hotkeys")]
	[SerializeField]
	private LongConfirmHandler _longConfirmHandler;

	[SerializeField]
	private Hotkey _confirmHotkey;

	[SerializeField]
	private TextMeshProUGUI _confirmHotkeyLabel;

	[SerializeField]
	private LocalHotkeys _hotkeys;

	[SerializeField]
	private LongPressHandler _cancelLongPressHandler;

	[SerializeField]
	private Hotkey _cancelHotkey;

	[SerializeField]
	private TMP_Text _cancelHotkeyLabel;

	[Header("Users")]
	[SerializeField]
	private GameObject partyPanel;

	[SerializeField]
	private TextMeshProUGUI partyName;

	[SerializeField]
	private MultiplayerUserState[] partyUsers;

	[Header("Confirm")]
	[SerializeField]
	private ExtendedButton continueButton;

	[SerializeField]
	private GUIAnimator m_EnableConfirmAnimator;

	[SerializeField]
	private GameObject m_HighlightConfirmButton;

	[SerializeField]
	private ControllerInputClickeable controllerContinueTip;

	[SerializeField]
	private bool m_IsAllowedToEscapeDuringSave;

	public static UIMultiplayerJoinSessionWindow s_This;

	private UnityAction continueAction;

	private ConnectionState connectionState;

	public UnityEvent OnStartJoin;

	public UnityEvent OnFailedJoin;

	public UnityEvent OnStartCancel;

	public UnityEvent OnFinishCancel;

	private Color? confirmTextColor;

	private IHotkeySession _hotkeySession;

	private SessionHotkey _enterHotkey;

	private SessionHotkey _editHotkey;

	private SimpleKeyActionHandlerBlocker _simpleKeyActionHandlerBlocker;

	private SimpleKeyActionHandlerBlocker _editNameKeyActionHandlerBlocker;

	private SimpleKeyActionHandlerBlocker _setNameKeyActionHandlerBlocker;

	private SimpleKeyActionHandlerBlocker _cancelBlocker;

	private bool _needToAbortSessionSearch;

	private bool _joiningSessionStarted;

	public bool IsAllowedToEscapeDuringSave => m_IsAllowedToEscapeDuringSave;

	public ConnectionState CurrentConnectionState => connectionState;

	private bool HasName => inviteCodeInput.text.IsNOTNullOrEmpty();

	protected override void Awake()
	{
		base.Awake();
		_editNameKeyActionHandlerBlocker = new SimpleKeyActionHandlerBlocker(isBlock: true);
		_setNameKeyActionHandlerBlocker = new SimpleKeyActionHandlerBlocker(isBlock: true);
		_cancelBlocker = new SimpleKeyActionHandlerBlocker(isBlock: true);
		if (!confirmTextColor.HasValue && !InputManager.GamePadInUse)
		{
			confirmTextColor = continueButton.buttonText.color;
		}
		inviteCodeInput.onValueChanged.AddListener(ValidateCode);
		showAnimator.OnAnimationFinished.AddListener(delegate
		{
			if (inviteCodeInput.text.IsNullOrEmpty())
			{
				inviteCodeInput.ActivateInputField();
				inviteCodeInput.Select();
			}
		});
		escapableGameObject.OnEscaped.AddListener(OnKeyboardEscape);
		if (InputManager.GamePadInUse)
		{
			InitGamepadInput();
			keyboardBehaviour.OnPlatformSpecificCallback += OnKeyboardEscape;
			_confirmHotkeyLabel.SetText(LocalizationManager.GetTranslation("GUI_SUBMIT"));
		}
		else
		{
			continueButton.onClick.AddListener(OnContinueButtonClicked);
		}
		SubscribeOnInputFieldSelect();
		if (_cancelLongPressHandler != null)
		{
			_cancelLongPressHandler.gameObject.SetActive(value: false);
		}
		if (_cancelHotkeyLabel != null)
		{
			_cancelHotkeyLabel.SetText(LocalizationManager.GetTranslation("GUI_CANCEL"));
		}
		I2.Loc.LocalizationManager.OnLocalizeEvent += OnChangeLocalization;
	}

	private void OnChangeLocalization()
	{
		if (_cancelHotkeyLabel != null)
		{
			_cancelHotkeyLabel.SetText(LocalizationManager.GetTranslation("GUI_CANCEL"));
		}
	}

	protected override void OnDestroy()
	{
		continueButton.onClick.RemoveAllListeners();
		escapableGameObject.OnEscaped.RemoveListener(OnKeyboardEscape);
		if (InputManager.GamePadInUse)
		{
			DisableGamepadInput();
			keyboardBehaviour.OnPlatformSpecificCallback -= OnKeyboardEscape;
		}
		else
		{
			continueButton.onClick.RemoveListener(OnContinueButtonClicked);
		}
		inviteCodeInput.onValueChanged.RemoveAllListeners();
		OnStartJoin.RemoveAllListeners();
		OnFailedJoin.RemoveAllListeners();
		OnStartCancel.RemoveAllListeners();
		OnFinishCancel.RemoveAllListeners();
		HideHotkeys();
		UnsubscribeOnInputFieldSelect();
		base.OnDestroy();
	}

	protected void OnEnable()
	{
		s_This = this;
		Clear(clearInput: true);
		if (PlatformLayer.Networking.HasInvitePending)
		{
			PlatformLayer.Networking.JoinOnPendingInvite(delegate(string sessionId)
			{
				JoinSession(sessionId);
			});
		}
		if (PlatformLayer.Networking.HasEPICInvitePending)
		{
			PlatformLayer.Networking.JoinOnPendingEPICInvite(delegate(string sessionId)
			{
				JoinSession(sessionId);
			});
		}
		if (NetworkManager.LocalMultiplayer)
		{
			inviteCodeInput.text = "Local Host Connect";
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		UIWindowManager.UnregisterEscapable(this);
		s_This = null;
		controllerKeyboard.Hide();
		InputManager.UnregisterToOnPressed(KeyAction.UI_ENTER, OnInputFieldPressed);
		InputManager.UnregisterToOnPressed(KeyAction.UI_EDIT, OnInputFieldPressed);
	}

	private void Update()
	{
		if (_needToAbortSessionSearch)
		{
			AbortSessionSearching();
			_needToAbortSessionSearch = false;
		}
	}

	private void AbortSessionSearching()
	{
		FFSNetwork.Manager.ConnectingToSession = false;
		FFSNetwork.Manager.CancelJoiningSession(delegate
		{
			OnConnectionFailed(ConnectionErrorCode.SessionNotFound);
		});
	}

	private void InitGamepadInput()
	{
		_simpleKeyActionHandlerBlocker = new SimpleKeyActionHandlerBlocker(isBlock: true);
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, OnContinueButtonClicked).AddBlocker(_simpleKeyActionHandlerBlocker));
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_CANCEL, OnCancelButtonClicked).AddBlocker(_cancelBlocker));
	}

	private void DisableGamepadInput()
	{
		if (Singleton<KeyActionHandlerController>.Instance != null)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_SUBMIT, OnContinueButtonClicked);
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_CANCEL, OnCancelButtonClicked);
		}
	}

	private void ShowJoinSessionHotkeys()
	{
		if (InputManager.GamePadInUse && _cancelLongPressHandler != null)
		{
			_cancelHotkey.Initialize(Singleton<UINavigation>.Instance.Input);
			_cancelLongPressHandler.gameObject.SetActive(value: true);
			_cancelBlocker.SetBlock(value: false);
			_editNameKeyActionHandlerBlocker.SetBlock(value: true);
		}
	}

	private void HideJoinSessionHotkeys()
	{
		if (_cancelLongPressHandler != null && _cancelLongPressHandler.IsActive)
		{
			_cancelHotkey.Deinitialize();
			_cancelHotkey.DisplayHotkey(active: false);
			_cancelLongPressHandler.gameObject.SetActive(value: false);
			_cancelBlocker.SetBlock(value: true);
			_editNameKeyActionHandlerBlocker.SetBlock(value: false);
		}
	}

	private void ShowKeyboardHotkeys()
	{
		_hotkeySession = _hotkeys.GetSessionOrEmpty().AddOrReplaceHotkeys("Back");
	}

	private void DisposeHotkeys()
	{
		if (_hotkeySession != null)
		{
			_hotkeySession.Dispose();
			_hotkeySession = null;
		}
	}

	protected void ShowHotkeys()
	{
		if (_hotkeySession == null)
		{
			_hotkeySession = _hotkeys.GetSessionOrEmpty().AddOrReplaceHotkeys("Back");
			_editHotkey = _hotkeySession.GetHotkey("Edit");
			_enterHotkey = _hotkeySession.GetHotkey("Enter");
		}
		if (InputManager.GamePadInUse)
		{
			ShowLongConfirm();
			_confirmHotkey.Initialize(Singleton<UINavigation>.Instance.Input);
			_simpleKeyActionHandlerBlocker.SetBlock(value: false);
			ChangeHotkeySelect();
		}
	}

	protected void HideHotkeys()
	{
		DisposeHotkeys();
		HideLongConfirm();
		if (InputManager.GamePadInUse)
		{
			_confirmHotkey.Deinitialize();
			_confirmHotkey.DisplayHotkey(active: false);
			_simpleKeyActionHandlerBlocker.SetBlock(value: true);
		}
	}

	private void ShowLongConfirm()
	{
		if (_longConfirmHandler != null)
		{
			_longConfirmHandler.gameObject.SetActive(value: true);
		}
	}

	private void HideLongConfirm()
	{
		if (_longConfirmHandler != null)
		{
			_longConfirmHandler.gameObject.SetActive(value: false);
		}
	}

	private void OnKeyboardEscape()
	{
		keyboardBehaviour.enabled = false;
		DisposeHotkeys();
		ShowHotkeys();
	}

	private void OnKeyboardShow()
	{
		keyboardBehaviour.enabled = true;
		HideHotkeys();
		ShowKeyboardHotkeys();
	}

	private void ChangeHotkeySelect()
	{
		bool hasName = HasName;
		_editHotkey.SetShown(hasName);
		_enterHotkey.SetShown(!hasName);
		_editNameKeyActionHandlerBlocker.SetBlock(!hasName);
		_setNameKeyActionHandlerBlocker.SetBlock(hasName);
		if (hasName)
		{
			_longConfirmHandler.gameObject.SetActive(!keyboardBehaviour.enabled);
		}
		else
		{
			_longConfirmHandler.gameObject.SetActive(value: false);
		}
	}

	private void SubscribeOnInputFieldSelect()
	{
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_EDIT, OnInputFieldPressed).AddBlocker(new ControllerAreaLocalFocusKeyActionHandlerBlocker(controllerArea)).AddBlocker(_editNameKeyActionHandlerBlocker));
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, OnInputFieldPressed).AddBlocker(new ControllerAreaLocalFocusKeyActionHandlerBlocker(controllerArea)).AddBlocker(_setNameKeyActionHandlerBlocker));
	}

	private void UnsubscribeOnInputFieldSelect()
	{
		if (Singleton<KeyActionHandlerController>.IsInitialized)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_EDIT, OnInputFieldPressed);
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_SUBMIT, OnInputFieldPressed);
		}
	}

	private void OnCancelButtonClicked()
	{
		if (InputManager.GamePadInUse && _cancelLongPressHandler != null && _cancelLongPressHandler.IsActive)
		{
			_cancelLongPressHandler.Pressed(Cancel);
		}
	}

	private void OnContinueButtonClicked()
	{
		if (InputManager.GamePadInUse)
		{
			if (_longConfirmHandler != null && _longConfirmHandler.gameObject.activeInHierarchy)
			{
				_longConfirmHandler.Pressed(delegate
				{
					continueAction?.Invoke();
				});
			}
		}
		else
		{
			continueAction?.Invoke();
		}
	}

	protected override void OnControllerFocused()
	{
		base.OnControllerFocused();
		if (connectionState == ConnectionState.None && !_joiningSessionStarted)
		{
			ShowHotkeys();
		}
	}

	protected override void OnControllerUnfocused()
	{
		base.OnControllerUnfocused();
		HideHotkeys();
		controllerKeyboard.Hide();
	}

	private void OnInputFieldPressed()
	{
		OnKeyboardShow();
		controllerKeyboard.Show();
	}

	private void ValidateCode(string code)
	{
		bool flag = code.Length >= minSizeSessionId;
		if (!InputManager.GamePadInUse)
		{
			if (flag && !continueButton.interactable)
			{
				m_EnableConfirmAnimator.Play();
			}
			EnableConfirmationButton(flag);
		}
	}

	private void Clear(bool clearInput)
	{
		Debug.Log("GUI: Join session screen reset.");
		UIWindowManager.UnregisterEscapable(this);
		connectionState = ConnectionState.None;
		continueAction = JoinSessionNULL;
		if (!InputManager.GamePadInUse)
		{
			continueButton.TextLanguageKey = "GUI_SUBMIT";
		}
		else
		{
			_confirmHotkeyLabel.SetText(LocalizationManager.GetTranslation("GUI_SUBMIT"));
		}
		inviteCodeInput.interactable = true;
		_joiningSessionStarted = false;
		statusPanel.SetActive(value: false);
		partyPanel.SetActive(value: false);
		connectingAnimation.StopLoop(resetToInitial: true);
		window.escapeKeyAction = UIWindow.EscapeKeyAction.HideOnlyThis;
		if (clearInput)
		{
			inviteCodeInput.text = string.Empty;
			inviteCodeInput.caretPosition = 0;
		}
		if (!InputManager.GamePadInUse)
		{
			EnableConfirmationButton(inviteCodeInput.text.Length >= minSizeSessionId);
		}
	}

	private void EnableConfirmationButton(bool interactable)
	{
		if (!interactable)
		{
			m_EnableConfirmAnimator.Stop();
		}
		if (!confirmTextColor.HasValue)
		{
			confirmTextColor = continueButton.buttonText.color;
		}
		continueButton.targetGraphic.material = (interactable ? null : UIInfoTools.Instance.disabledGrayscaleMaterial);
		continueButton.buttonText.color = (interactable ? confirmTextColor.Value : UIInfoTools.Instance.greyedOutTextColor);
		m_HighlightConfirmButton.SetActive(interactable);
	}

	public void Cancel()
	{
		UIWindowManager.UnregisterEscapable(this);
		window.escapeKeyAction = UIWindow.EscapeKeyAction.Skip;
		if (!InputManager.GamePadInUse)
		{
			EnableConfirmationButton(interactable: false);
		}
		HideJoinSessionHotkeys();
		OnStartCancel.Invoke();
		FFSNetwork.Manager.CancelJoiningSession(delegate
		{
			Clear(clearInput: false);
			OnFinishCancel.Invoke();
			ShowHotkeys();
		});
	}

	public void Retry()
	{
		Clear(clearInput: false);
	}

	private void JoinSessionNULL()
	{
		JoinSession();
	}

	public bool JoinSession(string sessionID = null)
	{
		Debug.Log("[UIMultiplayerJoinSessionWindow] JoinSession(" + sessionID + ") Called");
		if (!sessionID.IsNOTNullOrEmpty() && inviteCodeInput.text.Length <= 0)
		{
			return false;
		}
		if (!InputManager.GamePadInUse)
		{
			continueAction = Cancel;
			continueButton.TextLanguageKey = "GUI_CANCEL";
			continueButton.interactable = true;
			EnableConfirmationButton(interactable: true);
		}
		HideHotkeys();
		UIWindowManager.RegisterEscapable(this);
		window.escapeKeyAction = UIWindow.EscapeKeyAction.Skip;
		inviteCodeInput.interactable = false;
		_joiningSessionStarted = true;
		if (!InputManager.GamePadInUse)
		{
			controllerKeyboard.Hide();
		}
		statusTitle.SetTextKey("GUI_MULTIPLAYER_CONNECTING");
		statusTitle.Text.color = UIInfoTools.Instance.playerConnectingColor;
		connectingAnimation.StartLoop(resetToInitial: true);
		statusPanel.SetActive(value: true);
		statusText.SetTextKey("GUI_MULTIPLAYER_CONNECTING_SearchingForSession");
		sessionID = (sessionID.IsNOTNullOrEmpty() ? sessionID : inviteCodeInput.text.ToUpper());
		OnStartJoin.Invoke();
		FFSNetwork.Manager.JoinSession(sessionID, OnConnectionEstablished, OnConnectionStateUpdated, OnConnectionFailed, OnDisconnected);
		new Task(delegate
		{
			JoiningAbortTimer(joiningAbortTimer * 1000);
		}).Start();
		return true;
	}

	private async void JoiningAbortTimer(int milliseconds)
	{
		await Task.Delay(milliseconds);
		if (connectionState == ConnectionState.SearchingForSession)
		{
			_needToAbortSessionSearch = true;
		}
	}

	private void OnConnectionEstablished(CustomDataToken gameData)
	{
		HideJoinSessionHotkeys();
		Debug.Log("GUI: OnConnectionEstablished");
		connectionState = ConnectionState.None;
		connectingAnimation.StopLoop(resetToInitial: true);
		continueAction = null;
		statusTitle.SetTextKey("GUI_MULTIPLAYER_INVITE_ACCEPTED");
		statusTitle.Text.color = UIInfoTools.Instance.playerOnlineColor;
		statusText.SetTextKey("GUI_MULTIPLAYER_JOINING_SESSION");
		partyName.text = ((GameToken)gameData).SaveName;
		if (PlatformLayer.Instance.IsConsole)
		{
			FFSNetwork.StartPSPlayerSessionNegotiation(OnSessionReceived);
		}
	}

	private void OnSessionReceived(FFSNetwork.SessionReceivedResult receivedResult, string sessionCode)
	{
		if (receivedResult == FFSNetwork.SessionReceivedResult.Success)
		{
			PlatformLayer.Networking.JoinSession(sessionCode, null);
		}
		else
		{
			LogUtils.LogError($"Failed to receive session {receivedResult} {sessionCode}");
		}
	}

	private void OnConnectionStateUpdated(ConnectionState connectionState)
	{
		Debug.Log("GUI: OnConnectionStateUpdated (" + connectionState.ToString() + ") " + PlayerRegistry.AllPlayers.IsNullOrEmpty());
		this.connectionState = connectionState;
		statusText.SetTextKey($"GUI_MULTIPLAYER_CONNECTING_{connectionState}");
		switch (connectionState)
		{
		case ConnectionState.SessionFound:
			HideJoinSessionHotkeys();
			ShowPlayers();
			break;
		case ConnectionState.SearchingForSession:
			ShowJoinSessionHotkeys();
			break;
		}
	}

	private void ShowPlayers()
	{
		for (int i = 0; i < PlayerRegistry.AllPlayers.Count; i++)
		{
			partyUsers[i].Show(PlayerRegistry.AllPlayers[i], !PlayerRegistry.IsJoining(PlayerRegistry.AllPlayers[i]));
		}
		for (int j = PlayerRegistry.AllPlayers.Count; j < partyUsers.Length; j++)
		{
			partyUsers[j].Hide();
		}
		partyPanel.SetActive(value: true);
	}

	public void OnConnectionFailed(ConnectionErrorCode errorCode)
	{
		Debug.Log("GUI: OnConnectionFailed " + errorCode);
		ShowConnectionError(errorCode.ToString());
		PlatformLayer.Networking.LeaveSession(null);
		window.escapeKeyAction = UIWindow.EscapeKeyAction.HideOnlyThis;
		OnFailedJoin.Invoke();
		HideJoinSessionHotkeys();
		ShowHotkeys();
	}

	private void ShowConnectionError(string errorCode)
	{
		if (!InputManager.GamePadInUse)
		{
			continueAction = Retry;
			continueButton.TextLanguageKey = "GUI_RETRY";
		}
		else
		{
			_confirmHotkeyLabel.SetText(LocalizationManager.GetTranslation("GUI_RETRY"));
		}
		statusTitle.Text.color = UIInfoTools.Instance.warningColor;
		if (errorCode == ConnectionErrorCode.CrossplayDisabledByClient.ToString() || errorCode == ConnectionErrorCode.CrossplayDisabledByServer.ToString() || errorCode == ConnectionErrorCode.NotEnoughMemory.ToString() || errorCode == ConnectionErrorCode.MultiplayerValidationFail.ToString() || errorCode == ConnectionErrorCode.UserInSessionBlockedByCurrentUser.ToString() || errorCode == ConnectionErrorCode.AuthenticationFail.ToString() || errorCode == ConnectionErrorCode.SessionCouldNotBeJoined.ToString() || errorCode == ConnectionErrorCode.CurrentUserBlockedByUserInSession.ToString() || errorCode == ConnectionErrorCode.DLCDoNotMatchJotl.ToString() || errorCode == ConnectionErrorCode.DLCDoNotMatchSoloScenarious.ToString() || errorCode == ConnectionErrorCode.DLCDoNotMatchJotlAndSoloScenarious.ToString() || errorCode == ConnectionErrorCode.SessionShutDown.ToString())
		{
			statusTitle.SetTextKey("GUI_MULTIPLAYER_CONNECTION_FAILED");
			statusText.SetTextKey("Consoles/GUI_MULTIPLAYER_CONNECTION_FAILED_" + errorCode);
		}
		else
		{
			statusTitle.SetTextKey("GUI_MULTIPLAYER_CONNECTION_FAILED");
			statusText.SetTextKey("GUI_MULTIPLAYER_CONNECTION_FAILED_" + errorCode);
		}
		connectingAnimation.StopLoop(resetToInitial: true);
	}

	private void OnDisconnected(DisconnectionErrorCode errorCode)
	{
		if (connectionState.In(ConnectionState.WaitUntilSavePoint, ConnectionState.SavePointReached, ConnectionState.DownloadingNewSave, ConnectionState.Connecting))
		{
			ShowConnectionError(errorCode.ToString());
		}
		else if (connectionState != ConnectionState.None && errorCode == DisconnectionErrorCode.HostEndedSession)
		{
			Clear(clearInput: true);
			string explanation = string.Format(LocalizationManager.GetTranslation("GUI_MULTIPLAYER_HOST_DISCONNECTED_CONFIRMATION"), PlayerRegistry.HostPlayer.UserNameWithPlatformIcon());
			UIConfirmationBoxManager.MainMenuInstance.ShowGenericCancelConfirmation(LocalizationManager.GetTranslation("GUI_MULTIPLAYER_HOST_DISCONNECTED_CONFIRMATION_TITLE"), explanation, "GUI_CONTINUE");
		}
		PlatformLayer.Networking.LeaveSession(null);
		HideJoinSessionHotkeys();
		window.escapeKeyAction = UIWindow.EscapeKeyAction.HideOnlyThis;
		OnFailedJoin.Invoke();
	}

	public bool Escape()
	{
		Cancel();
		return true;
	}

	public int Order()
	{
		return 0;
	}
}
