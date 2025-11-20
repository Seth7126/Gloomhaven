using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO;

/// <summary>Implements a <see cref="T:System.IO.TextReader" /> that reads characters from a byte stream in a particular encoding.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
public class StreamReader : TextReader
{
	private class NullStreamReader : StreamReader
	{
		public override Stream BaseStream => Stream.Null;

		public override Encoding CurrentEncoding => Encoding.Unicode;

		internal NullStreamReader()
		{
			Init(Stream.Null);
		}

		protected override void Dispose(bool disposing)
		{
		}

		public override int Peek()
		{
			return -1;
		}

		public override int Read()
		{
			return -1;
		}

		public override int Read(char[] buffer, int index, int count)
		{
			return 0;
		}

		public override string ReadLine()
		{
			return null;
		}

		public override string ReadToEnd()
		{
			return string.Empty;
		}

		internal override int ReadBuffer()
		{
			return 0;
		}
	}

	/// <summary>A <see cref="T:System.IO.StreamReader" /> object around an empty stream.</summary>
	/// <filterpriority>1</filterpriority>
	public new static readonly StreamReader Null = new NullStreamReader();

	private const int DefaultBufferSize = 1024;

	private const int DefaultFileStreamBufferSize = 4096;

	private const int MinBufferSize = 128;

	private Stream _stream;

	private Encoding _encoding;

	private Decoder _decoder;

	private byte[] _byteBuffer;

	private char[] _charBuffer;

	private int _charPos;

	private int _charLen;

	private int _byteLen;

	private int _bytePos;

	private int _maxCharsPerBuffer;

	private bool _detectEncoding;

	private bool _checkPreamble;

	private bool _isBlocked;

	private bool _closable;

	private Task _asyncReadTask = Task.CompletedTask;

	/// <summary>Gets the current character encoding that the current <see cref="T:System.IO.StreamReader" /> object is using.</summary>
	/// <returns>The current character encoding used by the current reader. The value can be different after the first call to any <see cref="Overload:System.IO.StreamReader.Read" /> method of <see cref="T:System.IO.StreamReader" />, since encoding autodetection is not done until the first call to a <see cref="Overload:System.IO.StreamReader.Read" /> method.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual Encoding CurrentEncoding => _encoding;

	/// <summary>Returns the underlying stream.</summary>
	/// <returns>The underlying stream.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual Stream BaseStream => _stream;

	internal bool LeaveOpen => !_closable;

	/// <summary>Gets a value that indicates whether the current stream position is at the end of the stream.</summary>
	/// <returns>true if the current stream position is at the end of the stream; otherwise false.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The underlying stream has been disposed.</exception>
	/// <filterpriority>1</filterpriority>
	public bool EndOfStream
	{
		get
		{
			if (_stream == null)
			{
				throw new ObjectDisposedException(null, "Cannot read from a closed TextReader.");
			}
			CheckAsyncTaskInProgress();
			if (_charPos < _charLen)
			{
				return false;
			}
			return ReadBuffer() == 0;
		}
	}

	private void CheckAsyncTaskInProgress()
	{
		if (!_asyncReadTask.IsCompleted)
		{
			ThrowAsyncIOInProgress();
		}
	}

	private static void ThrowAsyncIOInProgress()
	{
		throw new InvalidOperationException("The stream is currently in use by a previous operation on the stream.");
	}

