#define DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UdpKit.Platform.Photon.Coroutine;
using UdpKit.Platform.Photon.Realtime;
using UdpKit.Platform.Photon.Utils;
using UdpKit.Utils;
using UnityEngine;

namespace UdpKit.Platform.Photon;

internal abstract class PhotonPoller : MonoBehaviour, IOnEventCallback, IMatchmakingCallbacks, IInRoomCallbacks
{
	internal class PhotonPacket
	{
		public byte[] Data;

		public int Remote;

		public PhotonPacket()
		{
		}

		public PhotonPacket(int size)
		{
			Data = new byte[size];
		}
	}

	private const byte DATA_EVENT_CODE = 150;

	private const byte PING_EVENT_CODE = 151;

	private const byte PING_CANCEL_EVENT_CODE = 152;

	private object _joinToken;

	private ConnectionControl _currentConnectionControl = default(ConnectionControl);

	private volatile Action<bool, UdpSessionError> _currentAsyncCallback;

	private volatile IEnumerator _currentRoutine;

	private volatile IEnumerator _currentTimeoutRoutine;

	private PhotonPlatformConfig _config;

	private PhotonClient _lbClient;

	private UdpEndPoint _serverRelayEndPoint = UdpEndPoint.Any;

	private PhotonPlatform _platform;

	private readonly ISynchronizedQueue<PhotonPacket> _packetPool = new SynchronizedQueue<PhotonPacket>();

	private readonly ISynchronizedQueue<PhotonPacket> _packetSend = new SynchronizedQueue<PhotonPacket>();

	private readonly ISynchronizedQueue<PhotonPacket> _packetRecv = new SynchronizedQueue<PhotonPacket>();

	private static PhotonPoller _instance;

	private static Type _externalType;

	private readonly RaiseEventOptions _cachedEvtOptions = new RaiseEventOptions
	{
		CachingOption = EventCaching.DoNotCache,
		TargetActors = new int[1]
	};

	private readonly SendOptions _cachedEvtSendOptionsUnreliable = new SendOptions
	{
		DeliveryMode = DeliveryMode.Unreliable,
		Channel = 0
	};

	private readonly SendOptions _cachedEvtSendOptionsReliable = new SendOptions
	{
		DeliveryMode = DeliveryMode.ReliableUnsequenced,
		Channel = 0
	};

	private readonly Dictionary<ConnectState, ConnectState[]> _stateTransitions = new Dictionary<ConnectState, ConnectState[]>
	{
		{
			ConnectState.Idle,
			new ConnectState[3]
			{
				ConnectState.JoinRoomPending,
				ConnectState.DisconnectPending,
				ConnectState.CreateRoomPending
			}
		},
		{
			ConnectState.CreateRoomPending,
			new ConnectState[2]
			{
				ConnectState.Connected,
				ConnectState.CreateRoomFailed
			}
		},
		{
			ConnectState.JoinRoomPending,
			new ConnectState[3]
			{
				ConnectState.JoinRoomFailed,
				ConnectState.DirectPending,
				ConnectState.RelayPending
			}
		},
		{
			ConnectState.DirectPending,
			new ConnectState[4]
			{
				ConnectState.DirectSuccess,
				ConnectState.DirectFailed,
				ConnectState.Refused,
				ConnectState.JoinRoomFailed
			}
		},
		{
			ConnectState.DirectSuccess,
			new ConnectState[1] { ConnectState.Connected }
		},
		{
			ConnectState.DirectFailed,
			new ConnectState[1] { ConnectState.RelayPending }
		},
		{
			ConnectState.RelayPending,
			new ConnectState[4]
			{
				ConnectState.RelayFailed,
				ConnectState.RelaySuccess,
				ConnectState.Refused,
				ConnectState.JoinRoomFailed
			}
		},
		{
			ConnectState.RelaySuccess,
			new ConnectState[1] { ConnectState.Connected }
		},
		{
			ConnectState.RelayFailed,
			new ConnectState[1]
		},
		{
			ConnectState.Connected,
			new ConnectState[1] { ConnectState.DisconnectPending }
		},
		{
			ConnectState.CreateRoomFailed,
			new ConnectState[1]
		},
		{
			ConnectState.DisconnectPending,
			new ConnectState[1]
		},
		{
			ConnectState.JoinRoomFailed,
			new ConnectState[1]
		},
		{
			ConnectState.Refused,
			new ConnectState[1]
		}
	};

	private readonly string[] defaultPublicProperties = new string[2] { "UdpSessionId", "UserToken" };

	public static PhotonPoller Instance => _instance;

	public LoadBalancingClient LoadBalancerClient => _lbClient;

	private ConnectState ConnectState { get; set; }

	private UdpConnectionDisconnectReason LastDisconnectReason { get; set; } = UdpConnectionDisconnectReason.Unknown;

	private UdpEndPoint ServerRelayEndPoint
	{
		get
		{
			if (_lbClient.IsJoined())
			{
				if (_serverRelayEndPoint == UdpEndPoint.Any)
				{
					_serverRelayEndPoint = new UdpEndPoint(new UdpIPv4Address((uint)_lbClient.CurrentRoom.MasterClientId), 0);
				}
				return _serverRelayEndPoint;
			}
			return UdpEndPoint.Any;
		}
	}

	public event Action OnUpdate;

	static PhotonPoller()
	{
		_instance = null;
		_externalType = null;
	}

