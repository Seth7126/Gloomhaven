#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using Chronos;
using FFSNet;
using GLOOM;
using MEC;
using Photon.Bolt;
using SM.Gamepad;
using SM.Utils;
using Script.GUI.SMNavigation;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle), typeof(UIWindow))]
public class UIReadyToggle : Singleton<UIReadyToggle>
{
	public enum EReadyUpType
	{
		Participant,
		Player
	}

	private const string GUIMultiplayerReadying = "GUI_MULTIPLAYER_READYING";

	private const string GUIMultiplayerUnreadying = "GUI_MULTIPLAYER_UNREADYING";

	[SerializeField]
	private Color _defaultLabelColor;

	[SerializeField]
	private TextLocalizedListener _buttonText;

	[SerializeField]
	private Graphic _textGraphic;

	[Header("Input Progress")]
	[SerializeField]
	private ImageProgressBar _progressBar;

	[SerializeField]
	private bool _useProgressConfirm = true;

	[SerializeField]
	private bool _useProgressCancel;

	[Header("Gamepad")]
	[SerializeField]
	private LongConfirmHandler _longConfirmHandler;

	[SerializeField]
	private Hotkey _confirmHotKey;

	[SerializeField]
	private Hotkey _cancelHotKey;

	[SerializeField]
	private TextMeshProUGUI _confirmLabel;

	[SerializeField]
	private TextMeshProUGUI _cancelLabel;

	private static readonly float stateSyncCheckInterval = 0.2f;

	private static readonly float clientStateSyncTimeoutDuration = 20f;

	private static readonly float globalStateSyncTimeoutDuration = 30f;

	private UIWindow window;

	private ExtendedToggle toggle;

	private UnityAction onReady;

	private UnityAction onUnready;

	private UnityAction onAllPlayersReady;

	private Action onShortPressed;

	private UnityAction<NetworkPlayer, bool> onReadiedPlayersChanged;

	private Func<bool> canReadyUp;

	private UnityAction<bool> onStartProgress;

	private UnityAction<bool> onEndProgress;

	private bool validateReadyUpOnPlayerLeft;

	private string readyTextLoc;

	private string unreadyTextLoc;

	private string readyAudioItem;

	private List<NetworkPlayer> playersAwaited = new List<NetworkPlayer>();

	private CoroutineHandle stateSyncCheckRoutine;

	private EReadyUpType readyUpType;

	private EReadyUpToggleStates readyUpToggleState;

	private string controllerAreaAllowed;

	private bool _isOn;

	private bool _requestVisible;

	private bool _interactable;

	private bool _allPlayersReady;

	private HashSet<object> _visibilityRequests = new HashSet<object>();

	private readonly SimpleKeyActionHandlerBlocker _confirmBoxShownBlocker = new SimpleKeyActionHandlerBlocker();

	private SkipFrameKeyActionHandlerBlocker _skipFrameConfirmButtonBlocker;

	private SkipFrameKeyActionHandlerBlocker _skipFrameCancelButtonBlocker;

	private bool _shortPressed;

	public Dictionary<NetworkPlayer, EReadyUpToggleStates> PlayerReadiedState { get; } = new Dictionary<NetworkPlayer, EReadyUpToggleStates>();

	public List<NetworkPlayer> PlayersReady { get; } = new List<NetworkPlayer>();

	public bool IsInteractable => _interactable;

	public bool ToggledOn => _isOn;

	public bool IsVisible => window.IsOpen;

	public EReadyUpType ReadyUpType => readyUpType;

	public bool IsProgressingBar => _progressBar.gameObject.activeInHierarchy;

	public bool ShouldBeVisible
	{
		get
		{
			if (_requestVisible && _interactable && !_allPlayersReady)
			{
				return _visibilityRequests.Count == 0;
			}
			return false;
		}
	}

	public bool CanBeToggled { get; set; }

