using System.Collections;
using System.Collections.Generic;

namespace Photon.Bolt;

public class EntityList : IEnumerable<BoltEntity>, IEnumerable
{
	private readonly List<Entity> _list;

	public int Count => _list.Count;

	internal EntityList(List<Entity> l)
	{
		_list = l;
	}

	public IEnumerator<BoltEntity> GetEnumerator()
	{
		foreach (Entity entity in _list)
		{
			if (entity != null && entity.IsAttached && entity.UnityObject != null)
			{
				yield return entity.UnityObject;
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
