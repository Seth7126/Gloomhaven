using Google.Protobuf.Reflection;

namespace RedLynx.Api.Banner;

public enum BannerType
{
	[OriginalName("BANNER_TYPE_TIPS")]
	Tips,
	[OriginalName("BANNER_TYPE_PROMO")]
	Promo
}
