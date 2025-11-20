using Google.Protobuf.Reflection;

namespace Hydra.Api.GooglePlay;

public enum GooglePlayPurchaseType
{
	[OriginalName("GOOGLE_PLAY_PURCHASE_TYPE_TEST")]
	Test = 0,
	[OriginalName("GOOGLE_PLAY_PURCHASE_TYPE_PROMO")]
	Promo = 1,
	[OriginalName("GOOGLE_PLAY_PURCHASE_TYPE_REWARDED")]
	Rewarded = 2,
	[OriginalName("GOOGLE_PLAY_PURCHASE_TYPE_REAL_MONEY")]
	RealMoney = 100,
	[OriginalName("GOOGLE_PLAY_PURCHASE_TYPE_UNKNOWN")]
	Unknown = 200
}
