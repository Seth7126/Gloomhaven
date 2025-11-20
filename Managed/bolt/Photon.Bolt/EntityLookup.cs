using System.Collections;
using System.Collections.Generic;
using Photon.Bolt.Internal;

namespace Photon.Bolt;

public class EntityLookup : IEnumerable<BoltEntity>, IEnumerable
{
	private readonly Dictionary<NetworkId, EntityProxy> _dict;

	public int Count => _dict.Count;

	internal EntityLookup(Dictionary<NetworkId, EntityProxy> d)
	{
		_dict = d;
	}

	public bool TryGet(NetworkId id, out BoltEntity entity)
	{
		if (_dict.TryGetValue(id, out var value) && value.Entity != null && value.Entity.UnityObject != null)
		{
			entity = value.Entity.UnityObject;
			return true;
		}
		entity = null;
		return false;
	}

	public IEnumerator<BoltEntity> GetEnumerator()
	{
		foreach (EntityProxy proxy in _dict.Values)
		{
			if (proxy != null && proxy.Entity != null && proxy.Entity.IsAttached && proxy.Entity.UnityObject != null)
			{
				yield return proxy.Entity.UnityObject;
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
