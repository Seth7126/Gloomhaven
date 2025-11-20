using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello.Rest;

internal class WebApiClient : IRestClient, IDisposable
{
	private readonly string _baseUri;

	private readonly HttpClient _client;

	public WebApiClient(string baseUri)
	{
		_client = TrelloConfiguration.HttpClientFactory();
		_baseUri = baseUri;
	}

	~WebApiClient()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public Task<IRestResponse> Execute(IRestRequest request, CancellationToken ct)
	{
		return ExecuteAsync(request, ct);
	}

	public Task<IRestResponse<T>> Execute<T>(IRestRequest request, CancellationToken ct) where T : class
	{
		return ExecuteAsync<T>(request, ct);
	}

	private async Task<IRestResponse> ExecuteAsync(IRestRequest request, CancellationToken ct)
	{
		WebApiRestRequest webRequest = (WebApiRestRequest)request;
		return request.Method switch
		{
			RestMethod.Get => await ExecuteWithRetry(() => _client.GetAsync(GetFullResource(webRequest), ct)), 
			RestMethod.Put => await ExecuteWithRetry(() => _client.PutAsync(GetFullResource(webRequest), GetContent(webRequest), ct)), 
			RestMethod.Post => await ExecuteWithRetry(() => _client.PostAsync(GetFullResource(webRequest), GetContent(webRequest), ct)), 
			RestMethod.Delete => await ExecuteWithRetry(() => _client.DeleteAsync(GetFullResource(webRequest), ct)), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private async Task<IRestResponse<T>> ExecuteAsync<T>(IRestRequest request, CancellationToken ct) where T : class
	{
		WebApiRestRequest webRequest = (WebApiRestRequest)request;
		return request.Method switch
		{
			RestMethod.Get => await ExecuteWithRetry<T>(() => _client.GetAsync(GetFullResource(webRequest), ct)), 
			RestMethod.Put => await ExecuteWithRetry<T>(() => _client.PutAsync(GetFullResource(webRequest), GetContent(webRequest), ct)), 
			RestMethod.Post => await ExecuteWithRetry<T>(() => _client.PostAsync(GetFullResource(webRequest), GetContent(webRequest), ct)), 
			RestMethod.Delete => await ExecuteWithRetry<T>(() => _client.DeleteAsync(GetFullResource(webRequest), ct)), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private static async Task<IRestResponse> ExecuteWithRetry(Func<Task<HttpResponseMessage>> call)
	{
		int count = 0;
		HttpResponseMessage response;
		IRestResponse restResponse;
		do
		{
			count++;
			response = await call();
			restResponse = await MapResponse(response);
		}
		while (TrelloConfiguration.RetryPredicate(restResponse, count));
		if (!response.IsSuccessStatusCode)
		{
			throw new HttpRequestException("Received a failure from Trello.\n" + $"Status Code: {response.StatusCode} ({(int)response.StatusCode})\n" + "Content: " + restResponse.Content);
		}
		return restResponse;
	}

	private static async Task<IRestResponse> MapResponse(HttpResponseMessage response)
	{
		WebApiRestResponse webApiRestResponse = new WebApiRestResponse();
		WebApiRestResponse webApiRestResponse2 = webApiRestResponse;
		webApiRestResponse2.Content = await response.Content.ReadAsStringAsync();
		webApiRestResponse.StatusCode = response.StatusCode;
		WebApiRestResponse webApiRestResponse3 = webApiRestResponse;
		TrelloConfiguration.Log.Info($"Status Code: {response.StatusCode} ({(int)response.StatusCode})");
		TrelloConfiguration.Log.Debug("\tContent: " + webApiRestResponse3.Content);
		return webApiRestResponse3;
	}

	private static async Task<IRestResponse<T>> ExecuteWithRetry<T>(Func<Task<HttpResponseMessage>> call) where T : class
	{
		int count = 0;
		IRestResponse<T> restResponse;
		do
		{
			count++;
			restResponse = await MapResponse<T>(await call());
		}
		while (TrelloConfiguration.RetryPredicate(restResponse, count));
		return restResponse;
	}

	private static async Task<IRestResponse<T>> MapResponse<T>(HttpResponseMessage response) where T : class
	{
		WebApiRestResponse<T> webApiRestResponse = new WebApiRestResponse<T>();
		WebApiRestResponse<T> webApiRestResponse2 = webApiRestResponse;
		webApiRestResponse2.Content = await response.Content.ReadAsStringAsync();
		webApiRestResponse.StatusCode = response.StatusCode;
		WebApiRestResponse<T> webApiRestResponse3 = webApiRestResponse;
		TrelloConfiguration.Log.Info($"Status Code: {response.StatusCode} ({(int)response.StatusCode})");
		TrelloConfiguration.Log.Debug("\tContent: " + webApiRestResponse3.Content);
		try
		{
			string content = webApiRestResponse3.Content;
			if (!response.IsSuccessStatusCode || response.Content.Headers.ContentType.MediaType == "text/plain")
			{
				webApiRestResponse3.Exception = new TrelloInteractionException(content);
			}
			else
			{
				webApiRestResponse3.Data = TrelloConfiguration.Deserializer.Deserialize<T>(content);
			}
		}
		catch (Exception exception)
		{
			webApiRestResponse3.Exception = exception;
		}
		return webApiRestResponse3;
	}

	private static HttpContent GetContent(WebApiRestRequest request)
	{
		if (request.File != null)
		{
			MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
			foreach (KeyValuePair<string, object> parameter in request.Parameters)
			{
				StringContent content = new StringContent(parameter.Value.ToString());
				multipartFormDataContent.Add(content, "\"" + parameter.Key + "\"");
			}
			ByteArrayContent content2 = new ByteArrayContent(request.File);
			multipartFormDataContent.Add(content2, "\"file\"", "\"" + request.FileName + "\"");
			TrelloConfiguration.Log.Debug($"\tContent: {multipartFormDataContent}");
			return multipartFormDataContent;
		}
		if (request.Body == null)
		{
			return null;
		}
		string text = TrelloConfiguration.Serializer.Serialize(request.Body);
		StringContent stringContent = new StringContent(text);
		stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
		TrelloConfiguration.Log.Debug("\tContent: " + text);
		return stringContent;
	}

	private string GetFullResource(WebApiRestRequest request)
	{
		TrelloConfiguration.Log.Info($"Sending: {request.Method} {request.Resource}");
		if (request.File != null)
		{
			return _baseUri + "/" + request.Resource;
		}
		return _baseUri + "/" + request.Resource + "?" + string.Join("&", request.Parameters.Select((KeyValuePair<string, object> kvp) => kvp.Key + "=" + UrlEncode(kvp.Value)));
	}

	private void Dispose(bool disposing)
	{
		if (disposing)
		{
			_client?.Dispose();
		}
	}

	private static string UrlEncode(object value)
	{
		return WebUtility.UrlEncode(value.ToString());
	}
}
