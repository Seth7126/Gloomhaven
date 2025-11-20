using System;
using System.Collections.Generic;
using Platforms;
using Platforms.Social;
using UnityEngine.Events;

public interface IPlatformNetworking
{
	bool PlatformInvitesSupported { get; }

	bool HasInvitePending { get; }

	void Initialize(IPlatform platform);

	void Shutdown();

	void OpenPlatformInviteOverlay();

	void JoinSession(string sessionId, Action<OperationResult> resultCallback);

	void CreateClientSession(string sessionId, Action<OperationResult> resultCallback);

	bool CheckCommandLineForPendingInvite();

	void JoinOnPendingInvite(UnityAction<string> joinCompleteAction);

	void RegisterNetworkSessionService(INetworkSessionService sessionService, INetworkHeroAssignService heroAssignService);

	void LeaveSession(Action<OperationResult> resultCallback);

	byte[] GetRecentPlayerKey();

	void InviteUser(string userId, Action<OperationResult> resultCallback);

	void GetCurrentUserPrivilegesAsync(Action<OperationResult, bool> resultCallback, PrivilegePlatform privilegePlatform, Privilege target);

	void AskUserToAcquirePrivilegeAsync(Privilege privilege, Action<OperationResult> resultCallback);

	void GetPermissionsTowardsPlatformUsersAsync(HashSet<string> usersIds, Action<OperationResult, Dictionary<string, Dictionary<Permission, List<PermissionOperationResult>>>> resultCallback);
}
