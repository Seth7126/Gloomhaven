using Google.Protobuf.Reflection;

namespace Hydra.Api.Presence;

public enum PartyJoinDelegation
{
	[OriginalName("PARTY_JOIN_DELEGATION_DISABLED")]
	Disabled,
	[OriginalName("PARTY_JOIN_DELEGATION_EVERYONE")]
	Everyone,
	[OriginalName("PARTY_JOIN_DELEGATION_ALLOWED_USERS")]
	AllowedUsers
}
