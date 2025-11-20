namespace UdpKit;

public enum UdpConnectionDisconnectReason
{
	Unknown,
	Timeout,
	Error,
	Disconnected,
	Authentication,
	MaxCCUReached,
	CloudTimeout
}