	internal StreamReader()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamReader" /> class for the specified stream.</summary>
	/// <param name="stream">The stream to be read. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="stream" /> does not support reading. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is null. </exception>
	public StreamReader(Stream stream)
		: this(stream, detectEncodingFromByteOrderMarks: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamReader" /> class for the specified stream, with the specified byte order mark detection option.</summary>
	/// <param name="stream">The stream to be read. </param>
	/// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="stream" /> does not support reading. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is null. </exception>
	public StreamReader(Stream stream, bool detectEncodingFromByteOrderMarks)
		: this(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks, 1024, leaveOpen: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamReader" /> class for the specified stream, with the specified character encoding.</summary>
	/// <param name="stream">The stream to be read. </param>
	/// <param name="encoding">The character encoding to use. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="stream" /> does not support reading. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> or <paramref name="encoding" /> is null. </exception>
	public StreamReader(Stream stream, Encoding encoding)
		: this(stream, encoding, detectEncodingFromByteOrderMarks: true, 1024, leaveOpen: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamReader" /> class for the specified stream, with the specified character encoding and byte order mark detection option.</summary>
	/// <param name="stream">The stream to be read. </param>
	/// <param name="encoding">The character encoding to use. </param>
	/// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="stream" /> does not support reading. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> or <paramref name="encoding" /> is null. </exception>
	public StreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks)
		: this(stream, encoding, detectEncodingFromByteOrderMarks, 1024, leaveOpen: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamReader" /> class for the specified stream, with the specified character encoding, byte order mark detection option, and buffer size.</summary>
	/// <param name="stream">The stream to be read. </param>
	/// <param name="encoding">The character encoding to use. </param>
	/// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file. </param>
	/// <param name="bufferSize">The minimum buffer size. </param>
	/// <exception cref="T:System.ArgumentException">The stream does not support reading. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> or <paramref name="encoding" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="bufferSize" /> is less than or equal to zero. </exception>
	public StreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
		: this(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize, leaveOpen: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamReader" /> class for the specified stream based on the specified character encoding, byte order mark detection option, and buffer size, and optionally leaves the stream open.</summary>
	/// <param name="stream">The stream to read.</param>
	/// <param name="encoding">The character encoding to use.</param>
	/// <param name="detectEncodingFromByteOrderMarks">true to look for byte order marks at the beginning of the file; otherwise, false.</param>
	/// <param name="bufferSize">The minimum buffer size.</param>
	/// <param name="leaveOpen">true to leave the stream open after the <see cref="T:System.IO.StreamReader" /> object is disposed; otherwise, false.</param>
	public StreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize, bool leaveOpen)
	{
		if (stream == null || encoding == null)
		{
			throw new ArgumentNullException((stream == null) ? "stream" : "encoding");
		}
		if (!stream.CanRead)
		{
			throw new ArgumentException("Stream was not readable.");
		}
		if (bufferSize <= 0)
		{
			throw new ArgumentOutOfRangeException("bufferSize", "Positive number required.");
		}
		Init(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize, leaveOpen);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamReader" /> class for the specified file name.</summary>
	/// <param name="path">The complete file path to be read. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is an empty string (""). </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file cannot be found. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive. </exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="path" /> includes an incorrect or invalid syntax for file name, directory name, or volume label. </exception>
	public StreamReader(string path)
		: this(path, detectEncodingFromByteOrderMarks: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamReader" /> class for the specified file name, with the specified byte order mark detection option.</summary>
	/// <param name="path">The complete file path to be read. </param>
	/// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is an empty string (""). </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file cannot be found. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive. </exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="path" /> includes an incorrect or invalid syntax for file name, directory name, or volume label. </exception>
	public StreamReader(string path, bool detectEncodingFromByteOrderMarks)
		: this(path, Encoding.UTF8, detectEncodingFromByteOrderMarks, 1024)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamReader" /> class for the specified file name, with the specified character encoding.</summary>
	/// <param name="path">The complete file path to be read. </param>
	/// <param name="encoding">The character encoding to use. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is an empty string (""). </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> or <paramref name="encoding" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file cannot be found. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> includes an incorrect or invalid syntax for file name, directory name, or volume label. </exception>
	public StreamReader(string path, Encoding encoding)
		: this(path, encoding, detectEncodingFromByteOrderMarks: true, 1024)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamReader" /> class for the specified file name, with the specified character encoding and byte order mark detection option.</summary>
	/// <param name="path">The complete file path to be read. </param>
	/// <param name="encoding">The character encoding to use. </param>
	/// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is an empty string (""). </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> or <paramref name="encoding" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file cannot be found. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> includes an incorrect or invalid syntax for file name, directory name, or volume label. </exception>
	public StreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks)
		: this(path, encoding, detectEncodingFromByteOrderMarks, 1024)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamReader" /> class for the specified file name, with the specified character encoding, byte order mark detection option, and buffer size.</summary>
	/// <param name="path">The complete file path to be read. </param>
	/// <param name="encoding">The character encoding to use. </param>
	/// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file. </param>
	/// <param name="bufferSize">The minimum buffer size, in number of 16-bit characters. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is an empty string (""). </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> or <paramref name="encoding" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file cannot be found. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> includes an incorrect or invalid syntax for file name, directory name, or volume label. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="buffersize" /> is less than or equal to zero. </exception>
	public StreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
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
		Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
		Init(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize, leaveOpen: false);
	}

	private void Init(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize, bool leaveOpen)
	{
		_stream = stream;
		_encoding = encoding;
		_decoder = encoding.GetDecoder();
		if (bufferSize < 128)
		{
			bufferSize = 128;
		}
		_byteBuffer = new byte[bufferSize];
		_maxCharsPerBuffer = encoding.GetMaxCharCount(bufferSize);
		_charBuffer = new char[_maxCharsPerBuffer];
		_byteLen = 0;
		_bytePos = 0;
		_detectEncoding = detectEncodingFromByteOrderMarks;
		_checkPreamble = encoding.Preamble.Length > 0;
		_isBlocked = false;
		_closable = !leaveOpen;
	}

	internal void Init(Stream stream)
	{
		_stream = stream;
		_closable = true;
	}

	/// <summary>Closes the <see cref="T:System.IO.StreamReader" /> object and the underlying stream, and releases any system resources associated with the reader.</summary>
	/// <filterpriority>1</filterpriority>
	public override void Close()
	{
		Dispose(disposing: true);
	}

	/// <summary>Closes the underlying stream, releases the unmanaged resources used by the <see cref="T:System.IO.StreamReader" />, and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected override void Dispose(bool disposing)
	{
		try
		{
			if (!LeaveOpen && disposing && _stream != null)
			{
				_stream.Close();
			}
		}
		finally
		{
			if (!LeaveOpen && _stream != null)
			{
				_stream = null;
				_encoding = null;
				_decoder = null;
				_byteBuffer = null;
				_charBuffer = null;
				_charPos = 0;
				_charLen = 0;
				base.Dispose(disposing);
			}
		}
	}

	/// <summary>Clears the internal buffer.</summary>
	/// <filterpriority>2</filterpriority>
	public void DiscardBufferedData()
	{
		CheckAsyncTaskInProgress();
		_byteLen = 0;
		_charLen = 0;
		_charPos = 0;
		if (_encoding != null)
		{
			_decoder = _encoding.GetDecoder();
		}
		_isBlocked = false;
	}

	/// <summary>Returns the next available character but does not consume it.</summary>
	/// <returns>An integer representing the next character to be read, or -1 if there are no characters to be read or if the stream does not support seeking.</returns>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public override int Peek()
	{
		if (_stream == null)
		{
			throw new ObjectDisposedException(null, "Cannot read from a closed TextReader.");
		}
		CheckAsyncTaskInProgress();
		if (_charPos == _charLen && (_isBlocked || ReadBuffer() == 0))
		{
			return -1;
		}
		return _charBuffer[_charPos];
	}

	/// <summary>Reads the next character from the input stream and advances the character position by one character.</summary>
	/// <returns>The next character from the input stream represented as an <see cref="T:System.Int32" /> object, or -1 if no more characters are available.</returns>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public override int Read()
	{
		if (_stream == null)
		{
			throw new ObjectDisposedException(null, "Cannot read from a closed TextReader.");
		}
		CheckAsyncTaskInProgress();
		if (_charPos == _charLen && ReadBuffer() == 0)
		{
			return -1;
		}
		char result = _charBuffer[_charPos];
		_charPos++;
		return result;
	}

	/// <summary>Reads a specified maximum of characters from the current stream into a buffer, beginning at the specified index.</summary>
	/// <returns>The number of characters that have been read, or 0 if at the end of the stream and no data was read. The number will be less than or equal to the <paramref name="count" /> parameter, depending on whether the data is available within the stream.</returns>
	/// <param name="buffer">When this method returns, contains the specified character array with the values between <paramref name="index" /> and (<paramref name="index + count - 1" />) replaced by the characters read from the current source. </param>
	/// <param name="index">The index of <paramref name="buffer" /> at which to begin writing. </param>
	/// <param name="count">The maximum number of characters to read. </param>
	/// <exception cref="T:System.ArgumentException">The buffer length minus <paramref name="index" /> is less than <paramref name="count" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is negative. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs, such as the stream is closed. </exception>
	/// <filterpriority>1</filterpriority>
	public override int Read(char[] buffer, int index, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer", "Buffer cannot be null.");
		}
		if (index < 0 || count < 0)
		{
			throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", "Non-negative number required.");
		}
		if (buffer.Length - index < count)
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		return ReadSpan(new Span<char>(buffer, index, count));
	}

	public override int Read(Span<char> buffer)
	{
		if (!(GetType() == typeof(StreamReader)))
		{
			return base.Read(buffer);
		}
		return ReadSpan(buffer);
	}

	private int ReadSpan(Span<char> buffer)
	{
		if (_stream == null)
		{
			throw new ObjectDisposedException(null, "Cannot read from a closed TextReader.");
		}
		CheckAsyncTaskInProgress();
		int num = 0;
		bool readToUserBuffer = false;
		int num2 = buffer.Length;
		while (num2 > 0)
		{
			int num3 = _charLen - _charPos;
			if (num3 == 0)
			{
				num3 = ReadBuffer(buffer.Slice(num), out readToUserBuffer);
			}
			if (num3 == 0)
			{
				break;
			}
			if (num3 > num2)
			{
				num3 = num2;
			}
			if (!readToUserBuffer)
			{
				new Span<char>(_charBuffer, _charPos, num3).CopyTo(buffer.Slice(num));
				_charPos += num3;
			}
			num += num3;
			num2 -= num3;
			if (_isBlocked)
			{
				break;
			}
		}
		return num;
	}

	/// <summary>Reads all characters from the current position to the end of the stream.</summary>
	/// <returns>The rest of the stream as a string, from the current position to the end. If the current position is at the end of the stream, returns an empty string ("").</returns>
	/// <exception cref="T:System.OutOfMemoryException">There is insufficient memory to allocate a buffer for the returned string. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public override string ReadToEnd()
	{
		if (_stream == null)
		{
			throw new ObjectDisposedException(null, "Cannot read from a closed TextReader.");
		}
		CheckAsyncTaskInProgress();
		StringBuilder stringBuilder = new StringBuilder(_charLen - _charPos);
		do
		{
			stringBuilder.Append(_charBuffer, _charPos, _charLen - _charPos);
			_charPos = _charLen;
			ReadBuffer();
		}
		while (_charLen > 0);
		return stringBuilder.ToString();
	}

	/// <summary>Reads a specified maximum number of characters from the current stream and writes the data to a buffer, beginning at the specified index.</summary>
	/// <returns>The number of characters that have been read. The number will be less than or equal to <paramref name="count" />, depending on whether all input characters have been read.</returns>
	/// <param name="buffer">When this method returns, contains the specified character array with the values between <paramref name="index" /> and (<paramref name="index + count - 1" />) replaced by the characters read from the current source.</param>
	/// <param name="index">The position in <paramref name="buffer" /> at which to begin writing.</param>
	/// <param name="count">The maximum number of characters to read.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">The buffer length minus <paramref name="index" /> is less than <paramref name="count" />. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is negative. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.StreamReader" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	public override int ReadBlock(char[] buffer, int index, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer", "Buffer cannot be null.");
		}
		if (index < 0 || count < 0)
		{
			throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", "Non-negative number required.");
		}
		if (buffer.Length - index < count)
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		if (_stream == null)
		{
			throw new ObjectDisposedException(null, "Cannot read from a closed TextReader.");
		}
		CheckAsyncTaskInProgress();
		return base.ReadBlock(buffer, index, count);
	}

