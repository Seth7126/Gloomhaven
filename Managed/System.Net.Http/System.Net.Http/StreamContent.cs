using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http;

/// <summary>Provides HTTP content based on a stream.</summary>
public class StreamContent : HttpContent
{
	private readonly Stream content;

	private readonly int bufferSize;

	private readonly CancellationToken cancellationToken;

	private readonly long startPosition;

	private bool contentCopied;

	/// <summary>Creates a new instance of the <see cref="T:System.Net.Http.StreamContent" /> class.</summary>
	/// <param name="content">The content used to initialize the <see cref="T:System.Net.Http.StreamContent" />.</param>
	public StreamContent(Stream content)
		: this(content, 16384)
	{
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Net.Http.StreamContent" /> class.</summary>
	/// <param name="content">The content used to initialize the <see cref="T:System.Net.Http.StreamContent" />.</param>
	/// <param name="bufferSize">The size, in bytes, of the buffer for the <see cref="T:System.Net.Http.StreamContent" />.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="content" /> was null.</exception>
	/// <exception cref="T:System.OutOfRangeException">The <paramref name="bufferSize" /> was less than or equal to zero. </exception>
	public StreamContent(Stream content, int bufferSize)
	{
		if (content == null)
		{
			throw new ArgumentNullException("content");
		}
		if (bufferSize <= 0)
		{
			throw new ArgumentOutOfRangeException("bufferSize");
		}
		this.content = content;
		this.bufferSize = bufferSize;
		if (content.CanSeek)
		{
			startPosition = content.Position;
		}
	}

	internal StreamContent(Stream content, CancellationToken cancellationToken)
		: this(content)
	{
		this.cancellationToken = cancellationToken;
	}

	/// <summary>Write the HTTP stream content to a memory stream as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation.</returns>
	protected override Task<Stream> CreateContentReadStreamAsync()
	{
		return Task.FromResult(content);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Net.Http.StreamContent" /> and optionally disposes of the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to releases only unmanaged resources.</param>
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			content.Dispose();
		}
		base.Dispose(disposing);
	}

	/// <summary>Serialize the HTTP content to a stream as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task" />.The task object representing the asynchronous operation.</returns>
	/// <param name="stream">The target stream.</param>
	/// <param name="context">Information about the transport (channel binding token, for example). This parameter may be null.</param>
	protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
	{
		if (contentCopied)
		{
			if (!content.CanSeek)
			{
				throw new InvalidOperationException("The stream was already consumed. It cannot be read again.");
			}
			content.Seek(startPosition, SeekOrigin.Begin);
		}
		else
		{
			contentCopied = true;
		}
		return content.CopyToAsync(stream, bufferSize, cancellationToken);
	}

	/// <summary>Determines whether the stream content has a valid length in bytes.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if <paramref name="length" /> is a valid length; otherwise, false.</returns>
	/// <param name="length">The length in bytes of the stream content.</param>
	protected internal override bool TryComputeLength(out long length)
	{
		if (!content.CanSeek)
		{
			length = 0L;
			return false;
		}
		length = content.Length - startPosition;
		return true;
	}
}
