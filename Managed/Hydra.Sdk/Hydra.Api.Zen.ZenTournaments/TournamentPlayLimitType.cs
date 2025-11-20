using Google.Protobuf.Reflection;

namespace Hydra.Api.Zen.ZenTournaments;

public enum TournamentPlayLimitType
{
	[OriginalName("TOURNAMENT_PLAY_LIMIT_TYPE_NONE")]
	None,
	[OriginalName("TOURNAMENT_PLAY_LIMIT_TYPE_DAY")]
	Day,
	[OriginalName("TOURNAMENT_PLAY_LIMIT_TYPE_TOTAL")]
	Total
}
