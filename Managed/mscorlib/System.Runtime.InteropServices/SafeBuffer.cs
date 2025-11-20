using System.Runtime.CompilerServices;
using Microsoft.Win32.SafeHandles;

namespace System.Runtime.InteropServices;

/// <summary>Provides a controlled memory buffer that can be used for reading and writing. Attempts to access memory outside the controlled buffer (underruns and overruns) raise exceptions.</summary>
public abstract class SafeBuffer : SafeHandleZeroOrMinusOneIsInvalid
{
	private static readonly UIntPtr Uninitialized = ((UIntPtr.Size == 4) ? ((UIntPtr)uint.MaxValue) : ((UIntPtr)ulong.MaxValue));

	private UIntPtr _numBytes;

	/// <summary>Gets the size of the buffer, in bytes.</summary>
	/// <returns>The number of bytes in the memory buffer.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="Overload:System.Runtime.InteropServices.SafeBuffer.Initialize" /> method has not been called.</exception>
	[CLSCompliant(false)]
	public ulong ByteLength
	{
		get
		{
			if (_numBytes == Uninitialized)
			{
				throw NotInitialized();
			}
			return (ulong)_numBytes;
		}
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Runtime.InteropServices.SafeBuffer" /> class, and specifies whether the buffer handle is to be reliably released. </summary>
	/// <param name="ownsHandle">true to reliably release the handle during the finalization phase; false to prevent reliable release (not recommended).</param>
	protected SafeBuffer(bool ownsHandle)
		: base(ownsHandle)
	{
		_numBytes = Uninitialized;
	}

	/// <summary>Defines the allocation size of the memory region in bytes. You must call this method before you use the <see cref="T:System.Runtime.InteropServices.SafeBuffer" /> instance.</summary>
	/// <param name="numBytes">The number of bytes in the buffer.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="numBytes" /> is less than zero.-or-<paramref name="numBytes" /> is greater than the available address space.</exception>
	[CLSCompliant(false)]
	public void Initialize(ulong numBytes)
	{
		if (IntPtr.Size == 4 && numBytes > uint.MaxValue)
		{
			throw new ArgumentOutOfRangeException("numBytes", "The number of bytes cannot exceed the virtual address space on a 32 bit machine.");
		}
		if (numBytes >= (ulong)Uninitialized)
		{
			throw new ArgumentOutOfRangeException("numBytes", "The length of the buffer must be less than the maximum UIntPtr value for your platform.");
		}
		_numBytes = (UIntPtr)numBytes;
	}

	/// <summary>Specifies the allocation size of the memory buffer by using the specified number of elements and element size. You must call this method before you use the <see cref="T:System.Runtime.InteropServices.SafeBuffer" /> instance.</summary>
	/// <param name="numElements">The number of elements in the buffer.</param>
	/// <param name="sizeOfEachElement">The size of each element in the buffer.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="numElements" /> is less than zero. -or-<paramref name="sizeOfEachElement" /> is less than zero.-or-<paramref name="numElements" /> multiplied by <paramref name="sizeOfEachElement" /> is greater than the available address space.</exception>
	[CLSCompliant(false)]
	public void Initialize(uint numElements, uint sizeOfEachElement)
	{
		if (IntPtr.Size == 4 && numElements * sizeOfEachElement > uint.MaxValue)
		{
			throw new ArgumentOutOfRangeException("numBytes", "The number of bytes cannot exceed the virtual address space on a 32 bit machine.");
		}
		if (numElements * sizeOfEachElement >= (ulong)Uninitialized)
		{
			throw new ArgumentOutOfRangeException("numElements", "The length of the buffer must be less than the maximum UIntPtr value for your platform.");
		}
		_numBytes = (UIntPtr)checked(numElements * sizeOfEachElement);
	}

	/// <summary>Defines the allocation size of the memory region by specifying the number of value types. You must call this method before you use the <see cref="T:System.Runtime.InteropServices.SafeBuffer" /> instance.</summary>
	/// <param name="numElements">The number of elements of the value type to allocate memory for.</param>
	/// <typeparam name="T">The value type to allocate memory for.</typeparam>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="numElements" /> is less than zero.-or-<paramref name="numElements" /> multiplied by the size of each element is greater than the available address space.</exception>
	[CLSCompliant(false)]
	public void Initialize<T>(uint numElements) where T : struct
	{
		Initialize(numElements, AlignedSizeOf<T>());
	}

	/// <summary>Obtains a pointer from a <see cref="T:System.Runtime.InteropServices.SafeBuffer" /> object for a block of memory.</summary>
	/// <param name="pointer">A byte pointer, passed by reference, to receive the pointer from within the <see cref="T:System.Runtime.InteropServices.SafeBuffer" /> object. You must set this pointer to null before you call this method.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="Overload:System.Runtime.InteropServices.SafeBuffer.Initialize" /> method has not been called. </exception>
	[CLSCompliant(false)]
	public unsafe void AcquirePointer(ref byte* pointer)
	{
		if (_numBytes == Uninitialized)
		{
			throw NotInitialized();
		}
		pointer = null;
		bool success = false;
		DangerousAddRef(ref success);
		pointer = (byte*)(void*)handle;
	}

	/// <summary>Releases a pointer that was obtained by the <see cref="M:System.Runtime.InteropServices.SafeBuffer.AcquirePointer(System.Byte*@)" /> method.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="Overload:System.Runtime.InteropServices.SafeBuffer.Initialize" /> method has not been called.</exception>
	public void ReleasePointer()
	{
		if (_numBytes == Uninitialized)
		{
			throw NotInitialized();
		}
		DangerousRelease();
	}

	/// <summary>Reads a value type from memory at the specified offset.</summary>
	/// <returns>The value type that was read from memory.</returns>
	/// <param name="byteOffset">The location from which to read the value type. You may have to consider alignment issues.</param>
	/// <typeparam name="T">The value type to read.</typeparam>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="Overload:System.Runtime.InteropServices.SafeBuffer.Initialize" /> method has not been called.</exception>
	[CLSCompliant(false)]
	public unsafe T Read<T>(ulong byteOffset) where T : struct
	{
		if (_numBytes == Uninitialized)
		{
			throw NotInitialized();
		}
		uint num = SizeOf<T>();
		byte* ptr = (byte*)(void*)handle + byteOffset;
		SpaceCheck(ptr, num);
		T source = default(T);
		bool success = false;
		try
		{
			DangerousAddRef(ref success);
			fixed (byte* dest = &Unsafe.As<T, byte>(ref source))
			{
				Buffer.Memmove(dest, ptr, num);
				return source;
			}
		}
		finally
		{
			if (success)
			{
				DangerousRelease();
			}
		}
	}

	/// <summary>Reads the specified number of value types from memory starting at the offset, and writes them into an array starting at the index. </summary>
	/// <param name="byteOffset">The location from which to start reading.</param>
	/// <param name="array">The output array to write to.</param>
	/// <param name="index">The location in the output array to begin writing to.</param>
	/// <param name="count">The number of value types to read from the input array and to write to the output array.</param>
	/// <typeparam name="T">The value type to read.</typeparam>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or-<paramref name="count" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The length of the array minus the index is less than <paramref name="count" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="Overload:System.Runtime.InteropServices.SafeBuffer.Initialize" /> method has not been called.</exception>
	[CLSCompliant(false)]
	public unsafe void ReadArray<T>(ulong byteOffset, T[] array, int index, int count) where T : struct
	{
		if (array == null)
		{
			throw new ArgumentNullException("array", "Buffer cannot be null.");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", "Non-negative number required.");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Non-negative number required.");
		}
		if (array.Length - index < count)
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		if (_numBytes == Uninitialized)
		{
			throw NotInitialized();
		}
		uint num = SizeOf<T>();
		uint num2 = AlignedSizeOf<T>();
		byte* ptr = (byte*)(void*)handle + byteOffset;
		SpaceCheck(ptr, checked((ulong)(num2 * count)));
		bool success = false;
		try
		{
			DangerousAddRef(ref success);
			if (count <= 0)
			{
				return;
			}
			fixed (byte* ptr2 = &Unsafe.As<T, byte>(ref array[index]))
			{
				for (int i = 0; i < count; i++)
				{
					Buffer.Memmove(ptr2 + num * i, ptr + num2 * i, num);
				}
			}
		}
		finally
		{
			if (success)
			{
				DangerousRelease();
			}
		}
	}

