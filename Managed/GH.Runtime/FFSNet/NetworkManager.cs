#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MEC;
using Photon.Bolt;
using Photon.Bolt.Matchmaking;
using Platforms.Social;
using UdpKit;
using UdpKit.Platform;
using UdpKit.Platform.Photon;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FFSNet;

public class NetworkManager : ScriptableObject
{
	[Range(1f, 7f)]
	[SerializeField]
	private int maxPlayers = 4;

	[Range(0.05f, 1f)]
	[SerializeField]
	private float sessionListUpdateFrequency = 0.5f;

	[Range(1f, 60f)]
	[SerializeField]
	private float matchmakingTimeOutDuration = 3f;

	[Range(3f, 15f)]
	[SerializeField]
	private int inviteCodeLength = 8;

	[Range(0.05f, 0.5f)]
	[SerializeField]
	private float actionQueueProcessingInterval = 0.3f;

	[Range(1f, 30f)]
	[Header("How long to wait after detecting the next action in the processing queue cannot be played at the current phase.")]
	[SerializeField]
	private float incorrectActionDetectedTimeOutDuration = 5f;

	[Header("Auto-assign removed/disconnected players' controllables to the host player.")]
	[SerializeField]
	private bool autoAssignControllablesToHost = true;

	[Header("Automatically shutdown FFSNetwork in case my NetworkPlayer is detached.")]
	[SerializeField]
	private bool autoShutdownUponMyPlayerRemoved = true;

	[Header("Automatically shutdown FFSNetwork whenever joining a session fails.")]
	[SerializeField]
	private bool autoShutdownUponJoiningFailed = true;

	[Header("Automatically shutdown FFSNetwork whenever a desynchronization occurs.")]
	[SerializeField]
	private bool autoShutdownUponDesynchronization = true;

	[Header("Prints the acting controllable name as part of the error logging for incorrect actions detected.")]
	[SerializeField]
	private bool displayControllableNameForIncorrectActionErrors = true;

	public static bool LocalMultiplayer;

	private CoroutineHandle joinSessionRoutine;

	private bool cancelingJoining;

	private UdpEndPoint serverEndPoint;

	private UdpEndPoint clientEndPoint;

	public int MaxPlayers => maxPlayers;

	public float ActionQueueProcessingInterval => actionQueueProcessingInterval;

	public float IncorrectActionDetectedTimeOutDuration => incorrectActionDetectedTimeOutDuration;

	public bool AutoAssignControllablesToHost => autoAssignControllablesToHost;

	public bool AutoShutdownUponMyPlayerRemoved => autoShutdownUponMyPlayerRemoved;

	public bool AutoShutdownUponJoiningFailed => autoShutdownUponJoiningFailed;

	public bool AutoShutdownUponDesynchronization => autoShutdownUponDesynchronization;

	public bool DisplayControllableNameForIncorrectActionErrors => displayControllableNameForIncorrectActionErrors;

	public string SessionID { get; private set; } = string.Empty;

	public int DebugPort { get; private set; } = -1;

	public bool JoinSessionOnceClientStarted { get; set; }

	public bool ConnectingToSession { get; set; }

	public BoltConfig NetworkConfig { get; private set; }

	public UnityEvent HostingStartedEvent { get; private set; } = new UnityEvent();

	public UnityEvent HostingEndedEvent { get; private set; } = new UnityEvent();

	public UnityAction<CustomDataToken> OnConnectionEstablished { get; private set; }

	public UnityAction<ConnectionState> OnConnectionStateUpdated { get; private set; }

	public UnityAction<ConnectionErrorCode> OnConnectionFailed { get; private set; }

	public UnityAction<DisconnectionErrorCode> OnDisconnected { get; private set; }

	[UsedImplicitly]
	private void OnDestroy()
	{
		HostingEndedEvent.RemoveAllListeners();
		HostingStartedEvent.RemoveAllListeners();
		OnConnectionEstablished = null;
		OnConnectionStateUpdated = null;
		OnConnectionFailed = null;
		OnDisconnected = null;
	}

