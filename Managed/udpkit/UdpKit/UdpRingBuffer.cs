using System;
using System.Collections;
using System.Collections.Generic;

namespace UdpKit;

public class UdpRingBuffer<T> : IEnumerable<T>, IEnumerable
{
	private int _head;

	private int _tail;

	private int _count;

	private bool _autofree;

	private readonly T[] array;

	public bool Full => _count == array.Length;

	public float FillRatio => UdpMath.Clamp((float)_count / (float)array.Length, 0f, 1f);

	public bool Empty => _count == 0;

	public bool AutoFree
	{
		get
		{
			return _autofree;
		}
		set
		{
			_autofree = value;
		}
	}

	public int Count => _count;

	public T First
	{
		get
		{
			VerifyNotEmpty();
			return this[0];
		}
		set
		{
			VerifyNotEmpty();
			this[0] = value;
		}
	}

	public T FirstOrDefault
	{
		get
		{
			if (Count > 0)
			{
				return First;
			}
			return default(T);
		}
	}

	public T Last
	{
		get
		{
			VerifyNotEmpty();
			return this[Count - 1];
		}
		set
		{
			VerifyNotEmpty();
			this[Count - 1] = value;
		}
	}

	public T LastOrDefault
	{
		get
		{
			if (Count > 0)
			{
				return Last;
			}
			return default(T);
		}
	}

	public T this[int index]
	{
		get
		{
			VerifyNotEmpty();
			return array[(_tail + index) % array.Length];
		}
		set
		{
			if (index >= _count)
			{
				throw new IndexOutOfRangeException("can't change value of non-existand index");
			}
			array[(_tail + index) % array.Length] = value;
		}
	}

	public UdpRingBuffer(int size)
	{
		array = new T[size];
	}

	public void Enqueue(T item)
	{
		if (_count == array.Length)
		{
			if (!_autofree)
			{
				throw new InvalidOperationException("buffer is full");
			}
			Dequeue();
		}
		array[_head] = item;
		_head = (_head + 1) % array.Length;
		_count++;
	}

	public T Dequeue()
	{
		VerifyNotEmpty();
		T result = array[_tail];
		array[_tail] = default(T);
		_tail = (_tail + 1) % array.Length;
		_count--;
		return result;
	}

	public T Peek()
	{
		VerifyNotEmpty();
		return array[_tail];
	}

	public void Clear()
	{
		Array.Clear(array, 0, array.Length);
		_count = (_tail = (_head = 0));
	}

	public void CopyTo(UdpRingBuffer<T> other)
	{
		if (array.Length != other.array.Length)
		{
			throw new InvalidOperationException("buffers must be of the same capacity");
		}
		other._head = _head;
		other._tail = _tail;
		other._count = _count;
		Array.Copy(array, 0, other.array, 0, array.Length);
	}

	private void VerifyNotEmpty()
	{
		if (_count == 0)
		{
			throw new InvalidOperationException("buffer is empty");
		}
	}

	public IEnumerator<T> GetEnumerator()
	{
		int i = 0;
		while (i < _count)
		{
			yield return this[i];
			int num = i + 1;
			i = num;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
