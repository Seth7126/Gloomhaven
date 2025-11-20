using System.Buffers;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO;

/// <summary>Provides a generic view of a sequence of bytes.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
public abstract class Stream : MarshalByRefObject, IDisposable, IAsyncDisposable
{
	private struct ReadWriteParameters
	{
		internal byte[] Buffer;

		internal int Offset;

		internal int Count;
	}

	private sealed class ReadWriteTask : Task<int>, ITaskCompletionAction
	{
		internal readonly bool _isRead;

		internal readonly bool _apm;

		internal Stream _stream;

		internal byte[] _buffer;

		internal readonly int _offset;

		internal readonly int _count;

		private AsyncCallback _callback;

		private ExecutionContext _context;

		private static ContextCallback s_invokeAsyncCallback;

		bool ITaskCompletionAction.InvokeMayRunArbitraryCode => true;

		internal void ClearBeginState()
		{
			_stream = null;
			_buffer = null;
		}

		public ReadWriteTask(bool isRead, bool apm, Func<object, int> function, object state, Stream stream, byte[] buffer, int offset, int count, AsyncCallback callback)
			: base(function, state, CancellationToken.None, TaskCreationOptions.DenyChildAttach)
		{
			_isRead = isRead;
			_apm = apm;
			_stream = stream;
			_buffer = buffer;
			_offset = offset;
			_count = count;
			if (callback != null)
			{
				_callback = callback;
				_context = ExecutionContext.Capture();
				AddCompletionAction(this);
			}
		}

		private static void InvokeAsyncCallback(object completedTask)
		{
			ReadWriteTask readWriteTask = (ReadWriteTask)completedTask;
			AsyncCallback callback = readWriteTask._callback;
			readWriteTask._callback = null;
			callback(readWriteTask);
		}

		void ITaskCompletionAction.Invoke(Task completingTask)
		{
			ExecutionContext context = _context;
			if (context == null)
			{
				AsyncCallback callback = _callback;
				_callback = null;
				callback(completingTask);
			}
			else
			{
				_context = null;
				ContextCallback callback2 = InvokeAsyncCallback;
				ExecutionContext.RunInternal(context, callback2, this);
			}
		}
	}

	private sealed class NullStream : Stream
	{
		private static readonly Task<int> s_zeroTask = Task.FromResult(0);

		public override bool CanRead => true;

		public override bool CanWrite => true;

		public override bool CanSeek => true;

		public override long Length => 0L;

		public override long Position
		{
			get
			{
				return 0L;
			}
			set
			{
			}
		}

		internal NullStream()
		{
		}

		public override void CopyTo(Stream destination, int bufferSize)
		{
			StreamHelpers.ValidateCopyToArgs(this, destination, bufferSize);
		}

		public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
		{
			StreamHelpers.ValidateCopyToArgs(this, destination, bufferSize);
			if (!cancellationToken.IsCancellationRequested)
			{
				return Task.CompletedTask;
			}
			return Task.FromCanceled(cancellationToken);
		}

		protected override void Dispose(bool disposing)
		{
		}

		public override void Flush()
		{
		}

		public override Task FlushAsync(CancellationToken cancellationToken)
		{
			if (!cancellationToken.IsCancellationRequested)
			{
				return Task.CompletedTask;
			}
			return Task.FromCanceled(cancellationToken);
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			if (!CanRead)
			{
				throw Error.GetReadNotSupported();
			}
			return BlockingBeginRead(buffer, offset, count, callback, state);
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			return BlockingEndRead(asyncResult);
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			if (!CanWrite)
			{
				throw Error.GetWriteNotSupported();
			}
			return BlockingBeginWrite(buffer, offset, count, callback, state);
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			BlockingEndWrite(asyncResult);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return 0;
		}

		public override int Read(Span<byte> buffer)
		{
			return 0;
		}

		public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			return s_zeroTask;
		}