	public static void CreatePoller(PhotonPlatform photonPlatform, PhotonPlatformConfig config, bool force = false)
	{
		if (force && _instance != null)
		{
			UnityEngine.Object.Destroy(_instance.gameObject);
			_instance = null;
		}
		if (_instance == null)
		{
			PhotonPoller[] array = UnityEngine.Object.FindObjectsOfType<PhotonPoller>();
			if (array.Length == 0)
			{
				InstatiatePoller(out _instance);
			}
			else if (array.Length >= 1)
			{
				_instance = array[0];
				for (int i = 1; i < array.Length; i++)
				{
					UnityEngine.Object.Destroy(array[i].gameObject);
				}
			}
			_instance._config = config;
			_instance._platform = photonPlatform;
			_instance.gameObject.AddComponent<RoutineManager>();
			UnityEngine.Object.DontDestroyOnLoad(_instance);
		}
		if (_instance != null)
		{
			_instance.OnUpdate = delegate
			{
			};
		}
		_instance.LastDisconnectReason = UdpConnectionDisconnectReason.Unknown;
	}

	protected internal void StartPhotonClient(UdpEndPoint localEndPoint, Action<bool, UdpConnectionDisconnectReason> doneCallback)
	{
		if (_lbClient == null)
		{
			if (string.IsNullOrEmpty(_config.AppId))
			{
				UdpLog.Error("[{0}] Empty AppID. Please fill the Application ID on Bolt Settings", "PhotonPoller");
				doneCallback(arg1: false, UdpConnectionDisconnectReason.Error);
			}
			else
			{
				ChangeState(ConnectState.Idle);
				SetupPhotonClient(localEndPoint, doneCallback);
			}
		}
	}

	private void SetupPhotonClient(UdpEndPoint localEndPoint, Action<bool, UdpConnectionDisconnectReason> doneCallback)
	{
		UdpLog.Info("[{0}] Starting LBC with Protocol: {1}", "PhotonPoller", _config.ConnectionProtocol);
		_lbClient = new PhotonClient(_config);
		if (_config.CurrentPlatform == RuntimePlatform.Switch)
		{
			_lbClient.LoadBalancingPeer.SocketImplementationConfig[ConnectionProtocol.Udp] = typeof(SocketUdpPool);
		}
		IEnumerator asyncCancel = RoutineManager.EnqueueRoutine(StartClientTimeout(_config.BackgroundConnectionTimeout + _lbClient.LoadBalancingPeer.DisconnectTimeout, doneCallback));
		if (!_lbClient.Connect(_config.Region, callbackHandler))
		{
			callbackHandler(result: false, UdpConnectionDisconnectReason.Error);
			return;
		}
		UdpLog.Info("[{0}] Running background connection with Region Master", "PhotonPoller");
		void callbackHandler(bool result, UdpConnectionDisconnectReason disconnectReason)
		{
			UdpLog.Info("[{0}] Initialization callback: {1}", "PhotonPoller", result);
			UdpLog.Info("[{0}] Cancel Startup timeout routine", "PhotonPoller");
			RoutineManager.CancelRoutine(asyncCancel);
			if (!result)
			{
				UdpLog.Error("[{0}] Failed to connect to Region Master: {1}", "PhotonPoller", disconnectReason);
				StopPhotonClient();
			}
			else
			{
				_lbClient.OpResponseReceived += HandlerOperationResponse;
				_lbClient.StateChanged += HandleStateChanged;
				_lbClient.AddCallbackTarget(this);
			}
			doneCallback(result, disconnectReason);
		}
	}

	private IEnumerator StartClientTimeout(float timeout, Action<bool, UdpConnectionDisconnectReason> callback)
	{
		UdpLog.Info("[{0}] Starting timeout watcher: {1}", "PhotonPoller", timeout / 1000f);
		yield return new WaitForSecondsRealtime(timeout / 1000f);
		callback?.Invoke(arg1: false, UdpConnectionDisconnectReason.CloudTimeout);
	}

	public static void StopPhotonClient()
	{
		Instance._lbClient?.Disable();
	}

	protected void Update()
	{
		try
		{
			if (_lbClient == null)
			{
				return;
			}
			PollOut();
			_lbClient.Update();
		}
		catch (Exception ex)
		{
			UdpLog.Error(ex.StackTrace);
			UdpLog.Error("Exception on Photon Loop, disconnect");
			BoltDisconnect();
		}
		if (this.OnUpdate != null)
		{
			this.OnUpdate();
		}
	}

	internal static void StartDone(UdpEndPoint localEndPoint, Action<bool, UdpConnectionDisconnectReason> doneCallback)
	{
		Instance.StartPhotonClient(localEndPoint, doneCallback);
	}

	internal static void RegisterInstance<T>() where T : PhotonPoller
	{
		_externalType = typeof(T);
	}

	private static void InstatiatePoller(out PhotonPoller photonPoller)
	{
		if (_externalType != null)
		{
			photonPoller = new GameObject(_externalType.Name).AddComponent(_externalType) as PhotonPoller;
			return;
		}
		throw new Exception("Error loading external implementation of PhotonPoller");
	}

	private void OnDestroy()
	{
		StopPhotonClient();
	}

	private void OnDisable()
	{
		StopPhotonClient();
	}

