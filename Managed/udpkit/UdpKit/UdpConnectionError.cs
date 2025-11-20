namespace UdpKit;

internal enum UdpConnectionError
{
	None,
	SequenceOutOfBounds,
	IncorrectCommand,
	SendWindowFull,
	UnknownStreamChannel,
	InvalidBlockNumber
}