	public void OnEnable()
	{
		ActionProcessor.MaxConsecutiveIncorrectActionsAllowed = Mathf.RoundToInt(incorrectActionDetectedTimeOutDuration / ActionQueueProcessingInterval);
		BoltRuntimeSettings.instance.consoleVisibleByDefault = false;
		BoltRuntimeSettings.instance.consoleToggleKey = KeyCode.None;
		SwitchRegion(PhotonRegion.Regions.US, updateImmediately: false);
		NetworkConfig = BoltRuntimeSettings.instance.GetConfigCopy();
		NetworkConfig.connectionTimeout = 30000;
		NetworkConfig.connectionRequestTimeout = 1000;
		NetworkConfig.connectionRequestAttempts = 20;
		NetworkConfig.serverConnectionAcceptMode = BoltConnectionAcceptMode.Manual;
		NetworkConfig.packetSize = 1200;
		NetworkConfig.disableAutoSceneLoading = true;
		NetworkConfig.useNetworkSimulation = false;
		NetworkConfig.packetStreamSize = 1024;
		Console.LogInfo("NetworkManager enabled.");
	}

	public void Reset(bool resetHostingEndedEvent = true)
	{
		Timing.KillCoroutines(joinSessionRoutine);
		HostingStartedEvent.RemoveAllListeners();
		if (resetHostingEndedEvent)
		{
			HostingEndedEvent.RemoveAllListeners();
		}
		OnConnectionEstablished = null;
		OnConnectionStateUpdated = null;
		OnConnectionFailed = null;
		OnDisconnected = null;
		JoinSessionOnceClientStarted = false;
		ConnectingToSession = false;
		cancelingJoining = false;
		SessionID = string.Empty;
	}

	private void SetupLocalMPDebugging()
	{
		if (LocalMultiplayer)
		{
			BoltRuntimeSettings instance = BoltRuntimeSettings.instance;
			serverEndPoint = new UdpEndPoint(UdpIPv4Address.Localhost, (ushort)instance.debugStartPort);
			clientEndPoint = new UdpEndPoint(UdpIPv4Address.Localhost, 0);
			NetworkConfig.connectionTimeout = 60000000;
			NetworkConfig.connectionRequestTimeout = 500;
			NetworkConfig.connectionRequestAttempts = 1000;
			Console.LogInfo("Using Local UdpEndPoint for multiplayer.");
		}
	}

	public void SwitchRegion(PhotonRegion.Regions regionType, bool updateImmediately = true, Action callback = null)
	{
		PhotonRegion region = PhotonRegion.GetRegion(regionType);
		BoltRuntimeSettings.instance.UpdateBestRegion(region);
		if (updateImmediately)
		{
			BoltLauncher.SetUdpPlatform(new PhotonPlatform());
			callback?.Invoke();
		}
		Console.LogInfo("Region set to " + region.Name);
	}

	public async void ToggleServer(UnityAction onHostingStarted = null, UnityAction onHostingEnded = null)
	{
		if (!FFSNetwork.IsOnline && !FFSNetwork.IsStartingUp)
		{
			Debug.Log("About to start a server.");
			FFSNetwork.StartUp();
			if (onHostingStarted != null)
			{
				HostingStartedEvent.AddListener(onHostingStarted);
			}
			if (onHostingEnded != null)
			{
				HostingEndedEvent.AddListener(onHostingEnded);
			}
			HostingStartedEvent.AddListener(StartVoiceHost);
			HostingEndedEvent.AddListener(StopVoiceHost);
			if (LocalMultiplayer)
			{
				SetupLocalMPDebugging();
				BoltLauncher.StartServer(serverEndPoint, NetworkConfig);
			}
			else
			{
				SwitchRegion(PhotonRegion.Regions.US, updateImmediately: true, delegate
				{
					BoltLauncher.StartServer(NetworkConfig);
				});
			}
		}
		else
		{
			FFSNetwork.Shutdown(new DisconnectionErrorToken(DisconnectionErrorCode.HostEndedSession));
			if (global::Singleton<UIResultsManager>.Instance != null && global::Singleton<UIResultsManager>.Instance.IsShown)
			{
				global::Singleton<UIResultsManager>.Instance.MPSessionEndedOnResults();
			}
		}
	}

	private void StartVoiceHost()
	{
		Debug.Log("[NetworkManager]About to start a Voice Host.");
	}

	private void StopVoiceHost()
	{
		Debug.Log("[NetworkManager]About to stop a Voice Host.");
	}

	public void ToggleClient()
	{
		if (!FFSNetwork.IsOnline && !FFSNetwork.IsStartingUp)
		{
			StartClient();
		}
		else
		{
			FFSNetwork.Shutdown();
		}
	}

