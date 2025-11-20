using Google.Protobuf.Reflection;

namespace Hydra.Api.Zen.ZenTournaments;

public enum PostTournamentResultType
{
	[OriginalName("POST_TOURNAMENT_RESULT_TYPE_NONE")]
	None,
	[OriginalName("POST_TOURNAMENT_RESULT_TYPE_LIMIT_REACHED")]
	LimitReached,
	[OriginalName("POST_TOURNAMENT_RESULT_TYPE_PARTICIPANTS_LIMIT_REACHED")]
	ParticipantsLimitReached,
	[OriginalName("POST_TOURNAMENT_RESULT_TYPE_UPDATED")]
	Updated,
	[OriginalName("POST_TOURNAMENT_RESULT_TYPE_OK")]
	Ok
}
