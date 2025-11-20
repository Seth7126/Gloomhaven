using Google.Protobuf.Reflection;

namespace Hydra.Api.Push;

public enum PushClientMessageType
{
	[OriginalName("PUSH_CLIENT_MESSAGE_TYPE_UNKNOWN")]
	Unknown,
	[OriginalName("PUSH_CLIENT_MESSAGE_TYPE_SIGNALING")]
	Signaling
}
