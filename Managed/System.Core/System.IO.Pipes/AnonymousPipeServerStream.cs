using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace System.IO.Pipes;

/// <summary>Exposes a stream around an anonymous pipe, which supports both synchronous and asynchronous read and write operations.</summary>
public sealed class AnonymousPipeServerStream : PipeStream
{
	private SafePipeHandle _clientHandle;

	private bool _clientHandleExposed;

	/// <summary>Gets the safe handle for the <see cref="T:System.IO.Pipes.AnonymousPipeClientStream" /> object that is currently connected to the <see cref="T:System.IO.Pipes.AnonymousPipeServerStream" /> object.</summary>
	/// <returns>A handle for the <see cref="T:System.IO.Pipes.AnonymousPipeClientStream" /> object that is currently connected to the <see cref="T:System.IO.Pipes.AnonymousPipeServerStream" /> object.</returns>
	public SafePipeHandle ClientSafePipeHandle
	{
		get
		{
			_clientHandleExposed = true;
			return _clientHandle;
		}
	}

	/// <summary>Gets the pipe transmission mode that is supported by the current pipe.</summary>
	/// <returns>The <see cref="T:System.IO.Pipes.PipeTransmissionMode" /> that is supported by the current pipe.</returns>
	public override PipeTransmissionMode TransmissionMode => PipeTransmissionMode.Byte;

	/// <summary>Sets the reading mode for the <see cref="T:System.IO.Pipes.AnonymousPipeServerStream" /> object. For anonymous pipes, transmission mode must be <see cref="F:System.IO.Pipes.PipeTransmissionMode.Byte" />.</summary>
	/// <returns>The reading mode for the <see cref="T:System.IO.Pipes.AnonymousPipeServerStream" /> object.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The transmission mode is not valid. For anonymous pipes, only <see cref="F:System.IO.Pipes.PipeTransmissionMode.Byte" /> is supported. </exception>
	/// <exception cref="T:System.NotSupportedException">The property is set to <see cref="F:System.IO.Pipes.PipeTransmissionMode.Message" />, which is not supported for anonymous pipes.</exception>
	/// <exception cref="T:System.IO.IOException">The connection is broken or another I/O error occurs.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	public override PipeTransmissionMode ReadMode
	{
		set
		{
			CheckPipePropertyOperations();
			switch (value)
			{
			default:
				throw new ArgumentOutOfRangeException("value", "For named pipes, transmission mode can be TransmissionMode.Byte or PipeTransmissionMode.Message. For anonymous pipes, transmission mode can be TransmissionMode.Byte.");
			case PipeTransmissionMode.Message:
				throw new NotSupportedException("Anonymous pipes do not support PipeTransmissionMode.Message ReadMode.");
			case PipeTransmissionMode.Byte:
				break;
			}
		}
	}

	private void Create(PipeDirection direction, HandleInheritability inheritability, int bufferSize)
	{
		Create(direction, inheritability, bufferSize, null);
	}

