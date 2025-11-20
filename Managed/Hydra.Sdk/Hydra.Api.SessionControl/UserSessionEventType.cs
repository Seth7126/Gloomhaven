using Google.Protobuf.Reflection;

namespace Hydra.Api.SessionControl;

public enum UserSessionEventType
{
	[OriginalName("USER_SESSION_EVENT_TYPE_UNKNOWN")]
	Unknown,
	[OriginalName("USER_SESSION_EVENT_TYPE_KR_PARTY_OWNER")]
	KrPartyOwner,
	[OriginalName("USER_SESSION_EVENT_TYPE_KR_REJECTED")]
	KrRejected,
	[OriginalName("USER_SESSION_EVENT_TYPE_KR_TIMEOUT")]
	KrTimeout,
	[OriginalName("USER_SESSION_EVENT_TYPE_KR_CHEAT")]
	KrCheat,
	[OriginalName("USER_SESSION_EVENT_TYPE_KR_AFK")]
	KrAfk,
	[OriginalName("USER_SESSION_EVENT_TYPE_KR_RETARD")]
	KrRetard,
	[OriginalName("USER_SESSION_EVENT_TYPE_KR_READY_TIMEOUT")]
	KrReadyTimeout,
	[OriginalName("USER_SESSION_EVENT_TYPE_DISBAND")]
	Disband,
	[OriginalName("USER_SESSION_EVENT_TYPE_SC_NO_MATCHING_DSM")]
	ScNoMatchingDsm,
	[OriginalName("USER_SESSION_EVENT_TYPE_SC_PENDING_TIMEOUT")]
	ScPendingTimeout,
	[OriginalName("USER_SESSION_EVENT_TYPE_SC_UNKNOWN")]
	ScUnknown,
	[OriginalName("USER_SESSION_EVENT_TYPE_SC_NO_SLOTS_DSM")]
	ScNoSlotsDsm,
	[OriginalName("USER_SESSION_EVENT_TYPE_SC_NO_PROVIDER")]
	ScNoProvider,
	[OriginalName("USER_SESSION_EVENT_TYPE_SC_TIMEOUT_QUEUE")]
	ScTimeoutQueue,
	[OriginalName("USER_SESSION_EVENT_TYPE_REMATCH_DISBAND")]
	RematchDisband,
	[OriginalName("USER_SESSION_EVENT_TYPE_SESSION_JOIN_FAILED")]
	SessionJoinFailed
}