	internal static void ConnectAttempt(UdpEndPoint endpoint)
	{
		ConnectState connectState = Instance.ConnectState;
		ConnectState connectState2 = connectState;
		if (connectState2 != ConnectState.DirectPending)
		{
			return;
		}
		Instance._currentConnectionControl.ConnectionAttempts++;
		if (Instance._currentConnectionControl.ConnectionLocal)
		{
			if (Instance._currentConnectionControl.ConnectionAttempts >= Instance._currentConnectionControl.ConnectionThreshold)
			{
				Instance._currentConnectionControl.ConnectionLocal = false;
				BoltCancelConnect(endpoint);
				BoltConnect(Instance._currentConnectionControl.WanEndPoint);
			}
		}
		else if (Instance._currentConnectionControl.ConnectionAttempts >= Instance._currentConnectionControl.ConnectionTrials - 2)
		{
			BoltCancelConnect(endpoint);
			ChangeState(ConnectState.DirectFailed);
		}
	}

	internal static void ConnectFailed(bool refused = false)
	{
		if (refused)
		{
			ChangeState(ConnectState.Refused);
			return;
		}
		switch (Instance.ConnectState)
		{
		case ConnectState.DirectPending:
			ChangeState(ConnectState.DirectFailed);
			break;
		case ConnectState.RelayPending:
			ChangeState(ConnectState.RelayFailed);
			break;
		}
	}

	internal static void Connected(UdpConnection connection)
	{
		switch (Instance.ConnectState)
		{
		case ConnectState.DirectPending:
			connection.ConnectionType = UdpConnectionType.Direct;
			ChangeState(ConnectState.DirectSuccess);
			RoutineManager.CancelRoutine(Instance._currentRoutine, Instance._currentTimeoutRoutine);
			Instance.SendRelayReliable(152, Instance._lbClient.CurrentMasterId, null);
			break;
		case ConnectState.RelayPending:
			connection.ConnectionType = UdpConnectionType.Relayed;
			ChangeState(ConnectState.RelaySuccess);
			RoutineManager.CancelRoutine(Instance._currentRoutine, Instance._currentTimeoutRoutine);
			Instance.SendRelayReliable(152, Instance._lbClient.CurrentMasterId, null);
			break;
		case ConnectState.Connected:
			if (!BoltIsClient())
			{
				if (!connection.RemoteEndPoint.IPv6)
				{
					connection.ConnectionType = ((connection.RemoteEndPoint.Port != 0) ? UdpConnectionType.Direct : UdpConnectionType.Relayed);
				}
				else
				{
					connection.ConnectionType = UdpConnectionType.Direct;
				}
			}
			break;
		}
	}

	private void HandlerOperationResponse(OperationResponse response)
	{
		switch (response.OperationCode)
		{
		case 230:
		case 231:
			if (response.ReturnCode != 0)
			{
				UdpLog.Error("Authenticate operation failed with code: {0}", response.ReturnCode);
				ChangeState(ConnectState.DisconnectPending);
			}
			break;
		case 227:
			if (response.ReturnCode != 0)
			{
				UdpLog.Error("Create Room operation failed with code: {0}", response.ReturnCode);
			}
			break;
		case 226:
			if (response.ReturnCode != 0)
			{
				UdpLog.Error("Join Room operation failed with code: {0}", response.ReturnCode);
			}
			break;
		case 252:
			if (response.ReturnCode != 0)
			{
				UdpLog.Error("Update Room properties failed with code: {0}", response.ReturnCode);
			}
			else
			{
				UdpLog.Info("Update room properties success");
			}
			break;
		default:
			if (response.ReturnCode != 0)
			{
				UdpLog.Error("Error on Operation {0}({1}): {2}", response.OperationCode, response.ReturnCode, response.DebugMessage);
			}
			break;
		}
		switch (response.ReturnCode)
		{
		case 32766:
			UdpLog.Error("Create Session failed: GameIdAlreadyExists");
			ChangeState(ConnectState.CreateRoomFailed, response.ReturnCode);
			break;
		case 32760:
			UdpLog.Error("Join Session failed: NoRandomMatchFound");
			ChangeState(ConnectState.JoinRoomFailed, response.ReturnCode);
			break;
		case 32765:
			UdpLog.Error("Join Session failed: GameFull");
			ChangeState(ConnectState.JoinRoomFailed, response.ReturnCode);
			break;
		case 32764:
			UdpLog.Error("Join Session failed: GameClosed");
			ChangeState(ConnectState.JoinRoomFailed, response.ReturnCode);
			break;
		case 32758:
			UdpLog.Error("Join Session failed: GameDoesNotExist");
			ChangeState(ConnectState.JoinRoomFailed, response.ReturnCode);
			break;
		case -2:
			switch (response.OperationCode)
			{
			case 227:
				UdpLog.Error("Create Session failed: InvalidOperation");
				ChangeState(ConnectState.CreateRoomFailed, response.ReturnCode);
				break;
			case 226:
				UdpLog.Error("Join Session failed: InvalidOperation");
				ChangeState(ConnectState.JoinRoomFailed, response.ReturnCode);
				break;
			}
			break;
		}
	}

