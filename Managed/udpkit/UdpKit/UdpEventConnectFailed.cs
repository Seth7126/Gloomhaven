namespace UdpKit;

public class UdpEventConnectFailed : UdpEventBase
{
	public byte[] Token;

	public UdpEndPoint EndPoint;

	public override int Type => 4;
}
