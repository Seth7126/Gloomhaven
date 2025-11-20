using Google.Protobuf.Reflection;

namespace Hydra.Api.Presence;

public enum MatchmakeJIPState
{
	[OriginalName("MATCHMAKE_JIP_STATE_DISABLED")]
	Disabled,
	[OriginalName("MATCHMAKE_JIP_STATE_ENABLED")]
	Enabled
}
