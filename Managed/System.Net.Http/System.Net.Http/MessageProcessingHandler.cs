using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http;

/// <summary>A base type for handlers which only do some small processing of request and/or response messages.</summary>
public abstract class MessageProcessingHandler : DelegatingHandler
{
	/// <summary>Creates an instance of a <see cref="T:System.Net.Http.MessageProcessingHandler" /> class.</summary>
	protected MessageProcessingHandler()
	{
	}

	/// <summary>Creates an instance of a <see cref="T:System.Net.Http.MessageProcessingHandler" /> class with a specific inner handler.</summary>
	/// <param name="innerHandler">The inner handler which is responsible for processing the HTTP response messages.</param>
	protected MessageProcessingHandler(HttpMessageHandler innerHandler)
		: base(innerHandler)
	{
	}

	/// <summary>Performs processing on each request sent to the server.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.HttpRequestMessage" />.The HTTP request message that was processed.</returns>
	/// <param name="request">The HTTP request message to process.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	protected abstract HttpRequestMessage ProcessRequest(HttpRequestMessage request, CancellationToken cancellationToken);

	/// <summary>Perform processing on each response from the server.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.HttpResponseMessage" />.The HTTP response message that was processed.</returns>
	/// <param name="response">The HTTP response message to process.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	protected abstract HttpResponseMessage ProcessResponse(HttpResponseMessage response, CancellationToken cancellationToken);

	/// <summary>Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation.</returns>
	/// <param name="request">The HTTP request message to send to the server.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="request" /> was null.</exception>
	protected internal sealed override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		request = ProcessRequest(request, cancellationToken);
		return ProcessResponse(await base.SendAsync(request, cancellationToken).ConfigureAwait(continueOnCapturedContext: false), cancellationToken);
	}
}
