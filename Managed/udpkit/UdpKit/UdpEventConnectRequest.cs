namespace UdpKit;

public class UdpEventConnectRequest : UdpEventBase
{
	public byte[] Token;

	public UdpEndPoint EndPoint;

	public override int Type => 2;
}
