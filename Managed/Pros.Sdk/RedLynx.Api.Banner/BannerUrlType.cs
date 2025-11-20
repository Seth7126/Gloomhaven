using Google.Protobuf.Reflection;

namespace RedLynx.Api.Banner;

public enum BannerUrlType
{
	[OriginalName("BANNER_URL_TYPE_EXTERNAL_URL")]
	ExternalUrl,
	[OriginalName("BANNER_URL_TYPE_IN_APP_GAME_CONTENT")]
	InAppGameContent,
	[OriginalName("BANNER_URL_TYPE_IN_APP_EXTERNAL_CONTENT")]
	InAppExternalContent
}
