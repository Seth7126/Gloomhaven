using Google.Protobuf.Reflection;

namespace Hydra.Api.Telemetry;

public enum TelemetryPackCompression
{
	[OriginalName("TELEMETRY_PACK_COMPRESSION_NONE")]
	None,
	[OriginalName("TELEMETRY_PACK_COMPRESSION_ZLIB")]
	Zlib,
	[OriginalName("TELEMETRY_PACK_COMPRESSION_GZIP")]
	Gzip
}