	public void StartClient()
	{
		Debug.Log("About to start the client.");
		FFSNetwork.StartUp();
		if (LocalMultiplayer)
		{
			SetupLocalMPDebugging();
			BoltLauncher.StartClient(clientEndPoint, NetworkConfig);
			OnConnectionStateUpdated?.Invoke(ConnectionState.StartingClient);
		}
		else if (SessionID.Length > 0)
		{
			(PhotonRegion.Regions, char) tuple = Utility.RegionCodePrefixes.FirstOrDefault(((PhotonRegion.Regions, char) x) => x.Item2 == SessionID[0]);
			if (tuple.Item2.Equals('\0'))
			{
				Debug.LogError("Error joining the session. Invalid code used.");
				OnConnectionFailed?.Invoke(ConnectionErrorCode.InvalidCode);
				Reset();
				FFSNetwork.IsStartingUp = false;
			}
			else
			{
				SwitchRegion(tuple.Item1, updateImmediately: true, delegate
				{
					BoltLauncher.StartClient(NetworkConfig);
					OnConnectionStateUpdated?.Invoke(ConnectionState.StartingClient);
				});
			}
		}
		else
		{
			SwitchRegion(PhotonRegion.Regions.US, updateImmediately: true, delegate
			{
				BoltLauncher.StartClient(NetworkConfig);
				OnConnectionStateUpdated?.Invoke(ConnectionState.StartingClient);
			});
		}
	}

	public void CreateSession()
	{
		GenerateSessionID();
		BoltMatchmaking.CreateSession(SessionID);
		HostingStartedEvent?.Invoke();
	}

	public void GenerateSessionID()
	{
		if (LocalMultiplayer)
		{
			SessionID = "Local Host";
		}
		else
		{
			SessionID = Utility.GetRandomCode(inviteCodeLength, includeRegionBasedPrefix: true);
		}
	}

	public void TryJoinPendingSession()
	{
		if (JoinSessionOnceClientStarted)
		{
			Console.LogInfo("About to join a pending session.");
			if (SessionID.IsNullOrEmpty())
			{
				JoinSession();
			}
			else
			{
				JoinSession(SessionID);
			}
		}
	}

	public void JoinSession(Text sessionIDText)
	{
		if (sessionIDText != null)
		{
			if (sessionIDText.text.IsNullOrEmpty())
			{
				JoinSession();
			}
			else
			{
				JoinSession(sessionIDText.text);
			}
		}
		else
		{
			Debug.LogError("Tried joining a session by session ID but the Text component provided returns null.");
		}
	}

	public void JoinSession(string sessionID = "", UnityAction<CustomDataToken> onConnectionEstablished = null, UnityAction<ConnectionState> onConnectionStateUpdated = null, UnityAction<ConnectionErrorCode> onConnectionFailed = null, UnityAction<DisconnectionErrorCode> onDisconnected = null, int debugPort = -1)
	{
		Debug.Log("[NetworkManager] JoinSession(" + sessionID + ") Called \n" + Environment.StackTrace);
		PlatformLayer.Networking.CheckForPrivilegeValidityAsync(Privilege.Multiplayer, delegate(bool isMultiplayerValid)
		{
			Debug.Log("[NetworkManager] Response from CheckForPrivilegeValidityAsync(): isMultiplayerValid = " + isMultiplayerValid + " ");
			if (isMultiplayerValid)
			{
				if (PlatformLayer.Instance.IsDelayedInit && SaveData.Instance.Global.CrossplayEnabled)
				{
					PlatformLayer.Networking.GetCurrentUserPrivilegesAsync(OnCheckPrivilegeValidity, PrivilegePlatform.AllExceptSwitch, Privilege.CrossPlay);
				}
				else
				{
					JoinSessionAfterMultiplayerValidation();
				}
			}
			else
			{
				PlatformLayer.Networking.LeaveSession(null);
				onConnectionFailed?.Invoke(ConnectionErrorCode.MultiplayerValidationFail);
			}
		}, PrivilegePlatform.AllExceptSwitch);
		void JoinSessionAfterMultiplayerValidation()
		{
			if (ConnectingToSession && !JoinSessionOnceClientStarted)
			{
				Console.LogError("ERROR_MULTIPLAYER_00032", "We are already connecting to a session.");
			}
			else
			{
				ConnectingToSession = true;
				SessionID = sessionID;
				DebugPort = debugPort;
				if (!JoinSessionOnceClientStarted)
				{
					OnConnectionEstablished = onConnectionEstablished;
					OnConnectionStateUpdated = onConnectionStateUpdated;
					OnConnectionFailed = onConnectionFailed;
					OnDisconnected = onDisconnected;
				}
				if (BoltNetwork.IsRunning)
				{
					OnConnectionStateUpdated?.Invoke(ConnectionState.SearchingForSession);
					if (SessionID.Equals(string.Empty))
					{
						Console.LogInfo("About to join a random session.");
						JoinSessionOnceClientStarted = false;
						GetUserToken(BoltMatchmaking.JoinRandomSession);
					}
					else
					{
						Console.LogInfo("About to join a session by ID: " + sessionID);
						JoinSessionOnceClientStarted = false;
						if (LocalMultiplayer)
						{
							joinSessionRoutine = Timing.RunCoroutine(JoinLocalEndPoint());
						}
						else
						{
							GetUserToken(delegate(UserToken userToken)
							{
								BoltMatchmaking.JoinSession(SessionID, userToken);
							});
						}
					}
				}
				else
				{
					JoinSessionOnceClientStarted = true;
					StartClient();
				}
			}
		}
		void OnCheckPrivilegeValidity(OperationResult operationResult, bool privilegeProvided)
		{
			SaveData.Instance.Global.CrossplayEnabled = privilegeProvided;
			JoinSessionAfterMultiplayerValidation();
		}
	}

