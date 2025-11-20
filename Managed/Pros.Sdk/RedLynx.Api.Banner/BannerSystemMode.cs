using Google.Protobuf.Reflection;

namespace RedLynx.Api.Banner;

public enum BannerSystemMode
{
	[OriginalName("BANNER_SYSTEM_MODE_NORMAL")]
	Normal,
	[OriginalName("BANNER_SYSTEM_MODE_TURN_OFF")]
	TurnOff
}