	public override int ReadBlock(Span<char> buffer)
	{
		if (GetType() != typeof(StreamReader))
		{
			return base.ReadBlock(buffer);
		}
		int num = 0;
		int num2;
		do
		{
			num2 = ReadSpan(buffer.Slice(num));
			num += num2;
		}
		while (num2 > 0 && num < buffer.Length);
		return num;
	}

	private void CompressBuffer(int n)
	{
		Buffer.BlockCopy(_byteBuffer, n, _byteBuffer, 0, _byteLen - n);
		_byteLen -= n;
	}

	private void DetectEncoding()
	{
		if (_byteLen < 2)
		{
			return;
		}
		_detectEncoding = false;
		bool flag = false;
		if (_byteBuffer[0] == 254 && _byteBuffer[1] == byte.MaxValue)
		{
			_encoding = Encoding.BigEndianUnicode;
			CompressBuffer(2);
			flag = true;
		}
		else if (_byteBuffer[0] == byte.MaxValue && _byteBuffer[1] == 254)
		{
			if (_byteLen < 4 || _byteBuffer[2] != 0 || _byteBuffer[3] != 0)
			{
				_encoding = Encoding.Unicode;
				CompressBuffer(2);
				flag = true;
			}
			else
			{
				_encoding = Encoding.UTF32;
				CompressBuffer(4);
				flag = true;
			}
		}
		else if (_byteLen >= 3 && _byteBuffer[0] == 239 && _byteBuffer[1] == 187 && _byteBuffer[2] == 191)
		{
			_encoding = Encoding.UTF8;
			CompressBuffer(3);
			flag = true;
		}
		else if (_byteLen >= 4 && _byteBuffer[0] == 0 && _byteBuffer[1] == 0 && _byteBuffer[2] == 254 && _byteBuffer[3] == byte.MaxValue)
		{
			_encoding = new UTF32Encoding(bigEndian: true, byteOrderMark: true);
			CompressBuffer(4);
			flag = true;
		}
		else if (_byteLen == 2)
		{
			_detectEncoding = true;
		}
		if (flag)
		{
			_decoder = _encoding.GetDecoder();
			int maxCharCount = _encoding.GetMaxCharCount(_byteBuffer.Length);
			if (maxCharCount > _maxCharsPerBuffer)
			{
				_charBuffer = new char[maxCharCount];
			}
			_maxCharsPerBuffer = maxCharCount;
		}
	}

