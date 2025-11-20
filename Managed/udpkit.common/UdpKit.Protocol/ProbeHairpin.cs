namespace UdpKit.Protocol;

internal class ProbeHairpin : Query
{
	public override bool IsUnique => true;

	public override bool Resend => true;
}
