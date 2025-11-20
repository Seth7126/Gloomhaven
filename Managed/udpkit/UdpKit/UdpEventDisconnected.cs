namespace UdpKit;

public class UdpEventDisconnected : UdpEventBase
{
	public UdpConnection Connection;

	public override int Type => 12;
}
