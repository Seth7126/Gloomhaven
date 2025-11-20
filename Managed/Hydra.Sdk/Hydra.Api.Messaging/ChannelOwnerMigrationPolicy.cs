using Google.Protobuf.Reflection;

namespace Hydra.Api.Messaging;

public enum ChannelOwnerMigrationPolicy
{
	[OriginalName("CHANNEL_OWNER_MIGRATION_POLICY_UNKNOWN")]
	Unknown,
	[OriginalName("CHANNEL_OWNER_MIGRATION_POLICY_NONE")]
	None,
	[OriginalName("CHANNEL_OWNER_MIGRATION_POLICY_AUTOMATIC")]
	Automatic,
	[OriginalName("CHANNEL_OWNER_MIGRATION_POLICY_SERVER")]
	Server
}
