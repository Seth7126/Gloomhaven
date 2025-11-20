using Google.Protobuf.Reflection;

namespace Hydra.Api.Push.Presence;

public enum PresencePartyUpdateType
{
	[OriginalName("PRESENCE_PARTY_UPDATE_TYPE_NONE")]
	None = 0,
	[OriginalName("PRESENCE_PARTY_UPDATE_TYPE_ID")]
	Id = 1,
	[OriginalName("PRESENCE_PARTY_UPDATE_TYPE_DATA")]
	Data = 2,
	[OriginalName("PRESENCE_PARTY_UPDATE_TYPE_SETTINGS")]
	Settings = 4,
	[OriginalName("PRESENCE_PARTY_UPDATE_TYPE_MEMBERS")]
	Members = 8,
	[OriginalName("PRESENCE_PARTY_UPDATE_TYPE_JOIN_CODE")]
	JoinCode = 0x10
}
