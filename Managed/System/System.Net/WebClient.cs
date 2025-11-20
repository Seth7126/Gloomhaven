using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net.Cache;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net;

/// <summary>Provides common methods for sending data to and receiving data from a resource identified by a URI.</summary>
public class WebClient : Component
{
	private sealed class ProgressData
	{
		internal long BytesSent;

		internal long TotalBytesToSend = -1L;

		internal long BytesReceived;

		internal long TotalBytesToReceive = -1L;

		internal bool HasUploadPhase;

		internal void Reset()
		{
			BytesSent = 0L;
			TotalBytesToSend = -1L;
			BytesReceived = 0L;
			TotalBytesToReceive = -1L;
			HasUploadPhase = false;
		}
	}

	private sealed class WebClientWriteStream : DelegatingStream
	{
		private readonly WebRequest _request;

		private readonly WebClient _webClient;

		public WebClientWriteStream(Stream stream, WebRequest request, WebClient webClient)
			: base(stream)
		{
			_request = request;
			_webClient = webClient;
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					_webClient.GetWebResponse(_request).Dispose();
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}
	}

	private const int DefaultCopyBufferLength = 8192;

	private const int DefaultDownloadBufferLength = 65536;

	private const string DefaultUploadFileContentType = "application/octet-stream";

	private const string UploadFileContentType = "multipart/form-data";

	private const string UploadValuesContentType = "application/x-www-form-urlencoded";

	private Uri _baseAddress;

	private ICredentials _credentials;

	private WebHeaderCollection _headers;

	private NameValueCollection _requestParameters;

	private WebResponse _webResponse;

	private WebRequest _webRequest;

	private Encoding _encoding = Encoding.Default;

	private string _method;

	private long _contentLength = -1L;

	private bool _initWebClientAsync;

	private bool _canceled;

	private ProgressData _progress;

	private IWebProxy _proxy;

	private bool _proxySet;

	private int _callNesting;

	private AsyncOperation _asyncOp;

	private SendOrPostCallback _downloadDataOperationCompleted;

	private SendOrPostCallback _openReadOperationCompleted;

	private SendOrPostCallback _openWriteOperationCompleted;

	private SendOrPostCallback _downloadStringOperationCompleted;

	private SendOrPostCallback _downloadFileOperationCompleted;

	private SendOrPostCallback _uploadStringOperationCompleted;

	private SendOrPostCallback _uploadDataOperationCompleted;

	private SendOrPostCallback _uploadFileOperationCompleted;

	private SendOrPostCallback _uploadValuesOperationCompleted;

	private SendOrPostCallback _reportDownloadProgressChanged;

	private SendOrPostCallback _reportUploadProgressChanged;

	private static readonly char[] s_parseContentTypeSeparators = new char[3] { ';', '=', ' ' };

	private static readonly Encoding[] s_knownEncodings = new Encoding[4]
	{
		Encoding.UTF8,
		Encoding.UTF32,
		Encoding.Unicode,
		Encoding.BigEndianUnicode
	};

	/// <summary>Gets and sets the <see cref="T:System.Text.Encoding" /> used to upload and download strings.</summary>
	/// <returns>A <see cref="T:System.Text.Encoding" /> that is used to encode strings. The default value of this property is the encoding returned by <see cref="P:System.Text.Encoding.Default" />.</returns>
	public Encoding Encoding
	{
		get
		{
			return _encoding;
		}
		set
		{
			ThrowIfNull(value, "Encoding");
			_encoding = value;
		}
	}

