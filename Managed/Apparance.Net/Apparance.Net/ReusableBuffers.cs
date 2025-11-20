using System.Collections.Generic;

namespace Apparance.Net;

public class ReusableBuffers<T>
{
	private int initialCapacity;

	private List<ReusableBuffer<T>> bufferList;

	public ReusableBuffers(int initial_capacity)
	{
		initialCapacity = initial_capacity;
		bufferList = new List<ReusableBuffer<T>>();
	}

	public T[] Buffer(int index, int capacity)
	{
		while (index >= bufferList.Count)
		{
			bufferList.Add(new ReusableBuffer<T>(initialCapacity));
		}
		return bufferList[index].Buffer(capacity);
	}
}
