namespace UdpKit.Protocol;

internal class HostInfo : Message
{
	public UdpSession Host;

	protected override void OnSerialize()
	{
		base.OnSerialize();
		Serialize(ref Host);
	}
}
