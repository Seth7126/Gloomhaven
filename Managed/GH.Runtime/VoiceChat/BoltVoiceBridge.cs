#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using ExitGames.Client.Photon;
using Photon.Bolt;
using Photon.Bolt.Matchmaking;
using Photon.Realtime;
using Photon.Voice.Unity;
using SM.Utils;
using UdpKit;
using UnityEngine;

namespace VoiceChat;

[RequireComponent(typeof(VoiceConnection), typeof(Recorder))]
public class BoltVoiceBridge : GlobalEventListener, IConnectionCallbacks, IMatchmakingCallbacks
{
	[SerializeField]
	private GameObject BoltSpeakerPrefab;

	public static BoltVoiceBridge Instance;

	private Dictionary<int, BoltVoiceSpeakerController> speakerRegistry;

	private VoiceConnection voiceConnection;

	private Recorder voiceRecorder;

	private byte voiceRecorderDefaultGroup;

	public string RoomName
	{
		get
		{
			string hostName = BoltMatchmaking.CurrentSession.HostName;
			return $"{hostName}_voice";
		}
	}

	public string Region => BoltMatchmaking.CurrentMetadata["Region"] as string;

	public int LocalPlayerID
	{
		get
		{
			if (IsConnected)
			{
				return voiceConnection.Client.LocalPlayer.ActorNumber;
			}
			return -1;
		}
	}

	private EnterRoomParams EnterRoomParams => new EnterRoomParams
	{
		RoomName = RoomName,
		RoomOptions = new RoomOptions
		{
			IsVisible = false,
			PublishUserId = true
		}
	};

	public bool IsConnected => voiceConnection.Client.IsConnected;

	public event Action<int, string, string, string, bool, Speaker> EventAddIncomingVoiceUser;

	public event Action<int> EventRemoveIncomingVoiceUser;

	public event Action EventJoinRoom;

	public event Action EventLeftRoom;

	public event Action EventStateUpdate;

	public event Action<string> EventTextNotification;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public override void SessionCreatedOrUpdated(UdpSession session)
	{
		LogUtils.Log("Session Created or updated with SessionId: " + session.HostName + ".");
		base.SessionCreatedOrUpdated(session);
		this.EventStateUpdate?.Invoke();
	}

	public override void SessionConnected(UdpSession session, IProtocolToken token)
	{
		LogUtils.Log("Connected to Session with SessionId: " + session.HostName + ".");
		base.SessionConnected(session, token);
		this.EventStateUpdate?.Invoke();
	}

	public override void Disconnected(BoltConnection connection)
	{
		LogUtils.Log("Disconnected from session.");
		base.Disconnected(connection);
		this.EventStateUpdate?.Invoke();
	}

	public override void Connected(BoltConnection connection)
	{
		base.Connected(connection);
		this.EventStateUpdate?.Invoke();
	}

	public override void BoltShutdownBegin(AddCallback registerDoneCallback, UdpConnectionDisconnectReason disconnectReason)
	{
		if (BoltNetwork.IsServer || BoltNetwork.IsClient)
		{
			ShutdownVoiceConnection();
			this.EventStateUpdate?.Invoke();
		}
	}

	public void SetupAndConnect()
	{
		SetUpUsername();
		voiceConnection.Client.AddCallbackTarget(this);
		ConnectNow();
	}

	public void ShutdownVoiceConnection()
	{
		voiceConnection.Client.RemoveCallbackTarget(this);
		voiceConnection.Client.Disconnect();
	}

	private void Start()
	{
		voiceConnection = GetComponent<VoiceConnection>();
		voiceConnection.SpeakerFactory = CustomBoltSpeakerFactory;
		voiceRecorder = GetComponent<Recorder>();
		voiceRecorderDefaultGroup = voiceRecorder.InterestGroup;
		speakerRegistry = new Dictionary<int, BoltVoiceSpeakerController>();
		if (!PlatformLayer.Instance.IsDelayedInit)
		{
			SetUpUsername();
		}
	}

