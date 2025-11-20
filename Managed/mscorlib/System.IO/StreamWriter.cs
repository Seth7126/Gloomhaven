using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO;

/// <summary>Implements a <see cref="T:System.IO.TextWriter" /> for writing characters to a stream in a particular encoding.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
public class StreamWriter : TextWriter
{
	internal const int DefaultBufferSize = 1024;

	private const int DefaultFileStreamBufferSize = 4096;

	private const int MinBufferSize = 128;

	private const int DontCopyOnWriteLineThreshold = 512;

	/// <summary>Provides a StreamWriter with no backing store that can be written to, but not read from.</summary>
	/// <filterpriority>1</filterpriority>
	public new static readonly StreamWriter Null = new StreamWriter(Stream.Null, UTF8NoBOM, 128, leaveOpen: true);

	private Stream _stream;

	private Encoding _encoding;

	private Encoder _encoder;

	private byte[] _byteBuffer;

	private char[] _charBuffer;

	private int _charPos;

	private int _charLen;

	private bool _autoFlush;

	private bool _haveWrittenPreamble;

	private bool _closable;

	private Task _asyncWriteTask = Task.CompletedTask;

	private static Encoding UTF8NoBOM => EncodingHelper.UTF8Unmarked;

	/// <summary>Gets or sets a value indicating whether the <see cref="T:System.IO.StreamWriter" /> will flush its buffer to the underlying stream after every call to <see cref="M:System.IO.StreamWriter.Write(System.Char)" />.</summary>
	/// <returns>true to force <see cref="T:System.IO.StreamWriter" /> to flush its buffer; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	public virtual bool AutoFlush
	{
		get
		{
			return _autoFlush;
		}
		set
		{
			CheckAsyncTaskInProgress();
			_autoFlush = value;
			if (value)
			{
				Flush(flushStream: true, flushEncoder: false);
			}
		}
	}

	/// <summary>Gets the underlying stream that interfaces with a backing store.</summary>
	/// <returns>The stream this StreamWriter is writing to.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual Stream BaseStream => _stream;

	internal bool LeaveOpen => !_closable;

	internal bool HaveWrittenPreamble
	{
		set
		{
			_haveWrittenPreamble = value;
		}
	}

	/// <summary>Gets the <see cref="T:System.Text.Encoding" /> in which the output is written.</summary>
	/// <returns>The <see cref="T:System.Text.Encoding" /> specified in the constructor for the current instance, or <see cref="T:System.Text.UTF8Encoding" /> if an encoding was not specified.</returns>
	/// <filterpriority>2</filterpriority>
	public override Encoding Encoding => _encoding;

	private int CharPos_Prop
	{
		set
		{
			_charPos = value;
		}
	}

	private bool HaveWrittenPreamble_Prop
	{
		set
		{
			_haveWrittenPreamble = value;
		}
	}

	private void CheckAsyncTaskInProgress()
	{
		if (!_asyncWriteTask.IsCompleted)
		{
			ThrowAsyncIOInProgress();
		}
	}

	private static void ThrowAsyncIOInProgress()
	{
		throw new InvalidOperationException("The stream is currently in use by a previous operation on the stream.");
	}

