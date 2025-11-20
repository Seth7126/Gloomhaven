using System;
using System.Runtime.InteropServices;

namespace UnityEngine.Formats.Alembic.Sdk;

internal class PinnedObject<T> : IDisposable
{
	private T m_data;

	private GCHandle m_gch;

	public T Object => m_data;

	public IntPtr Pointer => m_gch.AddrOfPinnedObject();

	public PinnedObject(T data)
	{
		m_data = data;
		m_gch = GCHandle.Alloc(m_data, GCHandleType.Pinned);
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

	public static implicit operator IntPtr(PinnedObject<T> v)
	{
		return v?.Pointer ?? IntPtr.Zero;
	}

	internal static IntPtr ToIntPtr(PinnedObject<T> v)
	{
		return v;
	}
}