	private void Create(PipeDirection direction, HandleInheritability inheritability, int bufferSize, PipeSecurity pipeSecurity)
	{
		GCHandle pinningHandle = default(GCHandle);
		bool flag;
		SafePipeHandle hWritePipe;
		try
		{
			global::Interop.Kernel32.SECURITY_ATTRIBUTES lpPipeAttributes = PipeStream.GetSecAttrs(inheritability, pipeSecurity, ref pinningHandle);
			flag = ((direction != PipeDirection.In) ? global::Interop.Kernel32.CreatePipe(out _clientHandle, out hWritePipe, ref lpPipeAttributes, bufferSize) : global::Interop.Kernel32.CreatePipe(out hWritePipe, out _clientHandle, ref lpPipeAttributes, bufferSize));
		}
		finally
		{
			if (pinningHandle.IsAllocated)
			{
				pinningHandle.Free();
			}
		}
		if (!flag)
		{
			throw Win32Marshal.GetExceptionForLastWin32Error();
		}
		if (!global::Interop.Kernel32.DuplicateHandle(global::Interop.Kernel32.GetCurrentProcess(), hWritePipe, global::Interop.Kernel32.GetCurrentProcess(), out var lpTargetHandle, 0u, bInheritHandle: false, 2u))
		{
			throw Win32Marshal.GetExceptionForLastWin32Error();
		}
		hWritePipe.Dispose();
		InitializeHandle(lpTargetHandle, isExposed: false, isAsync: false);
		base.State = PipeState.Connected;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.AnonymousPipeServerStream" /> class.</summary>
	public AnonymousPipeServerStream()
		: this(PipeDirection.Out, HandleInheritability.None, 0)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.AnonymousPipeServerStream" /> class with the specified pipe direction.</summary>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.Anonymous pipes can only be in one direction, so <paramref name="direction" /> cannot be set to <see cref="F:System.IO.Pipes.PipeDirection.InOut" />.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="direction" /> is set to <see cref="F:System.IO.Pipes.PipeDirection.InOut" />.</exception>
	public AnonymousPipeServerStream(PipeDirection direction)
		: this(direction, HandleInheritability.None, 0)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.AnonymousPipeServerStream" /> class with the specified pipe direction and inheritability mode.</summary>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.Anonymous pipes can only be in one direction, so <paramref name="direction" /> cannot be set to <see cref="F:System.IO.Pipes.PipeDirection.InOut" />.</param>
	/// <param name="inheritability">One of the enumeration values that determines whether the underlying handle can be inherited by child processes. Must be set to either <see cref="F:System.IO.HandleInheritability.None" /> or <see cref="F:System.IO.HandleInheritability.Inheritable" />. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="inheritability" /> is not set to either <see cref="F:System.IO.HandleInheritability.None" /> or <see cref="F:System.IO.HandleInheritability.Inheritable" />.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="direction" /> is set to <see cref="F:System.IO.Pipes.PipeDirection.InOut" />.</exception>
	public AnonymousPipeServerStream(PipeDirection direction, HandleInheritability inheritability)
		: this(direction, inheritability, 0)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.AnonymousPipeServerStream" /> class from the specified pipe handles.</summary>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.Anonymous pipes can only be in one direction, so <paramref name="direction" /> cannot be set to <see cref="F:System.IO.Pipes.PipeDirection.InOut" />.</param>
	/// <param name="serverSafePipeHandle">A safe handle for the pipe that this <see cref="T:System.IO.Pipes.AnonymousPipeServerStream" /> object will encapsulate.</param>
	/// <param name="clientSafePipeHandle">A safe handle for the <see cref="T:System.IO.Pipes.AnonymousPipeClientStream" /> object.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="serverSafePipeHandle" /> or <paramref name="clientSafePipeHandle" /> is an invalid handle.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="serverSafePipeHandle" /> or <paramref name="clientSafePipeHandle" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="direction" /> is set to <see cref="F:System.IO.Pipes.PipeDirection.InOut" />.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error, such as a disk error, has occurred.-or-The stream has been closed.</exception>
	public AnonymousPipeServerStream(PipeDirection direction, SafePipeHandle serverSafePipeHandle, SafePipeHandle clientSafePipeHandle)
		: base(direction, 0)
	{
		if (direction == PipeDirection.InOut)
		{
			throw new NotSupportedException("Anonymous pipes can only be in one direction.");
		}
		if (serverSafePipeHandle == null)
		{
			throw new ArgumentNullException("serverSafePipeHandle");
		}
		if (clientSafePipeHandle == null)
		{
			throw new ArgumentNullException("clientSafePipeHandle");
		}
		if (serverSafePipeHandle.IsInvalid)
		{
			throw new ArgumentException("Invalid handle.", "serverSafePipeHandle");
		}
		if (clientSafePipeHandle.IsInvalid)
		{
			throw new ArgumentException("Invalid handle.", "clientSafePipeHandle");
		}
		ValidateHandleIsPipe(serverSafePipeHandle);
		ValidateHandleIsPipe(clientSafePipeHandle);
		InitializeHandle(serverSafePipeHandle, isExposed: true, isAsync: false);
		_clientHandle = clientSafePipeHandle;
		_clientHandleExposed = true;
		base.State = PipeState.Connected;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.AnonymousPipeServerStream" /> class with the specified pipe direction, inheritability mode, and buffer size.</summary>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.Anonymous pipes can only be in one direction, so <paramref name="direction" /> cannot be set to <see cref="F:System.IO.Pipes.PipeDirection.InOut" />.</param>
	/// <param name="inheritability">One of the enumeration values that determines whether the underlying handle can be inherited by child processes. Must be set to either <see cref="F:System.IO.HandleInheritability.None" /> or <see cref="F:System.IO.HandleInheritability.Inheritable" />.</param>
	/// <param name="bufferSize">The size of the buffer. This value must be greater than or equal to 0. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="inheritability" /> is not set to either <see cref="F:System.IO.HandleInheritability.None" /> or <see cref="F:System.IO.HandleInheritability.Inheritable" />.-or-<paramref name="bufferSize" /> is less than 0.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="direction" /> is set to <see cref="F:System.IO.Pipes.PipeDirection.InOut" />.</exception>
	public AnonymousPipeServerStream(PipeDirection direction, HandleInheritability inheritability, int bufferSize)
		: base(direction, bufferSize)
	{
		if (direction == PipeDirection.InOut)
		{
			throw new NotSupportedException("Anonymous pipes can only be in one direction.");
		}
		if (inheritability < HandleInheritability.None || inheritability > HandleInheritability.Inheritable)
		{
			throw new ArgumentOutOfRangeException("inheritability", "HandleInheritability.None or HandleInheritability.Inheritable required.");
		}
		Create(direction, inheritability, bufferSize);
	}

	~AnonymousPipeServerStream()
	{
		Dispose(disposing: false);
	}

	/// <summary>Gets the connected <see cref="T:System.IO.Pipes.AnonymousPipeClientStream" /> object's handle as a string.</summary>
	/// <returns>A string that represents the connected <see cref="T:System.IO.Pipes.AnonymousPipeClientStream" /> object's handle.</returns>
	public string GetClientHandleAsString()
	{
		_clientHandleExposed = true;
		GC.SuppressFinalize(_clientHandle);
		return _clientHandle.DangerousGetHandle().ToString();
	}

	/// <summary>Closes the local copy of the <see cref="T:System.IO.Pipes.AnonymousPipeClientStream" /> object's handle.</summary>
	public void DisposeLocalCopyOfClientHandle()
	{
		if (_clientHandle != null && !_clientHandle.IsClosed)
		{
			_clientHandle.Dispose();
		}
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (!_clientHandleExposed && _clientHandle != null && !_clientHandle.IsClosed)
			{
				_clientHandle.Dispose();
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.AnonymousPipeServerStream" /> class with the specified pipe direction, inheritability mode, buffer size, and pipe security.</summary>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.Anonymous pipes can only be in one direction, so <paramref name="direction" /> cannot be set to <see cref="F:System.IO.Pipes.PipeDirection.InOut" />.</param>
	/// <param name="inheritability">One of the enumeration values that determines whether the underlying handle can be inherited by child processes.</param>
	/// <param name="bufferSize">The size of the buffer. This value must be greater than or equal to 0. </param>
	/// <param name="pipeSecurity">An object that determines the access control and audit security for the pipe.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="inheritability" /> is not set to either <see cref="F:System.IO.HandleInheritability.None" /> or <see cref="F:System.IO.HandleInheritability.Inheritable" />.-or-<paramref name="bufferSize" /> is less than 0.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="direction" /> is set to <see cref="F:System.IO.Pipes.PipeDirection.InOut" />.</exception>
	public AnonymousPipeServerStream(PipeDirection direction, HandleInheritability inheritability, int bufferSize, PipeSecurity pipeSecurity)
		: base(direction, bufferSize)
	{
		if (direction == PipeDirection.InOut)
		{
			throw new NotSupportedException("Anonymous pipes can only be in one direction.");
		}
		if (inheritability < HandleInheritability.None || inheritability > HandleInheritability.Inheritable)
		{
			throw new ArgumentOutOfRangeException("inheritability", "HandleInheritability.None or HandleInheritability.Inheritable required.");
		}
		Create(direction, inheritability, bufferSize, pipeSecurity);
	}
}
