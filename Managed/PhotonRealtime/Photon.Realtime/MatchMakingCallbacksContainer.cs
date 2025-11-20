using System.Collections.Generic;

namespace Photon.Realtime;

public class MatchMakingCallbacksContainer : List<IMatchmakingCallbacks>, IMatchmakingCallbacks
{
	private readonly LoadBalancingClient client;

	public MatchMakingCallbacksContainer(LoadBalancingClient client)
	{
		this.client = client;
	}

	public void OnCreatedRoom()
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.OnCreatedRoom();
		}
	}

	public void OnJoinedRoom()
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.OnJoinedRoom();
		}
	}

	public void OnCreateRoomFailed(short returnCode, string message)
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.OnCreateRoomFailed(returnCode, message);
		}
	}

	public void OnJoinRandomFailed(short returnCode, string message)
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.OnJoinRandomFailed(returnCode, message);
		}
	}

	public void OnJoinRoomFailed(short returnCode, string message)
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.OnJoinRoomFailed(returnCode, message);
		}
	}

	public void OnLeftRoom()
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.OnLeftRoom();
		}
	}

	public void OnFriendListUpdate(List<FriendInfo> friendList)
	{
		client.UpdateCallbackTargets();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.OnFriendListUpdate(friendList);
		}
	}
}
