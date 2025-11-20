using Google.Protobuf.Reflection;

namespace Hydra.Api.Push.Presence;

public enum PresencePartyMemberUpdateType
{
	[OriginalName("PRESENCE_PARTY_MEMBER_UPDATE_TYPE_NONE")]
	None,
	[OriginalName("PRESENCE_PARTY_MEMBER_UPDATE_TYPE_ADD")]
	Add,
	[OriginalName("PRESENCE_PARTY_MEMBER_UPDATE_TYPE_REMOVE")]
	Remove,
	[OriginalName("PRESENCE_PARTY_MEMBER_UPDATE_TYPE_UPDATE")]
	Update
}
