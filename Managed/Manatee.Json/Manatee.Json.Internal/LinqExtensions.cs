using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Manatee.Json.Internal;

internal static class LinqExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> items) where T : class
	{
		return items.Where((T i) => i != null);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool ContentsEqual<T>(this IEnumerable<T> a, IEnumerable<T> b)
	{
		return MultiSetComparer<T>.Default.Equals(a, b);
	}

	private static IEnumerable<TOut> _LeftJoin<TA, TB, TKey, TOut>(this IEnumerable<TA> aItems, IEnumerable<TB> bItems, Func<TA, TKey> aKeySelector, Func<TB, TKey> bKeySelector, Func<TA, TB, TOut> selector)
	{
		return from t in Enumerable.GroupJoin(aItems, bItems, aKeySelector, bKeySelector, (TA a, IEnumerable<TB> bSet) => new { a, bSet })
			from b in t.bSet.DefaultIfEmpty()
			select selector(t.a, b);
	}

	private static IEnumerable<TOut> _LeftJoin<TA, TB, TKey, TOut>(this IEnumerable<TA> aItems, IEnumerable<TB> bItems, Func<TA, TKey> aKeySelector, Func<TB, TKey> bKeySelector, Func<TA, TB, TOut> selector, IEqualityComparer<TKey> comparer)
	{
		return from t in Enumerable.GroupJoin(aItems, bItems, aKeySelector, bKeySelector, (TA a, IEnumerable<TB> bSet) => new { a, bSet }, comparer)
			from b in t.bSet.DefaultIfEmpty()
			select selector(t.a, b);
	}

	public static IEnumerable<TOut> FullOuterJoin<TA, TB, TKey, TOut>(this IEnumerable<TA> aItems, IEnumerable<TB> bItems, Func<TA, TKey> aKeySelector, Func<TB, TKey> bKeySelector, Func<TA, TB, TOut> selector)
	{
		var first = aItems._LeftJoin(bItems, aKeySelector, bKeySelector, (TA a, TB b) => new { a, b });
		var second = bItems._LeftJoin(aItems, bKeySelector, aKeySelector, (TB b, TA a) => new { a, b });
		return from x in first.Union(second)
			select selector(x.a, x.b);
	}

	public static IEnumerable<TOut> FullOuterJoin<TA, TB, TKey, TOut>(this IEnumerable<TA> aItems, IEnumerable<TB> bItems, Func<TA, TKey> aKeySelector, Func<TB, TKey> bKeySelector, Func<TA, TB, TOut> selector, IEqualityComparer<TKey> comparer)
	{
		var first = aItems._LeftJoin(bItems, aKeySelector, bKeySelector, (TA a, TB b) => new { a, b }, comparer);
		var second = bItems._LeftJoin(aItems, bKeySelector, aKeySelector, (TB b, TA a) => new { a, b }, comparer);
		return from x in first.Union(second)
			select selector(x.a, x.b);
	}

	public static IEnumerable<int> IndexesWhere<T>(this IEnumerable<T> items, Func<T, bool> predicate)
	{
		return from i in Enumerable.Select(items, (T item, int i) => new
			{
				Item = item,
				Index = i
			})
			where predicate(i.Item)
			select i.Index;
	}
}
