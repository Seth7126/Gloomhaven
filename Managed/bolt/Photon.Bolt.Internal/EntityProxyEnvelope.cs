using System;
using System.Collections.Generic;

namespace Photon.Bolt.Internal;

public class EntityProxyEnvelope : BoltObject, IDisposable
{
	public int PacketNumber;

	public ProxyFlags Flags;

	public EntityProxy Proxy = null;

	public List<Priority> Written = new List<Priority>();

	public IProtocolToken ControlTokenLost;

	public IProtocolToken ControlTokenGained;

	public void Dispose()
	{
		Proxy = null;
		Flags = ProxyFlags.ZERO;
		Written.Clear();
		EntityProxyEnvelopePool.Release(this);
	}
}
