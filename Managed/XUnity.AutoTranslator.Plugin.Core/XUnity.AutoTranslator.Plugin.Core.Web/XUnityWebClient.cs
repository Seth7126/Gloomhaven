using System;
using System.Net;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Web.Internal;

namespace XUnity.AutoTranslator.Plugin.Core.Web;

public class XUnityWebClient : ConnectionTrackingWebClient
{
	private HttpStatusCode? _responseCode;

	private CookieCollection _responseCookies;

	private CookieContainer _requestCookies;

	private WebHeaderCollection _requestHeaders;

	public XUnityWebClient()
	{
		base.Encoding = Encoding.UTF8;
	}

	private void UnityWebClient_UploadStringCompleted(object sender, XUnityUploadStringCompletedEventArgs ev)
	{
		base.UploadStringCompleted -= UnityWebClient_UploadStringCompleted;
		XUnityWebResponse xUnityWebResponse = ev.UserState as XUnityWebResponse;
		try
		{
			xUnityWebResponse.SetCompleted(_responseCode.HasValue ? _responseCode.Value : HttpStatusCode.BadRequest, ev.Result, responseHeaders, _responseCookies, ev.Error);
		}
		catch (Exception)
		{
			xUnityWebResponse.SetCompleted(_responseCode.HasValue ? _responseCode.Value : HttpStatusCode.BadRequest, null, responseHeaders, _responseCookies, ev.Error);
		}
	}

	private void UnityWebClient_DownloadStringCompleted(object sender, XUnityDownloadStringCompletedEventArgs ev)
	{
		base.DownloadStringCompleted -= UnityWebClient_DownloadStringCompleted;
		XUnityWebResponse xUnityWebResponse = ev.UserState as XUnityWebResponse;
		try
		{
			xUnityWebResponse.SetCompleted(_responseCode.HasValue ? _responseCode.Value : HttpStatusCode.BadRequest, ev.Result, responseHeaders, _responseCookies, ev.Error);
		}
		catch (Exception)
		{
			xUnityWebResponse.SetCompleted(_responseCode.HasValue ? _responseCode.Value : HttpStatusCode.BadRequest, null, responseHeaders, _responseCookies, ev.Error);
		}
	}

	protected override WebRequest GetWebRequest(Uri address)
	{
		WebRequest webRequest = base.GetWebRequest(address);
		SetRequestVariables(webRequest);
		return webRequest;
	}

	protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
	{
		WebResponse webResponse = base.GetWebResponse(request, result);
		SetResponseVariables(webResponse);
		return webResponse;
	}

	protected override WebResponse GetWebResponse(WebRequest request)
	{
		WebResponse webResponse = base.GetWebResponse(request);
		SetResponseVariables(webResponse);
		return webResponse;
	}

	private void SetRequestVariables(WebRequest r)
	{
		if (r is HttpWebRequest httpWebRequest)
		{
			if (_requestCookies != null)
			{
				httpWebRequest.CookieContainer = _requestCookies;
			}
			if (_requestHeaders != null)
			{
				base.Headers = _requestHeaders;
			}
			httpWebRequest.ReadWriteTimeout = (int)(Settings.Timeout * 1000f) - 10000;
			httpWebRequest.Timeout = (int)(Settings.Timeout * 1000f) - 5000;
		}
	}

	private void SetResponseVariables(WebResponse r)
	{
		if (r is HttpWebResponse httpWebResponse)
		{
			_responseCode = httpWebResponse.StatusCode;
			_responseCookies = httpWebResponse.Cookies;
		}
	}

	public XUnityWebResponse Send(XUnityWebRequest request)
	{
		XUnityWebResponse xUnityWebResponse = new XUnityWebResponse();
		_requestCookies = request.Cookies;
		_requestHeaders = request.Headers;
		if (request.Data == null)
		{
			try
			{
				base.DownloadStringCompleted += UnityWebClient_DownloadStringCompleted;
				DownloadStringAsync(request.Address, xUnityWebResponse);
			}
			catch
			{
				base.DownloadStringCompleted -= UnityWebClient_DownloadStringCompleted;
				throw;
			}
		}
		else
		{
			try
			{
				base.UploadStringCompleted += UnityWebClient_UploadStringCompleted;
				UploadStringAsync(request.Address, request.Method, request.Data, xUnityWebResponse);
			}
			catch
			{
				base.UploadStringCompleted -= UnityWebClient_UploadStringCompleted;
				throw;
			}
		}
		return xUnityWebResponse;
	}
}
