using Google.Protobuf.Reflection;

namespace Hydra.Api.EndpointDispatcher;

public enum EnvironmentStatus
{
	[OriginalName("ENVIRONMENT_STATUS_UNKNOWN")]
	Unknown,
	[OriginalName("ENVIRONMENT_STATUS_READY")]
	Ready,
	[OriginalName("ENVIRONMENT_STATUS_MAINTENANCE")]
	Maintenance
}
