using Google.Protobuf.Reflection;

namespace Hydra.Api.Messaging;

public enum ChannelMembershipPolicy
{
	[OriginalName("CHANNEL_MEMBERSHIP_POLICY_UNKNOWN")]
	Unknown,
	[OriginalName("CHANNEL_MEMBERSHIP_POLICY_LOCK")]
	Lock,
	[OriginalName("CHANNEL_MEMBERSHIP_POLICY_UNLOCK")]
	Unlock
}