	private bool IsPreamble()
	{
		if (!_checkPreamble)
		{
			return _checkPreamble;
		}
		ReadOnlySpan<byte> preamble = _encoding.Preamble;
		int num = ((_byteLen >= preamble.Length) ? (preamble.Length - _bytePos) : (_byteLen - _bytePos));
		int num2 = 0;
		while (num2 < num)
		{
			if (_byteBuffer[_bytePos] != preamble[_bytePos])
			{
				_bytePos = 0;
				_checkPreamble = false;
				break;
			}
			num2++;
			_bytePos++;
		}
		if (_checkPreamble && _bytePos == preamble.Length)
		{
			CompressBuffer(preamble.Length);
			_bytePos = 0;
			_checkPreamble = false;
			_detectEncoding = false;
		}
		return _checkPreamble;
	}

	internal virtual int ReadBuffer()
	{
		_charLen = 0;
		_charPos = 0;
		if (!_checkPreamble)
		{
			_byteLen = 0;
		}
		do
		{
			if (_checkPreamble)
			{
				int num = _stream.Read(_byteBuffer, _bytePos, _byteBuffer.Length - _bytePos);
				if (num == 0)
				{
					if (_byteLen > 0)
					{
						_charLen += _decoder.GetChars(_byteBuffer, 0, _byteLen, _charBuffer, _charLen);
						_bytePos = (_byteLen = 0);
					}
					return _charLen;
				}
				_byteLen += num;
			}
			else
			{
				_byteLen = _stream.Read(_byteBuffer, 0, _byteBuffer.Length);
				if (_byteLen == 0)
				{
					return _charLen;
				}
			}
			_isBlocked = _byteLen < _byteBuffer.Length;
			if (!IsPreamble())
			{
				if (_detectEncoding && _byteLen >= 2)
				{
					DetectEncoding();
				}
				_charLen += _decoder.GetChars(_byteBuffer, 0, _byteLen, _charBuffer, _charLen);
			}
		}
		while (_charLen == 0);
		return _charLen;
	}

