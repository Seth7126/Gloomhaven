using Google.Protobuf.Reflection;

namespace Hydra.Api.Infrastructure.Context;

public enum UserIdentityType
{
	[OriginalName("USER_IDENTITY_TYPE_UNKNOWN")]
	Unknown,
	[OriginalName("USER_IDENTITY_TYPE_PURE_USER_ID")]
	PureUserId,
	[OriginalName("USER_IDENTITY_TYPE_ANONYMOUS_JWE")]
	AnonymousJwe
}
