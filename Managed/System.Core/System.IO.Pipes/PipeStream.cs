using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace System.IO.Pipes;

/// <summary>Exposes a <see cref="T:System.IO.Stream" /> object around a pipe, which supports both anonymous and named pipes.</summary>
public abstract class PipeStream : Stream
{
	internal const bool CheckOperationsRequiresSetHandle = true;

	internal ThreadPoolBoundHandle _threadPoolBinding;

	internal const string AnonymousPipeName = "anonymous";

	private static readonly Task<int> s_zeroTask = Task.FromResult(0);

	private SafePipeHandle _handle;

	private bool _canRead;

	private bool _canWrite;

	private bool _isAsync;

	private bool _isCurrentUserOnly;

	private bool _isMessageComplete;

	private bool _isFromExistingHandle;

	private bool _isHandleExposed;

	private PipeTransmissionMode _readMode;

	private PipeTransmissionMode _transmissionMode;

	private PipeDirection _pipeDirection;

	private int _outBufferSize;

	private PipeState _state;

	/// <summary>Gets the pipe transmission mode supported by the current pipe.</summary>
	/// <returns>One of the <see cref="T:System.IO.Pipes.PipeTransmissionMode" /> values that indicates the transmission mode supported by the current pipe.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The handle has not been set.-or-The pipe is waiting to connect in an anonymous client/server operation or with a named client. </exception>
	/// <exception cref="T:System.IO.IOException">The pipe is broken or another I/O error occurred.</exception>
	public virtual PipeTransmissionMode TransmissionMode
	{
		get
		{
			CheckPipePropertyOperations();
			if (_isFromExistingHandle)
			{
				if (!global::Interop.Kernel32.GetNamedPipeInfo(_handle, out var lpFlags, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero))
				{
					throw WinIOError(Marshal.GetLastWin32Error());
				}
				if ((lpFlags & 4) != 0)
				{
					return PipeTransmissionMode.Message;
				}
				return PipeTransmissionMode.Byte;
			}
			return _transmissionMode;
		}
	}

	/// <summary>Gets the size, in bytes, of the inbound buffer for a pipe.</summary>
	/// <returns>An integer value that represents the inbound buffer size, in bytes.</returns>
	/// <exception cref="T:System.NotSupportedException">The stream is unreadable.</exception>
	/// <exception cref="T:System.InvalidOperationException">The pipe is waiting to connect.</exception>
	/// <exception cref="T:System.IO.IOException">The pipe is broken or another I/O error occurred.</exception>
	public virtual int InBufferSize
	{
		get
		{
			CheckPipePropertyOperations();
			if (!CanRead)
			{
				throw new NotSupportedException("Stream does not support reading.");
			}
			if (!global::Interop.Kernel32.GetNamedPipeInfo(_handle, IntPtr.Zero, IntPtr.Zero, out var lpInBufferSize, IntPtr.Zero))
			{
				throw WinIOError(Marshal.GetLastWin32Error());
			}
			return lpInBufferSize;
		}
	}

	/// <summary>Gets the size, in bytes, of the outbound buffer for a pipe.</summary>
	/// <returns>The outbound buffer size, in bytes.</returns>
	/// <exception cref="T:System.NotSupportedException">The stream is unwriteable.</exception>
	/// <exception cref="T:System.InvalidOperationException">The pipe is waiting to connect.</exception>
	/// <exception cref="T:System.IO.IOException">The pipe is broken or another I/O error occurred.</exception>
	public virtual int OutBufferSize
	{
		get
		{
			CheckPipePropertyOperations();
			if (!CanWrite)
			{
				throw new NotSupportedException("Stream does not support writing.");
			}
			if (_pipeDirection == PipeDirection.Out)
			{
				return _outBufferSize;
			}
			if (!global::Interop.Kernel32.GetNamedPipeInfo(_handle, IntPtr.Zero, out var lpOutBufferSize, IntPtr.Zero, IntPtr.Zero))
			{
				throw WinIOError(Marshal.GetLastWin32Error());
			}
			return lpOutBufferSize;
		}
	}

