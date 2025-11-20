using Google.Protobuf.Reflection;

namespace Hydra.Api.Messaging;

public enum ChannelCredentialPolicy
{
	[OriginalName("CHANNEL_CREDENTIAL_POLICY_UNKNOWN")]
	Unknown,
	[OriginalName("CHANNEL_CREDENTIAL_POLICY_EMPTY")]
	Empty,
	[OriginalName("CHANNEL_CREDENTIAL_POLICY_PASSWORD")]
	Password
}
