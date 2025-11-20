using Google.Protobuf.Reflection;

namespace Hydra.Api.User;

public enum UserStateDataType
{
	[OriginalName("USER_STATE_DATA_TYPE_UNKNOWN")]
	Unknown,
	[OriginalName("USER_STATE_DATA_TYPE_NUMERICAL")]
	Numerical,
	[OriginalName("USER_STATE_DATA_TYPE_TIME_BASED")]
	TimeBased,
	[OriginalName("USER_STATE_DATA_TYPE_STRING")]
	String,
	[OriginalName("USER_STATE_DATA_TYPE_VECTOR_STRING")]
	VectorString
}
