namespace UdpKit.Platform.Photon;

internal struct ConnectionControl
{
	public bool ConnectionLocal;

	public uint ConnectionAttempts;

	public uint ConnectionTrials;

	public uint ConnectionThreshold;

	public UdpEndPoint LanEndPoint;

	public UdpEndPoint WanEndPoint;
}
