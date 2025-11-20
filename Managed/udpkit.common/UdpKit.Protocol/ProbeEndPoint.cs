namespace UdpKit.Protocol;

internal class ProbeEndPoint : Query<ProbeEndPointResult>
{
	public override bool Resend => true;

	public override bool IsUnique => true;
}
