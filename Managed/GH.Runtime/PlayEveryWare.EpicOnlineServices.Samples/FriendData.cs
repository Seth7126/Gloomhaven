using Epic.OnlineServices;
using Epic.OnlineServices.Friends;

namespace PlayEveryWare.EpicOnlineServices.Samples;

public class FriendData
{
	public EpicAccountId LocalUserId;

	public EpicAccountId UserId;

	public ProductUserId UserProductUserId;

	public string Name;

	public FriendsStatus Status;

	public PresenceInfo Presence;

	public bool needPresence = true;
}
