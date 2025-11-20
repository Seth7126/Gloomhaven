using Google.Protobuf.Reflection;

namespace Hydra.Api.Zen.ZenTournaments;

public enum TournamentSortOrder
{
	[OriginalName("TOURNAMENT_SORT_ORDER_NONE")]
	None,
	[OriginalName("TOURNAMENT_SORT_ORDER_ASCENDING")]
	Ascending,
	[OriginalName("TOURNAMENT_SORT_ORDER_DESCENDING")]
	Descending
}
