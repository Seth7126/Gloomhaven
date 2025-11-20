using System.Reflection;

namespace UdpKit;

[Obfuscation(Exclude = false, Feature = "rename", ApplyToMembers = true)]
internal class UdpEventConnectEndPointCancel : UdpEventBase
{
	public UdpEndPoint EndPoint;

	public bool InternalOnly;

	public override int Type => 5;
}