	/// <summary>Gets or sets the reading mode for a <see cref="T:System.IO.Pipes.PipeStream" /> object.</summary>
	/// <returns>One of the <see cref="T:System.IO.Pipes.PipeTransmissionMode" /> values that indicates how the <see cref="T:System.IO.Pipes.PipeStream" /> object reads from the pipe.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The supplied value is not a valid <see cref="T:System.IO.Pipes.PipeTransmissionMode" /> value.</exception>
	/// <exception cref="T:System.NotSupportedException">The supplied value is not a supported <see cref="T:System.IO.Pipes.PipeTransmissionMode" /> value for this pipe stream.</exception>
	/// <exception cref="T:System.InvalidOperationException">The handle has not been set.-or-The pipe is waiting to connect with a named client.</exception>
	/// <exception cref="T:System.IO.IOException">The pipe is broken or an I/O error occurred with a named client.</exception>
	public unsafe virtual PipeTransmissionMode ReadMode
	{
		get
		{
			CheckPipePropertyOperations();
			if (_isFromExistingHandle || IsHandleExposed)
			{
				UpdateReadMode();
			}
			return _readMode;
		}
		set
		{
			CheckPipePropertyOperations();
			if (value < PipeTransmissionMode.Byte || value > PipeTransmissionMode.Message)
			{
				throw new ArgumentOutOfRangeException("value", "For named pipes, transmission mode can be TransmissionMode.Byte or PipeTransmissionMode.Message. For anonymous pipes, transmission mode can be TransmissionMode.Byte.");
			}
			int num = (int)value << 1;
			if (!global::Interop.Kernel32.SetNamedPipeHandleState(_handle, &num, IntPtr.Zero, IntPtr.Zero))
			{
				throw WinIOError(Marshal.GetLastWin32Error());
			}
			_readMode = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether a <see cref="T:System.IO.Pipes.PipeStream" /> object is connected.</summary>
	/// <returns>true if the <see cref="T:System.IO.Pipes.PipeStream" /> object is connected; otherwise, false.</returns>
	public bool IsConnected
	{
		get
		{
			return State == PipeState.Connected;
		}
		protected set
		{
			_state = (value ? PipeState.Connected : PipeState.Disconnected);
		}
	}

	/// <summary>Gets a value indicating whether a <see cref="T:System.IO.Pipes.PipeStream" /> object was opened asynchronously or synchronously.</summary>
	/// <returns>true if the <see cref="T:System.IO.Pipes.PipeStream" /> object was opened asynchronously; otherwise, false.</returns>
	public bool IsAsync => _isAsync;

	/// <summary>Gets a value indicating whether there is more data in the message returned from the most recent read operation.</summary>
	/// <returns>true if there are no more characters to read in the message; otherwise, false.</returns>
	/// <exception cref="T:System.InvalidOperationException">The pipe is not connected.-or-The pipe handle has not been set.-or-The pipe's <see cref="P:System.IO.Pipes.PipeStream.ReadMode" /> property value is not <see cref="F:System.IO.Pipes.PipeTransmissionMode.Message" />.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	public bool IsMessageComplete
	{
		get
		{
			if (_state == PipeState.WaitingToConnect)
			{
				throw new InvalidOperationException("Pipe hasn't been connected yet.");
			}
			if (_state == PipeState.Disconnected)
			{
				throw new InvalidOperationException("Pipe is in a disconnected state.");
			}
			if (_handle == null)
			{
				throw new InvalidOperationException("Pipe handle has not been set.  Did your PipeStream implementation call InitializeHandle?");
			}
			if (_state == PipeState.Closed || (_handle != null && _handle.IsClosed))
			{
				throw Error.GetPipeNotOpen();
			}
			if (_readMode != PipeTransmissionMode.Message)
			{
				throw new InvalidOperationException("ReadMode is not of PipeTransmissionMode.Message.");
			}
			return _isMessageComplete;
		}
	}

	/// <summary>Gets the safe handle for the local end of the pipe that the current <see cref="T:System.IO.Pipes.PipeStream" /> object encapsulates.</summary>
	/// <returns>A <see cref="T:Microsoft.Win32.SafeHandles.SafePipeHandle" /> object for the pipe that is encapsulated by the current <see cref="T:System.IO.Pipes.PipeStream" /> object.</returns>
	/// <exception cref="T:System.InvalidOperationException">The pipe handle has not been set.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	public SafePipeHandle SafePipeHandle
	{
		get
		{
			if (_handle == null)
			{
				throw new InvalidOperationException("Pipe handle has not been set.  Did your PipeStream implementation call InitializeHandle?");
			}
			if (_handle.IsClosed)
			{
				throw Error.GetPipeNotOpen();
			}
			_isHandleExposed = true;
			return _handle;
		}
	}

	internal SafePipeHandle InternalHandle => _handle;

	/// <summary>Gets a value indicating whether a handle to a <see cref="T:System.IO.Pipes.PipeStream" /> object is exposed.</summary>
	/// <returns>true if a handle to the <see cref="T:System.IO.Pipes.PipeStream" /> object is exposed; otherwise, false.</returns>
	protected bool IsHandleExposed => _isHandleExposed;

	/// <summary>Gets a value indicating whether the current stream supports read operations.</summary>
	/// <returns>true if the stream supports read operations; otherwise, false.</returns>
	public override bool CanRead => _canRead;

	/// <summary>Gets a value indicating whether the current stream supports write operations.</summary>
	/// <returns>true if the stream supports write operations; otherwise, false.</returns>
	public override bool CanWrite => _canWrite;

	/// <summary>Gets a value indicating whether the current stream supports seek operations.</summary>
	/// <returns>false in all cases.</returns>
	public override bool CanSeek => false;

	/// <summary>Gets the length of a stream, in bytes.</summary>
	/// <returns>0 in all cases.</returns>
	/// <exception cref="T:System.NotSupportedException">Always thrown.</exception>
	public override long Length
	{
		get
		{
			throw Error.GetSeekNotSupported();
		}
	}

	/// <summary>Gets or sets the current position of the current stream.</summary>
	/// <returns>0 in all cases.</returns>
	/// <exception cref="T:System.NotSupportedException">Always thrown.</exception>
	public override long Position
	{
		get
		{
			throw Error.GetSeekNotSupported();
		}
		set
		{
			throw Error.GetSeekNotSupported();
		}
	}

	internal PipeState State
	{
		get
		{
			return _state;
		}
		set
		{
			_state = value;
		}
	}

	internal bool IsCurrentUserOnly
	{
		get
		{
			return _isCurrentUserOnly;
		}
		set
		{
			_isCurrentUserOnly = value;
		}
	}

	internal static string GetPipePath(string serverName, string pipeName)
	{
		string fullPath = Path.GetFullPath("\\\\" + serverName + "\\pipe\\" + pipeName);
		if (string.Equals(fullPath, "\\\\.\\pipe\\anonymous", StringComparison.OrdinalIgnoreCase))
		{
			throw new ArgumentOutOfRangeException("pipeName", "The pipeName \\\"anonymous\\\" is reserved.");
		}
		return fullPath;
	}

	internal void ValidateHandleIsPipe(SafePipeHandle safePipeHandle)
	{
		if (global::Interop.Kernel32.GetFileType(safePipeHandle) != 3)
		{
			throw new IOException("Invalid pipe handle.");
		}
	}

	private void InitializeAsyncHandle(SafePipeHandle handle)
	{
		_threadPoolBinding = ThreadPoolBoundHandle.BindHandle(handle);
	}

	private void DisposeCore(bool disposing)
	{
		if (disposing)
		{
			_threadPoolBinding?.Dispose();
		}
	}

	private unsafe int ReadCore(Span<byte> buffer)
	{
		int errorCode = 0;
		int num = ReadFileNative(_handle, buffer, null, out errorCode);
		if (num == -1)
		{
			if (errorCode != 109 && errorCode != 233)
			{
				throw Win32Marshal.GetExceptionForWin32Error(errorCode, string.Empty);
			}
			State = PipeState.Broken;
			num = 0;
		}
		_isMessageComplete = errorCode != 234;
		return num;
	}

	private unsafe Task<int> ReadAsyncCore(Memory<byte> buffer, CancellationToken cancellationToken)
	{
		ReadWriteCompletionSource readWriteCompletionSource = new ReadWriteCompletionSource(this, buffer, isWrite: false);
		int errorCode = 0;
		if (ReadFileNative(_handle, buffer.Span, readWriteCompletionSource.Overlapped, out errorCode) == -1)
		{
			switch (errorCode)
			{
			case 109:
			case 233:
				State = PipeState.Broken;
				readWriteCompletionSource.Overlapped->InternalLow = IntPtr.Zero;
				readWriteCompletionSource.ReleaseResources();
				UpdateMessageCompletion(completion: true);
				return s_zeroTask;
			default:
				throw Win32Marshal.GetExceptionForWin32Error(errorCode);
			case 997:
				break;
			}
		}
		readWriteCompletionSource.RegisterForCancellation(cancellationToken);
		return readWriteCompletionSource.Task;
	}

	private unsafe void WriteCore(ReadOnlySpan<byte> buffer)
	{
		int errorCode = 0;
		if (WriteFileNative(_handle, buffer, null, out errorCode) == -1)
		{
			throw WinIOError(errorCode);
		}
	}

	private unsafe Task WriteAsyncCore(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
	{
		ReadWriteCompletionSource readWriteCompletionSource = new ReadWriteCompletionSource(this, buffer, isWrite: true);
		int errorCode = 0;
		if (WriteFileNative(_handle, buffer.Span, readWriteCompletionSource.Overlapped, out errorCode) == -1 && errorCode != 997)
		{
			readWriteCompletionSource.ReleaseResources();
			throw WinIOError(errorCode);
		}
		readWriteCompletionSource.RegisterForCancellation(cancellationToken);
		return readWriteCompletionSource.Task;
	}

	/// <summary>Waits for the other end of the pipe to read all sent bytes.</summary>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	/// <exception cref="T:System.NotSupportedException">The pipe does not support write operations.</exception>
	/// <exception cref="T:System.IO.IOException">The pipe is broken or another I/O error occurred.</exception>
	public void WaitForPipeDrain()
	{
		CheckWriteOperations();
		if (!CanWrite)
		{
			throw Error.GetWriteNotSupported();
		}
		if (!global::Interop.Kernel32.FlushFileBuffers(_handle))
		{
			throw WinIOError(Marshal.GetLastWin32Error());
		}
	}

	private unsafe int ReadFileNative(SafePipeHandle handle, Span<byte> buffer, NativeOverlapped* overlapped, out int errorCode)
	{
		if (buffer.Length == 0)
		{
			errorCode = 0;
			return 0;
		}
		int numBytesRead = 0;
		int num;
		fixed (byte* reference = &MemoryMarshal.GetReference(buffer))
		{
			num = (_isAsync ? global::Interop.Kernel32.ReadFile(handle, reference, buffer.Length, IntPtr.Zero, overlapped) : global::Interop.Kernel32.ReadFile(handle, reference, buffer.Length, out numBytesRead, IntPtr.Zero));
		}
		if (num == 0)
		{
			errorCode = Marshal.GetLastWin32Error();
			if (errorCode != 234)
			{
				return -1;
			}
			return numBytesRead;
		}
		errorCode = 0;
		return numBytesRead;
	}

	private unsafe int WriteFileNative(SafePipeHandle handle, ReadOnlySpan<byte> buffer, NativeOverlapped* overlapped, out int errorCode)
	{
		if (buffer.Length == 0)
		{
			errorCode = 0;
			return 0;
		}
		int numBytesWritten = 0;
		int num;
		fixed (byte* reference = &MemoryMarshal.GetReference(buffer))
		{
			num = (_isAsync ? global::Interop.Kernel32.WriteFile(handle, reference, buffer.Length, IntPtr.Zero, overlapped) : global::Interop.Kernel32.WriteFile(handle, reference, buffer.Length, out numBytesWritten, IntPtr.Zero));
		}
		if (num == 0)
		{
			errorCode = Marshal.GetLastWin32Error();
			return -1;
		}
		errorCode = 0;
		return numBytesWritten;
	}

	internal unsafe static global::Interop.Kernel32.SECURITY_ATTRIBUTES GetSecAttrs(HandleInheritability inheritability)
	{
		global::Interop.Kernel32.SECURITY_ATTRIBUTES result = default(global::Interop.Kernel32.SECURITY_ATTRIBUTES);
		if ((inheritability & HandleInheritability.Inheritable) != HandleInheritability.None)
		{
			result = new global::Interop.Kernel32.SECURITY_ATTRIBUTES
			{
				nLength = (uint)sizeof(global::Interop.Kernel32.SECURITY_ATTRIBUTES),
				bInheritHandle = global::Interop.BOOL.TRUE
			};
		}
		return result;
	}

	internal unsafe static global::Interop.Kernel32.SECURITY_ATTRIBUTES GetSecAttrs(HandleInheritability inheritability, PipeSecurity pipeSecurity, ref GCHandle pinningHandle)
	{
		global::Interop.Kernel32.SECURITY_ATTRIBUTES result = new global::Interop.Kernel32.SECURITY_ATTRIBUTES
		{
			nLength = (uint)sizeof(global::Interop.Kernel32.SECURITY_ATTRIBUTES)
		};
		if ((inheritability & HandleInheritability.Inheritable) != HandleInheritability.None)
		{
			result.bInheritHandle = global::Interop.BOOL.TRUE;
		}
		if (pipeSecurity != null)
		{
			byte[] securityDescriptorBinaryForm = pipeSecurity.GetSecurityDescriptorBinaryForm();
			pinningHandle = GCHandle.Alloc(securityDescriptorBinaryForm, GCHandleType.Pinned);
			fixed (byte* ptr = securityDescriptorBinaryForm)
			{
				result.lpSecurityDescriptor = (IntPtr)ptr;
			}
		}
		return result;
	}

	private void UpdateReadMode()
	{
		if (!global::Interop.Kernel32.GetNamedPipeHandleState(SafePipeHandle, out var lpState, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0))
		{
			throw WinIOError(Marshal.GetLastWin32Error());
		}
		if ((lpState & 2) != 0)
		{
			_readMode = PipeTransmissionMode.Message;
		}
		else
		{
			_readMode = PipeTransmissionMode.Byte;
		}
	}

	internal Exception WinIOError(int errorCode)
	{
		switch (errorCode)
		{
		case 109:
		case 232:
		case 233:
			_state = PipeState.Broken;
			return new IOException("Pipe is broken.", Win32Marshal.MakeHRFromErrorCode(errorCode));
		case 38:
			return Error.GetEndOfFile();
		case 6:
			_handle.SetHandleAsInvalid();
			_state = PipeState.Broken;
			break;
		}
		return Win32Marshal.GetExceptionForWin32Error(errorCode);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.PipeStream" /> class using the specified <see cref="T:System.IO.Pipes.PipeDirection" /> value and buffer size.</summary>
	/// <param name="direction">One of the <see cref="T:System.IO.Pipes.PipeDirection" /> values that indicates the direction of the pipe object.</param>
	/// <param name="bufferSize">A positive <see cref="T:System.Int32" /> value greater than or equal to 0 that indicates the buffer size.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.-or-<paramref name="bufferSize" /> is less than 0.</exception>
	protected PipeStream(PipeDirection direction, int bufferSize)
	{
		if (direction < PipeDirection.In || direction > PipeDirection.InOut)
		{
			throw new ArgumentOutOfRangeException("direction", "For named pipes, the pipe direction can be PipeDirection.In, PipeDirection.Out or PipeDirection.InOut. For anonymous pipes, the pipe direction can be PipeDirection.In or PipeDirection.Out.");
		}
		if (bufferSize < 0)
		{
			throw new ArgumentOutOfRangeException("bufferSize", "Non negative number is required.");
		}
		Init(direction, PipeTransmissionMode.Byte, bufferSize);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.PipeStream" /> class using the specified <see cref="T:System.IO.Pipes.PipeDirection" />, <see cref="T:System.IO.Pipes.PipeTransmissionMode" />, and buffer size.</summary>
	/// <param name="direction">One of the <see cref="T:System.IO.Pipes.PipeDirection" /> values that indicates the direction of the pipe object.</param>
	/// <param name="transmissionMode">One of the <see cref="T:System.IO.Pipes.PipeTransmissionMode" /> values that indicates the transmission mode of the pipe object.</param>
	/// <param name="outBufferSize">A positive <see cref="T:System.Int32" /> value greater than or equal to 0 that indicates the buffer size.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.-or-<paramref name="transmissionMode" /> is not a valid <see cref="T:System.IO.Pipes.PipeTransmissionMode" /> value.-or-<paramref name="bufferSize" /> is less than 0.</exception>
	protected PipeStream(PipeDirection direction, PipeTransmissionMode transmissionMode, int outBufferSize)
	{
		if (direction < PipeDirection.In || direction > PipeDirection.InOut)
		{
			throw new ArgumentOutOfRangeException("direction", "For named pipes, the pipe direction can be PipeDirection.In, PipeDirection.Out or PipeDirection.InOut. For anonymous pipes, the pipe direction can be PipeDirection.In or PipeDirection.Out.");
		}
		if (transmissionMode < PipeTransmissionMode.Byte || transmissionMode > PipeTransmissionMode.Message)
		{
			throw new ArgumentOutOfRangeException("transmissionMode", "For named pipes, transmission mode can be TransmissionMode.Byte or PipeTransmissionMode.Message. For anonymous pipes, transmission mode can be TransmissionMode.Byte.");
		}
		if (outBufferSize < 0)
		{
			throw new ArgumentOutOfRangeException("outBufferSize", "Non negative number is required.");
		}
		Init(direction, transmissionMode, outBufferSize);
	}

	private void Init(PipeDirection direction, PipeTransmissionMode transmissionMode, int outBufferSize)
	{
		_readMode = transmissionMode;
		_transmissionMode = transmissionMode;
		_pipeDirection = direction;
		if ((_pipeDirection & PipeDirection.In) != 0)
		{
			_canRead = true;
		}
		if ((_pipeDirection & PipeDirection.Out) != 0)
		{
			_canWrite = true;
		}
		_outBufferSize = outBufferSize;
		_isMessageComplete = true;
		_state = PipeState.WaitingToConnect;
	}

	/// <summary>Initializes a <see cref="T:System.IO.Pipes.PipeStream" /> object from the specified <see cref="T:Microsoft.Win32.SafeHandles.SafePipeHandle" /> object.</summary>
	/// <param name="handle">The <see cref="T:Microsoft.Win32.SafeHandles.SafePipeHandle" /> object of the pipe to initialize.</param>
	/// <param name="isExposed">true to expose the handle; otherwise, false.</param>
	/// <param name="isAsync">true to indicate that the handle was opened asynchronously; otherwise, false.</param>
	/// <exception cref="T:System.IO.IOException">A handle cannot be bound to the pipe.</exception>
	protected void InitializeHandle(SafePipeHandle handle, bool isExposed, bool isAsync)
	{
		if (isAsync && handle != null)
		{
			InitializeAsyncHandle(handle);
		}
		_handle = handle;
		_isAsync = isAsync;
		_isHandleExposed = isExposed;
		_isFromExistingHandle = isExposed;
	}

	/// <summary>Reads a block of bytes from a stream and writes the data to a specified buffer.</summary>
	/// <returns>The total number of bytes that are read into <paramref name="buffer" />. This might be less than the number of bytes requested if that number of bytes is not currently available, or 0 if the end of the stream is reached.</returns>
	/// <param name="buffer">When this method returns, contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
	/// <param name="offset">The byte offset in the <paramref name="buffer" /> array at which the bytes that are read will be placed.</param>
	/// <param name="count">The maximum number of bytes to read.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is less than 0.-or-<paramref name="count" /> is less than 0.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="count" /> is greater than the number of bytes available in <paramref name="buffer" />.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	/// <exception cref="T:System.NotSupportedException">The pipe does not support read operations.</exception>
	/// <exception cref="T:System.InvalidOperationException">The pipe is disconnected, waiting to connect, or the handle has not been set.</exception>
	/// <exception cref="T:System.IO.IOException">Any I/O error occurred.</exception>
	public override int Read(byte[] buffer, int offset, int count)
	{
		if (_isAsync)
		{
			return ReadAsync(buffer, offset, count, CancellationToken.None).GetAwaiter().GetResult();
		}
		CheckReadWriteArgs(buffer, offset, count);
		if (!CanRead)
		{
			throw Error.GetReadNotSupported();
		}
		CheckReadOperations();
		return ReadCore(new Span<byte>(buffer, offset, count));
	}

	public override int Read(Span<byte> buffer)
	{
		if (_isAsync)
		{
			return base.Read(buffer);
		}
		if (!CanRead)
		{
			throw Error.GetReadNotSupported();
		}
		CheckReadOperations();
		return ReadCore(buffer);
	}

	public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		CheckReadWriteArgs(buffer, offset, count);
		if (!CanRead)
		{
			throw Error.GetReadNotSupported();
		}
		if (cancellationToken.IsCancellationRequested)
		{
			return Task.FromCanceled<int>(cancellationToken);
		}
		CheckReadOperations();
		if (!_isAsync)
		{
			return base.ReadAsync(buffer, offset, count, cancellationToken);
		}
		if (count == 0)
		{
			UpdateMessageCompletion(completion: false);
			return s_zeroTask;
		}
		return ReadAsyncCore(new Memory<byte>(buffer, offset, count), cancellationToken);
	}

	public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_isAsync)
		{
			return base.ReadAsync(buffer, cancellationToken);
		}
		if (!CanRead)
		{
			throw Error.GetReadNotSupported();
		}
		if (cancellationToken.IsCancellationRequested)
		{
			return new ValueTask<int>(Task.FromCanceled<int>(cancellationToken));
		}
		CheckReadOperations();
		if (buffer.Length == 0)
		{
			UpdateMessageCompletion(completion: false);
			return new ValueTask<int>(0);
		}
		return new ValueTask<int>(ReadAsyncCore(buffer, cancellationToken));
	}

	/// <summary>Begins an asynchronous read operation.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> object that references the asynchronous read.</returns>
	/// <param name="buffer">The buffer to read data into.</param>
	/// <param name="offset">The byte offset in <paramref name="buffer" /> at which to begin reading.</param>
	/// <param name="count">The maximum number of bytes to read.</param>
	/// <param name="callback">The method to call when the asynchronous read operation is completed.</param>
	/// <param name="state">A user-provided object that distinguishes this particular asynchronous read request from other requests.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is less than 0.-or-<paramref name="count" /> is less than 0.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="count" /> is greater than the number of bytes available in <paramref name="buffer" />.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	/// <exception cref="T:System.NotSupportedException">The pipe does not support read operations.</exception>
	/// <exception cref="T:System.InvalidOperationException">The pipe is disconnected, waiting to connect, or the handle has not been set.</exception>
	/// <exception cref="T:System.IO.IOException">The pipe is broken or another I/O error occurred.</exception>
	public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		if (_isAsync)
		{
			return TaskToApm.Begin(ReadAsync(buffer, offset, count, CancellationToken.None), callback, state);
		}
		return base.BeginRead(buffer, offset, count, callback, state);
	}

	/// <summary>Ends a pending asynchronous read request.</summary>
	/// <returns>The number of bytes that were read. A return value of 0 indicates the end of the stream (the pipe has been closed).</returns>
	/// <param name="asyncResult">The reference to the pending asynchronous request.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> did not originate from a <see cref="M:System.IO.Pipes.PipeStream.BeginRead(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)" /> method on the current stream. </exception>
	/// <exception cref="T:System.IO.IOException">The stream is closed or an internal error has occurred.</exception>
	public override int EndRead(IAsyncResult asyncResult)
	{
		if (_isAsync)
		{
			return TaskToApm.End<int>(asyncResult);
		}
		return base.EndRead(asyncResult);
	}

	/// <summary>Writes a block of bytes to the current stream using data from a buffer.</summary>
	/// <param name="buffer">The buffer that contains data to write to the pipe.</param>
	/// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
	/// <param name="count">The maximum number of bytes to write to the current stream.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is less than 0.-or-<paramref name="count" /> is less than 0.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="count" /> is greater than the number of bytes available in <paramref name="buffer" />.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	/// <exception cref="T:System.NotSupportedException">The pipe does not support write operations.</exception>
	/// <exception cref="T:System.IO.IOException">The pipe is broken or another I/O error occurred.</exception>
	public override void Write(byte[] buffer, int offset, int count)
	{
		if (_isAsync)
		{
			WriteAsync(buffer, offset, count, CancellationToken.None).GetAwaiter().GetResult();
			return;
		}
		CheckReadWriteArgs(buffer, offset, count);
		if (!CanWrite)
		{
			throw Error.GetWriteNotSupported();
		}
		CheckWriteOperations();
		WriteCore(new ReadOnlySpan<byte>(buffer, offset, count));
	}

	public override void Write(ReadOnlySpan<byte> buffer)
	{
		if (_isAsync)
		{
			base.Write(buffer);
			return;
		}
		if (!CanWrite)
		{
			throw Error.GetWriteNotSupported();
		}
		CheckWriteOperations();
		WriteCore(buffer);
	}

	public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		CheckReadWriteArgs(buffer, offset, count);
		if (!CanWrite)
		{
			throw Error.GetWriteNotSupported();
		}
		if (cancellationToken.IsCancellationRequested)
		{
			return Task.FromCanceled<int>(cancellationToken);
		}
		CheckWriteOperations();
		if (!_isAsync)
		{
			return base.WriteAsync(buffer, offset, count, cancellationToken);
		}
		if (count == 0)
		{
			return Task.CompletedTask;
		}
		return WriteAsyncCore(new ReadOnlyMemory<byte>(buffer, offset, count), cancellationToken);
	}

