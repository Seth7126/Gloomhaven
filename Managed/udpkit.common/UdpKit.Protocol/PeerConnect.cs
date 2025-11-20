namespace UdpKit.Protocol;

internal class PeerConnect : Query<PeerConnectResult>
{
	public override bool Resend => true;

	public override bool IsUnique => true;
}
