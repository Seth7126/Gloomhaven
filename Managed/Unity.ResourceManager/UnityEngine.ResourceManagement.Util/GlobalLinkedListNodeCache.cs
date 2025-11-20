using System.Collections.Generic;

namespace UnityEngine.ResourceManagement.Util;

internal static class GlobalLinkedListNodeCache<T>
{
	private static LinkedListNodeCache<T> m_globalCache;

	public static LinkedListNode<T> Acquire(T val)
	{
		if (m_globalCache == null)
		{
			m_globalCache = new LinkedListNodeCache<T>();
		}
		return m_globalCache.Acquire(val);
	}

	public static void Release(LinkedListNode<T> node)
	{
		if (m_globalCache == null)
		{
			m_globalCache = new LinkedListNodeCache<T>();
		}
		m_globalCache.Release(node);
	}
}
