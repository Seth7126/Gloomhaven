using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Rest;

namespace Manatee.Trello.Internal.RequestProcessing;

internal static class RestRequestProcessor
{
	private const string BaseUrl = "https://trello.com/1";

	private static IRestClient Client => TrelloConfiguration.RestClientProvider.CreateRestClient("https://trello.com/1");

	public static event Func<Task> LastCall;

	public static Task<IRestResponse> AddRequest(IRestRequest request, CancellationToken ct)
	{
		return Process(() => Client.Execute(request, ct), request, ct);
	}

	public static Task<IRestResponse> AddRequest<T>(IRestRequest request, CancellationToken ct) where T : class
	{
		return Process(async () => await Client.Execute<T>(request, ct), request, ct);
	}

	public static async Task Flush()
	{
		if (RestRequestProcessor.LastCall != null)
		{
			await Task.WhenAll(from Func<Task> h in RestRequestProcessor.LastCall.GetInvocationList()
				select h());
		}
	}

	private static async Task<IRestResponse> Process(Func<Task<IRestResponse>> ask, IRestRequest request, CancellationToken ct)
	{
		IRestResponse result;
		try
		{
			result = await Execute(ask, request, ct);
		}
		catch (Exception ex)
		{
			result = new NullRestResponse
			{
				Exception = ex
			};
			TrelloConfiguration.Log.Error(ex);
		}
		return result;
	}

	private static async Task<IRestResponse> Execute(Func<Task<IRestResponse>> ask, IRestRequest request, CancellationToken ct)
	{
		IRestResponse result;
		if (!ct.IsCancellationRequested)
		{
			try
			{
				result = await ask();
			}
			catch (Exception ex)
			{
				TrelloInteractionException e = new TrelloInteractionException(ex);
				result = new NullRestResponse
				{
					Exception = ex
				};
				TrelloConfiguration.Log.Error(e);
			}
		}
		else
		{
			result = new NullRestResponse();
		}
		return result;
	}
}
