using Google.Protobuf.Reflection;

namespace Hydra.Api.Presence;

public enum PartyState
{
	[OriginalName("PARTY_STATE_NONE")]
	None,
	[OriginalName("PARTY_STATE_CREATED")]
	Created
}
