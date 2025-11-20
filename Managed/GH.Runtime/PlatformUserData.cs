#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.Sessions;
using Epic.OnlineServices.UserInfo;
using FFSNet;
using Photon.Bolt;
using Platforms;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

public class PlatformUserData : MonoBehaviour, IPlatformUserData
{
	public class FriendData
	{
		public EpicAccountId LocalUserId;

		public EpicAccountId UserId;

		public ProductUserId UserProductUserId;

		public string Name;
	}

	public enum EPlatformAuthStatus
	{
		None,
		Processing,
		Authorised,
		NotAuthorised
	}

	private LoginCredentialType m_CurrentLoginType;

	private UserInfoInterface m_EOSUserInfoInterface;

	private Dictionary<EpicAccountId, FriendData> m_CachedFriends;

	[SerializeField]
	private Sprite _defaultUserAvatarSprite;

	protected const string c_EditorUserName = "Editor User";

	public EPlatformAuthStatus CurrentAuthStatus;

	public static bool IsInitialised;

	private List<Tuple<Image, SaveOwner>> m_ReceivedSaveOwnerAvatarUpdatesToProcess;

	private List<Tuple<Image, NetworkPlayer>> m_ReceivedNetworkPlayerAvatarUpdatesToProcess;

	private IEnumerator m_AvatarProcessingRoutine;

	private string m_PlatformPlayerID;

	private string m_PlatformAccountID;

	private string m_Username;

	public EPlatformAuthStatus CurrentEOSAuthStatus { get; set; }

	public string PlatformPlayerID
	{
		get
		{
			if (string.IsNullOrEmpty(m_PlatformPlayerID))
			{
				SteamId steamId = (SteamClient.IsValid ? SteamClient.SteamId : ((SteamId)0uL));
				m_PlatformPlayerID = steamId.Value.ToString();
				m_PlatformAccountID = steamId.AccountId.ToString();
			}
			return m_PlatformPlayerID;
		}
		private set
		{
			m_PlatformPlayerID = value;
		}
	}

	public string PlatformAccountID
	{
		get
		{
			if (string.IsNullOrEmpty(m_PlatformAccountID))
			{
				SteamId steamId = (SteamClient.IsValid ? SteamClient.SteamId : ((SteamId)0uL));
				m_PlatformPlayerID = steamId.Value.ToString();
				m_PlatformAccountID = steamId.AccountId.ToString();
			}
			return m_PlatformAccountID;
		}
		private set
		{
			m_PlatformAccountID = value;
		}
	}

	public string PlatformNetworkAccountPlayerID => PlatformAccountID;

	public string UserName
	{
		get
		{
			if (m_Username.IsNullOrEmpty())
			{
				m_Username = (SteamClient.IsValid ? SteamClient.Name : "Editor User");
			}
			return m_Username;
		}
		private set
		{
			m_Username = value;
		}
	}

	public bool IsSignedIn => SteamClient.IsValid;

	public void EOSInitialise()
	{
		m_CachedFriends = new Dictionary<EpicAccountId, FriendData>();
		GetMainUserDetails();
		IsInitialised = true;
	}

