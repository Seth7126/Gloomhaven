using Google.Protobuf.Reflection;

namespace Hydra.Api.Zen.ZenTournaments;

public enum TournamentFilterState
{
	[OriginalName("TOURNAMENT_FILTER_STATE_NONE")]
	None,
	[OriginalName("TOURNAMENT_FILTER_STATE_ACTIVE")]
	Active,
	[OriginalName("TOURNAMENT_FILTER_STATE_FINISHED")]
	Finished
}
