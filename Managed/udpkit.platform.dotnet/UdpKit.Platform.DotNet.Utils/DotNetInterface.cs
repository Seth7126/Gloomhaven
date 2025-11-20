using System.Net.NetworkInformation;
using System.Reflection;

namespace UdpKit.Platform.DotNet.Utils;

[Obfuscation(Exclude = false, Feature = "rename", ApplyToMembers = true)]
internal class DotNetInterface : UdpPlatformInterface
{
	private readonly string name;

	private readonly UdpLinkType linkType;

	private readonly byte[] physicalAddress;

	private readonly UdpIPv4Address[] gatewayAddresses;

	private readonly UdpIPv4Address[] unicastAddresses;

	private readonly UdpIPv4Address[] multicastAddresses;

	internal override string Name => name;

	internal override UdpLinkType LinkType => linkType;

	internal override byte[] PhysicalAddress => physicalAddress;

	internal override UdpIPv4Address[] GatewayAddresses => gatewayAddresses;

	internal override UdpIPv4Address[] UnicastAddresses => unicastAddresses;

	internal override UdpIPv4Address[] MulticastAddresses => multicastAddresses;

	public DotNetInterface(NetworkInterface n, UdpIPv4Address[] gw, UdpIPv4Address[] uni, UdpIPv4Address[] multi)
	{
		name = ParseName(n);
		linkType = ParseLinkType(n);
		physicalAddress = ParsePhysicalAddress(n);
		gatewayAddresses = gw;
		unicastAddresses = uni;
		multicastAddresses = multi;
	}

	private static string ParseName(NetworkInterface n)
	{
		try
		{
			return n.Description;
		}
		catch
		{
			return "UNKNOWN";
		}
	}

	private static byte[] ParsePhysicalAddress(NetworkInterface n)
	{
		try
		{
			return n.GetPhysicalAddress().GetAddressBytes();
		}
		catch
		{
			return new byte[0];
		}
	}

	private static UdpLinkType ParseLinkType(NetworkInterface n)
	{
		switch (n.NetworkInterfaceType)
		{
		case NetworkInterfaceType.Ethernet:
		case NetworkInterfaceType.Ethernet3Megabit:
		case NetworkInterfaceType.FastEthernetT:
		case NetworkInterfaceType.FastEthernetFx:
		case NetworkInterfaceType.GigabitEthernet:
			return UdpLinkType.Ethernet;
		case NetworkInterfaceType.Wireless80211:
			return UdpLinkType.Wifi;
		default:
			return UdpLinkType.Unknown;
		}
	}
}