	private IEnumerator<float> JoinLocalEndPoint()
	{
		Console.LogInfo("Connecting To EndPoint:" + BoltRuntimeSettings.instance.debugStartPort);
		OnConnectionStateUpdated?.Invoke(ConnectionState.Connecting);
		bool isConnected = false;
		GetUserToken(delegate(UserToken userToken)
		{
			BoltNetwork.Connect((ushort)BoltRuntimeSettings.instance.debugStartPort, userToken);
			isConnected = true;
		});
		while (!isConnected)
		{
			yield return 0f;
		}
		yield return 0f;
	}

	private void GetUserToken(Action<UserToken> onCallback)
	{
		string userName = PlatformLayer.UserData.UserName;
		string platformPlayerID = PlatformLayer.UserData.PlatformPlayerID;
		string platformID = PlatformLayer.Instance.PlatformID;
		bool crossplayEnabled = SaveData.Instance.Global.CrossplayEnabled;
		string empty = string.Empty;
		string empty2 = string.Empty;
		string platformNetworkAccountPlayerID = PlatformLayer.UserData.PlatformNetworkAccountPlayerID;
		byte[] recentPlayerKey = null;
		if (Application.platform == RuntimePlatform.Switch)
		{
			recentPlayerKey = PlatformLayer.Networking.GetRecentPlayerKey();
		}
		empty = NetworkVersion.Current;
		UserToken userToken = new UserToken(0, string.Empty, userName, platformPlayerID, empty, empty2, platformID, crossplayEnabled, platformNetworkAccountPlayerID, recentPlayerKey);
		switch (PlatformLayer.Instance.PlatformID)
		{
		case "Standalone":
		case "Steam":
		case "GoGGalaxy":
		case "EpicGamesStore":
			userToken.MaskBadWordsInUsername(delegate
			{
				onCallback?.Invoke(userToken);
			});
			break;
		default:
			onCallback?.Invoke(userToken);
			break;
		}
	}

	public void CancelJoiningSession(UnityAction onJoiningCanceled = null)
	{
		if (!cancelingJoining)
		{
			Console.LogInfo("Canceling session join.");
			cancelingJoining = true;
			OnConnectionStateUpdated?.Invoke(ConnectionState.CancelingConnection);
			FFSNetwork.Shutdown(null, delegate
			{
				cancelingJoining = false;
				onJoiningCanceled?.Invoke();
			});
		}
	}

	public void PrintSessionInfo()
	{
		Console.LogInfo("Printing session info.");
		UdpSession currentSession = BoltMatchmaking.CurrentSession;
		Console.LogInfo("Session ID: " + currentSession.HostName);
		if (currentSession is PhotonSession photonSession)
		{
			Console.LogInfo("IsOpen: " + photonSession.IsOpen + ", IsVisible: " + photonSession.IsVisible);
			foreach (object key in photonSession.Properties.Keys)
			{
				Console.LogInfo(key?.ToString() + " = " + photonSession.Properties[key]);
			}
		}
		if (PlayerRegistry.MyPlayer != null)
		{
			Console.LogInfo("Username: " + PlayerRegistry.MyPlayer.Username);
			Console.LogInfo("Game State : Action #" + PlayerRegistry.MyPlayer.state.LatestProcessedActionID);
		}
		else
		{
			Console.LogInfo("No a network player initialized yet for this connection.");
		}
	}
}
