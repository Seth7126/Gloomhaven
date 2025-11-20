namespace UdpKit.Platform.Photon;

internal enum ConnectState
{
	Idle,
	Connected,
	Refused,
	CreateRoomPending,
	CreateRoomFailed,
	JoinRoomPending,
	JoinRoomFailed,
	DisconnectPending,
	DirectPending,
	DirectFailed,
	DirectSuccess,
	RelayPending,
	RelayFailed,
	RelaySuccess
}
