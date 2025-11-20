#define DEBUG
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Photon.Bolt.Internal;
using Photon.Bolt.Utils;
using UnityEngine;

namespace Photon.Bolt.Services;

internal class CCUService : GlobalEventListenerBase
{
	private class CCUReport
	{
		public int Total { get; set; }

		public string LicenseKey { get; set; }

		public string AppId { get; set; }
	}

	private readonly float PublishInterval = 10f;

	private readonly string ServerUrl = "http://localhost:3000/ccu";

	private string AppId;

	private string LicenseKey;

	public override void BoltStartDone()
	{
		AppId = BoltRuntimeSettings.instance.photonAppId;
		LicenseKey = default(Guid).ToString();
		StartCoroutine(SendReport());
	}

	private IEnumerator SendReport()
	{
		while (true)
		{
			SendData(total: BoltNetwork.Connections.Count(), serverUrl: ServerUrl, appId: AppId, licenseKey: LicenseKey);
			yield return new WaitForSecondsRealtime(PublishInterval);
		}
	}

	private void SendData(string serverUrl, string appId, string licenseKey, int total)
	{
		if (string.IsNullOrEmpty(serverUrl) || string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(licenseKey))
		{
			return;
		}
		CCUReport obj = new CCUReport
		{
			AppId = appId,
			LicenseKey = licenseKey,
			Total = total
		};
		string s = JsonUtility.ToJson(obj);
		string message = null;
		try
		{
			WebRequest webRequest = WebRequest.Create(BuildURL(serverUrl, appId, licenseKey, total));
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			webRequest.Method = "POST";
			webRequest.ContentType = "text/plain";
			webRequest.ContentLength = 0L;
			using Stream stream = webRequest.GetRequestStream();
			stream.Write(bytes, 0, bytes.Length);
			stream.Close();
			using WebResponse webResponse = webRequest.GetResponse();
			using StreamReader streamReader = new StreamReader(webResponse.GetResponseStream());
			message = streamReader.ReadToEnd();
		}
		catch (Exception exception)
		{
			BoltLog.Exception(exception);
		}
		BoltLog.Info(message);
	}

	private string BuildURL(string baseURL, string appID, string license, int total)
	{
		return $"{baseURL}/{license}/{appID}/{total}";
	}
}
