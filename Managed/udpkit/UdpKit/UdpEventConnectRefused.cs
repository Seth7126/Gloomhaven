namespace UdpKit;

public class UdpEventConnectRefused : UdpEventBase
{
	public byte[] Token;

	public UdpEndPoint EndPoint;

	public override int Type => 6;
}
