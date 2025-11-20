using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http;

/// <summary>A type for HTTP handlers that delegate the processing of HTTP response messages to another handler, called the inner handler.</summary>
public abstract class DelegatingHandler : HttpMessageHandler
{
	private bool disposed;

	private HttpMessageHandler handler;

	/// <summary>Gets or sets the inner handler which processes the HTTP response messages.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.HttpMessageHandler" />.The inner handler for HTTP response messages.</returns>
	public HttpMessageHandler InnerHandler
	{
		get
		{
			return handler;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("InnerHandler");
			}
			handler = value;
		}
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Net.Http.DelegatingHandler" /> class.</summary>
	protected DelegatingHandler()
	{
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Net.Http.DelegatingHandler" /> class with a specific inner handler.</summary>
	/// <param name="innerHandler">The inner handler which is responsible for processing the HTTP response messages.</param>
	protected DelegatingHandler(HttpMessageHandler innerHandler)
	{
		if (innerHandler == null)
		{
			throw new ArgumentNullException("innerHandler");
		}
		InnerHandler = innerHandler;
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Net.Http.DelegatingHandler" />, and optionally disposes of the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to releases only unmanaged resources. </param>
	protected override void Dispose(bool disposing)
	{
		if (disposing && !disposed)
		{
			disposed = true;
			if (InnerHandler != null)
			{
				InnerHandler.Dispose();
			}
		}
		base.Dispose(disposing);
	}

	/// <summary>Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />. The task object representing the asynchronous operation.</returns>
	/// <param name="request">The HTTP request message to send to the server.</param>
	/// <param name="cancellationToken">A cancellation token to cancel operation.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="request" /> was null.</exception>
	protected internal override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		if (InnerHandler == null)
		{
			throw new InvalidOperationException("The inner handler has not been assigned.");
		}
		return InnerHandler.SendAsync(request, cancellationToken);
	}
}
