using Google.Protobuf.Reflection;

namespace RedLynx.Api.Banner;

public enum BannerSeverity
{
	[OriginalName("BANNER_SEVERITY_NONE")]
	None,
	[OriginalName("BANNER_SEVERITY_IMPORTANT")]
	Important
}
