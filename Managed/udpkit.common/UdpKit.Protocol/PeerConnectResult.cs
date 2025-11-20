namespace UdpKit.Protocol;

internal class PeerConnectResult : Result
{
	public UdpEndPoint Probe0;

	public UdpEndPoint Probe1;

	public UdpEndPoint Probe2;

	protected override void OnSerialize()
	{
		base.OnSerialize();
		Serialize(ref Probe0);
		Serialize(ref Probe1);
		Serialize(ref Probe2);
	}
}
