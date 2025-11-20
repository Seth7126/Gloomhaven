using System;
using System.Collections.Generic;

namespace Platforms.Social;

public interface IPlatformSocial
{
	List<User> BlockedUsers { get; }

	event Action<string> OnInviteAccepted;

	event Action<List<User>> OnBlockedUsersListUpdated;

	event Action<List<User>> OnMutedUsersListUpdated;

	event Action<List<Privilege>> OnPrivilegesListUpdated;

	event Action EventAttemptJoinOrCreateSession;

	event Action<string> OnUserLeftPlayerSession;

	void RegisterInviteEvent();

	void UnregisterInviteEvent();

	void GetFriendsListAsync(Action<OperationResult, List<User>> resultCallback, bool isUserStatusRequired = false);

	void GetBlockListAsync(Action<OperationResult, List<User>> resultCallback, bool isUserStatusRequired = false);

	bool IsUserInBlockList(string userId);

	void AddToBlocklist(string networkAccountID, Action callback);

	void GetMuteListAsync(Action<OperationResult, List<User>> resultCallback, bool isUserStatusRequired = false);

	bool IsUserInMuteList(string userId);

	void InviteAsync(string connectionString, List<string> userIDs, Action<OperationResult> resultCallback);

	void DeleteActivityAsync(Action<OperationResult> resultCallback);

	void SetActivityAsync(string connectionString, uint maxPlayers, uint currentPlayers, string groupId, Action<OperationResult> resultCallback, string psPlayerSessionID = null, bool clientSession = false);

	void ViewUserProfile(ulong userId, string userName);

	void GetUsersAsync(List<string> userIDs, Action<OperationResult, List<User>> resultCallback, bool isUserStatusRequired = false);

	void GetUserAsync(ulong userId, Action<OperationResult, User> resultCallback, bool isUserStatusRequired = false);

	void UpdateRecentPlayers(List<RecentPlayer> recentPlayers, RecentPlayerType recentPlayerType = RecentPlayerType.Default);

	void ClearRecentPlayers();

	void GetCurrentUserPrivilegesAsync(Action<OperationResult, List<Privilege>> resultCallback);

	void AskUserToAcquirePrivilegeAsync(Privilege privilege, Action<OperationResult> resultCallback);

	void GetPermissionsTowardsPlatformUsersAsync(HashSet<string> usersIds, Action<OperationResult, Dictionary<string, Dictionary<Permission, List<PermissionOperationResult>>>> resultCallback);

	string GetPSPlayerSession();
}
