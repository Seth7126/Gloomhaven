using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core.Web.Internal;

public class ConnectionTrackingWebClient
{
	private class ServicePointState
	{
		public ServicePoint ServicePoint { get; }

		public DateTime LastUse { get; set; }

		public ServicePointState(ServicePoint servicePoint)
		{
			ServicePoint = servicePoint;
		}
	}

	private static readonly TimeSpan MaxUnusedLifespan;

	private static readonly string ConnectionGroupName;

	private static readonly Dictionary<string, ServicePointState> ActiveConnections;

	private static readonly Dictionary<string, ServicePoint> TouchedServicePoints;

	private bool async;

	private Uri baseAddress;

	private string baseString;

	private ICredentials credentials;

	private Encoding encoding = Encoding.Default;

	private WebHeaderCollection headers;

	private static byte[] hexBytes;

	private bool is_busy;

	private IWebProxy proxy;

	private NameValueCollection queryString;

	protected WebHeaderCollection responseHeaders;

	private static readonly string urlEncodedCType;

	public string BaseAddress
	{
		get
		{
			if (baseString == null && baseAddress == null)
			{
				return string.Empty;
			}
			baseString = baseAddress.ToString();
			return baseString;
		}
		set
		{
			if (value == null || value.Length == 0)
			{
				baseAddress = null;
			}
			else
			{
				baseAddress = new Uri(value);
			}
		}
	}

	public RequestCachePolicy CachePolicy
	{
		get
		{
			throw GetMustImplement();
		}
		set
		{
			throw GetMustImplement();
		}
	}

	public ICredentials Credentials
	{
		get
		{
			return credentials;
		}
		set
		{
			credentials = value;
		}
	}

