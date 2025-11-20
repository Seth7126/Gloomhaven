using Google.Protobuf.Reflection;

namespace Hydra.Api.Messaging;

public enum ChannelCredentialType
{
	[OriginalName("CHANNEL_CREDENTIAL_TYPE_UNKNOWN")]
	Unknown,
	[OriginalName("CHANNEL_CREDENTIAL_TYPE_PASSWORD")]
	Password
}
