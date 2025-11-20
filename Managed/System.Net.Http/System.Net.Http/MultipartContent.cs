using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace System.Net.Http;

/// <summary>Provides a collection of <see cref="T:System.Net.Http.HttpContent" /> objects that get serialized using the multipart/* content type specification.</summary>
public class MultipartContent : HttpContent, IEnumerable<HttpContent>, IEnumerable
{
	private List<HttpContent> nested_content;

	private readonly string boundary;

	/// <summary>Creates a new instance of the <see cref="T:System.Net.Http.MultipartContent" /> class.</summary>
	public MultipartContent()
		: this("mixed")
	{
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Net.Http.MultipartContent" /> class.</summary>
	/// <param name="subtype">The subtype of the multipart content.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="subtype" /> was null or contains only white space characters.</exception>
	public MultipartContent(string subtype)
		: this(subtype, Guid.NewGuid().ToString("D", CultureInfo.InvariantCulture))
	{
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Net.Http.MultipartContent" /> class.</summary>
	/// <param name="subtype">The subtype of the multipart content.</param>
	/// <param name="boundary">The boundary string for the multipart content.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="subtype" /> was null or an empty string.The <paramref name="boundary" /> was null or contains only white space characters.-or-The <paramref name="boundary" /> ends with a space character.</exception>
	/// <exception cref="T:System.OutOfRangeException">The length of the <paramref name="boundary" /> was greater than 70.</exception>
	public MultipartContent(string subtype, string boundary)
	{
		if (string.IsNullOrWhiteSpace(subtype))
		{
			throw new ArgumentException("boundary");
		}
		if (string.IsNullOrWhiteSpace(boundary))
		{
			throw new ArgumentException("boundary");
		}
		if (boundary.Length > 70)
		{
			throw new ArgumentOutOfRangeException("boundary");
		}
		if (boundary.Last() == ' ' || !IsValidRFC2049(boundary))
		{
			throw new ArgumentException("boundary");
		}
		this.boundary = boundary;
		nested_content = new List<HttpContent>(2);
		base.Headers.ContentType = new MediaTypeHeaderValue("multipart/" + subtype)
		{
			Parameters = 
			{
				new NameValueHeaderValue("boundary", "\"" + boundary + "\"")
			}
		};
	}

	private static bool IsValidRFC2049(string s)
	{
		foreach (char c in s)
		{
			if ((c < 'a' || c > 'z') && (c < 'A' || c > 'Z') && (c < '0' || c > '9'))
			{
				switch (c)
				{
				case '\'':
				case '(':
				case ')':
				case '+':
				case ',':
				case '-':
				case '.':
				case '/':
				case ':':
				case '=':
				case '?':
					continue;
				}
				return false;
			}
		}
		return true;
	}

	/// <summary>Add multipart HTTP content to a collection of <see cref="T:System.Net.Http.HttpContent" /> objects that get serialized using the multipart/* content type specification.</summary>
	/// <param name="content">The HTTP content to add to the collection.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="content" /> was null.</exception>
	public virtual void Add(HttpContent content)
	{
		if (content == null)
		{
			throw new ArgumentNullException("content");
		}
		if (nested_content == null)
		{
			nested_content = new List<HttpContent>();
		}
		nested_content.Add(content);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Net.Http.MultipartContent" /> and optionally disposes of the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to releases only unmanaged resources.</param>
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			foreach (HttpContent item in nested_content)
			{
				item.Dispose();
			}
			nested_content = null;
		}
		base.Dispose(disposing);
	}

	/// <summary>Serialize the multipart HTTP content to a stream as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task" />.The task object representing the asynchronous operation.</returns>
	/// <param name="stream">The target stream.</param>
	/// <param name="context">Information about the transport (channel binding token, for example). This parameter may be null.</param>
	protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
	{
		StringBuilder sb = new StringBuilder();
		sb.Append('-').Append('-');
		sb.Append(boundary);
		sb.Append('\r').Append('\n');
		byte[] bytes;
		for (int i = 0; i < nested_content.Count; i++)
		{
			HttpContent c = nested_content[i];
			foreach (KeyValuePair<string, IEnumerable<string>> header in c.Headers)
			{
				sb.Append(header.Key);
				sb.Append(':').Append(' ');
				foreach (string item in header.Value)
				{
					sb.Append(item);
				}
				sb.Append('\r').Append('\n');
			}
			sb.Append('\r').Append('\n');
			bytes = Encoding.ASCII.GetBytes(sb.ToString());
			sb.Clear();
			await stream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(continueOnCapturedContext: false);
			await c.SerializeToStreamAsync_internal(stream, context).ConfigureAwait(continueOnCapturedContext: false);
			if (i != nested_content.Count - 1)
			{
				sb.Append('\r').Append('\n');
				sb.Append('-').Append('-');
				sb.Append(boundary);
				sb.Append('\r').Append('\n');
			}
		}
		sb.Append('\r').Append('\n');
		sb.Append('-').Append('-');
		sb.Append(boundary);
		sb.Append('-').Append('-');
		sb.Append('\r').Append('\n');
		bytes = Encoding.ASCII.GetBytes(sb.ToString());
		await stream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(continueOnCapturedContext: false);
	}

	/// <summary>Determines whether the HTTP multipart content has a valid length in bytes.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if <paramref name="length" /> is a valid length; otherwise, false.</returns>
	/// <param name="length">The length in bytes of the HHTP content.</param>
	protected internal override bool TryComputeLength(out long length)
	{
		length = 12 + 2 * boundary.Length;
		for (int i = 0; i < nested_content.Count; i++)
		{
			HttpContent httpContent = nested_content[i];
			foreach (KeyValuePair<string, IEnumerable<string>> header in httpContent.Headers)
			{
				length += header.Key.Length;
				length += 4L;
				foreach (string item in header.Value)
				{
					length += item.Length;
				}
			}
			if (!httpContent.TryComputeLength(out var length2))
			{
				return false;
			}
			length += 2L;
			length += length2;
			if (i != nested_content.Count - 1)
			{
				length += 6L;
				length += boundary.Length;
			}
		}
		return true;
	}

	/// <summary>Returns an enumerator that iterates through the collection of <see cref="T:System.Net.Http.HttpContent" /> objects that get serialized using the multipart/* content type specification..</summary>
	/// <returns>Returns <see cref="T:System.Collections.Generic.IEnumerator`1" />.An object that can be used to iterate through the collection.</returns>
	public IEnumerator<HttpContent> GetEnumerator()
	{
		return nested_content.GetEnumerator();
	}

	/// <summary>The explicit implementation of the <see cref="M:System.Net.Http.MultipartContent.GetEnumerator" /> method.</summary>
	/// <returns>Returns <see cref="T:System.Collections.IEnumerator" />.An object that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return nested_content.GetEnumerator();
	}
}
