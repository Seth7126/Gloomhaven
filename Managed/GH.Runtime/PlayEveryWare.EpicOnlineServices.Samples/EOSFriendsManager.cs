#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.Friends;
using Epic.OnlineServices.Presence;
using Epic.OnlineServices.UI;
using Epic.OnlineServices.UserInfo;

namespace PlayEveryWare.EpicOnlineServices.Samples;

public class EOSFriendsManager : IEOSSubManager
{
	public delegate void OnFriendsCallback(Result result);

	private Dictionary<EpicAccountId, FriendData> CachedFriends;

	private bool CachedFriendsDirty;

	private Dictionary<EpicAccountId, FriendData> CachedSearchResults;

	private bool CachedSearchResultsDirty;

	private OnFriendsCallback AddFriendCallback;

	private OnFriendsCallback QueryFriendCallback;

	private OnFriendsCallback AcceptInviteCallback;

	private OnFriendsCallback RejectInviteCallback;

	private OnFriendsCallback ShowFriendsOverlayCallback;

	private OnFriendsCallback HideFriendsOverlayCallback;

	private OnFriendsCallback QueryUserInfoCallback;

	private FriendsInterface FriendsHandle;

	private UserInfoInterface UserInfoHandle;

	private PresenceInterface PresenceHandle;

	private ConnectInterface ConnectHandle;

	private Dictionary<EpicAccountId, ulong> FriendNotifications = new Dictionary<EpicAccountId, ulong>();

	private Dictionary<EpicAccountId, ulong> PresenceNotifications = new Dictionary<EpicAccountId, ulong>();

	public EOSFriendsManager()
	{
		CachedFriends = new Dictionary<EpicAccountId, FriendData>();
		CachedFriendsDirty = true;
		CachedSearchResults = new Dictionary<EpicAccountId, FriendData>();
		CachedSearchResultsDirty = true;
		FriendsHandle = EOSManager.Instance.GetEOSPlatformInterface().GetFriendsInterface();
		UserInfoHandle = EOSManager.Instance.GetEOSPlatformInterface().GetUserInfoInterface();
		PresenceHandle = EOSManager.Instance.GetEOSPlatformInterface().GetPresenceInterface();
		ConnectHandle = EOSManager.Instance.GetEOSPlatformInterface().GetConnectInterface();
	}

	public bool GetCachedFriends(out Dictionary<EpicAccountId, FriendData> Friends)
	{
		Friends = CachedFriends;
		bool cachedFriendsDirty = CachedFriendsDirty;
		CachedFriendsDirty = false;
		return cachedFriendsDirty;
	}

	public bool AnyCachedFriends()
	{
		return CachedFriends.Count > 0;
	}

	public EpicAccountId GetAccountMapping(ProductUserId targetUserId)
	{
		foreach (FriendData value in CachedFriends.Values)
		{
			if (targetUserId == value.UserProductUserId)
			{
				return value.UserId;
			}
		}
		return new EpicAccountId();
	}

	public string GetDisplayName(EpicAccountId targetAccountId)
	{
		if (CachedFriends.TryGetValue(targetAccountId, out var value))
		{
			return value.Name;
		}
		return string.Empty;
	}

	public bool GetCachedSearchResults(out Dictionary<EpicAccountId, FriendData> SearchResults)
	{
		SearchResults = CachedSearchResults;
		bool cachedSearchResultsDirty = CachedSearchResultsDirty;
		CachedSearchResultsDirty = false;
		return cachedSearchResultsDirty;
	}

	public void ClearCachedSearchResults()
	{
		CachedSearchResults.Clear();
		CachedSearchResultsDirty = true;
	}

