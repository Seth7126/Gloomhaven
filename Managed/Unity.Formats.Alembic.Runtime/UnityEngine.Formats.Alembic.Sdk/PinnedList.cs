using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UnityEngine.Formats.Alembic.Sdk;

internal class PinnedList<T> : IDisposable, IEnumerable<T>, IEnumerable where T : struct
{
	private List<T> m_list;

	private T[] m_data;

	private GCHandle m_gch;

	public int Capacity => m_data.Length;

	public int Count => m_list.Count;

	public T this[int i]
	{
		get
		{
			return m_data[i];
		}
		set
		{
			m_data[i] = value;
		}
	}

	public List<T> List => m_list;

	public IntPtr Pointer
	{
		get
		{
			if (Count != 0)
			{
				return m_gch.AddrOfPinnedObject();
			}
			return IntPtr.Zero;
		}
	}

	public PinnedList(int size = 0)
	{
		m_data = new T[size];
		m_list = PinnedListImpl.CreateIntrusiveList(m_data);
		m_gch = GCHandle.Alloc(m_data, GCHandleType.Pinned);
	}

	public PinnedList(T[] data, bool clone = false)
	{
		if (data != null)
		{
			m_data = (clone ? ((T[])data.Clone()) : data);
			m_list = PinnedListImpl.CreateIntrusiveList(m_data);
			m_gch = GCHandle.Alloc(m_data, GCHandleType.Pinned);
		}
	}

	public PinnedList(List<T> data, bool clone = false)
	{
		m_list = (clone ? new List<T>(data) : data);
		m_data = PinnedListImpl.GetInternalArray(m_list);
		m_gch = GCHandle.Alloc(m_data, GCHandleType.Pinned);
	}

	public T[] GetArray()
	{
		return m_data;
	}

	public void LockList(Action<List<T>> body)
	{
		if (body != null)
		{
			if (m_gch.IsAllocated)
			{
				m_gch.Free();
			}
			body(m_list);
			m_data = PinnedListImpl.GetInternalArray(m_list);
			m_gch = GCHandle.Alloc(m_data, GCHandleType.Pinned);
		}
	}

	public void Resize(int size)
	{
		if (size > m_data.Length)
		{
			LockList(delegate(List<T> l)
			{
				l.Capacity = size;
			});
		}
		PinnedListImpl.SetCount(m_list, size);
	}

	public void ResizeDiscard(int size)
	{
		if (size > m_data.Length)
		{
			if (m_gch.IsAllocated)
			{
				m_gch.Free();
			}
			m_data = new T[size];
			m_list = PinnedListImpl.CreateIntrusiveList(m_data);
			m_gch = GCHandle.Alloc(m_data, GCHandleType.Pinned);
		}
		else
		{
			PinnedListImpl.SetCount(m_list, size);
		}
	}

	public void Clear()
	{
		if (m_data.Length != 0)
		{
			PinnedListImpl.SetCount(m_list, 0);
		}
	}

	public PinnedList<T> Clone()
	{
		return new PinnedList<T>(m_list, clone: true);
	}

	public void Assign(T[] source)
	{
		if (source != null)
		{
			ResizeDiscard(source.Length);
			Array.Copy(source, m_data, source.Length);
		}
	}

	public void Assign(List<T> sourceList)
	{
		if (sourceList != null)
		{
			T[] internalArray = PinnedListImpl.GetInternalArray(sourceList);
			int count = sourceList.Count;
			ResizeDiscard(count);
			Array.Copy(internalArray, m_data, count);
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing && m_gch.IsAllocated)
		{
			m_gch.Free();
		}
	}

	public IEnumerator<T> GetEnumerator()
	{
		return (IEnumerator<T>)m_data.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public static implicit operator IntPtr(PinnedList<T> v)
	{
		return v?.Pointer ?? IntPtr.Zero;
	}

	internal static IntPtr ToIntPtr(PinnedList<T> v)
	{
		return v;
	}
}
