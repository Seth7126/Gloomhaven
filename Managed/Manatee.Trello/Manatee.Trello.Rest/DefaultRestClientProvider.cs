using System;

namespace Manatee.Trello.Rest;

public class DefaultRestClientProvider : IRestClientProvider, IDisposable
{
	private WebApiClient _client;

	public static DefaultRestClientProvider Instance { get; } = new DefaultRestClientProvider();

	public virtual IRestRequestProvider RequestProvider { get; }

	protected DefaultRestClientProvider()
	{
		RequestProvider = new WebApiRequestProvider();
	}

	~DefaultRestClientProvider()
	{
		Dispose(disposing: false);
	}

	public virtual IRestClient CreateRestClient(string apiBaseUrl)
	{
		return _client ?? (_client = new WebApiClient(apiBaseUrl));
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_client?.Dispose();
		}
	}
}
