using Google.Protobuf.Reflection;

namespace Hydra.Api.Infrastructure;

public enum ServiceAccessRole
{
	[OriginalName("UNKNOWN")]
	Unknown,
	[OriginalName("GUEST")]
	Guest,
	[OriginalName("GAME_CLIENT")]
	GameClient,
	[OriginalName("GAME_SERVER")]
	GameServer,
	[OriginalName("WEB_PORTAL")]
	WebPortal,
	[OriginalName("SERVER_MANAGER_AGENT")]
	ServerManagerAgent,
	[OriginalName("STANDALONE_SERVER")]
	StandaloneServer,
	[OriginalName("BOT_MANAGER")]
	BotManager,
	[OriginalName("ADMIN")]
	Admin,
	[OriginalName("PLUGIN")]
	Plugin,
	[OriginalName("PLUGIN_ADMIN")]
	PluginAdmin,
	[OriginalName("CONFIGURATION_MANAGER")]
	ConfigurationManager,
	[OriginalName("DIAGNOSTIC_MANAGER")]
	DiagnosticManager,
	[OriginalName("GAME_NEXUS_API")]
	GameNexusApi,
	[OriginalName("DEDICATED_SERVER_UPLOADER")]
	DedicatedServerUploader,
	[OriginalName("ATHENA")]
	Athena
}
