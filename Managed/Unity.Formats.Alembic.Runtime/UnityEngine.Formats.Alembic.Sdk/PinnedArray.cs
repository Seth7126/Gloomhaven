using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UnityEngine.Formats.Alembic.Sdk;

internal class PinnedArray<T> : IDisposable, IEnumerable<T>, IEnumerable where T : struct
{
	private T[] m_data;

	private GCHandle m_gch;

	public int Length => m_data.Length;

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

	public IntPtr Pointer
	{
		get
		{
			if (m_data.Length != 0)
			{
				return m_gch.AddrOfPinnedObject();
			}
			return IntPtr.Zero;
		}
	}

	public PinnedArray(int size)
	{
		m_data = new T[size];
		m_gch = GCHandle.Alloc(m_data, GCHandleType.Pinned);
	}

	public PinnedArray(T[] data, bool clone = false)
	{
		if (data != null)
		{
			m_data = (clone ? ((T[])data.Clone()) : data);
			m_gch = GCHandle.Alloc(m_data, GCHandleType.Pinned);
		}
	}

	public T[] GetArray()
	{
		return m_data;
	}

	public PinnedArray<T> Clone()
	{
		return new PinnedArray<T>((T[])m_data.Clone());
	}

	public bool Assign(T[] source)
	{
		if (source != null && m_data.Length == source.Length)
		{
			Array.Copy(source, m_data, m_data.Length);
			return true;
		}
		return false;
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

	public static implicit operator IntPtr(PinnedArray<T> v)
	{
		return v?.Pointer ?? IntPtr.Zero;
	}

	internal static IntPtr ToIntPtr(PinnedArray<T> v)
	{
		return v;
	}
}
