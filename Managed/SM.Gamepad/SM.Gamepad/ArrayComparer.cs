using System.Collections.Generic;

namespace SM.Gamepad;

public class ArrayComparer<T> : IComparer<T>
{
	private readonly bool _missingElementsInTheEnd;

	private readonly Dictionary<T, int> _elements = new Dictionary<T, int>();

	public ArrayComparer(T[] elements, bool missingElementsInTheEnd = true)
	{
		_missingElementsInTheEnd = missingElementsInTheEnd;
		for (int i = 0; i < elements.Length; i++)
		{
			_elements[elements[i]] = i;
		}
	}

	public int Compare(T x, T y)
	{
		int indexOf = GetIndexOf(x);
		int indexOf2 = GetIndexOf(y);
		return indexOf.CompareTo(indexOf2);
	}

	private int GetIndexOf(T element)
	{
		if (!_elements.TryGetValue(element, out var value))
		{
			return _missingElementsInTheEnd ? int.MaxValue : int.MinValue;
		}
		return value;
	}
}
