using Google.Protobuf.Reflection;

namespace Hydra.Api.User;

public enum UserStateOpType
{
	[OriginalName("USER_STATE_OP_TYPE_UNKNOWN")]
	Unknown,
	[OriginalName("USER_STATE_OP_TYPE_ADD")]
	Add,
	[OriginalName("USER_STATE_OP_TYPE_REMOVE")]
	Remove,
	[OriginalName("USER_STATE_OP_TYPE_RENT")]
	Rent,
	[OriginalName("USER_STATE_OP_TYPE_SET")]
	Set,
	[OriginalName("USER_STATE_OP_TYPE_SUB")]
	Sub,
	[OriginalName("USER_STATE_OP_TYPE_REQUIRE")]
	Require,
	[OriginalName("USER_STATE_OP_TYPE_REQUIRELESS")]
	Requireless
}
