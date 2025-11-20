using Google.Protobuf.Reflection;

namespace Hydra.Api.SessionControl;

public enum SessionFinishReason
{
	[OriginalName("SESSION_FINISH_REASON_NONE")]
	None,
	[OriginalName("SESSION_FINISH_REASON_NORMAL")]
	Normal,
	[OriginalName("SESSION_FINISH_REASON_NO_MATCHING_DSM")]
	NoMatchingDsm,
	[OriginalName("SESSION_FINISH_REASON_TIMEOUT_ACTIVATE")]
	TimeoutActivate,
	[OriginalName("SESSION_FINISH_REASON_TIMEOUT_DS")]
	TimeoutDs,
	[OriginalName("SESSION_FINISH_REASON_REJECTED")]
	Rejected,
	[OriginalName("SESSION_FINISH_REASON_NO_SLOTS_DSM")]
	NoSlotsDsm,
	[OriginalName("SESSION_FINISH_REASON_NO_PROVIDER")]
	NoProvider,
	[OriginalName("SESSION_FINISH_REASON_TIMEOUT_QUEUE")]
	TimeoutQueue
}
