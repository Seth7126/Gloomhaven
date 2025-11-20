using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine.Internal;

namespace Unity.Collections;

[NativeContainerSupportsDeferredConvertListToArray]
[NativeContainerSupportsDeallocateOnJobCompletion]
[NativeContainerSupportsMinMaxWriteRestriction]
[NativeContainer]
[DebuggerDisplay("Length = {Length}")]
[DebuggerTypeProxy(typeof(NativeArrayDebugView<>))]
public struct NativeArray<T> : IDisposable, IEnumerable<T>, IEnumerable, IEquatable<NativeArray<T>> where T : struct
{
	[ExcludeFromDocs]
	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		private NativeArray<T> m_Array;

		private int m_Index;

		public T Current => m_Array[m_Index];

		object IEnumerator.Current => Current;

		public Enumerator(ref NativeArray<T> array)
		{
			m_Array = array;
			m_Index = -1;
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			m_Index++;
			return m_Index < m_Array.Length;
		}

		public void Reset()
		{
			m_Index = -1;
		}
	}

	[NativeContainerIsReadOnly]
	[NativeContainer]
	[DebuggerTypeProxy(typeof(NativeArrayReadOnlyDebugView<>))]
	[DebuggerDisplay("Length = {Length}")]
	public struct ReadOnly : IEnumerable<T>, IEnumerable
	{
		[ExcludeFromDocs]
		public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
		{
			private ReadOnly m_Array;

			private int m_Index;

			public T Current => m_Array[m_Index];

			object IEnumerator.Current => Current;

			public Enumerator(in ReadOnly array)
			{
				m_Array = array;
				m_Index = -1;
			}

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				m_Index++;
				return m_Index < m_Array.Length;
			}

			public void Reset()
			{
				m_Index = -1;
			}
		}

		[NativeDisableUnsafePtrRestriction]
		internal unsafe void* m_Buffer;

		internal int m_Length;

		public int Length => m_Length;

		public unsafe T this[int index] => UnsafeUtility.ReadArrayElement<T>(m_Buffer, index);

		public unsafe bool IsCreated => m_Buffer != null;

		internal unsafe ReadOnly(void* buffer, int length)
		{
			m_Buffer = buffer;
			m_Length = length;
		}

		public void CopyTo(T[] array)
		{
			NativeArray<T>.Copy(this, array);
		}

		public void CopyTo(NativeArray<T> array)
		{
			NativeArray<T>.Copy(this, array);
		}

		public T[] ToArray()
		{
			T[] array = new T[m_Length];
			NativeArray<T>.Copy(this, array, m_Length);
			return array;
		}

		public unsafe NativeArray<U>.ReadOnly Reinterpret<U>() where U : struct
		{
			return new NativeArray<U>.ReadOnly(m_Buffer, m_Length);
		}

		[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
		private void CheckElementReadAccess(int index)
		{
			if (index < 0 || index >= m_Length)
			{
				throw new IndexOutOfRangeException($"Index {index} is out of range (must be between 0 and {m_Length - 1}).");
			}
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(in this);
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	[NativeDisableUnsafePtrRestriction]
	internal unsafe void* m_Buffer;

	internal int m_Length;

	internal Allocator m_AllocatorLabel;

	public int Length => m_Length;

	public unsafe T this[int index]
	{
		get
		{
			return UnsafeUtility.ReadArrayElement<T>(m_Buffer, index);
		}
		[WriteAccessRequired]
		set
		{
			UnsafeUtility.WriteArrayElement(m_Buffer, index, value);
		}
	}

	public unsafe bool IsCreated => m_Buffer != null;

	public unsafe NativeArray(int length, Allocator allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory)
	{
		Allocate(length, allocator, out this);
		if ((options & NativeArrayOptions.ClearMemory) == NativeArrayOptions.ClearMemory)
		{
			UnsafeUtility.MemClear(m_Buffer, (long)Length * (long)UnsafeUtility.SizeOf<T>());
		}
	}

	public NativeArray(T[] array, Allocator allocator)
	{
		Allocate(array.Length, allocator, out this);
		Copy(array, this);
	}

	public NativeArray(NativeArray<T> array, Allocator allocator)
	{
		Allocate(array.Length, allocator, out this);
		Copy(array, 0, this, 0, array.Length);
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private static void CheckAllocateArguments(int length, Allocator allocator, long totalSize)
	{
		if (allocator <= Allocator.None)
		{
			throw new ArgumentException("Allocator must be Temp, TempJob or Persistent", "allocator");
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", "Length must be >= 0");
		}
		IsUnmanagedAndThrow();
	}

	private unsafe static void Allocate(int length, Allocator allocator, out NativeArray<T> array)
	{
		long size = (long)UnsafeUtility.SizeOf<T>() * (long)length;
		array = default(NativeArray<T>);
		array.m_Buffer = UnsafeUtility.Malloc(size, UnsafeUtility.AlignOf<T>(), allocator);
		array.m_Length = length;
		array.m_AllocatorLabel = allocator;
	}

	[BurstDiscard]
	internal static void IsUnmanagedAndThrow()
	{
		if (!UnsafeUtility.IsValidNativeContainerElementType<T>())
		{
			throw new InvalidOperationException($"{typeof(T)} used in NativeArray<{typeof(T)}> must be unmanaged (contain no managed types) and cannot itself be a native container type.");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private void CheckElementReadAccess(int index)
	{
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private void CheckElementWriteAccess(int index)
	{
	}

	[WriteAccessRequired]
	public unsafe void Dispose()
	{
		if (m_Buffer == null)
		{
			throw new ObjectDisposedException("The NativeArray is already disposed.");
		}
		if (m_AllocatorLabel == Allocator.Invalid)
		{
			throw new InvalidOperationException("The NativeArray can not be Disposed because it was not allocated with a valid allocator.");
		}
		if (m_AllocatorLabel > Allocator.None)
		{
			UnsafeUtility.Free(m_Buffer, m_AllocatorLabel);
			m_AllocatorLabel = Allocator.Invalid;
		}
		m_Buffer = null;
	}

	public unsafe JobHandle Dispose(JobHandle inputDeps)
	{
		if (m_AllocatorLabel == Allocator.Invalid)
		{
			throw new InvalidOperationException("The NativeArray can not be Disposed because it was not allocated with a valid allocator.");
		}
		if (m_Buffer == null)
		{
			throw new InvalidOperationException("The NativeArray is already disposed.");
		}
		if (m_AllocatorLabel > Allocator.None)
		{
			JobHandle result = new NativeArrayDisposeJob
			{
				Data = new NativeArrayDispose
				{
					m_Buffer = m_Buffer,
					m_AllocatorLabel = m_AllocatorLabel
				}
			}.Schedule(inputDeps);
			m_Buffer = null;
			m_AllocatorLabel = Allocator.Invalid;
			return result;
		}
		m_Buffer = null;
		return inputDeps;
	}

	[WriteAccessRequired]
	public void CopyFrom(T[] array)
	{
		Copy(array, this);
	}

	[WriteAccessRequired]
	public void CopyFrom(NativeArray<T> array)
	{
		Copy(array, this);
	}

	public void CopyTo(T[] array)
	{
		Copy(this, array);
	}

	public void CopyTo(NativeArray<T> array)
	{
		Copy(this, array);
	}

	public T[] ToArray()
	{
		T[] array = new T[Length];
		Copy(this, array, Length);
		return array;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(ref this);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return new Enumerator(ref this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public unsafe bool Equals(NativeArray<T> other)
	{
		return m_Buffer == other.m_Buffer && m_Length == other.m_Length;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		return obj is NativeArray<T> && Equals((NativeArray<T>)obj);
	}

	public unsafe override int GetHashCode()
	{
		return ((int)m_Buffer * 397) ^ m_Length;
	}

	public static bool operator ==(NativeArray<T> left, NativeArray<T> right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(NativeArray<T> left, NativeArray<T> right)
	{
		return !left.Equals(right);
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private static void CheckCopyLengths(int srcLength, int dstLength)
	{
		if (srcLength != dstLength)
		{
			throw new ArgumentException("source and destination length must be the same");
		}
	}

	public static void Copy(NativeArray<T> src, NativeArray<T> dst)
	{
		Copy(src, 0, dst, 0, src.Length);
	}

	public static void Copy(ReadOnly src, NativeArray<T> dst)
	{
		Copy(src, 0, dst, 0, src.Length);
	}

	public static void Copy(T[] src, NativeArray<T> dst)
	{
		Copy(src, 0, dst, 0, src.Length);
	}

	public static void Copy(NativeArray<T> src, T[] dst)
	{
		Copy(src, 0, dst, 0, src.Length);
	}

	public static void Copy(ReadOnly src, T[] dst)
	{
		Copy(src, 0, dst, 0, src.Length);
	}

	public static void Copy(NativeArray<T> src, NativeArray<T> dst, int length)
	{
		Copy(src, 0, dst, 0, length);
	}

	public static void Copy(ReadOnly src, NativeArray<T> dst, int length)
	{
		Copy(src, 0, dst, 0, length);
	}

	public static void Copy(T[] src, NativeArray<T> dst, int length)
	{
		Copy(src, 0, dst, 0, length);
	}

	public static void Copy(NativeArray<T> src, T[] dst, int length)
	{
		Copy(src, 0, dst, 0, length);
	}

	public static void Copy(ReadOnly src, T[] dst, int length)
	{
		Copy(src, 0, dst, 0, length);
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private static void CheckCopyArguments(int srcLength, int srcIndex, int dstLength, int dstIndex, int length)
	{
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", "length must be equal or greater than zero.");
		}
		if (srcIndex < 0 || srcIndex > srcLength || (srcIndex == srcLength && srcLength > 0))
		{
			throw new ArgumentOutOfRangeException("srcIndex", "srcIndex is outside the range of valid indexes for the source NativeArray.");
		}
		if (dstIndex < 0 || dstIndex > dstLength || (dstIndex == dstLength && dstLength > 0))
		{
			throw new ArgumentOutOfRangeException("dstIndex", "dstIndex is outside the range of valid indexes for the destination NativeArray.");
		}
		if (srcIndex + length > srcLength)
		{
			throw new ArgumentException("length is greater than the number of elements from srcIndex to the end of the source NativeArray.", "length");
		}
		if (srcIndex + length < 0)
		{
			throw new ArgumentException("srcIndex + length causes an integer overflow");
		}
		if (dstIndex + length > dstLength)
		{
			throw new ArgumentException("length is greater than the number of elements from dstIndex to the end of the destination NativeArray.", "length");
		}
		if (dstIndex + length < 0)
		{
			throw new ArgumentException("dstIndex + length causes an integer overflow");
		}
	}

	public unsafe static void Copy(NativeArray<T> src, int srcIndex, NativeArray<T> dst, int dstIndex, int length)
	{
		UnsafeUtility.MemCpy((byte*)dst.m_Buffer + dstIndex * UnsafeUtility.SizeOf<T>(), (byte*)src.m_Buffer + srcIndex * UnsafeUtility.SizeOf<T>(), length * UnsafeUtility.SizeOf<T>());
	}

	public unsafe static void Copy(ReadOnly src, int srcIndex, NativeArray<T> dst, int dstIndex, int length)
	{
		UnsafeUtility.MemCpy((byte*)dst.m_Buffer + dstIndex * UnsafeUtility.SizeOf<T>(), (byte*)src.m_Buffer + srcIndex * UnsafeUtility.SizeOf<T>(), length * UnsafeUtility.SizeOf<T>());
	}

	public unsafe static void Copy(T[] src, int srcIndex, NativeArray<T> dst, int dstIndex, int length)
	{
		GCHandle gCHandle = GCHandle.Alloc(src, GCHandleType.Pinned);
		IntPtr intPtr = gCHandle.AddrOfPinnedObject();
		UnsafeUtility.MemCpy((byte*)dst.m_Buffer + dstIndex * UnsafeUtility.SizeOf<T>(), (byte*)(void*)intPtr + srcIndex * UnsafeUtility.SizeOf<T>(), length * UnsafeUtility.SizeOf<T>());
		gCHandle.Free();
	}

	public unsafe static void Copy(NativeArray<T> src, int srcIndex, T[] dst, int dstIndex, int length)
	{
		GCHandle gCHandle = GCHandle.Alloc(dst, GCHandleType.Pinned);
		IntPtr intPtr = gCHandle.AddrOfPinnedObject();
		UnsafeUtility.MemCpy((byte*)(void*)intPtr + dstIndex * UnsafeUtility.SizeOf<T>(), (byte*)src.m_Buffer + srcIndex * UnsafeUtility.SizeOf<T>(), length * UnsafeUtility.SizeOf<T>());
		gCHandle.Free();
	}

	public unsafe static void Copy(ReadOnly src, int srcIndex, T[] dst, int dstIndex, int length)
	{
		GCHandle gCHandle = GCHandle.Alloc(dst, GCHandleType.Pinned);
		IntPtr intPtr = gCHandle.AddrOfPinnedObject();
		UnsafeUtility.MemCpy((byte*)(void*)intPtr + dstIndex * UnsafeUtility.SizeOf<T>(), (byte*)src.m_Buffer + srcIndex * UnsafeUtility.SizeOf<T>(), length * UnsafeUtility.SizeOf<T>());
		gCHandle.Free();
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private void CheckReinterpretLoadRange<U>(int sourceIndex) where U : struct
	{
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private void CheckReinterpretStoreRange<U>(int destIndex) where U : struct
	{
	}

	public unsafe U ReinterpretLoad<U>(int sourceIndex) where U : struct
	{
		byte* source = (byte*)m_Buffer + (long)UnsafeUtility.SizeOf<T>() * (long)sourceIndex;
		return UnsafeUtility.ReadArrayElement<U>(source, 0);
	}

	public unsafe void ReinterpretStore<U>(int destIndex, U data) where U : struct
	{
		byte* destination = (byte*)m_Buffer + (long)UnsafeUtility.SizeOf<T>() * (long)destIndex;
		UnsafeUtility.WriteArrayElement(destination, 0, data);
	}

	private unsafe NativeArray<U> InternalReinterpret<U>(int length) where U : struct
	{
		return NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<U>(m_Buffer, length, m_AllocatorLabel);
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private static void CheckReinterpretSize<U>() where U : struct
	{
		if (UnsafeUtility.SizeOf<T>() != UnsafeUtility.SizeOf<U>())
		{
			throw new InvalidOperationException($"Types {typeof(T)} and {typeof(U)} are different sizes - direct reinterpretation is not possible. If this is what you intended, use Reinterpret(<type size>)");
		}
	}

	public NativeArray<U> Reinterpret<U>() where U : struct
	{
		return InternalReinterpret<U>(Length);
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private void CheckReinterpretSize<U>(long tSize, long uSize, int expectedTypeSize, long byteLen, long uLen)
	{
		if (tSize != expectedTypeSize)
		{
			throw new InvalidOperationException($"Type {typeof(T)} was expected to be {expectedTypeSize} but is {tSize} bytes");
		}
		if (uLen * uSize != byteLen)
		{
			throw new InvalidOperationException($"Types {typeof(T)} (array length {Length}) and {typeof(U)} cannot be aliased due to size constraints. The size of the types and lengths involved must line up.");
		}
	}

	public NativeArray<U> Reinterpret<U>(int expectedTypeSize) where U : struct
	{
		long num = UnsafeUtility.SizeOf<T>();
		long num2 = UnsafeUtility.SizeOf<U>();
		long num3 = Length * num;
		long num4 = num3 / num2;
		return InternalReinterpret<U>((int)num4);
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private void CheckGetSubArrayArguments(int start, int length)
	{
		if (start < 0)
		{
			throw new ArgumentOutOfRangeException("start", "start must be >= 0");
		}
		if (start + length > Length)
		{
			throw new ArgumentOutOfRangeException("length", $"sub array range {start}-{start + length - 1} is outside the range of the native array 0-{Length - 1}");
		}
		if (start + length < 0)
		{
			throw new ArgumentException($"sub array range {start}-{start + length - 1} caused an integer overflow and is outside the range of the native array 0-{Length - 1}");
		}
	}

	public unsafe NativeArray<T> GetSubArray(int start, int length)
	{
		return NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>((byte*)m_Buffer + (long)UnsafeUtility.SizeOf<T>() * (long)start, length, Allocator.None);
	}

	public unsafe ReadOnly AsReadOnly()
	{
		return new ReadOnly(m_Buffer, m_Length);
	}
}
