namespace UdpKit.Protocol;

internal class GetHostList : Query
{
	public override bool Resend => true;

	public override bool IsUnique => true;
}
