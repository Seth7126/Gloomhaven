using Google.Protobuf.Reflection;

namespace RedLynx.Api.Account;

public enum LinkStatus
{
	[OriginalName("LINK_STATUS_UNKNOWN")]
	Unknown,
	[OriginalName("LINK_STATUS_NOT_LINKED")]
	NotLinked,
	[OriginalName("LINK_STATUS_LINKED")]
	Linked,
	[OriginalName("LINK_STATUS_CODE_TIMEOUT")]
	CodeTimeout,
	[OriginalName("LINK_STATUS_CODE_READ")]
	CodeRead
}