	/// <summary>Gets or sets the base URI for requests made by a <see cref="T:System.Net.WebClient" />.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the base URI for requests made by a <see cref="T:System.Net.WebClient" /> or <see cref="F:System.String.Empty" /> if no base address has been specified.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <see cref="P:System.Net.WebClient.BaseAddress" /> is set to an invalid URI. The inner exception may contain information that will help you locate the error.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public string BaseAddress
	{
		get
		{
			if (!(_baseAddress != null))
			{
				return string.Empty;
			}
			return _baseAddress.ToString();
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				_baseAddress = null;
				return;
			}
			try
			{
				_baseAddress = new Uri(value);
			}
			catch (UriFormatException innerException)
			{
				throw new ArgumentException("The specified value is not a valid base address.", "value", innerException);
			}
		}
	}

	/// <summary>Gets or sets the network credentials that are sent to the host and used to authenticate the request.</summary>
	/// <returns>An <see cref="T:System.Net.ICredentials" /> containing the authentication credentials for the request. The default is null.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public ICredentials Credentials
	{
		get
		{
			return _credentials;
		}
		set
		{
			_credentials = value;
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that controls whether the <see cref="P:System.Net.CredentialCache.DefaultCredentials" /> are sent with requests.</summary>
	/// <returns>true if the default credentials are used; otherwise false. The default value is false.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="USERNAME" />
	/// </PermissionSet>
	public bool UseDefaultCredentials
	{
		get
		{
			return _credentials == CredentialCache.DefaultCredentials;
		}
		set
		{
			_credentials = (value ? CredentialCache.DefaultCredentials : null);
		}
	}

	/// <summary>Gets or sets a collection of header name/value pairs associated with the request.</summary>
	/// <returns>A <see cref="T:System.Net.WebHeaderCollection" /> containing header name/value pairs associated with this request.</returns>
	public WebHeaderCollection Headers
	{
		get
		{
			return _headers ?? (_headers = new WebHeaderCollection());
		}
		set
		{
			_headers = value;
		}
	}

	/// <summary>Gets or sets a collection of query name/value pairs associated with the request.</summary>
	/// <returns>A <see cref="T:System.Collections.Specialized.NameValueCollection" /> that contains query name/value pairs associated with the request. If no pairs are associated with the request, the value is an empty <see cref="T:System.Collections.Specialized.NameValueCollection" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public NameValueCollection QueryString
	{
		get
		{
			return _requestParameters ?? (_requestParameters = new NameValueCollection());
		}
		set
		{
			_requestParameters = value;
		}
	}

	/// <summary>Gets a collection of header name/value pairs associated with the response.</summary>
	/// <returns>A <see cref="T:System.Net.WebHeaderCollection" /> containing header name/value pairs associated with the response, or null if no response has been received.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public WebHeaderCollection ResponseHeaders => _webResponse?.Headers;

	/// <summary>Gets or sets the proxy used by this <see cref="T:System.Net.WebClient" /> object.</summary>
	/// <returns>An <see cref="T:System.Net.IWebProxy" /> instance used to send requests.</returns>
	/// <exception cref="T:System.ArgumentNullException">
	///   <see cref="P:System.Net.WebClient.Proxy" /> is set to null. </exception>
	public IWebProxy Proxy
	{
		get
		{
			if (!_proxySet)
			{
				return WebRequest.DefaultWebProxy;
			}
			return _proxy;
		}
		set
		{
			_proxy = value;
			_proxySet = true;
		}
	}

	/// <summary>Gets or sets the application's cache policy for any resources obtained by this WebClient instance using <see cref="T:System.Net.WebRequest" /> objects.</summary>
	/// <returns>A <see cref="T:System.Net.Cache.RequestCachePolicy" /> object that represents the application's caching requirements.</returns>
	public RequestCachePolicy CachePolicy { get; set; }

	/// <summary>Gets whether a Web request is in progress.</summary>
	/// <returns>true if the Web request is still in progress; otherwise false.</returns>
	public bool IsBusy => _asyncOp != null;

	/// <summary>Gets or sets a value that indicates whether to buffer the data read from the Internet resource for a <see cref="T:System.Net.WebClient" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true to enable buffering of the data received from the Internet resource; false to disable buffering. The default is true.</returns>
	[Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool AllowReadStreamBuffering { get; set; }

	/// <summary>Gets or sets a value that indicates whether to buffer the data written to the Internet resource for a <see cref="T:System.Net.WebClient" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true to enable buffering of the data written to the Internet resource; false to disable buffering. The default is true.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
	public bool AllowWriteStreamBuffering { get; set; }

	/// <summary>Occurs when an asynchronous resource-download operation completes.</summary>
	public event DownloadStringCompletedEventHandler DownloadStringCompleted;

	/// <summary>Occurs when an asynchronous data download operation completes.</summary>
	public event DownloadDataCompletedEventHandler DownloadDataCompleted;

	/// <summary>Occurs when an asynchronous file download operation completes.</summary>
	public event AsyncCompletedEventHandler DownloadFileCompleted;

	/// <summary>Occurs when an asynchronous string-upload operation completes.</summary>
	public event UploadStringCompletedEventHandler UploadStringCompleted;

	/// <summary>Occurs when an asynchronous data-upload operation completes.</summary>
	public event UploadDataCompletedEventHandler UploadDataCompleted;

	/// <summary>Occurs when an asynchronous file-upload operation completes.</summary>
	public event UploadFileCompletedEventHandler UploadFileCompleted;

	/// <summary>Occurs when an asynchronous upload of a name/value collection completes.</summary>
	public event UploadValuesCompletedEventHandler UploadValuesCompleted;

	/// <summary>Occurs when an asynchronous operation to open a stream containing a resource completes.</summary>
	public event OpenReadCompletedEventHandler OpenReadCompleted;

	/// <summary>Occurs when an asynchronous operation to open a stream to write data to a resource completes.</summary>
	public event OpenWriteCompletedEventHandler OpenWriteCompleted;

	/// <summary>Occurs when an asynchronous download operation successfully transfers some or all of the data.</summary>
	public event DownloadProgressChangedEventHandler DownloadProgressChanged;

	/// <summary>Occurs when an asynchronous upload operation successfully transfers some or all of the data.</summary>
	public event UploadProgressChangedEventHandler UploadProgressChanged;

	/// <summary>Occurs when an asynchronous operation to write data to a resource using a write stream is closed.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
	public event WriteStreamClosedEventHandler WriteStreamClosed
	{
		add
		{
		}
		remove
		{
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.WebClient" /> class.</summary>
	public WebClient()
	{
		if (GetType() == typeof(WebClient))
		{
			GC.SuppressFinalize(this);
		}
	}

	/// <summary>Raises the <see cref="E:System.Net.WebClient.DownloadStringCompleted" /> event.</summary>
	/// <param name="e">A <see cref="T:System.Net.DownloadStringCompletedEventArgs" /> object containing event data.</param>
	protected virtual void OnDownloadStringCompleted(DownloadStringCompletedEventArgs e)
	{
		this.DownloadStringCompleted?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.Net.WebClient.DownloadDataCompleted" /> event.</summary>
	/// <param name="e">A <see cref="T:System.Net.DownloadDataCompletedEventArgs" /> object that contains event data.</param>
	protected virtual void OnDownloadDataCompleted(DownloadDataCompletedEventArgs e)
	{
		this.DownloadDataCompleted?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.Net.WebClient.DownloadFileCompleted" /> event.</summary>
	/// <param name="e">An <see cref="T:System.ComponentModel.AsyncCompletedEventArgs" /> object containing event data.</param>
	protected virtual void OnDownloadFileCompleted(AsyncCompletedEventArgs e)
	{
		this.DownloadFileCompleted?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.Net.WebClient.DownloadProgressChanged" /> event.</summary>
	/// <param name="e">A <see cref="T:System.Net.DownloadProgressChangedEventArgs" /> object containing event data.</param>
	protected virtual void OnDownloadProgressChanged(DownloadProgressChangedEventArgs e)
	{
		this.DownloadProgressChanged?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.Net.WebClient.UploadStringCompleted" /> event.</summary>
	/// <param name="e">An <see cref="T:System.Net.UploadStringCompletedEventArgs" />  object containing event data.</param>
	protected virtual void OnUploadStringCompleted(UploadStringCompletedEventArgs e)
	{
		this.UploadStringCompleted?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.Net.WebClient.UploadDataCompleted" /> event.</summary>
	/// <param name="e">A <see cref="T:System.Net.UploadDataCompletedEventArgs" />  object containing event data.</param>
	protected virtual void OnUploadDataCompleted(UploadDataCompletedEventArgs e)
	{
		this.UploadDataCompleted?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.Net.WebClient.UploadFileCompleted" /> event.</summary>
	/// <param name="e">An <see cref="T:System.Net.UploadFileCompletedEventArgs" /> object containing event data.</param>
	protected virtual void OnUploadFileCompleted(UploadFileCompletedEventArgs e)
	{
		this.UploadFileCompleted?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.Net.WebClient.UploadValuesCompleted" /> event.</summary>
	/// <param name="e">A <see cref="T:System.Net.UploadValuesCompletedEventArgs" />  object containing event data.</param>
	protected virtual void OnUploadValuesCompleted(UploadValuesCompletedEventArgs e)
	{
		this.UploadValuesCompleted?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.Net.WebClient.UploadProgressChanged" /> event.</summary>
	/// <param name="e">An <see cref="T:System.Net.UploadProgressChangedEventArgs" /> object containing event data.</param>
	protected virtual void OnUploadProgressChanged(UploadProgressChangedEventArgs e)
	{
		this.UploadProgressChanged?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.Net.WebClient.OpenReadCompleted" /> event.</summary>
	/// <param name="e">A <see cref="T:System.Net.OpenReadCompletedEventArgs" />  object containing event data.</param>
	protected virtual void OnOpenReadCompleted(OpenReadCompletedEventArgs e)
	{
		this.OpenReadCompleted?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.Net.WebClient.OpenWriteCompleted" /> event.</summary>
	/// <param name="e">A <see cref="T:System.Net.OpenWriteCompletedEventArgs" /> object containing event data.</param>
	protected virtual void OnOpenWriteCompleted(OpenWriteCompletedEventArgs e)
	{
		this.OpenWriteCompleted?.Invoke(this, e);
	}

	private void StartOperation()
	{
		if (Interlocked.Increment(ref _callNesting) > 1)
		{
			EndOperation();
			throw new NotSupportedException("WebClient does not support concurrent I/O operations.");
		}
		_contentLength = -1L;
		_webResponse = null;
		_webRequest = null;
		_method = null;
		_canceled = false;
		_progress?.Reset();
	}

	private AsyncOperation StartAsyncOperation(object userToken)
	{
		if (!_initWebClientAsync)
		{
			_openReadOperationCompleted = delegate(object arg)
			{
				OnOpenReadCompleted((OpenReadCompletedEventArgs)arg);
			};
			_openWriteOperationCompleted = delegate(object arg)
			{
				OnOpenWriteCompleted((OpenWriteCompletedEventArgs)arg);
			};
			_downloadStringOperationCompleted = delegate(object arg)
			{
				OnDownloadStringCompleted((DownloadStringCompletedEventArgs)arg);
			};
			_downloadDataOperationCompleted = delegate(object arg)
			{
				OnDownloadDataCompleted((DownloadDataCompletedEventArgs)arg);
			};
			_downloadFileOperationCompleted = delegate(object arg)
			{
				OnDownloadFileCompleted((AsyncCompletedEventArgs)arg);
			};
			_uploadStringOperationCompleted = delegate(object arg)
			{
				OnUploadStringCompleted((UploadStringCompletedEventArgs)arg);
			};
			_uploadDataOperationCompleted = delegate(object arg)
			{
				OnUploadDataCompleted((UploadDataCompletedEventArgs)arg);
			};
			_uploadFileOperationCompleted = delegate(object arg)
			{
				OnUploadFileCompleted((UploadFileCompletedEventArgs)arg);
			};
			_uploadValuesOperationCompleted = delegate(object arg)
			{
				OnUploadValuesCompleted((UploadValuesCompletedEventArgs)arg);
			};
			_reportDownloadProgressChanged = delegate(object arg)
			{
				OnDownloadProgressChanged((DownloadProgressChangedEventArgs)arg);
			};
			_reportUploadProgressChanged = delegate(object arg)
			{
				OnUploadProgressChanged((UploadProgressChangedEventArgs)arg);
			};
			_progress = new ProgressData();
			_initWebClientAsync = true;
		}
		AsyncOperation asyncOperation = AsyncOperationManager.CreateOperation(userToken);
		StartOperation();
		_asyncOp = asyncOperation;
		return asyncOperation;
	}

	private void EndOperation()
	{
		Interlocked.Decrement(ref _callNesting);
	}

	/// <summary>Returns a <see cref="T:System.Net.WebRequest" /> object for the specified resource.</summary>
	/// <returns>A new <see cref="T:System.Net.WebRequest" /> object for the specified resource.</returns>
	/// <param name="address">A <see cref="T:System.Uri" /> that identifies the resource to request.</param>
	protected virtual WebRequest GetWebRequest(Uri address)
	{
		WebRequest webRequest = WebRequest.Create(address);
		CopyHeadersTo(webRequest);
		if (Credentials != null)
		{
			webRequest.Credentials = Credentials;
		}
		if (_method != null)
		{
			webRequest.Method = _method;
		}
		if (_contentLength != -1)
		{
			webRequest.ContentLength = _contentLength;
		}
		if (_proxySet)
		{
			webRequest.Proxy = _proxy;
		}
		if (CachePolicy != null)
		{
			webRequest.CachePolicy = CachePolicy;
		}
		return webRequest;
	}

	/// <summary>Returns the <see cref="T:System.Net.WebResponse" /> for the specified <see cref="T:System.Net.WebRequest" />.</summary>
	/// <returns>A <see cref="T:System.Net.WebResponse" /> containing the response for the specified <see cref="T:System.Net.WebRequest" />.</returns>
	/// <param name="request">A <see cref="T:System.Net.WebRequest" /> that is used to obtain the response. </param>
	protected virtual WebResponse GetWebResponse(WebRequest request)
	{
		return _webResponse = request.GetResponse();
	}

	/// <summary>Returns the <see cref="T:System.Net.WebResponse" /> for the specified <see cref="T:System.Net.WebRequest" /> using the specified <see cref="T:System.IAsyncResult" />.</summary>
	/// <returns>A <see cref="T:System.Net.WebResponse" /> containing the response for the specified <see cref="T:System.Net.WebRequest" />.</returns>
	/// <param name="request">A <see cref="T:System.Net.WebRequest" /> that is used to obtain the response.</param>
	/// <param name="result">An <see cref="T:System.IAsyncResult" /> object obtained from a previous call to <see cref="M:System.Net.WebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" /> .</param>
	protected virtual WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
	{
		return _webResponse = request.EndGetResponse(result);
	}

	private async Task<WebResponse> GetWebResponseTaskAsync(WebRequest request)
	{
		BeginEndAwaitableAdapter beginEndAwaitableAdapter = new BeginEndAwaitableAdapter();
		request.BeginGetResponse(BeginEndAwaitableAdapter.Callback, beginEndAwaitableAdapter);
		return GetWebResponse(request, await beginEndAwaitableAdapter);
	}

	/// <summary>Downloads the resource as a <see cref="T:System.Byte" /> array from the URI specified.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the downloaded resource.</returns>
	/// <param name="address">The URI from which to download data. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while downloading data. </exception>
	/// <exception cref="T:System.NotSupportedException">The method has been called simultaneously on multiple threads.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public byte[] DownloadData(string address)
	{
		return DownloadData(GetUri(address));
	}

	/// <summary>Downloads the resource as a <see cref="T:System.Byte" /> array from the URI specified.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the downloaded resource.</returns>
	/// <param name="address">The URI represented by the <see cref="T:System.Uri" />  object, from which to download data.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	public byte[] DownloadData(Uri address)
	{
		ThrowIfNull(address, "address");
		StartOperation();
		try
		{
			WebRequest request;
			return DownloadDataInternal(address, out request);
		}
		finally
		{
			EndOperation();
		}
	}

	private byte[] DownloadDataInternal(Uri address, out WebRequest request)
	{
		request = null;
		try
		{
			request = (_webRequest = GetWebRequest(GetUri(address)));
			return DownloadBits(request, new ChunkedMemoryStream());
		}
		catch (Exception ex) when (!(ex is OutOfMemoryException))
		{
			AbortRequest(request);
			if (ex is WebException || ex is SecurityException)
			{
				throw;
			}
			throw new WebException("An exception occurred during a WebClient request.", ex);
		}
	}

	/// <summary>Downloads the resource with the specified URI to a local file.</summary>
	/// <param name="address">The URI from which to download data. </param>
	/// <param name="fileName">The name of the local file that is to receive the data. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- <paramref name="filename" /> is null or <see cref="F:System.String.Empty" />.-or-The file does not exist.-or- An error occurred while downloading data. </exception>
	/// <exception cref="T:System.NotSupportedException">The method has been called simultaneously on multiple threads.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void DownloadFile(string address, string fileName)
	{
		DownloadFile(GetUri(address), fileName);
	}

	/// <summary>Downloads the resource with the specified URI to a local file.</summary>
	/// <param name="address">The URI specified as a <see cref="T:System.String" />, from which to download data. </param>
	/// <param name="fileName">The name of the local file that is to receive the data. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- <paramref name="filename" /> is null or <see cref="F:System.String.Empty" />.-or- The file does not exist. -or- An error occurred while downloading data. </exception>
	/// <exception cref="T:System.NotSupportedException">The method has been called simultaneously on multiple threads.</exception>
	public void DownloadFile(Uri address, string fileName)
	{
		ThrowIfNull(address, "address");
		ThrowIfNull(fileName, "fileName");
		WebRequest request = null;
		FileStream fileStream = null;
		bool flag = false;
		StartOperation();
		try
		{
			fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
			request = (_webRequest = GetWebRequest(GetUri(address)));
			DownloadBits(request, fileStream);
			flag = true;
		}
		catch (Exception ex) when (!(ex is OutOfMemoryException))
		{
			AbortRequest(request);
			if (ex is WebException || ex is SecurityException)
			{
				throw;
			}
			throw new WebException("An exception occurred during a WebClient request.", ex);
		}
		finally
		{
			if (fileStream != null)
			{
				fileStream.Close();
				if (!flag)
				{
					File.Delete(fileName);
				}
			}
			EndOperation();
		}
	}

	/// <summary>Opens a readable stream for the data downloaded from a resource with the URI specified as a <see cref="T:System.String" />.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> used to read data from a resource.</returns>
	/// <param name="address">The URI specified as a <see cref="T:System.String" /> from which to download data. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, <paramref name="address" /> is invalid.-or- An error occurred while downloading data. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public Stream OpenRead(string address)
	{
		return OpenRead(GetUri(address));
	}

	/// <summary>Opens a readable stream for the data downloaded from a resource with the URI specified as a <see cref="T:System.Uri" /></summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> used to read data from a resource.</returns>
	/// <param name="address">The URI specified as a <see cref="T:System.Uri" /> from which to download data. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, <paramref name="address" /> is invalid.-or- An error occurred while downloading data. </exception>
	public Stream OpenRead(Uri address)
	{
		ThrowIfNull(address, "address");
		WebRequest request = null;
		StartOperation();
		try
		{
			request = (_webRequest = GetWebRequest(GetUri(address)));
			return (_webResponse = GetWebResponse(request)).GetResponseStream();
		}
		catch (Exception ex) when (!(ex is OutOfMemoryException))
		{
			AbortRequest(request);
			if (ex is WebException || ex is SecurityException)
			{
				throw;
			}
			throw new WebException("An exception occurred during a WebClient request.", ex);
		}
		finally
		{
			EndOperation();
		}
	}

	/// <summary>Opens a stream for writing data to the specified resource.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> used to write data to the resource.</returns>
	/// <param name="address">The URI of the resource to receive the data. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public Stream OpenWrite(string address)
	{
		return OpenWrite(GetUri(address), null);
	}

	/// <summary>Opens a stream for writing data to the specified resource.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> used to write data to the resource.</returns>
	/// <param name="address">The URI of the resource to receive the data.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream. </exception>
	public Stream OpenWrite(Uri address)
	{
		return OpenWrite(address, null);
	}

	/// <summary>Opens a stream for writing data to the specified resource, using the specified method.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> used to write data to the resource.</returns>
	/// <param name="address">The URI of the resource to receive the data. </param>
	/// <param name="method">The method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public Stream OpenWrite(string address, string method)
	{
		return OpenWrite(GetUri(address), method);
	}

	/// <summary>Opens a stream for writing data to the specified resource, by using the specified method.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> used to write data to the resource.</returns>
	/// <param name="address">The URI of the resource to receive the data.</param>
	/// <param name="method">The method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream. </exception>
	public Stream OpenWrite(Uri address, string method)
	{
		ThrowIfNull(address, "address");
		if (method == null)
		{
			method = MapToDefaultMethod(address);
		}
		WebRequest webRequest = null;
		StartOperation();
		try
		{
			_method = method;
			webRequest = (_webRequest = GetWebRequest(GetUri(address)));
			return new WebClientWriteStream(webRequest.GetRequestStream(), webRequest, this);
		}
		catch (Exception ex) when (!(ex is OutOfMemoryException))
		{
			AbortRequest(webRequest);
			if (ex is WebException || ex is SecurityException)
			{
				throw;
			}
			throw new WebException("An exception occurred during a WebClient request.", ex);
		}
		finally
		{
			EndOperation();
		}
	}

	/// <summary>Uploads a data buffer to a resource identified by a URI.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the body of the response from the resource.</returns>
	/// <param name="address">The URI of the resource to receive the data. </param>
	/// <param name="data">The data buffer to send to the resource. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- <paramref name="data" /> is null. -or-An error occurred while sending the data.-or- There was no response from the server hosting the resource. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public byte[] UploadData(string address, byte[] data)
	{
		return UploadData(GetUri(address), null, data);
	}

	/// <summary>Uploads a data buffer to a resource identified by a URI.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the body of the response from the resource.</returns>
	/// <param name="address">The URI of the resource to receive the data. </param>
	/// <param name="data">The data buffer to send to the resource. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- <paramref name="data" /> is null. -or-An error occurred while sending the data.-or- There was no response from the server hosting the resource. </exception>
	public byte[] UploadData(Uri address, byte[] data)
	{
		return UploadData(address, null, data);
	}

	/// <summary>Uploads a data buffer to the specified resource, using the specified method.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the body of the response from the resource.</returns>
	/// <param name="address">The URI of the resource to receive the data. </param>
	/// <param name="method">The HTTP method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The data buffer to send to the resource. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- <paramref name="data" /> is null.-or- An error occurred while uploading the data.-or- There was no response from the server hosting the resource. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public byte[] UploadData(string address, string method, byte[] data)
	{
		return UploadData(GetUri(address), method, data);
	}

	/// <summary>Uploads a data buffer to the specified resource, using the specified method.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the body of the response from the resource.</returns>
	/// <param name="address">The URI of the resource to receive the data. </param>
	/// <param name="method">The HTTP method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The data buffer to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- <paramref name="data" /> is null.-or- An error occurred while uploading the data.-or- There was no response from the server hosting the resource. </exception>
	public byte[] UploadData(Uri address, string method, byte[] data)
	{
		ThrowIfNull(address, "address");
		ThrowIfNull(data, "data");
		if (method == null)
		{
			method = MapToDefaultMethod(address);
		}
		StartOperation();
		try
		{
			WebRequest request;
			return UploadDataInternal(address, method, data, out request);
		}
		finally
		{
			EndOperation();
		}
	}

	private byte[] UploadDataInternal(Uri address, string method, byte[] data, out WebRequest request)
	{
		request = null;
		try
		{
			_method = method;
			_contentLength = data.Length;
			request = (_webRequest = GetWebRequest(GetUri(address)));
			return UploadBits(request, null, data, 0, null, null);
		}
		catch (Exception ex) when (!(ex is OutOfMemoryException))
		{
			AbortRequest(request);
			if (ex is WebException || ex is SecurityException)
			{
				throw;
			}
			throw new WebException("An exception occurred during a WebClient request.", ex);
		}
	}

	private void OpenFileInternal(bool needsHeaderAndBoundary, string fileName, ref FileStream fs, ref byte[] buffer, ref byte[] formHeaderBytes, ref byte[] boundaryBytes)
	{
		fileName = Path.GetFullPath(fileName);
		WebHeaderCollection headers = Headers;
		string text = headers["Content-Type"];
		if (text == null)
		{
			text = "application/octet-stream";
		}
		else if (text.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase))
		{
			throw new WebException("The Content-Type header cannot be set to a multipart type for this request.");
		}
		fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
		int num = 8192;
		_contentLength = -1L;
		if (string.Equals(_method, "POST", StringComparison.Ordinal))
		{
			if (needsHeaderAndBoundary)
			{
				string text2 = "---------------------" + DateTime.Now.Ticks.ToString("x", NumberFormatInfo.InvariantInfo);
				headers["Content-Type"] = "multipart/form-data; boundary=" + text2;
				string s = "--" + text2 + "\r\nContent-Disposition: form-data; name=\"file\"; filename=\"" + Path.GetFileName(fileName) + "\"\r\nContent-Type: " + text + "\r\n\r\n";
				formHeaderBytes = Encoding.UTF8.GetBytes(s);
				boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + text2 + "--\r\n");
			}
			else
			{
				formHeaderBytes = Array.Empty<byte>();
				boundaryBytes = Array.Empty<byte>();
			}
			if (fs.CanSeek)
			{
				_contentLength = fs.Length + formHeaderBytes.Length + boundaryBytes.Length;
				num = (int)Math.Min(8192L, fs.Length);
			}
		}
		else
		{
			headers["Content-Type"] = text;
			formHeaderBytes = null;
			boundaryBytes = null;
			if (fs.CanSeek)
			{
				_contentLength = fs.Length;
				num = (int)Math.Min(8192L, fs.Length);
			}
		}
		buffer = new byte[num];
	}

	/// <summary>Uploads the specified local file to a resource with the specified URI.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the body of the response from the resource.</returns>
	/// <param name="address">The URI of the resource to receive the file. For example, ftp://localhost/samplefile.txt.</param>
	/// <param name="fileName">The file to send to the resource. For example, "samplefile.txt".</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- <paramref name="fileName" /> is null, is <see cref="F:System.String.Empty" />, contains invalid characters, or does not exist.-or- An error occurred while uploading the file.-or- There was no response from the server hosting the resource.-or- The Content-type header begins with multipart. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public byte[] UploadFile(string address, string fileName)
	{
		return UploadFile(GetUri(address), fileName);
	}

	/// <summary>Uploads the specified local file to a resource with the specified URI.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the body of the response from the resource.</returns>
	/// <param name="address">The URI of the resource to receive the file. For example, ftp://localhost/samplefile.txt.</param>
	/// <param name="fileName">The file to send to the resource. For example, "samplefile.txt".</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- <paramref name="fileName" /> is null, is <see cref="F:System.String.Empty" />, contains invalid characters, or does not exist.-or- An error occurred while uploading the file.-or- There was no response from the server hosting the resource.-or- The Content-type header begins with multipart. </exception>
	public byte[] UploadFile(Uri address, string fileName)
	{
		return UploadFile(address, null, fileName);
	}

	/// <summary>Uploads the specified local file to the specified resource, using the specified method.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the body of the response from the resource.</returns>
	/// <param name="address">The URI of the resource to receive the file.</param>
	/// <param name="method">The method used to send the file to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="fileName">The file to send to the resource. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- <paramref name="fileName" /> is null, is <see cref="F:System.String.Empty" />, contains invalid characters, or does not exist.-or- An error occurred while uploading the file.-or- There was no response from the server hosting the resource.-or- The Content-type header begins with multipart. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public byte[] UploadFile(string address, string method, string fileName)
	{
		return UploadFile(GetUri(address), method, fileName);
	}

	/// <summary>Uploads the specified local file to the specified resource, using the specified method.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the body of the response from the resource.</returns>
	/// <param name="address">The URI of the resource to receive the file.</param>
	/// <param name="method">The method used to send the file to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="fileName">The file to send to the resource. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- <paramref name="fileName" /> is null, is <see cref="F:System.String.Empty" />, contains invalid characters, or does not exist.-or- An error occurred while uploading the file.-or- There was no response from the server hosting the resource.-or- The Content-type header begins with multipart. </exception>
	public byte[] UploadFile(Uri address, string method, string fileName)
	{
		ThrowIfNull(address, "address");
		ThrowIfNull(fileName, "fileName");
		if (method == null)
		{
			method = MapToDefaultMethod(address);
		}
		FileStream fs = null;
		WebRequest request = null;
		StartOperation();
		try
		{
			_method = method;
			byte[] formHeaderBytes = null;
			byte[] boundaryBytes = null;
			byte[] buffer = null;
			Uri uri = GetUri(address);
			bool needsHeaderAndBoundary = uri.Scheme != Uri.UriSchemeFile;
			OpenFileInternal(needsHeaderAndBoundary, fileName, ref fs, ref buffer, ref formHeaderBytes, ref boundaryBytes);
			request = (_webRequest = GetWebRequest(uri));
			return UploadBits(request, fs, buffer, 0, formHeaderBytes, boundaryBytes);
		}
		catch (Exception ex)
		{
			fs?.Close();
			if (ex is OutOfMemoryException)
			{
				throw;
			}
			AbortRequest(request);
			if (ex is WebException || ex is SecurityException)
			{
				throw;
			}
			throw new WebException("An exception occurred during a WebClient request.", ex);
		}
		finally
		{
			EndOperation();
		}
	}

	private byte[] GetValuesToUpload(NameValueCollection data)
	{
		WebHeaderCollection headers = Headers;
		string text = headers["Content-Type"];
		if (text != null && !string.Equals(text, "application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
		{
			throw new WebException("The Content-Type header cannot be changed from its default value for this request.");
		}
		headers["Content-Type"] = "application/x-www-form-urlencoded";
		string value = string.Empty;
		StringBuilder stringBuilder = new StringBuilder();
		string[] allKeys = data.AllKeys;
		foreach (string text2 in allKeys)
		{
			stringBuilder.Append(value);
			stringBuilder.Append(UrlEncode(text2));
			stringBuilder.Append('=');
			stringBuilder.Append(UrlEncode(data[text2]));
			value = "&";
		}
		byte[] bytes = Encoding.ASCII.GetBytes(stringBuilder.ToString());
		_contentLength = bytes.Length;
		return bytes;
	}

	/// <summary>Uploads the specified name/value collection to the resource identified by the specified URI.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the body of the response from the resource.</returns>
	/// <param name="address">The URI of the resource to receive the collection. </param>
	/// <param name="data">The <see cref="T:System.Collections.Specialized.NameValueCollection" /> to send to the resource. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- <paramref name="data" /> is null.-or- There was no response from the server hosting the resource.-or- An error occurred while opening the stream.-or- The Content-type header is not null or "application/x-www-form-urlencoded". </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public byte[] UploadValues(string address, NameValueCollection data)
	{
		return UploadValues(GetUri(address), null, data);
	}

	/// <summary>Uploads the specified name/value collection to the resource identified by the specified URI.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the body of the response from the resource.</returns>
	/// <param name="address">The URI of the resource to receive the collection. </param>
	/// <param name="data">The <see cref="T:System.Collections.Specialized.NameValueCollection" /> to send to the resource. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- <paramref name="data" /> is null.-or- There was no response from the server hosting the resource.-or- An error occurred while opening the stream.-or- The Content-type header is not null or "application/x-www-form-urlencoded". </exception>
	public byte[] UploadValues(Uri address, NameValueCollection data)
	{
		return UploadValues(address, null, data);
	}

	/// <summary>Uploads the specified name/value collection to the resource identified by the specified URI, using the specified method.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the body of the response from the resource.</returns>
	/// <param name="address">The URI of the resource to receive the collection. </param>
	/// <param name="method">The HTTP method used to send the file to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The <see cref="T:System.Collections.Specialized.NameValueCollection" /> to send to the resource. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- <paramref name="data" /> is null.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource.-or- The Content-type header value is not null and is not application/x-www-form-urlencoded. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public byte[] UploadValues(string address, string method, NameValueCollection data)
	{
		return UploadValues(GetUri(address), method, data);
	}

	/// <summary>Uploads the specified name/value collection to the resource identified by the specified URI, using the specified method.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the body of the response from the resource.</returns>
	/// <param name="address">The URI of the resource to receive the collection. </param>
	/// <param name="method">The HTTP method used to send the file to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The <see cref="T:System.Collections.Specialized.NameValueCollection" /> to send to the resource. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- <paramref name="data" /> is null.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource.-or- The Content-type header value is not null and is not application/x-www-form-urlencoded. </exception>
	public byte[] UploadValues(Uri address, string method, NameValueCollection data)
	{
		ThrowIfNull(address, "address");
		ThrowIfNull(data, "data");
		if (method == null)
		{
			method = MapToDefaultMethod(address);
		}
		WebRequest request = null;
		StartOperation();
		try
		{
			byte[] valuesToUpload = GetValuesToUpload(data);
			_method = method;
			request = (_webRequest = GetWebRequest(GetUri(address)));
			return UploadBits(request, null, valuesToUpload, 0, null, null);
		}
		catch (Exception ex) when (!(ex is OutOfMemoryException))
		{
			AbortRequest(request);
			if (ex is WebException || ex is SecurityException)
			{
				throw;
			}
			throw new WebException("An exception occurred during a WebClient request.", ex);
		}
		finally
		{
			EndOperation();
		}
	}

	/// <summary>Uploads the specified string to the specified resource, using the POST method.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the response sent by the server.</returns>
	/// <param name="address">The URI of the resource to receive the string. For Http resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page. </param>
	/// <param name="data">The string to be uploaded.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- There was no response from the server hosting the resource.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public string UploadString(string address, string data)
	{
		return UploadString(GetUri(address), null, data);
	}

	/// <summary>Uploads the specified string to the specified resource, using the POST method.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the response sent by the server.</returns>
	/// <param name="address">The URI of the resource to receive the string. For Http resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page. </param>
	/// <param name="data">The string to be uploaded.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- There was no response from the server hosting the resource.</exception>
	public string UploadString(Uri address, string data)
	{
		return UploadString(address, null, data);
	}

	/// <summary>Uploads the specified string to the specified resource, using the specified method.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the response sent by the server.</returns>
	/// <param name="address">The URI of the resource to receive the string. This URI must identify a resource that can accept a request sent with the <paramref name="method" /> method. </param>
	/// <param name="method">The HTTP method used to send the string to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The string to be uploaded.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- There was no response from the server hosting the resource.-or-<paramref name="method" /> cannot be used to send content.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public string UploadString(string address, string method, string data)
	{
		return UploadString(GetUri(address), method, data);
	}

	/// <summary>Uploads the specified string to the specified resource, using the specified method.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the response sent by the server.</returns>
	/// <param name="address">The URI of the resource to receive the string. This URI must identify a resource that can accept a request sent with the <paramref name="method" /> method. </param>
	/// <param name="method">The HTTP method used to send the string to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The string to be uploaded.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- There was no response from the server hosting the resource.-or-<paramref name="method" /> cannot be used to send content.</exception>
	public string UploadString(Uri address, string method, string data)
	{
		ThrowIfNull(address, "address");
		ThrowIfNull(data, "data");
		if (method == null)
		{
			method = MapToDefaultMethod(address);
		}
		StartOperation();
		try
		{
			byte[] bytes = Encoding.GetBytes(data);
			WebRequest request;
			byte[] data2 = UploadDataInternal(address, method, bytes, out request);
			return GetStringUsingEncoding(request, data2);
		}
		finally
		{
			EndOperation();
		}
	}

	/// <summary>Downloads the requested resource as a <see cref="T:System.String" />. The resource to download is specified as a <see cref="T:System.String" /> containing the URI.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the requested resource.</returns>
	/// <param name="address">A <see cref="T:System.String" /> containing the URI to download.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while downloading the resource. </exception>
	/// <exception cref="T:System.NotSupportedException">The method has been called simultaneously on multiple threads.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public string DownloadString(string address)
	{
		return DownloadString(GetUri(address));
	}

	/// <summary>Downloads the requested resource as a <see cref="T:System.String" />. The resource to download is specified as a <see cref="T:System.Uri" />.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the requested resource.</returns>
	/// <param name="address">A <see cref="T:System.Uri" /> object containing the URI to download.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while downloading the resource. </exception>
	/// <exception cref="T:System.NotSupportedException">The method has been called simultaneously on multiple threads.</exception>
	public string DownloadString(Uri address)
	{
		ThrowIfNull(address, "address");
		StartOperation();
		try
		{
			WebRequest request;
			byte[] data = DownloadDataInternal(address, out request);
			return GetStringUsingEncoding(request, data);
		}
		finally
		{
			EndOperation();
		}
	}

	private static void AbortRequest(WebRequest request)
	{
		try
		{
			request?.Abort();
		}
		catch (Exception ex) when (!(ex is OutOfMemoryException))
		{
		}
	}

	private void CopyHeadersTo(WebRequest request)
	{
		if (_headers != null && request is HttpWebRequest httpWebRequest)
		{
			string text = _headers["Accept"];
			string text2 = _headers["Connection"];
			string text3 = _headers["Content-Type"];
			string text4 = _headers["Expect"];
			string text5 = _headers["Referer"];
			string text6 = _headers["User-Agent"];
			string text7 = _headers["Host"];
			_headers.Remove("Accept");
			_headers.Remove("Connection");
			_headers.Remove("Content-Type");
			_headers.Remove("Expect");
			_headers.Remove("Referer");
			_headers.Remove("User-Agent");
			_headers.Remove("Host");
			request.Headers = _headers;
			if (!string.IsNullOrEmpty(text))
			{
				httpWebRequest.Accept = text;
			}
			if (!string.IsNullOrEmpty(text2))
			{
				httpWebRequest.Connection = text2;
			}
			if (!string.IsNullOrEmpty(text3))
			{
				httpWebRequest.ContentType = text3;
			}
			if (!string.IsNullOrEmpty(text4))
			{
				httpWebRequest.Expect = text4;
			}
			if (!string.IsNullOrEmpty(text5))
			{
				httpWebRequest.Referer = text5;
			}
			if (!string.IsNullOrEmpty(text6))
			{
				httpWebRequest.UserAgent = text6;
			}
			if (!string.IsNullOrEmpty(text7))
			{
				httpWebRequest.Host = text7;
			}
		}
	}

	private Uri GetUri(string address)
	{
		ThrowIfNull(address, "address");
		Uri result;
		if (_baseAddress != null)
		{
			if (!Uri.TryCreate(_baseAddress, address, out result))
			{
				return new Uri(Path.GetFullPath(address));
			}
		}
		else if (!Uri.TryCreate(address, UriKind.Absolute, out result))
		{
			return new Uri(Path.GetFullPath(address));
		}
		return GetUri(result);
	}

	private Uri GetUri(Uri address)
	{
		ThrowIfNull(address, "address");
		Uri result = address;
		if (!address.IsAbsoluteUri && _baseAddress != null && !Uri.TryCreate(_baseAddress, address, out result))
		{
			return address;
		}
		if (string.IsNullOrEmpty(result.Query) && _requestParameters != null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string value = string.Empty;
			for (int i = 0; i < _requestParameters.Count; i++)
			{
				stringBuilder.Append(value).Append(_requestParameters.AllKeys[i]).Append('=')
					.Append(_requestParameters[i]);
				value = "&";
			}
			result = new UriBuilder(result)
			{
				Query = stringBuilder.ToString()
			}.Uri;
		}
		return result;
	}

	private byte[] DownloadBits(WebRequest request, Stream writeStream)
	{
		try
		{
			WebResponse webResponse = (_webResponse = GetWebResponse(request));
			long contentLength = webResponse.ContentLength;
			byte[] array = new byte[(contentLength == -1 || contentLength > 65536) ? 65536 : contentLength];
			if (writeStream is ChunkedMemoryStream)
			{
				if (contentLength > int.MaxValue)
				{
					throw new WebException("The message length limit was exceeded", WebExceptionStatus.MessageLengthLimitExceeded);
				}
				writeStream.SetLength(array.Length);
			}
			using (Stream stream = webResponse.GetResponseStream())
			{
				if (stream != null)
				{
					int count;
					while ((count = stream.Read(array, 0, array.Length)) != 0)
					{
						writeStream.Write(array, 0, count);
					}
				}
			}
			return (writeStream as ChunkedMemoryStream)?.ToArray();
		}
		catch (Exception ex) when (!(ex is OutOfMemoryException))
		{
			writeStream?.Close();
			AbortRequest(request);
			if (ex is WebException || ex is SecurityException)
			{
				throw;
			}
			throw new WebException("An exception occurred during a WebClient request.", ex);
		}
	}

	private async void DownloadBitsAsync(WebRequest request, Stream writeStream, AsyncOperation asyncOp, Action<byte[], Exception, AsyncOperation> completionDelegate)
	{
		Exception exception = null;
		try
		{
			WebResponse webResponse = (_webResponse = await GetWebResponseTaskAsync(request).ConfigureAwait(continueOnCapturedContext: false));
			long contentLength = webResponse.ContentLength;
			byte[] copyBuffer = new byte[(contentLength == -1 || contentLength > 65536) ? 65536 : contentLength];
			if (writeStream is ChunkedMemoryStream)
			{
				if (contentLength > int.MaxValue)
				{
					throw new WebException("The message length limit was exceeded", WebExceptionStatus.MessageLengthLimitExceeded);
				}
				writeStream.SetLength(copyBuffer.Length);
			}
			if (contentLength >= 0)
			{
				_progress.TotalBytesToReceive = contentLength;
			}
			using (writeStream)
			{
				using Stream readStream = webResponse.GetResponseStream();
				if (readStream != null)
				{
					while (true)
					{
						int num = await readStream.ReadAsync(new Memory<byte>(copyBuffer)).ConfigureAwait(continueOnCapturedContext: false);
						if (num == 0)
						{
							break;
						}
						_progress.BytesReceived += num;
						if (_progress.BytesReceived != _progress.TotalBytesToReceive)
						{
							PostProgressChanged(asyncOp, _progress);
						}
						await writeStream.WriteAsync(new ReadOnlyMemory<byte>(copyBuffer, 0, num)).ConfigureAwait(continueOnCapturedContext: false);
					}
				}
				if (_progress.TotalBytesToReceive < 0)
				{
					_progress.TotalBytesToReceive = _progress.BytesReceived;
				}
				PostProgressChanged(asyncOp, _progress);
			}
			completionDelegate((writeStream as ChunkedMemoryStream)?.ToArray(), null, asyncOp);
		}
		catch (Exception ex) when (!(ex is OutOfMemoryException))
		{
			exception = GetExceptionToPropagate(ex);
			AbortRequest(request);
			writeStream?.Close();
		}
		finally
		{
			if (exception != null)
			{
				completionDelegate(null, exception, asyncOp);
			}
		}
	}

	private byte[] UploadBits(WebRequest request, Stream readStream, byte[] buffer, int chunkSize, byte[] header, byte[] footer)
	{
		try
		{
			if (request.RequestUri.Scheme == Uri.UriSchemeFile)
			{
				header = (footer = null);
			}
			using (Stream stream = request.GetRequestStream())
			{
				if (header != null)
				{
					stream.Write(header, 0, header.Length);
				}
				if (readStream != null)
				{
					using (readStream)
					{
						while (true)
						{
							int num = readStream.Read(buffer, 0, buffer.Length);
							if (num > 0)
							{
								stream.Write(buffer, 0, num);
								continue;
							}
							break;
						}
					}
				}
				else
				{
					int num2;
					for (int i = 0; i < buffer.Length; i += num2)
					{
						num2 = buffer.Length - i;
						if (chunkSize != 0 && num2 > chunkSize)
						{
							num2 = chunkSize;
						}
						stream.Write(buffer, i, num2);
					}
				}
				if (footer != null)
				{
					stream.Write(footer, 0, footer.Length);
				}
			}
			return DownloadBits(request, new ChunkedMemoryStream());
		}
		catch (Exception ex) when (!(ex is OutOfMemoryException))
		{
			AbortRequest(request);
			if (ex is WebException || ex is SecurityException)
			{
				throw;
			}
			throw new WebException("An exception occurred during a WebClient request.", ex);
		}
	}

	private async void UploadBitsAsync(WebRequest request, Stream readStream, byte[] buffer, int chunkSize, byte[] header, byte[] footer, AsyncOperation asyncOp, Action<byte[], Exception, AsyncOperation> completionDelegate)
	{
		_progress.HasUploadPhase = true;
		Exception exception = null;
		try
		{
			if (request.RequestUri.Scheme == Uri.UriSchemeFile)
			{
				byte[] array;
				footer = (array = null);
				header = array;
			}
			using (Stream writeStream = await request.GetRequestStreamAsync().ConfigureAwait(continueOnCapturedContext: false))
			{
				if (header != null)
				{
					await writeStream.WriteAsync(new ReadOnlyMemory<byte>(header)).ConfigureAwait(continueOnCapturedContext: false);
					_progress.BytesSent += header.Length;
					PostProgressChanged(asyncOp, _progress);
				}
				if (readStream != null)
				{
					using (readStream)
					{
						while (true)
						{
							int bytesRead = await readStream.ReadAsync(new Memory<byte>(buffer)).ConfigureAwait(continueOnCapturedContext: false);
							if (bytesRead <= 0)
							{
								break;
							}
							await writeStream.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, bytesRead)).ConfigureAwait(continueOnCapturedContext: false);
							_progress.BytesSent += bytesRead;
							PostProgressChanged(asyncOp, _progress);
						}
					}
				}
				else
				{
					int bytesRead = 0;
					while (bytesRead < buffer.Length)
					{
						int toWrite = buffer.Length - bytesRead;
						if (chunkSize != 0 && toWrite > chunkSize)
						{
							toWrite = chunkSize;
						}
						await writeStream.WriteAsync(new ReadOnlyMemory<byte>(buffer, bytesRead, toWrite)).ConfigureAwait(continueOnCapturedContext: false);
						bytesRead += toWrite;
						_progress.BytesSent += toWrite;
						PostProgressChanged(asyncOp, _progress);
					}
				}
				if (footer != null)
				{
					await writeStream.WriteAsync(new ReadOnlyMemory<byte>(footer)).ConfigureAwait(continueOnCapturedContext: false);
					_progress.BytesSent += footer.Length;
					PostProgressChanged(asyncOp, _progress);
				}
			}
			DownloadBitsAsync(request, new ChunkedMemoryStream(), asyncOp, completionDelegate);
		}
		catch (Exception ex) when (!(ex is OutOfMemoryException))
		{
			exception = GetExceptionToPropagate(ex);
			AbortRequest(request);
		}
		finally
		{
			if (exception != null)
			{
				completionDelegate(null, exception, asyncOp);
			}
		}
	}

	private static bool ByteArrayHasPrefix(byte[] prefix, byte[] byteArray)
	{
		if (prefix == null || byteArray == null || prefix.Length > byteArray.Length)
		{
			return false;
		}
		for (int i = 0; i < prefix.Length; i++)
		{
			if (prefix[i] != byteArray[i])
			{
				return false;
			}
		}
		return true;
	}

	private string GetStringUsingEncoding(WebRequest request, byte[] data)
	{
		Encoding encoding = null;
		int num = -1;
		string text;
		try
		{
			text = request.ContentType;
		}
		catch (Exception ex) when (ex is NotImplementedException || ex is NotSupportedException)
		{
			text = null;
		}
		if (text != null)
		{
			text = text.ToLower(CultureInfo.InvariantCulture);
			string[] array = text.Split(s_parseContentTypeSeparators);
			bool flag = false;
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				if (text2 == "charset")
				{
					flag = true;
				}
				else if (flag)
				{
					try
					{
						encoding = Encoding.GetEncoding(text2);
					}
					catch (ArgumentException)
					{
						break;
					}
				}
			}
		}
		if (encoding == null)
		{
			Encoding[] array3 = s_knownEncodings;
			for (int j = 0; j < array3.Length; j++)
			{
				byte[] preamble = array3[j].GetPreamble();
				if (ByteArrayHasPrefix(preamble, data))
				{
					encoding = array3[j];
					num = preamble.Length;
					break;
				}
			}
		}
		if (encoding == null)
		{
			encoding = Encoding;
		}
		if (num == -1)
		{
			byte[] preamble2 = encoding.GetPreamble();
			num = (ByteArrayHasPrefix(preamble2, data) ? preamble2.Length : 0);
		}
		return encoding.GetString(data, num, data.Length - num);
	}

	private string MapToDefaultMethod(Uri address)
	{
		if (!string.Equals(((!address.IsAbsoluteUri && _baseAddress != null) ? new Uri(_baseAddress, address) : address).Scheme, Uri.UriSchemeFtp, StringComparison.Ordinal))
		{
			return "POST";
		}
		return "STOR";
	}

	private static string UrlEncode(string str)
	{
		if (str == null)
		{
			return null;
		}
		byte[] bytes = Encoding.UTF8.GetBytes(str);
		return Encoding.ASCII.GetString(UrlEncodeBytesToBytesInternal(bytes, 0, bytes.Length, alwaysCreateReturnValue: false));
	}

	private static byte[] UrlEncodeBytesToBytesInternal(byte[] bytes, int offset, int count, bool alwaysCreateReturnValue)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < count; i++)
		{
			char c = (char)bytes[offset + i];
			if (c == ' ')
			{
				num++;
			}
			else if (!IsSafe(c))
			{
				num2++;
			}
		}
		if (!alwaysCreateReturnValue && num == 0 && num2 == 0)
		{
			return bytes;
		}
		byte[] array = new byte[count + num2 * 2];
		int num3 = 0;
		for (int j = 0; j < count; j++)
		{
			byte b = bytes[offset + j];
			char c2 = (char)b;
			if (IsSafe(c2))
			{
				array[num3++] = b;
				continue;
			}
			if (c2 == ' ')
			{
				array[num3++] = 43;
				continue;
			}
			array[num3++] = 37;
			array[num3++] = (byte)IntToHex((b >> 4) & 0xF);
			array[num3++] = (byte)IntToHex(b & 0xF);
		}
		return array;
	}

	private static char IntToHex(int n)
	{
		if (n <= 9)
		{
			return (char)(n + 48);
		}
		return (char)(n - 10 + 97);
	}

	private static bool IsSafe(char ch)
	{
		if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9'))
		{
			return true;
		}
		switch (ch)
		{
		case '!':
		case '\'':
		case '(':
		case ')':
		case '*':
		case '-':
		case '.':
		case '_':
			return true;
		default:
			return false;
		}
	}

	private void InvokeOperationCompleted(AsyncOperation asyncOp, SendOrPostCallback callback, AsyncCompletedEventArgs eventArgs)
	{
		if (Interlocked.CompareExchange(ref _asyncOp, null, asyncOp) == asyncOp)
		{
			EndOperation();
			asyncOp.PostOperationCompleted(callback, eventArgs);
		}
	}

	/// <summary>Opens a readable stream containing the specified resource. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to retrieve.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and address is invalid.-or- An error occurred while downloading the resource. -or- An error occurred while opening the stream.</exception>
	public void OpenReadAsync(Uri address)
	{
		OpenReadAsync(address, null);
	}

	/// <summary>Opens a readable stream containing the specified resource. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to retrieve.</param>
	/// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and address is invalid.-or- An error occurred while downloading the resource. -or- An error occurred while opening the stream.</exception>
	public void OpenReadAsync(Uri address, object userToken)
	{
		ThrowIfNull(address, "address");
		AsyncOperation asyncOp = StartAsyncOperation(userToken);
		try
		{
			WebRequest request = (_webRequest = GetWebRequest(GetUri(address)));
			request.BeginGetResponse(delegate(IAsyncResult iar)
			{
				Stream result = null;
				Exception exception = null;
				try
				{
					result = (_webResponse = GetWebResponse(request, iar)).GetResponseStream();
				}
				catch (Exception ex2) when (!(ex2 is OutOfMemoryException))
				{
					exception = GetExceptionToPropagate(ex2);
				}
				InvokeOperationCompleted(asyncOp, _openReadOperationCompleted, new OpenReadCompletedEventArgs(result, exception, _canceled, asyncOp.UserSuppliedState));
			}, null);
		}
		catch (Exception ex) when (!(ex is OutOfMemoryException))
		{
			InvokeOperationCompleted(asyncOp, _openReadOperationCompleted, new OpenReadCompletedEventArgs(null, GetExceptionToPropagate(ex), _canceled, asyncOp.UserSuppliedState));
		}
	}

	/// <summary>Opens a stream for writing data to the specified resource. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to receive the data. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	public void OpenWriteAsync(Uri address)
	{
		OpenWriteAsync(address, null, null);
	}

	/// <summary>Opens a stream for writing data to the specified resource. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to receive the data. </param>
	/// <param name="method">The method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	public void OpenWriteAsync(Uri address, string method)
	{
		OpenWriteAsync(address, method, null);
	}

	/// <summary>Opens a stream for writing data to the specified resource, using the specified method. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to receive the data.</param>
	/// <param name="method">The method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream. </exception>
	public void OpenWriteAsync(Uri address, string method, object userToken)
	{
		ThrowIfNull(address, "address");
		if (method == null)
		{
			method = MapToDefaultMethod(address);
		}
		AsyncOperation asyncOp = StartAsyncOperation(userToken);
		try
		{
			_method = method;
			WebRequest request = (_webRequest = GetWebRequest(GetUri(address)));
			request.BeginGetRequestStream(delegate(IAsyncResult iar)
			{
				WebClientWriteStream result = null;
				Exception exception = null;
				try
				{
					result = new WebClientWriteStream(request.EndGetRequestStream(iar), request, this);
				}
				catch (Exception ex2) when (!(ex2 is OutOfMemoryException))
				{
					exception = GetExceptionToPropagate(ex2);
				}
				InvokeOperationCompleted(asyncOp, _openWriteOperationCompleted, new OpenWriteCompletedEventArgs(result, exception, _canceled, asyncOp.UserSuppliedState));
			}, null);
		}
		catch (Exception ex) when (!(ex is OutOfMemoryException))
		{
			OpenWriteCompletedEventArgs eventArgs = new OpenWriteCompletedEventArgs(null, GetExceptionToPropagate(ex), _canceled, asyncOp.UserSuppliedState);
			InvokeOperationCompleted(asyncOp, _openWriteOperationCompleted, eventArgs);
		}
	}

	private void DownloadStringAsyncCallback(byte[] returnBytes, Exception exception, object state)
	{
		AsyncOperation asyncOperation = (AsyncOperation)state;
		string result = null;
		try
		{
			if (returnBytes != null)
			{
				result = GetStringUsingEncoding(_webRequest, returnBytes);
			}
		}
		catch (Exception ex) when (!(ex is OutOfMemoryException))
		{
			exception = GetExceptionToPropagate(ex);
		}
		DownloadStringCompletedEventArgs eventArgs = new DownloadStringCompletedEventArgs(result, exception, _canceled, asyncOperation.UserSuppliedState);
		InvokeOperationCompleted(asyncOperation, _downloadStringOperationCompleted, eventArgs);
	}

	/// <summary>Downloads the resource specified as a <see cref="T:System.Uri" />. This method does not block the calling thread.</summary>
	/// <param name="address">A <see cref="T:System.Uri" /> containing the URI to download.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while downloading the resource. </exception>
	public void DownloadStringAsync(Uri address)
	{
		DownloadStringAsync(address, null);
	}

	/// <summary>Downloads the specified string to the specified resource. This method does not block the calling thread.</summary>
	/// <param name="address">A <see cref="T:System.Uri" /> containing the URI to download.</param>
	/// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while downloading the resource. </exception>
	public void DownloadStringAsync(Uri address, object userToken)
	{
		ThrowIfNull(address, "address");
		AsyncOperation asyncOperation = StartAsyncOperation(userToken);
		try
		{
			DownloadBitsAsync(_webRequest = GetWebRequest(GetUri(address)), new ChunkedMemoryStream(), asyncOperation, DownloadStringAsyncCallback);
		}
		catch (Exception ex) when (!(ex is OutOfMemoryException))
		{
			DownloadStringAsyncCallback(null, GetExceptionToPropagate(ex), asyncOperation);
		}
	}

	private void DownloadDataAsyncCallback(byte[] returnBytes, Exception exception, object state)
	{
		AsyncOperation asyncOperation = (AsyncOperation)state;
		DownloadDataCompletedEventArgs eventArgs = new DownloadDataCompletedEventArgs(returnBytes, exception, _canceled, asyncOperation.UserSuppliedState);
		InvokeOperationCompleted(asyncOperation, _downloadDataOperationCompleted, eventArgs);
	}

	/// <summary>Downloads the resource as a <see cref="T:System.Byte" /> array from the URI specified as an asynchronous operation. </summary>
	/// <param name="address">A <see cref="T:System.Uri" /> containing the URI to download.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while downloading the resource. </exception>
	public void DownloadDataAsync(Uri address)
	{
		DownloadDataAsync(address, null);
	}

	/// <summary>Downloads the resource as a <see cref="T:System.Byte" /> array from the URI specified as an asynchronous operation. </summary>
	/// <param name="address">A <see cref="T:System.Uri" /> containing the URI to download.</param>
	/// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while downloading the resource. </exception>
	public void DownloadDataAsync(Uri address, object userToken)
	{
		ThrowIfNull(address, "address");
		AsyncOperation asyncOperation = StartAsyncOperation(userToken);
		try
		{
			DownloadBitsAsync(_webRequest = GetWebRequest(GetUri(address)), new ChunkedMemoryStream(), asyncOperation, DownloadDataAsyncCallback);
		}
		catch (Exception ex) when (!(ex is OutOfMemoryException))
		{
			DownloadDataAsyncCallback(null, GetExceptionToPropagate(ex), asyncOperation);
		}
	}

	private void DownloadFileAsyncCallback(byte[] returnBytes, Exception exception, object state)
	{
		AsyncOperation asyncOperation = (AsyncOperation)state;
		AsyncCompletedEventArgs eventArgs = new AsyncCompletedEventArgs(exception, _canceled, asyncOperation.UserSuppliedState);
		InvokeOperationCompleted(asyncOperation, _downloadFileOperationCompleted, eventArgs);
	}

	/// <summary>Downloads, to a local file, the resource with the specified URI. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to download. </param>
	/// <param name="fileName">The name of the file to be placed on the local computer. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while downloading the resource. </exception>
	/// <exception cref="T:System.InvalidOperationException">The local file specified by <paramref name="fileName" /> is in use by another thread.</exception>
	public void DownloadFileAsync(Uri address, string fileName)
	{
		DownloadFileAsync(address, fileName, null);
	}

	/// <summary>Downloads, to a local file, the resource with the specified URI. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to download. </param>
	/// <param name="fileName">The name of the file to be placed on the local computer. </param>
	/// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while downloading the resource. </exception>
	/// <exception cref="T:System.InvalidOperationException">The local file specified by <paramref name="fileName" /> is in use by another thread.</exception>
	public void DownloadFileAsync(Uri address, string fileName, object userToken)
	{
		ThrowIfNull(address, "address");
		ThrowIfNull(fileName, "fileName");
		FileStream fileStream = null;
		AsyncOperation asyncOperation = StartAsyncOperation(userToken);
		try
		{
			fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
			DownloadBitsAsync(_webRequest = GetWebRequest(GetUri(address)), fileStream, asyncOperation, DownloadFileAsyncCallback);
		}
		catch (Exception ex) when (!(ex is OutOfMemoryException))
		{
			fileStream?.Close();
			DownloadFileAsyncCallback(null, GetExceptionToPropagate(ex), asyncOperation);
		}
	}

	/// <summary>Uploads the specified string to the specified resource. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to receive the string. For HTTP resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page. </param>
	/// <param name="data">The string to be uploaded.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> is null.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- There was no response from the server hosting the resource.</exception>
	public void UploadStringAsync(Uri address, string data)
	{
		UploadStringAsync(address, null, data, null);
	}

	/// <summary>Uploads the specified string to the specified resource. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to receive the string. For HTTP resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page.</param>
	/// <param name="method">The HTTP method used to send the file to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The string to be uploaded.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or-<paramref name="method" /> cannot be used to send content.-or- There was no response from the server hosting the resource.</exception>
	public void UploadStringAsync(Uri address, string method, string data)
	{
		UploadStringAsync(address, method, data, null);
	}

	/// <summary>Uploads the specified string to the specified resource. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to receive the string. For HTTP resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page.</param>
	/// <param name="method">The HTTP method used to send the file to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The string to be uploaded.</param>
	/// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or-<paramref name="method" /> cannot be used to send content.-or- There was no response from the server hosting the resource.</exception>
	public void UploadStringAsync(Uri address, string method, string data, object userToken)
	{
		ThrowIfNull(address, "address");
		ThrowIfNull(data, "data");
		if (method == null)
		{
			method = MapToDefaultMethod(address);
		}
		AsyncOperation asyncOperation = StartAsyncOperation(userToken);
		try
		{
			byte[] bytes = Encoding.GetBytes(data);
			_method = method;
			_contentLength = bytes.Length;
			UploadBitsAsync(_webRequest = GetWebRequest(GetUri(address)), null, bytes, 0, null, null, asyncOperation, delegate(byte[] bytesResult, Exception error, AsyncOperation uploadAsyncOp)
			{
				string result = null;
				if (error == null && bytesResult != null)
				{
					try
					{
						result = GetStringUsingEncoding(_webRequest, bytesResult);
					}
					catch (Exception ex2) when (!(ex2 is OutOfMemoryException))
					{
						error = GetExceptionToPropagate(ex2);
					}
				}
				InvokeOperationCompleted(uploadAsyncOp, _uploadStringOperationCompleted, new UploadStringCompletedEventArgs(result, error, _canceled, uploadAsyncOp.UserSuppliedState));
			});
		}
		catch (Exception ex) when (!(ex is OutOfMemoryException))
		{
			UploadStringCompletedEventArgs eventArgs = new UploadStringCompletedEventArgs(null, GetExceptionToPropagate(ex), _canceled, asyncOperation.UserSuppliedState);
			InvokeOperationCompleted(asyncOperation, _uploadStringOperationCompleted, eventArgs);
		}
	}

	/// <summary>Uploads a data buffer to a resource identified by a URI, using the POST method. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to receive the data. </param>
	/// <param name="data">The data buffer to send to the resource. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource. </exception>
	public void UploadDataAsync(Uri address, byte[] data)
	{
		UploadDataAsync(address, null, data, null);
	}

	/// <summary>Uploads a data buffer to a resource identified by a URI, using the specified method. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to receive the data.</param>
	/// <param name="method">The method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The data buffer to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource. </exception>
	public void UploadDataAsync(Uri address, string method, byte[] data)
	{
		UploadDataAsync(address, method, data, null);
	}

	/// <summary>Uploads a data buffer to a resource identified by a URI, using the specified method and identifying token.</summary>
	/// <param name="address">The URI of the resource to receive the data.</param>
	/// <param name="method">The method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The data buffer to send to the resource.</param>
	/// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource. </exception>
	public void UploadDataAsync(Uri address, string method, byte[] data, object userToken)
	{
		ThrowIfNull(address, "address");
		ThrowIfNull(data, "data");
		if (method == null)
		{
			method = MapToDefaultMethod(address);
		}
		AsyncOperation asyncOp = StartAsyncOperation(userToken);
		try
		{
			_method = method;
			_contentLength = data.Length;
			WebRequest request = (_webRequest = GetWebRequest(GetUri(address)));
			int chunkSize = 0;
			if (this.UploadProgressChanged != null)
			{
				chunkSize = (int)Math.Min(8192L, data.Length);
			}
			UploadBitsAsync(request, null, data, chunkSize, null, null, asyncOp, delegate(byte[] result, Exception error, AsyncOperation uploadAsyncOp)
			{
				InvokeOperationCompleted(asyncOp, _uploadDataOperationCompleted, new UploadDataCompletedEventArgs(result, error, _canceled, uploadAsyncOp.UserSuppliedState));
			});
		}
		catch (Exception ex) when (!(ex is OutOfMemoryException))
		{
			UploadDataCompletedEventArgs eventArgs = new UploadDataCompletedEventArgs(null, GetExceptionToPropagate(ex), _canceled, asyncOp.UserSuppliedState);
			InvokeOperationCompleted(asyncOp, _uploadDataOperationCompleted, eventArgs);
		}
	}

	/// <summary>Uploads the specified local file to the specified resource, using the POST method. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to receive the file. For HTTP resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page. </param>
	/// <param name="fileName">The file to send to the resource. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- <paramref name="fileName" /> is null, is <see cref="F:System.String.Empty" />, contains invalid character, or the specified path to the file does not exist.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource.-or- The Content-type header begins with multipart. </exception>
	public void UploadFileAsync(Uri address, string fileName)
	{
		UploadFileAsync(address, null, fileName, null);
	}

	/// <summary>Uploads the specified local file to the specified resource, using the POST method. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to receive the file. For HTTP resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page. </param>
	/// <param name="method">The method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="fileName">The file to send to the resource. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- <paramref name="fileName" /> is null, is <see cref="F:System.String.Empty" />, contains invalid character, or the specified path to the file does not exist.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource.-or- The Content-type header begins with multipart. </exception>
	public void UploadFileAsync(Uri address, string method, string fileName)
	{
		UploadFileAsync(address, method, fileName, null);
	}

	/// <summary>Uploads the specified local file to the specified resource, using the POST method. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to receive the file. For HTTP resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page.</param>
	/// <param name="method">The method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="fileName">The file to send to the resource.</param>
	/// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- <paramref name="fileName" /> is null, is <see cref="F:System.String.Empty" />, contains invalid character, or the specified path to the file does not exist.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource.-or- The Content-type header begins with multipart. </exception>
	public void UploadFileAsync(Uri address, string method, string fileName, object userToken)
	{
		ThrowIfNull(address, "address");
		ThrowIfNull(fileName, "fileName");
		if (method == null)
		{
			method = MapToDefaultMethod(address);
		}
		FileStream fs = null;
		AsyncOperation asyncOp = StartAsyncOperation(userToken);
		try
		{
			_method = method;
			byte[] formHeaderBytes = null;
			byte[] boundaryBytes = null;
			byte[] buffer = null;
			Uri uri = GetUri(address);
			bool needsHeaderAndBoundary = uri.Scheme != Uri.UriSchemeFile;
			OpenFileInternal(needsHeaderAndBoundary, fileName, ref fs, ref buffer, ref formHeaderBytes, ref boundaryBytes);
			UploadBitsAsync(_webRequest = GetWebRequest(uri), fs, buffer, 0, formHeaderBytes, boundaryBytes, asyncOp, delegate(byte[] result, Exception error, AsyncOperation uploadAsyncOp)
			{
				InvokeOperationCompleted(asyncOp, _uploadFileOperationCompleted, new UploadFileCompletedEventArgs(result, error, _canceled, uploadAsyncOp.UserSuppliedState));
			});
		}
		catch (Exception ex) when (!(ex is OutOfMemoryException))
		{
			fs?.Close();
			UploadFileCompletedEventArgs eventArgs = new UploadFileCompletedEventArgs(null, GetExceptionToPropagate(ex), _canceled, asyncOp.UserSuppliedState);
			InvokeOperationCompleted(asyncOp, _uploadFileOperationCompleted, eventArgs);
		}
	}

	/// <summary>Uploads the data in the specified name/value collection to the resource identified by the specified URI. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to receive the collection. This URI must identify a resource that can accept a request sent with the default method. See remarks.</param>
	/// <param name="data">The <see cref="T:System.Collections.Specialized.NameValueCollection" /> to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- There was no response from the server hosting the resource.</exception>
	public void UploadValuesAsync(Uri address, NameValueCollection data)
	{
		UploadValuesAsync(address, null, data, null);
	}

	/// <summary>Uploads the data in the specified name/value collection to the resource identified by the specified URI, using the specified method. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to receive the collection. This URI must identify a resource that can accept a request sent with the <paramref name="method" /> method.</param>
	/// <param name="method">The method used to send the string to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The <see cref="T:System.Collections.Specialized.NameValueCollection" /> to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- There was no response from the server hosting the resource.-or-<paramref name="method" /> cannot be used to send content.</exception>
	public void UploadValuesAsync(Uri address, string method, NameValueCollection data)
	{
		UploadValuesAsync(address, method, data, null);
	}

	/// <summary>Uploads the data in the specified name/value collection to the resource identified by the specified URI, using the specified method. This method does not block the calling thread, and allows the caller to pass an object to the method that is invoked when the operation completes.</summary>
	/// <param name="address">The URI of the resource to receive the collection. This URI must identify a resource that can accept a request sent with the <paramref name="method" /> method.</param>
	/// <param name="method">The HTTP method used to send the string to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The <see cref="T:System.Collections.Specialized.NameValueCollection" /> to send to the resource.</param>
	/// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- There was no response from the server hosting the resource.-or-<paramref name="method" /> cannot be used to send content.</exception>
	public void UploadValuesAsync(Uri address, string method, NameValueCollection data, object userToken)
	{
		ThrowIfNull(address, "address");
		ThrowIfNull(data, "data");
		if (method == null)
		{
			method = MapToDefaultMethod(address);
		}
		AsyncOperation asyncOp = StartAsyncOperation(userToken);
		try
		{
			byte[] valuesToUpload = GetValuesToUpload(data);
			_method = method;
			WebRequest request = (_webRequest = GetWebRequest(GetUri(address)));
			int chunkSize = 0;
			if (this.UploadProgressChanged != null)
			{
				chunkSize = (int)Math.Min(8192L, valuesToUpload.Length);
			}
			UploadBitsAsync(request, null, valuesToUpload, chunkSize, null, null, asyncOp, delegate(byte[] result, Exception error, AsyncOperation uploadAsyncOp)
			{
				InvokeOperationCompleted(asyncOp, _uploadValuesOperationCompleted, new UploadValuesCompletedEventArgs(result, error, _canceled, uploadAsyncOp.UserSuppliedState));
			});
		}
		catch (Exception ex) when (!(ex is OutOfMemoryException))
		{
			UploadValuesCompletedEventArgs eventArgs = new UploadValuesCompletedEventArgs(null, GetExceptionToPropagate(ex), _canceled, asyncOp.UserSuppliedState);
			InvokeOperationCompleted(asyncOp, _uploadValuesOperationCompleted, eventArgs);
		}
	}

	private static Exception GetExceptionToPropagate(Exception e)
	{
		if (!(e is WebException) && !(e is SecurityException))
		{
			return new WebException("An exception occurred during a WebClient request.", e);
		}
		return e;
	}

	/// <summary>Cancels a pending asynchronous operation.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void CancelAsync()
	{
		WebRequest webRequest = _webRequest;
		_canceled = true;
		AbortRequest(webRequest);
	}

	/// <summary>Downloads the resource as a <see cref="T:System.String" /> from the URI specified as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the downloaded resource.</returns>
	/// <param name="address">The URI of the resource to download.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while downloading the resource. </exception>
	public Task<string> DownloadStringTaskAsync(string address)
	{
		return DownloadStringTaskAsync(GetUri(address));
	}

	/// <summary>Downloads the resource as a <see cref="T:System.String" /> from the URI specified as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the downloaded resource.</returns>
	/// <param name="address">The URI of the resource to download.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while downloading the resource. </exception>
	public Task<string> DownloadStringTaskAsync(Uri address)
	{
		TaskCompletionSource<string> tcs = new TaskCompletionSource<string>(address);
		DownloadStringCompletedEventHandler handler = null;
		handler = delegate(object sender, DownloadStringCompletedEventArgs e)
		{
			HandleCompletion(tcs, e, (DownloadStringCompletedEventArgs args) => args.Result, handler, delegate(WebClient webClient, DownloadStringCompletedEventHandler completion)
			{
				webClient.DownloadStringCompleted -= completion;
			});
		};
		DownloadStringCompleted += handler;
		try
		{
			DownloadStringAsync(address, tcs);
		}
		catch
		{
			DownloadStringCompleted -= handler;
			throw;
		}
		return tcs.Task;
	}

	/// <summary>Opens a readable stream containing the specified resource as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.IO.Stream" /> used to read data from a resource.</returns>
	/// <param name="address">The URI of the resource to retrieve.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and address is invalid.-or- An error occurred while downloading the resource. -or- An error occurred while opening the stream.</exception>
	public Task<Stream> OpenReadTaskAsync(string address)
	{
		return OpenReadTaskAsync(GetUri(address));
	}

	/// <summary>Opens a readable stream containing the specified resource as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.IO.Stream" /> used to read data from a resource.</returns>
	/// <param name="address">The URI of the resource to retrieve.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and address is invalid.-or- An error occurred while downloading the resource. -or- An error occurred while opening the stream.</exception>
	public Task<Stream> OpenReadTaskAsync(Uri address)
	{
		TaskCompletionSource<Stream> tcs = new TaskCompletionSource<Stream>(address);
		OpenReadCompletedEventHandler handler = null;
		handler = delegate(object sender, OpenReadCompletedEventArgs e)
		{
			HandleCompletion(tcs, e, (OpenReadCompletedEventArgs args) => args.Result, handler, delegate(WebClient webClient, OpenReadCompletedEventHandler completion)
			{
				webClient.OpenReadCompleted -= completion;
			});
		};
		OpenReadCompleted += handler;
		try
		{
			OpenReadAsync(address, tcs);
		}
		catch
		{
			OpenReadCompleted -= handler;
			throw;
		}
		return tcs.Task;
	}

	/// <summary>Opens a stream for writing data to the specified resource as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.IO.Stream" /> used to write data to the resource.</returns>
	/// <param name="address">The URI of the resource to receive the data.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream. </exception>
	public Task<Stream> OpenWriteTaskAsync(string address)
	{
		return OpenWriteTaskAsync(GetUri(address), null);
	}

	/// <summary>Opens a stream for writing data to the specified resource as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.IO.Stream" /> used to write data to the resource.</returns>
	/// <param name="address">The URI of the resource to receive the data.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream. </exception>
	public Task<Stream> OpenWriteTaskAsync(Uri address)
	{
		return OpenWriteTaskAsync(address, null);
	}

	/// <summary>Opens a stream for writing data to the specified resource as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.IO.Stream" /> used to write data to the resource.</returns>
	/// <param name="address">The URI of the resource to receive the data.</param>
	/// <param name="method">The method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream. </exception>
	public Task<Stream> OpenWriteTaskAsync(string address, string method)
	{
		return OpenWriteTaskAsync(GetUri(address), method);
	}

	/// <summary>Opens a stream for writing data to the specified resource as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.IO.Stream" /> used to write data to the resource.</returns>
	/// <param name="address">The URI of the resource to receive the data.</param>
	/// <param name="method">The method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream. </exception>
	public Task<Stream> OpenWriteTaskAsync(Uri address, string method)
	{
		TaskCompletionSource<Stream> tcs = new TaskCompletionSource<Stream>(address);
		OpenWriteCompletedEventHandler handler = null;
		handler = delegate(object sender, OpenWriteCompletedEventArgs e)
		{
			HandleCompletion(tcs, e, (OpenWriteCompletedEventArgs args) => args.Result, handler, delegate(WebClient webClient, OpenWriteCompletedEventHandler completion)
			{
				webClient.OpenWriteCompleted -= completion;
			});
		};
		OpenWriteCompleted += handler;
		try
		{
			OpenWriteAsync(address, method, tcs);
		}
		catch
		{
			OpenWriteCompleted -= handler;
			throw;
		}
		return tcs.Task;
	}

	/// <summary>Uploads the specified string to the specified resource as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.String" /> containing the response sent by the server.</returns>
	/// <param name="address">The URI of the resource to receive the string. For HTTP resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page.</param>
	/// <param name="data">The string to be uploaded.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> is null.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- There was no response from the server hosting the resource.</exception>
	public Task<string> UploadStringTaskAsync(string address, string data)
	{
		return UploadStringTaskAsync(address, null, data);
	}

	/// <summary>Uploads the specified string to the specified resource as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.String" /> containing the response sent by the server.</returns>
	/// <param name="address">The URI of the resource to receive the string. For HTTP resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page.</param>
	/// <param name="data">The string to be uploaded.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> is null.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- There was no response from the server hosting the resource.</exception>
	public Task<string> UploadStringTaskAsync(Uri address, string data)
	{
		return UploadStringTaskAsync(address, null, data);
	}

	/// <summary>Uploads the specified string to the specified resource as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.String" /> containing the response sent by the server.</returns>
	/// <param name="address">The URI of the resource to receive the string. For HTTP resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page.</param>
	/// <param name="method">The HTTP method used to send the file to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The string to be uploaded.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> is null.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or-<paramref name="method" /> cannot be used to send content.-or- There was no response from the server hosting the resource.</exception>
	public Task<string> UploadStringTaskAsync(string address, string method, string data)
	{
		return UploadStringTaskAsync(GetUri(address), method, data);
	}

	/// <summary>Uploads the specified string to the specified resource as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.String" /> containing the response sent by the server.</returns>
	/// <param name="address">The URI of the resource to receive the string. For HTTP resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page.</param>
	/// <param name="method">The HTTP method used to send the file to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The string to be uploaded.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> is null.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or-<paramref name="method" /> cannot be used to send content.-or- There was no response from the server hosting the resource.</exception>
	public Task<string> UploadStringTaskAsync(Uri address, string method, string data)
	{
		TaskCompletionSource<string> tcs = new TaskCompletionSource<string>(address);
		UploadStringCompletedEventHandler handler = null;
		handler = delegate(object sender, UploadStringCompletedEventArgs e)
		{
			HandleCompletion(tcs, e, (UploadStringCompletedEventArgs args) => args.Result, handler, delegate(WebClient webClient, UploadStringCompletedEventHandler completion)
			{
				webClient.UploadStringCompleted -= completion;
			});
		};
		UploadStringCompleted += handler;
		try
		{
			UploadStringAsync(address, method, data, tcs);
		}
		catch
		{
			UploadStringCompleted -= handler;
			throw;
		}
		return tcs.Task;
	}

	/// <summary>Downloads the resource as a <see cref="T:System.Byte" /> array from the URI specified as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the downloaded resource.</returns>
	/// <param name="address">The URI of the resource to download. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while downloading the resource. </exception>
	public Task<byte[]> DownloadDataTaskAsync(string address)
	{
		return DownloadDataTaskAsync(GetUri(address));
	}

	/// <summary>Downloads the resource as a <see cref="T:System.Byte" /> array from the URI specified as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the downloaded resource.</returns>
	/// <param name="address">The URI of the resource to download.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while downloading the resource. </exception>
	public Task<byte[]> DownloadDataTaskAsync(Uri address)
	{
		TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>(address);
		DownloadDataCompletedEventHandler handler = null;
		handler = delegate(object sender, DownloadDataCompletedEventArgs e)
		{
			HandleCompletion(tcs, e, (DownloadDataCompletedEventArgs args) => args.Result, handler, delegate(WebClient webClient, DownloadDataCompletedEventHandler completion)
			{
				webClient.DownloadDataCompleted -= completion;
			});
		};
		DownloadDataCompleted += handler;
		try
		{
			DownloadDataAsync(address, tcs);
		}
		catch
		{
			DownloadDataCompleted -= handler;
			throw;
		}
		return tcs.Task;
	}

	/// <summary>Downloads the specified resource to a local file as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task" />.The task object representing the asynchronous operation.</returns>
	/// <param name="address">The URI of the resource to download.</param>
	/// <param name="fileName">The name of the file to be placed on the local computer.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while downloading the resource. </exception>
	/// <exception cref="T:System.InvalidOperationException">The local file specified by <paramref name="fileName" /> is in use by another thread.</exception>
	public Task DownloadFileTaskAsync(string address, string fileName)
	{
		return DownloadFileTaskAsync(GetUri(address), fileName);
	}

	/// <summary>Downloads the specified resource to a local file as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task" />.The task object representing the asynchronous operation.</returns>
	/// <param name="address">The URI of the resource to download.</param>
	/// <param name="fileName">The name of the file to be placed on the local computer.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while downloading the resource. </exception>
	/// <exception cref="T:System.InvalidOperationException">The local file specified by <paramref name="fileName" /> is in use by another thread.</exception>
	public Task DownloadFileTaskAsync(Uri address, string fileName)
	{
		TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(address);
		AsyncCompletedEventHandler handler = null;
		handler = delegate(object sender, AsyncCompletedEventArgs e)
		{
			HandleCompletion(tcs, e, (AsyncCompletedEventArgs args) => (object)null, handler, delegate(WebClient webClient, AsyncCompletedEventHandler completion)
			{
				webClient.DownloadFileCompleted -= completion;
			});
		};
		DownloadFileCompleted += handler;
		try
		{
			DownloadFileAsync(address, fileName, tcs);
		}
		catch
		{
			DownloadFileCompleted -= handler;
			throw;
		}
		return tcs.Task;
	}

	/// <summary>Uploads a data buffer that contains a <see cref="T:System.Byte" /> array to the URI specified as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the body of the response received from the resource when the data buffer was uploaded.</returns>
	/// <param name="address">The URI of the resource to receive the data.</param>
	/// <param name="data">The data buffer to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource. </exception>
	public Task<byte[]> UploadDataTaskAsync(string address, byte[] data)
	{
		return UploadDataTaskAsync(GetUri(address), null, data);
	}

	/// <summary>Uploads a data buffer that contains a <see cref="T:System.Byte" /> array to the URI specified as an asynchronous operation using a task object.  </summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the body of the response received from the resource when the data buffer was uploaded.</returns>
	/// <param name="address">The URI of the resource to receive the data.</param>
	/// <param name="data">The data buffer to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource. </exception>
	public Task<byte[]> UploadDataTaskAsync(Uri address, byte[] data)
	{
		return UploadDataTaskAsync(address, null, data);
	}

	/// <summary>Uploads a data buffer that contains a <see cref="T:System.Byte" /> array to the URI specified as an asynchronous operation using a task object.  </summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the body of the response received from the resource when the data buffer was uploaded.</returns>
	/// <param name="address">The URI of the resource to receive the data.</param>
	/// <param name="method">The method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The data buffer to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource. </exception>
	public Task<byte[]> UploadDataTaskAsync(string address, string method, byte[] data)
	{
		return UploadDataTaskAsync(GetUri(address), method, data);
	}

	/// <summary>Uploads a data buffer that contains a <see cref="T:System.Byte" /> array to the URI specified as an asynchronous operation using a task object.  </summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the body of the response received from the resource when the data buffer was uploaded.</returns>
	/// <param name="address">The URI of the resource to receive the data.</param>
	/// <param name="method">The method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The data buffer to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource. </exception>
	public Task<byte[]> UploadDataTaskAsync(Uri address, string method, byte[] data)
	{
		TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>(address);
		UploadDataCompletedEventHandler handler = null;
		handler = delegate(object sender, UploadDataCompletedEventArgs e)
		{
			HandleCompletion(tcs, e, (UploadDataCompletedEventArgs args) => args.Result, handler, delegate(WebClient webClient, UploadDataCompletedEventHandler completion)
			{
				webClient.UploadDataCompleted -= completion;
			});
		};
		UploadDataCompleted += handler;
		try
		{
			UploadDataAsync(address, method, data, tcs);
		}
		catch
		{
			UploadDataCompleted -= handler;
			throw;
		}
		return tcs.Task;
	}

	/// <summary>Uploads the specified local file to a resource as an asynchronous operation using a task object. </summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the body of the response received from the resource when the file was uploaded.</returns>
	/// <param name="address">The URI of the resource to receive the file. For HTTP resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page.</param>
	/// <param name="fileName">The local file to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- <paramref name="fileName" /> is null, is <see cref="F:System.String.Empty" />, contains invalid character, or the specified path to the file does not exist.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource.-or- The Content-type header begins with multipart. </exception>
	public Task<byte[]> UploadFileTaskAsync(string address, string fileName)
	{
		return UploadFileTaskAsync(GetUri(address), null, fileName);
	}

	/// <summary>Uploads the specified local file to a resource as an asynchronous operation using a task object. </summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the body of the response received from the resource when the file was uploaded.</returns>
	/// <param name="address">The URI of the resource to receive the file. For HTTP resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page.</param>
	/// <param name="fileName">The local file to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- <paramref name="fileName" /> is null, is <see cref="F:System.String.Empty" />, contains invalid character, or the specified path to the file does not exist.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource.-or- The Content-type header begins with multipart. </exception>
	public Task<byte[]> UploadFileTaskAsync(Uri address, string fileName)
	{
		return UploadFileTaskAsync(address, null, fileName);
	}

	/// <summary>Uploads the specified local file to a resource as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the body of the response received from the resource when the file was uploaded.</returns>
	/// <param name="address">The URI of the resource to receive the file. For HTTP resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page.</param>
	/// <param name="method">The method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="fileName">The local file to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- <paramref name="fileName" /> is null, is <see cref="F:System.String.Empty" />, contains invalid character, or the specified path to the file does not exist.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource.-or- The Content-type header begins with multipart. </exception>
	public Task<byte[]> UploadFileTaskAsync(string address, string method, string fileName)
	{
		return UploadFileTaskAsync(GetUri(address), method, fileName);
	}

	/// <summary>Uploads the specified local file to a resource as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the body of the response received from the resource when the file was uploaded.</returns>
	/// <param name="address">The URI of the resource to receive the file. For HTTP resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page.</param>
	/// <param name="method">The method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="fileName">The local file to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- <paramref name="fileName" /> is null, is <see cref="F:System.String.Empty" />, contains invalid character, or the specified path to the file does not exist.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource.-or- The Content-type header begins with multipart. </exception>
	public Task<byte[]> UploadFileTaskAsync(Uri address, string method, string fileName)
	{
		TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>(address);
		UploadFileCompletedEventHandler handler = null;
		handler = delegate(object sender, UploadFileCompletedEventArgs e)
		{
			HandleCompletion(tcs, e, (UploadFileCompletedEventArgs args) => args.Result, handler, delegate(WebClient webClient, UploadFileCompletedEventHandler completion)
			{
				webClient.UploadFileCompleted -= completion;
			});
		};
		UploadFileCompleted += handler;
		try
		{
			UploadFileAsync(address, method, fileName, tcs);
		}
		catch
		{
			UploadFileCompleted -= handler;
			throw;
		}
		return tcs.Task;
	}

	/// <summary>Uploads the specified name/value collection to the resource identified by the specified URI as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the response sent by the server.</returns>
	/// <param name="address">The URI of the resource to receive the collection.</param>
	/// <param name="data">The <see cref="T:System.Collections.Specialized.NameValueCollection" /> to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- There was no response from the server hosting the resource.-or- An error occurred while opening the stream.-or- The Content-type header is not null or "application/x-www-form-urlencoded". </exception>
	public Task<byte[]> UploadValuesTaskAsync(string address, NameValueCollection data)
	{
		return UploadValuesTaskAsync(GetUri(address), null, data);
	}

	/// <summary>Uploads the specified name/value collection to the resource identified by the specified URI as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the response sent by the server.</returns>
	/// <param name="address">The URI of the resource to receive the collection.</param>
	/// <param name="method">The HTTP method used to send the collection to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The <see cref="T:System.Collections.Specialized.NameValueCollection" /> to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or-<paramref name="method" /> cannot be used to send content.-or- There was no response from the server hosting the resource.-or- An error occurred while opening the stream.-or- The Content-type header is not null or "application/x-www-form-urlencoded". </exception>
	public Task<byte[]> UploadValuesTaskAsync(string address, string method, NameValueCollection data)
	{
		return UploadValuesTaskAsync(GetUri(address), method, data);
	}

	/// <summary>Uploads the specified name/value collection to the resource identified by the specified URI as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the response sent by the server.</returns>
	/// <param name="address">The URI of the resource to receive the collection.</param>
	/// <param name="data">The <see cref="T:System.Collections.Specialized.NameValueCollection" /> to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource.-or- The Content-type header value is not null and is not application/x-www-form-urlencoded. </exception>
	public Task<byte[]> UploadValuesTaskAsync(Uri address, NameValueCollection data)
	{
		return UploadValuesTaskAsync(address, null, data);
	}

	/// <summary>Uploads the specified name/value collection to the resource identified by the specified URI as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the response sent by the server.</returns>
	/// <param name="address">The URI of the resource to receive the collection.</param>
	/// <param name="method">The HTTP method used to send the collection to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The <see cref="T:System.Collections.Specialized.NameValueCollection" /> to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or-<paramref name="method" /> cannot be used to send content.-or- There was no response from the server hosting the resource.-or- An error occurred while opening the stream.-or- The Content-type header is not null or "application/x-www-form-urlencoded". </exception>
	public Task<byte[]> UploadValuesTaskAsync(Uri address, string method, NameValueCollection data)
	{
		TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>(address);
		UploadValuesCompletedEventHandler handler = null;
		handler = delegate(object sender, UploadValuesCompletedEventArgs e)
		{
			HandleCompletion(tcs, e, (UploadValuesCompletedEventArgs args) => args.Result, handler, delegate(WebClient webClient, UploadValuesCompletedEventHandler completion)
			{
				webClient.UploadValuesCompleted -= completion;
			});
		};
		UploadValuesCompleted += handler;
		try
		{
			UploadValuesAsync(address, method, data, tcs);
		}
		catch
		{
			UploadValuesCompleted -= handler;
			throw;
		}
		return tcs.Task;
	}

	private void HandleCompletion<TAsyncCompletedEventArgs, TCompletionDelegate, T>(TaskCompletionSource<T> tcs, TAsyncCompletedEventArgs e, Func<TAsyncCompletedEventArgs, T> getResult, TCompletionDelegate handler, Action<WebClient, TCompletionDelegate> unregisterHandler) where TAsyncCompletedEventArgs : AsyncCompletedEventArgs
	{
		if (e.UserState != tcs)
		{
			return;
		}
		try
		{
			unregisterHandler(this, handler);
		}
		finally
		{
			if (e.Error != null)
			{
				tcs.TrySetException(e.Error);
			}
			else if (e.Cancelled)
			{
				tcs.TrySetCanceled();
			}
			else
			{
				tcs.TrySetResult(getResult(e));
			}
		}
	}

	private void PostProgressChanged(AsyncOperation asyncOp, ProgressData progress)
	{
		if (asyncOp == null || (progress.BytesSent <= 0 && progress.BytesReceived <= 0))
		{
			return;
		}
		if (progress.HasUploadPhase)
		{
			if (this.UploadProgressChanged != null)
			{
				int progressPercentage = (int)((progress.TotalBytesToReceive >= 0 || progress.BytesReceived != 0L) ? ((progress.TotalBytesToSend < 0) ? 50 : ((progress.TotalBytesToReceive == 0L) ? 100 : (50 * progress.BytesReceived / progress.TotalBytesToReceive + 50))) : ((progress.TotalBytesToSend >= 0) ? ((progress.TotalBytesToSend == 0L) ? 50 : (50 * progress.BytesSent / progress.TotalBytesToSend)) : 0));
				asyncOp.Post(_reportUploadProgressChanged, new UploadProgressChangedEventArgs(progressPercentage, asyncOp.UserSuppliedState, progress.BytesSent, progress.TotalBytesToSend, progress.BytesReceived, progress.TotalBytesToReceive));
			}
		}
		else if (this.DownloadProgressChanged != null)
		{
			int progressPercentage = (int)((progress.TotalBytesToReceive >= 0) ? ((progress.TotalBytesToReceive == 0L) ? 100 : (100 * progress.BytesReceived / progress.TotalBytesToReceive)) : 0);
			asyncOp.Post(_reportDownloadProgressChanged, new DownloadProgressChangedEventArgs(progressPercentage, asyncOp.UserSuppliedState, progress.BytesReceived, progress.TotalBytesToReceive));
		}
	}

	private static void ThrowIfNull(object argument, string parameterName)
	{
		if (argument == null)
		{
			throw new ArgumentNullException(parameterName);
		}
	}

	/// <summary>Raises the <see cref="E:System.Net.WebClient.WriteStreamClosed" /> event.</summary>
	/// <param name="e">A <see cref="T:System.Net.WriteStreamClosedEventArgs" />  object containing event data.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
	protected virtual void OnWriteStreamClosed(WriteStreamClosedEventArgs e)
	{
	}
}
