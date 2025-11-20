using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.IO;

/// <summary>Provides random access to unmanaged blocks of memory from managed code.</summary>
public class UnmanagedMemoryAccessor : IDisposable
{
	private SafeBuffer _buffer;

	private long _offset;

	private long _capacity;

	private FileAccess _access;

	private bool _isOpen;

	private bool _canRead;

	private bool _canWrite;

	/// <summary>Gets the capacity of the accessor.</summary>
	/// <returns>The capacity of the accessor.</returns>
	public long Capacity => _capacity;

	/// <summary>Determines whether the accessor is readable.</summary>
	/// <returns>true if the accessor is readable; otherwise, false. </returns>
	public bool CanRead
	{
		get
		{
			if (_isOpen)
			{
				return _canRead;
			}
			return false;
		}
	}

	/// <summary>Determines whether the accessory is writable.</summary>
	/// <returns>true if the accessor is writable; otherwise, false. </returns>
	public bool CanWrite
	{
		get
		{
			if (_isOpen)
			{
				return _canWrite;
			}
			return false;
		}
	}

	/// <summary>Determines whether the accessor is currently open by a process.</summary>
	/// <returns>true if the accessor is open; otherwise, false. </returns>
	protected bool IsOpen => _isOpen;

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.UnmanagedMemoryAccessor" /> class. </summary>
	protected UnmanagedMemoryAccessor()
	{
		_isOpen = false;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.UnmanagedMemoryAccessor" /> class with a specified buffer, offset, and capacity.</summary>
	/// <param name="buffer">The buffer to contain the accessor.</param>
	/// <param name="offset">The byte at which to start the accessor.</param>
	/// <param name="capacity">The size, in bytes, of memory to allocate.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="offset" /> plus <paramref name="capacity" /> is greater than <paramref name="buffer" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="capacity" /> is less than zero.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="offset" /> plus <paramref name="capacity" /> would wrap around the high end of the address space.</exception>
	public UnmanagedMemoryAccessor(SafeBuffer buffer, long offset, long capacity)
	{
		Initialize(buffer, offset, capacity, FileAccess.Read);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.UnmanagedMemoryAccessor" /> class with a specified buffer, offset, capacity, and access right.</summary>
	/// <param name="buffer">The buffer to contain the accessor.</param>
	/// <param name="offset">The byte at which to start the accessor.</param>
	/// <param name="capacity">The size, in bytes, of memory to allocate.</param>
	/// <param name="access">The type of access allowed to the memory. The default is <see cref="F:System.IO.MemoryMappedFiles.MemoryMappedFileAccess.ReadWrite" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="offset" /> plus <paramref name="capacity" /> is greater than <paramref name="buffer" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="capacity" /> is less than zero.-or-<paramref name="access" /> is not a valid <see cref="T:System.IO.MemoryMappedFiles.MemoryMappedFileAccess" /> enumeration value.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="offset" /> plus <paramref name="capacity" /> would wrap around the high end of the address space.</exception>
	public UnmanagedMemoryAccessor(SafeBuffer buffer, long offset, long capacity, FileAccess access)
	{
		Initialize(buffer, offset, capacity, access);
	}

	/// <summary>Sets the initial values for the accessor.</summary>
	/// <param name="buffer">The buffer to contain the accessor.</param>
	/// <param name="offset">The byte at which to start the accessor.</param>
	/// <param name="capacity">The size, in bytes, of memory to allocate.</param>
	/// <param name="access">The type of access allowed to the memory. The default is <see cref="F:System.IO.MemoryMappedFiles.MemoryMappedFileAccess.ReadWrite" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="offset" /> plus <paramref name="capacity" /> is greater than <paramref name="buffer" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="capacity" /> is less than zero.-or-<paramref name="access" /> is not a valid <see cref="T:System.IO.MemoryMappedFiles.MemoryMappedFileAccess" /> enumeration value.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="offset" /> plus <paramref name="capacity" /> would wrap around the high end of the address space.</exception>
	protected unsafe void Initialize(SafeBuffer buffer, long offset, long capacity, FileAccess access)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "Non-negative number required.");
		}
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException("capacity", "Non-negative number required.");
		}
		if (buffer.ByteLength < (ulong)(offset + capacity))
		{
			throw new ArgumentException("Offset and capacity were greater than the size of the view.");
		}
		if (access < FileAccess.Read || access > FileAccess.ReadWrite)
		{
			throw new ArgumentOutOfRangeException("access");
		}
		if (_isOpen)
		{
			throw new InvalidOperationException("The method cannot be called twice on the same instance.");
		}
		byte* pointer = null;
		try
		{
			buffer.AcquirePointer(ref pointer);
			if ((nuint)((long)pointer + offset + capacity) < (nuint)pointer)
			{
				throw new ArgumentException("The UnmanagedMemoryAccessor capacity and offset would wrap around the high end of the address space.");
			}
		}
		finally
		{
			if (pointer != null)
			{
				buffer.ReleasePointer();
			}
		}
		_offset = offset;
		_buffer = buffer;
		_capacity = capacity;
		_access = access;
		_isOpen = true;
		_canRead = (_access & FileAccess.Read) != 0;
		_canWrite = (_access & FileAccess.Write) != 0;
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.IO.UnmanagedMemoryAccessor" /> and optionally releases the managed resources. </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected virtual void Dispose(bool disposing)
	{
		_isOpen = false;
	}

	/// <summary>Releases all resources used by the <see cref="T:System.IO.UnmanagedMemoryAccessor" />. </summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Reads a Boolean value from the accessor.</summary>
	/// <returns>true or false.</returns>
	/// <param name="position">The number of bytes into the accessor at which to begin reading. </param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to read a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	public bool ReadBoolean(long position)
	{
		return ReadByte(position) != 0;
	}

	/// <summary>Reads a byte value from the accessor.</summary>
	/// <returns>The value that was read.</returns>
	/// <param name="position">The number of bytes into the accessor at which to begin reading.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to read a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	public unsafe byte ReadByte(long position)
	{
		EnsureSafeToRead(position, 1);
		byte* pointer = null;
		try
		{
			_buffer.AcquirePointer(ref pointer);
			return (pointer + _offset)[position];
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	/// <summary>Reads a character from the accessor.</summary>
	/// <returns>The value that was read.</returns>
	/// <param name="position">The number of bytes into the accessor at which to begin reading.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to read a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	public char ReadChar(long position)
	{
		return (char)ReadInt16(position);
	}

	/// <summary>Reads a 16-bit integer from the accessor.</summary>
	/// <returns>The value that was read.</returns>
	/// <param name="position">The number of bytes into the accessor at which to begin reading.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to read a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	public unsafe short ReadInt16(long position)
	{
		EnsureSafeToRead(position, 2);
		byte* pointer = null;
		try
		{
			_buffer.AcquirePointer(ref pointer);
			return Unsafe.ReadUnaligned<short>(pointer + _offset + position);
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	/// <summary>Reads a 32-bit integer from the accessor.</summary>
	/// <returns>The value that was read.</returns>
	/// <param name="position">The number of bytes into the accessor at which to begin reading.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to read a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	public unsafe int ReadInt32(long position)
	{
		EnsureSafeToRead(position, 4);
		byte* pointer = null;
		try
		{
			_buffer.AcquirePointer(ref pointer);
			return Unsafe.ReadUnaligned<int>(pointer + _offset + position);
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	/// <summary>Reads a 64-bit integer from the accessor.</summary>
	/// <returns>The value that was read.</returns>
	/// <param name="position">The number of bytes into the accessor at which to begin reading.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to read a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	public unsafe long ReadInt64(long position)
	{
		EnsureSafeToRead(position, 8);
		byte* pointer = null;
		try
		{
			_buffer.AcquirePointer(ref pointer);
			return Unsafe.ReadUnaligned<long>(pointer + _offset + position);
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	/// <summary>Reads a decimal value from the accessor.</summary>
	/// <returns>The value that was read.</returns>
	/// <param name="position">The number of bytes into the accessor at which to begin reading.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to read a value.-or-The decimal to read is invalid.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	public unsafe decimal ReadDecimal(long position)
	{
		EnsureSafeToRead(position, 16);
		byte* pointer = null;
		int lo;
		int mid;
		int hi;
		int num;
		try
		{
			_buffer.AcquirePointer(ref pointer);
			pointer += _offset + position;
			lo = Unsafe.ReadUnaligned<int>(pointer);
			mid = Unsafe.ReadUnaligned<int>(pointer + 4);
			hi = Unsafe.ReadUnaligned<int>(pointer + 8);
			num = Unsafe.ReadUnaligned<int>(pointer + 12);
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
		if ((num & 0x7F00FFFF) != 0 || (num & 0xFF0000) > 1835008)
		{
			throw new ArgumentException("Read an invalid decimal value from the buffer.");
		}
		bool isNegative = (num & int.MinValue) != 0;
		byte scale = (byte)(num >> 16);
		return new decimal(lo, mid, hi, isNegative, scale);
	}

	/// <summary>Reads a single-precision floating-point value from the accessor.</summary>
	/// <returns>The value that was read.</returns>
	/// <param name="position">The number of bytes into the accessor at which to begin reading.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to read a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	public float ReadSingle(long position)
	{
		return BitConverter.Int32BitsToSingle(ReadInt32(position));
	}

	/// <summary>Reads a double-precision floating-point value from the accessor.</summary>
	/// <returns>The value that was read.</returns>
	/// <param name="position">The number of bytes into the accessor at which to begin reading.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to read a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	public double ReadDouble(long position)
	{
		return BitConverter.Int64BitsToDouble(ReadInt64(position));
	}

	/// <summary>Reads an 8-bit signed integer from the accessor.</summary>
	/// <returns>The value that was read.</returns>
	/// <param name="position">The number of bytes into the accessor at which to begin reading.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to read a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[CLSCompliant(false)]
	public sbyte ReadSByte(long position)
	{
		return (sbyte)ReadByte(position);
	}

	/// <summary>Reads an unsigned 16-bit integer from the accessor.</summary>
	/// <returns>The value that was read.</returns>
	/// <param name="position">The number of bytes into the accessor at which to begin reading.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to read a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[CLSCompliant(false)]
	public ushort ReadUInt16(long position)
	{
		return (ushort)ReadInt16(position);
	}

	/// <summary>Reads an unsigned 32-bit integer from the accessor.</summary>
	/// <returns>The value that was read.</returns>
	/// <param name="position">The number of bytes into the accessor at which to begin reading.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to read a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[CLSCompliant(false)]
	public uint ReadUInt32(long position)
	{
		return (uint)ReadInt32(position);
	}

	/// <summary>Reads an unsigned 64-bit integer from the accessor.</summary>
	/// <returns>The value that was read.</returns>
	/// <param name="position">The number of bytes into the accessor at which to begin reading.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to read a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[CLSCompliant(false)]
	public ulong ReadUInt64(long position)
	{
		return (ulong)ReadInt64(position);
	}

	/// <summary>Reads a structure of type <paramref name="T" /> from the accessor into a provided reference.</summary>
	/// <param name="position">The position in the accessor at which to begin reading.</param>
	/// <param name="structure">The structure to contain the read data.</param>
	/// <typeparam name="T">The type of structure.</typeparam>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to read in a structure of type <paramref name="T" />.-or-T is a value type that contains one or more reference types.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	public void Read<T>(long position, out T structure) where T : struct
	{
		if (position < 0)
		{
			throw new ArgumentOutOfRangeException("position", "Non-negative number required.");
		}
		if (!_isOpen)
		{
			throw new ObjectDisposedException("UnmanagedMemoryAccessor", "Cannot access a closed accessor.");
		}
		if (!_canRead)
		{
			throw new NotSupportedException("Accessor does not support reading.");
		}
		uint num = SafeBuffer.SizeOf<T>();
		if (position > _capacity - num)
		{
			if (position >= _capacity)
			{
				throw new ArgumentOutOfRangeException("position", "The position may not be greater or equal to the capacity of the accessor.");
			}
			throw new ArgumentException(SR.Format("There are not enough bytes remaining in the accessor to read at this position.", typeof(T)), "position");
		}
		structure = _buffer.Read<T>((ulong)(_offset + position));
	}

	/// <summary>Reads structures of type <paramref name="T" /> from the accessor into an array of type <paramref name="T" />.</summary>
	/// <returns>The number of structures read into <paramref name="array" />. This value can be less than <paramref name="count" /> if there are fewer structures available, or zero if the end of the accessor is reached.</returns>
	/// <param name="position">The number of bytes in the accessor at which to begin reading.</param>
	/// <param name="array">The array to contain the structures read from the accessor.</param>
	/// <param name="offset">The index in <paramref name="array" /> in which to place the first copied structure. </param>
	/// <param name="count">The number of structures of type <paramref name="T" /> to read from the accessor.</param>
	/// <typeparam name="T">The type of structure.</typeparam>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is not large enough to contain <paramref name="count" /> of structures (starting from <paramref name="position" />). </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	public int ReadArray<T>(long position, T[] array, int offset, int count) where T : struct
	{
		if (array == null)
		{
			throw new ArgumentNullException("array", "Buffer cannot be null.");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "Non-negative number required.");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Non-negative number required.");
		}
		if (array.Length - offset < count)
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		if (!_isOpen)
		{
			throw new ObjectDisposedException("UnmanagedMemoryAccessor", "Cannot access a closed accessor.");
		}
		if (!_canRead)
		{
			throw new NotSupportedException("Accessor does not support reading.");
		}
		if (position < 0)
		{
			throw new ArgumentOutOfRangeException("position", "Non-negative number required.");
		}
		uint num = SafeBuffer.AlignedSizeOf<T>();
		if (position >= _capacity)
		{
			throw new ArgumentOutOfRangeException("position", "The position may not be greater or equal to the capacity of the accessor.");
		}
		int num2 = count;
		long num3 = _capacity - position;
		if (num3 < 0)
		{
			num2 = 0;
		}
		else
		{
			ulong num4 = (ulong)(num * count);
			if ((ulong)num3 < num4)
			{
				num2 = (int)(num3 / num);
			}
		}
		_buffer.ReadArray((ulong)(_offset + position), array, offset, num2);
		return num2;
	}

	/// <summary>Writes a Boolean value into the accessor.</summary>
	/// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
	/// <param name="value">The value to write.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to write a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	public void Write(long position, bool value)
	{
		Write(position, (byte)(value ? 1u : 0u));
	}

	/// <summary>Writes a byte value into the accessor.</summary>
	/// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
	/// <param name="value">The value to write.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to write a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	public unsafe void Write(long position, byte value)
	{
		EnsureSafeToWrite(position, 1);
		byte* pointer = null;
		try
		{
			_buffer.AcquirePointer(ref pointer);
			(pointer + _offset)[position] = value;
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	/// <summary>Writes a character into the accessor.</summary>
	/// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
	/// <param name="value">The value to write.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to write a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	public void Write(long position, char value)
	{
		Write(position, (short)value);
	}

	/// <summary>Writes a 16-bit integer into the accessor.</summary>
	/// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
	/// <param name="value">The value to write.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to write a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	public unsafe void Write(long position, short value)
	{
		EnsureSafeToWrite(position, 2);
		byte* pointer = null;
		try
		{
			_buffer.AcquirePointer(ref pointer);
			Unsafe.WriteUnaligned(pointer + _offset + position, value);
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	/// <summary>Writes a 32-bit integer into the accessor.</summary>
	/// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
	/// <param name="value">The value to write.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to write a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	public unsafe void Write(long position, int value)
	{
		EnsureSafeToWrite(position, 4);
		byte* pointer = null;
		try
		{
			_buffer.AcquirePointer(ref pointer);
			Unsafe.WriteUnaligned(pointer + _offset + position, value);
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	/// <summary>Writes a 64-bit integer into the accessor.</summary>
	/// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
	/// <param name="value">The value to write.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after position to write a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	public unsafe void Write(long position, long value)
	{
		EnsureSafeToWrite(position, 8);
		byte* pointer = null;
		try
		{
			_buffer.AcquirePointer(ref pointer);
			Unsafe.WriteUnaligned(pointer + _offset + position, value);
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	/// <summary>Writes a decimal value into the accessor.</summary>
	/// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
	/// <param name="value">The value to write.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to write a value.-or-The decimal is invalid.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	public unsafe void Write(long position, decimal value)
	{
		EnsureSafeToWrite(position, 16);
		int* ptr = (int*)(&value);
		int value2 = *ptr;
		int value3 = ptr[1];
		int value4 = ptr[2];
		int value5 = ptr[3];
		byte* pointer = null;
		try
		{
			_buffer.AcquirePointer(ref pointer);
			pointer += _offset + position;
			Unsafe.WriteUnaligned(pointer, value4);
			Unsafe.WriteUnaligned(pointer + 4, value5);
			Unsafe.WriteUnaligned(pointer + 8, value3);
			Unsafe.WriteUnaligned(pointer + 12, value2);
		}
		finally
		{
			if (pointer != null)
			{
				_buffer.ReleasePointer();
			}
		}
	}

	/// <summary>Writes a Single into the accessor.</summary>
	/// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
	/// <param name="value">The value to write.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to write a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	public void Write(long position, float value)
	{
		Write(position, BitConverter.SingleToInt32Bits(value));
	}

	/// <summary>Writes a Double value into the accessor.</summary>
	/// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
	/// <param name="value">The value to write.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to write a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	public void Write(long position, double value)
	{
		Write(position, BitConverter.DoubleToInt64Bits(value));
	}

	/// <summary>Writes an 8-bit integer into the accessor.</summary>
	/// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
	/// <param name="value">The value to write.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to write a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[CLSCompliant(false)]
	public void Write(long position, sbyte value)
	{
		Write(position, (byte)value);
	}

	/// <summary>Writes an unsigned 16-bit integer into the accessor.</summary>
	/// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
	/// <param name="value">The value to write.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to write a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[CLSCompliant(false)]
	public void Write(long position, ushort value)
	{
		Write(position, (short)value);
	}

	/// <summary>Writes an unsigned 32-bit integer into the accessor.</summary>
	/// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
	/// <param name="value">The value to write.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to write a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[CLSCompliant(false)]
	public void Write(long position, uint value)
	{
		Write(position, (int)value);
	}

	/// <summary>Writes an unsigned 64-bit integer into the accessor.</summary>
	/// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
	/// <param name="value">The value to write.</param>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes after <paramref name="position" /> to write a value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	[CLSCompliant(false)]
	public void Write(long position, ulong value)
	{
		Write(position, (long)value);
	}

	/// <summary>Writes a structure into the accessor.</summary>
	/// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
	/// <param name="structure">The structure to write.</param>
	/// <typeparam name="T">The type of structure.</typeparam>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes in the accessor after <paramref name="position" /> to write a structure of type <paramref name="T" />.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	public void Write<T>(long position, ref T structure) where T : struct
	{
		if (position < 0)
		{
			throw new ArgumentOutOfRangeException("position", "Non-negative number required.");
		}
		if (!_isOpen)
		{
			throw new ObjectDisposedException("UnmanagedMemoryAccessor", "Cannot access a closed accessor.");
		}
		if (!_canWrite)
		{
			throw new NotSupportedException("Accessor does not support writing.");
		}
		uint num = SafeBuffer.SizeOf<T>();
		if (position > _capacity - num)
		{
			if (position >= _capacity)
			{
				throw new ArgumentOutOfRangeException("position", "The position may not be greater or equal to the capacity of the accessor.");
			}
			throw new ArgumentException(SR.Format("There are not enough bytes remaining in the accessor to write at this position.", typeof(T)), "position");
		}
		_buffer.Write((ulong)(_offset + position), structure);
	}

	/// <summary>Writes structures from an array of type <paramref name="T" /> into the accessor.</summary>
	/// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
	/// <param name="array">The array to write into the accessor.</param>
	/// <param name="offset">The index in <paramref name="array" /> to start writing from.</param>
	/// <param name="count">The number of structures in <paramref name="array" /> to write.</param>
	/// <typeparam name="T">The type of structure.</typeparam>
	/// <exception cref="T:System.ArgumentException">There are not enough bytes in the accessor after <paramref name="position" /> to write the number of structures specified by <paramref name="count" />.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="position" /> is less than zero or greater than the capacity of the accessor.-or-<paramref name="offset" /> or <paramref name="count" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">The accessor does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The accessor has been disposed.</exception>
	public void WriteArray<T>(long position, T[] array, int offset, int count) where T : struct
	{
		if (array == null)
		{
			throw new ArgumentNullException("array", "Buffer cannot be null.");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "Non-negative number required.");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Non-negative number required.");
		}
		if (array.Length - offset < count)
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		if (position < 0)
		{
			throw new ArgumentOutOfRangeException("position", "Non-negative number required.");
		}
		if (position >= Capacity)
		{
			throw new ArgumentOutOfRangeException("position", "The position may not be greater or equal to the capacity of the accessor.");
		}
		if (!_isOpen)
		{
			throw new ObjectDisposedException("UnmanagedMemoryAccessor", "Cannot access a closed accessor.");
		}
		if (!_canWrite)
		{
			throw new NotSupportedException("Accessor does not support writing.");
		}
		_buffer.WriteArray((ulong)(_offset + position), array, offset, count);
	}

	private void EnsureSafeToRead(long position, int sizeOfType)
	{
		if (!_isOpen)
		{
			throw new ObjectDisposedException("UnmanagedMemoryAccessor", "Cannot access a closed accessor.");
		}
		if (!_canRead)
		{
			throw new NotSupportedException("Accessor does not support reading.");
		}
		if (position < 0)
		{
			throw new ArgumentOutOfRangeException("position", "Non-negative number required.");
		}
		if (position > _capacity - sizeOfType)
		{
			if (position >= _capacity)
			{
				throw new ArgumentOutOfRangeException("position", "The position may not be greater or equal to the capacity of the accessor.");
			}
			throw new ArgumentException("There are not enough bytes remaining in the accessor to read at this position.", "position");
		}
	}

	private void EnsureSafeToWrite(long position, int sizeOfType)
	{
		if (!_isOpen)
		{
			throw new ObjectDisposedException("UnmanagedMemoryAccessor", "Cannot access a closed accessor.");
		}
		if (!_canWrite)
		{
			throw new NotSupportedException("Accessor does not support writing.");
		}
		if (position < 0)
		{
			throw new ArgumentOutOfRangeException("position", "Non-negative number required.");
		}
		if (position > _capacity - sizeOfType)
		{
			if (position >= _capacity)
			{
				throw new ArgumentOutOfRangeException("position", "The position may not be greater or equal to the capacity of the accessor.");
			}
			throw new ArgumentException("There are not enough bytes remaining in the accessor to write at this position.", "position");
		}
	}
}