	public void OnEvent(EventData evt)
	{
		switch (evt.Code)
		{
		case 150:
		{
			int remote = (int)evt.Parameters[254];
			byte[] data = (byte[])evt.Parameters[245];
			_packetRecv.Enqueue(new PhotonPacket
			{
				Data = data,
				Remote = remote
			});
			break;
		}
		case 151:
		{
			int playerID2 = (int)evt.Parameters[254];
			if (evt.Parameters[245] is byte[] encodedEndPoint)
			{
				_platform.stunManager.RegisterPingTarget(playerID2, encodedEndPoint.DeserializeEndPoint());
			}
			break;
		}
		case 152:
		{
			int playerID = (int)evt.Parameters[254];
			_platform.stunManager.RemovePingTarget(playerID);
			break;
		}
		}
	}

	private void HandleStateChanged(ClientState fromState, ClientState toState)
	{
		switch (toState)
		{
		case ClientState.Disconnected:
		{
			UdpConnectionDisconnectReason udpConnectionDisconnectReason = ConvertDisconnectReason(Instance._lbClient.DisconnectedCause);
			bool flag = false;
			UdpConnectionDisconnectReason udpConnectionDisconnectReason2 = udpConnectionDisconnectReason;
			UdpConnectionDisconnectReason udpConnectionDisconnectReason3 = udpConnectionDisconnectReason2;
			if (udpConnectionDisconnectReason3 == UdpConnectionDisconnectReason.CloudTimeout && LastDisconnectReason != UdpConnectionDisconnectReason.CloudTimeout && Instance._lbClient.ReconnectAndRejoin())
			{
				flag = true;
			}
			LastDisconnectReason = udpConnectionDisconnectReason;
			if (!flag)
			{
				ChangeState(ConnectState.DisconnectPending);
			}
			break;
		}
		case ClientState.Joined:
			_packetSend.Clear();
			_packetRecv.Clear();
			break;
		}
	}

	private void PollOut()
	{
		if (_lbClient != null && _lbClient.LoadBalancingPeer != null && _lbClient.IsConnectedAndReady)
		{
			PhotonPacket item;
			while (_packetSend.TryDequeue(out item))
			{
				SendRelayUNreliable(150, item.Remote, item.Data);
			}
		}
	}

	private void SendRelayUNreliable(byte code, int target, object data)
	{
		_cachedEvtOptions.TargetActors[0] = target;
		_lbClient.OpRaiseEvent(code, data ?? new byte[0], _cachedEvtOptions, _cachedEvtSendOptionsUnreliable);
	}

	private void SendRelayReliable(byte code, int target, object data)
	{
		_cachedEvtOptions.TargetActors[0] = target;
		_lbClient.OpRaiseEvent(code, data ?? new byte[0], _cachedEvtOptions, _cachedEvtSendOptionsReliable);
	}

	internal static void OnConnectStateChange(ConnectState state, int errorCode = 0)
	{
		bool flag = false;
		switch (state)
		{
		case ConnectState.Connected:
			Instance._currentAsyncCallback?.Invoke(arg1: true, ConvertErrorCode(errorCode));
			Instance._currentAsyncCallback = null;
			RoutineManager.CancelRoutine(Instance._currentRoutine, Instance._currentTimeoutRoutine);
			break;
		case ConnectState.DirectSuccess:
		case ConnectState.RelaySuccess:
			ChangeState(ConnectState.Connected);
			break;
		case ConnectState.CreateRoomFailed:
			UdpLog.Warn("Failed to Create/Update Room.");
			Instance._currentAsyncCallback?.Invoke(arg1: false, ConvertErrorCode(errorCode));
			Instance._currentAsyncCallback = null;
			flag = true;
			break;
		case ConnectState.Refused:
			UdpLog.Warn("Connection refused.");
			flag = true;
			break;
		case ConnectState.JoinRoomFailed:
			UdpLog.Warn("Failed to Join Room.");
			Instance._currentAsyncCallback?.Invoke(arg1: false, ConvertErrorCode(errorCode));
			Instance._currentAsyncCallback = null;
			flag = true;
			break;
		case ConnectState.DirectFailed:
			UdpLog.Warn("Failed to Direct Connect.");
			break;
		case ConnectState.RelayFailed:
			UdpLog.Warn("Failed to Relay Connect.");
			flag = true;
			break;
		case ConnectState.DisconnectPending:
			UdpLog.Warn("Disconnecting...");
			BoltDisconnect();
			Instance._lbClient = null;
			break;
		}
		if (flag && Instance != null)
		{
			ChangeState(ConnectState.Idle);
			RoutineManager.CancelRoutine(Instance._currentRoutine, Instance._currentTimeoutRoutine);
			Instance._lbClient?.JoinLobby();
			Instance._joinToken = null;
		}
	}

	internal static bool SetHostInfo(UdpSession session, Action<bool, UdpSessionError> result)
	{
		if (Instance.ConnectState == ConnectState.CreateRoomPending)
		{
			UdpLog.Warn("Already attempting to create a photon room");
			return false;
		}
		RoutineManager.CancelRoutine(Instance._currentTimeoutRoutine);
		Instance.LastDisconnectReason = UdpConnectionDisconnectReason.Unknown;
		Instance._currentRoutine = RoutineManager.EnqueueRoutine(Instance.SetHostInfoRoutine(session, delegate
		{
			Instance._currentTimeoutRoutine = RoutineManager.EnqueueRoutine(Instance.FailedCreateCallback());
		}, result));
		return true;
	}

