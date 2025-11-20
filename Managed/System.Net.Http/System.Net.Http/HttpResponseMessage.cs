using System.Net.Http.Headers;
using System.Text;

namespace System.Net.Http;

/// <summary>Represents a HTTP response message including the status code and data.</summary>
public class HttpResponseMessage : IDisposable
{
	private HttpResponseHeaders headers;

	private HttpResponseHeaders trailingHeaders;

	private string reasonPhrase;

	private HttpStatusCode statusCode;

	private Version version;

	private bool disposed;

	/// <summary>Gets or sets the content of a HTTP response message. </summary>
	/// <returns>Returns <see cref="T:System.Net.Http.HttpContent" />.The content of the HTTP response message.</returns>
	public HttpContent Content { get; set; }

	/// <summary>Gets the collection of HTTP response headers. </summary>
	/// <returns>Returns <see cref="T:System.Net.Http.Headers.HttpResponseHeaders" />.The collection of HTTP response headers.</returns>
	public HttpResponseHeaders Headers => headers ?? (headers = new HttpResponseHeaders());

	/// <summary>Gets a value that indicates if the HTTP response was successful.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.A value that indicates if the HTTP response was successful. true if <see cref="P:System.Net.Http.HttpResponseMessage.StatusCode" /> was in the range 200-299; otherwise false.</returns>
	public bool IsSuccessStatusCode
	{
		get
		{
			if (statusCode >= HttpStatusCode.OK)
			{
				return statusCode < HttpStatusCode.MultipleChoices;
			}
			return false;
		}
	}

	/// <summary>Gets or sets the reason phrase which typically is sent by servers together with the status code. </summary>
	/// <returns>Returns <see cref="T:System.String" />.The reason phrase sent by the server.</returns>
	public string ReasonPhrase
	{
		get
		{
			return reasonPhrase ?? HttpStatusDescription.Get(statusCode);
		}
		set
		{
			reasonPhrase = value;
		}
	}

	/// <summary>Gets or sets the request message which led to this response message.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.HttpRequestMessage" />.The request message which led to this response message.</returns>
	public HttpRequestMessage RequestMessage { get; set; }

	/// <summary>Gets or sets the status code of the HTTP response.</summary>
	/// <returns>Returns <see cref="T:System.Net.HttpStatusCode" />.The status code of the HTTP response.</returns>
	public HttpStatusCode StatusCode
	{
		get
		{
			return statusCode;
		}
		set
		{
			if (value < (HttpStatusCode)0)
			{
				throw new ArgumentOutOfRangeException();
			}
			statusCode = value;
		}
	}

	/// <summary>Gets or sets the HTTP message version. </summary>
	/// <returns>Returns <see cref="T:System.Version" />.The HTTP message version. The default is 1.1. </returns>
	public Version Version
	{
		get
		{
			return version ?? HttpVersion.Version11;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Version");
			}
			version = value;
		}
	}

	public HttpResponseHeaders TrailingHeaders
	{
		get
		{
			if (trailingHeaders == null)
			{
				trailingHeaders = new HttpResponseHeaders();
			}
			return trailingHeaders;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.HttpResponseMessage" /> class.</summary>
	public HttpResponseMessage()
		: this(HttpStatusCode.OK)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.HttpResponseMessage" /> class with a specific <see cref="P:System.Net.Http.HttpResponseMessage.StatusCode" />.</summary>
	/// <param name="statusCode">The status code of the HTTP response.</param>
	public HttpResponseMessage(HttpStatusCode statusCode)
	{
		StatusCode = statusCode;
	}

	/// <summary>Releases the unmanaged resources and disposes of unmanaged resources used by the <see cref="T:System.Net.Http.HttpResponseMessage" />.</summary>
	public void Dispose()
	{
		Dispose(disposing: true);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Net.Http.HttpResponseMessage" /> and optionally disposes of the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to releases only unmanaged resources.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (disposing && !disposed)
		{
			disposed = true;
			if (Content != null)
			{
				Content.Dispose();
			}
		}
	}

	/// <summary>Throws an exception if the <see cref="P:System.Net.Http.HttpResponseMessage.IsSuccessStatusCode" /> property for the HTTP response is false.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.HttpResponseMessage" />.The HTTP response message if the call is successful.</returns>
	public HttpResponseMessage EnsureSuccessStatusCode()
	{
		if (IsSuccessStatusCode)
		{
			return this;
		}
		throw new HttpRequestException($"{(int)statusCode} ({ReasonPhrase})");
	}

	/// <summary>Returns a string that represents the current object.</summary>
	/// <returns>Returns <see cref="T:System.String" />.A string representation of the current object.</returns>
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("StatusCode: ").Append((int)StatusCode);
		stringBuilder.Append(", ReasonPhrase: '").Append(ReasonPhrase ?? "<null>");
		stringBuilder.Append("', Version: ").Append(Version);
		stringBuilder.Append(", Content: ").Append((Content != null) ? Content.ToString() : "<null>");
		stringBuilder.Append(", Headers:\r\n{\r\n").Append(Headers);
		if (Content != null)
		{
			stringBuilder.Append(Content.Headers);
		}
		stringBuilder.Append("}");
		return stringBuilder.ToString();
	}
}
