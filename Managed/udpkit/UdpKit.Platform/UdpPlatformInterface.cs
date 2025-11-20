namespace UdpKit.Platform;

public abstract class UdpPlatformInterface
{
	internal object Token { get; set; }

	internal abstract string Name { get; }

	internal abstract byte[] PhysicalAddress { get; }

	internal abstract UdpLinkType LinkType { get; }

	internal abstract UdpIPv4Address[] UnicastAddresses { get; }

	internal abstract UdpIPv4Address[] MulticastAddresses { get; }

	internal abstract UdpIPv4Address[] GatewayAddresses { get; }
}
