using Google.Protobuf.Reflection;

namespace Hydra.Api.Rating;

public enum MatchResult
{
	[OriginalName("MATCH_RESULT_NONE")]
	None,
	[OriginalName("MATCH_RESULT_WIN")]
	Win,
	[OriginalName("MATCH_RESULT_LOSE")]
	Lose
}
