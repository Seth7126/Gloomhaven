using System.Reflection;

namespace UdpKit;

[Obfuscation(Exclude = false, Feature = "rename", ApplyToMembers = true)]
internal class UdpEventAcceptConnect : UdpEventBase
{
	public byte[] Token;

	public object UserObject;

	public UdpEndPoint EndPoint;

	public override int Type => 7;
}
