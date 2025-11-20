using System;
using System.Collections;
using System.Collections.Generic;

namespace Photon.Bolt;

public abstract class NetworkArray_Values<T> : NetworkObj, IEnumerable<T>, IEnumerable
{
	private int _length;

	private int _stride;

	public int Length => _length;

	internal override NetworkStorage Storage => Root.Storage;

	public T this[int index]
	{
		get
		{
			if (index < 0 || index >= _length)
			{
				throw new IndexOutOfRangeException();
			}
			return GetValue(OffsetStorage + index * _stride);
		}
		set
		{
			if (index < 0 || index >= _length)
			{
				throw new IndexOutOfRangeException();
			}
			if (SetValue(OffsetStorage + index * _stride, value))
			{
				Storage.PropertyChanged(OffsetProperties + index);
			}
		}
	}

	internal NetworkArray_Values(int length, int stride)
		: base(NetworkArray_Meta.Instance)
	{
		_length = length;
		_stride = stride;
	}

	protected abstract T GetValue(int index);

	protected abstract bool SetValue(int index, T value);

	public IEnumerator<T> GetEnumerator()
	{
		int i = 0;
		while (i < _length)
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