		public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default(CancellationToken))
		{
			return new ValueTask<int>(0);
		}

		public override int ReadByte()
		{
			return -1;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
		}

		public override void Write(ReadOnlySpan<byte> buffer)
		{
		}

		public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			if (!cancellationToken.IsCancellationRequested)
			{
				return Task.CompletedTask;
			}
			return Task.FromCanceled(cancellationToken);
		}

		public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (!cancellationToken.IsCancellationRequested)
			{
				return default(ValueTask);
			}
			return new ValueTask(Task.FromCanceled(cancellationToken));
		}

		public override void WriteByte(byte value)
		{
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return 0L;
		}

		public override void SetLength(long length)
		{
		}
	}

	private sealed class SynchronousAsyncResult : IAsyncResult
	{
		private readonly object _stateObject;

		private readonly bool _isWrite;

		private ManualResetEvent _waitHandle;

		private ExceptionDispatchInfo _exceptionInfo;

		private bool _endXxxCalled;

		private int _bytesRead;

		public bool IsCompleted => true;

		public WaitHandle AsyncWaitHandle => LazyInitializer.EnsureInitialized(ref _waitHandle, () => new ManualResetEvent(initialState: true));

		public object AsyncState => _stateObject;

		public bool CompletedSynchronously => true;

		internal SynchronousAsyncResult(int bytesRead, object asyncStateObject)
		{
			_bytesRead = bytesRead;
			_stateObject = asyncStateObject;
		}

		internal SynchronousAsyncResult(object asyncStateObject)
		{
			_stateObject = asyncStateObject;
			_isWrite = true;
		}

		internal SynchronousAsyncResult(Exception ex, object asyncStateObject, bool isWrite)
		{
			_exceptionInfo = ExceptionDispatchInfo.Capture(ex);
			_stateObject = asyncStateObject;
			_isWrite = isWrite;
		}

		internal void ThrowIfError()
		{
			if (_exceptionInfo != null)
			{
				_exceptionInfo.Throw();
			}
		}

		internal static int EndRead(IAsyncResult asyncResult)
		{
			if (!(asyncResult is SynchronousAsyncResult { _isWrite: false } synchronousAsyncResult))
			{
				throw new ArgumentException("IAsyncResult object did not come from the corresponding async method on this type.");
			}
			if (synchronousAsyncResult._endXxxCalled)
			{
				throw new ArgumentException("EndRead can only be called once for each asynchronous operation.");
			}
			synchronousAsyncResult._endXxxCalled = true;
			synchronousAsyncResult.ThrowIfError();
			return synchronousAsyncResult._bytesRead;
		}

		internal static void EndWrite(IAsyncResult asyncResult)
		{
			if (!(asyncResult is SynchronousAsyncResult { _isWrite: not false } synchronousAsyncResult))
			{
				throw new ArgumentException("IAsyncResult object did not come from the corresponding async method on this type.");
			}
			if (synchronousAsyncResult._endXxxCalled)
			{
				throw new ArgumentException("EndWrite can only be called once for each asynchronous operation.");
			}
			synchronousAsyncResult._endXxxCalled = true;
			synchronousAsyncResult.ThrowIfError();
		}
	}

	private sealed class SyncStream : Stream, IDisposable
	{
		private Stream _stream;

		public override bool CanRead => _stream.CanRead;

		public override bool CanWrite => _stream.CanWrite;

		public override bool CanSeek => _stream.CanSeek;

		public override bool CanTimeout => _stream.CanTimeout;

		public override long Length
		{
			get
			{
				lock (_stream)
				{
					return _stream.Length;
				}
			}
		}

		public override long Position
		{
			get
			{
				lock (_stream)
				{
					return _stream.Position;
				}
			}
			set
			{
				lock (_stream)
				{
					_stream.Position = value;
				}
			}
		}

		public override int ReadTimeout
		{
			get
			{
				return _stream.ReadTimeout;
			}
			set
			{
				_stream.ReadTimeout = value;
			}
		}

		public override int WriteTimeout
		{
			get
			{
				return _stream.WriteTimeout;
			}
			set
			{
				_stream.WriteTimeout = value;
			}
		}

		internal SyncStream(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			_stream = stream;
		}

		public override void Close()
		{
			lock (_stream)
			{
				try
				{
					_stream.Close();
				}
				finally
				{
					base.Dispose(disposing: true);
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			lock (_stream)
			{
				try
				{
					if (disposing)
					{
						((IDisposable)_stream).Dispose();
					}
				}
				finally
				{
					base.Dispose(disposing);
				}
			}
		}

		public override void Flush()
		{
			lock (_stream)
			{
				_stream.Flush();
			}
		}

		public override int Read(byte[] bytes, int offset, int count)
		{
			lock (_stream)
			{
				return _stream.Read(bytes, offset, count);
			}
		}

		public override int Read(Span<byte> buffer)
		{
			lock (_stream)
			{
				return _stream.Read(buffer);
			}
		}

		public override int ReadByte()
		{
			lock (_stream)
			{
				return _stream.ReadByte();
			}
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			bool flag = _stream.HasOverriddenBeginEndRead();
			lock (_stream)
			{
				return flag ? _stream.BeginRead(buffer, offset, count, callback, state) : _stream.BeginReadInternal(buffer, offset, count, callback, state, serializeAsynchronously: true, apm: true);
			}
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			lock (_stream)
			{
				return _stream.EndRead(asyncResult);
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			lock (_stream)
			{
				return _stream.Seek(offset, origin);
			}
		}

		public override void SetLength(long length)
		{
			lock (_stream)
			{
				_stream.SetLength(length);
			}
		}

		public override void Write(byte[] bytes, int offset, int count)
		{
			lock (_stream)
			{
				_stream.Write(bytes, offset, count);
			}
		}

		public override void Write(ReadOnlySpan<byte> buffer)
		{
			lock (_stream)
			{
				_stream.Write(buffer);
			}
		}

		public override void WriteByte(byte b)
		{
			lock (_stream)
			{
				_stream.WriteByte(b);
			}
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			bool flag = _stream.HasOverriddenBeginEndWrite();
			lock (_stream)
			{
				return flag ? _stream.BeginWrite(buffer, offset, count, callback, state) : _stream.BeginWriteInternal(buffer, offset, count, callback, state, serializeAsynchronously: true, apm: true);
			}
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			lock (_stream)
			{
				_stream.EndWrite(asyncResult);
			}
		}
	}

	/// <summary>A Stream with no backing store.</summary>
	/// <filterpriority>1</filterpriority>
	public static readonly Stream Null = new NullStream();

	private const int DefaultCopyBufferSize = 81920;

	[NonSerialized]
	private ReadWriteTask _activeReadWriteTask;

	[NonSerialized]
	private SemaphoreSlim _asyncActiveSemaphore;

	/// <summary>When overridden in a derived class, gets a value indicating whether the current stream supports reading.</summary>
	/// <returns>true if the stream supports reading; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	public abstract bool CanRead { get; }

	/// <summary>When overridden in a derived class, gets a value indicating whether the current stream supports seeking.</summary>
	/// <returns>true if the stream supports seeking; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	public abstract bool CanSeek { get; }

	/// <summary>Gets a value that determines whether the current stream can time out.</summary>
	/// <returns>A value that determines whether the current stream can time out.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual bool CanTimeout => false;

	/// <summary>When overridden in a derived class, gets a value indicating whether the current stream supports writing.</summary>
	/// <returns>true if the stream supports writing; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	public abstract bool CanWrite { get; }

	/// <summary>When overridden in a derived class, gets the length in bytes of the stream.</summary>
	/// <returns>A long value representing the length of the stream in bytes.</returns>
	/// <exception cref="T:System.NotSupportedException">A class derived from Stream does not support seeking. </exception>
	/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
	/// <filterpriority>1</filterpriority>
	public abstract long Length { get; }

	/// <summary>When overridden in a derived class, gets or sets the position within the current stream.</summary>
	/// <returns>The current position within the stream.</returns>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.NotSupportedException">The stream does not support seeking. </exception>
	/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
	/// <filterpriority>1</filterpriority>
	public abstract long Position { get; set; }

	/// <summary>Gets or sets a value, in miliseconds, that determines how long the stream will attempt to read before timing out. </summary>
	/// <returns>A value, in miliseconds, that determines how long the stream will attempt to read before timing out.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.IO.Stream.ReadTimeout" /> method always throws an <see cref="T:System.InvalidOperationException" />. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual int ReadTimeout
	{
		get
		{
			throw new InvalidOperationException("Timeouts are not supported on this stream.");
		}
		set
		{
			throw new InvalidOperationException("Timeouts are not supported on this stream.");
		}
	}

	/// <summary>Gets or sets a value, in miliseconds, that determines how long the stream will attempt to write before timing out. </summary>
	/// <returns>A value, in miliseconds, that determines how long the stream will attempt to write before timing out.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.IO.Stream.WriteTimeout" /> method always throws an <see cref="T:System.InvalidOperationException" />. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual int WriteTimeout
	{
		get
		{
			throw new InvalidOperationException("Timeouts are not supported on this stream.");
		}
		set
		{
			throw new InvalidOperationException("Timeouts are not supported on this stream.");
		}
	}

	internal SemaphoreSlim EnsureAsyncActiveSemaphoreInitialized()
	{
		return LazyInitializer.EnsureInitialized(ref _asyncActiveSemaphore, () => new SemaphoreSlim(1, 1));
	}

	/// <summary>Asynchronously reads the bytes from the current stream and writes them to another stream.</summary>
	/// <returns>A task that represents the asynchronous copy operation.</returns>
	/// <param name="destination">The stream to which the contents of the current stream will be copied.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destination" /> is null.</exception>
	/// <exception cref="T:System.ObjectDisposedException">Either the current stream or the destination stream is disposed.</exception>
	/// <exception cref="T:System.NotSupportedException">The current stream does not support reading, or the destination stream does not support writing.</exception>
	public Task CopyToAsync(Stream destination)
	{
		int copyBufferSize = GetCopyBufferSize();
		return CopyToAsync(destination, copyBufferSize);
	}

	/// <summary>Asynchronously reads the bytes from the current stream and writes them to another stream, using a specified buffer size.</summary>
	/// <returns>A task that represents the asynchronous copy operation.</returns>
	/// <param name="destination">The stream to which the contents of the current stream will be copied.</param>
	/// <param name="bufferSize">The size, in bytes, of the buffer. This value must be greater than zero. The default size is 4096.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destination" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="buffersize" /> is negative or zero.</exception>
	/// <exception cref="T:System.ObjectDisposedException">Either the current stream or the destination stream is disposed.</exception>
	/// <exception cref="T:System.NotSupportedException">The current stream does not support reading, or the destination stream does not support writing.</exception>
	public Task CopyToAsync(Stream destination, int bufferSize)
	{
		return CopyToAsync(destination, bufferSize, CancellationToken.None);
	}

	public Task CopyToAsync(Stream destination, CancellationToken cancellationToken)
	{
		int copyBufferSize = GetCopyBufferSize();
		return CopyToAsync(destination, copyBufferSize, cancellationToken);
	}

	/// <summary>Asynchronously reads the bytes from the current stream and writes them to another stream, using a specified buffer size and cancellation token.</summary>
	/// <returns>A task that represents the asynchronous copy operation.</returns>
	/// <param name="destination">The stream to which the contents of the current stream will be copied.</param>
	/// <param name="bufferSize">The size, in bytes, of the buffer. This value must be greater than zero. The default size is 4096.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destination" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="buffersize" /> is negative or zero.</exception>
	/// <exception cref="T:System.ObjectDisposedException">Either the current stream or the destination stream is disposed.</exception>
	/// <exception cref="T:System.NotSupportedException">The current stream does not support reading, or the destination stream does not support writing.</exception>
	public virtual Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
	{
		StreamHelpers.ValidateCopyToArgs(this, destination, bufferSize);
		return CopyToAsyncInternal(destination, bufferSize, cancellationToken);
	}

	private async Task CopyToAsyncInternal(Stream destination, int bufferSize, CancellationToken cancellationToken)
	{
		byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
		try
		{
			while (true)
			{
				int num = await ReadAsync(new Memory<byte>(buffer), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				if (num == 0)
				{
					break;
				}
				await destination.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, num), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
		}
	}

	/// <summary>Reads the bytes from the current stream and writes them to another stream.</summary>
	/// <param name="destination">The stream to which the contents of the current stream will be copied.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destination" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">The current stream does not support reading.-or-<paramref name="destination" /> does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">Either the current stream or <paramref name="destination" /> were closed before the <see cref="M:System.IO.Stream.CopyTo(System.IO.Stream)" /> method was called.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred.</exception>
	public void CopyTo(Stream destination)
	{
		int copyBufferSize = GetCopyBufferSize();
		CopyTo(destination, copyBufferSize);
	}

	/// <summary>Reads the bytes from the current stream and writes them to another stream, using a specified buffer size.</summary>
	/// <param name="destination">The stream to which the contents of the current stream will be copied.</param>
	/// <param name="bufferSize">The size of the buffer. This value must be greater than zero. The default size is 4096.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destination" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="bufferSize" /> is negative or zero.</exception>
	/// <exception cref="T:System.NotSupportedException">The current stream does not support reading.-or-<paramref name="destination" /> does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">Either the current stream or <paramref name="destination" /> were closed before the <see cref="M:System.IO.Stream.CopyTo(System.IO.Stream)" /> method was called.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred.</exception>
	public virtual void CopyTo(Stream destination, int bufferSize)
	{
		StreamHelpers.ValidateCopyToArgs(this, destination, bufferSize);
		byte[] array = ArrayPool<byte>.Shared.Rent(bufferSize);
		try
		{
			int count;
			while ((count = Read(array, 0, array.Length)) != 0)
			{
				destination.Write(array, 0, count);
			}
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(array);
		}
	}

	private int GetCopyBufferSize()
	{
		int num = 81920;
		if (CanSeek)
		{
			long length = Length;
			long position = Position;
			if (length <= position)
			{
				num = 1;
			}
			else
			{
				long num2 = length - position;
				if (num2 > 0)
				{
					num = (int)Math.Min(num, num2);
				}
			}
		}
		return num;
	}

	/// <summary>Closes the current stream and releases any resources (such as sockets and file handles) associated with the current stream. Instead of calling this method, ensure that the stream is properly disposed.</summary>
	/// <filterpriority>1</filterpriority>
	public virtual void Close()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases all resources used by the <see cref="T:System.IO.Stream" />.</summary>
	public void Dispose()
	{
		Close();
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.IO.Stream" /> and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	protected virtual void Dispose(bool disposing)
	{
	}

	/// <summary>When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.</summary>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>2</filterpriority>
	public abstract void Flush();

	/// <summary>Asynchronously clears all buffers for this stream and causes any buffered data to be written to the underlying device.</summary>
	/// <returns>A task that represents the asynchronous flush operation.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
	public Task FlushAsync()
	{
		return FlushAsync(CancellationToken.None);
	}

	/// <summary>Asynchronously clears all buffers for this stream, causes any buffered data to be written to the underlying device, and monitors cancellation requests.</summary>
	/// <returns>A task that represents the asynchronous flush operation.</returns>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
	public virtual Task FlushAsync(CancellationToken cancellationToken)
	{
		return Task.Factory.StartNew(delegate(object state)
		{
			((Stream)state).Flush();
		}, this, cancellationToken, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
	}

	/// <summary>Allocates a <see cref="T:System.Threading.WaitHandle" /> object.</summary>
	/// <returns>A reference to the allocated WaitHandle.</returns>
	[Obsolete("CreateWaitHandle will be removed eventually.  Please use \"new ManualResetEvent(false)\" instead.")]
	protected virtual WaitHandle CreateWaitHandle()
	{
		return new ManualResetEvent(initialState: false);
	}

	/// <summary>Begins an asynchronous read operation. (Consider using <see cref="M:System.IO.Stream.ReadAsync(System.Byte[],System.Int32,System.Int32)" /> instead; see the Remarks section.)</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that represents the asynchronous read, which could still be pending.</returns>
	/// <param name="buffer">The buffer to read the data into. </param>
	/// <param name="offset">The byte offset in <paramref name="buffer" /> at which to begin writing data read from the stream. </param>
	/// <param name="count">The maximum number of bytes to read. </param>
	/// <param name="callback">An optional asynchronous callback, to be called when the read is complete. </param>
	/// <param name="state">A user-provided object that distinguishes this particular asynchronous read request from other requests. </param>
	/// <exception cref="T:System.IO.IOException">Attempted an asynchronous read past the end of the stream, or a disk error occurs. </exception>
	/// <exception cref="T:System.ArgumentException">One or more of the arguments is invalid. </exception>
	/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
	/// <exception cref="T:System.NotSupportedException">The current Stream implementation does not support the read operation. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		return BeginReadInternal(buffer, offset, count, callback, state, serializeAsynchronously: false, apm: true);
	}

	internal IAsyncResult BeginReadInternal(byte[] buffer, int offset, int count, AsyncCallback callback, object state, bool serializeAsynchronously, bool apm)
	{
		if (!CanRead)
		{
			throw Error.GetReadNotSupported();
		}
		SemaphoreSlim semaphoreSlim = EnsureAsyncActiveSemaphoreInitialized();
		Task task = null;
		if (serializeAsynchronously)
		{
			task = semaphoreSlim.WaitAsync();
		}
		else
		{
			semaphoreSlim.Wait();
		}
		ReadWriteTask readWriteTask = new ReadWriteTask(isRead: true, apm, delegate
		{
			ReadWriteTask readWriteTask2 = Task.InternalCurrent as ReadWriteTask;
			try
			{
				return readWriteTask2._stream.Read(readWriteTask2._buffer, readWriteTask2._offset, readWriteTask2._count);
			}
			finally
			{
				if (!readWriteTask2._apm)
				{
					readWriteTask2._stream.FinishTrackingAsyncOperation();
				}
				readWriteTask2.ClearBeginState();
			}
		}, state, this, buffer, offset, count, callback);
		if (task != null)
		{
			RunReadWriteTaskWhenReady(task, readWriteTask);
		}
		else
		{
			RunReadWriteTask(readWriteTask);
		}
		return readWriteTask;
	}

	/// <summary>Waits for the pending asynchronous read to complete. (Consider using <see cref="M:System.IO.Stream.ReadAsync(System.Byte[],System.Int32,System.Int32)" /> instead; see the Remarks section.)</summary>
	/// <returns>The number of bytes read from the stream, between zero (0) and the number of bytes you requested. Streams return zero (0) only at the end of the stream, otherwise, they should block until at least one byte is available.</returns>
	/// <param name="asyncResult">The reference to the pending asynchronous request to finish. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">A handle to the pending read operation is not available.-or-The pending operation does not support reading.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="asyncResult" /> did not originate from a <see cref="M:System.IO.Stream.BeginRead(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)" /> method on the current stream.</exception>
	/// <exception cref="T:System.IO.IOException">The stream is closed or an internal error has occurred.</exception>
	/// <filterpriority>2</filterpriority>
	public virtual int EndRead(IAsyncResult asyncResult)
	{
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		ReadWriteTask activeReadWriteTask = _activeReadWriteTask;
		if (activeReadWriteTask == null)
		{
			throw new ArgumentException("Either the IAsyncResult object did not come from the corresponding async method on this type, or EndRead was called multiple times with the same IAsyncResult.");
		}
		if (activeReadWriteTask != asyncResult)
		{
			throw new InvalidOperationException("Either the IAsyncResult object did not come from the corresponding async method on this type, or EndRead was called multiple times with the same IAsyncResult.");
		}
		if (!activeReadWriteTask._isRead)
		{
			throw new ArgumentException("Either the IAsyncResult object did not come from the corresponding async method on this type, or EndRead was called multiple times with the same IAsyncResult.");
		}
		try
		{
			return activeReadWriteTask.GetAwaiter().GetResult();
		}
		finally
		{
			FinishTrackingAsyncOperation();
		}
	}

	/// <summary>Asynchronously reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.</summary>
	/// <returns>A task that represents the asynchronous read operation. The value of the <paramref name="TResult" /> parameter contains the total number of bytes read into the buffer. The result value can be less than the number of bytes requested if the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the stream has been reached. </returns>
	/// <param name="buffer">The buffer to write the data into.</param>
	/// <param name="offset">The byte offset in <paramref name="buffer" /> at which to begin writing data from the stream.</param>
	/// <param name="count">The maximum number of bytes to read.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="count" /> is negative.</exception>
	/// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset" /> and <paramref name="count" /> is larger than the buffer length.</exception>
	/// <exception cref="T:System.NotSupportedException">The stream does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The stream is currently in use by a previous read operation. </exception>
	public Task<int> ReadAsync(byte[] buffer, int offset, int count)
	{
		return ReadAsync(buffer, offset, count, CancellationToken.None);
	}

	/// <summary>Asynchronously reads a sequence of bytes from the current stream, advances the position within the stream by the number of bytes read, and monitors cancellation requests.</summary>
	/// <returns>A task that represents the asynchronous read operation. The value of the <paramref name="TResult" /> parameter contains the total number of bytes read into the buffer. The result value can be less than the number of bytes requested if the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the stream has been reached. </returns>
	/// <param name="buffer">The buffer to write the data into.</param>
	/// <param name="offset">The byte offset in <paramref name="buffer" /> at which to begin writing data from the stream.</param>
	/// <param name="count">The maximum number of bytes to read.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="count" /> is negative.</exception>
	/// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset" /> and <paramref name="count" /> is larger than the buffer length.</exception>
	/// <exception cref="T:System.NotSupportedException">The stream does not support reading.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The stream is currently in use by a previous read operation. </exception>
	public virtual Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		if (!cancellationToken.IsCancellationRequested)
		{
			return BeginEndReadAsync(buffer, offset, count);
		}
		return Task.FromCanceled<int>(cancellationToken);
	}

	public virtual ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (MemoryMarshal.TryGetArray((ReadOnlyMemory<byte>)buffer, out ArraySegment<byte> segment))
		{
			return new ValueTask<int>(ReadAsync(segment.Array, segment.Offset, segment.Count, cancellationToken));
		}
		byte[] array = ArrayPool<byte>.Shared.Rent(buffer.Length);
		return FinishReadAsync(ReadAsync(array, 0, buffer.Length, cancellationToken), array, buffer);
		static async ValueTask<int> FinishReadAsync(Task<int> readTask, byte[] localBuffer, Memory<byte> localDestination)
		{
			try
			{
				int num = await readTask.ConfigureAwait(continueOnCapturedContext: false);
				new Span<byte>(localBuffer, 0, num).CopyTo(localDestination.Span);
				return num;
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(localBuffer);
			}
		}
	}

	private Task<int> BeginEndReadAsync(byte[] buffer, int offset, int count)
	{
		if (!HasOverriddenBeginEndRead())
		{
			return (Task<int>)BeginReadInternal(buffer, offset, count, null, null, serializeAsynchronously: true, apm: false);
		}
		return TaskFactory<int>.FromAsyncTrim(this, new ReadWriteParameters
		{
			Buffer = buffer,
			Offset = offset,
			Count = count
		}, (Stream stream, ReadWriteParameters args, AsyncCallback callback, object state) => stream.BeginRead(args.Buffer, args.Offset, args.Count, callback, state), (Stream stream, IAsyncResult asyncResult) => stream.EndRead(asyncResult));
	}

	/// <summary>Begins an asynchronous write operation. (Consider using <see cref="M:System.IO.Stream.WriteAsync(System.Byte[],System.Int32,System.Int32)" /> instead; see the Remarks section.)</summary>
	/// <returns>An IAsyncResult that represents the asynchronous write, which could still be pending.</returns>
	/// <param name="buffer">The buffer to write data from. </param>
	/// <param name="offset">The byte offset in <paramref name="buffer" /> from which to begin writing. </param>
	/// <param name="count">The maximum number of bytes to write. </param>
	/// <param name="callback">An optional asynchronous callback, to be called when the write is complete. </param>
	/// <param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests. </param>
	/// <exception cref="T:System.IO.IOException">Attempted an asynchronous write past the end of the stream, or a disk error occurs. </exception>
	/// <exception cref="T:System.ArgumentException">One or more of the arguments is invalid. </exception>
	/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
	/// <exception cref="T:System.NotSupportedException">The current Stream implementation does not support the write operation. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		return BeginWriteInternal(buffer, offset, count, callback, state, serializeAsynchronously: false, apm: true);
	}

	internal IAsyncResult BeginWriteInternal(byte[] buffer, int offset, int count, AsyncCallback callback, object state, bool serializeAsynchronously, bool apm)
	{
		if (!CanWrite)
		{
			throw Error.GetWriteNotSupported();
		}
		SemaphoreSlim semaphoreSlim = EnsureAsyncActiveSemaphoreInitialized();
		Task task = null;
		if (serializeAsynchronously)
		{
			task = semaphoreSlim.WaitAsync();
		}
		else
		{
			semaphoreSlim.Wait();
		}
		ReadWriteTask readWriteTask = new ReadWriteTask(isRead: false, apm, delegate
		{
			ReadWriteTask readWriteTask2 = Task.InternalCurrent as ReadWriteTask;
			try
			{
				readWriteTask2._stream.Write(readWriteTask2._buffer, readWriteTask2._offset, readWriteTask2._count);
				return 0;
			}
			finally
			{
				if (!readWriteTask2._apm)
				{
					readWriteTask2._stream.FinishTrackingAsyncOperation();
				}
				readWriteTask2.ClearBeginState();
			}
		}, state, this, buffer, offset, count, callback);
		if (task != null)
		{
			RunReadWriteTaskWhenReady(task, readWriteTask);
		}
		else
		{
			RunReadWriteTask(readWriteTask);
		}
		return readWriteTask;
	}

	private void RunReadWriteTaskWhenReady(Task asyncWaiter, ReadWriteTask readWriteTask)
	{
		if (asyncWaiter.IsCompleted)
		{
			RunReadWriteTask(readWriteTask);
			return;
		}
		asyncWaiter.ContinueWith(delegate(Task t, object state)
		{
			ReadWriteTask readWriteTask2 = (ReadWriteTask)state;
			readWriteTask2._stream.RunReadWriteTask(readWriteTask2);
		}, readWriteTask, default(CancellationToken), TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
	}

	private void RunReadWriteTask(ReadWriteTask readWriteTask)
	{
		_activeReadWriteTask = readWriteTask;
		readWriteTask.m_taskScheduler = TaskScheduler.Default;
		readWriteTask.ScheduleAndStart(needsProtection: false);
	}

	private void FinishTrackingAsyncOperation()
	{
		_activeReadWriteTask = null;
		_asyncActiveSemaphore.Release();
	}

	/// <summary>Ends an asynchronous write operation. (Consider using <see cref="M:System.IO.Stream.WriteAsync(System.Byte[],System.Int32,System.Int32)" /> instead; see the Remarks section.)</summary>
	/// <param name="asyncResult">A reference to the outstanding asynchronous I/O request. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">A handle to the pending write operation is not available.-or-The pending operation does not support writing.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="asyncResult" /> did not originate from a <see cref="M:System.IO.Stream.BeginWrite(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)" /> method on the current stream.</exception>
	/// <exception cref="T:System.IO.IOException">The stream is closed or an internal error has occurred.</exception>
	/// <filterpriority>2</filterpriority>
	public virtual void EndWrite(IAsyncResult asyncResult)
	{
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		ReadWriteTask activeReadWriteTask = _activeReadWriteTask;
		if (activeReadWriteTask == null)
		{
			throw new ArgumentException("Either the IAsyncResult object did not come from the corresponding async method on this type, or EndWrite was called multiple times with the same IAsyncResult.");
		}
		if (activeReadWriteTask != asyncResult)
		{
			throw new InvalidOperationException("Either the IAsyncResult object did not come from the corresponding async method on this type, or EndWrite was called multiple times with the same IAsyncResult.");
		}
		if (activeReadWriteTask._isRead)
		{
			throw new ArgumentException("Either the IAsyncResult object did not come from the corresponding async method on this type, or EndWrite was called multiple times with the same IAsyncResult.");
		}
		try
		{
			activeReadWriteTask.GetAwaiter().GetResult();
		}
		finally
		{
			FinishTrackingAsyncOperation();
		}
	}

	/// <summary>Asynchronously writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.</summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="buffer">The buffer to write data from.</param>
	/// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> from which to begin copying bytes to the stream.</param>
	/// <param name="count">The maximum number of bytes to write.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="count" /> is negative.</exception>
	/// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset" /> and <paramref name="count" /> is larger than the buffer length.</exception>
	/// <exception cref="T:System.NotSupportedException">The stream does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The stream is currently in use by a previous write operation. </exception>
	public Task WriteAsync(byte[] buffer, int offset, int count)
	{
		return WriteAsync(buffer, offset, count, CancellationToken.None);
	}

	/// <summary>Asynchronously writes a sequence of bytes to the current stream, advances the current position within this stream by the number of bytes written, and monitors cancellation requests.</summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="buffer">The buffer to write data from.</param>
	/// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> from which to begin copying bytes to the stream.</param>
	/// <param name="count">The maximum number of bytes to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="count" /> is negative.</exception>
	/// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset" /> and <paramref name="count" /> is larger than the buffer length.</exception>
	/// <exception cref="T:System.NotSupportedException">The stream does not support writing.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The stream is currently in use by a previous write operation. </exception>
	public virtual Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		if (!cancellationToken.IsCancellationRequested)
		{
			return BeginEndWriteAsync(buffer, offset, count);
		}
		return Task.FromCanceled(cancellationToken);
	}

	public virtual ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (MemoryMarshal.TryGetArray(buffer, out var segment))
		{
			return new ValueTask(WriteAsync(segment.Array, segment.Offset, segment.Count, cancellationToken));
		}
		byte[] array = ArrayPool<byte>.Shared.Rent(buffer.Length);
		buffer.Span.CopyTo(array);
		return new ValueTask(FinishWriteAsync(WriteAsync(array, 0, buffer.Length, cancellationToken), array));
	}

	private async Task FinishWriteAsync(Task writeTask, byte[] localBuffer)
	{
		try
		{
			await writeTask.ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(localBuffer);
		}
	}

	private Task BeginEndWriteAsync(byte[] buffer, int offset, int count)
	{
		if (!HasOverriddenBeginEndWrite())
		{
			return (Task)BeginWriteInternal(buffer, offset, count, null, null, serializeAsynchronously: true, apm: false);
		}
		return TaskFactory<VoidTaskResult>.FromAsyncTrim(this, new ReadWriteParameters
		{
			Buffer = buffer,
			Offset = offset,
			Count = count
		}, (Stream stream, ReadWriteParameters args, AsyncCallback callback, object state) => stream.BeginWrite(args.Buffer, args.Offset, args.Count, callback, state), delegate(Stream stream, IAsyncResult asyncResult)
		{
			stream.EndWrite(asyncResult);
			return default(VoidTaskResult);
		});
	}

	/// <summary>When overridden in a derived class, sets the position within the current stream.</summary>
	/// <returns>The new position within the current stream.</returns>
	/// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter. </param>
	/// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to obtain the new position. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. </exception>
	/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
	/// <filterpriority>1</filterpriority>
	public abstract long Seek(long offset, SeekOrigin origin);

	/// <summary>When overridden in a derived class, sets the length of the current stream.</summary>
	/// <param name="value">The desired length of the current stream in bytes. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.NotSupportedException">The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output. </exception>
	/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
	/// <filterpriority>2</filterpriority>
	public abstract void SetLength(long value);

	/// <summary>When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.</summary>
	/// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
	/// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source. </param>
	/// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream. </param>
	/// <param name="count">The maximum number of bytes to be read from the current stream. </param>
	/// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset" /> and <paramref name="count" /> is larger than the buffer length. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="count" /> is negative. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
	/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
	/// <filterpriority>1</filterpriority>
	public abstract int Read(byte[] buffer, int offset, int count);

	public virtual int Read(Span<byte> buffer)
	{
		byte[] array = ArrayPool<byte>.Shared.Rent(buffer.Length);
		try
		{
			int num = Read(array, 0, buffer.Length);
			if ((uint)num > buffer.Length)
			{
				throw new IOException("Stream was too long.");
			}
			new Span<byte>(array, 0, num).CopyTo(buffer);
			return num;
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(array);
		}
	}

	/// <summary>Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.</summary>
	/// <returns>The unsigned byte cast to an Int32, or -1 if at the end of the stream.</returns>
	/// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
	/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual int ReadByte()
	{
		byte[] array = new byte[1];
		if (Read(array, 0, 1) == 0)
		{
			return -1;
		}
		return array[0];
	}

	/// <summary>When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.</summary>
	/// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream. </param>
	/// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream. </param>
	/// <param name="count">The number of bytes to be written to the current stream. </param>
	/// <filterpriority>1</filterpriority>
	public abstract void Write(byte[] buffer, int offset, int count);

	public virtual void Write(ReadOnlySpan<byte> buffer)
	{
		byte[] array = ArrayPool<byte>.Shared.Rent(buffer.Length);
		try
		{
			buffer.CopyTo(array);
			Write(array, 0, buffer.Length);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(array);
		}
	}

	/// <summary>Writes a byte to the current position in the stream and advances the position within the stream by one byte.</summary>
	/// <param name="value">The byte to write to the stream. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.NotSupportedException">The stream does not support writing, or the stream is already closed. </exception>
	/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual void WriteByte(byte value)
	{
		Write(new byte[1] { value }, 0, 1);
	}

	/// <summary>Creates a thread-safe (synchronized) wrapper around the specified <see cref="T:System.IO.Stream" /> object.</summary>
	/// <returns>A thread-safe <see cref="T:System.IO.Stream" /> object.</returns>
	/// <param name="stream">The <see cref="T:System.IO.Stream" /> object to synchronize.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is null.</exception>
	public static Stream Synchronized(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (stream is SyncStream)
		{
			return stream;
		}
		return new SyncStream(stream);
	}

	/// <summary>Provides support for a <see cref="T:System.Diagnostics.Contracts.Contract" />.</summary>
	[Obsolete("Do not call or override this method.")]
	protected virtual void ObjectInvariant()
	{
	}

	internal IAsyncResult BlockingBeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		SynchronousAsyncResult synchronousAsyncResult;
		try
		{
			synchronousAsyncResult = new SynchronousAsyncResult(Read(buffer, offset, count), state);
		}
		catch (IOException ex)
		{
			synchronousAsyncResult = new SynchronousAsyncResult(ex, state, isWrite: false);
		}
		callback?.Invoke(synchronousAsyncResult);
		return synchronousAsyncResult;
	}

	internal static int BlockingEndRead(IAsyncResult asyncResult)
	{
		return SynchronousAsyncResult.EndRead(asyncResult);
	}

	internal IAsyncResult BlockingBeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		SynchronousAsyncResult synchronousAsyncResult;
		try
		{
			Write(buffer, offset, count);
			synchronousAsyncResult = new SynchronousAsyncResult(state);
		}
		catch (IOException ex)
		{
			synchronousAsyncResult = new SynchronousAsyncResult(ex, state, isWrite: true);
		}
		callback?.Invoke(synchronousAsyncResult);
		return synchronousAsyncResult;
	}

	internal static void BlockingEndWrite(IAsyncResult asyncResult)
	{
		SynchronousAsyncResult.EndWrite(asyncResult);
	}

	private bool HasOverriddenBeginEndRead()
	{
		return true;
	}

	private bool HasOverriddenBeginEndWrite()
	{
		return true;
	}

	public virtual ValueTask DisposeAsync()
	{
		try
		{
			Dispose();
			return default(ValueTask);
		}
		catch (Exception exception)
		{
			return new ValueTask(Task.FromException(exception));
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Stream" /> class. </summary>
	protected Stream()
	{
	}
}
