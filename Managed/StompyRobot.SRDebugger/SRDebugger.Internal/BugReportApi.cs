using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SRDebugger.Services;
using SRF;
using UnityEngine;
using UnityEngine.Networking;

namespace SRDebugger.Internal;

internal class BugReportApi : MonoBehaviour
{
	private string _apiKey;

	private BugReport _bugReport;

	private bool _isBusy;

	private UnityWebRequest _webRequest;

	private Action<BugReportSubmitResult> _onComplete;

	private IProgress<float> _progress;

	public static void Submit(BugReport report, string apiKey, Action<BugReportSubmitResult> onComplete, IProgress<float> progress)
	{
		GameObject obj = new GameObject("BugReportApi");
		obj.transform.parent = Hierarchy.Get("SRDebugger");
		BugReportApi bugReportApi = obj.AddComponent<BugReportApi>();
		bugReportApi.Init(report, apiKey, onComplete, progress);
		bugReportApi.StartCoroutine(bugReportApi.Submit());
	}

	private void Init(BugReport report, string apiKey, Action<BugReportSubmitResult> onComplete, IProgress<float> progress)
	{
		_bugReport = report;
		_apiKey = apiKey;
		_onComplete = onComplete;
		_progress = progress;
	}

	private void Update()
	{
		if (_isBusy && _webRequest != null)
		{
			_progress.Report(_webRequest.uploadProgress);
		}
	}

	private IEnumerator Submit()
	{
		if (_isBusy)
		{
			throw new InvalidOperationException("BugReportApi is already sending a bug report");
		}
		_isBusy = true;
		_webRequest = null;
		byte[] bytes;
		try
		{
			string s = BuildJsonRequest(_bugReport);
			bytes = Encoding.UTF8.GetBytes(s);
		}
		catch (Exception message)
		{
			Debug.LogError(message);
			SetCompletionState(BugReportSubmitResult.Error("Error building bug report."));
			yield break;
		}
		try
		{
			_webRequest = new UnityWebRequest("https://srdebugger.stompyrobot.uk/report/submit", "POST", new DownloadHandlerBuffer(), new UploadHandlerRaw(bytes)
			{
				contentType = "application/json"
			});
			_webRequest.SetRequestHeader("Accept", "application/json");
			_webRequest.SetRequestHeader("X-ApiKey", _apiKey);
		}
		catch (Exception message2)
		{
			Debug.LogError(message2);
			if (_webRequest != null)
			{
				_webRequest.Dispose();
				_webRequest = null;
			}
		}
		if (_webRequest == null)
		{
			SetCompletionState(BugReportSubmitResult.Error("Error building bug report request."));
			yield break;
		}
		yield return _webRequest.SendWebRequest();
		if (_webRequest.result == UnityWebRequest.Result.ConnectionError || _webRequest.result == UnityWebRequest.Result.DataProcessingError)
		{
			SetCompletionState(BugReportSubmitResult.Error("Request Error: " + _webRequest.error));
			_webRequest.Dispose();
			yield break;
		}
		long responseCode = _webRequest.responseCode;
		string text = _webRequest.downloadHandler.text;
		_webRequest.Dispose();
		if (responseCode != 200)
		{
			SetCompletionState(BugReportSubmitResult.Error("Server: " + SRDebugApiUtil.ParseErrorResponse(text, "Unknown response from server")));
		}
		else
		{
			SetCompletionState(BugReportSubmitResult.Success);
		}
	}

	private void SetCompletionState(BugReportSubmitResult result)
	{
		_bugReport.ScreenshotData = null;
		_isBusy = false;
		if (!result.IsSuccessful)
		{
			Debug.LogError("Bug Reporter Error: " + result.ErrorMessage);
		}
		UnityEngine.Object.Destroy(base.gameObject);
		_onComplete(result);
	}

	private static string BuildJsonRequest(BugReport report)
	{
		Hashtable hashtable = new Hashtable();
		hashtable.Add("userEmail", report.Email);
		hashtable.Add("userDescription", report.UserDescription);
		hashtable.Add("console", CreateConsoleDump());
		hashtable.Add("systemInformation", report.SystemInformation);
		hashtable.Add("applicationIdentifier", Application.identifier);
		if (report.ScreenshotData != null)
		{
			hashtable.Add("screenshot", Convert.ToBase64String(report.ScreenshotData));
		}
		return Json.Serialize(hashtable);
	}

	private static List<List<string>> CreateConsoleDump()
	{
		IReadOnlyList<ConsoleEntry> allEntries = Service.Console.AllEntries;
		List<List<string>> list = new List<List<string>>(allEntries.Count);
		foreach (ConsoleEntry item in allEntries)
		{
			List<string> list2 = new List<string>(5);
			list2.Add(item.LogType.ToString());
			list2.Add(item.Message);
			list2.Add(item.StackTrace);
			if (item.Count > 1)
			{
				list2.Add(item.Count.ToString());
			}
			list.Add(list2);
		}
		return list;
	}
}
