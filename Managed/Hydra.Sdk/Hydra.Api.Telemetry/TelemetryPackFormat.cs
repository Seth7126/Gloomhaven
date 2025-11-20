using Google.Protobuf.Reflection;

namespace Hydra.Api.Telemetry;

public enum TelemetryPackFormat
{
	[OriginalName("TELEMETRY_PACK_FORMAT_UNKNOWN")]
	Unknown,
	[OriginalName("TELEMETRY_PACK_FORMAT_JSON_BASED")]
	JsonBased
}
