using Google.Protobuf.Reflection;

namespace Hydra.Api.Messaging;

public enum ChannelType
{
	[OriginalName("CHANNEL_TYPE_NONE")]
	None,
	[OriginalName("CHANNEL_TYPE_PRIVATE")]
	Private,
	[OriginalName("CHANNEL_TYPE_SERVICE")]
	Service,
	[OriginalName("CHANNEL_TYPE_USER")]
	User
}
