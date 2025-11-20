using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO;

/// <summary>Provides access to unmanaged blocks of memory from managed code.</summary>
/// <filterpriority>2</filterpriority>
public class UnmanagedMemoryStream : Stream
{
	private SafeBuffer _buffer;

	private unsafe byte* _mem;

	private long _length;

	private long _capacity;

	private long _position;

	private long _offset;

	private FileAccess _access;

	internal bool _isOpen;

	private Task<int> _lastReadTask;

	/// <summary>Gets a value indicating whether a stream supports reading.</summary>
	/// <returns>false if the object was created by a constructor with an <paramref name="access" /> parameter that did not include reading the stream and if the stream is closed; otherwise, true.</returns>
	/// <filterpriority>2</filterpriority>
	public override bool CanRead
	{
		get
		{
			if (_isOpen)
			{
				return (_access & FileAccess.Read) != 0;
			}
			return false;
		}
	}

	/// <summary>Gets a value indicating whether a stream supports seeking.</summary>
	/// <returns>false if the stream is closed; otherwise, true.</returns>
	/// <filterpriority>2</filterpriority>
	public override bool CanSeek => _isOpen;

	/// <summary>Gets a value indicating whether a stream supports writing.</summary>
	/// <returns>false if the object was created by a constructor with an <paramref name="access" /> parameter value that supports writing or was created by a constructor that had no parameters, or if the stream is closed; otherwise, true.</returns>
	/// <filterpriority>2</filterpriority>
	public override bool CanWrite
	{
		get
		{
			if (_isOpen)
			{
				return (_access & FileAccess.Write) != 0;
			}
			return false;
		}
	}

	/// <summary>Gets the length of the data in a stream.</summary>
	/// <returns>The length of the data in the stream.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
	/// <filterpriority>2</filterpriority>
	public override long Length
	{
		get
		{
			EnsureNotClosed();
			return Interlocked.Read(ref _length);
		}
	}

	/// <summary>Gets the stream length (size) or the total amount of memory assigned to a stream (capacity).</summary>
	/// <returns>The size or capacity of the stream.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
	/// <filterpriority>2</filterpriority>
	public long Capacity
	{
		get
		{
			EnsureNotClosed();
			return _capacity;
		}
	}

