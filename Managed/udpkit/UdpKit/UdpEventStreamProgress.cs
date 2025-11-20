using System.Reflection;

namespace UdpKit;

[Obfuscation(Exclude = false, Feature = "rename", ApplyToMembers = true)]
internal class UdpEventStreamProgress : UdpEventBase
{
	public UdpConnection Connection;

	public UdpChannelName ChannelName;

	public ulong streamID;

	public float percent;

	public override int Type => 44;
}
