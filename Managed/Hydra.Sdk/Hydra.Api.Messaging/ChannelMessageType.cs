using Google.Protobuf.Reflection;

namespace Hydra.Api.Messaging;

public enum ChannelMessageType
{
	[OriginalName("CHANNEL_MESSAGE_TYPE_NONE")]
	None = 0,
	[OriginalName("CHANNEL_MESSAGE_TYPE_PRIVATE")]
	Private = 100,
	[OriginalName("CHANNEL_MESSAGE_TYPE_CHANNEL")]
	Channel = 200
}
