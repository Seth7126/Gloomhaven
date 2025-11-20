using Google.Protobuf.Reflection;

namespace Hydra.Api.Presence;

public enum PartyInviteDelegation
{
	[OriginalName("PARTY_INVITE_DELEGATION_OWNER")]
	Owner,
	[OriginalName("PARTY_INVITE_DELEGATION_EVERYONE")]
	Everyone
}
