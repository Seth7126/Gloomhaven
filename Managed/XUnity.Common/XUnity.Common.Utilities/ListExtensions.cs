using System;
using System.Collections.Generic;

namespace XUnity.Common.Utilities;

public static class ListExtensions
{
	public static void BinarySearchInsert<T>(this List<T> items, T item) where T : IComparable<T>
	{
		int num = items.BinarySearch(item);
		if (num < 0)
		{
			items.Insert(~num, item);
		}
		else
		{
			items.Insert(num, item);
		}
	}
}
