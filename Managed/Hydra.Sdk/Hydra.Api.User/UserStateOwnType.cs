using Google.Protobuf.Reflection;

namespace Hydra.Api.User;

public enum UserStateOwnType
{
	[OriginalName("USER_STATE_OWN_TYPE_UNKNOWN")]
	Unknown,
	[OriginalName("USER_STATE_OWN_TYPE_DONT_HAVE")]
	DontHave,
	[OriginalName("USER_STATE_OWN_TYPE_BOUGHT")]
	Bought,
	[OriginalName("USER_STATE_OWN_TYPE_RENTED")]
	Rented
}
