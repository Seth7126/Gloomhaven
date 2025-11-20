using System;
using System.Collections;
using System.Collections.Generic;

namespace Photon.Bolt.Collections;

[Documentation(Ignore = true)]
public class BoltSingleList<T> : IEnumerable<T>, IEnumerable where T : class, IBoltListNode<T>
{
	private T _head;

	private T _tail;

	private int _count;

	public int count => _count;

	public T first
	{
		get
		{
			VerifyNotEmpty();
			return _head;
		}
	}

	public T last
	{
		get
		{
			VerifyNotEmpty();
			return _tail;
		}
	}

	public BoltIterator<T> GetIterator()
	{
		return new BoltIterator<T>(_head, _count);
	}

	public void AddFirst(T item)
	{
		VerifyCanInsert(item);
		if (_count == 0)
		{
			_head = (_tail = item);
		}
		else
		{
			item.next = _head;
			_head = item;
		}
		item.list = this;
		_count++;
	}

	public void AddLast(T item)
	{
		VerifyCanInsert(item);
		if (_count == 0)
		{
			_head = (_tail = item);
		}
		else
		{
			_tail.next = item;
			_tail = item;
		}
		item.list = this;
		_count++;
	}

	public T PeekFirst()
	{
		VerifyNotEmpty();
		return _head;
	}

	public T RemoveFirst()
	{
		VerifyNotEmpty();
		T head = _head;
		if (_count == 1)
		{
			_head = (_tail = null);
		}
		else
		{
			_head = _head.next;
		}
		_count--;
		head.list = null;
		return head;
	}

	public void Clear()
	{
		_head = null;
		_tail = null;
		_count = 0;
	}

	public T Next(T item)
	{
		VerifyInList(item);
		return item.next;
	}

	public IEnumerator<T> GetEnumerator()
	{
		for (T current = _head; current != null; current = current.next)
		{
			yield return current;
		}
	}

	private void VerifyNotEmpty()
	{
		if (_count == 0)
		{
			throw new InvalidOperationException("List is empty");
		}
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

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public static implicit operator bool(BoltSingleList<T> list)
	{
		return list != null;
	}
}
