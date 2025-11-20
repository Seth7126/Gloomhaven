using Google.Protobuf.Reflection;

namespace Hydra.Api.ServerManagerScheduler;

public enum SessionRejectReason
{
	[OriginalName("SESSION_REJECT_REASON_UNKNOWN")]
	Unknown,
	[OriginalName("SESSION_REJECT_REASON_NO_VERSION")]
	NoVersion,
	[OriginalName("SESSION_REJECT_REASON_NOT_ENOUGH_RESOURCES")]
	NotEnoughResources
}