	private int ReadBuffer(Span<char> userBuffer, out bool readToUserBuffer)
	{
		_charLen = 0;
		_charPos = 0;
		if (!_checkPreamble)
		{
			_byteLen = 0;
		}
		int num = 0;
		readToUserBuffer = userBuffer.Length >= _maxCharsPerBuffer;
		do
		{
			if (_checkPreamble)
			{
				int num2 = _stream.Read(_byteBuffer, _bytePos, _byteBuffer.Length - _bytePos);
				if (num2 == 0)
				{
					if (_byteLen > 0)
					{
						if (readToUserBuffer)
						{
							num = _decoder.GetChars(new ReadOnlySpan<byte>(_byteBuffer, 0, _byteLen), userBuffer.Slice(num), flush: false);
							_charLen = 0;
						}
						else
						{
							num = _decoder.GetChars(_byteBuffer, 0, _byteLen, _charBuffer, num);
							_charLen += num;
						}
					}
					return num;
				}
				_byteLen += num2;
			}
			else
			{
				_byteLen = _stream.Read(_byteBuffer, 0, _byteBuffer.Length);
				if (_byteLen == 0)
				{
					break;
				}
			}
			_isBlocked = _byteLen < _byteBuffer.Length;
			if (!IsPreamble())
			{
				if (_detectEncoding && _byteLen >= 2)
				{
					DetectEncoding();
					readToUserBuffer = userBuffer.Length >= _maxCharsPerBuffer;
				}
				_charPos = 0;
				if (readToUserBuffer)
				{
					num += _decoder.GetChars(new ReadOnlySpan<byte>(_byteBuffer, 0, _byteLen), userBuffer.Slice(num), flush: false);
					_charLen = 0;
				}
				else
				{
					num = _decoder.GetChars(_byteBuffer, 0, _byteLen, _charBuffer, num);
					_charLen += num;
				}
			}
		}
		while (num == 0);
		_isBlocked &= num < userBuffer.Length;
		return num;
	}

