using System;
using System.Collections.Generic;
using System.Linq;
using Platforms.Social;

namespace Platforms.Generic;

public class PlatformSocialGeneric : IPlatformSocial
{
	private readonly List<User> _blockedUsers = new List<User>();

	public List<User> BlockedUsers => _blockedUsers;

	public bool PlayerOnline { get; set; }

	public event Action<string> OnInviteAccepted;

	public event Action<List<User>> OnBlockedUsersListUpdated;

	public event Action<List<User>> OnMutedUsersListUpdated;

	public event Action<List<Privilege>> OnPrivilegesListUpdated;

	public event Action EventAttemptJoinOrCreateSession;

	public event Action<string> OnUserLeftPlayerSession;

	public void GetFriendsListAsync(Action<OperationResult, List<User>> resultCallback, bool isUserStatusRequired = false)
	{
		resultCallback?.Invoke(OperationResult.UnspecifiedError, null);
	}

	public void GetBlockListAsync(Action<OperationResult, List<User>> resultCallback, bool isUserStatusRequired = false)
	{
		resultCallback?.Invoke(OperationResult.UnspecifiedError, null);
	}

	public bool IsUserInBlockList(string userId)
	{
		return false;
	}

	public void AddToBlocklist(string networkAccountID, Action callback)
	{
		if (_blockedUsers.All((User x) => x.UserId != networkAccountID))
		{
			_blockedUsers.Add(new User
			{
				PictureUri = string.Empty,
				UserId = networkAccountID,
				UserName = networkAccountID,
				UserStatus = UserStatus.Unknown
			});
		}
		this.OnBlockedUsersListUpdated?.Invoke(_blockedUsers);
		callback?.Invoke();
	}

	public void GetMuteListAsync(Action<OperationResult, List<User>> resultCallback, bool isUserStatusRequired = false)
	{
		resultCallback?.Invoke(OperationResult.UnspecifiedError, null);
	}

	public bool IsUserInMuteList(string userId)
	{
		return false;
	}

	public void GetCurrentUserPrivilegesAsync(Action<OperationResult, List<Privilege>> resultCallback)
	{
		resultCallback?.Invoke(OperationResult.UnspecifiedError, null);
	}

	public void InviteAsync(string connectionString, List<string> userIDs, Action<OperationResult> resultCallback)
	{
		resultCallback?.Invoke(OperationResult.UnspecifiedError);
	}

	public void DeleteActivityAsync(Action<OperationResult> resultCallback)
	{
		resultCallback?.Invoke(OperationResult.UnspecifiedError);
	}

	public void SetActivityAsync(string connectionString, uint maxPlayers, uint currentPlayers, string groupId, Action<OperationResult> resultCallback, string psPlayerSessionID = null, bool clientSession = false)
	{
		resultCallback?.Invoke(OperationResult.UnspecifiedError);
	}

	public void RegisterInviteEvent()
	{
	}

	public void UnregisterInviteEvent()
	{
	}

	public void ViewUserProfile(ulong userId, string userName)
	{
	}

	public void UpdateRecentPlayers(List<RecentPlayer> recentPlayers, RecentPlayerType recentPlayerType = RecentPlayerType.Default)
	{
	}

	public void ClearRecentPlayers()
	{
	}

	public void AskUserToAcquirePrivilegeAsync(Privilege privilege, Action<OperationResult> resultCallback)
	{
		resultCallback?.Invoke(OperationResult.UnspecifiedError);
	}

	public void GetPermissionsTowardsPlatformUsersAsync(HashSet<string> usersIds, Action<OperationResult, Dictionary<string, Dictionary<Permission, List<PermissionOperationResult>>>> resultCallback)
	{
		resultCallback?.Invoke(OperationResult.Success, null);
	}

	public string GetPSPlayerSession()
	{
		return null;
	}

	public void GetUsersAsync(List<string> userIDs, Action<OperationResult, List<User>> resultCallback, bool isUserStatusRequired = false)
	{
		resultCallback?.Invoke(OperationResult.UnspecifiedError, null);
	}

	public void GetUserAsync(ulong userId, Action<OperationResult, User> resultCallback, bool isUserStatusRequired = false)
	{
		resultCallback?.Invoke(OperationResult.UnspecifiedError, new User());
	}
}
