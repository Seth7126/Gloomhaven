using Google.Protobuf.Reflection;

namespace Hydra.Api.Facts;

public enum FactsPackFormat
{
	[OriginalName("FACTS_PACK_FORMAT_UNKNOWN")]
	Unknown,
	[OriginalName("FACTS_PACK_FORMAT_NET_CLIENT_PACK")]
	NetClientPack,
	[OriginalName("FACTS_PACK_FORMAT_PRINTF_RAW")]
	PrintfRaw
}
