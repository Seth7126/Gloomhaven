namespace UdpKit.Protocol;

internal class ProbeUnsolicited : Message
{
	public UdpEndPoint WanEndPoint;

	protected override void OnSerialize()
	{
		base.OnSerialize();
		Serialize(ref WanEndPoint);
	}
}
