namespace UdpKit;

public class UdpEventSessionConnectFailed : UdpEventBase
{
	public byte[] Token;

	public UdpSession Session;

	public UdpSessionError Error;

	public override int Type => 36;
}
