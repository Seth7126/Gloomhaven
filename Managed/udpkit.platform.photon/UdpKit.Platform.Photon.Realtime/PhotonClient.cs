#define DEBUG
using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;

namespace UdpKit.Platform.Photon.Realtime;

internal class PhotonClient : LoadBalancingClient, IConnectionCallbacks, ILobbyCallbacks, IInRoomCallbacks, IMatchmakingCallbacks
{
	internal int OriginalRoomMasterClient = -1;

	internal bool WaitForRoomPropertyUpdate = false;

	private readonly Dictionary<int, RoomInfo> _roomInfoMap = new Dictionary<int, RoomInfo>();

	private readonly List<RoomInfo> _roomList = new List<RoomInfo>();

	private readonly object _roomInfoLock = new object();

	private readonly string _originalNameServerHost;

	private Action<bool, UdpConnectionDisconnectReason> _connectToRegionCallback;

	private const string SERVER_HOST_CN = "ns.photonengine.cn";

	private bool? retryJoinLobby = false;

	private string _regionCacheSummary;

	private byte fallbackThreadId = byte.MaxValue;

	private bool didSendAcks;

	private int deltaSinceStartedToAck;

	private int startedAckingTimestamp;

	public int KeepAliveInBackground = 60000;

	public int CurrentMasterId => (base.CurrentRoom != null) ? base.CurrentRoom.MasterClientId : (-1);

	public bool Enabled { get; private set; }

	public Dictionary<string, object> CustomAuthData { get; private set; }

	public bool RoomInfoListHasChanged { get; private set; }

	internal TypedLobbyInfo lastLobbyStatistics { get; private set; }

	public List<RoomInfo> RoomInfoList
	{
		get
		{
			lock (_roomInfoLock)
			{
				if (RoomInfoListHasChanged)
				{
					_roomList.Clear();
					foreach (KeyValuePair<int, RoomInfo> item in _roomInfoMap)
					{
						_roomList.Add(item.Value);
					}
					RoomInfoListHasChanged = false;
				}
				return _roomList;
			}
		}
		private set
		{
			lock (_roomInfoLock)
			{
				RoomInfoListHasChanged = value.Count != 0;
				foreach (RoomInfo item in value)
				{
					if (item.RemovedFromList && _roomInfoMap.ContainsKey(item.GetHashCode()))
					{
						_roomInfoMap.Remove(item.GetHashCode());
					}
					else
					{
						_roomInfoMap[item.GetHashCode()] = item;
					}
				}
			}
		}
	}

	public bool FallbackThreadRunning => fallbackThreadId < byte.MaxValue;

	internal int Tick => Environment.TickCount & 0x7FFFFFFF;

	public int LocalPlayerID()
	{
		return base.LocalPlayer.ActorNumber;
	}

	public bool IsJoined()
	{
		return base.InRoom;
	}

	public bool LocalPlayerIsMasterClient()
	{
		return base.LocalPlayer.IsMasterClient;
	}

	public PhotonClient(PhotonPlatformConfig config)
		: base(config.ConnectionProtocol)
	{
		base.AppId = config.AppId;
		_originalNameServerHost = NameServerHost;
		base.LoadBalancingPeer.SerializationProtocolType = config.SerializationProtocol;
		EnableLobbyStatistics = true;
		OriginalRoomMasterClient = -1;
		if (config.AuthenticationValues != null)
		{
			base.AuthValues = config.AuthenticationValues;
		}
		base.EnableProtocolFallback = true;
		CustomAuthData = null;
		KeepAliveInBackground = config.BackgroundConnectionTimeout;
		base.LoadBalancingPeer.DisconnectTimeout = config.BackgroundConnectionTimeout;
		base.LoadBalancingPeer.SentCountAllowance = base.LoadBalancingPeer.SentCountAllowance * 2;
		if (config.CurrentPlatform.HasValue)
		{
			RuntimePlatform? currentPlatform = config.CurrentPlatform;
			RuntimePlatform? runtimePlatform = currentPlatform;
			if (runtimePlatform.HasValue)
			{
				RuntimePlatform valueOrDefault = runtimePlatform.GetValueOrDefault();
				if (valueOrDefault == RuntimePlatform.Switch)
				{
					UdpLog.Info("Ignore background acknowledge Thread on {0}", config.CurrentPlatform.ToString());
					goto IL_0169;
				}
			}
			StartFallbackSendAckThread();
		}
		else
		{
			StartFallbackSendAckThread();
		}
		goto IL_0169;
		IL_0169:
		AddCallbackTarget(this);
	}