	public void SetUpUsername()
	{
		if (!PhotonPeer.RegisterType(typeof(PhotonUserData), 228, Serialize, Deserialize))
		{
			Debug.LogError("[PhotonVoice] Error on type registration");
		}
		if (PlatformLayer.UserData.IsSignedIn)
		{
			switch (PlatformLayer.Instance.PlatformID)
			{
			case "Standalone":
			case "Steam":
			case "GoGGalaxy":
			case "EpicGamesStore":
				PlatformLayer.UserData.UserName.GetCensoredStringAsync(delegate(string censoredUsername)
				{
					voiceRecorder.UserData = new PhotonUserData(censoredUsername, PlatformLayer.UserData.PlatformNetworkAccountPlayerID, PlatformLayer.Instance.PlatformID, FFSNetwork.IsHost);
				});
				break;
			default:
				voiceRecorder.UserData = new PhotonUserData(PlatformLayer.UserData.UserName, PlatformLayer.UserData.PlatformNetworkAccountPlayerID, PlatformLayer.Instance.PlatformID, FFSNetwork.IsHost);
				break;
			}
		}
		else
		{
			voiceRecorder.UserData = new PhotonUserData("TempUser", "0", PlatformLayer.Instance.PlatformID, FFSNetwork.IsHost);
		}
		static object Deserialize(byte[] custom)
		{
			using MemoryStream serializationStream = new MemoryStream(custom);
			return new BinaryFormatter().Deserialize(serializationStream);
		}
		static byte[] Serialize(object custom)
		{
			using MemoryStream memoryStream = new MemoryStream();
			new BinaryFormatter().Serialize(memoryStream, custom);
			return memoryStream.GetBuffer();
		}
	}

	protected Speaker CustomBoltSpeakerFactory(int playerId, byte voiceId, object userData)
	{
		BoltVoiceSpeakerController value = speakerRegistry.FirstOrDefault((KeyValuePair<int, BoltVoiceSpeakerController> pair) => !pair.Value.IsBusy).Value;
		Speaker component;
		if (value != null)
		{
			component = value.gameObject.GetComponent<Speaker>();
			speakerRegistry.Remove(value.PlayerID);
			speakerRegistry.Add(playerId, value);
			value.PlayerID = playerId;
			value.IsBusy = true;
		}
		else
		{
			GameObject obj = UnityEngine.Object.Instantiate(BoltSpeakerPrefab);
			UnityEngine.Object.DontDestroyOnLoad(obj);
			component = obj.GetComponent<Speaker>();
			BoltVoiceSpeakerController component2 = obj.GetComponent<BoltVoiceSpeakerController>();
			component2.PlayerID = playerId;
			component2.IsBusy = true;
			speakerRegistry.Add(playerId, component2);
		}
		StartCoroutine(TryRegisterInboundConnection(playerId, (PhotonUserData)userData, component));
		return component;
	}

	private IEnumerator TryRegisterInboundConnection(int playerId, PhotonUserData userData, Speaker speaker)
	{
		while (speaker.RemoteVoiceLink == null)
		{
			LogUtils.LogWarning("[BoltVoiceBridge.Manager] Waiting for a speaker to be assigned a remote connection");
			yield return null;
		}
		this.EventAddIncomingVoiceUser?.Invoke(playerId, userData.Name, userData.AccountID, userData.PlatformName, userData.IsHost, speaker);
		speaker.RemoteVoiceLink.RemoteVoiceRemoved += delegate
		{
			if (speaker.TryGetComponent<BoltVoiceSpeakerController>(out var component))
			{
				component.IsBusy = false;
			}
			this.EventRemoveIncomingVoiceUser?.Invoke(playerId);
		};
	}

	public void ChangeVoiceGround(int groupID)
	{
		if (voiceRecorder != null)
		{
			voiceConnection.Client.OpChangeGroups(new byte[0], new byte[1] { (byte)groupID });
			voiceRecorder.InterestGroup = (byte)groupID;
		}
	}

