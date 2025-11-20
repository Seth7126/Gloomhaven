using Google.Protobuf.Reflection;

namespace Hydra.Api.Challenges;

public enum ChallengeOperationType
{
	[OriginalName("CHALLENGE_OPERTAION_TYPE_NONE")]
	ChallengeOpertaionTypeNone,
	[OriginalName("CHALLENGE_OPERTAION_TYPE_INCREASE")]
	ChallengeOpertaionTypeIncrease,
	[OriginalName("CHALLENGE_OPERTAION_TYPE_RESET")]
	ChallengeOpertaionTypeReset
}
