using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace System.Net.Http;

/// <summary>A base class representing an HTTP entity body and content headers.</summary>
public abstract class HttpContent : IDisposable
{
	private sealed class FixedMemoryStream : MemoryStream
	{
		private readonly long maxSize;

		public FixedMemoryStream(long maxSize)
		{
			this.maxSize = maxSize;
		}

		private void CheckOverflow(int count)
		{
			if (Length + count > maxSize)
			{
				throw new HttpRequestException($"Cannot write more bytes to the buffer than the configured maximum buffer size: {maxSize}");
			}
		}

		public override void WriteByte(byte value)
		{
			CheckOverflow(1);
			base.WriteByte(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			CheckOverflow(count);
			base.Write(buffer, offset, count);
		}
	}

	private FixedMemoryStream buffer;

	private Stream stream;

	private bool disposed;

	private HttpContentHeaders headers;

	/// <summary>Gets the HTTP content headers as defined in RFC 2616.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.Headers.HttpContentHeaders" />.The content headers as defined in RFC 2616.</returns>
	public HttpContentHeaders Headers => headers ?? (headers = new HttpContentHeaders(this));

	internal long? LoadedBufferLength
	{
		get
		{
			if (buffer != null)
			{
				return buffer.Length;
			}
			return null;
		}
	}

	internal void CopyTo(Stream stream)
	{
		CopyToAsync(stream).Wait();
	}

	/// <summary>Serialize the HTTP content into a stream of bytes and copies it to the stream object provided as the <paramref name="stream" /> parameter.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task" />.The task object representing the asynchronous operation.</returns>
	/// <param name="stream">The target stream.</param>
	public Task CopyToAsync(Stream stream)
	{
		return CopyToAsync(stream, null);
	}

	/// <summary>Serialize the HTTP content into a stream of bytes and copies it to the stream object provided as the <paramref name="stream" /> parameter.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task" />.The task object representing the asynchronous operation.</returns>
	/// <param name="stream">The target stream.</param>
	/// <param name="context">Information about the transport (channel binding token, for example). This parameter may be null.</param>
	public Task CopyToAsync(Stream stream, TransportContext context)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (buffer != null)
		{
			return buffer.CopyToAsync(stream);
		}
		return SerializeToStreamAsync(stream, context);
	}

	/// <summary>Serialize the HTTP content to a memory stream as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation.</returns>
	protected virtual async Task<Stream> CreateContentReadStreamAsync()
	{
		await LoadIntoBufferAsync().ConfigureAwait(continueOnCapturedContext: false);
		return buffer;
	}

	private static FixedMemoryStream CreateFixedMemoryStream(long maxBufferSize)
	{
		return new FixedMemoryStream(maxBufferSize);
	}

	/// <summary>Releases the unmanaged resources and disposes of the managed resources used by the <see cref="T:System.Net.Http.HttpContent" />.</summary>
	public void Dispose()
	{
		Dispose(disposing: true);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Net.Http.HttpContent" /> and optionally disposes of the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to releases only unmanaged resources.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (disposing && !disposed)
		{
			disposed = true;
			if (buffer != null)
			{
				buffer.Dispose();
			}
		}
	}

	/// <summary>Serialize the HTTP content to a memory buffer as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task" />.The task object representing the asynchronous operation.</returns>
	public Task LoadIntoBufferAsync()
	{
		return LoadIntoBufferAsync(2147483647L);
	}

	/// <summary>Serialize the HTTP content to a memory buffer as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task" />.The task object representing the asynchronous operation.</returns>
	/// <param name="maxBufferSize">The maximum size, in bytes, of the buffer to use.</param>
	public async Task LoadIntoBufferAsync(long maxBufferSize)
	{
		if (disposed)
		{
			throw new ObjectDisposedException(GetType().ToString());
		}
		if (buffer == null)
		{
			buffer = CreateFixedMemoryStream(maxBufferSize);
			await SerializeToStreamAsync(buffer, null).ConfigureAwait(continueOnCapturedContext: false);
			buffer.Seek(0L, SeekOrigin.Begin);
		}
	}

	/// <summary>Serialize the HTTP content and return a stream that represents the content as an asynchronous operation. </summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation.</returns>
	public async Task<Stream> ReadAsStreamAsync()
	{
		if (disposed)
		{
			throw new ObjectDisposedException(GetType().ToString());
		}
		if (buffer != null)
		{
			return new MemoryStream(buffer.GetBuffer(), 0, (int)buffer.Length, writable: false);
		}
		if (stream == null)
		{
			stream = await CreateContentReadStreamAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		return stream;
	}

	/// <summary>Serialize the HTTP content to a byte array as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation.</returns>
	public async Task<byte[]> ReadAsByteArrayAsync()
	{
		await LoadIntoBufferAsync().ConfigureAwait(continueOnCapturedContext: false);
		return buffer.ToArray();
	}

	/// <summary>Serialize the HTTP content to a string as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation.</returns>
	public async Task<string> ReadAsStringAsync()
	{
		await LoadIntoBufferAsync().ConfigureAwait(continueOnCapturedContext: false);
		if (buffer.Length == 0L)
		{
			return string.Empty;
		}
		byte[] array = buffer.GetBuffer();
		int num = (int)buffer.Length;
		int preambleLength = 0;
		Encoding encoding;
		if (headers != null && headers.ContentType != null && headers.ContentType.CharSet != null)
		{
			encoding = Encoding.GetEncoding(headers.ContentType.CharSet);
			preambleLength = StartsWith(array, num, encoding.GetPreamble());
		}
		else
		{
			encoding = GetEncodingFromBuffer(array, num, ref preambleLength) ?? Encoding.UTF8;
		}
		return encoding.GetString(array, preambleLength, num - preambleLength);
	}

	private static Encoding GetEncodingFromBuffer(byte[] buffer, int length, ref int preambleLength)
	{
		Encoding[] array = new Encoding[3]
		{
			Encoding.UTF8,
			Encoding.UTF32,
			Encoding.Unicode
		};
		foreach (Encoding encoding in array)
		{
			if ((preambleLength = StartsWith(buffer, length, encoding.GetPreamble())) != 0)
			{
				return encoding;
			}
		}
		return null;
	}

	private static int StartsWith(byte[] array, int length, byte[] value)
	{
		if (length < value.Length)
		{
			return 0;
		}
		for (int i = 0; i < value.Length; i++)
		{
			if (array[i] != value[i])
			{
				return 0;
			}
		}
		return value.Length;
	}

	internal Task SerializeToStreamAsync_internal(Stream stream, TransportContext context)
	{
		return SerializeToStreamAsync(stream, context);
	}

	/// <summary>Serialize the HTTP content to a stream as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task" />.The task object representing the asynchronous operation.</returns>
	/// <param name="stream">The target stream.</param>
	/// <param name="context">Information about the transport (channel binding token, for example). This parameter may be null.</param>
	protected abstract Task SerializeToStreamAsync(Stream stream, TransportContext context);

	/// <summary>Determines whether the HTTP content has a valid length in bytes.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if <paramref name="length" /> is a valid length; otherwise, false.</returns>
	/// <param name="length">The length in bytes of the HTTP content.</param>
	protected internal abstract bool TryComputeLength(out long length);

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.HttpContent" /> class.</summary>
	protected HttpContent()
	{
	}
}
