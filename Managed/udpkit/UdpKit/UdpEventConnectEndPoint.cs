using System.Reflection;

namespace UdpKit;

[Obfuscation(Exclude = false, Feature = "rename", ApplyToMembers = true)]
internal class UdpEventConnectEndPoint : UdpEventBase
{
	public byte[] Token;

	public UdpEndPoint EndPoint;

	public override int Type => 3;
}
