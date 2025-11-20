namespace UdpKit;

internal class NatFeatures
{
	public UdpEndPoint WanEndPoint;

	public UdpEndPoint LanEndPoint;

	public NatFeatureStates AllowsUnsolicitedTraffic;

	public NatFeatureStates SupportsHairpinTranslation;

	public NatFeatureStates SupportsEndPointPreservation;

	public NatFeatures Clone()
	{
		return (NatFeatures)MemberwiseClone();
	}

	public override string ToString()
	{
		return $"[NatFeatures Lan={LanEndPoint} Wan={WanEndPoint} AllowsUnsolicitedTraffic={AllowsUnsolicitedTraffic}, HairpinTranslation={SupportsHairpinTranslation}, EndPointPreservation={SupportsEndPointPreservation}";
	}
}