	public bool ShortPressed
	{
		get
		{
			bool shortPressed = _shortPressed;
			_shortPressed = false;
			return shortPressed;
		}
		private set
		{
			_shortPressed = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		window = GetComponent<UIWindow>();
		window.onHidden.AddListener(OnWindowHidden);
		window.onShown.AddListener(OnWindowShown);
		_skipFrameConfirmButtonBlocker = new SkipFrameKeyActionHandlerBlocker(this, defaultIsBlock: false);
		_skipFrameCancelButtonBlocker = new SkipFrameKeyActionHandlerBlocker(this, defaultIsBlock: false);
		toggle = GetComponent<ExtendedToggle>();
		_confirmBoxShownBlocker.SetBlock(value: false);
		if (!InputManager.GamePadInUse)
		{
			toggle.onValueChanged.AddListener(InputToggle);
		}
		else
		{
			SubscribeOnGamepadEvents();
			SetActiveCancelButton(isActive: true);
			_cancelHotKey.Initialize(Singleton<UINavigation>.Instance.Input);
			SetActiveCancelButton(isActive: false);
			_confirmHotKey.Initialize(Singleton<UINavigation>.Instance.Input);
		}
		CanBeToggled = true;
	}

	protected override void OnDestroy()
	{
		if (!InputManager.GamePadInUse)
		{
			toggle.onValueChanged.RemoveAllListeners();
		}
		window.onHidden.RemoveListener(OnWindowHidden);
		window.onShown.RemoveListener(OnWindowShown);
		PlayerRegistry.OnPlayerLeft = (PlayersChangedEvent)Delegate.Remove(PlayerRegistry.OnPlayerLeft, new PlayersChangedEvent(OnPlayerLeft));
		ResetPlayerACKs();
		if (InputManager.GamePadInUse)
		{
			UnsubscribeOnGamepadEvents();
			_cancelHotKey.Deinitialize();
			_confirmHotKey.Deinitialize();
		}
		base.OnDestroy();
	}

	private void OnEnable()
	{
		if (InputManager.GamePadInUse)
		{
			_confirmHotKey.UpdateHotkeyLabel();
		}
	}

	public void SetSendDefaultSubmitEvent(bool enable)
	{
		_longConfirmHandler.SetSendSubmitEventOnShort(enable);
	}

	public void SendSubmitPlayerActionOnShort(bool enable)
	{
		_longConfirmHandler.SetSendSubmitPlayerActionOnShort(enable);
	}

	public void InitializeLabelGamepad(string text, Color color = default(Color))
	{
		if (InputManager.GamePadInUse)
		{
			if (color != default(Color))
			{
				SetLabelColorGamepad(color);
			}
			else
			{
				SetDefaultLabelColorGamepad();
			}
			SetLabelTextGamepad(text);
		}
	}

	public void SetLabelTextGamepad(string text)
	{
		if (InputManager.GamePadInUse)
		{
			_confirmLabel.text = text;
		}
	}

	public void SetLabelColorGamepad(Color color)
	{
		if (InputManager.GamePadInUse)
		{
			_confirmLabel.color = color;
		}
	}

	public void SetDefaultLabelColorGamepad()
	{
		_confirmLabel.color = _defaultLabelColor;
	}

	public void SetCancelLabelTextGamepad(string text)
	{
		if (InputManager.GamePadInUse)
		{
			_cancelLabel.text = text;
		}
	}

	private void SubscribeOnGamepadEvents()
	{
		Singleton<UIConfirmationBoxManager>.Instance.BoxWindowShown += OnConfirmBoxWindowShown;
		Singleton<UIConfirmationBoxManager>.Instance.BoxWindowHidden += OnConfirmBoxWindowHidden;
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, OnConfirmButtonPressed).AddBlocker(_confirmBoxShownBlocker).AddBlocker(_skipFrameConfirmButtonBlocker));
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_CANCEL, OnCancelButtonPressed).AddBlocker(_confirmBoxShownBlocker).AddBlocker(_skipFrameCancelButtonBlocker));
	}

	private void UnsubscribeOnGamepadEvents()
	{
		if (Singleton<UIConfirmationBoxManager>.Instance != null)
		{
			Singleton<UIConfirmationBoxManager>.Instance.BoxWindowShown -= OnConfirmBoxWindowShown;
			Singleton<UIConfirmationBoxManager>.Instance.BoxWindowHidden -= OnConfirmBoxWindowHidden;
		}
		if (Singleton<KeyActionHandlerController>.Instance != null)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_SUBMIT, OnConfirmButtonPressed);
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_CANCEL, OnCancelButtonPressed);
		}
	}

	private void OnConfirmBoxWindowShown()
	{
		_skipFrameConfirmButtonBlocker.Run();
		_skipFrameCancelButtonBlocker.Run();
		_confirmBoxShownBlocker.SetBlock(value: true);
	}

	private void OnConfirmBoxWindowHidden()
	{
		_skipFrameConfirmButtonBlocker.Run();
		_skipFrameCancelButtonBlocker.Run();
		_confirmBoxShownBlocker.SetBlock(value: false);
	}

	private void OnWindowShown()
	{
		DisableOtherConfirmButtons();
	}

	private void OnWindowHidden()
	{
		EnableOtherButtons();
	}

	private void OnConfirmButtonPressed()
	{
		if (!_longConfirmHandler.gameObject.activeInHierarchy || !CanBeToggled || !IsVisible)
		{
			onShortPressed?.Invoke();
		}
		else
		{
			_longConfirmHandler.Pressed(OnLongConfirmButtonPressed, HandleShortPressed);
		}
	}

	private void HandleShortPressed()
	{
		onShortPressed?.Invoke();
		ShortPressed = true;
	}

	private void OnLongConfirmButtonPressed()
	{
		InputToggle(isOn: true);
	}

	private void OnCancelButtonPressed()
	{
		if (_cancelHotKey.gameObject.activeInHierarchy && IsVisible)
		{
			InputToggle(isOn: false);
		}
	}

	private void SetActiveCancelButton(bool isActive)
	{
		_cancelHotKey.gameObject.SetActive(isActive);
	}

	private void SetActiveSubmitButton(bool isActive)
	{
		_longConfirmHandler.gameObject.SetActive(isActive);
	}

	private void DisableOtherConfirmButtons()
	{
		if (Singleton<AdventureMapUIManager>.Instance != null)
		{
			Singleton<AdventureMapUIManager>.Instance.LockSinglePlayerConfirmButton();
		}
	}

	private void EnableOtherButtons()
	{
		if (Singleton<AdventureMapUIManager>.Instance != null)
		{
			Singleton<AdventureMapUIManager>.Instance.UnlockSinglePlayerConfirmButton();
		}
	}

	private void SetIsOnWithoutNotify(bool isOn)
	{
		_isOn = isOn;
		if (InputManager.GamePadInUse)
		{
			SetActiveSubmitButton(!isOn);
			SetActiveCancelButton(isOn);
		}
		else
		{
			toggle.SetIsOnWithoutNotify(isOn);
			toggle.mouseDownAudioItem = (isOn ? null : readyAudioItem);
			_buttonText.SetTextKey(isOn ? unreadyTextLoc : readyTextLoc);
		}
	}

	private void InputToggle(bool isOn)
	{
		if (isOn && IsReadyUpForbidden())
		{
			return;
		}
		SetIsOnWithoutNotify(isOn);
		if (isOn != PlayersReady.Contains(PlayerRegistry.MyPlayer))
		{
			if (UseProgressForInput(isOn))
			{
				LogUtils.Log($"Start ready {isOn} progression");
				_progressBar.SetAmount(0f, 1f);
				_progressBar.gameObject.SetActive(value: true);
				string translation = LocalizationManager.GetTranslation(isOn ? "GUI_MULTIPLAYER_READYING" : "GUI_MULTIPLAYER_UNREADYING");
				onStartProgress?.Invoke(isOn);
				_progressBar.PlayProgressTo(1f, translation, delegate
				{
					OnEndAnimationProgressBar(isOn);
				});
			}
			else
			{
				ReadyUp(isOn);
			}
		}
		else
		{
			LogUtils.Log("Cancel ready progression");
			_progressBar.gameObject.SetActive(value: false);
			onEndProgress?.Invoke(isOn);
		}
	}

	private bool UseProgressForInput(bool value)
	{
		if (!value || !_useProgressConfirm)
		{
			if (!value)
			{
				return _useProgressCancel;
			}
			return false;
		}
		return true;
	}

	private void OnEndAnimationProgressBar(bool isOn)
	{
		LogUtils.Log($"Completed ready {isOn} progression");
		onEndProgress?.Invoke(isOn);
		ReadyUp(isOn);
	}

	public void Initialize(bool show = true, UnityAction onReady = null, UnityAction onUnready = null, UnityAction onAllPlayersReady = null, UnityAction<NetworkPlayer, bool> onReadiedPlayersChanged = null, Action onShortPressed = null, Func<bool> canReadyUp = null, string readyTextLoc = "GUI_READY", string unreadyTextLoc = "GUI_UNREADY", string readyAudioItem = "PlaySound_UIMultiPlayerReady", bool bringToFront = false, UnityAction<bool> onStartProgress = null, UnityAction<bool> onEndProgress = null, bool validateReadyUpOnPlayerLeft = false, EReadyUpType readyUpType = EReadyUpType.Participant, EReadyUpToggleStates readyUpToggleState = EReadyUpToggleStates.NotSet, string controllerAreaAllowed = null)
	{
		this.onReady = onReady;
		this.onUnready = onUnready;
		this.onAllPlayersReady = onAllPlayersReady;
		this.onReadiedPlayersChanged = onReadiedPlayersChanged;
		this.onShortPressed = onShortPressed;
		this.canReadyUp = canReadyUp;
		this.readyTextLoc = readyTextLoc;
		this.unreadyTextLoc = unreadyTextLoc;
		this.readyAudioItem = readyAudioItem;
		this.onStartProgress = onStartProgress;
		this.onEndProgress = onEndProgress;
		this.validateReadyUpOnPlayerLeft = validateReadyUpOnPlayerLeft;
		this.readyUpType = readyUpType;
		this.readyUpToggleState = readyUpToggleState;
		this.controllerAreaAllowed = controllerAreaAllowed;
		_allPlayersReady = false;
		_progressBar.gameObject.SetActive(value: false);
		SetInteractable(interactable: false);
		SetIsOnWithoutNotify(isOn: false);
		PlayerRegistry.OnPlayerLeft = (PlayersChangedEvent)Delegate.Remove(PlayerRegistry.OnPlayerLeft, new PlayersChangedEvent(OnPlayerLeft));
		PlayerRegistry.OnPlayerLeft = (PlayersChangedEvent)Delegate.Combine(PlayerRegistry.OnPlayerLeft, new PlayersChangedEvent(OnPlayerLeft));
		foreach (NetworkPlayer item in PlayersReady.Except(PlayerRegistry.AllPlayers).ToList())
		{
			OnPlayerLeft(item);
		}
		if (show)
		{
			ToggleVisibility(visible: true, bringToFront);
		}
	}

	public void Reset()
	{
		LogUtils.Log("Reset players ready");
		onReady = null;
		onUnready = null;
		onAllPlayersReady = null;
		onReadiedPlayersChanged = null;
		validateReadyUpOnPlayerLeft = false;
		_allPlayersReady = false;
		_progressBar.gameObject.SetActive(value: false);
		SetIsOnWithoutNotify(isOn: false);
		SetInteractable(interactable: false);
		PlayerRegistry.OnPlayerLeft = (PlayersChangedEvent)Delegate.Remove(PlayerRegistry.OnPlayerLeft, new PlayersChangedEvent(OnPlayerLeft));
		ResetPlayerACKs();
		PlayersReady.Clear();
		PlayerReadiedState.Clear();
		playersAwaited.Clear();
		ToggleVisibility(visible: false);
	}

	public void ResetPlayerACKs()
	{
		if (FFSNetwork.IsHost)
		{
			Timing.KillCoroutines(stateSyncCheckRoutine);
			PlayerRegistry.AllPlayers.ForEach(delegate(NetworkPlayer x)
			{
				x.PlayersACKedMyLatestControllableState.Clear();
			});
		}
		else
		{
			PlayerRegistry.AllPlayers.ForEach(delegate(NetworkPlayer x)
			{
				Timing.KillCoroutines(x.PlayerID.ToString());
			});
		}
	}

	public void BlockVisibility(object request)
	{
		_visibilityRequests.Add(request);
		UpdateVisiblity();
	}

	public void UnblockVisibility(object request)
	{
		_visibilityRequests.Remove(request);
		UpdateVisiblity();
	}

	public void ToggleVisibility(bool visible, bool bringToFront = false)
	{
		_requestVisible = visible;
		if (visible && bringToFront)
		{
			UIUtility.BringToFront(window.gameObject);
		}
		UpdateVisiblity();
	}

	public void SetInteractable(bool interactable)
	{
		if (_interactable != interactable)
		{
			_interactable = interactable;
			toggle.interactable = interactable;
			if (!InputManager.GamePadInUse)
			{
				_textGraphic.CrossFadeColor(interactable ? UIInfoTools.Instance.White : UIInfoTools.Instance.greyedOutTextColor, 0f, ignoreTimeScale: true, useAlpha: true);
			}
			UpdateVisiblity();
			FFSNet.Console.LogInfo("MP Ready Toggle set to " + (interactable ? " INTERACTABLE." : " UNINTERACTABLE."));
		}
	}

	private void UpdateVisiblity()
	{
		bool shouldBeVisible = ShouldBeVisible;
		if (shouldBeVisible != IsVisible)
		{
			if (shouldBeVisible)
			{
				window.Show();
			}
			else
			{
				CancelProgress();
				window.Hide();
			}
			FFSNet.Console.LogInfo("MP Ready Toggle set to " + (ShouldBeVisible ? " VISIBLE." : "INVISIBLE."));
		}
	}

	public void CancelProgress()
	{
		if (_progressBar.gameObject.activeSelf)
		{
			_progressBar.gameObject.SetActive(value: false);
			bool isOn = _isOn;
			SetIsOnWithoutNotify(!isOn);
			onEndProgress?.Invoke(isOn);
			LogUtils.Log("Cancel ready progression");
		}
	}

	public void WaitForPlayerBeforeProceeding(NetworkPlayer player)
	{
		if (!playersAwaited.Contains(player))
		{
			playersAwaited.Add(player);
			FFSNet.Console.LogInfo(player.Username + " added to the list of awaited players.");
		}
	}

	private bool IsReadyUpForbidden()
	{
		if (canReadyUp != null && !canReadyUp())
		{
			SetIsOnWithoutNotify(isOn: false);
			return true;
		}
		return false;
	}

	public void ReadyUp(bool toggledOn)
	{
		ReadyUp(toggledOn, autoValidateUnreadying: false);
	}

	public void ReadyUp(bool toggledOn, bool autoValidateUnreadying)
	{
		_progressBar.gameObject.SetActive(value: false);
		if (!FFSNetwork.IsOnline || (PlayersReady.Count >= ((readyUpType == EReadyUpType.Participant) ? PlayerRegistry.Participants : PlayerRegistry.AllPlayers).Count && !(!toggledOn && autoValidateUnreadying)))
		{
			return;
		}
		if (toggledOn)
		{
			if (!IsReadyUpForbidden())
			{
				ReadyUpPlayer(PlayerRegistry.MyPlayer, readyUpToggleState);
			}
		}
		else
		{
			UnreadyPlayer(PlayerRegistry.MyPlayer, isValidatedAction: false, autoValidateUnreadying);
		}
	}

	private bool ReadyUpPlayer(NetworkPlayer player, EReadyUpToggleStates state)
	{
		if (PlayersReady.Exists((NetworkPlayer e) => e.PlayerID == player.PlayerID))
		{
			return false;
		}
		if (playersAwaited.Contains(player))
		{
			playersAwaited.Remove(player);
		}
		PlayersReady.Add(player);
		PlayerReadiedState[player] = state;
		OnReadiedPlayersChanged(player, ready: true);
		FFSNet.Console.LogInfo("Readied " + player.Username + ". Players Ready Count: " + PlayersReady.Count);
		if (player.Equals(PlayerRegistry.MyPlayer))
		{
			SetIsOnWithoutNotify(isOn: true);
			onReady?.Invoke();
			if (FFSNetwork.IsHost)
			{
				ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
				IProtocolToken supplementaryDataToken = new ControllableStateRevisionToken(PlayerRegistry.MyPlayer);
				IProtocolToken supplementaryDataToken2 = new ReadyUpToken(readyUpToggleState.ToString());
				Synchronizer.SendGameAction(GameActionType.ReadyUpPlayer, currentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken, supplementaryDataToken2);
			}
			else
			{
				ActionPhaseType currentPhase2 = ActionProcessor.CurrentPhase;
				IProtocolToken supplementaryDataToken2 = new ReadyUpToken(readyUpToggleState.ToString());
				Synchronizer.SendGameAction(GameActionType.ReadyUpPlayer, currentPhase2, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), null, supplementaryDataToken2);
			}
		}
		if (FFSNetwork.IsHost)
		{
			if (readyUpType == EReadyUpType.Participant && PlayerRegistry.MyPlayer.IsParticipant && PlayersReady.Count >= PlayerRegistry.Participants.Count && playersAwaited.Count == 0 && PlayersReady.Contains(PlayerRegistry.MyPlayer))
			{
				stateSyncCheckRoutine = Timing.RunCoroutine(WaitForStateSyncBeforeProceeding());
			}
			else if (readyUpType == EReadyUpType.Player && PlayersReady.Count >= PlayerRegistry.AllPlayers.Count && playersAwaited.Count == 0)
			{
				stateSyncCheckRoutine = Timing.RunCoroutine(WaitForStateSyncBeforeProceeding());
			}
		}
		return true;
	}

	private IEnumerator<float> WaitForStateSyncBeforeProceeding()
	{
		float timeOutTime = Timekeeper.instance.m_GlobalClock.time + globalStateSyncTimeoutDuration;
		ResetPlayerACKs();
		LogUtils.Log("WaitForStateSyncBeforeProceeding " + readyUpType.ToString() + " (connecting:" + ((PlayerRegistry.ConnectingUsers != null) ? PlayerRegistry.ConnectingUsers.Count : 0) + ")");
		foreach (NetworkPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			allPlayer.PlayersACKedMyLatestControllableState.Add(PlayerRegistry.MyPlayer);
			Synchronizer.SendGameAction(targetPhaseType: ActionProcessor.CurrentPhase, supplementaryDataToken: new ControllableStateRevisionToken(allPlayer), actionType: GameActionType.AllPlayersReady, validateOnServerBeforeExecuting: false, disableAutoReplication: false, actorID: 0, supplementaryDataIDMin: 0, supplementaryDataIDMed: 0, supplementaryDataIDMax: allPlayer.PlayerID);
		}
		while (PlayersReady.Exists((NetworkPlayer x) => x.IsActive && ((readyUpType == EReadyUpType.Player) ? PlayerRegistry.AllPlayers : PlayerRegistry.Participants).Where((NetworkPlayer w) => !w.HasDesynched).ToList().Exists((NetworkPlayer y) => y != x && !x.PlayersACKedMyLatestControllableState.Contains(y))))
		{
			if (Timekeeper.instance.m_GlobalClock.time > timeOutTime)
			{
				string text = "Player ACKs Missing:\n";
				foreach (NetworkPlayer player in PlayersReady.Where((NetworkPlayer w) => w.IsActive))
				{
					IEnumerable<string> enumerable = from s in (readyUpType == EReadyUpType.Player) ? PlayerRegistry.AllPlayers : PlayerRegistry.Participants
						where s != player && !player.PlayersACKedMyLatestControllableState.Contains(s)
						select s.Username;
					if (enumerable.Count() > 0)
					{
						text = text + player.Username + " is missing ACK from: " + string.Join(", ", enumerable);
					}
				}
				FFSNetwork.HandleDesync(new Exception("Timed out waiting for clients to acknowledge the latest controllable states.\n" + text));
				yield break;
			}
			yield return Timing.WaitForSeconds(stateSyncCheckInterval);
			if (!PlayerRegistry.ConnectingUsers.IsNullOrEmpty())
			{
				LogUtils.Log("User entered room while WaitForStateSyncBeforeProceeding");
			}
		}
		while (ActionProcessor.CurrentPhase == ActionPhaseType.MapLoadoutScreen && ActionProcessor.ActionQueue.Any((GameAction it) => it.TargetPhaseID == 3))
		{
			LogUtils.LogError("Waiting all actions end");
			yield return float.NegativeInfinity;
		}
		Synchronizer.SendGameAction(GameActionType.ReadyProceed, ActionProcessor.CurrentPhase);
		Proceed();
	}

	private void Proceed()
	{
		if (onAllPlayersReady != null)
		{
			LogUtils.Log("AllPlayersReady (connecting: " + ((PlayerRegistry.ConnectingUsers != null) ? PlayerRegistry.ConnectingUsers.Count : 0) + ")");
			UnityAction unityAction = onAllPlayersReady;
			Reset();
			unityAction();
		}
		else
		{
			FFSNet.Console.LogWarning("Error proceeding. No callback method found.");
		}
	}

	private bool UnreadyPlayer(NetworkPlayer player, bool isValidatedAction = false, bool autoValidateAction = false)
	{
		if (!PlayersReady.Exists((NetworkPlayer e) => e.PlayerID == player.PlayerID))
		{
			return false;
		}
		LogUtils.Log("Unready player " + player.Username);
		if (playersAwaited.Contains(player))
		{
			playersAwaited.Remove(player);
		}
		if (player.Equals(PlayerRegistry.MyPlayer))
		{
			if (!isValidatedAction)
			{
				if (FFSNetwork.IsClient)
				{
					SetInteractable(interactable: false);
				}
				Synchronizer.SendGameAction(GameActionType.UnreadyPlayer, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: true, disableAutoReplication: false, 0, 0, 0, 0, autoValidateAction);
			}
		}
		else if (FFSNetwork.IsHost && !autoValidateAction && ((readyUpType == EReadyUpType.Participant && PlayerRegistry.MyPlayer.IsParticipant && PlayersReady.Count >= PlayerRegistry.Participants.Count) || (readyUpType == EReadyUpType.Player && PlayersReady.Count >= PlayerRegistry.AllPlayers.Count)))
		{
			return false;
		}
		if (FFSNetwork.IsHost || isValidatedAction)
		{
			if (PlayersReady.Contains(player))
			{
				Timing.KillCoroutines(player.PlayerID.ToString());
				if (FFSNetwork.IsHost)
				{
					FFSNet.Console.LogInfo("Cleared Player ACKed players for Player with ID: " + player.PlayerID);
					player.PlayersACKedMyLatestControllableState.Clear();
					Timing.KillCoroutines(stateSyncCheckRoutine);
				}
				PlayersReady.Remove(player);
				PlayerReadiedState.Remove(player);
				OnReadiedPlayersChanged(player, ready: false);
				FFSNet.Console.LogInfo("Players Ready Count: " + PlayersReady.Count + ": " + string.Join(",", PlayersReady.Select((NetworkPlayer it) => it.Username)));
			}
			if (player.Equals(PlayerRegistry.MyPlayer))
			{
				SetInteractable(interactable: true);
				SetIsOnWithoutNotify(isOn: false);
				onUnready?.Invoke();
			}
		}
		return true;
	}

	private void OnPlayerLeft(NetworkPlayer player)
	{
		bool flag = PlayersReady.Remove(player);
		PlayerReadiedState.Remove(player);
		if (FFSNetwork.IsHost)
		{
			PlayersReady.ForEach(delegate(NetworkPlayer x)
			{
				x.PlayersACKedMyLatestControllableState.Remove(player);
			});
		}
		if (playersAwaited.Contains(player))
		{
			playersAwaited.Remove(player);
		}
		if (!validateReadyUpOnPlayerLeft)
		{
			return;
		}
		if (flag)
		{
			OnReadiedPlayersChanged(player, ready: false);
			if (player.Equals(PlayerRegistry.MyPlayer))
			{
				onUnready?.Invoke();
			}
		}
		if (FFSNetwork.IsHost && ((readyUpType == EReadyUpType.Participant && PlayerRegistry.MyPlayer.IsParticipant && PlayersReady.Count >= PlayerRegistry.Participants.Count && PlayersReady.Contains(PlayerRegistry.MyPlayer)) || (readyUpType == EReadyUpType.Player && PlayersReady.Count >= PlayerRegistry.AllPlayers.Count)) && playersAwaited.Count == 0)
		{
			stateSyncCheckRoutine = Timing.RunCoroutine(WaitForStateSyncBeforeProceeding());
		}
	}

	private void OnReadiedPlayersChanged(NetworkPlayer player, bool ready)
	{
		onReadiedPlayersChanged?.Invoke(player, ready);
		_allPlayersReady = ready && PlayersReady.Count == PlayerRegistry.Participants.Count;
		UpdateVisiblity();
	}

	public void ClientProceed()
	{
		FFSNet.Console.LogInfo("Proceeding to the next phase.");
		Proceed();
	}

	public void ProxySetReadyState(GameAction action, ref bool forwardAction)
	{
		NetworkPlayer player = PlayerRegistry.GetPlayer(action.PlayerID);
		if (player != null)
		{
			if (action.ActionTypeID == 20)
			{
				ReadyUpToken readyUpToken = (ReadyUpToken)action.SupplementaryDataToken2;
				forwardAction = ReadyUpPlayer(player, (EReadyUpToggleStates)Enum.Parse(typeof(EReadyUpToggleStates), readyUpToken.ToggleState));
				if (FFSNetwork.IsHost & forwardAction)
				{
					action.SupplementaryDataToken = new ControllableStateRevisionToken(player);
				}
			}
			else
			{
				forwardAction = UnreadyPlayer(player, isValidatedAction: true, action.SupplementaryDataBoolean);
			}
			return;
		}
		throw new Exception("Error readying up a proxy playerActor. The player does not exist.");
	}

	public void ClientCheckPlayerControllableStateRevisionsMatch(GameAction action)
	{
		if (FFSNetwork.IsClient)
		{
			NetworkPlayer player = PlayerRegistry.GetPlayer(action.SupplementaryDataIDMax);
			ControllableStateRevisionToken controllableStateRevisionToken = action.SupplementaryDataToken as ControllableStateRevisionToken;
			controllableStateRevisionToken.PrintControllableStateRevisionData();
			Timing.KillCoroutines(player.PlayerID.ToString());
			Timing.RunCoroutine(WaitUntilRevisionsMatch(player, controllableStateRevisionToken), player.PlayerID.ToString());
		}
	}

	private IEnumerator<float> WaitUntilRevisionsMatch(NetworkPlayer targetPlayer, ControllableStateRevisionToken latestRevision)
	{
		float timeOutTime = Timekeeper.instance.m_GlobalClock.time + clientStateSyncTimeoutDuration;
		while (!latestRevision.IsSameRevision(targetPlayer) || ActionProcessor.IsProcessing)
		{
			if (Timekeeper.instance.m_GlobalClock.time > timeOutTime)
			{
				FFSNetwork.HandleDesync(new Exception("State revisions do not match. Timed out waiting for the latest state update."));
				yield break;
			}
			yield return Timing.WaitForSeconds(stateSyncCheckInterval);
		}
		Synchronizer.SendGameAction(GameActionType.StatesSynchronized, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: true, 0, 0, 0, targetPlayer.PlayerID);
	}

	public void ServerACKSyncedStateRevision(GameAction action)
	{
		NetworkPlayer player = PlayerRegistry.GetPlayer(action.SupplementaryDataIDMax);
		if (player != null)
		{
			NetworkPlayer player2 = PlayerRegistry.GetPlayer(action.PlayerID);
			if (player2 != null)
			{
				player.PlayersACKedMyLatestControllableState.Add(player2);
				FFSNet.Console.LogInfo("Player " + player2.Username + "(PlayerID: " + player2.PlayerID + ") ACKed having just acquired the latest states for " + player.Username + "'s controllables (PlayerID: " + player.PlayerID + ").");
			}
			else
			{
				FFSNet.Console.LogWarning("Error ACKing synced state revision. ACKed player does not exist (PlayerID: " + action.PlayerID + ").");
			}
		}
		else
		{
			FFSNet.Console.LogWarning("Error ACKing synced state revision. Controlling player does not exist (PlayerID: " + action.SupplementaryDataIDMax + ").");
		}
	}
}
