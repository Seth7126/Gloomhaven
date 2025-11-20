using Microsoft.Win32.SafeHandles;

namespace System.IO.Pipes;

/// <summary>Exposes the client side of an anonymous pipe stream, which supports both synchronous and asynchronous read and write operations.</summary>
public sealed class AnonymousPipeClientStream : PipeStream
{
	/// <summary>Gets the pipe transmission mode supported by the current pipe.</summary>
	/// <returns>The <see cref="T:System.IO.Pipes.PipeTransmissionMode" /> supported by the current pipe.</returns>
	public override PipeTransmissionMode TransmissionMode => PipeTransmissionMode.Byte;

	/// <summary>Sets the reading mode for the <see cref="T:System.IO.Pipes.AnonymousPipeClientStream" /> object.</summary>
	/// <returns>The <see cref="T:System.IO.Pipes.PipeTransmissionMode" /> for the <see cref="T:System.IO.Pipes.AnonymousPipeClientStream" /> object.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The transmission mode is not valid. For anonymous pipes, only <see cref="F:System.IO.Pipes.PipeTransmissionMode.Byte" /> is supported.</exception>
	/// <exception cref="T:System.NotSupportedException">The transmission mode is <see cref="F:System.IO.Pipes.PipeTransmissionMode.Message" />.</exception>
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

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.AnonymousPipeClientStream" /> class with the specified string representation of the pipe handle.</summary>
	/// <param name="pipeHandleAsString">A string that represents the pipe handle.</param>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="pipeHandleAsString" /> is not a valid pipe handle.</exception>
	public AnonymousPipeClientStream(string pipeHandleAsString)
		: this(PipeDirection.In, pipeHandleAsString)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.AnonymousPipeClientStream" /> class with the specified pipe direction and a string representation of the pipe handle.</summary>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.Anonymous pipes can only be in one direction, so <paramref name="direction" /> cannot be set to <see cref="F:System.IO.Pipes.PipeDirection.InOut" />.</param>
	/// <param name="pipeHandleAsString">A string that represents the pipe handle.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeHandleAsString" /> is an invalid handle.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeHandleAsString" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="direction" /> is set to <see cref="F:System.IO.Pipes.PipeDirection.InOut" />.</exception>
	public AnonymousPipeClientStream(PipeDirection direction, string pipeHandleAsString)
		: base(direction, 0)
	{
		if (direction == PipeDirection.InOut)
		{
			throw new NotSupportedException("Anonymous pipes can only be in one direction.");
		}
		if (pipeHandleAsString == null)
		{
			throw new ArgumentNullException("pipeHandleAsString");
		}
		long result = 0L;
		if (!long.TryParse(pipeHandleAsString, out result))
		{
			throw new ArgumentException("Invalid handle.", "pipeHandleAsString");
		}
		SafePipeHandle safePipeHandle = new SafePipeHandle((IntPtr)result, ownsHandle: true);
		if (safePipeHandle.IsInvalid)
		{
			throw new ArgumentException("Invalid handle.", "pipeHandleAsString");
		}
		Init(direction, safePipeHandle);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.AnonymousPipeClientStream" /> class from the specified handle.</summary>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.Anonymous pipes can only be in one direction, so <paramref name="direction" /> cannot be set to <see cref="F:System.IO.Pipes.PipeDirection.InOut" />.</param>
	/// <param name="safePipeHandle">A safe handle for the pipe that this <see cref="T:System.IO.Pipes.AnonymousPipeClientStream" /> object will encapsulate.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="safePipeHandle " />is not a valid handle.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="safePipeHandle" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="direction" /> is set to <see cref="F:System.IO.Pipes.PipeDirection.InOut" />.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error, such as a disk error, has occurred.-or-The stream has been closed.</exception>
	public AnonymousPipeClientStream(PipeDirection direction, SafePipeHandle safePipeHandle)
		: base(direction, 0)
	{
		if (direction == PipeDirection.InOut)
		{
			throw new NotSupportedException("Anonymous pipes can only be in one direction.");
		}
		if (safePipeHandle == null)
		{
			throw new ArgumentNullException("safePipeHandle");
		}
		if (safePipeHandle.IsInvalid)
		{
			throw new ArgumentException("Invalid handle.", "safePipeHandle");
		}
		Init(direction, safePipeHandle);
	}

	private void Init(PipeDirection direction, SafePipeHandle safePipeHandle)
	{
		ValidateHandleIsPipe(safePipeHandle);
		InitializeHandle(safePipeHandle, isExposed: true, isAsync: false);
		base.State = PipeState.Connected;
	}

	~AnonymousPipeClientStream()
	{
		Dispose(disposing: false);
	}
}
