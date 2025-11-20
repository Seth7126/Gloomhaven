using System.Reflection;
using System.Threading;

namespace UdpKit;

[Obfuscation(Exclude = false, Feature = "rename", ApplyToMembers = true)]
internal class UdpEventClose : UdpEventBase
{
	public ManualResetEvent ResetEvent;

	public override int Type => 13;
}
