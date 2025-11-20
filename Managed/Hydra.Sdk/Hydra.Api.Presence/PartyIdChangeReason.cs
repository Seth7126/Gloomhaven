using Google.Protobuf.Reflection;

namespace Hydra.Api.Presence;

public enum PartyIdChangeReason
{
	[OriginalName("PARTY_ID_CHANGE_REASON_NONE")]
	None = 0,
	[OriginalName("PARTY_ID_CHANGE_REASON_CREATE")]
	Create = 1,
	[OriginalName("PARTY_ID_CHANGE_REASON_JOIN")]
	Join = 2,
	[OriginalName("PARTY_ID_CHANGE_REASON_LEAVE")]
	Leave = 4,
	[OriginalName("PARTY_ID_CHANGE_REASON_KICK")]
	Kick = 5,
	[OriginalName("PARTY_ID_CHANGE_REASON_DISBAND")]
	Disband = 6
}
