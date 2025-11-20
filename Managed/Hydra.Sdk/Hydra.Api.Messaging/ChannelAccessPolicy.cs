using Google.Protobuf.Reflection;

namespace Hydra.Api.Messaging;

public enum ChannelAccessPolicy
{
	[OriginalName("CHANNEL_ACCESS_POLICY_UNKNOWN")]
	Unknown,
	[OriginalName("CHANNEL_ACCESS_POLICY_ALLOW")]
	Allow,
	[OriginalName("CHANNEL_ACCESS_POLICY_BLOCK")]
	Block
}