	[Obsolete("SendInvite is obsolete.  ErrorCode=NotImplemented")]
	public void SendInvite(EpicAccountId friendUserId, OnFriendsCallback AddFriendCompleted)
	{
		if (friendUserId.IsValid())
		{
			Debug.LogError("Friends (AddFriend): friendUserId parameter is invalid!");
			AddFriendCompleted?.Invoke(Result.InvalidProductUserID);
			return;
		}
		SendInviteOptions options = new SendInviteOptions
		{
			LocalUserId = EOSManager.Instance.GetLocalUserId(),
			TargetUserId = friendUserId
		};
		AddFriendCallback = AddFriendCompleted;
		FriendsHandle.SendInvite(ref options, null, OnSendInviteCompleted);
	}

	private void OnSendInviteCompleted(ref SendInviteCallbackInfo data)
	{
		if (data.ResultCode != Result.Success)
		{
			Debug.LogErrorFormat("Friends (SendInviteCallback): SendInvite error: {0}", data.ResultCode);
			AddFriendCallback?.Invoke(data.ResultCode);
		}
		else
		{
			Debug.LogFormat("Friends (SendInviteCallback): SendInvite Complete for user id: {0}", data.LocalUserId);
			AddFriendCallback?.Invoke(Result.Success);
		}
	}

	public void AcceptInvite(EpicAccountId friendUserId, OnFriendsCallback AcceptInviteCompleted)
	{
		if (friendUserId.IsValid())
		{
			Debug.LogError("Friends (AcceptInvite): friendUserId parameter is invalid!");
			AcceptInviteCompleted?.Invoke(Result.InvalidProductUserID);
			return;
		}
		AcceptInviteOptions options = new AcceptInviteOptions
		{
			LocalUserId = EOSManager.Instance.GetLocalUserId(),
			TargetUserId = friendUserId
		};
		AcceptInviteCallback = AcceptInviteCompleted;
		FriendsHandle.AcceptInvite(ref options, null, OnAcceptInviteCompleted);
	}

	private void OnAcceptInviteCompleted(ref AcceptInviteCallbackInfo data)
	{
		if (data.ResultCode != Result.Success)
		{
			Debug.LogErrorFormat("Friends (OnAcceptInviteCompleted): AcceptInvite error: {0}", data.ResultCode);
			AcceptInviteCallback?.Invoke(data.ResultCode);
		}
		else
		{
			Debug.LogFormat("Friends (OnAcceptInviteCompleted): Accept Invite Complete for user id: {0}", data.LocalUserId);
			AcceptInviteCallback?.Invoke(Result.Success);
		}
	}

	public void RejectInvite(EpicAccountId friendUserId, OnFriendsCallback RejectInviteCompleted)
	{
		if (friendUserId.IsValid())
		{
			Debug.LogError("Friends (RejectInvite): friendUserId parameter is invalid!");
			RejectInviteCompleted?.Invoke(Result.InvalidProductUserID);
			return;
		}
		RejectInviteOptions options = new RejectInviteOptions
		{
			LocalUserId = EOSManager.Instance.GetLocalUserId(),
			TargetUserId = friendUserId
		};
		RejectInviteCallback = RejectInviteCompleted;
		FriendsHandle.RejectInvite(ref options, null, OnRejectInviteCompleted);
	}

	private void OnRejectInviteCompleted(ref RejectInviteCallbackInfo data)
	{
		if (data.ResultCode != Result.Success)
		{
			Debug.LogErrorFormat("Friends (OnRejectInviteCompleted): RejectInvite error: {0}", data.ResultCode);
			RejectInviteCallback?.Invoke(data.ResultCode);
		}
		else
		{
			Debug.LogFormat("Friends (OnRejectInviteCompleted): Reject Invite Complete for user id: {0}", data.LocalUserId);
			RejectInviteCallback?.Invoke(Result.Success);
		}
	}

	public void QueryFriends(OnFriendsCallback QueryFriendsCompleted)
	{
		QueryFriends(EOSManager.Instance.GetLocalUserId(), QueryFriendsCompleted);
	}

