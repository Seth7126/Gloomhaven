using System;

namespace UdpKit;

public class UdpEventSessionListUpdated : UdpEventBase
{
	public Map<Guid, UdpSession> SessionList;

	public override int Type => 30;
}
