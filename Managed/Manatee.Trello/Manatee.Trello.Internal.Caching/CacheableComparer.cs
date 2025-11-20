using System.Collections.Generic;
using System.Linq;

namespace Manatee.Trello.Internal.Caching;

internal class CacheableComparer
{
	private static readonly List<CacheableComparer> Cache = new List<CacheableComparer>();

	public static IEqualityComparer<T> Get<T>() where T : ICacheable
	{
		CacheableComparer<T> cacheableComparer = Cache.OfType<CacheableComparer<T>>().FirstOrDefault();
		if (cacheableComparer == null)
		{
			cacheableComparer = new CacheableComparer<T>();
			Cache.Add(cacheableComparer);
		}
		return cacheableComparer;
	}
}
internal class CacheableComparer<T> : CacheableComparer, IEqualityComparer<T> where T : ICacheable
{
	public bool Equals(T x, T y)
	{
		return object.Equals(x?.Id, y?.Id);
	}

	public int GetHashCode(T obj)
	{
		return obj.Id.GetHashCode();
	}
}