	public void QueryFriends(EpicAccountId userId, OnFriendsCallback QueryFriendsCompleted)
	{
		QueryFriendsOptions options = new QueryFriendsOptions
		{
			LocalUserId = userId
		};
		QueryFriendCallback = QueryFriendsCompleted;
		FriendsHandle.QueryFriends(ref options, null, QueryFriendsCallback);
	}

	public void QueryFriendsCallback(ref QueryFriendsCallbackInfo data)
	{
		if (data.ResultCode != Result.Success)
		{
			Debug.LogErrorFormat("Friends (QueryFriendsCallback): QueryFriends error: {0}", data.ResultCode);
			QueryFriendCallback?.Invoke(data.ResultCode);
			return;
		}
		Debug.LogFormat("Friends (QueryFriendsCallback): Query Friends Complete for user id: {0}", data.LocalUserId);
		GetFriendsCountOptions options = new GetFriendsCountOptions
		{
			LocalUserId = EOSManager.Instance.GetLocalUserId()
		};
		int friendsCount = FriendsHandle.GetFriendsCount(ref options);
		Debug.LogFormat("Friends (QueryFriendsCallback): Number of friends: {0}", friendsCount);
		GetFriendAtIndexOptions options2 = new GetFriendAtIndexOptions
		{
			LocalUserId = EOSManager.Instance.GetLocalUserId()
		};
		Dictionary<EpicAccountId, FriendData> dictionary = new Dictionary<EpicAccountId, FriendData>();
		for (int i = 0; i < friendsCount; i++)
		{
			options2.Index = i;
			EpicAccountId friendAtIndex = FriendsHandle.GetFriendAtIndex(ref options2);
			if (friendAtIndex.IsValid())
			{
				GetStatusOptions options3 = new GetStatusOptions
				{
					LocalUserId = EOSManager.Instance.GetLocalUserId(),
					TargetUserId = friendAtIndex
				};
				FriendsStatus status = FriendsHandle.GetStatus(ref options3);
				Debug.LogFormat("Friends (QueryFriendsCallback): Friend Status {0} => {1}", friendAtIndex, status);
				FriendData friendData = new FriendData
				{
					LocalUserId = data.LocalUserId,
					UserId = friendAtIndex,
					Name = "Pending...",
					Status = status
				};
				dictionary.Add(friendData.UserId, friendData);
			}
			else
			{
				Debug.LogWarning("Friends (QueryFriendsCallback): Invalid friend found in friends list");
			}
		}
		bool flag = true;
		if (CachedFriends.Count == dictionary.Count)
		{
			flag = false;
			foreach (EpicAccountId key in CachedFriends.Keys)
			{
				if (!dictionary.ContainsKey(key))
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			CachedFriends = dictionary;
			CachedFriendsDirty = true;
		}
		foreach (FriendData value in CachedFriends.Values)
		{
			if (value.needPresence)
			{
				QueryPresenceInfo(EOSManager.Instance.GetLocalUserId(), value.UserId);
			}
			QueryUserInfo(value.UserId, null);
			QueryFriendsConnectMappings();
		}
		QueryFriendCallback?.Invoke(Result.Success);
	}

	private void QueryPresenceInfo(EpicAccountId localUserId, EpicAccountId targetUserId)
	{
		QueryPresenceOptions options = new QueryPresenceOptions
		{
			LocalUserId = localUserId,
			TargetUserId = targetUserId
		};
		PresenceHandle.QueryPresence(ref options, null, OnQueryPresenceCompleted);
	}

	private void OnQueryPresenceCompleted(ref QueryPresenceCallbackInfo data)
	{
		if (data.ResultCode != Result.Success)
		{
			Debug.LogErrorFormat("Friends (OnQueryPresenceCompleted): Error calling QueryPresence: " + data.ResultCode);
			return;
		}
		Debug.Log("Friends (OnQueryPresenceCompleted): QueryPresence successful");
		CopyPresenceOptions options = new CopyPresenceOptions
		{
			LocalUserId = data.LocalUserId,
			TargetUserId = data.TargetUserId
		};
		Info? outPresence;
		Result result = PresenceHandle.CopyPresence(ref options, out outPresence);
		if (result != Result.Success)
		{
			Debug.LogErrorFormat("Friends (OnQueryPresenceCompleted): CopyPresence error: {0}", result);
			return;
		}
		PresenceInfo presenceInfo = new PresenceInfo
		{
			Application = outPresence?.ProductName,
			Platform = outPresence?.Platform,
			Status = (outPresence?.Status).Value,
			RichText = outPresence?.RichText
		};
		if (CachedFriends.TryGetValue(data.TargetUserId, out var value))
		{
			value.Presence = presenceInfo;
			value.needPresence = false;
			CachedFriendsDirty = true;
			Debug.LogFormat("Friends (OnQueryPresenceCompleted): PresenceInfo (Status) updated for target user: {0}, new Status: {1}", data.TargetUserId, presenceInfo.Status.ToString());
		}
		else
		{
			data.TargetUserId.ToString(out var outBuffer);
			if (string.IsNullOrEmpty(outBuffer))
			{
				Debug.LogWarningFormat("Friends (OnQueryPresenceCompleted): PresenceInfo not stored, couldn't find target user in friends cache: {0}, ", data.TargetUserId);
			}
		}
	}

	private void QueryFriendsConnectMappings()
	{
		if (CachedFriends.Count == 0)
		{
			Debug.LogError("Friends (QueryFriendsConnectMappings): No friend cache.");
			return;
		}
		Utf8String[] array = new Utf8String[CachedFriends.Count];
		int num = 0;
		foreach (EpicAccountId key in CachedFriends.Keys)
		{
			Utf8String outBuffer;
			Result result = key.ToString(out outBuffer);
			if (result != Result.Success)
			{
				Debug.LogErrorFormat("Friends (QueryFriendsConnectMappings): Couldn't convert EpicAccountId to string: {0}", result);
				return;
			}
			array[num] = outBuffer;
			num++;
		}
		QueryExternalAccountMappingsOptions options = new QueryExternalAccountMappingsOptions
		{
			AccountIdType = ExternalAccountType.Epic,
			LocalUserId = EOSManager.Instance.GetProductUserId(),
			ExternalAccountIds = array
		};
		ConnectHandle.QueryExternalAccountMappings(ref options, null, OnQueryExternalAccountMappingsCompleted);
	}

	private void OnQueryExternalAccountMappingsCompleted(ref QueryExternalAccountMappingsCallbackInfo data)
	{
		if (data.ResultCode != Result.Success)
		{
			Debug.LogErrorFormat("Friends (OnQueryExternalAccountMappingsCompleted): Error calling QueryExternalAccountMappings: " + data.ResultCode);
			return;
		}
		Debug.Log("Friends (OnQueryExternalAccountMappingsCompleted): QueryExternalAccountMappings successful");
		Dictionary<EpicAccountId, ProductUserId> dictionary = new Dictionary<EpicAccountId, ProductUserId>();
		foreach (FriendData value2 in CachedFriends.Values)
		{
			if (!(value2.UserProductUserId == null) && value2.UserProductUserId.IsValid())
			{
				continue;
			}
			Utf8String outBuffer;
			Result result = value2.UserId.ToString(out outBuffer);
			if (result == Result.Success)
			{
				GetExternalAccountMappingsOptions options = new GetExternalAccountMappingsOptions
				{
					AccountIdType = ExternalAccountType.Epic,
					LocalUserId = data.LocalUserId,
					TargetExternalUserId = outBuffer
				};
				ProductUserId externalAccountMapping = ConnectHandle.GetExternalAccountMapping(ref options);
				if (externalAccountMapping != null && externalAccountMapping.IsValid())
				{
					dictionary.Add(value2.UserId, externalAccountMapping);
					continue;
				}
				Debug.LogWarningFormat("Friends (OnQueryExternalAccountMappingsCompleted): No connected Epic Account associated with EpicAccountId = ({0})", outBuffer);
			}
			else
			{
				Debug.LogErrorFormat("Friends (OnQueryExternalAccountMappingsCompleted): ToString of FriendData.UserId failed with result = {0}", result);
			}
		}
		foreach (KeyValuePair<EpicAccountId, ProductUserId> item in dictionary)
		{
			if (CachedFriends.TryGetValue(item.Key, out var value))
			{
				value.UserProductUserId = item.Value;
				CopyProductUserInfoOptions options2 = new CopyProductUserInfoOptions
				{
					TargetUserId = value.UserProductUserId
				};
				ConnectHandle.CopyProductUserInfo(ref options2, out var _);
			}
			else
			{
				Debug.LogErrorFormat("Friends (OnQueryExternalAccountMappingsCompleted): Error updating ProductUserId for friend {0}", item.Key);
			}
		}
	}

	public void QueryUserInfo(string displayName, OnFriendsCallback QueryUserInfoCompleted)
	{
		CachedSearchResults.Clear();
		CachedSearchResultsDirty = true;
		QueryUserInfo(EOSManager.Instance.GetLocalUserId(), displayName, QueryUserInfoCompleted);
	}

	public void QueryUserInfo(EpicAccountId localUserId, string displayName, OnFriendsCallback QueryUserInfoCompleted)
	{
		QueryUserInfoByDisplayNameOptions options = new QueryUserInfoByDisplayNameOptions
		{
			LocalUserId = localUserId,
			DisplayName = displayName
		};
		UserInfoHandle.QueryUserInfoByDisplayName(ref options, null, QueryUserInfoByDisplaynameCompleted);
	}

	private void QueryUserInfoByDisplaynameCompleted(ref QueryUserInfoByDisplayNameCallbackInfo data)
	{
		if (data.ResultCode != Result.Success)
		{
			Debug.LogErrorFormat("Friends (QueryUserInfoByDisplaynameCompleted): ResultCode error: {0}", data.ResultCode);
			return;
		}
		Debug.LogFormat("Friends (QueryUserInfoByDisplaynameCompleted): Query User Info Complete - UserId: {0}", data.TargetUserId);
		FriendData friendData = new FriendData
		{
			Name = data.DisplayName,
			Status = FriendsStatus.NotFriends,
			LocalUserId = data.LocalUserId,
			UserId = data.TargetUserId
		};
		CachedSearchResults.Add(friendData.UserId, friendData);
		CachedSearchResultsDirty = true;
	}

	public void AddFriend(EpicAccountId friendUserId)
	{
		if (!friendUserId.IsValid())
		{
			Debug.LogError("EOSFriendManager (QueryUserInfoByDisplaynameCompleted): friendUserId parameter is invalid.");
			return;
		}
		SendInviteOptions options = new SendInviteOptions
		{
			LocalUserId = EOSManager.Instance.GetLocalUserId(),
			TargetUserId = friendUserId
		};
		FriendsHandle.SendInvite(ref options, null, SendInviteCompleted);
	}

	private void SendInviteCompleted(ref SendInviteCallbackInfo data)
	{
		if (data.ResultCode != Result.Success)
		{
			Debug.LogErrorFormat("Friends (SendInviteCompleted): ResultCode error: {0}", data.ResultCode);
		}
		else
		{
			Debug.Log("Friends (SendInviteCompleted): Invite Sent.");
			QueryFriends(data.LocalUserId, null);
		}
	}

	public void QueryUserInfo(EpicAccountId targetUserId, OnFriendsCallback QueryUserInfoCompleted)
	{
		if (!targetUserId.IsValid())
		{
			Debug.LogError("Friends (QueryUserInfo): targetUserId parameter is invalid!");
			QueryUserInfoCompleted?.Invoke(Result.InvalidParameters);
			return;
		}
		QueryUserInfoOptions options = new QueryUserInfoOptions
		{
			LocalUserId = EOSManager.Instance.GetLocalUserId(),
			TargetUserId = targetUserId
		};
		QueryUserInfoCallback = QueryUserInfoCompleted;
		UserInfoHandle.QueryUserInfo(ref options, null, OnQueryUserInfoCompleted);
	}

	private void OnQueryUserInfoCompleted(ref QueryUserInfoCallbackInfo data)
	{
		if (data.ResultCode != Result.Success)
		{
			Debug.LogErrorFormat("Friends (OnQueryUserInfoCompleted): Error calling QueryUserInfo: " + data.ResultCode);
			QueryUserInfoCallback?.Invoke(data.ResultCode);
			return;
		}
		Debug.Log("Friends (OnQueryUserInfoCompleted): QueryUserInfo successful");
		QueryUserInfoCallback?.Invoke(Result.Success);
		CopyUserInfoOptions options = new CopyUserInfoOptions
		{
			LocalUserId = data.LocalUserId,
			TargetUserId = data.TargetUserId
		};
		UserInfoData? outUserInfo;
		Result result = UserInfoHandle.CopyUserInfo(ref options, out outUserInfo);
		if (result != Result.Success)
		{
			Debug.LogErrorFormat("Friends (OnQueryUserInfoCompleted): CopyUserInfo error: {0}", result);
			QueryUserInfoCallback?.Invoke(result);
			return;
		}
		FriendData friendData = new FriendData
		{
			LocalUserId = data.LocalUserId,
			UserId = data.TargetUserId,
			Name = outUserInfo?.DisplayName
		};
		if (CachedFriends.TryGetValue(friendData.UserId, out var value))
		{
			Debug.Log("Friends (OnQueryUserInfoCompleted): FriendData (LocalUserId, Name) Updated");
			value.LocalUserId = friendData.LocalUserId;
			value.Name = friendData.Name;
			CachedFriendsDirty = true;
		}
		else
		{
			CachedFriends.Add(friendData.UserId, friendData);
			CachedFriendsDirty = true;
			Debug.Log("Friends (OnQueryUserInfoCompleted): FriendData Added");
		}
		QueryUserInfoCallback?.Invoke(Result.Success);
	}

	public void ShowFriendsOverlay(OnFriendsCallback ShowFriendsOverlayCompleted)
	{
		ShowFriendsOverlayCallback = ShowFriendsOverlayCompleted;
		ShowFriendsOptions options = new ShowFriendsOptions
		{
			LocalUserId = EOSManager.Instance.GetLocalUserId()
		};
		EOSManager.Instance.GetEOSPlatformInterface().GetUIInterface().ShowFriends(ref options, null, OnShowFriendsCallback);
	}

	private void OnShowFriendsCallback(ref ShowFriendsCallbackInfo data)
	{
		if (data.ResultCode != Result.Success)
		{
			Debug.LogErrorFormat("Friends (OnShowFriendsCallback): Error calling ShowFriends: " + data.ResultCode);
			ShowFriendsOverlayCallback?.Invoke(data.ResultCode);
		}
		else
		{
			Debug.Log("Friends (OnShowFriendsCallback): ShowFriends successful");
			ShowFriendsOverlayCallback?.Invoke(Result.Success);
		}
	}

	public void HideFriendsOverlay(OnFriendsCallback HideFriendsOverlayCompleted)
	{
		HideFriendsOverlayCallback = HideFriendsOverlayCompleted;
		HideFriendsOptions options = new HideFriendsOptions
		{
			LocalUserId = EOSManager.Instance.GetLocalUserId()
		};
		EOSManager.Instance.GetEOSPlatformInterface().GetUIInterface().HideFriends(ref options, null, OnHideFriendsCallback);
	}

	private void OnHideFriendsCallback(ref HideFriendsCallbackInfo data)
	{
		if (data.ResultCode != Result.Success)
		{
			Debug.LogErrorFormat("Friends (OnHideFriendsCallback): Error calling HideFriends: " + data.ResultCode);
			HideFriendsOverlayCallback?.Invoke(data.ResultCode);
		}
		else
		{
			Debug.Log("Friends (OnHideFriendsCallback): HideFriends successful");
			HideFriendsOverlayCallback?.Invoke(Result.Success);
		}
	}

	public void SubscribeToFriendUpdates(EpicAccountId userId)
	{
		if (userId == null || !userId.IsValid())
		{
			Debug.LogWarning("Friends (SubscribeToFriendUpdates): userId parameter is not valid.");
			return;
		}
		UnsubscribeFromFriendUpdates(userId);
		AddNotifyFriendsUpdateOptions options = default(AddNotifyFriendsUpdateOptions);
		ulong num = FriendsHandle.AddNotifyFriendsUpdate(ref options, null, OnFriendsUpdateCallbackHandler);
		if (num == 0L)
		{
			Debug.LogError("Friends (SubscribeToFriendUpdates): Could not subscribe to friend update notifications.");
		}
		else
		{
			FriendNotifications[userId] = num;
		}
		AddNotifyOnPresenceChangedOptions options2 = default(AddNotifyOnPresenceChangedOptions);
		ulong num2 = PresenceHandle.AddNotifyOnPresenceChanged(ref options2, null, OnPresenceChangedCallbackHandler);
		if (num2 == 0L)
		{
			Debug.LogError("Friends (SubscribeToFriendUpdates): Could not subscribe to presence changed notifications.");
		}
		else
		{
			PresenceNotifications[userId] = num2;
		}
	}

	private void UnsubscribeFromFriendUpdates(EpicAccountId userId)
	{
		if (userId == null || !userId.IsValid())
		{
			Debug.LogWarning("Friends (UnsubscribeFromFriendUpdates): userId parameter is not valid.");
			return;
		}
		if (FriendNotifications == null || PresenceNotifications == null)
		{
			Debug.LogWarning("Friends (UnsubscribeFromFriendUpdates): Not initialized yet, try again.");
			return;
		}
		if (FriendNotifications.TryGetValue(userId, out var value))
		{
			FriendsHandle.RemoveNotifyFriendsUpdate(value);
			FriendNotifications.Remove(userId);
		}
		if (PresenceNotifications.TryGetValue(userId, out var value2))
		{
			PresenceHandle.RemoveNotifyOnPresenceChanged(value2);
			PresenceNotifications.Remove(userId);
		}
	}

	private void OnFriendsUpdateCallbackHandler(ref OnFriendsUpdateInfo data)
	{
		if (!(EOSManager.Instance.GetLocalUserId() == data.LocalUserId))
		{
			return;
		}
		FriendData value;
		if (data.CurrentStatus == FriendsStatus.NotFriends)
		{
			if (CachedFriends.ContainsKey(data.TargetUserId))
			{
				CachedFriends.Remove(data.TargetUserId);
				CachedFriendsDirty = true;
			}
		}
		else if (CachedFriends.TryGetValue(data.TargetUserId, out value))
		{
			if (value.Status != data.CurrentStatus)
			{
				value.Status = data.CurrentStatus;
				CachedFriendsDirty = true;
			}
		}
		else
		{
			QueryFriends(null);
		}
	}

	private void OnPresenceChangedCallbackHandler(ref PresenceChangedCallbackInfo data)
	{
		QueryPresenceInfo(data.LocalUserId, data.PresenceUserId);
	}

	public void OnLoggedIn()
	{
		QueryFriends(null);
		SubscribeToFriendUpdates(EOSManager.Instance.GetLocalUserId());
	}

	public void OnLoggedOut()
	{
		EpicAccountId localUserId = EOSManager.Instance.GetLocalUserId();
		if (localUserId != null && localUserId.IsValid())
		{
			UnsubscribeFromFriendUpdates(localUserId);
		}
	}
}
