using System;
using System.Collections.Generic;

namespace AsmodeeNet.Utils;

public abstract class RandomGenerator
{
	protected uint _seed;

	public uint Seed
	{
		get
		{
			return _seed;
		}
		set
		{
			_seed = value;
			SeedUpdated();
		}
	}

	public abstract uint Rand();

	public abstract uint Range(uint min, uint max);

	protected virtual void SeedUpdated()
	{
	}

	public int RandomIndex<T>(IList<T> list)
	{
		int count = list.Count;
		if (count == 0)
		{
			throw new ArgumentOutOfRangeException("Empty list");
		}
		return (int)(Rand() % count);
	}

	public T ObjectAtRandomIndex<T>(IList<T> list)
	{
		try
		{
			int index = RandomIndex(list);
			return list[index];
		}
		catch
		{
			return default(T);
		}
	}

	public void Shuffle<T>(IList<T> list)
	{
		int count = list.Count;
		while (count > 1)
		{
			int index = (int)(Rand() % count--);
			T value = list[count];
			list[count] = list[index];
			list[index] = value;
		}
	}
}