	internal StreamWriter()
		: base(null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamWriter" /> class for the specified stream by using UTF-8 encoding and the default buffer size.</summary>
	/// <param name="stream">The stream to write to. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="stream" /> is not writable. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is null. </exception>
	public StreamWriter(Stream stream)
		: this(stream, UTF8NoBOM, 1024, leaveOpen: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamWriter" /> class for the specified stream by using the specified encoding and the default buffer size.</summary>
	/// <param name="stream">The stream to write to. </param>
	/// <param name="encoding">The character encoding to use. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> or <paramref name="encoding" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="stream" /> is not writable. </exception>
	public StreamWriter(Stream stream, Encoding encoding)
		: this(stream, encoding, 1024, leaveOpen: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamWriter" /> class for the specified stream by using the specified encoding and buffer size.</summary>
	/// <param name="stream">The stream to write to. </param>
	/// <param name="encoding">The character encoding to use. </param>
	/// <param name="bufferSize">The buffer size, in bytes. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> or <paramref name="encoding" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="bufferSize" /> is negative. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="stream" /> is not writable. </exception>
	public StreamWriter(Stream stream, Encoding encoding, int bufferSize)
		: this(stream, encoding, bufferSize, leaveOpen: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamWriter" /> class for the specified stream by using the specified encoding and buffer size, and optionally leaves the stream open.</summary>
	/// <param name="stream">The stream to write to.</param>
	/// <param name="encoding">The character encoding to use.</param>
	/// <param name="bufferSize">The buffer size, in bytes.</param>
	/// <param name="leaveOpen">true to leave the stream open after the <see cref="T:System.IO.StreamWriter" /> object is disposed; otherwise, false.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> or <paramref name="encoding" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="bufferSize" /> is negative. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="stream" /> is not writable. </exception>
	public StreamWriter(Stream stream, Encoding encoding, int bufferSize, bool leaveOpen)
		: base(null)
	{
		if (stream == null || encoding == null)
		{
			throw new ArgumentNullException((stream == null) ? "stream" : "encoding");
		}
		if (!stream.CanWrite)
		{
			throw new ArgumentException("Stream was not writable.");
		}
		if (bufferSize <= 0)
		{
			throw new ArgumentOutOfRangeException("bufferSize", "Positive number required.");
		}
		Init(stream, encoding, bufferSize, leaveOpen);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamWriter" /> class for the specified file by using the default encoding and buffer size.</summary>
	/// <param name="path">The complete file path to write to. <paramref name="path" /> can be a file name. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">Access is denied. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is an empty string (""). -or-<paramref name="path" /> contains the name of a system device (com1, com2, and so on).</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters. </exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="path" /> includes an incorrect or invalid syntax for file name, directory name, or volume label syntax. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public StreamWriter(string path)
		: this(path, append: false, UTF8NoBOM, 1024)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamWriter" /> class for the specified file by using the default encoding and buffer size. If the file exists, it can be either overwritten or appended to. If the file does not exist, this constructor creates a new file.</summary>
	/// <param name="path">The complete file path to write to. </param>
	/// <param name="append">true to append data to the file; false to overwrite the file. If the specified file does not exist, this parameter has no effect, and the constructor creates a new file. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">Access is denied. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is empty. -or-<paramref name="path" /> contains the name of a system device (com1, com2, and so on).</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="path" /> includes an incorrect or invalid syntax for file name, directory name, or volume label syntax. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public StreamWriter(string path, bool append)
		: this(path, append, UTF8NoBOM, 1024)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamWriter" /> class for the specified file by using the specified encoding and default buffer size. If the file exists, it can be either overwritten or appended to. If the file does not exist, this constructor creates a new file.</summary>
	/// <param name="path">The complete file path to write to. </param>
	/// <param name="append">true to append data to the file; false to overwrite the file. If the specified file does not exist, this parameter has no effect, and the constructor creates a new file.</param>
	/// <param name="encoding">The character encoding to use. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">Access is denied. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is empty. -or-<paramref name="path" /> contains the name of a system device (com1, com2, and so on).</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="path" /> includes an incorrect or invalid syntax for file name, directory name, or volume label syntax. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public StreamWriter(string path, bool append, Encoding encoding)
		: this(path, append, encoding, 1024)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamWriter" /> class for the specified file on the specified path, using the specified encoding and buffer size. If the file exists, it can be either overwritten or appended to. If the file does not exist, this constructor creates a new file.</summary>
	/// <param name="path">The complete file path to write to. </param>
	/// <param name="append">true to append data to the file; false to overwrite the file. If the specified file does not exist, this parameter has no effect, and the constructor creates a new file.</param>
	/// <param name="encoding">The character encoding to use. </param>
	/// <param name="bufferSize">The buffer size, in bytes. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is an empty string (""). -or-<paramref name="path" /> contains the name of a system device (com1, com2, and so on).</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> or <paramref name="encoding" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="bufferSize" /> is negative. </exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="path" /> includes an incorrect or invalid syntax for file name, directory name, or volume label syntax. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">Access is denied. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters. </exception>
	public StreamWriter(string path, bool append, Encoding encoding, int bufferSize)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (encoding == null)
		{
			throw new ArgumentNullException("encoding");
		}
		if (path.Length == 0)
		{
			throw new ArgumentException("Empty path name is not legal.");
		}
		if (bufferSize <= 0)
		{
			throw new ArgumentOutOfRangeException("bufferSize", "Positive number required.");
		}
		Init(new FileStream(path, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.SequentialScan), encoding, bufferSize, shouldLeaveOpen: false);
	}

	private void Init(Stream streamArg, Encoding encodingArg, int bufferSize, bool shouldLeaveOpen)
	{
		_stream = streamArg;
		_encoding = encodingArg;
		_encoder = _encoding.GetEncoder();
		if (bufferSize < 128)
		{
			bufferSize = 128;
		}
		_charBuffer = new char[bufferSize];
		_byteBuffer = new byte[_encoding.GetMaxByteCount(bufferSize)];
		_charLen = bufferSize;
		if (_stream.CanSeek && _stream.Position > 0)
		{
			_haveWrittenPreamble = true;
		}
		_closable = !shouldLeaveOpen;
	}

	/// <summary>Closes the current StreamWriter object and the underlying stream.</summary>
	/// <exception cref="T:System.Text.EncoderFallbackException">The current encoding does not support displaying half of a Unicode surrogate pair.</exception>
	/// <filterpriority>1</filterpriority>
	public override void Close()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.IO.StreamWriter" /> and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	/// <exception cref="T:System.Text.EncoderFallbackException">The current encoding does not support displaying half of a Unicode surrogate pair.</exception>
	protected override void Dispose(bool disposing)
	{
		try
		{
			if (_stream != null && disposing)
			{
				CheckAsyncTaskInProgress();
				Flush(flushStream: true, flushEncoder: true);
			}
		}
		finally
		{
			if (!LeaveOpen && _stream != null)
			{
				try
				{
					if (disposing)
					{
						_stream.Close();
					}
				}
				finally
				{
					_stream = null;
					_byteBuffer = null;
					_charBuffer = null;
					_encoding = null;
					_encoder = null;
					_charLen = 0;
					base.Dispose(disposing);
				}
			}
		}
	}

	public override ValueTask DisposeAsync()
	{
		if (!(GetType() != typeof(StreamWriter)))
		{
			return DisposeAsyncCore();
		}
		return base.DisposeAsync();
	}

	private async ValueTask DisposeAsyncCore()
	{
		try
		{
			if (_stream != null)
			{
				await FlushAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		finally
		{
			CloseStreamFromDispose(disposing: true);
		}
		GC.SuppressFinalize(this);
	}

	private void CloseStreamFromDispose(bool disposing)
	{
		if (LeaveOpen || _stream == null)
		{
			return;
		}
		try
		{
			if (disposing)
			{
				_stream.Close();
			}
		}
		finally
		{
			_stream = null;
			_byteBuffer = null;
			_charBuffer = null;
			_encoding = null;
			_encoder = null;
			_charLen = 0;
			base.Dispose(disposing);
		}
	}

	/// <summary>Clears all buffers for the current writer and causes any buffered data to be written to the underlying stream.</summary>
	/// <exception cref="T:System.ObjectDisposedException">The current writer is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error has occurred. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">The current encoding does not support displaying half of a Unicode surrogate pair. </exception>
	/// <filterpriority>1</filterpriority>
	public override void Flush()
	{
		CheckAsyncTaskInProgress();
		Flush(flushStream: true, flushEncoder: true);
	}

	private void Flush(bool flushStream, bool flushEncoder)
	{
		if (_stream == null)
		{
			throw new ObjectDisposedException(null, "Can not write to a closed TextWriter.");
		}
		if (_charPos == 0 && !flushStream && !flushEncoder)
		{
			return;
		}
		if (!_haveWrittenPreamble)
		{
			_haveWrittenPreamble = true;
			ReadOnlySpan<byte> preamble = _encoding.Preamble;
			if (preamble.Length > 0)
			{
				_stream.Write(preamble);
			}
		}
		int bytes = _encoder.GetBytes(_charBuffer, 0, _charPos, _byteBuffer, 0, flushEncoder);
		_charPos = 0;
		if (bytes > 0)
		{
			_stream.Write(_byteBuffer, 0, bytes);
		}
		if (flushStream)
		{
			_stream.Flush();
		}
	}

	/// <summary>Writes a character to the stream.</summary>
	/// <param name="value">The character to write to the stream. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ObjectDisposedException">
	///   <see cref="P:System.IO.StreamWriter.AutoFlush" /> is true or the <see cref="T:System.IO.StreamWriter" /> buffer is full, and current writer is closed. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <see cref="P:System.IO.StreamWriter.AutoFlush" /> is true or the <see cref="T:System.IO.StreamWriter" /> buffer is full, and the contents of the buffer cannot be written to the underlying fixed size stream because the <see cref="T:System.IO.StreamWriter" /> is at the end the stream. </exception>
	/// <filterpriority>1</filterpriority>
	public override void Write(char value)
	{
		CheckAsyncTaskInProgress();
		if (_charPos == _charLen)
		{
			Flush(flushStream: false, flushEncoder: false);
		}
		_charBuffer[_charPos] = value;
		_charPos++;
		if (_autoFlush)
		{
			Flush(flushStream: true, flushEncoder: false);
		}
	}

	/// <summary>Writes a character array to the stream.</summary>
	/// <param name="buffer">A character array containing the data to write. If <paramref name="buffer" /> is null, nothing is written. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ObjectDisposedException">
	///   <see cref="P:System.IO.StreamWriter.AutoFlush" /> is true or the <see cref="T:System.IO.StreamWriter" /> buffer is full, and current writer is closed. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <see cref="P:System.IO.StreamWriter.AutoFlush" /> is true or the <see cref="T:System.IO.StreamWriter" /> buffer is full, and the contents of the buffer cannot be written to the underlying fixed size stream because the <see cref="T:System.IO.StreamWriter" /> is at the end the stream. </exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public override void Write(char[] buffer)
	{
		WriteSpan(buffer, appendNewLine: false);
	}

	/// <summary>Writes a subarray of characters to the stream.</summary>
	/// <param name="buffer">A character array that contains the data to write. </param>
	/// <param name="index">The character position in the buffer at which to start reading data. </param>
	/// <param name="count">The maximum number of characters to write. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">The buffer length minus <paramref name="index" /> is less than <paramref name="count" />. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is negative. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ObjectDisposedException">
	///   <see cref="P:System.IO.StreamWriter.AutoFlush" /> is true or the <see cref="T:System.IO.StreamWriter" /> buffer is full, and current writer is closed. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <see cref="P:System.IO.StreamWriter.AutoFlush" /> is true or the <see cref="T:System.IO.StreamWriter" /> buffer is full, and the contents of the buffer cannot be written to the underlying fixed size stream because the <see cref="T:System.IO.StreamWriter" /> is at the end the stream. </exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public override void Write(char[] buffer, int index, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer", "Buffer cannot be null.");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", "Non-negative number required.");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Non-negative number required.");
		}
		if (buffer.Length - index < count)
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		WriteSpan(buffer.AsSpan(index, count), appendNewLine: false);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public override void Write(ReadOnlySpan<char> buffer)
	{
		if (GetType() == typeof(StreamWriter))
		{
			WriteSpan(buffer, appendNewLine: false);
		}
		else
		{
			base.Write(buffer);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe void WriteSpan(ReadOnlySpan<char> buffer, bool appendNewLine)
	{
		CheckAsyncTaskInProgress();
		if (buffer.Length <= 4 && buffer.Length <= _charLen - _charPos)
		{
			for (int i = 0; i < buffer.Length; i++)
			{
				_charBuffer[_charPos++] = buffer[i];
			}
		}
		else
		{
			char[] charBuffer = _charBuffer;
			if (charBuffer == null)
			{
				throw new ObjectDisposedException(null, "Can not write to a closed TextWriter.");
			}
			fixed (char* reference = &MemoryMarshal.GetReference(buffer))
			{
				fixed (char* ptr = &charBuffer[0])
				{
					char* ptr2 = reference;
					int num = buffer.Length;
					int num2 = _charPos;
					while (num > 0)
					{
						if (num2 == charBuffer.Length)
						{
							Flush(flushStream: false, flushEncoder: false);
							num2 = 0;
						}
						int num3 = Math.Min(charBuffer.Length - num2, num);
						int num4 = num3 * 2;
						Buffer.MemoryCopy(ptr2, ptr + num2, num4, num4);
						_charPos += num3;
						num2 += num3;
						ptr2 += num3;
						num -= num3;
					}
				}
			}
		}
		if (appendNewLine)
		{
			char[] coreNewLine = CoreNewLine;
			for (int j = 0; j < coreNewLine.Length; j++)
			{
				if (_charPos == _charLen)
				{
					Flush(flushStream: false, flushEncoder: false);
				}
				_charBuffer[_charPos] = coreNewLine[j];
				_charPos++;
			}
		}
		if (_autoFlush)
		{
			Flush(flushStream: true, flushEncoder: false);
		}
	}

	/// <summary>Writes a string to the stream.</summary>
	/// <param name="value">The string to write to the stream. If <paramref name="value" /> is null, nothing is written. </param>
	/// <exception cref="T:System.ObjectDisposedException">
	///   <see cref="P:System.IO.StreamWriter.AutoFlush" /> is true or the <see cref="T:System.IO.StreamWriter" /> buffer is full, and current writer is closed. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <see cref="P:System.IO.StreamWriter.AutoFlush" /> is true or the <see cref="T:System.IO.StreamWriter" /> buffer is full, and the contents of the buffer cannot be written to the underlying fixed size stream because the <see cref="T:System.IO.StreamWriter" /> is at the end the stream. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public override void Write(string value)
	{
		WriteSpan(value, appendNewLine: false);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public override void WriteLine(string value)
	{
		CheckAsyncTaskInProgress();
		WriteSpan(value, appendNewLine: true);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public override void WriteLine(ReadOnlySpan<char> value)
	{
		if (GetType() == typeof(StreamWriter))
		{
			CheckAsyncTaskInProgress();
			WriteSpan(value, appendNewLine: true);
		}
		else
		{
			base.WriteLine(value);
		}
	}

	/// <summary>Writes a character to the stream asynchronously.</summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="value">The character to write to the stream.</param>
	/// <exception cref="T:System.ObjectDisposedException">The stream writer is disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The stream writer is currently in use by a previous write operation.</exception>
	public override Task WriteAsync(char value)
	{
		if (GetType() != typeof(StreamWriter))
		{
			return base.WriteAsync(value);
		}
		if (_stream == null)
		{
			throw new ObjectDisposedException(null, "Can not write to a closed TextWriter.");
		}
		CheckAsyncTaskInProgress();
		return _asyncWriteTask = WriteAsyncInternal(this, value, _charBuffer, _charPos, _charLen, CoreNewLine, _autoFlush, appendNewLine: false);
	}

	private static async Task WriteAsyncInternal(StreamWriter _this, char value, char[] charBuffer, int charPos, int charLen, char[] coreNewLine, bool autoFlush, bool appendNewLine)
	{
		if (charPos == charLen)
		{
			await _this.FlushAsyncInternal(flushStream: false, flushEncoder: false, charBuffer, charPos).ConfigureAwait(continueOnCapturedContext: false);
			charPos = 0;
		}
		charBuffer[charPos] = value;
		charPos++;
		if (appendNewLine)
		{
			for (int i = 0; i < coreNewLine.Length; i++)
			{
				if (charPos == charLen)
				{
					await _this.FlushAsyncInternal(flushStream: false, flushEncoder: false, charBuffer, charPos).ConfigureAwait(continueOnCapturedContext: false);
					charPos = 0;
				}
				charBuffer[charPos] = coreNewLine[i];
				charPos++;
			}
		}
		if (autoFlush)
		{
			await _this.FlushAsyncInternal(flushStream: true, flushEncoder: false, charBuffer, charPos).ConfigureAwait(continueOnCapturedContext: false);
			charPos = 0;
		}
		_this.CharPos_Prop = charPos;
	}

	/// <summary>Writes a string to the stream asynchronously.</summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="value">The string to write to the stream. If <paramref name="value" /> is null, nothing is written.</param>
	/// <exception cref="T:System.ObjectDisposedException">The stream writer is disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The stream writer is currently in use by a previous write operation.</exception>
	public override Task WriteAsync(string value)
	{
		if (GetType() != typeof(StreamWriter))
		{
			return base.WriteAsync(value);
		}
		if (value != null)
		{
			if (_stream == null)
			{
				throw new ObjectDisposedException(null, "Can not write to a closed TextWriter.");
			}
			CheckAsyncTaskInProgress();
			return _asyncWriteTask = WriteAsyncInternal(this, value, _charBuffer, _charPos, _charLen, CoreNewLine, _autoFlush, appendNewLine: false);
		}
		return Task.CompletedTask;
	}

	private static async Task WriteAsyncInternal(StreamWriter _this, string value, char[] charBuffer, int charPos, int charLen, char[] coreNewLine, bool autoFlush, bool appendNewLine)
	{
		int count = value.Length;
		int index = 0;
		while (count > 0)
		{
			if (charPos == charLen)
			{
				await _this.FlushAsyncInternal(flushStream: false, flushEncoder: false, charBuffer, charPos).ConfigureAwait(continueOnCapturedContext: false);
				charPos = 0;
			}
			int num = charLen - charPos;
			if (num > count)
			{
				num = count;
			}
			value.CopyTo(index, charBuffer, charPos, num);
			charPos += num;
			index += num;
			count -= num;
		}
		if (appendNewLine)
		{
			for (int i = 0; i < coreNewLine.Length; i++)
			{
				if (charPos == charLen)
				{
					await _this.FlushAsyncInternal(flushStream: false, flushEncoder: false, charBuffer, charPos).ConfigureAwait(continueOnCapturedContext: false);
					charPos = 0;
				}
				charBuffer[charPos] = coreNewLine[i];
				charPos++;
			}
		}
		if (autoFlush)
		{
			await _this.FlushAsyncInternal(flushStream: true, flushEncoder: false, charBuffer, charPos).ConfigureAwait(continueOnCapturedContext: false);
			charPos = 0;
		}
		_this.CharPos_Prop = charPos;
	}

	/// <summary>Writes a subarray of characters to the stream asynchronously.</summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="buffer">A character array that contains the data to write.</param>
	/// <param name="index">The character position in the buffer at which to begin reading data.</param>
	/// <param name="count">The maximum number of characters to write.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="index" /> plus <paramref name="count" /> is greater than the buffer length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is negative.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream writer is disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The stream writer is currently in use by a previous write operation. </exception>
	public override Task WriteAsync(char[] buffer, int index, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer", "Buffer cannot be null.");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", "Non-negative number required.");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Non-negative number required.");
		}
		if (buffer.Length - index < count)
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		if (GetType() != typeof(StreamWriter))
		{
			return base.WriteAsync(buffer, index, count);
		}
		if (_stream == null)
		{
			throw new ObjectDisposedException(null, "Can not write to a closed TextWriter.");
		}
		CheckAsyncTaskInProgress();
		return _asyncWriteTask = WriteAsyncInternal(this, new ReadOnlyMemory<char>(buffer, index, count), _charBuffer, _charPos, _charLen, CoreNewLine, _autoFlush, appendNewLine: false, default(CancellationToken));
	}

	public override Task WriteAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (GetType() != typeof(StreamWriter))
		{
			return base.WriteAsync(buffer, cancellationToken);
		}
		if (_stream == null)
		{
			throw new ObjectDisposedException(null, "Can not write to a closed TextWriter.");
		}
		CheckAsyncTaskInProgress();
		if (cancellationToken.IsCancellationRequested)
		{
			return Task.FromCanceled(cancellationToken);
		}
		return _asyncWriteTask = WriteAsyncInternal(this, buffer, _charBuffer, _charPos, _charLen, CoreNewLine, _autoFlush, appendNewLine: false, cancellationToken);
	}

	private static async Task WriteAsyncInternal(StreamWriter _this, ReadOnlyMemory<char> source, char[] charBuffer, int charPos, int charLen, char[] coreNewLine, bool autoFlush, bool appendNewLine, CancellationToken cancellationToken)
	{
		int num;
		for (int copied = 0; copied < source.Length; copied += num)
		{
			if (charPos == charLen)
			{
				await _this.FlushAsyncInternal(flushStream: false, flushEncoder: false, charBuffer, charPos, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				charPos = 0;
			}
			num = Math.Min(charLen - charPos, source.Length - copied);
			ReadOnlySpan<char> readOnlySpan = source.Span;
			readOnlySpan = readOnlySpan.Slice(copied, num);
			readOnlySpan.CopyTo(new Span<char>(charBuffer, charPos, num));
			charPos += num;
		}
		if (appendNewLine)
		{
			for (int i = 0; i < coreNewLine.Length; i++)
			{
				if (charPos == charLen)
				{
					await _this.FlushAsyncInternal(flushStream: false, flushEncoder: false, charBuffer, charPos, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					charPos = 0;
				}
				charBuffer[charPos] = coreNewLine[i];
				charPos++;
			}
		}
		if (autoFlush)
		{
			await _this.FlushAsyncInternal(flushStream: true, flushEncoder: false, charBuffer, charPos, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			charPos = 0;
		}
		_this.CharPos_Prop = charPos;
	}

	/// <summary>Writes a line terminator asynchronously to the stream.</summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The stream writer is disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The stream writer is currently in use by a previous write operation.</exception>
	public override Task WriteLineAsync()
	{
		if (GetType() != typeof(StreamWriter))
		{
			return base.WriteLineAsync();
		}
		if (_stream == null)
		{
			throw new ObjectDisposedException(null, "Can not write to a closed TextWriter.");
		}
		CheckAsyncTaskInProgress();
		return _asyncWriteTask = WriteAsyncInternal(this, ReadOnlyMemory<char>.Empty, _charBuffer, _charPos, _charLen, CoreNewLine, _autoFlush, appendNewLine: true, default(CancellationToken));
	}

	/// <summary>Writes a character followed by a line terminator asynchronously to the stream.</summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="value">The character to write to the stream.</param>
	/// <exception cref="T:System.ObjectDisposedException">The stream writer is disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The stream writer is currently in use by a previous write operation.</exception>
	public override Task WriteLineAsync(char value)
	{
		if (GetType() != typeof(StreamWriter))
		{
			return base.WriteLineAsync(value);
		}
		if (_stream == null)
		{
			throw new ObjectDisposedException(null, "Can not write to a closed TextWriter.");
		}
		CheckAsyncTaskInProgress();
		return _asyncWriteTask = WriteAsyncInternal(this, value, _charBuffer, _charPos, _charLen, CoreNewLine, _autoFlush, appendNewLine: true);
	}

	/// <summary>Writes a string followed by a line terminator asynchronously to the stream.</summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="value">The string to write. If the value is null, only a line terminator is written. </param>
	/// <exception cref="T:System.ObjectDisposedException">The stream writer is disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The stream writer is currently in use by a previous write operation.</exception>
	public override Task WriteLineAsync(string value)
	{
		if (value == null)
		{
			return WriteLineAsync();
		}
		if (GetType() != typeof(StreamWriter))
		{
			return base.WriteLineAsync(value);
		}
		if (_stream == null)
		{
			throw new ObjectDisposedException(null, "Can not write to a closed TextWriter.");
		}
		CheckAsyncTaskInProgress();
		return _asyncWriteTask = WriteAsyncInternal(this, value, _charBuffer, _charPos, _charLen, CoreNewLine, _autoFlush, appendNewLine: true);
	}

	/// <summary>Writes a subarray of characters followed by a line terminator asynchronously to the stream.</summary>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	/// <param name="buffer">The character array to write data from.</param>
	/// <param name="index">The character position in the buffer at which to start reading data.</param>
	/// <param name="count">The maximum number of characters to write.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="index" /> plus <paramref name="count" /> is greater than the buffer length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is negative.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream writer is disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The stream writer is currently in use by a previous write operation. </exception>
	public override Task WriteLineAsync(char[] buffer, int index, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer", "Buffer cannot be null.");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", "Non-negative number required.");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Non-negative number required.");
		}
		if (buffer.Length - index < count)
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		if (GetType() != typeof(StreamWriter))
		{
			return base.WriteLineAsync(buffer, index, count);
		}
		if (_stream == null)
		{
			throw new ObjectDisposedException(null, "Can not write to a closed TextWriter.");
		}
		CheckAsyncTaskInProgress();
		return _asyncWriteTask = WriteAsyncInternal(this, new ReadOnlyMemory<char>(buffer, index, count), _charBuffer, _charPos, _charLen, CoreNewLine, _autoFlush, appendNewLine: true, default(CancellationToken));
	}

	public override Task WriteLineAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (GetType() != typeof(StreamWriter))
		{
			return base.WriteLineAsync(buffer, cancellationToken);
		}
		if (_stream == null)
		{
			throw new ObjectDisposedException(null, "Can not write to a closed TextWriter.");
		}
		CheckAsyncTaskInProgress();
		if (cancellationToken.IsCancellationRequested)
		{
			return Task.FromCanceled(cancellationToken);
		}
		return _asyncWriteTask = WriteAsyncInternal(this, buffer, _charBuffer, _charPos, _charLen, CoreNewLine, _autoFlush, appendNewLine: true, cancellationToken);
	}

	/// <summary>Clears all buffers for this stream asynchronously and causes any buffered data to be written to the underlying device.</summary>
	/// <returns>A task that represents the asynchronous flush operation.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
	public override Task FlushAsync()
	{
		if (GetType() != typeof(StreamWriter))
		{
			return base.FlushAsync();
		}
		if (_stream == null)
		{
			throw new ObjectDisposedException(null, "Can not write to a closed TextWriter.");
		}
		CheckAsyncTaskInProgress();
		return _asyncWriteTask = FlushAsyncInternal(flushStream: true, flushEncoder: true, _charBuffer, _charPos);
	}

	private Task FlushAsyncInternal(bool flushStream, bool flushEncoder, char[] sCharBuffer, int sCharPos, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return Task.FromCanceled(cancellationToken);
		}
		if (sCharPos == 0 && !flushStream && !flushEncoder)
		{
			return Task.CompletedTask;
		}
		Task result = FlushAsyncInternal(this, flushStream, flushEncoder, sCharBuffer, sCharPos, _haveWrittenPreamble, _encoding, _encoder, _byteBuffer, _stream, cancellationToken);
		_charPos = 0;
		return result;
	}

	private static async Task FlushAsyncInternal(StreamWriter _this, bool flushStream, bool flushEncoder, char[] charBuffer, int charPos, bool haveWrittenPreamble, Encoding encoding, Encoder encoder, byte[] byteBuffer, Stream stream, CancellationToken cancellationToken)
	{
		if (!haveWrittenPreamble)
		{
			_this.HaveWrittenPreamble_Prop = true;
			byte[] preamble = encoding.GetPreamble();
			if (preamble.Length != 0)
			{
				await stream.WriteAsync(new ReadOnlyMemory<byte>(preamble), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		int bytes = encoder.GetBytes(charBuffer, 0, charPos, byteBuffer, 0, flushEncoder);
		if (bytes > 0)
		{
			await stream.WriteAsync(new ReadOnlyMemory<byte>(byteBuffer, 0, bytes), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (flushStream)
		{
			await stream.FlushAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
	}
}
