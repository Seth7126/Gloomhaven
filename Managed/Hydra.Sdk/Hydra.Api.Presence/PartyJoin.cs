using Google.Protobuf.Reflection;

namespace Hydra.Api.Presence;

public enum PartyJoin
{
	[OriginalName("PARTY_JOIN_DISABLED")]
	Disabled,
	[OriginalName("PARTY_JOIN_ENABLED")]
	Enabled
}
