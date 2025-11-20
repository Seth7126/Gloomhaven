using System;
using System.Collections;
using System.Collections.Generic;

namespace Photon.Bolt.Collections;

[Documentation(Ignore = true)]
public class BoltDoubleList<T> : IEnumerable<T>, IEnumerable where T : class, IBoltListNode<T>
{
	private T _first;

	private int _count;

	public int count => _count;

	public T first
	{
		get
		{
			VerifyNotEmpty();
			return _first;
		}
	}

	public T firstOrDefault
	{
		get
		{
			if (_count > 0)
			{
				return first;
			}
			return null;
		}
	}

	public T last
	{
		get
		{
			VerifyNotEmpty();
			return _first.prev;
		}
	}

	public T lastOrDefault
	{
		get
		{
			if (_count > 0)
			{
				return last;
			}
			return null;
		}
	}

	public T this[int index]
	{
		get
		{
			if (index < 0 || index >= _count)
			{
				throw new IndexOutOfRangeException(index.ToString());
			}
			T val = first;
			while (index-- > 0)
			{
				val = Next(val);
			}
			return val;
		}
	}

	public BoltIterator<T> GetIterator()
	{
		return new BoltIterator<T>(_first, _count);
	}

	public bool Contains(T node)
	{
		return node != null && this == node.list;
	}

	public bool IsFirst(T node)
	{
		VerifyInList(node);
		if (_count == 0)
		{
			return false;
		}
		return node == _first;
	}

	public void AddLast(T node)
	{
		VerifyCanInsert(node);
		if (_count == 0)
		{
			InsertEmpty(node);
		}
		else
		{
			InsertBefore(node, _first);
		}
	}

	public void AddFirst(T node)
	{
		VerifyCanInsert(node);
		if (_count == 0)
		{
			InsertEmpty(node);
			return;
		}
		InsertBefore(node, _first);
		_first = node;
	}

	public T Remove(T node)
	{
		VerifyInList(node);
		VerifyNotEmpty();
		RemoveNode(node);
		return node;
	}

	public T RemoveFirst()
	{
		return Remove(_first);
	}

	public T RemoveLast()
	{
		return Remove(_first.prev);
	}

	public void Clear()
	{
		_first = null;
		_count = 0;
	}

	public T Prev(T node)
	{
		VerifyInList(node);
		return node.prev;
	}

	public T Next(T node)
	{
		VerifyInList(node);
		return node.next;
	}

	public void Replace(T node, T newNode)
	{
		VerifyInList(node);
		VerifyCanInsert(newNode);
		newNode.list = this;
		newNode.next = node.next;
		newNode.prev = node.prev;
		T next = newNode.next;
		T prev = newNode.prev;
		next.prev = newNode;
		prev.next = newNode;
		if (_first == node)
		{
			_first = newNode;
		}
		node.list = null;
		node.prev = null;
		node.next = null;
	}

	private void VerifyCanInsert(T node)
	{
		if (node.list != null)
		{
			throw new InvalidOperationException("Node is already in a list");
		}
	}

	private void VerifyInList(T node)
	{
		if (node.list != this)
		{
			throw new InvalidOperationException("Node is not in this list");
		}
	}

	private void InsertBefore(T node, T before)
	{
		node.next = before;
		node.prev = before.prev;
		T prev = before.prev;
		prev.next = node;
		before.prev = node;
		node.list = this;
		_count++;
	}

	private void InsertEmpty(T node)
	{
		_first = node;
		_first.next = node;
		_first.prev = node;
		node.list = this;
		_count++;
	}

	private void RemoveNode(T node)
	{
		if (_count == 1)
		{
			_first = null;
		}
		else
		{
			T next = node.next;
			T prev = node.prev;
			next.prev = node.prev;
			prev.next = node.next;
			if (_first == node)
			{
				_first = node.next;
			}
		}
		node.list = null;
		_count--;
	}

	private void VerifyNotEmpty()
	{
		if (_count == 0)
		{
			throw new InvalidOperationException("List is empty");
		}
	}

	public IEnumerator<T> GetEnumerator()
	{
		T n = _first;
		for (int c = count; c > 0; c--)
		{
			yield return n;
			n = n.next;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public static implicit operator bool(BoltDoubleList<T> list)
	{
		return list != null;
	}
}
