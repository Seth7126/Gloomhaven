namespace UdpKit.Protocol;

internal class HostRegister : Query
{
	public UdpSession Host;

	public override bool Resend => true;

	public override bool IsUnique => true;

	protected override void OnSerialize()
	{
		Serialize(ref Host);
	}
}
