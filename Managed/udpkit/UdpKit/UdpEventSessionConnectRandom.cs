using System.Reflection;

namespace UdpKit;

[Obfuscation(Exclude = false, Feature = "rename", ApplyToMembers = true)]
internal class UdpEventSessionConnectRandom : UdpEventBase
{
	public byte[] Token;

	public UdpSessionFilter SessionFilter;

	public override int Type => 45;
}
