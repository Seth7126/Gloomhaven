using System;

namespace Platforms.PS4;

[Flags]
public enum NotificationFlags
{
	None = 0,
	[Obsolete("NewGameDataMessage notifications are no longer available")]
	NewGameDataMessage = 1,
	NewInvitation = 2,
	UpdateBlockedUsersList = 4,
	UpdateFriendPresence = 8,
	UpdateFriendsList = 0x10,
	NewInGameMessage = 0x20
}