	public static bool JoinSession(UdpSession session, object token, Action<bool, UdpSessionError> result)
	{
		if (session.Source == UdpSessionSource.Photon)
		{
			if (Instance.ConnectState != ConnectState.Idle)
			{
				UdpLog.Warn("Already attempting connection to a photon room");
				return true;
			}
			RoutineManager.CancelRoutine(Instance._currentTimeoutRoutine);
			Instance.LastDisconnectReason = UdpConnectionDisconnectReason.Unknown;
			Instance._currentRoutine = RoutineManager.EnqueueRoutine(Instance.JoinSessionRoutine(session, token, result, delegate
			{
				Instance._currentTimeoutRoutine = RoutineManager.EnqueueRoutine(Instance.FailedJoinCallback());
			}));
			return true;
		}
		return false;
	}

	public static bool JoinRandomSession(UdpSessionFilter sessionFilter, object token, Action<bool, UdpSessionError> result)
	{
		if (Instance.ConnectState != ConnectState.Idle)
		{
			UdpLog.Warn("Already attempting connection to a photon room");
			return true;
		}
		RoutineManager.CancelRoutine(Instance._currentTimeoutRoutine);
		Instance.LastDisconnectReason = UdpConnectionDisconnectReason.Unknown;
		Instance._currentRoutine = RoutineManager.EnqueueRoutine(Instance.JoinSessionRandomRoutine(sessionFilter, token, result, delegate
		{
			Instance._currentTimeoutRoutine = RoutineManager.EnqueueRoutine(Instance.FailedJoinCallback());
		}));
		return true;
	}

	public static string GetConnectedRegion()
	{
		if (Instance._lbClient != null && Instance._lbClient.IsConnectedAndReady)
		{
			return Instance._lbClient.CloudRegion.Replace("/*", "");
		}
		return null;
	}

	public static Dictionary<string, object> GetCustomAuthData()
	{
		if (Instance._lbClient != null && Instance._lbClient.IsConnectedAndReady)
		{
			return Instance._lbClient.CustomAuthData ?? null;
		}
		return null;
	}

	public static string GetUserId()
	{
		if (Instance._lbClient != null && Instance._lbClient.IsConnectedAndReady)
		{
			return Instance._lbClient.UserId;
		}
		return null;
	}

	public static string GetNickName()
	{
		if (Instance._lbClient != null && Instance._lbClient.IsConnectedAndReady)
		{
			return Instance._lbClient.NickName;
		}
		return null;
	}

	public static UdpSession GetSession()
	{
		if (Instance._lbClient != null && Instance._lbClient.State == ClientState.Joined)
		{
			return PhotonSession.Convert(Instance._lbClient.CurrentRoom);
		}
		return null;
	}

	public static TypedLobbyInfo GetLobbyStatistics()
	{
		if (Instance._lbClient != null && Instance._lbClient.IsConnectedAndReady)
		{
			return Instance._lbClient.lastLobbyStatistics;
		}
		return null;
	}

	public static bool SessionHasChanged()
	{
		if (Instance._lbClient != null)
		{
			return Instance._lbClient.RoomInfoListHasChanged;
		}
		return false;
	}

	public static List<UdpSession> GetSessions()
	{
		List<UdpSession> list = new List<UdpSession>();
		if (Instance._lbClient != null && Instance._lbClient.State == ClientState.JoinedLobby && Instance._lbClient.RoomInfoList != null)
		{
			foreach (RoomInfo roomInfo in Instance._lbClient.RoomInfoList)
			{
				try
				{
					if (roomInfo.CustomProperties.ContainsKey("UdpSessionId"))
					{
						list.Add(PhotonSession.Convert(roomInfo));
					}
				}
				catch (Exception ex)
				{
					UdpLog.Error(ex.Message);
				}
			}
		}
		return list;
	}

	[Obfuscation(Exclude = false, Feature = "rename")]
	internal static void UpdateBestRegion(PhotonRegion region)
	{
		Instance._config.UpdateBestRegion(region);
	}

	[Obfuscation(Exclude = false, Feature = "rename")]
	internal static void BoltConnect(UdpEndPoint endPoint)
	{
		Instance.BoltConnectInternal(endPoint, Instance._joinToken);
	}

	[Obfuscation(Exclude = false, Feature = "rename")]
	internal static void BoltCancelConnect(UdpEndPoint endPoint)
	{
		Instance.BoltCancelConnectInternal(endPoint);
	}

	[Obfuscation(Exclude = false, Feature = "rename")]
	internal static void BoltDisconnect()
	{
		Instance.BoltDisconnectInternal(ConvertDisconnectReason(Instance._lbClient.DisconnectedCause));
	}

	[Obfuscation(Exclude = false, Feature = "rename")]
	internal static void BoltUpdateSessionList(Map<Guid, UdpSession> sessions)
	{
		Instance.BoltUpdateSessionListInternal(sessions);
	}

	[Obfuscation(Exclude = false, Feature = "rename")]
	internal static bool BoltIsClient()
	{
		return Instance.BoltIsClientInternal();
	}

	[Obfuscation(Exclude = false, Feature = "rename")]
	internal static UdpEndPoint BoltEndPoint()
	{
		return Instance.BoltEndPointInternal();
	}

	[Obfuscation(Exclude = false, Feature = "rename")]
	protected internal abstract void BoltConnectInternal(UdpEndPoint endPoint, object token);

	[Obfuscation(Exclude = false, Feature = "rename")]
	protected internal abstract void BoltDisconnectInternal(UdpConnectionDisconnectReason disconnectedCause);

