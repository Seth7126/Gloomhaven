using Google.Protobuf.Reflection;

namespace Hydra.Api.Challenges;

public enum ChallengeCounterFilterOperationType
{
	[OriginalName("CHALLENGE_COUNTER_FILTER_OPERATION_TYPE_NONE")]
	None,
	[OriginalName("CHALLENGE_COUNTER_FILTER_OPERATION_TYPE_EQUAL")]
	Equal,
	[OriginalName("CHALLENGE_COUNTER_FILTER_OPERATION_TYPE_NOT_EQUAL")]
	NotEqual
}