	/// <summary>Reads a line of characters from the current stream and returns the data as a string.</summary>
	/// <returns>The next line from the input stream, or null if the end of the input stream is reached.</returns>
	/// <exception cref="T:System.OutOfMemoryException">There is insufficient memory to allocate a buffer for the returned string. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public override string ReadLine()
	{
		if (_stream == null)
		{
			throw new ObjectDisposedException(null, "Cannot read from a closed TextReader.");
		}
		CheckAsyncTaskInProgress();
		if (_charPos == _charLen && ReadBuffer() == 0)
		{
			return null;
		}
		StringBuilder stringBuilder = null;
		do
		{
			int num = _charPos;
			do
			{
				char c = _charBuffer[num];
				if (c == '\r' || c == '\n')
				{
					string result;
					if (stringBuilder != null)
					{
						stringBuilder.Append(_charBuffer, _charPos, num - _charPos);
						result = stringBuilder.ToString();
					}
					else
					{
						result = new string(_charBuffer, _charPos, num - _charPos);
					}
					_charPos = num + 1;
					if (c == '\r' && (_charPos < _charLen || ReadBuffer() > 0) && _charBuffer[_charPos] == '\n')
					{
						_charPos++;
					}
					return result;
				}
				num++;
			}
			while (num < _charLen);
			num = _charLen - _charPos;
			if (stringBuilder == null)
			{
				stringBuilder = new StringBuilder(num + 80);
			}
			stringBuilder.Append(_charBuffer, _charPos, num);
		}
		while (ReadBuffer() > 0);
		return stringBuilder.ToString();
	}

	/// <summary>Reads a line of characters asynchronously from the current stream and returns the data as a string.</summary>
	/// <returns>A task that represents the asynchronous read operation. The value of the <paramref name="TResult" /> parameter contains the next line from the stream, or is null if all the characters have been read.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The number of characters in the next line is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The reader is currently in use by a previous read operation. </exception>
	public override Task<string> ReadLineAsync()
	{
		if (GetType() != typeof(StreamReader))
		{
			return base.ReadLineAsync();
		}
		if (_stream == null)
		{
			throw new ObjectDisposedException(null, "Cannot read from a closed TextReader.");
		}
		CheckAsyncTaskInProgress();
		return (Task<string>)(_asyncReadTask = ReadLineAsyncInternal());
	}

	private async Task<string> ReadLineAsyncInternal()
	{
		bool flag = _charPos == _charLen;
		if (flag)
		{
			flag = await ReadBufferAsync().ConfigureAwait(continueOnCapturedContext: false) == 0;
		}
		if (flag)
		{
			return null;
		}
		StringBuilder sb = null;
		do
		{
			char[] charBuffer = _charBuffer;
			int charLen = _charLen;
			int charPos = _charPos;
			int num = charPos;
			do
			{
				char c = charBuffer[num];
				if (c == '\r' || c == '\n')
				{
					string s;
					if (sb != null)
					{
						sb.Append(charBuffer, charPos, num - charPos);
						s = sb.ToString();
					}
					else
					{
						s = new string(charBuffer, charPos, num - charPos);
					}
					charPos = (_charPos = num + 1);
					flag = c == '\r';
					if (flag)
					{
						bool flag2 = charPos < charLen;
						if (!flag2)
						{
							flag2 = await ReadBufferAsync().ConfigureAwait(continueOnCapturedContext: false) > 0;
						}
						flag = flag2;
					}
					if (flag)
					{
						charPos = _charPos;
						if (_charBuffer[charPos] == '\n')
						{
							_charPos = charPos + 1;
						}
					}
					return s;
				}
				num++;
			}
			while (num < charLen);
			num = charLen - charPos;
			if (sb == null)
			{
				sb = new StringBuilder(num + 80);
			}
			sb.Append(charBuffer, charPos, num);
		}
		while (await ReadBufferAsync().ConfigureAwait(continueOnCapturedContext: false) > 0);
		return sb.ToString();
	}

	/// <summary>Reads all characters from the current position to the end of the stream asynchronously and returns them as one string.</summary>
	/// <returns>A task that represents the asynchronous read operation. The value of the <paramref name="TResult" /> parameter contains a string with the characters from the current position to the end of the stream.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The number of characters is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The reader is currently in use by a previous read operation. </exception>
	public override Task<string> ReadToEndAsync()
	{
		if (GetType() != typeof(StreamReader))
		{
			return base.ReadToEndAsync();
		}
		if (_stream == null)
		{
			throw new ObjectDisposedException(null, "Cannot read from a closed TextReader.");
		}
		CheckAsyncTaskInProgress();
		return (Task<string>)(_asyncReadTask = ReadToEndAsyncInternal());
	}

