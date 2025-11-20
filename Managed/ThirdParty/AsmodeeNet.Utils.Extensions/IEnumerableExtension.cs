using System.Collections.Generic;

namespace AsmodeeNet.Utils.Extensions;

public static class IEnumerableExtension
{
	public static T First<T>(this IEnumerable<T> items)
	{
		using IEnumerator<T> enumerator = items.GetEnumerator();
		enumerator.MoveNext();
		return enumerator.Current;
	}
}
