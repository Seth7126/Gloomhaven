namespace System.Net;

/// <summary>Defines the HTTP version numbers that are supported by the <see cref="T:System.Net.HttpWebRequest" /> and <see cref="T:System.Net.HttpWebResponse" /> classes.</summary>
public class HttpVersion
{
	public static readonly Version Unknown = new Version(0, 0);

	/// <summary>Defines a <see cref="T:System.Version" /> instance for HTTP 1.0.</summary>
	public static readonly Version Version10 = new Version(1, 0);

	/// <summary>Defines a <see cref="T:System.Version" /> instance for HTTP 1.1.</summary>
	public static readonly Version Version11 = new Version(1, 1);

	public static readonly Version Version20 = new Version(2, 0);

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.HttpVersion" /> class.</summary>
	public HttpVersion()
	{
	}
}
