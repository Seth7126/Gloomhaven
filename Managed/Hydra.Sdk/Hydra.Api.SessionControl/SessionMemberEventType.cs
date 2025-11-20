using Google.Protobuf.Reflection;

namespace Hydra.Api.SessionControl;

public enum SessionMemberEventType
{
	[OriginalName("SESSION_MEMBER_EVENT_TYPE_NONE")]
	None,
	[OriginalName("SESSION_MEMBER_EVENT_TYPE_ADD")]
	Add,
	[OriginalName("SESSION_MEMBER_EVENT_TYPE_REMOVE")]
	Remove,
	[OriginalName("SESSION_MEMBER_EVENT_TYPE_AWAITING")]
	Awaiting,
	[OriginalName("SESSION_MEMBER_EVENT_TYPE_RETURN")]
	Return
}
