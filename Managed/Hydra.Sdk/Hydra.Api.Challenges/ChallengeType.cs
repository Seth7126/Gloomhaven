using Google.Protobuf.Reflection;

namespace Hydra.Api.Challenges;

public enum ChallengeType
{
	[OriginalName("CHALLENGE_TYPE_NONE")]
	None,
	[OriginalName("CHALLENGE_TYPE_DAILY")]
	Daily,
	[OriginalName("CHALLENGE_TYPE_STATIC")]
	Static
}
