using System.Reflection;

namespace UdpKit;

[Obfuscation(Exclude = false, Feature = "rename", ApplyToMembers = true)]
internal class UdpEventStreamCreateChannel : UdpEventBase
{
	public UdpChannelConfig ChannelConfig;

	public override int Type => 25;
}
