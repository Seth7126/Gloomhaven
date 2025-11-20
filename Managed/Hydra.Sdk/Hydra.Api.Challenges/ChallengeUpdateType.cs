using Google.Protobuf.Reflection;

namespace Hydra.Api.Challenges;

public enum ChallengeUpdateType
{
	[OriginalName("CHALLENGE_UPDATE_TYPE_NONE")]
	None,
	[OriginalName("CHALLENGE_UPDATE_TYPE_FULL")]
	Full,
	[OriginalName("CHALLENGE_UPDATE_TYPE_INCREMENTAL")]
	Incremental,
	[OriginalName("CHALLENGE_UPDATE_TYPE_FILTERED")]
	Filtered
}
