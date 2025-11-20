using System.Net.Http.Headers;

namespace System.Net.Http;

/// <summary>A helper class for retrieving and comparing standard HTTP methods and for creating new HTTP methods.</summary>
public class HttpMethod : IEquatable<HttpMethod>
{
	private static readonly HttpMethod delete_method = new HttpMethod("DELETE");

	private static readonly HttpMethod get_method = new HttpMethod("GET");

	private static readonly HttpMethod head_method = new HttpMethod("HEAD");

	private static readonly HttpMethod options_method = new HttpMethod("OPTIONS");

	private static readonly HttpMethod post_method = new HttpMethod("POST");

	private static readonly HttpMethod put_method = new HttpMethod("PUT");

	private static readonly HttpMethod trace_method = new HttpMethod("TRACE");

	private readonly string method;

	/// <summary>Represents an HTTP DELETE protocol method.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.HttpMethod" />.</returns>
	public static HttpMethod Delete => delete_method;

	/// <summary>Represents an HTTP GET protocol method.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.HttpMethod" />.</returns>
	public static HttpMethod Get => get_method;

	/// <summary>Represents an HTTP HEAD protocol method. The HEAD method is identical to GET except that the server only returns message-headers in the response, without a message-body.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.HttpMethod" />.</returns>
	public static HttpMethod Head => head_method;

	/// <summary>An HTTP method. </summary>
	/// <returns>Returns <see cref="T:System.String" />.An HTTP method represented as a <see cref="T:System.String" />.</returns>
	public string Method => method;

	/// <summary>Represents an HTTP OPTIONS protocol method.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.HttpMethod" />.</returns>
	public static HttpMethod Options => options_method;

	/// <summary>Represents an HTTP POST protocol method that is used to post a new entity as an addition to a URI.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.HttpMethod" />.</returns>
	public static HttpMethod Post => post_method;

	/// <summary>Represents an HTTP PUT protocol method that is used to replace an entity identified by a URI.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.HttpMethod" />.</returns>
	public static HttpMethod Put => put_method;

	/// <summary>Represents an HTTP TRACE protocol method.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.HttpMethod" />.</returns>
	public static HttpMethod Trace => trace_method;

	public static HttpMethod Patch
	{
		get
		{
			throw new PlatformNotSupportedException();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.HttpMethod" /> class with a specific HTTP method.</summary>
	/// <param name="method">The HTTP method.</param>
	public HttpMethod(string method)
	{
		if (string.IsNullOrEmpty(method))
		{
			throw new ArgumentException("method");
		}
		Parser.Token.Check(method);
		this.method = method;
	}

	/// <summary>The equality operator for comparing two <see cref="T:System.Net.Http.HttpMethod" /> objects.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the specified <paramref name="left" /> and <paramref name="right" /> parameters are equal; otherwise, false.</returns>
	/// <param name="left">The left <see cref="T:System.Net.Http.HttpMethod" /> to an equality operator.</param>
	/// <param name="right">The right  <see cref="T:System.Net.Http.HttpMethod" /> to an equality operator.</param>
	public static bool operator ==(HttpMethod left, HttpMethod right)
	{
		if ((object)left == null || (object)right == null)
		{
			return (object)left == right;
		}
		return left.Equals(right);
	}

	/// <summary>The inequality operator for comparing two <see cref="T:System.Net.Http.HttpMethod" /> objects.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the specified <paramref name="left" /> and <paramref name="right" /> parameters are inequal; otherwise, false.</returns>
	/// <param name="left">The left <see cref="T:System.Net.Http.HttpMethod" /> to an inequality operator.</param>
	/// <param name="right">The right  <see cref="T:System.Net.Http.HttpMethod" /> to an inequality operator.</param>
	public static bool operator !=(HttpMethod left, HttpMethod right)
	{
		return !(left == right);
	}

	/// <summary>Determines whether the specified <see cref="T:System.Net.Http.HttpMethod" /> is equal to the current <see cref="T:System.Object" />.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the specified object is equal to the current object; otherwise, false.</returns>
	/// <param name="other">The HTTP method to compare with the current object.</param>
	public bool Equals(HttpMethod other)
	{
		return string.Equals(method, other.method, StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the specified object is equal to the current object; otherwise, false.</returns>
	/// <param name="obj">The object to compare with the current object.</param>
	public override bool Equals(object obj)
	{
		if (obj is HttpMethod other)
		{
			return Equals(other);
		}
		return false;
	}

	/// <summary>Serves as a hash function for this type.</summary>
	/// <returns>Returns <see cref="T:System.Int32" />.A hash code for the current <see cref="T:System.Object" />.</returns>
	public override int GetHashCode()
	{
		return method.GetHashCode();
	}

	/// <summary>Returns a string that represents the current object.</summary>
	/// <returns>Returns <see cref="T:System.String" />.A string representing the current object.</returns>
	public override string ToString()
	{
		return method;
	}
}