	public void ResetVoiceGroup()
	{
		if (voiceRecorder != null)
		{
			voiceConnection.Client.OpChangeGroups(new byte[0], new byte[1] { voiceRecorderDefaultGroup });
			voiceRecorder.InterestGroup = voiceRecorderDefaultGroup;
		}
	}

	public void RecorderTransmissionToggle()
	{
		voiceRecorder.TransmitEnabled = !voiceRecorder.TransmitEnabled;
	}

	public bool RecorderIsTransmitting()
	{
		return voiceRecorder.TransmitEnabled;
	}

	public bool GetSpeaker(int playerID, out GameObject speaker)
	{
		if (speakerRegistry.TryGetValue(playerID, out var value))
		{
			speaker = value.gameObject;
			return true;
		}
		speaker = null;
		return false;
	}

	public void ConnectNow()
	{
		AppSettings customSettings = new AppSettings();
		voiceConnection.Settings.CopyTo(customSettings);
		customSettings.FixedRegion = Region;
		ConnectUsingSettings();
		void ConnectUsingSettings()
		{
			voiceConnection.ConnectUsingSettings(customSettings);
		}
	}

	private void UpdateVoiceChatUI(string message)
	{
		this.EventTextNotification?.Invoke(message);
		this.EventStateUpdate?.Invoke();
	}

	public void OnCreatedRoom()
	{
		string message = "[BoltVoiceBridge] Room Created " + voiceConnection.Client.CurrentRoom.Name;
		UpdateVoiceChatUI(message);
	}

	public void OnCreateRoomFailed(short returnCode, string msg)
	{
		string message = "[BoltVoiceBridge] OnCreateRoomFailed errorCode=" + returnCode + " errorMessage=" + msg;
		UpdateVoiceChatUI(message);
	}

	public void OnFriendListUpdate(List<FriendInfo> friendList)
	{
		string text = "[BoltVoiceBridge] Friend List Updated!";
		string text2 = "\n";
		foreach (FriendInfo friend in friendList)
		{
			text2 = text2 + "Friend: UserId:" + friend.UserId + ", in room:" + friend.Room + ", isInRoom:" + friend.IsInRoom + "\n";
		}
		text += text2;
		UpdateVoiceChatUI(text);
		LogUtils.LogError(text);
	}

	public void OnJoinedRoom()
	{
		_ = "[BoltVoiceBridge] Joined Room as Player " + LocalPlayerID;
		this.EventJoinRoom?.Invoke();
		ResetVoiceGroup();
	}

	public void OnJoinRandomFailed(short returnCode, string msg)
	{
		string message = $"[BoltVoiceBridge] OnJoinRandomFailed errorCode={returnCode} errorMessage={msg}";
		UpdateVoiceChatUI(message);
	}

	public void OnJoinRoomFailed(short returnCode, string msg)
	{
		string message = "[BoltVoiceBridge] OnJoinRoomFailed roomName=" + RoomName + " errorCode=" + returnCode + " errorMessage=" + msg;
		UpdateVoiceChatUI(message);
	}

	public void OnLeftRoom()
	{
		string message = "[BoltVoiceBridge] Left Room";
		this.EventLeftRoom?.Invoke();
		UpdateVoiceChatUI(message);
	}

	public void OnConnected()
	{
		string message = "[BoltVoiceBridge] Connected";
		UpdateVoiceChatUI(message);
	}

	public void OnConnectedToMaster()
	{
		voiceConnection.Client.OpJoinOrCreateRoom(EnterRoomParams);
	}

	public void OnDisconnected(DisconnectCause cause)
	{
		string message = "[BoltVoiceBridge] OnDisconnected cause=" + cause;
		UpdateVoiceChatUI(message);
		if (cause != DisconnectCause.None)
		{
			_ = 15;
		}
	}

	public void OnRegionListReceived(RegionHandler regionHandler)
	{
	}

	public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
	{
	}

	public void OnCustomAuthenticationFailed(string debugMessage)
	{
	}
}
