using Google.Protobuf.Reflection;

namespace Hydra.Api.Zen.ZenTournaments;

public enum TournamentFilterPasswordProtected
{
	[OriginalName("TOURNAMENT_FILTER_PASSWORD_PROTECTED_NONE")]
	None,
	[OriginalName("TOURNAMENT_FILTER_PASSWORD_PROTECTED_TRUE")]
	True,
	[OriginalName("TOURNAMENT_FILTER_PASSWORD_PROTECTED_FALSE")]
	False
}
