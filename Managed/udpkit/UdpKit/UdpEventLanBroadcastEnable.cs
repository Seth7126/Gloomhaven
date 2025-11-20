using System.Reflection;

namespace UdpKit;

[Obfuscation(Exclude = false, Feature = "rename", ApplyToMembers = true)]
internal class UdpEventLanBroadcastEnable : UdpEventBase
{
	public ushort Port;

	public UdpIPv4Address LocalAddress;

	public UdpIPv4Address BroadcastAddress;

	public override int Type => 17;
}
