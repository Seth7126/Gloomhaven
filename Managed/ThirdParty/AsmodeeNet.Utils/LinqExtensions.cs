using System.Collections.Generic;
using System.Linq;

namespace AsmodeeNet.Utils;

public static class LinqExtensions
{
	public static IEnumerable<T> Diff<T>(this IEnumerable<T> first, IEnumerable<T> second)
	{
		return first.Except(second).Union(second.Except(first));
	}

	public static IEnumerable<T> Diff<T>(this IEnumerable<T> first, IEnumerable<T> second, IEqualityComparer<T> comparer)
	{
		return first.Except(second, comparer).Union(second.Except(first, comparer));
	}
}