	public Encoding Encoding
	{
		get
		{
			return encoding;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Encoding");
			}
			encoding = value;
		}
	}

	public WebHeaderCollection Headers
	{
		get
		{
			if (headers == null)
			{
				headers = new WebHeaderCollection();
			}
			return headers;
		}
		set
		{
			headers = value;
		}
	}

	public bool IsBusy => is_busy;

	public IWebProxy Proxy
	{
		get
		{
			return proxy;
		}
		set
		{
			proxy = value;
		}
	}

	public NameValueCollection QueryString
	{
		get
		{
			if (queryString == null)
			{
				queryString = new NameValueCollection();
			}
			return queryString;
		}
		set
		{
			queryString = value;
		}
	}

	public WebHeaderCollection ResponseHeaders => responseHeaders;

	public bool UseDefaultCredentials
	{
		get
		{
			throw GetMustImplement();
		}
		set
		{
			throw GetMustImplement();
		}
	}

	public event XUnityDownloadStringCompletedEventHandler DownloadStringCompleted;

	public event XUnityUploadStringCompletedEventHandler UploadStringCompleted;

	public event XUnityDownloadDataCompletedEventHandler DownloadDataCompleted;

	public event XUnityAsyncCompletedEventHandler DownloadFileCompleted;

	public event XUnityDownloadProgressChangedEventHandler DownloadProgressChanged;

	public event XUnityOpenReadCompletedEventHandler OpenReadCompleted;

	public event XUnityOpenWriteCompletedEventHandler OpenWriteCompleted;

	public event XUnityUploadDataCompletedEventHandler UploadDataCompleted;

	public event XUnityUploadFileCompletedEventHandler UploadFileCompleted;

	public event XUnityUploadProgressChangedEventHandler UploadProgressChanged;

	public event XUnityUploadValuesCompletedEventHandler UploadValuesCompleted;

	static ConnectionTrackingWebClient()
	{
		MaxUnusedLifespan = TimeSpan.FromSeconds(50.0);
		ConnectionGroupName = Guid.NewGuid().ToString();
		ActiveConnections = new Dictionary<string, ServicePointState>();
		TouchedServicePoints = new Dictionary<string, ServicePoint>();
		hexBytes = new byte[16];
		urlEncodedCType = "application/x-www-form-urlencoded";
		int num = 0;
		int num2 = 48;
		while (num2 <= 57)
		{
			hexBytes[num] = (byte)num2;
			num2++;
			num++;
		}
		int num3 = 97;
		while (num3 <= 102)
		{
			hexBytes[num] = (byte)num3;
			num3++;
			num++;
		}
	}

	private static void UpdateActiveConnections(Uri address)
	{
		string text = address.Scheme + "://" + address.Host + ":" + address.Port;
		Uri address2 = new Uri(text);
		lock (ActiveConnections)
		{
			if (!ActiveConnections.TryGetValue(text, out var value))
			{
				if (!TouchedServicePoints.TryGetValue(text, out var value2))
				{
					value2 = ServicePointManager.FindServicePoint(address2);
					TouchedServicePoints.Add(text, value2);
				}
				value = new ServicePointState(value2);
				ActiveConnections.Add(text, value);
			}
			value.LastUse = DateTime.UtcNow;
		}
	}

	internal static void CheckServicePoints()
	{
		List<KeyValuePair<string, ServicePointState>> idleEntries = null;
		lock (ActiveConnections)
		{
			DateTime utcNow = DateTime.UtcNow;
			foreach (KeyValuePair<string, ServicePointState> activeConnection in ActiveConnections)
			{
				if (utcNow - activeConnection.Value.LastUse > MaxUnusedLifespan)
				{
					if (idleEntries == null)
					{
						idleEntries = new List<KeyValuePair<string, ServicePointState>>();
					}
					idleEntries.Add(activeConnection);
				}
			}
			if (idleEntries != null)
			{
				foreach (KeyValuePair<string, ServicePointState> item in idleEntries)
				{
					ActiveConnections.Remove(item.Key);
					XuaLogger.AutoTranslator.Debug("Closing connections to endpoint '" + item.Key + "' due to inactivity.");
				}
			}
		}
		if (idleEntries == null)
		{
			return;
		}
		ThreadPool.QueueUserWorkItem(delegate
		{
			foreach (KeyValuePair<string, ServicePointState> item2 in idleEntries)
			{
				item2.Value.ServicePoint.CloseConnectionGroup(ConnectionGroupName);
			}
		});
	}

	internal static void CloseServicePoints()
	{
		List<KeyValuePair<string, ServicePointState>> idleEntries = null;
		lock (ActiveConnections)
		{
			_ = DateTime.UtcNow;
			foreach (KeyValuePair<string, ServicePointState> activeConnection in ActiveConnections)
			{
				if (idleEntries == null)
				{
					idleEntries = new List<KeyValuePair<string, ServicePointState>>();
				}
				idleEntries.Add(activeConnection);
			}
			if (idleEntries != null)
			{
				foreach (KeyValuePair<string, ServicePointState> item in idleEntries)
				{
					ActiveConnections.Remove(item.Key);
					XuaLogger.AutoTranslator.Debug("Closing connections to endpoint '" + item.Key + "' due to force shutdown.");
				}
			}
		}
		if (idleEntries == null)
		{
			return;
		}
		ThreadPool.QueueUserWorkItem(delegate
		{
			foreach (KeyValuePair<string, ServicePointState> item2 in idleEntries)
			{
				item2.Value.ServicePoint.CloseConnectionGroup(ConnectionGroupName);
			}
		});
	}

	private void CheckBusy()
	{
		if (IsBusy)
		{
			throw new NotSupportedException("WebClient does not support conccurent I/O operations.");
		}
	}

	private void CompleteAsync()
	{
		lock (this)
		{
			is_busy = false;
		}
	}

	private Uri CreateUri(string address)
	{
		return MakeUri(address);
	}

	private Uri CreateUri(Uri address)
	{
		string query = address.Query;
		if (string.IsNullOrEmpty(query))
		{
			query = GetQueryString(add_qmark: true);
		}
		if (baseAddress == null && query == null)
		{
			return address;
		}
		if (baseAddress == null)
		{
			return new Uri(address.ToString() + query, query != null);
		}
		if (query == null)
		{
			return new Uri(baseAddress, address.ToString());
		}
		return new Uri(baseAddress, address.ToString() + query, query != null);
	}

	private string DetermineMethod(Uri address, string method, bool is_upload)
	{
		if (method != null)
		{
			return method;
		}
		if (address.Scheme == Uri.UriSchemeFtp)
		{
			if (is_upload)
			{
				return "STOR";
			}
			return "RETR";
		}
		if (is_upload)
		{
			return "POST";
		}
		return "GET";
	}

	public byte[] DownloadData(string address)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		return DownloadData(CreateUri(address));
	}

	public byte[] DownloadData(Uri address)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		try
		{
			SetBusy();
			async = false;
			return DownloadDataCore(address, null);
		}
		finally
		{
			is_busy = false;
		}
	}

	public void DownloadDataAsync(Uri address)
	{
		DownloadDataAsync(address, null);
	}

	public void DownloadDataAsync(Uri address, object userToken)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		lock (this)
		{
			SetBusy();
			async = true;
			object[] state = new object[2] { address, userToken };
			ThreadPool.QueueUserWorkItem(delegate(object obj)
			{
				object[] array = (object[])obj;
				try
				{
					byte[] result = DownloadDataCore((Uri)array[0], array[1]);
					OnDownloadDataCompleted(new XUnityDownloadDataCompletedEventArgs(result, null, cancelled: false, array[1]));
				}
				catch (ThreadInterruptedException)
				{
					OnDownloadDataCompleted(new XUnityDownloadDataCompletedEventArgs(null, null, cancelled: true, array[1]));
					throw;
				}
				catch (Exception error)
				{
					OnDownloadDataCompleted(new XUnityDownloadDataCompletedEventArgs(null, error, cancelled: false, array[1]));
				}
			}, state);
		}
	}

	private byte[] DownloadDataCore(Uri address, object userToken)
	{
		WebRequest webRequest = null;
		try
		{
			webRequest = SetupRequest(address);
			using WebResponse webResponse = GetWebResponse(webRequest);
			using Stream stream = webResponse.GetResponseStream();
			byte[] result = ReadAll(stream, (int)webResponse.ContentLength, userToken);
			stream.Close();
			webResponse.Close();
			return result;
		}
		catch (ThreadInterruptedException)
		{
			webRequest?.Abort();
			throw;
		}
		catch (WebException)
		{
			throw;
		}
		catch (Exception innerException)
		{
			throw new WebException("An error occurred performing a WebClient request.", innerException);
		}
	}

	public void DownloadFile(string address, string fileName)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		DownloadFile(CreateUri(address), fileName);
	}

	public void DownloadFile(Uri address, string fileName)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		try
		{
			SetBusy();
			async = false;
			DownloadFileCore(address, fileName, null);
		}
		catch (WebException)
		{
			throw;
		}
		catch (Exception innerException)
		{
			throw new WebException("An error occurred performing a WebClient request.", innerException);
		}
		finally
		{
			is_busy = false;
		}
	}

	public void DownloadFileAsync(Uri address, string fileName)
	{
		DownloadFileAsync(address, fileName, null);
	}

	public void DownloadFileAsync(Uri address, string fileName, object userToken)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		lock (this)
		{
			SetBusy();
			async = true;
			object[] state = new object[3] { address, fileName, userToken };
			ThreadPool.QueueUserWorkItem(delegate(object obj)
			{
				object[] array = (object[])obj;
				try
				{
					DownloadFileCore((Uri)array[0], (string)array[1], array[2]);
					OnDownloadFileCompleted(new XUnityAsyncCompletedEventArgs(null, cancelled: false, array[2]));
				}
				catch (ThreadInterruptedException)
				{
					OnDownloadFileCompleted(new XUnityAsyncCompletedEventArgs(null, cancelled: true, array[2]));
				}
				catch (Exception error)
				{
					OnDownloadFileCompleted(new XUnityAsyncCompletedEventArgs(error, cancelled: false, array[2]));
				}
			}, state);
		}
	}

	private void DownloadFileCore(Uri address, string fileName, object userToken)
	{
		WebRequest webRequest = null;
		FileStream fileStream = new FileStream(fileName, FileMode.Create);
		try
		{
			webRequest = SetupRequest(address);
			using WebResponse webResponse = GetWebResponse(webRequest);
			using Stream stream = webResponse.GetResponseStream();
			int num = (int)webResponse.ContentLength;
			int num2 = ((num > -1 && num <= 32768) ? num : 32768);
			byte[] buffer = new byte[num2];
			int num3 = 0;
			long num4 = 0L;
			while ((num3 = stream.Read(buffer, 0, num2)) != 0)
			{
				if (async)
				{
					num4 += num3;
					OnDownloadProgressChanged(new XUnityDownloadProgressChangedEventArgs(num4, webResponse.ContentLength, userToken));
				}
				fileStream.Write(buffer, 0, num3);
			}
			stream.Close();
			webResponse.Close();
		}
		catch (ThreadInterruptedException)
		{
			webRequest?.Abort();
			throw;
		}
		finally
		{
			fileStream?.Dispose();
		}
	}

	public string DownloadString(string address)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		return encoding.GetString(DownloadData(CreateUri(address)));
	}

	public string DownloadString(Uri address)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		return encoding.GetString(DownloadData(CreateUri(address)));
	}

	public void DownloadStringAsync(Uri address)
	{
		DownloadStringAsync(address, null);
	}

	public void DownloadStringAsync(Uri address, object userToken)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		lock (this)
		{
			SetBusy();
			async = true;
			object[] state = new object[2] { address, userToken };
			ThreadPool.QueueUserWorkItem(delegate(object obj)
			{
				object[] array = (object[])obj;
				try
				{
					string result = encoding.GetString(DownloadDataCore((Uri)array[0], array[1]));
					OnDownloadStringCompleted(new XUnityDownloadStringCompletedEventArgs(result, null, cancelled: false, array[1]));
				}
				catch (ThreadInterruptedException)
				{
					OnDownloadStringCompleted(new XUnityDownloadStringCompletedEventArgs(null, null, cancelled: true, array[1]));
				}
				catch (Exception error)
				{
					OnDownloadStringCompleted(new XUnityDownloadStringCompletedEventArgs(null, error, cancelled: false, array[1]));
				}
			}, state);
		}
	}

	private static Exception GetMustImplement()
	{
		return new NotImplementedException();
	}

	private string GetQueryString(bool add_qmark)
	{
		if (queryString == null || queryString.Count == 0)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (add_qmark)
		{
			stringBuilder.Append('?');
		}
		foreach (string item in queryString)
		{
			stringBuilder.AppendFormat("{0}={1}&", item, UrlEncode(queryString[item]));
		}
		if (stringBuilder.Length != 0)
		{
			stringBuilder.Length--;
		}
		if (stringBuilder.Length == 0)
		{
			return null;
		}
		return stringBuilder.ToString();
	}

	protected virtual WebRequest GetWebRequest(Uri address)
	{
		WebRequest result = WebRequest.Create(address);
		UpdateActiveConnections(address);
		return result;
	}

	protected virtual WebResponse GetWebResponse(WebRequest request)
	{
		WebResponse response = request.GetResponse();
		responseHeaders = response.Headers;
		UpdateActiveConnections(request.RequestUri);
		return response;
	}

	protected virtual WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
	{
		WebResponse webResponse = request.EndGetResponse(result);
		responseHeaders = webResponse.Headers;
		UpdateActiveConnections(request.RequestUri);
		return webResponse;
	}

	private Uri MakeUri(string path)
	{
		string text = GetQueryString(add_qmark: true);
		if (baseAddress == null && text == null)
		{
			try
			{
				return new Uri(path);
			}
			catch (ArgumentNullException)
			{
				path = Path.GetFullPath(path);
				return new Uri("file://" + path);
			}
			catch (UriFormatException)
			{
				path = Path.GetFullPath(path);
				return new Uri("file://" + path);
			}
		}
		if (baseAddress == null)
		{
			return new Uri(path + text, text != null);
		}
		if (text == null)
		{
			return new Uri(baseAddress, path);
		}
		return new Uri(baseAddress, path + text, text != null);
	}

	protected virtual void OnDownloadDataCompleted(XUnityDownloadDataCompletedEventArgs args)
	{
		CompleteAsync();
		if (this.DownloadDataCompleted != null)
		{
			this.DownloadDataCompleted(this, args);
		}
	}

	protected virtual void OnDownloadFileCompleted(XUnityAsyncCompletedEventArgs args)
	{
		CompleteAsync();
		if (this.DownloadFileCompleted != null)
		{
			this.DownloadFileCompleted(this, args);
		}
	}

	protected virtual void OnDownloadProgressChanged(XUnityDownloadProgressChangedEventArgs e)
	{
		if (this.DownloadProgressChanged != null)
		{
			this.DownloadProgressChanged(this, e);
		}
	}

	protected virtual void OnDownloadStringCompleted(XUnityDownloadStringCompletedEventArgs args)
	{
		CompleteAsync();
		if (this.DownloadStringCompleted != null)
		{
			this.DownloadStringCompleted(this, args);
		}
	}

	protected virtual void OnOpenReadCompleted(XUnityOpenReadCompletedEventArgs args)
	{
		CompleteAsync();
		if (this.OpenReadCompleted != null)
		{
			this.OpenReadCompleted(this, args);
		}
	}

	protected virtual void OnOpenWriteCompleted(XUnityOpenWriteCompletedEventArgs args)
	{
		CompleteAsync();
		if (this.OpenWriteCompleted != null)
		{
			this.OpenWriteCompleted(this, args);
		}
	}

	protected virtual void OnUploadDataCompleted(XUnityUploadDataCompletedEventArgs args)
	{
		CompleteAsync();
		if (this.UploadDataCompleted != null)
		{
			this.UploadDataCompleted(this, args);
		}
	}

	protected virtual void OnUploadFileCompleted(XUnityUploadFileCompletedEventArgs args)
	{
		CompleteAsync();
		if (this.UploadFileCompleted != null)
		{
			this.UploadFileCompleted(this, args);
		}
	}

	protected virtual void OnUploadProgressChanged(XUnityUploadProgressChangedEventArgs e)
	{
		if (this.UploadProgressChanged != null)
		{
			this.UploadProgressChanged(this, e);
		}
	}

	protected virtual void OnUploadStringCompleted(XUnityUploadStringCompletedEventArgs args)
	{
		CompleteAsync();
		if (this.UploadStringCompleted != null)
		{
			this.UploadStringCompleted(this, args);
		}
	}

	protected virtual void OnUploadValuesCompleted(XUnityUploadValuesCompletedEventArgs args)
	{
		CompleteAsync();
		if (this.UploadValuesCompleted != null)
		{
			this.UploadValuesCompleted(this, args);
		}
	}

	public Stream OpenRead(string address)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		return OpenRead(CreateUri(address));
	}

	public Stream OpenRead(Uri address)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		WebRequest webRequest = null;
		try
		{
			SetBusy();
			async = false;
			webRequest = SetupRequest(address);
			return GetWebResponse(webRequest).GetResponseStream();
		}
		catch (WebException)
		{
			throw;
		}
		catch (Exception innerException)
		{
			throw new WebException("An error occurred performing a WebClient request.", innerException);
		}
		finally
		{
			is_busy = false;
		}
	}

	public void OpenReadAsync(Uri address)
	{
		OpenReadAsync(address, null);
	}

	public void OpenReadAsync(Uri address, object userToken)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		lock (this)
		{
			SetBusy();
			async = true;
			object[] state = new object[2] { address, userToken };
			ThreadPool.QueueUserWorkItem(delegate(object obj)
			{
				object[] array = (object[])obj;
				WebRequest webRequest = null;
				try
				{
					webRequest = SetupRequest((Uri)array[0]);
					Stream responseStream = GetWebResponse(webRequest).GetResponseStream();
					OnOpenReadCompleted(new XUnityOpenReadCompletedEventArgs(responseStream, null, cancelled: false, array[1]));
				}
				catch (ThreadInterruptedException)
				{
					webRequest?.Abort();
					OnOpenReadCompleted(new XUnityOpenReadCompletedEventArgs(null, null, cancelled: true, array[1]));
				}
				catch (Exception error)
				{
					OnOpenReadCompleted(new XUnityOpenReadCompletedEventArgs(null, error, cancelled: false, array[1]));
				}
			}, state);
		}
	}

	public Stream OpenWrite(string address)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		return OpenWrite(CreateUri(address));
	}

	public Stream OpenWrite(Uri address)
	{
		return OpenWrite(address, null);
	}

	public Stream OpenWrite(string address, string method)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		return OpenWrite(CreateUri(address), method);
	}

	public Stream OpenWrite(Uri address, string method)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		try
		{
			SetBusy();
			async = false;
			return SetupRequest(address, method, is_upload: true).GetRequestStream();
		}
		catch (WebException)
		{
			throw;
		}
		catch (Exception innerException)
		{
			throw new WebException("An error occurred performing a WebClient request.", innerException);
		}
		finally
		{
			is_busy = false;
		}
	}

	public void OpenWriteAsync(Uri address)
	{
		OpenWriteAsync(address, null);
	}

	public void OpenWriteAsync(Uri address, string method)
	{
		OpenWriteAsync(address, method, null);
	}

	public void OpenWriteAsync(Uri address, string method, object userToken)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		lock (this)
		{
			SetBusy();
			async = true;
			object[] state = new object[3] { address, method, userToken };
			ThreadPool.QueueUserWorkItem(delegate(object obj)
			{
				object[] array = (object[])obj;
				WebRequest webRequest = null;
				try
				{
					webRequest = SetupRequest((Uri)array[0], (string)array[1], is_upload: true);
					Stream requestStream = webRequest.GetRequestStream();
					OnOpenWriteCompleted(new XUnityOpenWriteCompletedEventArgs(requestStream, null, cancelled: false, array[2]));
				}
				catch (ThreadInterruptedException)
				{
					webRequest?.Abort();
					OnOpenWriteCompleted(new XUnityOpenWriteCompletedEventArgs(null, null, cancelled: true, array[2]));
				}
				catch (Exception error)
				{
					OnOpenWriteCompleted(new XUnityOpenWriteCompletedEventArgs(null, error, cancelled: false, array[2]));
				}
			}, state);
		}
	}

	private byte[] ReadAll(Stream stream, int length, object userToken)
	{
		MemoryStream memoryStream = null;
		bool flag = length == -1;
		int num = ((!flag) ? length : 8192);
		if (flag)
		{
			memoryStream = new MemoryStream();
		}
		int num2 = 0;
		int num3 = 0;
		byte[] array = new byte[num];
		while ((num2 = stream.Read(array, num3, num)) != 0)
		{
			if (flag)
			{
				memoryStream.Write(array, 0, num2);
				continue;
			}
			num3 += num2;
			num -= num2;
		}
		if (flag)
		{
			return memoryStream.ToArray();
		}
		return array;
	}

	private void SetBusy()
	{
		lock (this)
		{
			CheckBusy();
			is_busy = true;
		}
	}

	private WebRequest SetupRequest(Uri uri)
	{
		WebRequest webRequest = GetWebRequest(uri);
		webRequest.ConnectionGroupName = ConnectionGroupName;
		if (Proxy != null)
		{
			webRequest.Proxy = Proxy;
		}
		webRequest.Credentials = credentials;
		if (headers != null && headers.Count != 0 && webRequest is HttpWebRequest)
		{
			HttpWebRequest httpWebRequest = (HttpWebRequest)webRequest;
			string text = headers["Expect"];
			string text2 = headers["Content-Type"];
			string text3 = headers["Accept"];
			string text4 = headers["Connection"];
			string text5 = headers["User-Agent"];
			string text6 = headers["Referer"];
			headers.Remove("Expect");
			headers.Remove("Content-Type");
			headers.Remove("Accept");
			headers.Remove("Connection");
			headers.Remove("Referer");
			headers.Remove("User-Agent");
			webRequest.Headers = headers;
			if (text != null && text.Length > 0)
			{
				httpWebRequest.Expect = text;
			}
			if (text3 != null && text3.Length > 0)
			{
				httpWebRequest.Accept = text3;
			}
			if (text2 != null && text2.Length > 0)
			{
				httpWebRequest.ContentType = text2;
			}
			if (text4 != null && text4.Length > 0)
			{
				httpWebRequest.Connection = text4;
			}
			if (text5 != null && text5.Length > 0)
			{
				httpWebRequest.UserAgent = text5;
			}
			if (text6 != null && text6.Length > 0)
			{
				httpWebRequest.Referer = text6;
			}
		}
		responseHeaders = null;
		return webRequest;
	}

	private WebRequest SetupRequest(Uri uri, string method, bool is_upload)
	{
		WebRequest webRequest = SetupRequest(uri);
		webRequest.Method = DetermineMethod(uri, method, is_upload);
		return webRequest;
	}

	public byte[] UploadData(string address, byte[] data)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		return UploadData(CreateUri(address), data);
	}

	public byte[] UploadData(Uri address, byte[] data)
	{
		return UploadData(address, null, data);
	}

	public byte[] UploadData(string address, string method, byte[] data)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		return UploadData(CreateUri(address), method, data);
	}

	public byte[] UploadData(Uri address, string method, byte[] data)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		try
		{
			SetBusy();
			async = false;
			return UploadDataCore(address, method, data, null);
		}
		catch (WebException)
		{
			throw;
		}
		catch (Exception innerException)
		{
			throw new WebException("An error occurred performing a WebClient request.", innerException);
		}
		finally
		{
			is_busy = false;
		}
	}

	public void UploadDataAsync(Uri address, byte[] data)
	{
		UploadDataAsync(address, null, data);
	}

	public void UploadDataAsync(Uri address, string method, byte[] data)
	{
		UploadDataAsync(address, method, data, null);
	}

	public void UploadDataAsync(Uri address, string method, byte[] data, object userToken)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		lock (this)
		{
			SetBusy();
			async = true;
			object[] state = new object[4] { address, method, data, userToken };
			ThreadPool.QueueUserWorkItem(delegate(object obj)
			{
				object[] array = (object[])obj;
				try
				{
					byte[] result = UploadDataCore((Uri)array[0], (string)array[1], (byte[])array[2], array[3]);
					OnUploadDataCompleted(new XUnityUploadDataCompletedEventArgs(result, null, cancelled: false, array[3]));
				}
				catch (ThreadInterruptedException)
				{
					OnUploadDataCompleted(new XUnityUploadDataCompletedEventArgs(null, null, cancelled: true, array[3]));
				}
				catch (Exception error)
				{
					OnUploadDataCompleted(new XUnityUploadDataCompletedEventArgs(null, error, cancelled: false, array[3]));
				}
			}, state);
		}
	}

	private byte[] UploadDataCore(Uri address, string method, byte[] data, object userToken)
	{
		WebRequest webRequest = SetupRequest(address, method, is_upload: true);
		try
		{
			int num = data.Length;
			webRequest.ContentLength = num;
			using (Stream stream = webRequest.GetRequestStream())
			{
				stream.Write(data, 0, num);
			}
			using WebResponse webResponse = GetWebResponse(webRequest);
			using Stream stream2 = webResponse.GetResponseStream();
			byte[] result = ReadAll(stream2, (int)webResponse.ContentLength, userToken);
			stream2.Close();
			webResponse.Close();
			return result;
		}
		catch (ThreadInterruptedException)
		{
			webRequest?.Abort();
			throw;
		}
	}

	public byte[] UploadFile(string address, string fileName)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		return UploadFile(CreateUri(address), fileName);
	}

	public byte[] UploadFile(Uri address, string fileName)
	{
		return UploadFile(address, null, fileName);
	}

	public byte[] UploadFile(string address, string method, string fileName)
	{
		return UploadFile(CreateUri(address), method, fileName);
	}

	public byte[] UploadFile(Uri address, string method, string fileName)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		try
		{
			SetBusy();
			async = false;
			return UploadFileCore(address, method, fileName, null);
		}
		catch (WebException)
		{
			throw;
		}
		catch (Exception innerException)
		{
			throw new WebException("An error occurred performing a WebClient request.", innerException);
		}
		finally
		{
			is_busy = false;
		}
	}

	public void UploadFileAsync(Uri address, string fileName)
	{
		UploadFileAsync(address, null, fileName);
	}

	public void UploadFileAsync(Uri address, string method, string fileName)
	{
		UploadFileAsync(address, method, fileName, null);
	}

	public void UploadFileAsync(Uri address, string method, string fileName, object userToken)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		lock (this)
		{
			SetBusy();
			async = true;
			object[] state = new object[4] { address, method, fileName, userToken };
			ThreadPool.QueueUserWorkItem(delegate(object obj)
			{
				object[] array = (object[])obj;
				try
				{
					byte[] result = UploadFileCore((Uri)array[0], (string)array[1], (string)array[2], array[3]);
					OnUploadFileCompleted(new XUnityUploadFileCompletedEventArgs(result, null, cancelled: false, array[3]));
				}
				catch (ThreadInterruptedException)
				{
					OnUploadFileCompleted(new XUnityUploadFileCompletedEventArgs(null, null, cancelled: true, array[3]));
				}
				catch (Exception error)
				{
					OnUploadFileCompleted(new XUnityUploadFileCompletedEventArgs(null, error, cancelled: false, array[3]));
				}
			}, state);
		}
	}

	private byte[] UploadFileCore(Uri address, string method, string fileName, object userToken)
	{
		string text = Headers["Content-Type"];
		if (text != null)
		{
			if (text.ToLower().StartsWith("multipart/"))
			{
				throw new WebException("Content-Type cannot be set to a multipart type for this request.");
			}
		}
		else
		{
			text = "application/octet-stream";
		}
		string text2 = "------------" + DateTime.Now.Ticks.ToString("x");
		Headers["Content-Type"] = $"multipart/form-data; boundary={text2}";
		Stream stream = null;
		Stream stream2 = null;
		byte[] array = null;
		fileName = Path.GetFullPath(fileName);
		WebRequest webRequest = null;
		try
		{
			stream2 = File.OpenRead(fileName);
			webRequest = SetupRequest(address, method, is_upload: true);
			stream = webRequest.GetRequestStream();
			byte[] bytes = Encoding.ASCII.GetBytes("--" + text2 + "\r\n");
			stream.Write(bytes, 0, bytes.Length);
			string s = $"Content-Disposition: form-data; name=\"file\"; filename=\"{Path.GetFileName(fileName)}\"\r\nContent-Type: {text}\r\n\r\n";
			byte[] bytes2 = Encoding.UTF8.GetBytes(s);
			stream.Write(bytes2, 0, bytes2.Length);
			byte[] buffer = new byte[4096];
			int count;
			while ((count = stream2.Read(buffer, 0, 4096)) != 0)
			{
				stream.Write(buffer, 0, count);
			}
			stream.WriteByte(13);
			stream.WriteByte(10);
			stream.Write(bytes, 0, bytes.Length);
			stream.Close();
			stream = null;
			using WebResponse webResponse = GetWebResponse(webRequest);
			using Stream stream3 = webResponse.GetResponseStream();
			array = ReadAll(stream3, (int)webResponse.ContentLength, userToken);
			stream3.Close();
			webResponse.Close();
			return array;
		}
		catch (ThreadInterruptedException)
		{
			webRequest?.Abort();
			throw;
		}
		finally
		{
			stream2?.Close();
			stream?.Close();
		}
	}

	public string UploadString(string address, string data)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		byte[] bytes = UploadData(address, encoding.GetBytes(data));
		return encoding.GetString(bytes);
	}

	public string UploadString(Uri address, string data)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		byte[] bytes = UploadData(address, encoding.GetBytes(data));
		return encoding.GetString(bytes);
	}

	public string UploadString(string address, string method, string data)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		byte[] bytes = UploadData(address, method, encoding.GetBytes(data));
		return encoding.GetString(bytes);
	}

	public string UploadString(Uri address, string method, string data)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		byte[] bytes = UploadData(address, method, encoding.GetBytes(data));
		return encoding.GetString(bytes);
	}

	public void UploadStringAsync(Uri address, string data)
	{
		UploadStringAsync(address, null, data);
	}

	public void UploadStringAsync(Uri address, string method, string data)
	{
		UploadStringAsync(address, method, data, null);
	}

	public void UploadStringAsync(Uri address, string method, string data, object userToken)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		lock (this)
		{
			CheckBusy();
			async = true;
			object[] state = new object[4] { address, method, data, userToken };
			ThreadPool.QueueUserWorkItem(delegate(object obj)
			{
				object[] array = (object[])obj;
				try
				{
					string result = UploadString((Uri)array[0], (string)array[1], (string)array[2]);
					OnUploadStringCompleted(new XUnityUploadStringCompletedEventArgs(result, null, cancelled: false, array[3]));
				}
				catch (ThreadInterruptedException)
				{
					OnUploadStringCompleted(new XUnityUploadStringCompletedEventArgs(null, null, cancelled: true, array[3]));
				}
				catch (Exception error)
				{
					OnUploadStringCompleted(new XUnityUploadStringCompletedEventArgs(null, error, cancelled: false, array[3]));
				}
			}, state);
		}
	}

	public byte[] UploadValues(string address, NameValueCollection data)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		return UploadValues(CreateUri(address), data);
	}

	public byte[] UploadValues(Uri address, NameValueCollection data)
	{
		return UploadValues(address, null, data);
	}

	public byte[] UploadValues(string address, string method, NameValueCollection data)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		return UploadValues(CreateUri(address), method, data);
	}

	public byte[] UploadValues(Uri address, string method, NameValueCollection data)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		try
		{
			SetBusy();
			async = false;
			return UploadValuesCore(address, method, data, null);
		}
		catch (WebException)
		{
			throw;
		}
		catch (Exception innerException)
		{
			throw new WebException("An error occurred performing a WebClient request.", innerException);
		}
		finally
		{
			is_busy = false;
		}
	}

	public void UploadValuesAsync(Uri address, NameValueCollection values)
	{
		UploadValuesAsync(address, null, values);
	}

	public void UploadValuesAsync(Uri address, string method, NameValueCollection values)
	{
		UploadValuesAsync(address, method, values, null);
	}

	public void UploadValuesAsync(Uri address, string method, NameValueCollection values, object userToken)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		lock (this)
		{
			CheckBusy();
			async = true;
			object[] state = new object[4] { address, method, values, userToken };
			ThreadPool.QueueUserWorkItem(delegate(object obj)
			{
				object[] array = (object[])obj;
				try
				{
					byte[] result = UploadValuesCore((Uri)array[0], (string)array[1], (NameValueCollection)array[2], array[3]);
					OnUploadValuesCompleted(new XUnityUploadValuesCompletedEventArgs(result, null, cancelled: false, array[3]));
				}
				catch (ThreadInterruptedException)
				{
					OnUploadValuesCompleted(new XUnityUploadValuesCompletedEventArgs(null, null, cancelled: true, array[3]));
				}
				catch (Exception error)
				{
					OnUploadValuesCompleted(new XUnityUploadValuesCompletedEventArgs(null, error, cancelled: false, array[3]));
				}
			}, state);
		}
	}

	private byte[] UploadValuesCore(Uri uri, string method, NameValueCollection data, object userToken)
	{
		string text = Headers["Content-Type"];
		if (text != null && string.Compare(text, urlEncodedCType, ignoreCase: true, CultureInfo.InvariantCulture) != 0)
		{
			throw new WebException("Content-Type header cannot be changed from its default value for this request.");
		}
		Headers["Content-Type"] = urlEncodedCType;
		WebRequest webRequest = SetupRequest(uri, method, is_upload: true);
		try
		{
			MemoryStream memoryStream = new MemoryStream();
			foreach (string datum in data)
			{
				byte[] bytes = Encoding.UTF8.GetBytes(datum);
				UrlEncodeAndWrite(memoryStream, bytes);
				memoryStream.WriteByte(61);
				bytes = Encoding.UTF8.GetBytes(data[datum]);
				UrlEncodeAndWrite(memoryStream, bytes);
				memoryStream.WriteByte(38);
			}
			int num = (int)memoryStream.Length;
			if (num > 0)
			{
				memoryStream.SetLength(--num);
			}
			byte[] buffer = memoryStream.GetBuffer();
			webRequest.ContentLength = num;
			using (Stream stream = webRequest.GetRequestStream())
			{
				stream.Write(buffer, 0, num);
			}
			memoryStream.Close();
			using WebResponse webResponse = GetWebResponse(webRequest);
			using Stream stream2 = webResponse.GetResponseStream();
			byte[] result = ReadAll(stream2, (int)webResponse.ContentLength, userToken);
			stream2.Close();
			webResponse.Close();
			return result;
		}
		catch (ThreadInterruptedException)
		{
			webRequest.Abort();
			throw;
		}
	}

	private string UrlEncode(string str)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int length = str.Length;
		for (int i = 0; i < length; i++)
		{
			char c = str[i];
			if (c == ' ')
			{
				stringBuilder.Append('+');
			}
			else if ((c < '0' && c != '-' && c != '.') || (c < 'A' && c > '9') || (c > 'Z' && c < 'a' && c != '_') || c > 'z')
			{
				stringBuilder.Append('%');
				int num = (int)c >> 4;
				stringBuilder.Append((char)hexBytes[num]);
				num = c & 0xF;
				stringBuilder.Append((char)hexBytes[num]);
			}
			else
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	private static void UrlEncodeAndWrite(Stream stream, byte[] bytes)
	{
		if (bytes == null)
		{
			return;
		}
		int num = bytes.Length;
		if (num == 0)
		{
			return;
		}
		for (int i = 0; i < num; i++)
		{
			char c = (char)bytes[i];
			if (c == ' ')
			{
				stream.WriteByte(43);
			}
			else if ((c < '0' && c != '-' && c != '.') || (c < 'A' && c > '9') || (c > 'Z' && c < 'a' && c != '_') || c > 'z')
			{
				stream.WriteByte(37);
				int num2 = (int)c >> 4;
				stream.WriteByte(hexBytes[num2]);
				num2 = c & 0xF;
				stream.WriteByte(hexBytes[num2]);
			}
			else
			{
				stream.WriteByte((byte)c);
			}
		}
	}
}
