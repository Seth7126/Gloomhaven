using System.Collections.Generic;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions;

internal static class EnumerableExtensions
{
	public static HashSet<T> ToHashSet<T>(this IEnumerable<T> ts)
	{
		HashSet<T> hashSet = new HashSet<T>();
		foreach (T t in ts)
		{
			hashSet.Add(t);
		}
		return hashSet;
	}

	public static HashSet<T> ToHashSet<T>(this IEnumerable<T> ts, IEqualityComparer<T> equalityComparer)
	{
		HashSet<T> hashSet = new HashSet<T>(equalityComparer);
		foreach (T t in ts)
		{
			hashSet.Add(t);
		}
		return hashSet;
	}

	public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> values)
	{
		foreach (T value in values)
		{
			collection.Add(value);
		}
	}
}