	public bool Connect(PhotonRegion region, Action<bool, UdpConnectionDisconnectReason> result)
	{
		_connectToRegionCallback = result;
		switch (region.Region)
		{
		case PhotonRegion.Regions.BEST_REGION:
			return ConnectToNameServer();
		case PhotonRegion.Regions.CN:
			NameServerHost = "ns.photonengine.cn";
			break;
		default:
			NameServerHost = _originalNameServerHost;
			break;
		}
		UdpLog.Debug("Connecting to Region Master {0}", region.Code);
		return ConnectToRegionMaster(region.Code);
	}

	public void ClearRoomInfo()
	{
		lock (_roomInfoLock)
		{
			RoomInfoListHasChanged = false;
			_roomList.Clear();
			_roomInfoMap.Clear();
		}
	}

	public void JoinLobby()
	{
		ClearRoomInfo();
		retryJoinLobby = true;
		if (base.InRoom)
		{
			if (OpLeaveRoom(becomeInactive: false))
			{
				retryJoinLobby = null;
			}
		}
		else if (base.State == ClientState.JoinedLobby)
		{
			retryJoinLobby = false;
		}
		else if (base.State == ClientState.ConnectedToMasterServer)
		{
			retryJoinLobby = !OpJoinLobby(TypedLobby.Default);
		}
	}

	public void Disable(DisconnectCause cause = DisconnectCause.DisconnectByClientLogic)
	{
		StopFallbackSendAckThread();
		PeerStateValue peerState = base.LoadBalancingPeer.PeerState;
		PeerStateValue peerStateValue = peerState;
		if (peerStateValue != PeerStateValue.Disconnected && peerStateValue != PeerStateValue.Disconnecting)
		{
			Disconnect(cause);
		}
		RemoveCallbackTarget(this);
	}

	public void Update()
	{
		if (retryJoinLobby == true)
		{
			JoinLobby();
		}
		Service();
	}

	public override void DebugReturn(DebugLevel level, string message)
	{
		try
		{
			switch (level)
			{
			case DebugLevel.ERROR:
				UdpLog.Error(message);
				break;
			case DebugLevel.WARNING:
				UdpLog.Warn(message);
				break;
			case DebugLevel.INFO:
				UdpLog.Info(message);
				break;
			case DebugLevel.ALL:
				UdpLog.Debug(message);
				break;
			case (DebugLevel)4:
				break;
			}
		}
		catch (Exception)
		{
			switch (level)
			{
			case DebugLevel.ERROR:
				Debug.LogError(message);
				break;
			case DebugLevel.WARNING:
				Debug.LogWarning(message);
				break;
			case DebugLevel.INFO:
			case DebugLevel.ALL:
				Debug.Log(message);
				break;
			case (DebugLevel)4:
				break;
			}
		}
	}

	private void RaiseDoneCallback(bool result, UdpConnectionDisconnectReason disconnectReason)
	{
		if (_connectToRegionCallback != null)
		{
			if (!result)
			{
				UdpLog.Error("[{0}] Failed to connect to Region Master", "PhotonClient");
			}
			_connectToRegionCallback(result, disconnectReason);
			_connectToRegionCallback = null;
		}
	}

	public void OnConnected()
	{
	}

	public void OnConnectedToMaster()
	{
		OriginalRoomMasterClient = -1;
		JoinLobby();
	}

	public void OnCustomAuthenticationFailed(string debugMessage)
	{
		UdpLog.Error("Custom Authentication Failed: {0}", debugMessage);
		RaiseDoneCallback(result: false, UdpConnectionDisconnectReason.Authentication);
	}

	public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
	{
		CustomAuthData = data;
		UdpLog.Debug("Custom Auth Data: {0}", CustomAuthData.Keys.Join(", "));
	}

	public void OnDisconnected(DisconnectCause cause)
	{
		CustomAuthData = null;
		RaiseDoneCallback(result: false, PhotonPoller.ConvertDisconnectReason(cause));
	}

	public void OnRegionListReceived(RegionHandler regionHandler)
	{
		UdpLog.Debug("Region List Received");
		regionHandler.PingMinimumOfRegions(OnRegionPingCompleted, _regionCacheSummary ?? null);
	}