	/// <summary>Writes a value type to memory at the given location.</summary>
	/// <param name="byteOffset">The location at which to start writing. You may have to consider alignment issues.</param>
	/// <param name="value">The value to write.</param>
	/// <typeparam name="T">The value type to write.</typeparam>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="Overload:System.Runtime.InteropServices.SafeBuffer.Initialize" /> method has not been called.</exception>
	[CLSCompliant(false)]
	public unsafe void Write<T>(ulong byteOffset, T value) where T : struct
	{
		if (_numBytes == Uninitialized)
		{
			throw NotInitialized();
		}
		uint num = SizeOf<T>();
		byte* ptr = (byte*)(void*)handle + byteOffset;
		SpaceCheck(ptr, num);
		bool success = false;
		try
		{
			DangerousAddRef(ref success);
			fixed (byte* src = &Unsafe.As<T, byte>(ref value))
			{
				Buffer.Memmove(ptr, src, num);
			}
		}
		finally
		{
			if (success)
			{
				DangerousRelease();
			}
		}
	}

	/// <summary>Writes the specified number of value types to a memory location by reading bytes starting from the specified location in the input array.</summary>
	/// <param name="byteOffset">The location in memory to write to.</param>
	/// <param name="array">The input array.</param>
	/// <param name="index">The offset in the array to start reading from.</param>
	/// <param name="count">The number of value types to write.</param>
	/// <typeparam name="T">The value type to write.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">The length of the input array minus <paramref name="index" /> is less than <paramref name="count" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="Overload:System.Runtime.InteropServices.SafeBuffer.Initialize" /> method has not been called.</exception>
	[CLSCompliant(false)]
	public unsafe void WriteArray<T>(ulong byteOffset, T[] array, int index, int count) where T : struct
	{
		if (array == null)
		{
			throw new ArgumentNullException("array", "Buffer cannot be null.");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", "Non-negative number required.");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Non-negative number required.");
		}
		if (array.Length - index < count)
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		if (_numBytes == Uninitialized)
		{
			throw NotInitialized();
		}
		uint num = SizeOf<T>();
		uint num2 = AlignedSizeOf<T>();
		byte* ptr = (byte*)(void*)handle + byteOffset;
		SpaceCheck(ptr, checked((ulong)(num2 * count)));
		bool success = false;
		try
		{
			DangerousAddRef(ref success);
			if (count <= 0)
			{
				return;
			}
			fixed (byte* ptr2 = &Unsafe.As<T, byte>(ref array[index]))
			{
				for (int i = 0; i < count; i++)
				{
					Buffer.Memmove(ptr + num2 * i, ptr2 + num * i, num);
				}
			}
		}
		finally
		{
			if (success)
			{
				DangerousRelease();
			}
		}
	}

	private unsafe void SpaceCheck(byte* ptr, ulong sizeInBytes)
	{
		if ((ulong)_numBytes < sizeInBytes)
		{
			NotEnoughRoom();
		}
		if ((ulong)(ptr - (byte*)(void*)handle) > (ulong)_numBytes - sizeInBytes)
		{
			NotEnoughRoom();
		}
	}

	private static void NotEnoughRoom()
	{
		throw new ArgumentException("Not enough space available in the buffer.");
	}

	private static InvalidOperationException NotInitialized()
	{
		return new InvalidOperationException("You must call Initialize on this object instance before using it.");
	}

	internal static uint AlignedSizeOf<T>() where T : struct
	{
		uint num = SizeOf<T>();
		if (num == 1 || num == 2)
		{
			return num;
		}
		return (uint)((num + 3) & -4);
	}

	internal static uint SizeOf<T>() where T : struct
	{
		if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
		{
			throw new ArgumentException("The specified Type must be a struct containing no references.");
		}
		return (uint)Unsafe.SizeOf<T>();
	}
}
