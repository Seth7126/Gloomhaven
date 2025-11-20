using Google.Protobuf.Reflection;

namespace Hydra.Api.Diagnostics;

public enum DiagnosticsDataType
{
	[OriginalName("DIAGNOSTICS_DATA_TYPE_BINARY")]
	Binary,
	[OriginalName("DIAGNOSTICS_DATA_TYPE_TEXT")]
	Text
}
