using Google.Protobuf.Reflection;

namespace RedLynx.Api.Entitlement;

public enum EntitlementStatus
{
	[OriginalName("ENTITLEMENT_STATUS_UNKNOWN")]
	Unknown,
	[OriginalName("ENTITLEMENT_STATUS_UNCLAIMED")]
	Unclaimed,
	[OriginalName("ENTITLEMENT_STATUS_OWNED")]
	Owned
}
