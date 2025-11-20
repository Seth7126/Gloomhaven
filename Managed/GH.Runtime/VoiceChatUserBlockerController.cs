#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Script.Networking.Tokens;
using Photon.Bolt;
using Platforms.Social;
using SM.Utils;
using VoiceChat;

public class VoiceChatUserBlockerController
{
	private readonly IDictionary<string, HashSet<string>> _usersBlockData = new Dictionary<string, HashSet<string>>();

	private readonly HashSet<string> _mutedByPlayer = new HashSet<string>();

	private readonly Dictionary<IUserVoice, PermissionOperationResult> _gameCorePermissions = new Dictionary<IUserVoice, PermissionOperationResult>();

	private BoltVoiceChatService _boltVoiceChatService;

	private bool UsingPlaystationNetwork
	{
		get
		{
			string platformID = PlatformLayer.Instance.PlatformID;
			if (platformID == "PlayStation4" || platformID == "PlayStation5")
			{
				return true;
			}
			return false;
		}
	}

	public event Action EventBlockedUsersStateChanged;

	public void Init(BoltVoiceChatService boltVoiceChatService)
	{
		LogUtils.Log("VoiceChatUserBlockerController Initialized.");
		_boltVoiceChatService = boltVoiceChatService;
		if (UsingPlaystationNetwork)
		{
			_boltVoiceChatService.EventNewUserConnected += OnNewUserConnected;
			_boltVoiceChatService.EventUserDisconnected += OnUserDisconnected;
			PlatformLayer.Platform.PlatformSocial.OnBlockedUsersListUpdated += OnBlockedUsersSocialPlatformUpdated;
			OnBlockedUsersSocialPlatformUpdatedInternal(PlatformLayer.Platform.PlatformSocial.BlockedUsers, responseRequired: true);
		}
	}

	public void DeInit()
	{
		if (UsingPlaystationNetwork)
		{
			if (PlatformLayer.Platform != null)
			{
				PlatformLayer.Platform.PlatformSocial.OnBlockedUsersListUpdated -= OnBlockedUsersSocialPlatformUpdated;
			}
			_boltVoiceChatService.EventNewUserConnected -= OnNewUserConnected;
			_boltVoiceChatService.EventUserDisconnected -= OnUserDisconnected;
		}
	}

	public bool IsUserVoiceUnBlocked(IUserVoice voice, out PermissionOperationResult result, out bool mutedByPlayer)
	{
		mutedByPlayer = false;
		if (_gameCorePermissions.TryGetValue(voice, out var value) && value != PermissionOperationResult.Success)
		{
			result = value;
			return false;
		}
		if (IsBlockedBySelf(voice.PlatformAccountID))
		{
			result = PermissionOperationResult.UserInBlockList;
			return false;
		}
		if (IsBlockedByThatUser(voice.PlatformAccountID))
		{
			result = PermissionOperationResult.UserInBlockList;
			return false;
		}
		if (_mutedByPlayer.Contains(voice.PlatformAccountID))
		{
			result = PermissionOperationResult.Unknown;
			mutedByPlayer = true;
			return false;
		}
		result = PermissionOperationResult.Success;
		return true;
	}

	public void MuteByPlayer(string accountId)
	{
		_mutedByPlayer.Add(accountId);
		UpdateMuteStates();
	}

	public void UnMuteByPlayer(string accountId)
	{
		_mutedByPlayer.Remove(accountId);
		UpdateMuteStates();
	}

	public void OnOtherPlayerBlockedUsersReceived(string userId, List<string> blockedList, bool responseRequired)
	{
		if (UsingPlaystationNetwork)
		{
			UpdateLocalBlockedInformationAboutUser(userId, blockedList);
			if (responseRequired)
			{
				SendBlockedUsersToOtherPlayers(responseRequired: false);
			}
		}
	}

	public void ValidateGameCorePermissions(Action<OperationResult> operationResult, bool isCrossNetworkUserInSession, HashSet<string> usersIds)
	{
		PlatformLayer.Networking.GetPermissionsTowardsPlatformUsersAsync(usersIds, delegate(OperationResult result, Dictionary<string, Dictionary<Permission, List<PermissionOperationResult>>> dictionary)
		{
			_gameCorePermissions.Clear();
			foreach (ConnectedUserVoice playerVoice in _boltVoiceChatService.PlayerVoices)
			{
				if (result == OperationResult.Success)
				{
					if (playerVoice.PlatformName != PlatformLayer.Instance.PlatformID && dictionary.ContainsKey(CrossNetworkUser.CrossNetworkUser.ToString()))
					{
						PermissionOperationResult value = dictionary[CrossNetworkUser.CrossNetworkUser.ToString()][Permission.VoiceCommunications][0];
						_gameCorePermissions[playerVoice] = value;
					}
					else if (playerVoice.PlatformName == PlatformLayer.Instance.PlatformID && dictionary.ContainsKey(playerVoice.PlatformAccountID))
					{
						PermissionOperationResult value2 = dictionary[playerVoice.PlatformAccountID][Permission.VoiceCommunications][0];
						_gameCorePermissions[playerVoice] = value2;
					}
				}
				else
				{
					_gameCorePermissions[playerVoice] = PermissionOperationResult.Unknown;
				}
			}
			UpdateMuteStates();
			OnBlockedUsersSocialPlatformUpdated((from x in _gameCorePermissions
				where x.Value != PermissionOperationResult.Success
				select new User
				{
					UserId = x.Key.PlatformAccountID
				}).ToList());
			operationResult?.Invoke(result);
		});
	}

