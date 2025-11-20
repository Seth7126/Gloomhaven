using System.Reflection;
using System.Threading;

namespace UdpKit;

[Obfuscation(Exclude = false, Feature = "rename", ApplyToMembers = true)]
internal class UdpEventStart : UdpEventBase
{
	public UdpSocketMode Mode;

	public UdpEndPoint EndPoint;

	public ManualResetEvent ResetEvent;

	public override int Type => 1;
}
