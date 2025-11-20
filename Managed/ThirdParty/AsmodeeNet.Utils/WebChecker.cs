using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace AsmodeeNet.Utils;

public static class WebChecker
{
	private const string _defaultTargetURL = "https://api.asmodee.net/healthz";

	public static bool IsNetworkReachable => Application.internetReachability != NetworkReachability.NotReachable;

	public static IEnumerator WebRequest(IEnumerator connectionSuccess, IEnumerator connectionError, string targetURL = "https://api.asmodee.net/healthz")
	{
		if (!IsNetworkReachable)
		{
			yield return connectionError;
			yield break;
		}
		UnityWebRequest request = UnityWebRequest.Get(targetURL);
		request.timeout = 5;
		yield return request.SendWebRequest();
		if (request.isNetworkError || request.isHttpError)
		{
			yield return connectionError;
		}
		else
		{
			yield return connectionSuccess;
		}
	}

	public static IEnumerator WebRequest(Action connectionSuccess, Action connectionError, string targetURL = "https://api.asmodee.net/healthz")
	{
		if (!IsNetworkReachable)
		{
			connectionError?.Invoke();
			yield break;
		}
		UnityWebRequest request = UnityWebRequest.Get(targetURL);
		request.timeout = 5;
		yield return request.SendWebRequest();
		if (request.isNetworkError || request.isHttpError)
		{
			connectionError?.Invoke();
		}
		else
		{
			connectionSuccess?.Invoke();
		}
	}
}
