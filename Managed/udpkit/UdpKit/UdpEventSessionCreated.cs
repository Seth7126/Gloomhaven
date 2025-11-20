namespace UdpKit;

internal class UdpEventSessionCreated : UdpEventBase
{
	public UdpSession Session;

	public bool Success;

	public UdpSessionError Error;

	public override int Type => 38;
}
