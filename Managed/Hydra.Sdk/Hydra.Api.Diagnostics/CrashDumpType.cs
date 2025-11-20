using Google.Protobuf.Reflection;

namespace Hydra.Api.Diagnostics;

public enum CrashDumpType
{
	[OriginalName("CRASH_DUMP_TYPE_UNKNOWN")]
	Unknown,
	[OriginalName("CRASH_DUMP_TYPE_GAME_CLIENT")]
	GameClient,
	[OriginalName("CRASH_DUMP_TYPE_GAME_SERVER")]
	GameServer
}