	[Obfuscation(Exclude = false, Feature = "rename")]
	protected internal abstract void BoltUpdateSessionListInternal(Map<Guid, UdpSession> sessions);

	[Obfuscation(Exclude = false, Feature = "rename")]
	protected internal abstract bool BoltIsClientInternal();

	[Obfuscation(Exclude = false, Feature = "rename")]
	protected internal abstract UdpEndPoint BoltEndPointInternal();

	[Obfuscation(Exclude = false, Feature = "rename")]
	protected internal abstract void BoltCancelConnectInternal(UdpEndPoint endPoint);

	internal PhotonPacket AllocPacket(int size)
	{
		if (_packetPool.TryDequeue(out var item))
		{
			Array.Resize(ref item.Data, size);
			return item;
		}
		return new PhotonPacket(size);
	}

	internal void FreePacket(PhotonPacket packet)
	{
		_packetPool.Enqueue(packet);
	}

	internal static int RecvFrom(byte[] buffer, ref UdpEndPoint endpoint)
	{
		if (Instance != null && Instance._packetRecv.TryDequeue(out var item))
		{
			Buffer.BlockCopy(item.Data, 0, buffer, 0, item.Data.Length);
			endpoint = new UdpEndPoint(new UdpIPv4Address((uint)item.Remote), 0);
			return item.Data.Length;
		}
		return -1;
	}

	internal static bool RecvPoll()
	{
		return Instance != null && Instance._packetRecv.Count > 0;
	}

	internal static int SendTo(byte[] buffer, int bytesToSend, UdpEndPoint endpoint)
	{
		PhotonPacket photonPacket = Instance.AllocPacket(bytesToSend);
		photonPacket.Remote = (int)endpoint.Address.Packed;
		Buffer.BlockCopy(buffer, 0, photonPacket.Data, 0, bytesToSend);
		Instance._packetSend.Enqueue(photonPacket);
		return bytesToSend;
	}

	private byte[] CloneArray(byte[] array, int size)
	{
		byte[] array2 = new byte[size];
		Buffer.BlockCopy(array, 0, array2, 0, size);
		return array2;
	}

	private IEnumerator SetHostInfoRoutine(UdpSession session, Action startTimer, Action<bool, UdpSessionError> result = null)
	{
		_currentAsyncCallback = result;
		yield return new WaitWhile(() => _lbClient == null || !_lbClient.IsConnectedAndReady);
		bool create = !_lbClient.InRoom;
		if (create)
		{
			ChangeState(ConnectState.CreateRoomPending);
		}
		yield return new WaitWhile(() => _lbClient == null || _lbClient.State != (ClientState)(create ? 4 : 9));
		if (create && _lbClient.State != ClientState.JoinedLobby)
		{
			UdpLog.Error("Can't call BoltNetwork.SetServerInfo when not in lobby");
			ChangeState(ConnectState.CreateRoomFailed);
			yield break;
		}
		if (!create && (_lbClient.State != ClientState.Joined || !_lbClient.LocalPlayer.IsMasterClient))
		{
			UdpLog.Error("Can't call BoltNetwork.SetServerInfo while not in a room or not Owner of the room");
			ChangeState(ConnectState.CreateRoomFailed);
			yield break;
		}
		int maxPlayers = (session.IsDedicatedServer ? (session.ConnectionsMax + 1) : session.ConnectionsMax);
		ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable();
		RoomOptions roomOptions = new RoomOptions();
		List<string> publicPropertyListName = new List<string>(defaultPublicProperties);
		object protocolToken = session.HostObject;
		byte[] protocolTokenData = session.HostData;
		if (protocolToken is IPhotonRoomPropertiesInternal externalRoomProperties)
		{
			foreach (KeyValuePair<object, object> kv in externalRoomProperties.CustomRoomProperties)
			{
				customRoomProperties[kv.Key] = kv.Value;
			}
			publicPropertyListName.AddRange(externalRoomProperties.CustomRoomPropertiesInLobby);
			roomOptions.IsOpen = externalRoomProperties.IsOpen;
			roomOptions.IsVisible = externalRoomProperties.IsVisible;
		}
		if (protocolToken != null && protocolTokenData != null)
		{
			customRoomProperties[PhotonSession.KEY_USER_TOKEN] = protocolTokenData;
		}
		if (create)
		{
			customRoomProperties[PhotonSession.KEY_UDP_SESSION_ID] = Guid.NewGuid().ToString();
			customRoomProperties[PhotonSession.KEY_PUNCH_ENABLED] = _config.UsePunchThrough;
			if (_config.UsePunchThrough)
			{
				customRoomProperties[PhotonSession.KEY_LAN_ENDPOINT] = session.LanEndPoint.SerializeEndPoint();
				customRoomProperties[PhotonSession.KEY_WAN_ENDPOINT] = session.WanEndPoint.SerializeEndPoint();
			}
		}
		roomOptions.CustomRoomProperties = customRoomProperties;
		roomOptions.CustomRoomPropertiesForLobby = publicPropertyListName.ToArray();
		roomOptions.PlayerTtl = _config.BackgroundConnectionTimeout * 2;
		roomOptions.EmptyRoomTtl = _config.BackgroundConnectionTimeout * 2;
		if (create)
		{
			roomOptions.MaxPlayers = (byte)maxPlayers;
			EnterRoomParams enterParams = new EnterRoomParams
			{
				RoomName = session.HostName,
				RoomOptions = roomOptions,
				Lobby = _lbClient.CurrentLobby
			};
			if (_lbClient.OpCreateRoom(enterParams))
			{
				UdpLog.Debug("Creating new room: {0}", session.HostName);
				startTimer?.Invoke();
			}
			else
			{
				UdpLog.Debug("Failed to create room: {0}", session.HostName);
				ChangeState(ConnectState.CreateRoomFailed);
			}
			yield break;
		}
		_lbClient.CurrentRoom.IsVisible = roomOptions.IsVisible;
		_lbClient.CurrentRoom.IsOpen = roomOptions.IsOpen;
		_lbClient.WaitForRoomPropertyUpdate = true;
		if (_lbClient.CurrentRoom.SetCustomProperties(customRoomProperties))
		{
			UdpLog.Debug("Updating properties of room: {0}", _lbClient.CurrentRoom.Name);
			yield return new WaitWhile(() => _lbClient.WaitForRoomPropertyUpdate);
			_currentAsyncCallback?.Invoke(arg1: true, UdpSessionError.Ok);
			_currentAsyncCallback = null;
		}
		else
		{
			_currentAsyncCallback?.Invoke(arg1: false, UdpSessionError.Error);
			_currentAsyncCallback = null;
		}
	}

