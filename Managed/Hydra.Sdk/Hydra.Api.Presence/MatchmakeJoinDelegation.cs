using Google.Protobuf.Reflection;

namespace Hydra.Api.Presence;

public enum MatchmakeJoinDelegation
{
	[OriginalName("MATCHMAKE_JOIN_DELEGATION_DISABLED")]
	Disabled,
	[OriginalName("MATCHMAKE_JOIN_DELEGATION_EVERYONE")]
	Everyone,
	[OriginalName("MATCHMAKE_JOIN_DELEGATION_ALLOWED_USERS")]
	AllowedUsers
}
