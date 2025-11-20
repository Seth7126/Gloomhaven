using System.Collections.Generic;
using Unity;

namespace System.Net.Http.Headers;

/// <summary>Represents the collection of Content Headers as defined in RFC 2616.</summary>
public sealed class HttpContentHeaders : HttpHeaders
{
	private readonly HttpContent content;

	/// <summary>Gets the value of the Allow content header on an HTTP response. </summary>
	/// <returns>Returns <see cref="T:System.Collections.Generic.ICollection`1" />.The value of the Allow header on an HTTP response.</returns>
	public ICollection<string> Allow => GetValues<string>("Allow");

	/// <summary>Gets the value of the Content-Encoding content header on an HTTP response.</summary>
	/// <returns>Returns <see cref="T:System.Collections.Generic.ICollection`1" />.The value of the Content-Encoding content header on an HTTP response.</returns>
	public ICollection<string> ContentEncoding => GetValues<string>("Content-Encoding");

	/// <summary>Gets the value of the Content-Disposition content header on an HTTP response.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.Headers.ContentDispositionHeaderValue" />.The value of the Content-Disposition content header on an HTTP response.</returns>
	public ContentDispositionHeaderValue ContentDisposition
	{
		get
		{
			return GetValue<ContentDispositionHeaderValue>("Content-Disposition");
		}
		set
		{
			AddOrRemove("Content-Disposition", value);
		}
	}

	/// <summary>Gets the value of the Content-Language content header on an HTTP response.</summary>
	/// <returns>Returns <see cref="T:System.Collections.Generic.ICollection`1" />.The value of the Content-Language content header on an HTTP response.</returns>
	public ICollection<string> ContentLanguage => GetValues<string>("Content-Language");

	/// <summary>Gets or sets the value of the Content-Length content header on an HTTP response.</summary>
	/// <returns>Returns <see cref="T:System.Int64" />.The value of the Content-Length content header on an HTTP response.</returns>
	public long? ContentLength
	{
		get
		{
			long? value = GetValue<long?>("Content-Length");
			if (value.HasValue)
			{
				return value;
			}
			value = content.LoadedBufferLength;
			if (value.HasValue)
			{
				return value;
			}
			if (content.TryComputeLength(out var length))
			{
				SetValue("Content-Length", length);
				return length;
			}
			return null;
		}
		set
		{
			AddOrRemove("Content-Length", value);
		}
	}

	/// <summary>Gets or sets the value of the Content-Location content header on an HTTP response.</summary>
	/// <returns>Returns <see cref="T:System.Uri" />.The value of the Content-Location content header on an HTTP response.</returns>
	public Uri ContentLocation
	{
		get
		{
			return GetValue<Uri>("Content-Location");
		}
		set
		{
			AddOrRemove("Content-Location", value);
		}
	}

	/// <summary>Gets or sets the value of the Content-MD5 content header on an HTTP response.</summary>
	/// <returns>Returns <see cref="T:System.Byte" />.The value of the Content-MD5 content header on an HTTP response.</returns>
	public byte[] ContentMD5
	{
		get
		{
			return GetValue<byte[]>("Content-MD5");
		}
		set
		{
			AddOrRemove("Content-MD5", value, Parser.MD5.ToString);
		}
	}

	/// <summary>Gets or sets the value of the Content-Range content header on an HTTP response.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.Headers.ContentRangeHeaderValue" />.The value of the Content-Range content header on an HTTP response.</returns>
	public ContentRangeHeaderValue ContentRange
	{
		get
		{
			return GetValue<ContentRangeHeaderValue>("Content-Range");
		}
		set
		{
			AddOrRemove("Content-Range", value);
		}
	}

	/// <summary>Gets or sets the value of the Content-Type content header on an HTTP response.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.Headers.MediaTypeHeaderValue" />.The value of the Content-Type content header on an HTTP response.</returns>
	public MediaTypeHeaderValue ContentType
	{
		get
		{
			return GetValue<MediaTypeHeaderValue>("Content-Type");
		}
		set
		{
			AddOrRemove("Content-Type", value);
		}
	}

	/// <summary>Gets or sets the value of the Expires content header on an HTTP response.</summary>
	/// <returns>Returns <see cref="T:System.DateTimeOffset" />.The value of the Expires content header on an HTTP response.</returns>
	public DateTimeOffset? Expires
	{
		get
		{
			return GetValue<DateTimeOffset?>("Expires");
		}
		set
		{
			AddOrRemove("Expires", value, Parser.DateTime.ToString);
		}
	}

	/// <summary>Gets or sets the value of the Last-Modified content header on an HTTP response.</summary>
	/// <returns>Returns <see cref="T:System.DateTimeOffset" />.The value of the Last-Modified content header on an HTTP response.</returns>
	public DateTimeOffset? LastModified
	{
		get
		{
			return GetValue<DateTimeOffset?>("Last-Modified");
		}
		set
		{
			AddOrRemove("Last-Modified", value, Parser.DateTime.ToString);
		}
	}

	internal HttpContentHeaders(HttpContent content)
		: base(HttpHeaderKind.Content)
	{
		this.content = content;
	}

	internal HttpContentHeaders()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
