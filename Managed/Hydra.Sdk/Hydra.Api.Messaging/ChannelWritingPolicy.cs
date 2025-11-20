using Google.Protobuf.Reflection;

namespace Hydra.Api.Messaging;

public enum ChannelWritingPolicy
{
	[OriginalName("CHANNEL_WRITING_POLICY_UNKNOWN")]
	Unknown,
	[OriginalName("CHANNEL_WRITING_POLICY_WRITE")]
	Write,
	[OriginalName("CHANNEL_WRITING_POLICY_READ")]
	Read
}
