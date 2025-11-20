using Google.Protobuf.Reflection;

namespace Hydra.Api.Zen.ZenTournaments;

public enum TournamentStateFilter
{
	[OriginalName("TOURNAMENT_STATE_FILTER_NONE")]
	None = 0,
	[OriginalName("TOURNAMENT_STATE_FILTER_SCHEDULED")]
	Scheduled = 1,
	[OriginalName("TOURNAMENT_STATE_FILTER_RUNING")]
	Runing = 2,
	[OriginalName("TOURNAMENT_STATE_FILTER_FINISHED")]
	Finished = 4,
	[OriginalName("TOURNAMENT_STATE_FILTER_ALL")]
	All = 7
}
