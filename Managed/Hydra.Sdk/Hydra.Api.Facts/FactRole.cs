using Google.Protobuf.Reflection;

namespace Hydra.Api.Facts;

public enum FactRole
{
	[OriginalName("FACT_ROLE_GAME_CLIENT")]
	GameClient,
	[OriginalName("FACT_ROLE_GAME_SERVER")]
	GameServer
}