	private async Task<string> ReadToEndAsyncInternal()
	{
		StringBuilder sb = new StringBuilder(_charLen - _charPos);
		do
		{
			int charPos = _charPos;
			sb.Append(_charBuffer, charPos, _charLen - charPos);
			_charPos = _charLen;
			await ReadBufferAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		while (_charLen > 0);
		return sb.ToString();
	}

	/// <summary>Reads a specified maximum number of characters from the current stream asynchronously and writes the data to a buffer, beginning at the specified index. </summary>
	/// <returns>A task that represents the asynchronous read operation. The value of the <paramref name="TResult" /> parameter contains the total number of bytes read into the buffer. The result value can be less than the number of bytes requested if the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the stream has been reached.</returns>
	/// <param name="buffer">When this method returns, contains the specified character array with the values between <paramref name="index" /> and (<paramref name="index" /> + <paramref name="count" /> - 1) replaced by the characters read from the current source.</param>
	/// <param name="index">The position in <paramref name="buffer" /> at which to begin writing.</param>
	/// <param name="count">The maximum number of characters to read. If the end of the stream is reached before the specified number of characters is written into the buffer, the current method returns.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is negative.</exception>
	/// <exception cref="T:System.ArgumentException">The sum of <paramref name="index" /> and <paramref name="count" /> is larger than the buffer length.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The reader is currently in use by a previous read operation. </exception>
	public override Task<int> ReadAsync(char[] buffer, int index, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer", "Buffer cannot be null.");
		}
		if (index < 0 || count < 0)
		{
			throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", "Non-negative number required.");
		}
		if (buffer.Length - index < count)
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		if (GetType() != typeof(StreamReader))
		{
			return base.ReadAsync(buffer, index, count);
		}
		if (_stream == null)
		{
			throw new ObjectDisposedException(null, "Cannot read from a closed TextReader.");
		}
		CheckAsyncTaskInProgress();
		return (Task<int>)(_asyncReadTask = ReadAsyncInternal(new Memory<char>(buffer, index, count), default(CancellationToken)).AsTask());
	}

	public override ValueTask<int> ReadAsync(Memory<char> buffer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (GetType() != typeof(StreamReader))
		{
			return base.ReadAsync(buffer, cancellationToken);
		}
		if (_stream == null)
		{
			throw new ObjectDisposedException(null, "Cannot read from a closed TextReader.");
		}
		CheckAsyncTaskInProgress();
		if (cancellationToken.IsCancellationRequested)
		{
			return new ValueTask<int>(Task.FromCanceled<int>(cancellationToken));
		}
		return ReadAsyncInternal(buffer, cancellationToken);
	}

