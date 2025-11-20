using Google.Protobuf.Reflection;

namespace Hydra.Api.Auth;

public enum Platform
{
	[OriginalName("PLATFORM_UNKNOWN")]
	Unknown,
	[OriginalName("PLATFORM_WIN64")]
	Win64,
	[OriginalName("PLATFORM_LINUX64")]
	Linux64,
	[OriginalName("PLATFORM_DURANGO")]
	Durango,
	[OriginalName("PLATFORM_ORBIS")]
	Orbis,
	[OriginalName("PLATFORM_SWITCH")]
	Switch,
	[OriginalName("PLATFORM_IOS")]
	Ios,
	[OriginalName("PLATFORM_ANDROID")]
	Android,
	[OriginalName("PLATFORM_PROSPERO")]
	Prospero,
	[OriginalName("PLATFORM_SCARLETT")]
	Scarlett,
	[OriginalName("PLATFORM_STADIA")]
	Stadia,
	[OriginalName("PLATFORM_UNITY")]
	Unity,
	[OriginalName("PLATFORM_NET")]
	Net
}
