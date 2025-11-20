using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.RequestProcessing;
using Manatee.Trello.Rest;

namespace Manatee.Trello.Internal.DataAccess;

internal static class JsonRepository
{
	public static async Task Execute(TrelloAuthorization auth, Endpoint endpoint, CancellationToken ct, IDictionary<string, object> parameters = null)
	{
		ValidateResponse(await RestRequestProcessor.AddRequest(BuildRequest(auth, endpoint, parameters), ct));
	}

	public static async Task<T> Execute<T>(TrelloAuthorization auth, Endpoint endpoint, CancellationToken ct, IDictionary<string, object> parameters = null) where T : class
	{
		IRestResponse<T> obj = await ProcessRequest<T>(BuildRequest(auth, endpoint, parameters), ct);
		return (obj != null) ? obj.Data : null;
	}

	public static async Task<T> Execute<T>(TrelloAuthorization auth, Endpoint endpoint, T body, CancellationToken ct) where T : class
	{
		IRestRequest restRequest = BuildRequest(auth, endpoint);
		restRequest.AddBody(body);
		IRestResponse<T> obj = await ProcessRequest<T>(restRequest, ct);
		return (obj != null) ? obj.Data : null;
	}

	public static async Task<TResponse> Execute<TRequest, TResponse>(TrelloAuthorization auth, Endpoint endpoint, TRequest body, CancellationToken ct) where TResponse : class
	{
		IRestRequest restRequest = BuildRequest(auth, endpoint);
		restRequest.AddBody(body);
		IRestResponse<TResponse> obj = await ProcessRequest<TResponse>(restRequest, ct);
		return (obj != null) ? obj.Data : null;
	}

	private static IRestRequest BuildRequest(TrelloAuthorization auth, Endpoint endpoint, IDictionary<string, object> parameters = null)
	{
		IRestRequest restRequest = TrelloConfiguration.RestClientProvider.RequestProvider.Create(endpoint.ToString(), parameters);
		restRequest.Method = endpoint.Method;
		PrepRequest(restRequest, auth);
		return restRequest;
	}

	private static void PrepRequest(IRestRequest request, TrelloAuthorization auth)
	{
		request.AddParameter("key", auth.AppKey);
		if (auth.UserToken != null)
		{
			request.AddParameter("token", auth.UserToken);
		}
	}

	private static void ValidateResponse(IRestResponse response)
	{
		if (response.Exception != null)
		{
			TrelloConfiguration.Log.Error(response.Exception);
			if (TrelloConfiguration.ThrowOnTrelloError)
			{
				throw response.Exception;
			}
		}
	}

	private static async Task<IRestResponse<T>> ProcessRequest<T>(IRestRequest request, CancellationToken ct) where T : class
	{
		IRestResponse obj = await RestRequestProcessor.AddRequest<T>(request, ct);
		ValidateResponse(obj);
		return obj as IRestResponse<T>;
	}
}
