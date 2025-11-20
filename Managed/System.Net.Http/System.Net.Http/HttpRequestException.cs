namespace System.Net.Http;

/// <summary>A base class for exceptions thrown by the <see cref="T:System.Net.Http.HttpClient" /> and <see cref="T:System.Net.Http.HttpMessageHandler" /> classes.</summary>
[Serializable]
public class HttpRequestException : Exception
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.HttpRequestException" /> class.</summary>
	public HttpRequestException()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.HttpRequestException" /> class with a specific message that describes the current exception.</summary>
	/// <param name="message">A message that describes the current exception.</param>
	public HttpRequestException(string message)
		: base(message)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.HttpRequestException" /> class with a specific message that describes the current exception and an inner exception.</summary>
	/// <param name="message">A message that describes the current exception.</param>
	/// <param name="inner">The inner exception.</param>
	public HttpRequestException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
