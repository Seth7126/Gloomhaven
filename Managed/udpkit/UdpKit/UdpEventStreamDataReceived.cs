using System.Reflection;

namespace UdpKit;

[Obfuscation(Exclude = false, Feature = "rename", ApplyToMembers = true)]
internal class UdpEventStreamDataReceived : UdpEventBase
{
	public UdpConnection Connection;

	public UdpStreamData StreamData;

	public override int Type => 20;
}
