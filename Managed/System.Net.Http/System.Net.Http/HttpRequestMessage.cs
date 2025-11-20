using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace System.Net.Http;

/// <summary>Represents a HTTP request message.</summary>
public class HttpRequestMessage : IDisposable
{
	private HttpRequestHeaders headers;

	private HttpMethod method;

	private Version version;

	private Dictionary<string, object> properties;

	private Uri uri;

	private bool is_used;

	private bool disposed;

	/// <summary>Gets or sets the contents of the HTTP message. </summary>
	/// <returns>Returns <see cref="T:System.Net.Http.HttpContent" />.The content of a message</returns>
	public HttpContent Content { get; set; }

	/// <summary>Gets the collection of HTTP request headers.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.Headers.HttpRequestHeaders" />.The collection of HTTP request headers.</returns>
	public HttpRequestHeaders Headers => headers ?? (headers = new HttpRequestHeaders());

	/// <summary>Gets or sets the HTTP method used by the HTTP request message.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.HttpMethod" />.The HTTP method used by the request message. The default is the GET method.</returns>
	public HttpMethod Method
	{
		get
		{
			return method;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("method");
			}
			method = value;
		}
	}

	/// <summary>Gets a set of properties for the HTTP request.</summary>
	/// <returns>Returns <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
	public IDictionary<string, object> Properties => properties ?? (properties = new Dictionary<string, object>());

	/// <summary>Gets or sets the <see cref="T:System.Uri" /> used for the HTTP request.</summary>
	/// <returns>Returns <see cref="T:System.Uri" />.The <see cref="T:System.Uri" /> used for the HTTP request.</returns>
	public Uri RequestUri
	{
		get
		{
			return uri;
		}
		set
		{
			if (value != null && value.IsAbsoluteUri && !IsAllowedAbsoluteUri(value))
			{
				throw new ArgumentException("Only http or https scheme is allowed");
			}
			uri = value;
		}
	}

	/// <summary>Gets or sets the HTTP message version.</summary>
	/// <returns>Returns <see cref="T:System.Version" />.The HTTP message version. The default is 1.1.</returns>
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.HttpRequestMessage" /> class.</summary>
	public HttpRequestMessage()
	{
		method = HttpMethod.Get;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.HttpRequestMessage" /> class with an HTTP method and a request <see cref="T:System.Uri" />.</summary>
	/// <param name="method">The HTTP method.</param>
	/// <param name="requestUri">A string that represents the request  <see cref="T:System.Uri" />.</param>
	public HttpRequestMessage(HttpMethod method, string requestUri)
		: this(method, string.IsNullOrEmpty(requestUri) ? null : new Uri(requestUri, UriKind.RelativeOrAbsolute))
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.HttpRequestMessage" /> class with an HTTP method and a request <see cref="T:System.Uri" />.</summary>
	/// <param name="method">The HTTP method.</param>
	/// <param name="requestUri">The <see cref="T:System.Uri" /> to request.</param>
	public HttpRequestMessage(HttpMethod method, Uri requestUri)
	{
		Method = method;
		RequestUri = requestUri;
	}

	private static bool IsAllowedAbsoluteUri(Uri uri)
	{
		if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
		{
			return true;
		}
		if (uri.Scheme == Uri.UriSchemeFile && uri.OriginalString.StartsWith("/", StringComparison.Ordinal))
		{
			return true;
		}
		return false;
	}

	/// <summary>Releases the unmanaged resources and disposes of the managed resources used by the <see cref="T:System.Net.Http.HttpRequestMessage" />.</summary>
	public void Dispose()
	{
		Dispose(disposing: true);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Net.Http.HttpRequestMessage" /> and optionally disposes of the managed resources.</summary>
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

	internal bool SetIsUsed()
	{
		if (is_used)
		{
			return true;
		}
		is_used = true;
		return false;
	}

	/// <summary>Returns a string that represents the current object.</summary>
	/// <returns>Returns <see cref="T:System.String" />.A string representation of the current object.</returns>
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Method: ").Append(method);
		stringBuilder.Append(", RequestUri: '").Append((RequestUri != null) ? RequestUri.ToString() : "<null>");
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
