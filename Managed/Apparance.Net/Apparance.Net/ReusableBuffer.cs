namespace Apparance.Net;

public class ReusableBuffer<T>
{
	private int dataCapacity;

	private T[] dataBuffer;

	public ReusableBuffer(int initial_capacity)
	{
		dataCapacity = initial_capacity;
		dataBuffer = new T[0];
	}

	public T[] Buffer(int capacity)
	{
		if (capacity > dataBuffer.Length)
		{
			while (capacity > dataCapacity)
			{
				dataCapacity *= 2;
			}
			dataBuffer = new T[dataCapacity];
		}
		return dataBuffer;
	}
}
