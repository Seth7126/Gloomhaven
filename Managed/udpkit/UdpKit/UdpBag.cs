using System.Collections.Generic;

namespace UdpKit;

internal class UdpBag<T>
{
	public delegate T Modifier(T item);

	public delegate bool Predicate(T item);

	private readonly List<T> list = new List<T>();

	public void Add(T item)
	{
		lock (list)
		{
			list.Add(item);
		}
	}

	public void Remove(T item)
	{
		lock (list)
		{
			list.Remove(item);
		}
	}

	public T[] ToArray()
	{
		lock (list)
		{
			return list.ToArray();
		}
	}

	public void Map(Modifier func)
	{
		lock (list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				list[i] = func(list[i]);
			}
		}
	}

	public void Filter(Predicate func)
	{
		lock (list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (!func(list[i]))
				{
					list.RemoveAt(i);
					i--;
				}
			}
		}
	}

	public bool Update(Predicate predicate, Modifier modifier)
	{
		lock (list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (predicate(list[i]))
				{
					list[i] = modifier(list[i]);
					return true;
				}
			}
		}
		return false;
	}
}
