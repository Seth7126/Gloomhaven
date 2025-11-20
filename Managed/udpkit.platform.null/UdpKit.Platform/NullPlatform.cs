using System.Collections.Generic;
using UdpKit.Utils;

namespace UdpKit.Platform;

public sealed class NullPlatform : UdpPlatform
{
	internal override bool IsNull => true;

	public NullPlatform()
	{
		GetPrecisionTime();
	}

	internal override UdpPlatformSocket CreateSocket(bool ipv6)
	{
		return new NullSocket(this);
	}

	internal override UdpSessionSource GetSessionSource()
	{
		return UdpSessionSource.None;
	}

	internal override List<UdpPlatformInterface> GetNetworkInterfaces()
	{
		return new List<UdpPlatformInterface>();
	}

	internal override uint GetPrecisionTime()
	{
		return PrecisionTimer.GetCurrentTime();
	}
}
