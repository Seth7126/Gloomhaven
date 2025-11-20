using System.Reflection;

namespace UdpKit;

[Obfuscation(Exclude = false, Feature = "rename", ApplyToMembers = true)]
internal class UdpEventLanBroadcastDisable : UdpEventBase
{
	public override int Type => 19;
}
