namespace UdpKit;

public class UdpEventConnectAttempt : UdpEventBase
{
	public byte[] Token;

	public UdpEndPoint EndPoint;

	public override int Type => 10;
}
