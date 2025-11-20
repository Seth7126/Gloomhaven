using Google.Protobuf.Reflection;

namespace RedLynx.Api.Entitlement;

public enum EntitlementType
{
	[OriginalName("ENTITLEMENT_TYPE_UNKNOWN")]
	Unknown,
	[OriginalName("ENTITLEMENT_TYPE_DURABLE")]
	Durable,
	[OriginalName("ENTITLEMENT_TYPE_CONSUMABLE")]
	Consumable
}
