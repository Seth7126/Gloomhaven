using Google.Protobuf.Reflection;

namespace Hydra.Api.SessionControl;

public enum AcceptStatus
{
	[OriginalName("ACCEPT_STATUS_PENDING")]
	Pending,
	[OriginalName("ACCEPT_STATUS_ACCEPTED")]
	Accepted,
	[OriginalName("ACCEPT_STATUS_REJECTED")]
	Rejected
}
