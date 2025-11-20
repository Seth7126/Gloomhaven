using System.Collections.Generic;

namespace AsmodeeNet.Utils.Extensions;

public static class ListExtension
{
	public static T First<T>(this List<T> items)
	{
		if (items != null && items.Count > 0)
		{
			return items[0];
		}
		return default(T);
	}

	public static T Last<T>(this List<T> items)
	{
		if (items != null && items.Count > 0)
		{
			return items[items.Count - 1];
		}
		return default(T);
	}

	public static T RemoveFirst<T>(this List<T> items)
	{
		if (items != null && items.Count > 0)
		{
			T result = items[0];
			items.RemoveAt(0);
			return result;
		}
		return default(T);
	}

	public static T RemoveLast<T>(this List<T> items)
	{
		if (items != null && items.Count > 0)
		{
			T result = items[items.Count - 1];
			items.RemoveAt(items.Count - 1);
			return result;
		}
		return default(T);
	}

	public static int? Max(this List<int?> items)
	{
		int? num = null;
		if (items != null && items.Count > 0)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (!num.HasValue || items[i] > num)
				{
					num = items[i];
				}
			}
		}
		return num;
	}

	public static int Max(this List<int> items)
	{
		int num = -1;
		if (items != null && items.Count > 0)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i] > num)
				{
					num = items[i];
				}
			}
		}
		return num;
	}

	public static int Sum(this List<int> items)
	{
		int num = 0;
		if (items != null && items.Count > 0)
		{
			for (int i = 0; i < items.Count; i++)
			{
				num += items[i];
			}
		}
		return num;
	}
}
