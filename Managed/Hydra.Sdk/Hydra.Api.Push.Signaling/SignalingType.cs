using Google.Protobuf.Reflection;

namespace Hydra.Api.Push.Signaling;

public enum SignalingType
{
	[OriginalName("SIGNALING_TYPE_SIGNAL_UNKNOWN")]
	SignalUnknown,
	[OriginalName("SIGNALING_TYPE_SIGNAL_P2P")]
	SignalP2P
}