	internal override async ValueTask<int> ReadAsyncInternal(Memory<char> buffer, CancellationToken cancellationToken)
	{
		bool flag = _charPos == _charLen;
		if (flag)
		{
			flag = await ReadBufferAsync().ConfigureAwait(continueOnCapturedContext: false) == 0;
		}
		if (flag)
		{
			return 0;
		}
		int charsRead = 0;
		bool readToUserBuffer = false;
		byte[] tmpByteBuffer = _byteBuffer;
		Stream tmpStream = _stream;
		int count = buffer.Length;
		while (count > 0)
		{
			int n = _charLen - _charPos;
			if (n == 0)
			{
				_charLen = 0;
				_charPos = 0;
				if (!_checkPreamble)
				{
					_byteLen = 0;
				}
				readToUserBuffer = count >= _maxCharsPerBuffer;
				do
				{
					if (_checkPreamble)
					{
						int bytePos = _bytePos;
						int num = await tmpStream.ReadAsync(new Memory<byte>(tmpByteBuffer, bytePos, tmpByteBuffer.Length - bytePos), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
						if (num == 0)
						{
							if (_byteLen > 0)
							{
								if (readToUserBuffer)
								{
									n = _decoder.GetChars(new ReadOnlySpan<byte>(tmpByteBuffer, 0, _byteLen), buffer.Span.Slice(charsRead), flush: false);
									_charLen = 0;
								}
								else
								{
									n = _decoder.GetChars(tmpByteBuffer, 0, _byteLen, _charBuffer, 0);
									_charLen += n;
								}
							}
							_isBlocked = true;
							break;
						}
						_byteLen += num;
					}
					else
					{
						_byteLen = await tmpStream.ReadAsync(new Memory<byte>(tmpByteBuffer), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
						if (_byteLen == 0)
						{
							_isBlocked = true;
							break;
						}
					}
					_isBlocked = _byteLen < tmpByteBuffer.Length;
					if (!IsPreamble())
					{
						if (_detectEncoding && _byteLen >= 2)
						{
							DetectEncoding();
							readToUserBuffer = count >= _maxCharsPerBuffer;
						}
						_charPos = 0;
						if (readToUserBuffer)
						{
							n += _decoder.GetChars(new ReadOnlySpan<byte>(tmpByteBuffer, 0, _byteLen), buffer.Span.Slice(charsRead), flush: false);
							_charLen = 0;
						}
						else
						{
							n = _decoder.GetChars(tmpByteBuffer, 0, _byteLen, _charBuffer, 0);
							_charLen += n;
						}
					}
				}
				while (n == 0);
				if (n == 0)
				{
					break;
				}
			}
			if (n > count)
			{
				n = count;
			}
			if (!readToUserBuffer)
			{
				new Span<char>(_charBuffer, _charPos, n).CopyTo(buffer.Span.Slice(charsRead));
				_charPos += n;
			}
			charsRead += n;
			count -= n;
			if (_isBlocked)
			{
				break;
			}
		}
		return charsRead;
	}

	/// <summary>Reads a specified maximum number of characters from the current stream asynchronously and writes the data to a buffer, beginning at the specified index.</summary>
	/// <returns>A task that represents the asynchronous read operation. The value of the <paramref name="TResult" /> parameter contains the total number of bytes read into the buffer. The result value can be less than the number of bytes requested if the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the stream has been reached.</returns>
	/// <param name="buffer">When this method returns, contains the specified character array with the values between <paramref name="index" /> and (<paramref name="index" /> + <paramref name="count" /> - 1) replaced by the characters read from the current source.</param>
	/// <param name="index">The position in <paramref name="buffer" /> at which to begin writing.</param>
	/// <param name="count">The maximum number of characters to read. If the end of the stream is reached before the specified number of characters is written into the buffer, the method returns.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is negative.</exception>
	/// <exception cref="T:System.ArgumentException">The sum of <paramref name="index" /> and <paramref name="count" /> is larger than the buffer length.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The reader is currently in use by a previous read operation. </exception>
	public override Task<int> ReadBlockAsync(char[] buffer, int index, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer", "Buffer cannot be null.");
		}
		if (index < 0 || count < 0)
		{
			throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", "Non-negative number required.");
		}
		if (buffer.Length - index < count)
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		if (GetType() != typeof(StreamReader))
		{
			return base.ReadBlockAsync(buffer, index, count);
		}
		if (_stream == null)
		{
			throw new ObjectDisposedException(null, "Cannot read from a closed TextReader.");
		}
		CheckAsyncTaskInProgress();
		return (Task<int>)(_asyncReadTask = base.ReadBlockAsync(buffer, index, count));
	}

	public override ValueTask<int> ReadBlockAsync(Memory<char> buffer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (GetType() != typeof(StreamReader))
		{
			return base.ReadBlockAsync(buffer, cancellationToken);
		}
		if (_stream == null)
		{
			throw new ObjectDisposedException(null, "Cannot read from a closed TextReader.");
		}
		CheckAsyncTaskInProgress();
		if (cancellationToken.IsCancellationRequested)
		{
			return new ValueTask<int>(Task.FromCanceled<int>(cancellationToken));
		}
		ValueTask<int> result = ReadBlockAsyncInternal(buffer, cancellationToken);
		if (result.IsCompletedSuccessfully)
		{
			return result;
		}
		return new ValueTask<int>((Task<int>)(_asyncReadTask = result.AsTask()));
	}

	private async Task<int> ReadBufferAsync()
	{
		_charLen = 0;
		_charPos = 0;
		byte[] tmpByteBuffer = _byteBuffer;
		Stream tmpStream = _stream;
		if (!_checkPreamble)
		{
			_byteLen = 0;
		}
		do
		{
			if (_checkPreamble)
			{
				int bytePos = _bytePos;
				int num = await tmpStream.ReadAsync(new Memory<byte>(tmpByteBuffer, bytePos, tmpByteBuffer.Length - bytePos)).ConfigureAwait(continueOnCapturedContext: false);
				if (num == 0)
				{
					if (_byteLen > 0)
					{
						_charLen += _decoder.GetChars(tmpByteBuffer, 0, _byteLen, _charBuffer, _charLen);
						_bytePos = 0;
						_byteLen = 0;
					}
					return _charLen;
				}
				_byteLen += num;
			}
			else
			{
				_byteLen = await tmpStream.ReadAsync(new Memory<byte>(tmpByteBuffer)).ConfigureAwait(continueOnCapturedContext: false);
				if (_byteLen == 0)
				{
					return _charLen;
				}
			}
			_isBlocked = _byteLen < tmpByteBuffer.Length;
			if (!IsPreamble())
			{
				if (_detectEncoding && _byteLen >= 2)
				{
					DetectEncoding();
				}
				_charLen += _decoder.GetChars(tmpByteBuffer, 0, _byteLen, _charBuffer, _charLen);
			}
		}
		while (_charLen == 0);
		return _charLen;
	}

	internal bool DataAvailable()
	{
		return _charPos < _charLen;
	}
}
