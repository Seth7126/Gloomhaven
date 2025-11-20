using Google.Protobuf.Reflection;

namespace Hydra.Api.Presence;

public enum GameSessionIdChangeReason
{
	[OriginalName("GAME_SESSION_ID_CHANGE_REASON_NONE")]
	None,
	[OriginalName("GAME_SESSION_ID_CHANGE_REASON_JOIN")]
	Join,
	[OriginalName("GAME_SESSION_ID_CHANGE_REASON_LEAVE")]
	Leave,
	[OriginalName("GAME_SESSION_ID_CHANGE_REASON_KICK")]
	Kick,
	[OriginalName("GAME_SESSION_ID_CHANGE_REASON_DISBAND")]
	Disband,
	[OriginalName("GAME_SESSION_ID_CHANGE_REASON_SESSION_FINISHED_NORMAL")]
	SessionFinishedNormal,
	[OriginalName("GAME_SESSION_ID_CHANGE_REASON_SESSION_FINISHED_NO_MATCHING_DSM")]
	SessionFinishedNoMatchingDsm,
	[OriginalName("GAME_SESSION_ID_CHANGE_REASON_SESSION_FINISHED_PENDING_TIMEOUT")]
	SessionFinishedPendingTimeout,
	[OriginalName("GAME_SESSION_ID_CHANGE_REASON_SESSION_FINISHED_NO_SLOTS_DSM")]
	SessionFinishedNoSlotsDsm,
	[OriginalName("GAME_SESSION_ID_CHANGE_REASON_SESSION_FINISHED_NO_PROVIDER")]
	SessionFinishedNoProvider,
	[OriginalName("GAME_SESSION_ID_CHANGE_REASON_SESSION_FINISHED_TIMEOUT_QUEUE")]
	SessionFinishedTimeoutQueue
}
