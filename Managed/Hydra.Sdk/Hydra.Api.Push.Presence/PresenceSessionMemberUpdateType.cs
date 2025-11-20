using Google.Protobuf.Reflection;

namespace Hydra.Api.Push.Presence;

public enum PresenceSessionMemberUpdateType
{
	[OriginalName("PRESENCE_SESSION_MEMBER_UPDATE_TYPE_NONE")]
	None,
	[OriginalName("PRESENCE_SESSION_MEMBER_UPDATE_TYPE_ADD")]
	Add,
	[OriginalName("PRESENCE_SESSION_MEMBER_UPDATE_TYPE_REMOVE")]
	Remove,
	[OriginalName("PRESENCE_SESSION_MEMBER_UPDATE_TYPE_UPDATE")]
	Update
}
