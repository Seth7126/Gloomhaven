#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Photon.Bolt;
using Photon.Voice.Unity;
using Photon.Voice.Unity.UtilityScripts;
using Platforms.Social;
using SM.Utils;
using UnityEngine;

namespace VoiceChat;

public class BoltVoiceChatService : Singleton<BoltVoiceChatService>
{
	private readonly List<ConnectedUserVoice> _playerVoices = new List<ConnectedUserVoice>();

	private SelfUserVoice _selfUserVoice;

	private BoltVoiceBridge _voiceBridge;

	private bool _isVoiceChatUserBlockerControllerInited;

	private Recorder _recorder;

	public SelfUserVoice SelfUserVoice => _selfUserVoice;

	public IReadOnlyList<ConnectedUserVoice> PlayerVoices => _playerVoices;

	public bool IsVoiceChatConnected => _voiceBridge.IsConnected;

	public VoiceChatUserBlockerController VoiceChatUserBlockerController { get; private set; }

	public Recorder Recorder => _recorder;

	public event Action EventStateUpdate;

	public event Action EventJoinRoom;

	public event Action EventLeftRoom;

	public event Action<ConnectedUserVoice, int> EventNewUserConnected;

	public event Action<ConnectedUserVoice, int> EventUserDisconnected;

	public event Action EventValidatePermissionStarted;

	public event Action<OperationResult> EventValidatePermissionCompleted;

	[UsedImplicitly]
	protected void Start()
	{
		UnityEngine.Object.DontDestroyOnLoad(this);
		_voiceBridge = BoltVoiceBridge.Instance;
		Recorder recorder = (_recorder = _voiceBridge.GetComponent<Recorder>());
		if (PlatformLayer.Instance.IsConsole)
		{
			recorder.VoiceDetectionThreshold = 0.03f;
		}
		_selfUserVoice = new SelfUserVoice(_voiceBridge.GetComponent<MicAmplifier>(), recorder);
		if (BoltVoiceBridge.Instance == null)
		{
			LogUtils.LogWarning("BoltVoiceBridge.Instance does not exist yet");
			return;
		}
		if (!BoltNetwork.IsRunning)
		{
			LogUtils.Log("[BoltVoiceUIController] BoltNetwork is not running. Hiding UI Voice Chat Controller");
		}
		Application.RequestUserAuthorization(UserAuthorization.Microphone);
		Debug.Log("Requesting user authorization for microphone");
		_voiceBridge.EventStateUpdate += OnEventStateUpdate;
		_voiceBridge.EventAddIncomingVoiceUser += VoiceBridgeOnEventAddIncomingVoiceUser;
		_voiceBridge.EventRemoveIncomingVoiceUser += VoiceBridgeOnEventRemoveIncomingVoiceUser;
		_voiceBridge.EventJoinRoom += VoiceBridgeOnEventJoinRoom;
		_voiceBridge.EventLeftRoom += VoiceBridgeOnEventLeftRoom;
		_voiceBridge.EventTextNotification += OnTextNotification;
	}

	[UsedImplicitly]
	protected override void OnDestroy()
	{
		VoiceChatUserBlockerController.DeInit();
		_voiceBridge.EventStateUpdate -= OnEventStateUpdate;
		_voiceBridge.EventAddIncomingVoiceUser -= VoiceBridgeOnEventAddIncomingVoiceUser;
		_voiceBridge.EventRemoveIncomingVoiceUser -= VoiceBridgeOnEventRemoveIncomingVoiceUser;
		_voiceBridge.EventJoinRoom -= VoiceBridgeOnEventJoinRoom;
		_voiceBridge.EventLeftRoom -= VoiceBridgeOnEventLeftRoom;
		_voiceBridge.EventTextNotification -= OnTextNotification;
	}

	public void InitVoiceChatUserBlockerController()
	{
		if (!_isVoiceChatUserBlockerControllerInited)
		{
			VoiceChatUserBlockerController = new VoiceChatUserBlockerController();
			VoiceChatUserBlockerController.Init(this);
			_isVoiceChatUserBlockerControllerInited = true;
		}
	}

	private void OnTextNotification(string message)
	{
		LogUtils.Log("voiceBridge message" + message);
	}

	private void VoiceBridgeOnEventLeftRoom()
	{
		_playerVoices.Clear();
		this.EventLeftRoom?.Invoke();
	}

	private void VoiceBridgeOnEventJoinRoom()
	{
		this.EventJoinRoom?.Invoke();
	}

	private void OnEventStateUpdate()
	{
		for (int num = _playerVoices.Count - 1; num >= 0; num--)
		{
			ConnectedUserVoice connectedUserVoice = _playerVoices[num];
			if (!connectedUserVoice.IsLinked)
			{
				_playerVoices.RemoveAt(num);
			}
			this.EventUserDisconnected?.Invoke(connectedUserVoice, num);
		}
		this.EventStateUpdate?.Invoke();
	}

	private void VoiceBridgeOnEventRemoveIncomingVoiceUser(int voiceChatUserId)
	{
		int num = _playerVoices.FindIndex((ConnectedUserVoice info) => info.VoiceChatUserId == voiceChatUserId);
		if (num >= 0)
		{
			ConnectedUserVoice connectedUserVoice = _playerVoices[num];
			_playerVoices.Remove(connectedUserVoice);
			this.EventUserDisconnected?.Invoke(connectedUserVoice, num);
		}
	}

	private void VoiceBridgeOnEventAddIncomingVoiceUser(int voiceChatUserId, string userName, string platformAccountID, string platformName, bool isHost, Speaker voiceSpeaker)
	{
		AudioSource component = voiceSpeaker.GetComponent<AudioSource>();
		ConnectedUserVoice userInfo = new ConnectedUserVoice(component, voiceSpeaker, userName, platformAccountID, platformName, voiceChatUserId, isHost);
		userInfo.MaskBadWordsInUsername(delegate
		{
			_playerVoices.Add(userInfo);
			ValidatePermissions(delegate
			{
				this.EventValidatePermissionStarted?.Invoke();
			}, delegate(OperationResult result)
			{
				this.EventValidatePermissionCompleted?.Invoke(result);
			});
			this.EventNewUserConnected?.Invoke(userInfo, _playerVoices.Count - 1);
		});
	}

	public void ValidatePermissions(Action onStart = null, Action<OperationResult> onComplete = null)
	{
	}

	public void ShutdownVoiceConnection()
	{
		_voiceBridge.ShutdownVoiceConnection();
		_voiceBridge.OnLeftRoom();
	}

	public void SetupAndConnect()
	{
		_voiceBridge.SetupAndConnect();
	}

	[UsedImplicitly]
	private void Update()
	{
		_selfUserVoice.Update();
	}
}
