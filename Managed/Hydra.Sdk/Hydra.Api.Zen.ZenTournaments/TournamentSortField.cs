using Google.Protobuf.Reflection;

namespace Hydra.Api.Zen.ZenTournaments;

public enum TournamentSortField
{
	[OriginalName("TOURNAMENT_SORT_FIELD_NONE")]
	None,
	[OriginalName("TOURNAMENT_SORT_FIELD_PLAYERS_COUNT")]
	PlayersCount,
	[OriginalName("TOURNAMENT_SORT_FIELD_TIME_LEFT")]
	TimeLeft
}
