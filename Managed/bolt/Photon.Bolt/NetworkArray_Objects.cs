using System;
using System.Collections;
using System.Collections.Generic;

namespace Photon.Bolt;

[Documentation]
public class NetworkArray_Objects<T> : NetworkObj, IEnumerable<T>, IEnumerable where T : NetworkObj
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
			return (T)base.Objects[OffsetObjects + 1 + index * _stride];
		}
	}

	internal NetworkArray_Objects(int length, int stride)
		: base(NetworkArray_Meta.Instance)
	{
		_length = length;
		_stride = stride;
	}

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
