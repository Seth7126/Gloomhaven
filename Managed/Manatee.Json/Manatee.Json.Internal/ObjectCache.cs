using System;
using System.Threading;

namespace Manatee.Json.Internal;

internal class ObjectCache<T> where T : class
{
	private static readonly int _cacheSize = Environment.ProcessorCount * 2;

	private readonly T[] _items = new T[_cacheSize];

	private T _item;

	private readonly Func<T> _builder;

	public ObjectCache(Func<T> builder)
	{
		_builder = builder;
	}

	public T Acquire()
	{
		T val = _item;
		if (val == null || val != Interlocked.CompareExchange(ref _item, null, val))
		{
			val = _AcquireSlow();
		}
		return val;
	}

	public void Release(T obj)
	{
		if (_item == null)
		{
			_item = obj;
		}
		else
		{
			_ReleaseSlow(obj);
		}
	}

	private T _AcquireSlow()
	{
		for (int i = 0; i < _items.Length; i++)
		{
			T val = _items[i];
			if (val != null && val == Interlocked.CompareExchange(ref _items[i], null, val))
			{
				return val;
			}
		}
		return _builder();
	}

	private void _ReleaseSlow(T obj)
	{
		for (int i = 0; i < _items.Length; i++)
		{
			if (_items[i] == null)
			{
				_items[i] = obj;
				break;
			}
		}
	}
}
