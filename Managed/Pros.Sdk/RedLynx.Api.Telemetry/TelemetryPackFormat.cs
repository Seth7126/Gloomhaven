using Google.Protobuf.Reflection;

namespace RedLynx.Api.Telemetry;

public enum TelemetryPackFormat
{
	[OriginalName("TELEMETRY_PACK_FORMAT_UNKNOWN")]
	Unknown,
	[OriginalName("TELEMETRY_PACK_FORMAT_JSON_BASED")]
	JsonBased
}
