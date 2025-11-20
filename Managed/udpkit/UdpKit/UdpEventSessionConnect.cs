using System.Reflection;

namespace UdpKit;

[Obfuscation(Exclude = false, Feature = "rename", ApplyToMembers = true)]
internal class UdpEventSessionConnect : UdpEventBase
{
	public byte[] Token;

	public UdpSession Session;

	public override int Type => 31;
}
