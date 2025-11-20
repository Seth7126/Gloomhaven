#define DEBUG
using System;
using System.Collections.Generic;
using Photon.Bolt.Utils;

namespace Photon.Bolt;

public class ObjectPool<T> where T : new()
{
	private readonly Stack<T> _pool = new Stack<T>();

	public bool Available => _pool.Count > 0;

	public ObjectPool()
	{
		_pool = new Stack<T>();
	}

	public void Return(T obj)
	{
		if (_pool.Contains(obj))
		{
			BoltLog.Error("Duplicate return for {0}: \r\n{1}", obj, Environment.StackTrace);
		}
		_pool.Push(obj);
	}

	public T Get()
	{
		return (_pool.Count > 0) ? _pool.Pop() : new T();
	}
}