	private IEnumerator JoinSessionPre(object token, Action<bool, UdpSessionError> result = null)
	{
		ChangeState(ConnectState.JoinRoomPending);
		_currentAsyncCallback = result;
		yield return new WaitWhile(() => _lbClient == null || !_lbClient.IsConnectedAndReady || _lbClient.State != ClientState.JoinedLobby);
		if (token != null)
		{
			_joinToken = token;
		}
	}

	private IEnumerator JoinSessionPost(Action startTimer = null)
	{
		startTimer?.Invoke();
		yield return new WaitUntil(() => _lbClient.State == ClientState.Joined);
		if (_config.UsePunchThrough)
		{
			yield return new WaitUntil(() => _platform.stunManager.IsDone);
		}
		PhotonSession currentSession = GetSession() as PhotonSession;
		_currentConnectionControl = default(ConnectionControl);
		if (currentSession.IsPunchEnabled && _config.UsePunchThrough)
		{
			SendRelayReliable(151, _lbClient.CurrentMasterId, _platform.stunManager.ExternalEndpoint.SerializeEndPoint());
			ChangeState(ConnectState.DirectPending);
			_currentConnectionControl = new ConnectionControl
			{
				ConnectionLocal = false,
				ConnectionAttempts = 0u,
				ConnectionThreshold = _config.ConnectionRequestAttempts,
				ConnectionTrials = _config.ConnectionRequestAttempts,
				LanEndPoint = currentSession.LanEndPoint,
				WanEndPoint = currentSession.WanEndPoint
			};
			if (_platform.stunManager.InternalEndPoint.IsSameNetwork(currentSession.LanEndPoint))
			{
				_currentConnectionControl.ConnectionLocal = true;
				_currentConnectionControl.ConnectionThreshold = _config.ConnectionLANRequestAttempts;
				BoltConnect(currentSession.LanEndPoint);
			}
			else
			{
				BoltConnect(currentSession.WanEndPoint);
			}
			yield return new WaitWhile(() => ConnectState == ConnectState.DirectPending);
			if (ConnectState == ConnectState.DirectSuccess || ConnectState == ConnectState.Connected)
			{
				yield break;
			}
		}
		ChangeState(ConnectState.RelayPending);
		BoltConnect(ServerRelayEndPoint);
	}

	private IEnumerator JoinSessionRandomRoutine(UdpSessionFilter sessionFilter, object token, Action<bool, UdpSessionError> result = null, Action startTimer = null)
	{
		if (Instance.ConnectState != ConnectState.Idle)
		{
			UdpLog.Debug("Client not in Idle State, ignore Join request.");
			yield break;
		}
		yield return JoinSessionPre(token, result);
		ExitGames.Client.Photon.Hashtable filterParams = new ExitGames.Client.Photon.Hashtable();
		MatchmakingMode matchmakingMode = MatchmakingMode.FillRoom;
		if (sessionFilter != null)
		{
			matchmakingMode = ConvertMatchmakingMode(sessionFilter.FillMode);
			foreach (KeyValuePair<object, object> kv in sessionFilter)
			{
				filterParams[kv.Key] = kv.Value;
			}
		}
		_lbClient.OpJoinRandomRoom(new OpJoinRandomRoomParams
		{
			MatchingType = matchmakingMode,
			ExpectedCustomRoomProperties = filterParams,
			TypedLobby = _lbClient.CurrentLobby
		});
		yield return JoinSessionPost(startTimer);
	}

	private IEnumerator JoinSessionRoutine(UdpSession session, object token, Action<bool, UdpSessionError> result = null, Action startTimer = null)
	{
		if (Instance.ConnectState != ConnectState.Idle)
		{
			UdpLog.Debug("Client not in Idle State, ignore Join request.");
			yield break;
		}
		yield return JoinSessionPre(token, result);
		UdpLog.Debug("Joining to room: {0}", session.HostName);
		_lbClient.OpJoinRoom(new EnterRoomParams
		{
			RoomName = session.HostName
		});
		yield return JoinSessionPost(startTimer);
	}

