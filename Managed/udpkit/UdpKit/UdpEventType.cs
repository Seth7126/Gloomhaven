namespace UdpKit;

public enum UdpEventType
{
	SocketStartupDone = 22,
	SocketStartupFailed = 24,
	ConnectRequest = 2,
	ConnectFailed = 4,
	ConnectRefused = 6,
	ConnectAttempt = 10,
	Connected = 8,
	Disconnected = 12,
	PacketLost = 14,
	PacketReceived = 16,
	PacketDelivered = 18,
	StreamDataStarted = 42,
	StreamDataReceived = 20,
	StreamDataProgress = 44,
	StreamDataAbort = 46,
	SessionListUpdated = 30,
	SessionConnectFailed = 36,
	SessionConnected = 40,
	SessionCreated = 38,
	MasterServerConnectFailed = 34,
	MasterServerConnected = 26,
	MasterServerDisconnected = 28,
	MasterServerNatProbeResult = 32
}
