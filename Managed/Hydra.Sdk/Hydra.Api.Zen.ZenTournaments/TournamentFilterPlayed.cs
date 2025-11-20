using Google.Protobuf.Reflection;

namespace Hydra.Api.Zen.ZenTournaments;

public enum TournamentFilterPlayed
{
	[OriginalName("TOURNAMENT_FILTER_PLAYED_NONE")]
	None,
	[OriginalName("TOURNAMENT_FILTER_PLAYED_TRUE")]
	True,
	[OriginalName("TOURNAMENT_FILTER_PLAYED_FALSE")]
	False
}
