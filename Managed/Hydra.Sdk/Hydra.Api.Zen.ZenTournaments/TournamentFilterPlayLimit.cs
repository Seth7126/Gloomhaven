using Google.Protobuf.Reflection;

namespace Hydra.Api.Zen.ZenTournaments;

public enum TournamentFilterPlayLimit
{
	[OriginalName("TOURNAMENT_FILTER_PLAY_LIMIT_NONE")]
	None,
	[OriginalName("TOURNAMENT_FILTER_PLAY_LIMIT_UNLIMITED")]
	Unlimited,
	[OriginalName("TOURNAMENT_FILTER_PLAY_LIMIT_LIMITED")]
	Limited
}
