namespace UdpKit.Protocol;

internal class ProbeFeatures : Query
{
	public NatFeatures NatFeatures;

	public override bool IsUnique => true;

	public override bool Resend => true;

	protected override void OnSerialize()
	{
		base.OnSerialize();
		Serialize(ref NatFeatures);
	}
}
