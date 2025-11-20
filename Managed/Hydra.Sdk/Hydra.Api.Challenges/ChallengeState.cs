using Google.Protobuf.Reflection;

namespace Hydra.Api.Challenges;

public enum ChallengeState
{
	[OriginalName("CHALLENGE_STATE_NONE")]
	None,
	[OriginalName("CHALLENGE_STATE_ACTIVATED")]
	Activated,
	[OriginalName("CHALLENGE_STATE_COMPLETED")]
	Completed,
	[OriginalName("CHALLENGE_STATE_EXPIRED")]
	Expired,
	[OriginalName("CHALLENGE_STATE_REWARDED")]
	Rewarded
}
