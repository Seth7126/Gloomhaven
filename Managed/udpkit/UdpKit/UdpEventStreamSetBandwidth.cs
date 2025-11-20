using System.Reflection;

namespace UdpKit;

[Obfuscation(Exclude = false, Feature = "rename", ApplyToMembers = true)]
internal class UdpEventStreamSetBandwidth : UdpEventBase
{
	public UdpConnection Connection;

	public int BytesPerSecond;

	public override int Type => 29;
}
