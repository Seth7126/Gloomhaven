using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.Logging;
using Manatee.Trello.Json;
using Manatee.Trello.Rest;

namespace Manatee.Trello;

public static class TrelloConfiguration
{
	private static ILog _log;

	private static ISerializer _serializer;

	private static IDeserializer _deserializer;

	private static IRestClientProvider _restClientProvider;

	private static ICache _cache;

	private static IJsonFactory _jsonFactory;

	private static Func<IRestResponse, int, bool> _retryPredicate;

	private static Func<HttpClient> _httpClientFactory;

	public static ISerializer Serializer
	{
		get
		{
			return _serializer ?? (_serializer = DefaultJsonSerializer.Instance);
		}
		set
		{
			_serializer = value;
		}
	}

	public static IDeserializer Deserializer
	{
		get
		{
			return _deserializer ?? (_deserializer = DefaultJsonSerializer.Instance);
		}
		set
		{
			_deserializer = value;
		}
	}

	public static IRestClientProvider RestClientProvider
	{
		get
		{
			return _restClientProvider ?? (_restClientProvider = DefaultRestClientProvider.Instance);
		}
		set
		{
			_restClientProvider = value;
		}
	}

	public static ICache Cache
	{
		get
		{
			return _cache ?? (_cache = new ConcurrentCache());
		}
		set
		{
			_cache = value;
		}
	}

	public static bool RemoveDeletedItemsFromCache { get; set; }

	public static ILog Log
	{
		get
		{
			return _log ?? (_log = new DebugLog());
		}
		set
		{
			_log = value;
		}
	}

	public static IJsonFactory JsonFactory
	{
		get
		{
			return _jsonFactory ?? (_jsonFactory = DefaultJsonFactory.Instance);
		}
		set
		{
			_jsonFactory = value;
		}
	}

	public static bool ThrowOnTrelloError { get; set; }

	public static TimeSpan ChangeSubmissionTime { get; set; }

	public static TimeSpan RefreshThrottle { get; set; }

	public static IList<HttpStatusCode> RetryStatusCodes { get; }

	public static int MaxRetryCount { get; set; }

	public static TimeSpan DelayBetweenRetries { get; set; }

	public static Func<IRestResponse, int, bool> RetryPredicate
	{
		get
		{
			return _retryPredicate ?? new Func<IRestResponse, int, bool>(DefaultRetry);
		}
		set
		{
			_retryPredicate = value;
		}
	}

	public static Func<HttpClient> HttpClientFactory
	{
		get
		{
			return _httpClientFactory ?? ((Func<HttpClient>)(() => new HttpClient()));
		}
		set
		{
			_httpClientFactory = value;
		}
	}

	public static bool EnableConsistencyProcessing { get; set; }

	public static bool EnableDeepDownloads { get; set; }

	internal static Dictionary<string, Func<IJsonPowerUp, TrelloAuthorization, IPowerUp>> RegisteredPowerUps { get; }

	static TrelloConfiguration()
	{
		ThrowOnTrelloError = true;
		ChangeSubmissionTime = TimeSpan.FromMilliseconds(100.0);
		RegisteredPowerUps = new Dictionary<string, Func<IJsonPowerUp, TrelloAuthorization, IPowerUp>>();
		RetryStatusCodes = new List<HttpStatusCode>();
		RemoveDeletedItemsFromCache = true;
		RefreshThrottle = TimeSpan.FromSeconds(5.0);
		EnableDeepDownloads = true;
	}

	public static void RegisterPowerUp(string id, Func<IJsonPowerUp, TrelloAuthorization, IPowerUp> factory)
	{
		RegisteredPowerUps[id] = factory;
	}

	private static bool DefaultRetry(IRestResponse response, int callCount)
	{
		int num;
		if (RetryStatusCodes.Contains(response.StatusCode))
		{
			num = ((callCount <= MaxRetryCount) ? 1 : 0);
			if (num != 0)
			{
				Thread.Sleep(DelayBetweenRetries);
			}
		}
		else
		{
			num = 0;
		}
		return (byte)num != 0;
	}
}
