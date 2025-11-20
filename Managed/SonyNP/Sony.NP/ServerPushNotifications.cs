using System;
using System.Runtime.InteropServices;

namespace Sony.NP;

[Obsolete("ServerPushNotifications is deprecated, please use PushNotificationsFlags instead.")]
public struct ServerPushNotifications
{
	[MarshalAs(UnmanagedType.I1)]
	internal bool newGameDataMessage;

	[MarshalAs(UnmanagedType.I1)]
	internal bool newInvitation;

	[MarshalAs(UnmanagedType.I1)]
	internal bool updateBlockedUsersList;

	[MarshalAs(UnmanagedType.I1)]
	internal bool updateFriendPresence;

	[MarshalAs(UnmanagedType.I1)]
	internal bool updateFriendsList;

	[MarshalAs(UnmanagedType.I1)]
	internal bool newInGameMessage;

	public bool NewGameDataMessage
	{
		get
		{
			return newGameDataMessage;
		}
		set
		{
			newGameDataMessage = value;
		}
	}

	public bool NewInvitation
	{
		get
		{
			return newInvitation;
		}
		set
		{
			newInvitation = value;
		}
	}

	public bool UpdateBlockedUsersList
	{
		get
		{
			return updateBlockedUsersList;
		}
		set
		{
			updateBlockedUsersList = value;
		}
	}

	public bool UpdateFriendPresence
	{
		get
		{
			return updateFriendPresence;
		}
		set
		{
			updateFriendPresence = value;
		}
	}

	public bool UpdateFriendsList
	{
		get
		{
			return updateFriendsList;
		}
		set
		{
			updateFriendsList = value;
		}
	}

	public bool NewInGameMessage
	{
		get
		{
			return newInGameMessage;
		}
		set
		{
			newInGameMessage = value;
		}
	}

	public void Init()
	{
		newGameDataMessage = true;
		newInvitation = true;
		updateBlockedUsersList = true;
		updateFriendPresence = true;
		updateFriendsList = true;
		newInGameMessage = true;
	}
}
