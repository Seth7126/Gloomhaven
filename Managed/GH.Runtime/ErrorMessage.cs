using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FFSNet;
using GLOOM;
using Ionic.Zip;
using Ionic.Zlib;
using JetBrains.Annotations;
using Manatee.Trello;
using SM.Gamepad;
using Script.GUI;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.PopupStates;
using SharedLibrary.SimpleLog;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ErrorMessage : MonoBehaviour, IEscapable
{
	public class LabelAction
	{
		public string ButtonLabelKey;

		public ErrorDelegate ButtonAction;

		public KeyAction KeyAction;

		public LabelAction(string buttonLabelKey, ErrorDelegate buttonAction, KeyAction keyAction)
		{
			ButtonLabelKey = buttonLabelKey;
			ButtonAction = buttonAction;
			KeyAction = keyAction;
		}
	}

	public delegate void ErrorDelegate();

	[SerializeField]
	private ControllerInputAreaLocal m_ControllerArea;

	public TextMeshProUGUI TitleText;

	public TextMeshProUGUI MessageText;

	public TextMeshProUGUI DiscordText;

	public TextMeshProUGUI InputBoxPlaceholder;

	public TextMeshProUGUI InputBoxText;

	public TextMeshProUGUI SendingErrorReportText;

	public GameObject ErrorMessageGO;

	public GameObject TitleGO;

	public GameObject MessageGO;

	public GameObject MessageButtonsGO;

	public GameObject CreateErrorReportButtonGO;

	public GameObject ErrorReportDialogGO;

	public GameObject SendErrorReportButtonsGO;

	public GameObject SendingErrorReportTextGO;

	public GameObject ButtonsLayoutGroup;

	public GameObject ButtonPrefab;

	[Header("Progress Bar")]
	[SerializeField]
	private Slider m_Slider;

	public const float c_SliderMaxValue = 100f;

	[SerializeField]
	private bool m_IsAllowedToEscapeDuringSave;

	[Header("Hotkeys")]
	[SerializeField]
	private KeyActionBindingCollection _keyActionBindingCollection;

	[SerializeField]
	private LocalHotkeys _baseHotkeys;

	[SerializeField]
	private LocalHotkeys _createErrorReportHotkeys;

	[SerializeField]
	private LocalHotkeys _sendErrorReportHotkeys;

	private ErrorDelegate m_ButtonAction;

	private Dictionary<ExtendedButton, ErrorDelegate> m_ActiveButtonActions = new Dictionary<ExtendedButton, ErrorDelegate>();

	private ExtendedButton m_UndoButton;

	private string m_ErrorTitleKey;

	private string m_ErrorMessageKey;

	private string m_ErrorStack;

	private string m_ExceptionMessage;

	private bool m_GameObjectState;

	private bool m_SuppressShowingErrorMessages;

	private bool m_ErrorReportSent;

	private bool m_DebugErrorReport;

	private bool m_IsShowing;

	private UnityAction<bool> m_OnClosed;

	private ControllerInputAreaLocal _controllerInputAreaLocal;

	private SkipFrameKeyActionHandlerBlocker _skipFrameKeyActionHandlerBlocker;

	private readonly List<KeyActionHandler> _keyActionHandlers = new List<KeyActionHandler>();

	private readonly List<IHotkeySession> _activeSessions = new List<IHotkeySession>();

	private IHotkeySession _sendErrorReportSession;

	public bool IsAllowedToEscapeDuringSave => m_IsAllowedToEscapeDuringSave;

	public bool ShowingMessage
	{
		get
		{
			if (!m_GameObjectState)
			{
				return base.gameObject.activeInHierarchy;
			}
			return true;
		}
	}

	public bool CheckShowingMessageFromThread => m_GameObjectState;

	public bool SuppressShowingErrorMessages
	{
		get
		{
			return m_SuppressShowingErrorMessages;
		}
		set
		{
			m_SuppressShowingErrorMessages = value;
		}
	}

	private void Awake()
	{
		_controllerInputAreaLocal = GetComponent<ControllerInputAreaLocal>();
		_controllerInputAreaLocal.OnFocusedArea.AddListener(OnFocused);
		_controllerInputAreaLocal.OnUnfocusedArea.AddListener(OnUnfocused);
		_skipFrameKeyActionHandlerBlocker = new SkipFrameKeyActionHandlerBlocker(this);
	}

	private void OnFocused()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(PopupStateTag.ErrorMessage);
	}

	private void OnUnfocused()
	{
		Singleton<UINavigation>.Instance.StateMachine.ToPreviousState();
	}

	private void OnEnable()
	{
		_controllerInputAreaLocal.Enable();
		_skipFrameKeyActionHandlerBlocker.Run();
		InputBoxPlaceholder.text = LocalizationManager.GetTranslation("GUI_ERROR_REPORT_PLACEHOLDER");
		SendingErrorReportText.text = LocalizationManager.GetTranslation("GUI_SENDING_ERROR_REPORT");
		DiscordText.text = LocalizationManager.GetTranslation("ERROR_DISCORD");
		SimpleLog.WriteSimpleLogToFile();
	}

	private void OnDisable()
	{
		_controllerInputAreaLocal.Destroy();
		UIWindowManager.UnregisterEscapable(this);
	}

	private void DisposeKeyActionHandlers()
	{
		foreach (KeyActionHandler keyActionHandler in _keyActionHandlers)
		{
			UnsubscribeOnKeyActionHandler(keyActionHandler);
		}
		_keyActionHandlers.Clear();
	}

	private void ClearHotkeySessions()
	{
		foreach (IHotkeySession activeSession in _activeSessions)
		{
			activeSession.Dispose();
		}
		_activeSessions.Clear();
		DisposeKeyActionHandlers();
	}

	private void OnDestroy()
	{
		foreach (KeyValuePair<ExtendedButton, ErrorDelegate> activeButtonAction in m_ActiveButtonActions)
		{
			if (activeButtonAction.Key != null)
			{
				activeButtonAction.Key.onClick.RemoveAllListeners();
			}
		}
		m_ActiveButtonActions.Clear();
	}

	public void UpdateProgressBar(float amount)
	{
		amount = Math.Min(amount, 100f);
		m_Slider.value = amount;
	}

	public void IncrementProgressBar(float amount)
	{
		amount = Math.Min(amount, 100f);
		m_Slider.value += amount;
	}

	public void ToggleErrorReportDialog(bool active)
	{
		UIWindowManager.UnregisterEscapable(this);
		m_Slider.gameObject.SetActive(value: false);
		if (m_DebugErrorReport)
		{
			base.gameObject.SetActive(value: false);
			m_GameObjectState = false;
		}
		else
		{
			TitleGO.SetActive(!active);
			MessageGO.SetActive(!active);
			MessageButtonsGO.SetActive(!active);
			if (m_ErrorReportSent)
			{
				CreateErrorReportButtonGO.SetActive(value: false);
			}
			else
			{
				CreateErrorReportButtonGO.SetActive(!active && !PlatformLayer.Instance.IsConsole);
			}
			ErrorReportDialogGO.SetActive(active);
			DiscordText.gameObject.SetActive(!active);
			SendErrorReportButtonsGO.SetActive(active && !PlatformLayer.Instance.IsConsole);
			if (active)
			{
				IHotkeySession sessionOrEmpty = _sendErrorReportHotkeys.GetSessionOrEmpty();
				if (!(sessionOrEmpty is EmptyHotkeySession))
				{
					foreach (KeyActionHandler keyActionHandler in _keyActionHandlers)
					{
						UnsubscribeOnKeyActionHandler(keyActionHandler);
					}
					_sendErrorReportSession = sessionOrEmpty;
					sessionOrEmpty.AddOrReplaceHotkey(_keyActionBindingCollection.GetInputDisplayName(KeyAction.UI_SUBMIT), null);
					AddKeyActionHandler(KeyAction.UI_SUBMIT, SendErrorReport);
				}
			}
			else
			{
				foreach (KeyActionHandler keyActionHandler2 in _keyActionHandlers)
				{
					UnsubscribeOnKeyActionHandler(keyActionHandler2);
					SubscribeOnKeyActionHandler(keyActionHandler2);
				}
				_sendErrorReportSession?.Dispose();
				_sendErrorReportSession = null;
			}
			SendingErrorReportTextGO.SetActive(value: false);
			if (active)
			{
				UIWindowManager.RegisterEscapable(this);
				m_ControllerArea.Enable();
			}
		}
		if (!active)
		{
			m_ControllerArea.Destroy();
			m_OnClosed?.Invoke(m_ErrorReportSent);
		}
	}

	public void ShowMessageDefaultTitle(string errorMessageKey, string buttonLabelKey, string errorStack, ErrorDelegate action, string exceptionMessage = null, bool showErrorReportButton = true, bool trackDebug = true)
	{
		ShowMessage(errorMessageKey, buttonLabelKey, errorStack, "GUI_ERROR_TITLE", action, exceptionMessage, showErrorReportButton, trackDebug);
	}

	public void ShowMessage(string errorMessageKey, string buttonLabelKey, string errorStack, string errorTitleKey, ErrorDelegate action, string exceptionMessage = null, bool showErrorReportButton = true, bool trackDebug = true)
	{
		InputManager.RequestEnableInput(SceneController.Instance, EKeyActionTag.All);
		if (ShowingMessage || SuppressShowingErrorMessages)
		{
			return;
		}
		m_OnClosed = null;
		m_ErrorTitleKey = errorTitleKey;
		m_ErrorMessageKey = errorMessageKey;
		m_ExceptionMessage = exceptionMessage;
		m_ErrorStack = errorStack;
		m_ErrorReportSent = false;
		m_DebugErrorReport = false;
		ToggleErrorReportDialog(active: false);
		CreateErrorReportButtonGO.SetActive(showErrorReportButton && !PlatformLayer.Instance.IsConsole);
		SendErrorReportButtonsGO.SetActive(value: false);
		string translation = LocalizationManager.GetTranslation(errorTitleKey);
		string translation2 = LocalizationManager.GetTranslation(errorMessageKey);
		LocalizationManager.GetTranslation(buttonLabelKey);
		LevelEditorController s_Instance = LevelEditorController.s_Instance;
		if ((object)s_Instance != null && s_Instance.BulkLevelProcessingInProgress)
		{
			Debug.LogError(translation2);
			LevelEditorController.s_Instance.BulkProcessingErrorOccurred = true;
			return;
		}
		if (AutoTestController.s_AutoLogPlaybackInProgress)
		{
			AutoTestController.s_Instance.EndTestPlaybackFromErrorMessage(translation2);
		}
		else
		{
			if (SaveData.Instance.GameBootedForAutoTests)
			{
				Debug.LogError(translation2);
				AutoTestController.s_Instance.TestErrorOccurred = true;
				return;
			}
			if (!ShowingMessage)
			{
				TitleText.text = translation;
				MessageText.text = translation2;
				InitializeButton(action, KeyAction.UI_SUBMIT, buttonLabelKey, enableCancelButtonCheck: false, ProcessingSession(showErrorReportButton));
				m_UndoButton = null;
				base.gameObject.SetActive(value: true);
				m_GameObjectState = true;
				if (trackDebug)
				{
					translation = LocalizationManager.GetTranslation(errorTitleKey, FixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, "English");
					translation2 = ((exceptionMessage != null) ? (translation2 + ": " + exceptionMessage) : LocalizationManager.GetTranslation(errorMessageKey, FixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, "English"));
				}
			}
		}
		if (ShowingMessage)
		{
			CoroutineHelper.RunCoroutine(GloomUtility.TakeErrorScreenshot(ErrorMessageGO));
			SimpleLog.WriteSimpleLogToFile();
		}
	}

	[CanBeNull]
	private IHotkeySession ProcessingSession(bool showErrorReportHotkey)
	{
		if (!InputManager.GamePadInUse)
		{
			return null;
		}
		if (_baseHotkeys == null || _createErrorReportHotkeys == null)
		{
			throw new NullReferenceException("Some of hotkeys sessions is null or missing");
		}
		ClearHotkeySessions();
		if (showErrorReportHotkey)
		{
			IHotkeySession sessionOrEmpty = _createErrorReportHotkeys.GetSessionOrEmpty();
			if (!(sessionOrEmpty is EmptyHotkeySession))
			{
				sessionOrEmpty.AddOrReplaceHotkey(_keyActionBindingCollection.GetInputDisplayName(KeyAction.UI_RETRY), null);
				AddKeyActionHandler(KeyAction.UI_RETRY, delegate
				{
					ToggleErrorReportDialog(active: true);
				});
				_activeSessions.Add(sessionOrEmpty);
			}
		}
		IHotkeySession sessionOrEmpty2 = _baseHotkeys.GetSessionOrEmpty();
		_activeSessions.Add(sessionOrEmpty2);
		return sessionOrEmpty2;
	}

	private void CreateButtons(IEnumerable<LabelAction> buttons, bool showErrorReportButton)
	{
		IHotkeySession hotkeySession = ProcessingSession(showErrorReportButton);
		foreach (LabelAction button in buttons)
		{
			InitializeButton(button.ButtonAction, button.KeyAction, button.ButtonLabelKey, enableCancelButtonCheck: true, hotkeySession);
		}
	}

	private void AddKeyActionHandler(KeyAction keyAction, System.Action action)
	{
		KeyActionHandler keyActionHandler = new KeyActionHandler(keyAction, action);
		keyActionHandler.AddBlocker(new ControllerAreaLocalFocusKeyActionHandlerBlocker(_controllerInputAreaLocal)).AddBlocker(_skipFrameKeyActionHandlerBlocker);
		SubscribeOnKeyActionHandler(keyActionHandler);
		_keyActionHandlers.Add(keyActionHandler);
	}

	private void SubscribeOnKeyActionHandler(KeyActionHandler keyActionHandler)
	{
		Singleton<KeyActionHandlerController>.Instance.AddHandler(keyActionHandler);
	}

	private void InitializeButton(ErrorDelegate action, KeyAction keyAction, string buttonLabelKey, bool enableCancelButtonCheck = false, IHotkeySession hotkeySession = null)
	{
		if (hotkeySession != null && !(hotkeySession is EmptyHotkeySession))
		{
			if (keyAction == KeyAction.CONFIRM_ACTION_BUTTON)
			{
				keyAction = KeyAction.UI_CANCEL;
			}
			hotkeySession.AddOrReplaceHotkey(_keyActionBindingCollection.GetInputDisplayName(keyAction, buttonLabelKey), null);
			AddKeyActionHandler(keyAction, delegate
			{
				FireAction(action);
			});
			if (keyAction == KeyAction.UI_CANCEL)
			{
				UIWindowManager.RegisterEscapable(this);
			}
			return;
		}
		ExtendedButton newButton = InstantiateExtendedButton();
		if ((bool)newButton)
		{
			newButton.buttonText.text = LocalizationManager.GetTranslation(buttonLabelKey);
			newButton.onClick.AddListener(delegate
			{
				OnButtonClick(newButton);
			});
			ControllerInputClickeable controllerInputClickeable = newButton.GetComponent<ControllerInputClickeable>();
			if (controllerInputClickeable == null)
			{
				controllerInputClickeable = newButton.gameObject.AddComponent<ControllerInputClickeable>();
			}
			controllerInputClickeable.SetKeyAction(keyAction, !enableCancelButtonCheck || keyAction != KeyAction.UI_CANCEL);
			if (enableCancelButtonCheck && keyAction == KeyAction.UI_CANCEL)
			{
				m_UndoButton = newButton;
				UIWindowManager.RegisterEscapable(this);
			}
			m_ActiveButtonActions.Add(newButton, action);
		}
	}

	public void ShowMultiChoiceMessageDefaultTitle(string errorMessageKey, string errorStack, List<LabelAction> buttons, string exceptionMessage = null)
	{
		ShowMultiChoiceMessage(errorMessageKey, buttons, "GUI_ERROR_TITLE", errorStack, exceptionMessage);
	}

	public void ShowMultiChoiceMessage(string errorMessageKey, List<LabelAction> buttons, string errorTitleKey, string errorStack, string exceptionMessage = null, bool showErrorReportButton = true, bool trackDebug = true)
	{
		if (ShowingMessage || SuppressShowingErrorMessages)
		{
			return;
		}
		m_OnClosed = null;
		m_ErrorTitleKey = errorTitleKey;
		m_ErrorMessageKey = errorMessageKey;
		m_ExceptionMessage = exceptionMessage;
		m_ErrorStack = errorStack;
		m_ErrorReportSent = false;
		m_DebugErrorReport = false;
		ToggleErrorReportDialog(active: false);
		CreateErrorReportButtonGO.SetActive(showErrorReportButton && !PlatformLayer.Instance.IsConsole);
		SendErrorReportButtonsGO.SetActive(value: false);
		string translation = LocalizationManager.GetTranslation(errorTitleKey);
		string translation2 = LocalizationManager.GetTranslation(errorMessageKey);
		LevelEditorController s_Instance = LevelEditorController.s_Instance;
		if ((object)s_Instance != null && s_Instance.BulkLevelProcessingInProgress)
		{
			Debug.LogError(translation2);
			LevelEditorController.s_Instance.BulkProcessingErrorOccurred = true;
			return;
		}
		if (AutoTestController.s_AutoLogPlaybackInProgress)
		{
			AutoTestController.s_Instance.EndTestPlaybackFromErrorMessage(translation2);
		}
		else
		{
			if (SaveData.Instance.GameBootedForAutoTests)
			{
				Debug.LogError(translation2);
				AutoTestController.s_Instance.TestErrorOccurred = true;
				return;
			}
			if (!ShowingMessage)
			{
				TitleText.text = translation;
				MessageText.text = translation2;
				UIWindowManager.UnregisterEscapable(this);
				CreateButtons(buttons, showErrorReportButton);
				base.gameObject.SetActive(value: true);
				m_GameObjectState = true;
				if (trackDebug)
				{
					translation = LocalizationManager.GetTranslation(errorTitleKey, FixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, "English");
					translation2 = ((exceptionMessage != null) ? (translation2 + ": " + exceptionMessage) : LocalizationManager.GetTranslation(errorMessageKey, FixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, "English"));
				}
			}
		}
		if (ShowingMessage)
		{
			CoroutineHelper.RunCoroutine(GloomUtility.TakeErrorScreenshot(ErrorMessageGO));
			SimpleLog.WriteSimpleLogToFile();
		}
	}

	public void ShowGenericMessage(string titleKey, string errorMessageKey, string errorStack, List<LabelAction> buttons, string exceptionMessage = null)
	{
		if (!ShowingMessage && !SuppressShowingErrorMessages)
		{
			m_OnClosed = null;
			m_ErrorTitleKey = titleKey;
			m_ErrorMessageKey = errorMessageKey;
			m_ExceptionMessage = exceptionMessage;
			m_ErrorStack = errorStack;
			m_ErrorReportSent = false;
			m_DebugErrorReport = false;
			ToggleErrorReportDialog(active: false);
			CreateErrorReportButtonGO.SetActive(value: false);
			string translation = LocalizationManager.GetTranslation(errorMessageKey);
			LevelEditorController s_Instance = LevelEditorController.s_Instance;
			if ((object)s_Instance != null && s_Instance.BulkLevelProcessingInProgress)
			{
				Debug.LogError(translation);
				LevelEditorController.s_Instance.BulkProcessingErrorOccurred = true;
			}
			else if (AutoTestController.s_AutoLogPlaybackInProgress)
			{
				base.gameObject.SetActive(value: false);
				m_GameObjectState = false;
			}
			else if (SaveData.Instance.GameBootedForAutoTests)
			{
				Debug.LogError(translation);
				AutoTestController.s_Instance.TestErrorOccurred = true;
			}
			else if (!ShowingMessage)
			{
				TitleText.text = LocalizationManager.GetTranslation(titleKey);
				MessageText.text = translation;
				UIWindowManager.UnregisterEscapable(this);
				CreateButtons(buttons, showErrorReportButton: false);
				base.gameObject.SetActive(value: true);
				m_GameObjectState = true;
			}
		}
	}

	private ExtendedButton InstantiateExtendedButton()
	{
		GameObject obj = UnityEngine.Object.Instantiate(ButtonPrefab, ButtonsLayoutGroup.transform);
		obj.SetActive(value: true);
		return obj.GetComponentInChildren<ExtendedButton>();
	}

	public void ShowGenericDebugMessage(string title, string messageText, List<LabelAction> buttons)
	{
		if (!ShowingMessage && !SuppressShowingErrorMessages)
		{
			m_ErrorReportSent = false;
			m_DebugErrorReport = false;
			ToggleErrorReportDialog(active: false);
			CreateErrorReportButtonGO.SetActive(value: false);
			if (!ShowingMessage)
			{
				UIWindowManager.UnregisterEscapable(this);
				TitleText.text = title;
				MessageText.text = messageText;
				CreateButtons(buttons, showErrorReportButton: false);
				base.gameObject.SetActive(value: true);
				m_GameObjectState = true;
			}
		}
	}

	private void UnsubscribeOnKeyActionHandler(KeyActionHandler keyActionHandler)
	{
		if (keyActionHandler != null)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(keyActionHandler);
		}
	}

	private void FireAction(ErrorDelegate errorDelegate)
	{
		if (errorDelegate != null)
		{
			ClearHotkeySessions();
			DisposeButtons();
			errorDelegate();
		}
	}

	public void OnButtonClick(ExtendedButton buttonClicked)
	{
		if (m_ActiveButtonActions.TryGetValue(buttonClicked, out m_ButtonAction))
		{
			FireAction(m_ButtonAction);
		}
	}

	private void DisposeButtons()
	{
		if (!InputManager.GamePadInUse)
		{
			DestroyExtendedButtons();
		}
		base.gameObject.SetActive(value: false);
		UIWindowManager.UnregisterEscapable(this);
		m_GameObjectState = false;
	}

	private void DestroyExtendedButtons()
	{
		foreach (ExtendedButton key in m_ActiveButtonActions.Keys)
		{
			UnityEngine.Object.Destroy(key.transform.parent.gameObject);
		}
		m_ActiveButtonActions.Clear();
		m_UndoButton = null;
	}

	public void Hide()
	{
		UIWindowManager.UnregisterEscapable(this);
		base.gameObject.SetActive(value: false);
		m_GameObjectState = false;
	}

	public void CloseErrorMessage()
	{
		m_ActiveButtonActions.Clear();
		base.gameObject.SetActive(value: false);
		m_GameObjectState = false;
		m_UndoButton = null;
		UIWindowManager.UnregisterEscapable(this);
		FFSNetwork.Shutdown();
	}

	private void SaveCurrentSessions()
	{
	}

	public void ShowSendErrorReport(string errorMessageKey, UnityAction<bool> onFinished = null)
	{
		m_OnClosed = onFinished;
		m_ErrorMessageKey = errorMessageKey;
		m_ExceptionMessage = null;
		m_ErrorStack = Environment.StackTrace;
		m_ErrorReportSent = false;
		m_DebugErrorReport = false;
		ToggleErrorReportDialog(active: true);
		m_DebugErrorReport = true;
		CoroutineHelper.RunCoroutine(GloomUtility.TakeErrorScreenshot(ErrorMessageGO));
	}

	public void OnSendErrorReportClick()
	{
		SendErrorReport();
	}

	private async Task GetSimpleLogFromHost()
	{
		if (File.Exists(RootSaveData.SimpleLogPathFromHost))
		{
			File.Delete(RootSaveData.SimpleLogPathFromHost);
		}
		GHClientCallbacks.CurrentStreamMode = GHClientCallbacks.EStreamMode.ReceivingSimpleLog;
		Synchronizer.RequestCustomData(DataActionType.SendSimpleLogFromHost);
		int currentWaitTime = 0;
		while (!File.Exists(RootSaveData.SimpleLogPathFromHost) && currentWaitTime < 10000)
		{
			await Task.Delay(100);
			currentWaitTime += 100;
		}
	}

	private async Task GetPlayerLogFromHost()
	{
		if (File.Exists(RootSaveData.PlayerLogPathFromHost))
		{
			File.Delete(RootSaveData.PlayerLogPathFromHost);
		}
		GHClientCallbacks.CurrentStreamMode = GHClientCallbacks.EStreamMode.ReceivingPlayerLog;
		Synchronizer.RequestCustomData(DataActionType.SendPlayerLogFromHost);
		int currentWaitTime = 0;
		while (!File.Exists(RootSaveData.PlayerLogPathFromHost) && currentWaitTime < 10000)
		{
			await Task.Delay(100);
			currentWaitTime += 100;
		}
	}

	private async Task GetSimpleLogsFromClients()
	{
		List<string> list = (from s in PlayerRegistry.AllPlayers
			where s.IsClient && s.PlayerID != PlayerRegistry.MyPlayer.PlayerID
			select s.Username).ToList();
		List<string> clientSimpleLogPaths = list.Select((string s) => RootSaveData.SimpleLogFromClientPath(s)).ToList();
		RootSaveData.CleanClientSimpleLogs();
		if (list.Count <= 0)
		{
			return;
		}
		Synchronizer.RequestCustomDataFromClients(DataActionType.SendSimpleLogFromClient);
		for (int currentWaitTime = 0; currentWaitTime < 15000; currentWaitTime += 100)
		{
			bool flag = true;
			foreach (string item in clientSimpleLogPaths)
			{
				if (!File.Exists(item))
				{
					flag = false;
				}
			}
			if (flag)
			{
				break;
			}
			await Task.Delay(100);
		}
	}

	private async Task GetPlayerLogsFromClients()
	{
		List<string> list = (from s in PlayerRegistry.AllPlayers
			where s.IsClient && s.PlayerID != PlayerRegistry.MyPlayer.PlayerID
			select s.Username).ToList();
		List<string> clientPlayerLogPaths = list.Select((string s) => RootSaveData.PlayerLogFromClientPath(s)).ToList();
		RootSaveData.CleanClientPlayerLogs();
		if (list.Count <= 0)
		{
			return;
		}
		Synchronizer.RequestCustomDataFromClients(DataActionType.SendPlayerLogFromClient);
		for (int currentWaitTime = 0; currentWaitTime < 15000; currentWaitTime += 100)
		{
			bool flag = true;
			foreach (string item in clientPlayerLogPaths)
			{
				if (!File.Exists(item))
				{
					flag = false;
				}
			}
			if (flag)
			{
				break;
			}
			await Task.Delay(100);
		}
	}

	public async void SendErrorReport()
	{
		_ = 11;
		try
		{
			if (m_DebugErrorReport && RootSaveData.ReleaseVersionType != RootSaveData.EReleaseTypes.Release)
			{
				UIWindowManager.UnregisterEscapable(this);
				string userText = InputBoxText.text;
				SendingErrorReportText.text = LocalizationManager.GetTranslation("GUI_SENDING_ERROR_REPORT");
				SendingErrorReportTextGO.SetActive(value: true);
				ErrorReportDialogGO.SetActive(value: false);
				SendErrorReportButtonsGO.SetActive(value: false);
				m_Slider.gameObject.SetActive(value: true);
				UpdateProgressBar(0f);
				if (FFSNetwork.IsOnline && FFSNetwork.IsClient)
				{
					await GetSimpleLogFromHost();
					await GetSimpleLogsFromClients();
					await GetPlayerLogFromHost();
					await GetPlayerLogsFromClients();
					FFSNetwork.Shutdown();
				}
				else if (FFSNetwork.IsOnline && FFSNetwork.IsHost)
				{
					await GetSimpleLogsFromClients();
					await GetPlayerLogsFromClients();
					FFSNetwork.Shutdown();
				}
				if (m_DebugErrorReport && RootSaveData.ReleaseVersionType == RootSaveData.EReleaseTypes.Release)
				{
					m_ErrorMessageKey += "_RELEASE";
				}
				string cardName = m_ErrorMessageKey + ((SaveData.Instance.Global?.CurrentAdventureData?.IsModded == true) ? "_MODDED" : "") + "\n" + Application.version + "\n" + PlatformLayer.UserData.PlatformAccountID.ToString();
				string cardDesc = "Game Version: " + Application.version;
				if (!m_DebugErrorReport)
				{
					cardDesc = cardDesc + "\n\nError Title Key: " + m_ErrorTitleKey + "\nText: " + LocalizationManager.GetTranslation(m_ErrorTitleKey, FixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, "English");
					cardDesc = cardDesc + "\n\nError Message Key: " + m_ErrorMessageKey + "\nText: " + LocalizationManager.GetTranslation(m_ErrorMessageKey, FixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, "English");
				}
				cardDesc = cardDesc + "\n\nModded Save: " + ((SaveData.Instance.Global?.CurrentAdventureData?.IsModded == true) ? "True" : "False");
				cardDesc = cardDesc + "\n\nPlaying Multiplayer: " + (FFSNetwork.IsOnline ? "True" : "False");
				cardDesc = cardDesc + "\n\nDateTime: " + DateTime.Now.ToString("dd/MM/yy_H:mm:ss");
				cardDesc = cardDesc + "\n\nOperating System: " + Environment.OSVersion.Platform.ToString() + " " + Environment.OSVersion.VersionString;
				cardDesc = cardDesc + "\n\nSteam Username: " + PlatformLayer.UserData.UserName;
				cardDesc = cardDesc + "\n\nUser Message:\n" + userText;
				if (!m_DebugErrorReport)
				{
					if (!string.IsNullOrEmpty(m_ExceptionMessage))
					{
						cardDesc = cardDesc + "\n\nException Message:\n" + m_ExceptionMessage;
					}
					cardDesc = cardDesc + "\n\nMessage Stack:\n" + m_ErrorStack;
				}
				string zipPath = Path.Combine(SaveData.Instance.PersistentDataPath, "ErrorReport.zip");
				int segmentsCreated;
				using (ZipFile zipFile = new ZipFile())
				{
					if (SaveData.Instance?.Global?.CurrentAdventureData != null && File.Exists(SaveData.Instance.Global.CurrentAdventureData.AdventureMapStateFilePath))
					{
						zipFile.AddFile(SaveData.Instance.Global.CurrentAdventureData.AdventureMapStateFilePath, Path.Combine(SaveData.Instance.Global.CurrentAdventureData.PartySaveFolderOnly, SaveData.Instance.Global.CurrentAdventureData.PartySaveName));
						if (!SaveData.Instance.Global.CurrentAdventureData.AdventureMapScenarioCheckpointsFilepaths.IsNullOrEmpty())
						{
							zipFile.AddFile(SaveData.Instance.Global.CurrentAdventureData.AdventureMapScenarioCheckpointsFilepaths.Last(), Path.Combine(SaveData.Instance.Global.CurrentAdventureData.PartySaveFolderOnly, "ScenarioCheckpoints", SaveData.Instance.Global.CurrentAdventureData.PartySaveName));
						}
						if (File.Exists(RootSaveData.RootSaveFile))
						{
							zipFile.AddFile(RootSaveData.RootSaveFile, "");
						}
						if (File.Exists(SaveData.Instance.RootData.GlobalSaveFile))
						{
							zipFile.AddFile(SaveData.Instance.RootData.GlobalSaveFile, "");
						}
					}
					else if (m_ErrorMessageKey.Contains("ERROR_SAVEDATA"))
					{
						zipFile.AddDirectory(RootSaveData.RootSaveFolder);
					}
					if (File.Exists(RootSaveData.PlayerLogPath))
					{
						zipFile.AddFile(RootSaveData.PlayerLogPath, "");
					}
					if (File.Exists(RootSaveData.PlayerLogPathFromHost))
					{
						zipFile.AddFile(RootSaveData.PlayerLogPathFromHost, "");
					}
					string[] allClientPlayerLogs = RootSaveData.GetAllClientPlayerLogs();
					foreach (string fileName in allClientPlayerLogs)
					{
						zipFile.AddFile(fileName, "");
					}
					if (File.Exists(RootSaveData.SimpleLogPath))
					{
						zipFile.AddFile(RootSaveData.SimpleLogPath, "");
					}
					if (File.Exists(RootSaveData.SimpleLogPrevPath))
					{
						zipFile.AddFile(RootSaveData.SimpleLogPrevPath, "");
					}
					if (File.Exists(RootSaveData.SimpleLogPathFromHost))
					{
						zipFile.AddFile(RootSaveData.SimpleLogPathFromHost, "");
					}
					allClientPlayerLogs = RootSaveData.GetAllClientSimpleLogs();
					foreach (string fileName2 in allClientPlayerLogs)
					{
						zipFile.AddFile(fileName2, "");
					}
					if (File.Exists(RootSaveData.ScreenCaptureImagePath))
					{
						zipFile.AddFile(RootSaveData.ScreenCaptureImagePath, "");
					}
					zipFile.MaxOutputSegmentSize = 9437184;
					zipFile.CompressionLevel = CompressionLevel.BestCompression;
					zipFile.Save(zipPath);
					segmentsCreated = zipFile.NumberOfSegmentsForMostRecentSave;
					UpdateProgressBar(20f);
				}
				TrelloAuthorization.Default.AppKey = "45afb90c32ccb5a699994d40b9309105";
				TrelloAuthorization.Default.UserToken = "90362503f93b4958d0af83afc720e68bc715776c3a0f308d88d30b2de3bd907d";
				ITrelloFactory trelloFactory = new TrelloFactory();
				IBoard board = trelloFactory.Board("5c913fcadf7a3f109e5ba244");
				await board.Lists.Refresh();
				UpdateProgressBar(40f);
				string listName = m_ErrorMessageKey + ((SaveData.Instance.Global?.CurrentAdventureData?.IsModded == true) ? "_MODDED" : "");
				IList list = board.Lists.FirstOrDefault((IList f) => f.Name == listName);
				if (list == null)
				{
					list = await board.Lists.Add(listName);
				}
				UpdateProgressBar(60f);
				ICard newCard = await list.Cards.Add(cardName, cardDesc);
				UpdateProgressBar(80f);
				for (int i2 = 0; i2 < segmentsCreated; i2++)
				{
					if (i2 == 0)
					{
						if (!File.Exists(zipPath))
						{
							Debug.LogError("Unable to find zip file " + zipPath);
							break;
						}
						await newCard.Attachments.Add(File.ReadAllBytes(zipPath), Path.GetFileName(zipPath));
						File.Delete(zipPath);
					}
					else if (i2 < 10)
					{
						string segmentPath = zipPath.Replace(".zip", ".z0" + i2);
						if (!File.Exists(segmentPath))
						{
							Debug.LogError("Unable to find zip file " + segmentPath);
							break;
						}
						await newCard.Attachments.Add(File.ReadAllBytes(segmentPath), Path.GetFileName(segmentPath));
						File.Delete(segmentPath);
					}
					else
					{
						string segmentPath = zipPath.Replace(".zip", ".z" + i2);
						if (!File.Exists(segmentPath))
						{
							Debug.LogError("Unable to find zip file " + segmentPath);
							break;
						}
						await newCard.Attachments.Add(File.ReadAllBytes(segmentPath), Path.GetFileName(segmentPath));
						File.Delete(segmentPath);
					}
				}
			}
			UpdateProgressBar(100f);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to send error report.\nException: " + ex.Message + "\n" + ex.StackTrace);
		}
		m_ErrorReportSent = true;
		ToggleErrorReportDialog(active: false);
	}

	public bool Escape()
	{
		UIWindowManager.UnregisterEscapable(this);
		if (!SendErrorReportButtonsGO.activeInHierarchy)
		{
			if (base.gameObject.activeInHierarchy && m_GameObjectState && m_UndoButton != null)
			{
				OnButtonClick(m_UndoButton);
				return true;
			}
			return false;
		}
		ToggleErrorReportDialog(active: false);
		return true;
	}

	public int Order()
	{
		return 0;
	}
}
