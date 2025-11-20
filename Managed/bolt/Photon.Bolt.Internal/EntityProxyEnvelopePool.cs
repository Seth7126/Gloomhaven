using System.Collections.Generic;

namespace Photon.Bolt.Internal;

internal static class EntityProxyEnvelopePool
{
	private static readonly Stack<EntityProxyEnvelope> pool = new Stack<EntityProxyEnvelope>();

	internal static EntityProxyEnvelope Acquire()
	{
		if (pool.Count > 0)
		{
			return pool.Pop();
		}
		return new EntityProxyEnvelope();
	}

	internal static void Release(EntityProxyEnvelope obj)
	{
		pool.Push(obj);
	}
}
