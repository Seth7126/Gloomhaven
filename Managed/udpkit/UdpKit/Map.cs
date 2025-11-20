using System;
using System.Collections;
using System.Collections.Generic;

namespace UdpKit;

public sealed class Map<K, V> : IEquatable<Map<K, V>>, IEnumerable<KeyValuePair<K, V>>, IEnumerable where K : IEquatable<K>, IComparable<K>
{
	private static readonly Map<K, V> _empty = new Map<K, V>();

	private readonly K _key;

	private readonly V _value;

	private readonly int _count;

	private readonly int _height;

	private readonly Map<K, V> _left;

	private readonly Map<K, V> _right;

	private K Key
	{
		get
		{
			if (_count == 0)
			{
				throw new InvalidOperationException("Map is empty");
			}
			return _key;
		}
	}

	private V Value
	{
		get
		{
			if (_count == 0)
			{
				throw new InvalidOperationException("Map is empty");
			}
			return _value;
		}
	}

	private Map<K, V> Left
	{
		get
		{
			if (_count == 0)
			{
				throw new InvalidOperationException("Map is empty");
			}
			return _left;
		}
	}

	private Map<K, V> Right
	{
		get
		{
			if (_count == 0)
			{
				throw new InvalidOperationException("Map is empty");
			}
			return _right;
		}
	}

	private int Balance
	{
		get
		{
			if (_count == 0)
			{
				return 0;
			}
			return Left._height - Right._height;
		}
	}

	public int Count => _count;

	public V this[K key] => Find(key);

	public Map()
	{
		_key = default(K);
		_value = default(V);
		_count = 0;
		_height = 0;
		_left = null;
		_right = null;
	}

	private Map(K key, V value)
		: this(key, value, _empty, _empty)
	{
	}

	private Map(K key, V value, Map<K, V> left, Map<K, V> right)
	{
		_key = key;
		_value = value;
		_left = left;
		_right = right;
		_height = Math.Max(left._height, right._height) + 1;
		_count = left.Count + right.Count + 1;
	}

	public Map<K, V> Add(K key, V value)
	{
		if (_count == 0)
		{
			return new Map<K, V>(key, value, _empty, _empty);
		}
		int num = key.CompareTo(Key);
		if (num < 0)
		{
			return Rebalance(new Map<K, V>(Key, Value, Left.Add(key, value), Right));
		}
		if (num > 0)
		{
			return Rebalance(new Map<K, V>(Key, Value, Left, Right.Add(key, value)));
		}
		throw new InvalidOperationException("Key already exists");
	}

	public Map<K, V> Update(K key, V value)
	{
		if (_count == 0)
		{
			return new Map<K, V>(key, value, _empty, _empty);
		}
		int num = key.CompareTo(_key);
		if (num < 0)
		{
			return Rebalance(new Map<K, V>(Key, Value, Left.Update(key, value), Right));
		}
		if (num > 0)
		{
			return Rebalance(new Map<K, V>(Key, Value, Left, Right.Update(key, value)));
		}
		return new Map<K, V>(key, value, Left, Right);
	}

	public Map<K, V> Remove(K key)
	{
		if (_count == 0)
		{
			throw new KeyNotFoundException();
		}
		int num = key.CompareTo(Key);
		if (num < 0)
		{
			return Rebalance(new Map<K, V>(Key, Value, Left.Remove(key), Right));
		}
		if (num > 0)
		{
			return Rebalance(new Map<K, V>(Key, Value, Left, Right.Remove(key)));
		}
		if (Right.Count == 0)
		{
			if (Left.Count == 0)
			{
				return _empty;
			}
			return Rebalance(Left);
		}
		if (Left.Count == 0)
		{
			return Rebalance(Right);
		}
		Map<K, V> map = Right;
		while (map.Left.Count > 0)
		{
			map = map.Left;
		}
		Map<K, V> right = Right.Remove(map.Key);
		return Rebalance(new Map<K, V>(map.Key, map.Value, Left, right));
	}

	public V Find(K key)
	{
		Map<K, V> map = Search(key);
		if (map.Count == 0)
		{
			throw new KeyNotFoundException();
		}
		return map.Value;
	}

	public bool TryFind(K key, out V value)
	{
		Map<K, V> map = Search(key);
		if (map.Count == 0)
		{
			value = default(V);
			return false;
		}
		value = map.Value;
		return true;
	}

	public bool Equals(Map<K, V> other)
	{
		if (this == other)
		{
			return true;
		}
		if (Count != other.Count)
		{
			return false;
		}
		IEnumerator<KeyValuePair<K, V>> enumerator = GetEnumerator();
		IEnumerator<KeyValuePair<K, V>> enumerator2 = GetEnumerator();
		while (enumerator.MoveNext() && enumerator2.MoveNext())
		{
			KeyValuePair<K, V> current = enumerator.Current;
			KeyValuePair<K, V> current2 = enumerator2.Current;
			if (!current.Key.Equals(current2.Key))
			{
				return false;
			}
			if (!current.Value.Equals(current2.Value))
			{
				return false;
			}
		}
		return true;
	}

	public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
	{
		Stack<Map<K, V>> stack = new Stack<Map<K, V>>();
		stack.Push(this);
		while (stack.Count > 0)
		{
			Map<K, V> map = stack.Pop();
			if (map.Count != 0)
			{
				if (map.Left.Count == 0)
				{
					yield return new KeyValuePair<K, V>(map.Key, map.Value);
					stack.Push(map.Right);
				}
				else
				{
					stack.Push(map.Right);
					stack.Push(new Map<K, V>(map.Key, map.Value));
					stack.Push(map.Left);
				}
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private Map<K, V> Search(K key)
	{
		if (_count == 0)
		{
			return this;
		}
		int num = key.CompareTo(Key);
		if (num < 0)
		{
			return Left.Search(key);
		}
		if (num > 0)
		{
			return Right.Search(key);
		}
		return this;
	}

	private static Map<K, V> RotateLeft(Map<K, V> map)
	{
		return new Map<K, V>(map.Right.Key, map.Right.Value, new Map<K, V>(map.Key, map.Value, map.Left, map.Right.Left), map.Right.Right);
	}

	private static Map<K, V> RotateRight(Map<K, V> map)
	{
		return new Map<K, V>(map.Left.Key, map.Left.Value, map.Left.Left, new Map<K, V>(map.Key, map.Value, map.Left.Right, map.Right));
	}

	private static Map<K, V> Rebalance(Map<K, V> map)
	{
		int balance = map.Balance;
		if (balance < -2)
		{
			throw new Exception();
		}
		if (balance > 2)
		{
			throw new Exception();
		}
		switch (balance)
		{
		case 2:
			if (map.Left.Balance == -1)
			{
				map = new Map<K, V>(map.Key, map.Value, RotateLeft(map.Left), map.Right);
			}
			return RotateRight(map);
		case -2:
			if (map.Right.Balance == 1)
			{
				map = new Map<K, V>(map.Key, map.Value, map.Left, RotateRight(map.Right));
			}
			return RotateLeft(map);
		default:
			return map;
		}
	}
}