	private void OnBlockedUsersSocialPlatformUpdated(List<User> users)
	{
		OnBlockedUsersSocialPlatformUpdatedInternal(users, responseRequired: false);
	}

	private void OnBlockedUsersSocialPlatformUpdatedInternal(List<User> users, bool responseRequired)
	{
		string platformNetworkAccountPlayerID = PlatformLayer.UserData.PlatformNetworkAccountPlayerID;
		Debug.LogWarning("[VOICE CHAT BLOCK]: Block list updated!");
		UpdateLocalBlockedInformationAboutUser(platformNetworkAccountPlayerID, users.ConvertAll((User x) => x.UserId));
		SendBlockedUsersToOtherPlayers(responseRequired);
	}

	private void SendBlockedUsersToOtherPlayers(bool responseRequired)
	{
		string platformNetworkAccountPlayerID = PlatformLayer.UserData.PlatformNetworkAccountPlayerID;
		HashSet<string> collection = _usersBlockData[platformNetworkAccountPlayerID];
		Debug.LogWarning("[VOICE CHAT BLOCK]: Send BlockedUsersStateChangedEvent event!");
		BlockedUsersStateChangedEvent blockedUsersStateChangedEvent = BlockedUsersStateChangedEvent.Create(GlobalTargets.Others, ReliabilityModes.ReliableOrdered);
		blockedUsersStateChangedEvent.Data = new BlockedUsersDataToken
		{
			UserId = platformNetworkAccountPlayerID,
			BlockedUsersIds = new List<string>(collection),
			ResponseRequired = responseRequired
		};
		blockedUsersStateChangedEvent.Send();
	}

	private void UpdateLocalBlockedInformationAboutUser(string userId, List<string> blockedUsers)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (blockedUsers == null)
		{
			blockedUsers = new List<string>();
		}
		foreach (string blockedUser in blockedUsers)
		{
			stringBuilder.Append(blockedUser + " ");
		}
		Debug.LogWarning($"[VOICE CHAT BLOCK] User: User {userId} | Blocked users: {stringBuilder}");
		_usersBlockData[userId] = new HashSet<string>(blockedUsers);
		UpdateMuteStates();
		this.EventBlockedUsersStateChanged?.Invoke();
	}

	private void OnUserDisconnected(ConnectedUserVoice arg1, int arg2)
	{
		UpdateMuteStates();
	}

	private void OnNewUserConnected(ConnectedUserVoice arg1, int arg2)
	{
		UpdateMuteStates();
	}

	private void UpdateMuteStates()
	{
		foreach (ConnectedUserVoice playerVoice in _boltVoiceChatService.PlayerVoices)
		{
			UpdateVoiceMuted(playerVoice);
		}
		if (_boltVoiceChatService.SelfUserVoice != null)
		{
			UpdateVoiceMuted(_boltVoiceChatService.SelfUserVoice);
		}
	}

	private void UpdateVoiceMuted(IUserVoice userVoice)
	{
		PermissionOperationResult result;
		bool mutedByPlayer;
		bool flag = !IsUserVoiceUnBlocked(userVoice, out result, out mutedByPlayer);
		if (userVoice.IsMuted != flag)
		{
			if (flag)
			{
				userVoice.Mute();
			}
			else
			{
				userVoice.UnMute();
			}
		}
	}

	private bool IsBlockedByThatUser(string userId)
	{
		if (_usersBlockData.TryGetValue(userId, out var value))
		{
			return value.Contains(Singleton<BoltVoiceChatService>.Instance.SelfUserVoice.PlatformAccountID);
		}
		return false;
	}

	private bool IsBlockedBySelf(string userId)
	{
		string platformAccountID = Singleton<BoltVoiceChatService>.Instance.SelfUserVoice.PlatformAccountID;
		if (_usersBlockData.TryGetValue(platformAccountID, out var value))
		{
			return value.Contains(userId);
		}
		return false;
	}
}
