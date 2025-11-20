using System.Threading;

namespace UdpKit;

public class UdpEventStartFailed : UdpEventBase
{
	public ManualResetEvent ResetEvent;

	public UdpConnectionDisconnectReason disconnectReason;

	public override int Type => 24;
}