	/// <summary>Gets or sets the current position in a stream.</summary>
	/// <returns>The current position in the stream.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The position is set to a value that is less than zero, or the position is larger than <see cref="F:System.Int32.MaxValue" /> or results in overflow when added to the current pointer.</exception>
	/// <filterpriority>2</filterpriority>
	public override long Position
	{
		get
		{
			if (!CanSeek)
			{
				throw Error.GetStreamIsClosed();
			}
			return Interlocked.Read(ref _position);
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value", "Non-negative number required.");
			}
			if (!CanSeek)
			{
				throw Error.GetStreamIsClosed();
			}
			Interlocked.Exchange(ref _position, value);
		}
	}

	/// <summary>Gets or sets a byte pointer to a stream based on the current position in the stream.</summary>
	/// <returns>A byte pointer.</returns>
	/// <exception cref="T:System.IndexOutOfRangeException">The current position is larger than the capacity of the stream.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The position is being set is not a valid position in the current stream.</exception>
	/// <exception cref="T:System.IO.IOException">The pointer is being set to a lower value than the starting position of the stream.</exception>
	/// <exception cref="T:System.NotSupportedException">The stream was initialized for use with a <see cref="T:System.Runtime.InteropServices.SafeBuffer" />. The <see cref="P:System.IO.UnmanagedMemoryStream.PositionPointer" /> property is valid only for streams that are initialized with a <see cref="T:System.Byte" /> pointer. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[CLSCompliant(false)]
	public unsafe byte* PositionPointer
	{
		get
		{
			if (_buffer != null)
			{
				throw new NotSupportedException("This operation is not supported for an UnmanagedMemoryStream created from a SafeBuffer.");
			}
			EnsureNotClosed();
			long num = Interlocked.Read(ref _position);
			if (num > _capacity)
			{
				throw new IndexOutOfRangeException("Unmanaged memory stream position was beyond the capacity of the stream.");
			}
			return _mem + num;
		}
		set
		{
			if (_buffer != null)
			{
				throw new NotSupportedException("This operation is not supported for an UnmanagedMemoryStream created from a SafeBuffer.");
			}
			EnsureNotClosed();
			if (value < _mem)
			{
				throw new IOException("An attempt was made to move the position before the beginning of the stream.");
			}
			long num = (long)value - (long)_mem;
			if (num < 0)
			{
				throw new ArgumentOutOfRangeException("offset", "UnmanagedMemoryStream length must be non-negative and less than 2^63 - 1 - baseAddress.");
			}
			Interlocked.Exchange(ref _position, num);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.UnmanagedMemoryStream" /> class.</summary>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the required permission.</exception>
	protected unsafe UnmanagedMemoryStream()
	{
		_mem = null;
		_isOpen = false;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.UnmanagedMemoryStream" /> class in a safe buffer with a specified offset and length. </summary>
	/// <param name="buffer">The buffer to contain the unmanaged memory stream.</param>
	/// <param name="offset">The byte position in the buffer at which to start the unmanaged memory stream.</param>
	/// <param name="length">The length of the unmanaged memory stream.</param>
	public UnmanagedMemoryStream(SafeBuffer buffer, long offset, long length)
	{
		Initialize(buffer, offset, length, FileAccess.Read);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.UnmanagedMemoryStream" /> class in a safe buffer with a specified offset, length, and file access. </summary>
	/// <param name="buffer">The buffer to contain the unmanaged memory stream.</param>
	/// <param name="offset">The byte position in the buffer at which to start the unmanaged memory stream.</param>
	/// <param name="length">The length of the unmanaged memory stream.</param>
	/// <param name="access">The mode of file access to the unmanaged memory stream. </param>
	public UnmanagedMemoryStream(SafeBuffer buffer, long offset, long length, FileAccess access)
	{
		Initialize(buffer, offset, length, access);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.UnmanagedMemoryStream" /> class in a safe buffer with a specified offset, length, and file access. </summary>
	/// <param name="buffer">The buffer to contain the unmanaged memory stream.</param>
	/// <param name="offset">The byte position in the buffer at which to start the unmanaged memory stream.</param>
	/// <param name="length">The length of the unmanaged memory stream.</param>
	/// <param name="access">The mode of file access to the unmanaged memory stream.</param>
	protected unsafe void Initialize(SafeBuffer buffer, long offset, long length, FileAccess access)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "Non-negative number required.");
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", "Non-negative number required.");
		}
		if (buffer.ByteLength < (ulong)(offset + length))
		{
			throw new ArgumentException("Offset and length were greater than the size of the SafeBuffer.");
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
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			buffer.AcquirePointer(ref pointer);
			if (pointer + offset + length < pointer)
			{
				throw new ArgumentException("The UnmanagedMemoryStream capacity would wrap around the high end of the address space.");
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
		_length = length;
		_capacity = length;
		_access = access;
		_isOpen = true;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.UnmanagedMemoryStream" /> class using the specified location and memory length.</summary>
	/// <param name="pointer">A pointer to an unmanaged memory location.</param>
	/// <param name="length">The length of the memory to use.</param>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the required permission.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="pointer" /> value is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="length" /> value is less than zero.- or -The <paramref name="length" /> is large enough to cause an overflow.</exception>
	[CLSCompliant(false)]
	public unsafe UnmanagedMemoryStream(byte* pointer, long length)
	{
		Initialize(pointer, length, length, FileAccess.Read);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.UnmanagedMemoryStream" /> class using the specified location, memory length, total amount of memory, and file access values.</summary>
	/// <param name="pointer">A pointer to an unmanaged memory location.</param>
	/// <param name="length">The length of the memory to use.</param>
	/// <param name="capacity">The total amount of memory assigned to the stream.</param>
	/// <param name="access">One of the <see cref="T:System.IO.FileAccess" /> values.</param>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the required permission.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="pointer" /> value is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="length" /> value is less than zero.- or - The <paramref name="capacity" /> value is less than zero.- or -The <paramref name="length" /> value is greater than the <paramref name="capacity" /> value.</exception>
	[CLSCompliant(false)]
	public unsafe UnmanagedMemoryStream(byte* pointer, long length, long capacity, FileAccess access)
	{
		Initialize(pointer, length, capacity, access);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.UnmanagedMemoryStream" /> class by using a pointer to an unmanaged memory location. </summary>
	/// <param name="pointer">A pointer to an unmanaged memory location.</param>
	/// <param name="length">The length of the memory to use.</param>
	/// <param name="capacity">The total amount of memory assigned to the stream.</param>
	/// <param name="access">One of the <see cref="T:System.IO.FileAccess" /> values. </param>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the required permission.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="pointer" /> value is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="length" /> value is less than zero.- or - The <paramref name="capacity" /> value is less than zero.- or -The <paramref name="length" /> value is large enough to cause an overflow.</exception>
	[CLSCompliant(false)]
	protected unsafe void Initialize(byte* pointer, long length, long capacity, FileAccess access)
	{
		if (pointer == null)
		{
			throw new ArgumentNullException("pointer");
		}
		if (length < 0 || capacity < 0)
		{
			throw new ArgumentOutOfRangeException((length < 0) ? "length" : "capacity", "Non-negative number required.");
		}
		if (length > capacity)
		{
			throw new ArgumentOutOfRangeException("length", "The length cannot be greater than the capacity.");
		}
		if ((nuint)((long)pointer + capacity) < (nuint)pointer)
		{
			throw new ArgumentOutOfRangeException("capacity", "The UnmanagedMemoryStream capacity would wrap around the high end of the address space.");
		}
		if (access < FileAccess.Read || access > FileAccess.ReadWrite)
		{
			throw new ArgumentOutOfRangeException("access", "Enum value was out of legal range.");
		}
		if (_isOpen)
		{
			throw new InvalidOperationException("The method cannot be called twice on the same instance.");
		}
		_mem = pointer;
		_offset = 0L;
		_length = length;
		_capacity = capacity;
		_access = access;
		_isOpen = true;
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.IO.UnmanagedMemoryStream" /> and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	protected unsafe override void Dispose(bool disposing)
	{
		_isOpen = false;
		_mem = null;
		base.Dispose(disposing);
	}

	private void EnsureNotClosed()
	{
		if (!_isOpen)
		{
			throw Error.GetStreamIsClosed();
		}
	}

	private void EnsureReadable()
	{
		if (!CanRead)
		{
			throw Error.GetReadNotSupported();
		}
	}

	private void EnsureWriteable()
	{
		if (!CanWrite)
		{
			throw Error.GetWriteNotSupported();
		}
	}

	/// <summary>Overrides the <see cref="M:System.IO.Stream.Flush" /> method so that no action is performed.</summary>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
	/// <filterpriority>2</filterpriority>
	public override void Flush()
	{
		EnsureNotClosed();
	}

	public override Task FlushAsync(CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return Task.FromCanceled(cancellationToken);
		}
		try
		{
			Flush();
			return Task.CompletedTask;
		}
		catch (Exception exception)
		{
			return Task.FromException(exception);
		}
	}

	/// <summary>Reads the specified number of bytes into the specified array.</summary>
	/// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
	/// <param name="buffer">When this method returns, contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source. This parameter is passed uninitialized.</param>
	/// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream.</param>
	/// <param name="count">The maximum number of bytes to read from the current stream.</param>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
	/// <exception cref="T:System.NotSupportedException">The underlying memory does not support reading.- or - The <see cref="P:System.IO.UnmanagedMemoryStream.CanRead" /> property is set to false. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="buffer" /> parameter is set to null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="offset" /> parameter is less than zero. - or - The <paramref name="count" /> parameter is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">The length of the buffer array minus the <paramref name="offset" /> parameter is less than the <paramref name="count" /> parameter.</exception>
	/// <filterpriority>2</filterpriority>
	public override int Read(byte[] buffer, int offset, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer", "Buffer cannot be null.");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "Non-negative number required.");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Non-negative number required.");
		}
		if (buffer.Length - offset < count)
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		return ReadCore(new Span<byte>(buffer, offset, count));
	}

	public override int Read(Span<byte> buffer)
	{
		if (GetType() == typeof(UnmanagedMemoryStream))
		{
			return ReadCore(buffer);
		}
		return base.Read(buffer);
	}

	internal unsafe int ReadCore(Span<byte> buffer)
	{
		EnsureNotClosed();
		EnsureReadable();
		long num = Interlocked.Read(ref _position);
		long num2 = Math.Min(Interlocked.Read(ref _length) - num, buffer.Length);
		if (num2 <= 0)
		{
			return 0;
		}
		int num3 = (int)num2;
		if (num3 < 0)
		{
			return 0;
		}
		fixed (byte* reference = &MemoryMarshal.GetReference(buffer))
		{
			if (_buffer != null)
			{
				byte* pointer = null;
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
					_buffer.AcquirePointer(ref pointer);
					Buffer.Memcpy(reference, pointer + num + _offset, num3);
				}
				finally
				{
					if (pointer != null)
					{
						_buffer.ReleasePointer();
					}
				}
			}
			else
			{
				Buffer.Memcpy(reference, _mem + num, num3);
			}
		}
		Interlocked.Exchange(ref _position, num + num2);
		return num3;
	}

	public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer", "Buffer cannot be null.");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "Non-negative number required.");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Non-negative number required.");
		}
		if (buffer.Length - offset < count)
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		if (cancellationToken.IsCancellationRequested)
		{
			return Task.FromCanceled<int>(cancellationToken);
		}
		try
		{
			int num = Read(buffer, offset, count);
			Task<int> lastReadTask = _lastReadTask;
			return (lastReadTask != null && lastReadTask.Result == num) ? lastReadTask : (_lastReadTask = Task.FromResult(num));
		}
		catch (Exception exception)
		{
			return Task.FromException<int>(exception);
		}
	}

	public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return new ValueTask<int>(Task.FromCanceled<int>(cancellationToken));
		}
		try
		{
			ArraySegment<byte> segment;
			return new ValueTask<int>(MemoryMarshal.TryGetArray((ReadOnlyMemory<byte>)buffer, out segment) ? Read(segment.Array, segment.Offset, segment.Count) : Read(buffer.Span));
		}
		catch (Exception exception)
		{
			return new ValueTask<int>(Task.FromException<int>(exception));
		}
	}

	/// <summary>Reads a byte from a stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.</summary>
	/// <returns>The unsigned byte cast to an <see cref="T:System.Int32" /> object, or -1 if at the end of the stream.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
	/// <exception cref="T:System.NotSupportedException">The underlying memory does not support reading.- or -The current position is at the end of the stream.</exception>
	/// <filterpriority>2</filterpriority>
	public unsafe override int ReadByte()
	{
		EnsureNotClosed();
		EnsureReadable();
		long num = Interlocked.Read(ref _position);
		long num2 = Interlocked.Read(ref _length);
		if (num >= num2)
		{
			return -1;
		}
		Interlocked.Exchange(ref _position, num + 1);
		if (_buffer != null)
		{
			byte* pointer = null;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				_buffer.AcquirePointer(ref pointer);
				return (pointer + num)[_offset];
			}
			finally
			{
				if (pointer != null)
				{
					_buffer.ReleasePointer();
				}
			}
		}
		return _mem[num];
	}

	/// <summary>Sets the current position of the current stream to the given value.</summary>
	/// <returns>The new position in the stream.</returns>
	/// <param name="offset">The point relative to <paramref name="origin" /> to begin seeking from. </param>
	/// <param name="loc">Specifies the beginning, the end, or the current position as a reference point for <paramref name="origin" />, using a value of type <see cref="T:System.IO.SeekOrigin" />. </param>
	/// <exception cref="T:System.IO.IOException">An attempt was made to seek before the beginning of the stream.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="offset" /> value is larger than the maximum size of the stream.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="loc" /> is invalid.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
	/// <filterpriority>2</filterpriority>
	public override long Seek(long offset, SeekOrigin loc)
	{
		EnsureNotClosed();
		switch (loc)
		{
		case SeekOrigin.Begin:
			if (offset < 0)
			{
				throw new IOException("An attempt was made to move the position before the beginning of the stream.");
			}
			Interlocked.Exchange(ref _position, offset);
			break;
		case SeekOrigin.Current:
		{
			long num2 = Interlocked.Read(ref _position);
			if (offset + num2 < 0)
			{
				throw new IOException("An attempt was made to move the position before the beginning of the stream.");
			}
			Interlocked.Exchange(ref _position, offset + num2);
			break;
		}
		case SeekOrigin.End:
		{
			long num = Interlocked.Read(ref _length);
			if (num + offset < 0)
			{
				throw new IOException("An attempt was made to move the position before the beginning of the stream.");
			}
			Interlocked.Exchange(ref _position, num + offset);
			break;
		}
		default:
			throw new ArgumentException("Invalid seek origin.");
		}
		return Interlocked.Read(ref _position);
	}

	/// <summary>Sets the length of a stream to a specified value.</summary>
	/// <param name="value">The length of the stream.</param>
	/// <exception cref="T:System.IO.IOException">An I/O error has occurred. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
	/// <exception cref="T:System.NotSupportedException">The underlying memory does not support writing.- or -An attempt is made to write to the stream and the <see cref="P:System.IO.UnmanagedMemoryStream.CanWrite" /> property is false.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The specified <paramref name="value" /> exceeds the capacity of the stream.- or -The specified <paramref name="value" /> is negative.</exception>
	/// <filterpriority>2</filterpriority>
	public unsafe override void SetLength(long value)
	{
		if (value < 0)
		{
			throw new ArgumentOutOfRangeException("value", "Non-negative number required.");
		}
		if (_buffer != null)
		{
			throw new NotSupportedException("This operation is not supported for an UnmanagedMemoryStream created from a SafeBuffer.");
		}
		EnsureNotClosed();
		EnsureWriteable();
		if (value > _capacity)
		{
			throw new IOException("Unable to expand length of this stream beyond its capacity.");
		}
		long num = Interlocked.Read(ref _position);
		long num2 = Interlocked.Read(ref _length);
		if (value > num2)
		{
			Buffer.ZeroMemory(_mem + num2, value - num2);
		}
		Interlocked.Exchange(ref _length, value);
		if (num > value)
		{
			Interlocked.Exchange(ref _position, value);
		}
	}

	/// <summary>Writes a block of bytes to the current stream using data from a buffer.</summary>
	/// <param name="buffer">The byte array from which to copy bytes to the current stream.</param>
	/// <param name="offset">The offset in the buffer at which to begin copying bytes to the current stream.</param>
	/// <param name="count">The number of bytes to write to the current stream.</param>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
	/// <exception cref="T:System.NotSupportedException">The underlying memory does not support writing. - or -An attempt is made to write to the stream and the <see cref="P:System.IO.UnmanagedMemoryStream.CanWrite" /> property is false.- or -The <paramref name="count" /> value is greater than the capacity of the stream.- or -The position is at the end of the stream capacity.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">One of the specified parameters is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="offset" /> parameter minus the length of the <paramref name="buffer" /> parameter is less than the <paramref name="count" /> parameter.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="buffer" /> parameter is null.</exception>
	/// <filterpriority>2</filterpriority>
	public override void Write(byte[] buffer, int offset, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer", "Buffer cannot be null.");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "Non-negative number required.");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Non-negative number required.");
		}
		if (buffer.Length - offset < count)
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		WriteCore(new Span<byte>(buffer, offset, count));
	}

	public override void Write(ReadOnlySpan<byte> buffer)
	{
		if (GetType() == typeof(UnmanagedMemoryStream))
		{
			WriteCore(buffer);
		}
		else
		{
			base.Write(buffer);
		}
	}

	internal unsafe void WriteCore(ReadOnlySpan<byte> buffer)
	{
		EnsureNotClosed();
		EnsureWriteable();
		long num = Interlocked.Read(ref _position);
		long num2 = Interlocked.Read(ref _length);
		long num3 = num + buffer.Length;
		if (num3 < 0)
		{
			throw new IOException("Stream was too long.");
		}
		if (num3 > _capacity)
		{
			throw new NotSupportedException("Unable to expand length of this stream beyond its capacity.");
		}
		if (_buffer == null)
		{
			if (num > num2)
			{
				Buffer.ZeroMemory(_mem + num2, num - num2);
			}
			if (num3 > num2)
			{
				Interlocked.Exchange(ref _length, num3);
			}
		}
		fixed (byte* reference = &MemoryMarshal.GetReference(buffer))
		{
			if (_buffer != null)
			{
				if (_capacity - num < buffer.Length)
				{
					throw new ArgumentException("Not enough space available in the buffer.");
				}
				byte* pointer = null;
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
					_buffer.AcquirePointer(ref pointer);
					Buffer.Memcpy(pointer + num + _offset, reference, buffer.Length);
				}
				finally
				{
					if (pointer != null)
					{
						_buffer.ReleasePointer();
					}
				}
			}
			else
			{
				Buffer.Memcpy(_mem + num, reference, buffer.Length);
			}
		}
		Interlocked.Exchange(ref _position, num3);
	}

	public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer", "Buffer cannot be null.");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "Non-negative number required.");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Non-negative number required.");
		}
		if (buffer.Length - offset < count)
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		if (cancellationToken.IsCancellationRequested)
		{
			return Task.FromCanceled(cancellationToken);
		}
		try
		{
			Write(buffer, offset, count);
			return Task.CompletedTask;
		}
		catch (Exception exception)
		{
			return Task.FromException(exception);
		}
	}

	public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return new ValueTask(Task.FromCanceled(cancellationToken));
		}
		try
		{
			if (MemoryMarshal.TryGetArray(buffer, out var segment))
			{
				Write(segment.Array, segment.Offset, segment.Count);
			}
			else
			{
				Write(buffer.Span);
			}
			return default(ValueTask);
		}
		catch (Exception exception)
		{
			return new ValueTask(Task.FromException(exception));
		}
	}

	/// <summary>Writes a byte to the current position in the file stream.</summary>
	/// <param name="value">A byte value written to the stream.</param>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
	/// <exception cref="T:System.NotSupportedException">The underlying memory does not support writing.- or -An attempt is made to write to the stream and the <see cref="P:System.IO.UnmanagedMemoryStream.CanWrite" /> property is false.- or - The current position is at the end of the capacity of the stream.</exception>
	/// <exception cref="T:System.IO.IOException">The supplied <paramref name="value" /> causes the stream exceed its maximum capacity.</exception>
	/// <filterpriority>2</filterpriority>
	public unsafe override void WriteByte(byte value)
	{
		EnsureNotClosed();
		EnsureWriteable();
		long num = Interlocked.Read(ref _position);
		long num2 = Interlocked.Read(ref _length);
		long num3 = num + 1;
		if (num >= num2)
		{
			if (num3 < 0)
			{
				throw new IOException("Stream was too long.");
			}
			if (num3 > _capacity)
			{
				throw new NotSupportedException("Unable to expand length of this stream beyond its capacity.");
			}
			if (_buffer == null)
			{
				if (num > num2)
				{
					Buffer.ZeroMemory(_mem + num2, num - num2);
				}
				Interlocked.Exchange(ref _length, num3);
			}
		}
		if (_buffer != null)
		{
			byte* pointer = null;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				_buffer.AcquirePointer(ref pointer);
				(pointer + num)[_offset] = value;
			}
			finally
			{
				if (pointer != null)
				{
					_buffer.ReleasePointer();
				}
			}
		}
		else
		{
			_mem[num] = value;
		}
		Interlocked.Exchange(ref _position, num3);
	}
}