	private IEnumerator FailedJoinCallback()
	{
		UdpLog.Debug("Starting Room Join Timer with {0}s", _config.RoomJoinTimeout);
		yield return new WaitForSeconds(_config.RoomJoinTimeout);
		if (ConnectState == ConnectState.DirectPending || ConnectState == ConnectState.RelayPending || ConnectState == ConnectState.JoinRoomPending)
		{
			UdpLog.Warn("Room Join failed due to exceeded max time to join");
			ChangeState(ConnectState.JoinRoomFailed, -2);
		}
	}

	private IEnumerator FailedCreateCallback()
	{
		UdpLog.Debug("Starting Room Create Timer with {0}s", _config.RoomCreateTimeout);
		yield return new WaitForSeconds(_config.RoomCreateTimeout);
		if (ConnectState != ConnectState.Connected)
		{
			UdpLog.Warn("Room Create failed due to exceeded max time to create");
			ChangeState(ConnectState.CreateRoomFailed);
		}
	}

	internal static void ChangeState(ConnectState state, int error = 0)
	{
		if (Instance.ConnectState != state)
		{
			if (Array.IndexOf(Instance._stateTransitions[Instance.ConnectState], state) != -1)
			{
				UdpLog.Debug("Changing Connect State: {0} => {1}", Instance.ConnectState, state);
				Instance.ConnectState = state;
				OnConnectStateChange(state, error);
			}
			else
			{
				UdpLog.Warn("Invalid transition of Connect State: {0} => {1}", Instance.ConnectState, state);
			}
		}
	}

	internal static UdpConnectionDisconnectReason ConvertDisconnectReason(DisconnectCause disconnectedCause)
	{
		switch (disconnectedCause)
		{
		case DisconnectCause.InvalidAuthentication:
		case DisconnectCause.CustomAuthenticationFailed:
		case DisconnectCause.AuthenticationTicketExpired:
			return UdpConnectionDisconnectReason.Authentication;
		case DisconnectCause.MaxCcuReached:
			return UdpConnectionDisconnectReason.MaxCCUReached;
		case DisconnectCause.DisconnectByServerLogic:
		case DisconnectCause.DisconnectByClientLogic:
		case DisconnectCause.DisconnectByDisconnectMessage:
			return UdpConnectionDisconnectReason.Disconnected;
		case DisconnectCause.ExceptionOnConnect:
		case DisconnectCause.DnsExceptionOnConnect:
		case DisconnectCause.ServerAddressInvalid:
		case DisconnectCause.Exception:
		case DisconnectCause.InvalidRegion:
		case DisconnectCause.OperationNotAllowedInCurrentState:
			return UdpConnectionDisconnectReason.Error;
		case DisconnectCause.ServerTimeout:
		case DisconnectCause.ClientTimeout:
		case DisconnectCause.DisconnectByOperationLimit:
			return UdpConnectionDisconnectReason.CloudTimeout;
		default:
			return UdpConnectionDisconnectReason.Unknown;
		}
	}

	internal static UdpSessionError ConvertErrorCode(int error)
	{
		switch (error)
		{
		case 32764:
			return UdpSessionError.GameClosed;
		case 32758:
		case 32760:
			return UdpSessionError.GameDoesNotExist;
		case 32765:
			return UdpSessionError.GameFull;
		case 32766:
			return UdpSessionError.GameIdAlreadyExists;
		case 0:
			return UdpSessionError.Ok;
		default:
			return UdpSessionError.Error;
		}
	}

	private static MatchmakingMode ConvertMatchmakingMode(UdpSessionFillMode fillMode)
	{
		return fillMode switch
		{
			UdpSessionFillMode.Fill => MatchmakingMode.FillRoom, 
			UdpSessionFillMode.Random => MatchmakingMode.RandomMatching, 
			UdpSessionFillMode.Serial => MatchmakingMode.SerialMatching, 
			_ => MatchmakingMode.RandomMatching, 
		};
	}

	public void OnCreatedRoom()
	{
		ChangeState(ConnectState.Connected);
	}

	public void OnCreateRoomFailed(short returnCode, string message)
	{
	}

	public void OnFriendListUpdate(List<FriendInfo> friendList)
	{
	}

	public void OnJoinedRoom()
	{
		LastDisconnectReason = UdpConnectionDisconnectReason.Unknown;
	}

	public void OnJoinRandomFailed(short returnCode, string message)
	{
	}

	public void OnJoinRoomFailed(short returnCode, string message)
	{
	}

	public void OnLeftRoom()
	{
	}

	public void OnMasterClientSwitched(Player newMasterClient)
	{
	}

	public void OnPlayerEnteredRoom(Player newPlayer)
	{
	}

	public void OnPlayerLeftRoom(Player otherPlayer)
	{
		UdpLog.Debug("Player {0} has left the room, current master client is {1}", otherPlayer.ActorNumber, _lbClient.CurrentMasterId);
		if (otherPlayer.IsMasterClient || otherPlayer.ActorNumber == _lbClient.OriginalRoomMasterClient)
		{
			UdpLog.Debug("Master Client {0} has disconnected, disconnecting...", otherPlayer.ActorNumber);
			_lbClient.Disable(DisconnectCause.DisconnectByServerLogic);
		}
		else
		{
			UdpEndPoint endPoint = new UdpEndPoint(new UdpIPv4Address((uint)otherPlayer.ActorNumber), 0);
			_platform.udpSocket.Disconnect(endPoint);
		}
	}

	public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
	{
	}

	public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
	{
	}
}
