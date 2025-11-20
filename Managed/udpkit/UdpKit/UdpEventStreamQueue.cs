using System.Reflection;

namespace UdpKit;

[Obfuscation(Exclude = false, Feature = "rename", ApplyToMembers = true)]
internal class UdpEventStreamQueue : UdpEventBase
{
	public UdpConnection Connection;

	public UdpStreamOp StreamOp;

	public override int Type => 27;
}