	public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_isAsync)
		{
			return base.WriteAsync(buffer, cancellationToken);
		}
		if (!CanWrite)
		{
			throw Error.GetWriteNotSupported();
		}
		if (cancellationToken.IsCancellationRequested)
		{
			return new ValueTask(Task.FromCanceled<int>(cancellationToken));
		}
		CheckWriteOperations();
		if (buffer.Length == 0)
		{
			return default(ValueTask);
		}
		return new ValueTask(WriteAsyncCore(buffer, cancellationToken));
	}

	/// <summary>Begins an asynchronous write operation.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> object that references the asynchronous write operation.</returns>
	/// <param name="buffer">The buffer that contains the data to write to the current stream.</param>
	/// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
	/// <param name="count">The maximum number of bytes to write.</param>
	/// <param name="callback">The method to call when the asynchronous write operation is completed.</param>
	/// <param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is less than 0.-or-<paramref name="count" /> is less than 0.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="count" /> is greater than the number of bytes available in <paramref name="buffer" />.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	/// <exception cref="T:System.NotSupportedException">The pipe does not support write operations.</exception>
	/// <exception cref="T:System.InvalidOperationException">The pipe is disconnected, waiting to connect, or the handle has not been set.</exception>
	/// <exception cref="T:System.IO.IOException">The pipe is broken or another I/O error occurred.</exception>
	public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		if (_isAsync)
		{
			return TaskToApm.Begin(WriteAsync(buffer, offset, count, CancellationToken.None), callback, state);
		}
		return base.BeginWrite(buffer, offset, count, callback, state);
	}

	/// <summary>Ends a pending asynchronous write request.</summary>
	/// <param name="asyncResult">The reference to the pending asynchronous request.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> did not originate from a <see cref="M:System.IO.Pipes.PipeStream.BeginWrite(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)" /> method on the current stream. </exception>
	/// <exception cref="T:System.IO.IOException">The stream is closed or an internal error has occurred.</exception>
	public override void EndWrite(IAsyncResult asyncResult)
	{
		if (_isAsync)
		{
			TaskToApm.End(asyncResult);
		}
		else
		{
			base.EndWrite(asyncResult);
		}
	}

	private void CheckReadWriteArgs(byte[] buffer, int offset, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer", "Buffer cannot be null.");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "Non negative number is required.");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Non negative number is required.");
		}
		if (buffer.Length - offset < count)
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
	}

	[Conditional("DEBUG")]
	private static void DebugAssertHandleValid(SafePipeHandle handle)
	{
	}

	[Conditional("DEBUG")]
	private static void DebugAssertReadWriteArgs(byte[] buffer, int offset, int count, SafePipeHandle handle)
	{
	}

	/// <summary>Reads a byte from a pipe.</summary>
	/// <returns>The byte, cast to <see cref="T:System.Int32" />, or -1 indicates the end of the stream (the pipe has been closed).</returns>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	/// <exception cref="T:System.NotSupportedException">The pipe does not support read operations.</exception>
	/// <exception cref="T:System.InvalidOperationException">The pipe is disconnected, waiting to connect, or the handle has not been set.</exception>
	/// <exception cref="T:System.IO.IOException">Any I/O error occurred.</exception>
	public unsafe override int ReadByte()
	{
		byte result = default(byte);
		if (Read(new Span<byte>(&result, 1)) <= 0)
		{
			return -1;
		}
		return result;
	}

	/// <summary>Writes a byte to the current stream.</summary>
	/// <param name="value">The byte to write to the stream.</param>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	/// <exception cref="T:System.NotSupportedException">The pipe does not support write operations.</exception>
	/// <exception cref="T:System.InvalidOperationException">The pipe is disconnected, waiting to connect, or the handle has not been set.</exception>
	/// <exception cref="T:System.IO.IOException">The pipe is broken or another I/O error occurred.</exception>
	public unsafe override void WriteByte(byte value)
	{
		Write(new ReadOnlySpan<byte>(&value, 1));
	}

	/// <summary>Clears the buffer for the current stream and causes any buffered data to be written to the underlying device.</summary>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	/// <exception cref="T:System.NotSupportedException">The pipe does not support write operations.</exception>
	/// <exception cref="T:System.IO.IOException">The pipe is broken or another I/O error occurred.</exception>
	public override void Flush()
	{
		CheckWriteOperations();
		if (!CanWrite)
		{
			throw Error.GetWriteNotSupported();
		}
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.IO.Pipes.PipeStream" /> class and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	protected override void Dispose(bool disposing)
	{
		try
		{
			if (_handle != null && !_handle.IsClosed)
			{
				_handle.Dispose();
			}
			DisposeCore(disposing);
		}
		finally
		{
			base.Dispose(disposing);
		}
		_state = PipeState.Closed;
	}

	internal void UpdateMessageCompletion(bool completion)
	{
		_isMessageComplete = completion || _state == PipeState.Broken;
	}

	/// <summary>Sets the length of the current stream to the specified value.</summary>
	/// <param name="value">The new length of the stream.</param>
	public override void SetLength(long value)
	{
		throw Error.GetSeekNotSupported();
	}

	/// <summary>Sets the current position of the current stream to the specified value.</summary>
	/// <returns>The new position in the stream.</returns>
	/// <param name="offset">The point, relative to <paramref name="origin" />, to begin seeking from.</param>
	/// <param name="origin">Specifies the beginning, the end, or the current position as a reference point for <paramref name="offset" />, using a value of type <see cref="T:System.IO.SeekOrigin" />.</param>
	public override long Seek(long offset, SeekOrigin origin)
	{
		throw Error.GetSeekNotSupported();
	}

	/// <summary>Verifies that the pipe is in a proper state for getting or setting properties.</summary>
	protected internal virtual void CheckPipePropertyOperations()
	{
		if (_handle == null)
		{
			throw new InvalidOperationException("Pipe handle has not been set.  Did your PipeStream implementation call InitializeHandle?");
		}
		if (_state == PipeState.Closed || (_handle != null && _handle.IsClosed))
		{
			throw Error.GetPipeNotOpen();
		}
	}

	/// <summary>Verifies that the pipe is in a connected state for read operations.</summary>
	protected internal void CheckReadOperations()
	{
		if (_state == PipeState.WaitingToConnect)
		{
			throw new InvalidOperationException("Pipe hasn't been connected yet.");
		}
		if (_state == PipeState.Disconnected)
		{
			throw new InvalidOperationException("Pipe is in a disconnected state.");
		}
		if (_handle == null)
		{
			throw new InvalidOperationException("Pipe handle has not been set.  Did your PipeStream implementation call InitializeHandle?");
		}
		if (_state == PipeState.Closed || (_handle != null && _handle.IsClosed))
		{
			throw Error.GetPipeNotOpen();
		}
	}

	/// <summary>Verifies that the pipe is in a connected state for write operations.</summary>
	protected internal void CheckWriteOperations()
	{
		if (_state == PipeState.WaitingToConnect)
		{
			throw new InvalidOperationException("Pipe hasn't been connected yet.");
		}
		if (_state == PipeState.Disconnected)
		{
			throw new InvalidOperationException("Pipe is in a disconnected state.");
		}
		if (_handle == null)
		{
			throw new InvalidOperationException("Pipe handle has not been set.  Did your PipeStream implementation call InitializeHandle?");
		}
		if (_state == PipeState.Broken)
		{
			throw new IOException("Pipe is broken.");
		}
		if (_state == PipeState.Closed || (_handle != null && _handle.IsClosed))
		{
			throw Error.GetPipeNotOpen();
		}
	}

	/// <summary>Gets a <see cref="T:System.IO.Pipes.PipeSecurity" /> object that encapsulates the access control list (ACL) entries for the pipe described by the current <see cref="T:System.IO.Pipes.PipeStream" /> object.</summary>
	/// <returns>A <see cref="T:System.IO.Pipes.PipeSecurity" /> object that encapsulates the access control list (ACL) entries for the pipe described by the current <see cref="T:System.IO.Pipes.PipeStream" /> object.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The underlying call to set security information failed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The underlying call to set security information failed.</exception>
	/// <exception cref="T:System.NotSupportedException">The underlying call to set security information failed.</exception>
	public PipeSecurity GetAccessControl()
	{
		if (State == PipeState.Closed)
		{
			throw Error.GetPipeNotOpen();
		}
		return new PipeSecurity(SafePipeHandle, AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
	}

	/// <summary>Applies the access control list (ACL) entries specified by a <see cref="T:System.IO.Pipes.PipeSecurity" /> object to the pipe specified by the current <see cref="T:System.IO.Pipes.PipeStream" /> object.</summary>
	/// <param name="pipeSecurity">A <see cref="T:System.IO.Pipes.PipeSecurity" /> object that specifies an access control list (ACL) entry to apply to the current pipe.</param>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeSecurity" /> is null.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The underlying call to set security information failed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The underlying call to set security information failed.</exception>
	/// <exception cref="T:System.NotSupportedException">The underlying call to set security information failed.</exception>
	public void SetAccessControl(PipeSecurity pipeSecurity)
	{
		if (pipeSecurity == null)
		{
			throw new ArgumentNullException("pipeSecurity");
		}
		CheckPipePropertyOperations();
		pipeSecurity.Persist(SafePipeHandle);
	}
}