	public void StartLoginFlow()
	{
		CurrentEOSAuthStatus = EPlatformAuthStatus.Processing;
		bool flag = false;
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		string text = string.Empty;
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i] == "-AUTH_PASSWORD")
			{
				text = commandLineArgs[i + 1];
				if (flag)
				{
					Debug.LogFormat("[PLATFORM LAYER] EGS Exchange token: {0}", text);
				}
				break;
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			Debug.LogWarning("[PLATFORM LAYER] - Failed to retreive EGS Exchange token from launch parameters, beginning persistent login flow.");
			m_CurrentLoginType = LoginCredentialType.PersistentAuth;
			Epic.OnlineServices.Auth.Credentials value = new Epic.OnlineServices.Auth.Credentials
			{
				Type = m_CurrentLoginType
			};
			Epic.OnlineServices.Auth.LoginOptions loginOptions = new Epic.OnlineServices.Auth.LoginOptions
			{
				Credentials = value,
				ScopeFlags = (AuthScopeFlags.BasicProfile | AuthScopeFlags.FriendsList | AuthScopeFlags.Presence)
			};
			EOSManager.Instance.StartLoginWithLoginOptions(loginOptions, OnAuthLoginComplete);
		}
		else
		{
			if (flag)
			{
				Debug.Log("[PLATFORM LAYER] - Begin Exchange token login attempt");
			}
			m_CurrentLoginType = LoginCredentialType.ExchangeCode;
			EOSManager.Instance.StartLoginWithLoginTypeAndToken(m_CurrentLoginType, null, null, OnAuthLoginComplete);
		}
	}

	private void OnAuthLoginComplete(Epic.OnlineServices.Auth.LoginCallbackInfo loginCallbackInfo)
	{
		bool debug = false;
		EOSFriendsManager FriendsManager = EOSManager.Instance.GetOrCreateManager<EOSFriendsManager>();
		if (loginCallbackInfo.ResultCode == Epic.OnlineServices.Result.Success)
		{
			EOSManager.Instance.StartConnectLoginWithEpicAccount(loginCallbackInfo.LocalUserId, delegate(Epic.OnlineServices.Connect.LoginCallbackInfo connectLoginCallbackInfo)
			{
				if (connectLoginCallbackInfo.ResultCode == Epic.OnlineServices.Result.Success)
				{
					CurrentEOSAuthStatus = EPlatformAuthStatus.Authorised;
					if (debug)
					{
						Debug.LogFormat("[PLATFORM LAYER] - Successful login using: {0} | Logged in UserID: {1} | Logged in ProductUserID: {2}", m_CurrentLoginType.ToString(), EOSManager.Instance.GetLocalUserId(), EOSManager.Instance.GetProductUserId());
					}
					SetupEpicSession(FriendsManager);
				}
				else if (connectLoginCallbackInfo.ResultCode == Epic.OnlineServices.Result.InvalidUser)
				{
					EOSManager.Instance.CreateConnectUserWithContinuanceToken(connectLoginCallbackInfo.ContinuanceToken, delegate
					{
						EOSManager.Instance.StartConnectLoginWithEpicAccount(loginCallbackInfo.LocalUserId, delegate(Epic.OnlineServices.Connect.LoginCallbackInfo retryConnectLoginCallbackInfo)
						{
							if (retryConnectLoginCallbackInfo.ResultCode == Epic.OnlineServices.Result.Success)
							{
								CurrentEOSAuthStatus = EPlatformAuthStatus.Authorised;
								if (debug)
								{
									Debug.LogFormat("[PLATFORM LAYER] - Successful login using: {0} | Logged in UserID: {1} | Logged in ProductUserID: {2}", m_CurrentLoginType.ToString(), EOSManager.Instance.GetLocalUserId(), EOSManager.Instance.GetProductUserId());
								}
								SetupEpicSession(FriendsManager);
							}
							else
							{
								Debug.LogError("[PLATFORM LAYER] - Failed to achieve platform auth after receiving InvalidUser from successful login and attempting to create a connect user");
								CurrentEOSAuthStatus = EPlatformAuthStatus.NotAuthorised;
							}
						});
					});
				}
				else
				{
					Debug.LogError("[PLATFORM LAYER] - Failed to achieve platform login after receiving uncaught result type from attempted connection");
					CurrentEOSAuthStatus = EPlatformAuthStatus.NotAuthorised;
				}
			});
			return;
		}
		switch (m_CurrentLoginType)
		{
		case LoginCredentialType.ExchangeCode:
			if (debug)
			{
				Debug.LogWarning("[PLATFORM LAYER] - Failed to login using exchange token, beginning persistent auth login flow.");
			}
			CurrentEOSAuthStatus = EPlatformAuthStatus.Processing;
			m_CurrentLoginType = LoginCredentialType.PersistentAuth;
			EOSManager.Instance.StartPersistentLogin(OnAuthLoginComplete);
			break;
		case LoginCredentialType.PersistentAuth:
			if (debug)
			{
				Debug.LogWarning("[PLATFORM LAYER] - Failed to login using persistent auth, beginning account portal login flow.");
			}
			CurrentEOSAuthStatus = EPlatformAuthStatus.Processing;
			m_CurrentLoginType = LoginCredentialType.AccountPortal;
			EOSManager.Instance.StartLoginWithLoginTypeAndToken(m_CurrentLoginType, null, null, OnAuthLoginComplete);
			break;
		case LoginCredentialType.AccountPortal:
			if (debug)
			{
				Debug.LogError("[PLATFORM LAYER] - Failed to achieve platform login.");
			}
			CurrentEOSAuthStatus = EPlatformAuthStatus.NotAuthorised;
			break;
		}
	}

	private void GetMainUserDetails()
	{
		m_EOSUserInfoInterface = EOSManager.Instance.GetEOSPlatformInterface().GetUserInfoInterface();
		QueryUserInfo(EOSManager.Instance.GetLocalUserId());
	}

	private void SetupEpicSession(EOSFriendsManager FriendsManager)
	{
		SaveData.Instance.Global.EpicLogin = true;
		FriendsManager.QueryFriends(null);
		FriendsManager.SubscribeToFriendUpdates(EOSManager.Instance.GetLocalUserId());
		EOSSessionsManager orCreateManager = EOSManager.Instance.GetOrCreateManager<EOSSessionsManager>();
		orCreateManager.SubscribteToGameInvites(PlatformLayer.Networking.EpicGameLobbyJoinRequestedCallback);
		Session session = new Session
		{
			Name = "Gloomhaven",
			MaxPlayers = 4u,
			PermissionLevel = OnlineSessionPermissionLevel.InviteOnly
		};
		orCreateManager.CreateSession(session);
	}

	private void QueryUserInfo(EpicAccountId targetUserId)
	{
		if (!targetUserId.IsValid())
		{
			Debug.LogError("Friends (QueryUserInfo): targetUserId parameter is invalid!");
			return;
		}
		QueryUserInfoOptions options = new QueryUserInfoOptions
		{
			LocalUserId = EOSManager.Instance.GetLocalUserId(),
			TargetUserId = targetUserId
		};
		m_EOSUserInfoInterface.QueryUserInfo(ref options, null, OnQueryUserInfoCompleted);
	}

	private void OnQueryUserInfoCompleted(ref QueryUserInfoCallbackInfo data)
	{
		if (data.ResultCode != Epic.OnlineServices.Result.Success)
		{
			Debug.LogWarningFormat("[PLATFORM LAYER] - Failed to retrieve User Info - Result: {0}", data.ResultCode);
			return;
		}
		Debug.Log("[PLATFORM LAYER] - Successfully retreived user info, attempting to resolve user info");
		CopyUserInfoOptions options = new CopyUserInfoOptions
		{
			LocalUserId = data.LocalUserId,
			TargetUserId = data.TargetUserId
		};
		UserInfoData? outUserInfo;
		Epic.OnlineServices.Result result = m_EOSUserInfoInterface.CopyUserInfo(ref options, out outUserInfo);
		if (result != Epic.OnlineServices.Result.Success)
		{
			Debug.LogErrorFormat("[PLATFORM LAYER] - OnQueryUserInfoCompleted CopyUserInfo error: {0}", result);
			return;
		}
		FriendData friendData = new FriendData
		{
			LocalUserId = data.LocalUserId,
			UserId = data.TargetUserId,
			Name = outUserInfo?.DisplayName
		};
		if (m_CachedFriends.TryGetValue(friendData.UserId, out var value))
		{
			Debug.LogFormat("[PLATFORM LAYER] - OnQueryUserInfoCompleted FriendData Updated (LocalUserId: {0}, Name:{1})", friendData.LocalUserId, friendData.Name);
			value.LocalUserId = friendData.LocalUserId;
			value.Name = friendData.Name;
		}
		else
		{
			m_CachedFriends.Add(friendData.UserId, friendData);
			Debug.LogFormat("[PLATFORM LAYER] - OnQueryUserInfoCompleted Added FriendData (LocalUserId: {0}, Name:{1})", friendData.LocalUserId, friendData.Name);
		}
	}

	private void Start()
	{
		StartCoroutine(StartRoutine());
	}

	private IEnumerator StartRoutine()
	{
		while (!PlatformLayer.Initialised)
		{
			yield return null;
		}
		Initialise(PlatformLayer.Platform);
	}

	public bool IsUserDataInitialised()
	{
		return IsInitialised;
	}

	private void SetDefaultNetworkPlayerAvatar(NetworkPlayer networkPlayer)
	{
		networkPlayer.Avatar = _defaultUserAvatarSprite;
		networkPlayer.AvatarUpdated = true;
	}

	public void Initialise(IPlatform platform)
	{
		CurrentAuthStatus = EPlatformAuthStatus.Authorised;
		m_ReceivedSaveOwnerAvatarUpdatesToProcess = new List<Tuple<Image, SaveOwner>>();
		m_ReceivedNetworkPlayerAvatarUpdatesToProcess = new List<Tuple<Image, NetworkPlayer>>();
		m_AvatarProcessingRoutine = ProcessAvatarUpdates();
		StartCoroutine(m_AvatarProcessingRoutine);
		IsInitialised = true;
	}

	public void GetAvatarForSaveOwner(SaveOwner saveOwner)
	{
		if (SteamClient.IsValid && ulong.TryParse(saveOwner.PlatformPlayerID, out var result))
		{
			SteamFriends.GetLargeAvatarAsync(result).ContinueWith(GetSteamAvatarCallbackForSaveOwner, saveOwner);
		}
	}

	public void GetAvatarForNetworkPlayer(NetworkPlayer networkPlayer, string platformPlayerID)
	{
		if (SteamClient.IsValid)
		{
			ulong result;
			if (!(networkPlayer.PlatformName == "Steam"))
			{
				SetDefaultNetworkPlayerAvatar(networkPlayer);
			}
			else if (ulong.TryParse(platformPlayerID, out result))
			{
				SteamFriends.GetLargeAvatarAsync(result).ContinueWith(GetSteamAvatarCallbackForNetworkPlayer, networkPlayer);
			}
			else
			{
				SetDefaultNetworkPlayerAvatar(networkPlayer);
			}
		}
	}

	public string GetUserNameForConnection(BoltConnection connection)
	{
		if (!SteamClient.IsValid)
		{
			if (connection != null)
			{
				return ((UserToken)connection.ConnectToken).Username;
			}
			return "TempHost";
		}
		if (connection != null)
		{
			return ((UserToken)connection.ConnectToken).Username;
		}
		return SteamClient.Name;
	}

	public string GetPlatformIDForConnection(BoltConnection connection)
	{
		if (connection != null)
		{
			return ((UserToken)connection.ConnectToken).PlatformPlayerID;
		}
		if (!SteamClient.IsValid)
		{
			return "0";
		}
		return SteamClient.SteamId.Value.ToString();
	}

	public string GetDefaultPlatformID()
	{
		return default(SteamId).ToString();
	}

	public bool CanLogOutEpicStore()
	{
		return true;
	}

	private void GetSteamAvatarCallbackForSaveOwner(Task<Image?> task, object saveOwnerObject)
	{
		SaveOwner saveOwner = (SaveOwner)saveOwnerObject;
		if (saveOwner != null && task.HasValue() && task.Result.HasValue)
		{
			lock (m_ReceivedSaveOwnerAvatarUpdatesToProcess)
			{
				m_ReceivedSaveOwnerAvatarUpdatesToProcess.Add(new Tuple<Image, SaveOwner>(task.Result.Value, saveOwner));
			}
		}
	}

	private void GetSteamAvatarCallbackForNetworkPlayer(Task<Image?> task, object networkPlayerObject)
	{
		NetworkPlayer networkPlayer = (NetworkPlayer)networkPlayerObject;
		if (!(networkPlayer != null))
		{
			return;
		}
		if (task.HasValue() && task.Result.HasValue)
		{
			lock (m_ReceivedNetworkPlayerAvatarUpdatesToProcess)
			{
				m_ReceivedNetworkPlayerAvatarUpdatesToProcess.Add(new Tuple<Image, NetworkPlayer>(task.Result.Value, networkPlayer));
				return;
			}
		}
		SetDefaultNetworkPlayerAvatar(networkPlayer);
	}

	private Texture2D CreateTexture2DFromSteamImage(Image img)
	{
		Texture2D texture2D = new Texture2D((int)img.Width, (int)img.Height);
		for (int i = 0; i < img.Width; i++)
		{
			for (int j = 0; j < img.Height; j++)
			{
				Steamworks.Data.Color pixel = img.GetPixel(i, j);
				texture2D.SetPixel(i, (int)img.Height - j, new Color32(pixel.r, pixel.g, pixel.b, pixel.a));
			}
		}
		texture2D.Apply();
		return texture2D;
	}

	public Sprite CreateSpriteFromSteamImage(Image img)
	{
		Texture2D texture2D = CreateTexture2DFromSteamImage(img);
		return Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
	}

	private IEnumerator ProcessAvatarUpdates()
	{
		while (PlatformLayer.Initialised)
		{
			bool flag = m_ReceivedSaveOwnerAvatarUpdatesToProcess.Count > 0;
			for (int i = 0; i < m_ReceivedSaveOwnerAvatarUpdatesToProcess.Count; i++)
			{
				Texture2D avatar = CreateTexture2DFromSteamImage(m_ReceivedSaveOwnerAvatarUpdatesToProcess[i].Item1);
				m_ReceivedSaveOwnerAvatarUpdatesToProcess[i].Item2.SetAvatar(avatar);
			}
			bool flag2 = m_ReceivedNetworkPlayerAvatarUpdatesToProcess.Count > 0;
			for (int j = 0; j < m_ReceivedNetworkPlayerAvatarUpdatesToProcess.Count; j++)
			{
				Sprite sprite = CreateSpriteFromSteamImage(m_ReceivedNetworkPlayerAvatarUpdatesToProcess[j].Item1);
				m_ReceivedNetworkPlayerAvatarUpdatesToProcess[j].Item2.Avatar = sprite ?? _defaultUserAvatarSprite;
				m_ReceivedNetworkPlayerAvatarUpdatesToProcess[j].Item2.AvatarUpdated = true;
			}
			if (flag)
			{
				lock (m_ReceivedSaveOwnerAvatarUpdatesToProcess)
				{
					m_ReceivedSaveOwnerAvatarUpdatesToProcess.Clear();
				}
			}
			if (flag2)
			{
				lock (m_ReceivedNetworkPlayerAvatarUpdatesToProcess)
				{
					m_ReceivedNetworkPlayerAvatarUpdatesToProcess.Clear();
				}
			}
			yield return null;
		}
	}
}
