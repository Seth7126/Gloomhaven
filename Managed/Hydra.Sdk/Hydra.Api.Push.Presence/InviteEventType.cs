using Google.Protobuf.Reflection;

namespace Hydra.Api.Push.Presence;

public enum InviteEventType
{
	[OriginalName("INVITE_EVENT_TYPE_UNKNOWN")]
	Unknown,
	[OriginalName("INVITE_EVENT_TYPE_INVITE_SENT")]
	InviteSent,
	[OriginalName("INVITE_EVENT_TYPE_REVOKE_SENT")]
	RevokeSent,
	[OriginalName("INVITE_EVENT_TYPE_INVITE_RECEIVED")]
	InviteReceived,
	[OriginalName("INVITE_EVENT_TYPE_REVOKE_RECEIVED")]
	RevokeReceived,
	[OriginalName("INVITE_EVENT_TYPE_INVITE_ACCEPTED")]
	InviteAccepted,
	[OriginalName("INVITE_EVENT_TYPE_INVITE_REJECTED")]
	InviteRejected,
	[OriginalName("INVITE_EVENT_TYPE_ACCEPT_SUCCESS_RECEIVED")]
	AcceptSuccessReceived,
	[OriginalName("INVITE_EVENT_TYPE_ACCEPT_FAIL_RECEIVED")]
	AcceptFailReceived,
	[OriginalName("INVITE_EVENT_TYPE_REJECT_RECEIVED")]
	RejectReceived
}
