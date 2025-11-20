using Google.Protobuf.Reflection;

namespace Hydra.Api.Presence;

public enum MatchmakeState
{
	[OriginalName("MATCHMAKE_STATE_NONE")]
	None,
	[OriginalName("MATCHMAKE_STATE_QUEUE")]
	Queue,
	[OriginalName("MATCHMAKE_STATE_GAME")]
	Game
}