	private void OnRegionPingCompleted(RegionHandler regionHandler)
	{
		if (!base.IsConnectedAndReady)
		{
			return;
		}
		UdpLog.Debug("Available Regions");
		foreach (Region enabledRegion in regionHandler.EnabledRegions)
		{
			UdpLog.Debug(enabledRegion.ToString());
		}
		UdpLog.Debug("Connecting to Region Master {0}", regionHandler.BestRegion.Code);
		if (regionHandler.BestRegion.Code.Equals(PhotonRegion.GetRegion(PhotonRegion.Regions.CN).Code))
		{
			NameServerHost = "ns.photonengine.cn";
		}
		else
		{
			NameServerHost = _originalNameServerHost;
		}
		_regionCacheSummary = regionHandler.SummaryToCache;
		if (!ConnectToRegionMaster(regionHandler.BestRegion.Code))
		{
			RaiseDoneCallback(result: false, UdpConnectionDisconnectReason.Error);
		}
	}

	public void OnJoinedLobby()
	{
		RaiseDoneCallback(result: true, UdpConnectionDisconnectReason.Unknown);
		UdpLog.Info("Joined to Lobby {0}", base.CurrentLobby.Name);
		retryJoinLobby = false;
	}

	public void OnLeftLobby()
	{
	}

	public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
	{
		foreach (TypedLobbyInfo lobbyStatistic in lobbyStatistics)
		{
			if (lobbyStatistic.Type == TypedLobby.Default.Type)
			{
				lastLobbyStatistics = lobbyStatistic;
				break;
			}
		}
	}

	public void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		RoomInfoList = roomList;
	}

	public void OnMasterClientSwitched(Player newMasterClient)
	{
		UdpLog.Debug("Player {0} is now the new Master Client", newMasterClient.ActorNumber);
	}

	public void OnPlayerEnteredRoom(Player newPlayer)
	{
		UdpLog.Debug("Player {0} entered the room", newPlayer.ActorNumber);
	}

	public void OnPlayerLeftRoom(Player otherPlayer)
	{
	}

	public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
	}

	public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
	{
		WaitForRoomPropertyUpdate = false;
	}

	public void OnCreatedRoom()
	{
		UdpLog.Debug("Create Room: {0}", base.CurrentRoom.Name);
	}

	public void OnCreateRoomFailed(short returnCode, string message)
	{
	}

	public void OnJoinedRoom()
	{
		UdpLog.Debug("Joined to room: {0}", base.CurrentRoom.Name);
		UdpLog.Debug("Caching Original Master Client: {0}", CurrentMasterId);
		OriginalRoomMasterClient = CurrentMasterId;
	}

	public void OnJoinRandomFailed(short returnCode, string message)
	{
	}

	public void OnJoinRoomFailed(short returnCode, string message)
	{
		UdpLog.Debug("Join Room Failed: {0}: {1}", returnCode, message);
	}

	public void OnLeftRoom()
	{
		UdpLog.Debug("Left Room");
		if (!retryJoinLobby.HasValue)
		{
			retryJoinLobby = true;
		}
	}

	public void OnFriendListUpdate(List<FriendInfo> friendList)
	{
	}

	public void StartFallbackSendAckThread()
	{
		if (!FallbackThreadRunning)
		{
			fallbackThreadId = SupportClass.StartBackgroundCalls(RealtimeFallbackThread, 50);
			UdpLog.Debug("Starting Realtime background handler ({0})", fallbackThreadId);
		}
	}

	public void StopFallbackSendAckThread()
	{
		if (FallbackThreadRunning)
		{
			UdpLog.Debug("Stopping Realtime background handler ({0})", fallbackThreadId);
			if (SupportClass.StopBackgroundCalls(fallbackThreadId))
			{
				UdpLog.Debug("Realtime background handler stopped.");
			}
			fallbackThreadId = byte.MaxValue;
		}
	}

	public bool RealtimeFallbackThread()
	{
		if (!base.IsConnected)
		{
			didSendAcks = false;
			return true;
		}
		if (base.LoadBalancingPeer.ConnectionTime - base.LoadBalancingPeer.LastSendOutgoingTime > 100)
		{
			if (didSendAcks)
			{
				deltaSinceStartedToAck = Tick - startedAckingTimestamp;
				if (deltaSinceStartedToAck > KeepAliveInBackground)
				{
					return false;
				}
			}
			else
			{
				startedAckingTimestamp = Tick;
			}
			didSendAcks = true;
			base.LoadBalancingPeer.SendAcksOnly();
		}
		else
		{
			didSendAcks = false;
		}
		return true;
	}
}
