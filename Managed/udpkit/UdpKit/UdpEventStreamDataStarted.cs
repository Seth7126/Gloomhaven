using System.Reflection;

namespace UdpKit;

[Obfuscation(Exclude = false, Feature = "rename", ApplyToMembers = true)]
internal class UdpEventStreamDataStarted : UdpEventBase
{
	public UdpConnection Connection;

	public UdpChannelName ChannelName;

	public ulong streamID;

	public override int Type => 42;
}
