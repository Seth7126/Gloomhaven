using System.Reflection;

namespace UdpKit;

[Obfuscation(Exclude = false, Feature = "rename", ApplyToMembers = true)]
internal class UdpEventDisconnect : UdpEventBase
{
	public byte[] Token;

	public UdpConnection Connection;

	public UdpConnectionDisconnectReason DisconnectReason;

	public override int Type => 11;
}
