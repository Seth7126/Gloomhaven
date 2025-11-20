namespace UdpKit;

public class UdpEventSessionConnected : UdpEventBase
{
	public byte[] Token;

	public UdpSession Session;

	public override int Type => 40;
}
