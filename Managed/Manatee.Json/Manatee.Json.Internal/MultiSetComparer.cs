using System.Collections.Generic;

namespace Manatee.Json.Internal;

internal class MultiSetComparer<T> : IEqualityComparer<IEnumerable<T>>
{
	private readonly IEqualityComparer<T> _comparer;

	public static MultiSetComparer<T> Default { get; } = new MultiSetComparer<T>();

	public MultiSetComparer(IEqualityComparer<T>? comparer = null)
	{
		_comparer = comparer ?? EqualityComparer<T>.Default;
	}

	public bool Equals(IEnumerable<T> first, IEnumerable<T> second)
	{
		if (first == null)
		{
			return second == null;
		}
		if (second == null)
		{
			return false;
		}
		if (first == second)
		{
			return true;
		}
		if (first is ICollection<T> collection && second is ICollection<T> collection2)
		{
			if (collection.Count != collection2.Count)
			{
				return false;
			}
			if (collection.Count == 0)
			{
				return true;
			}
		}
		return !_HaveMismatchedElement(first, second);
	}

	private bool _HaveMismatchedElement(IEnumerable<T> first, IEnumerable<T> second)
	{
		int nullCount;
		Dictionary<T, int> dictionary = _GetElementCounts(first, out nullCount);
		int nullCount2;
		Dictionary<T, int> dictionary2 = _GetElementCounts(second, out nullCount2);
		if (nullCount != nullCount2 || dictionary.Count != dictionary2.Count)
		{
			return true;
		}
		foreach (KeyValuePair<T, int> item in dictionary)
		{
			int value = item.Value;
			dictionary2.TryGetValue(item.Key, out var value2);
			if (value != value2)
			{
				return true;
			}
		}
		return false;
	}

	private Dictionary<T, int> _GetElementCounts(IEnumerable<T> enumerable, out int nullCount)
	{
		Dictionary<T, int> dictionary = new Dictionary<T, int>(_comparer);
		nullCount = 0;
		foreach (T item in enumerable)
		{
			if (item == null)
			{
				nullCount++;
				continue;
			}
			dictionary.TryGetValue(item, out var value);
			value = (dictionary[item] = value + 1);
		}
		return dictionary;
	}

	public int GetHashCode(IEnumerable<T> enumerable)
	{
		return enumerable.GetCollectionHashCode();
	}
}
