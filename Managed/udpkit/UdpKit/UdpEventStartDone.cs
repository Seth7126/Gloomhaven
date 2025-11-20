using System.Threading;

namespace UdpKit;

public class UdpEventStartDone : UdpEventBase
{
	public UdpEndPoint EndPoint;

	public ManualResetEvent ResetEvent;

	public override int Type => 22;
}
