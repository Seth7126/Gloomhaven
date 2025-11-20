using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net.Cache;
using System.Net.Security;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mono.Net.Security;
using Mono.Security.Interface;
using Unity;

namespace System.Net;

/// <summary>Provides an HTTP-specific implementation of the <see cref="T:System.Net.WebRequest" /> class.</summary>
[Serializable]
public class HttpWebRequest : WebRequest, ISerializable
{
	private enum NtlmAuthState
	{
		None,
		Challenge,
		Response
	}

	private struct AuthorizationState
	{
		private readonly HttpWebRequest request;

		private readonly bool isProxy;

		private bool isCompleted;

		private NtlmAuthState ntlm_auth_state;

		public bool IsCompleted => isCompleted;

		public NtlmAuthState NtlmAuthState => ntlm_auth_state;

		public bool IsNtlmAuthenticated
		{
			get
			{
				if (isCompleted)
				{
					return ntlm_auth_state != NtlmAuthState.None;
				}
				return false;
			}
		}

		public AuthorizationState(HttpWebRequest request, bool isProxy)
		{
			this.request = request;
			this.isProxy = isProxy;
			isCompleted = false;
			ntlm_auth_state = NtlmAuthState.None;
		}

		public bool CheckAuthorization(WebResponse response, HttpStatusCode code)
		{
			isCompleted = false;
			if (code == HttpStatusCode.Unauthorized && request.credentials == null)
			{
				return false;
			}
			if (isProxy != (code == HttpStatusCode.ProxyAuthenticationRequired))
			{
				return false;
			}
			if (isProxy && (request.proxy == null || request.proxy.Credentials == null))
			{
				return false;
			}
			string[] values = response.Headers.GetValues(isProxy ? "Proxy-Authenticate" : "WWW-Authenticate");
			if (values == null || values.Length == 0)
			{
				return false;
			}
			ICredentials credentials = ((!isProxy) ? request.credentials : request.proxy.Credentials);
			Authorization authorization = null;
			string[] array = values;
			for (int i = 0; i < array.Length; i++)
			{
				authorization = AuthenticationManager.Authenticate(array[i], request, credentials);
				if (authorization != null)
				{
					break;
				}
			}
			if (authorization == null)
			{
				return false;
			}
			request.webHeaders[isProxy ? "Proxy-Authorization" : "Authorization"] = authorization.Message;
			isCompleted = authorization.Complete;
			if (authorization.ModuleAuthenticationType == "NTLM")
			{
				ntlm_auth_state++;
			}
			return true;
		}

		public void Reset()
		{
			isCompleted = false;
			ntlm_auth_state = NtlmAuthState.None;
			request.webHeaders.RemoveInternal(isProxy ? "Proxy-Authorization" : "Authorization");
		}

		public override string ToString()
		{
			return string.Format("{0}AuthState [{1}:{2}]", isProxy ? "Proxy" : "", isCompleted, ntlm_auth_state);
		}
	}

	private Uri requestUri;

	private Uri actualUri;

	private bool hostChanged;

	private bool allowAutoRedirect;

	private bool allowBuffering;

	private bool allowReadStreamBuffering;

	private X509CertificateCollection certificates;

	private string connectionGroup;

	private bool haveContentLength;

	private long contentLength;

	private HttpContinueDelegate continueDelegate;

	private CookieContainer cookieContainer;

	private ICredentials credentials;

	private bool haveResponse;

	private bool requestSent;

	private WebHeaderCollection webHeaders;

	private bool keepAlive;

	private int maxAutoRedirect;

	private string mediaType;

	private string method;

	private string initialMethod;

	private bool pipelined;

	private bool preAuthenticate;

	private bool usedPreAuth;

	private Version version;

	private bool force_version;

	private Version actualVersion;

	private IWebProxy proxy;

	private bool sendChunked;

	private ServicePoint servicePoint;

	private int timeout;

	private int continueTimeout;

	private WebRequestStream writeStream;

	private HttpWebResponse webResponse;

	private WebCompletionSource responseTask;

	private WebOperation currentOperation;

	private int aborted;

	private bool gotRequestStream;

	private int redirects;

	private bool expectContinue;

	private bool getResponseCalled;

	private object locker;

	private bool finished_reading;

	private DecompressionMethods auto_decomp;

	private int maxResponseHeadersLength;

	private static int defaultMaxResponseHeadersLength;

	private static int defaultMaximumErrorResponseLength;

	private static RequestCachePolicy defaultCachePolicy;

	private int readWriteTimeout;

	private MobileTlsProvider tlsProvider;

	private MonoTlsSettings tlsSettings;

	private ServerCertValidationCallback certValidationCallback;

	private bool hostHasPort;

	private Uri hostUri;

	private AuthorizationState auth_state;

	private AuthorizationState proxy_auth_state;

	[NonSerialized]
	internal Func<Stream, Task> ResendContentFactory;

	internal readonly int ID;

	private bool unsafe_auth_blah;

	/// <summary>Gets or sets the value of the Accept HTTP header.</summary>
	/// <returns>The value of the Accept HTTP header. The default value is null.</returns>
	public string Accept
	{
		get
		{
			return webHeaders["Accept"];
		}
		set
		{
			CheckRequestStarted();
			SetSpecialHeaders("Accept", value);
		}
	}

	/// <summary>Gets the Uniform Resource Identifier (URI) of the Internet resource that actually responds to the request.</summary>
	/// <returns>A <see cref="T:System.Uri" /> that identifies the Internet resource that actually responds to the request. The default is the URI used by the <see cref="M:System.Net.WebRequest.Create(System.String)" /> method to initialize the request.</returns>
	public Uri Address
	{
		get
		{
			return actualUri;
		}
		internal set
		{
			actualUri = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the request should follow redirection responses.</summary>
	/// <returns>true if the request should automatically follow redirection responses from the Internet resource; otherwise, false. The default value is true.</returns>
	public virtual bool AllowAutoRedirect
	{
		get
		{
			return allowAutoRedirect;
		}
		set
		{
			allowAutoRedirect = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether to buffer the data sent to the Internet resource.</summary>
	/// <returns>true to enable buffering of the data sent to the Internet resource; false to disable buffering. The default is true.</returns>
	public virtual bool AllowWriteStreamBuffering
	{
		get
		{
			return allowBuffering;
		}
		set
		{
			allowBuffering = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether to buffer the received from the  Internet resource.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true to enable buffering of the data received from the Internet resource; false to disable buffering. The default is true.</returns>
	public virtual bool AllowReadStreamBuffering
	{
		get
		{
			return allowReadStreamBuffering;
		}
		set
		{
			allowReadStreamBuffering = value;
		}
	}

	/// <summary>Gets or sets the type of decompression that is used.</summary>
	/// <returns>A T:System.Net.DecompressionMethods object that indicates the type of decompression that is used. </returns>
	/// <exception cref="T:System.InvalidOperationException">The object's current state does not allow this property to be set.</exception>
	public DecompressionMethods AutomaticDecompression
	{
		get
		{
			return auto_decomp;
		}
		set
		{
			CheckRequestStarted();
			auto_decomp = value;
		}
	}

	internal bool InternalAllowBuffering
	{
		get
		{
			if (allowBuffering)
			{
				return MethodWithBuffer;
			}
			return false;
		}
	}

	private bool MethodWithBuffer
	{
		get
		{
			if (method != "HEAD" && method != "GET" && method != "MKCOL" && method != "CONNECT")
			{
				return method != "TRACE";
			}
			return false;
		}
	}

	internal MobileTlsProvider TlsProvider => tlsProvider;

	internal MonoTlsSettings TlsSettings => tlsSettings;

	/// <summary>Gets or sets the collection of security certificates that are associated with this request.</summary>
	/// <returns>The <see cref="T:System.Security.Cryptography.X509Certificates.X509CertificateCollection" /> that contains the security certificates associated with this request.</returns>
	/// <exception cref="T:System.ArgumentNullException">The value specified for a set operation is null. </exception>
	public X509CertificateCollection ClientCertificates
	{
		get
		{
			if (certificates == null)
			{
				certificates = new X509CertificateCollection();
			}
			return certificates;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			certificates = value;
		}
	}

	/// <summary>Gets or sets the value of the Connection HTTP header.</summary>
	/// <returns>The value of the Connection HTTP header. The default value is null.</returns>
	/// <exception cref="T:System.ArgumentException">The value of <see cref="P:System.Net.HttpWebRequest.Connection" /> is set to Keep-alive or Close. </exception>
	public string Connection
	{
		get
		{
			return webHeaders["Connection"];
		}
		set
		{
			CheckRequestStarted();
			if (string.IsNullOrWhiteSpace(value))
			{
				webHeaders.RemoveInternal("Connection");
				return;
			}
			string text = value.ToLowerInvariant();
			if (text.Contains("keep-alive") || text.Contains("close"))
			{
				throw new ArgumentException("Keep-Alive and Close may not be set using this property.", "value");
			}
			string value2 = HttpValidationHelpers.CheckBadHeaderValueChars(value);
			webHeaders.CheckUpdate("Connection", value2);
		}
	}

	/// <summary>Gets or sets the name of the connection group for the request.</summary>
	/// <returns>The name of the connection group for this request. The default value is null.</returns>
	public override string ConnectionGroupName
	{
		get
		{
			return connectionGroup;
		}
		set
		{
			connectionGroup = value;
		}
	}

	/// <summary>Gets or sets the Content-length HTTP header.</summary>
	/// <returns>The number of bytes of data to send to the Internet resource. The default is -1, which indicates the property has not been set and that there is no request data to send.</returns>
	/// <exception cref="T:System.InvalidOperationException">The request has been started by calling the <see cref="M:System.Net.HttpWebRequest.GetRequestStream" />, <see cref="M:System.Net.HttpWebRequest.BeginGetRequestStream(System.AsyncCallback,System.Object)" />, <see cref="M:System.Net.HttpWebRequest.GetResponse" />, or <see cref="M:System.Net.HttpWebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" /> method. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The new <see cref="P:System.Net.HttpWebRequest.ContentLength" /> value is less than 0. </exception>
	public override long ContentLength
	{
		get
		{
			return contentLength;
		}
		set
		{
			CheckRequestStarted();
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value", "Content-Length must be >= 0");
			}
			contentLength = value;
			haveContentLength = true;
		}
	}

	internal long InternalContentLength
	{
		set
		{
			contentLength = value;
		}
	}

	internal bool ThrowOnError { get; set; }

	/// <summary>Gets or sets the value of the Content-type HTTP header.</summary>
	/// <returns>The value of the Content-type HTTP header. The default value is null.</returns>
	public override string ContentType
	{
		get
		{
			return webHeaders["Content-Type"];
		}
		set
		{
			SetSpecialHeaders("Content-Type", value);
		}
	}

	/// <summary>Gets or sets the delegate method called when an HTTP 100-continue response is received from the Internet resource.</summary>
	/// <returns>A delegate that implements the callback method that executes when an HTTP Continue response is returned from the Internet resource. The default value is null.</returns>
	public HttpContinueDelegate ContinueDelegate
	{
		get
		{
			return continueDelegate;
		}
		set
		{
			continueDelegate = value;
		}
	}

	/// <summary>Gets or sets the cookies associated with the request.</summary>
	/// <returns>A <see cref="T:System.Net.CookieContainer" /> that contains the cookies associated with this request.</returns>
	public virtual CookieContainer CookieContainer
	{
		get
		{
			return cookieContainer;
		}
		set
		{
			cookieContainer = value;
		}
	}

	/// <summary>Gets or sets authentication information for the request.</summary>
	/// <returns>An <see cref="T:System.Net.ICredentials" /> that contains the authentication credentials associated with the request. The default is null.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override ICredentials Credentials
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

	/// <summary>Get or set the Date HTTP header value to use in an HTTP request.</summary>
	/// <returns>The Date header value in the HTTP request.</returns>
	public DateTime Date
	{
		get
		{
			string text = webHeaders["Date"];
			if (text == null)
			{
				return DateTime.MinValue;
			}
			return DateTime.ParseExact(text, "r", CultureInfo.InvariantCulture).ToLocalTime();
		}
		set
		{
			SetDateHeaderHelper("Date", value);
		}
	}

	/// <summary>Gets or sets the default cache policy for this request.</summary>
	/// <returns>A <see cref="T:System.Net.Cache.HttpRequestCachePolicy" /> that specifies the cache policy in effect for this request when no other policy is applicable.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	///   <IPermission class="System.Net.WebPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoTODO]
	public new static RequestCachePolicy DefaultCachePolicy
	{
		get
		{
			return defaultCachePolicy;
		}
		set
		{
			defaultCachePolicy = value;
		}
	}

	/// <summary>Gets or sets the default maximum length of an HTTP error response.</summary>
	/// <returns>An integer that represents the default maximum length of an HTTP error response.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value is less than 0 and is not equal to -1. </exception>
	[System.MonoTODO]
	public static int DefaultMaximumErrorResponseLength
	{
		get
		{
			return defaultMaximumErrorResponseLength;
		}
		set
		{
			defaultMaximumErrorResponseLength = value;
		}
	}

	/// <summary>Gets or sets the value of the Expect HTTP header.</summary>
	/// <returns>The contents of the Expect HTTP header. The default value is null.NoteThe value for this property is stored in <see cref="T:System.Net.WebHeaderCollection" />. If WebHeaderCollection is set, the property value is lost.</returns>
	/// <exception cref="T:System.ArgumentException">Expect is set to a string that contains "100-continue" as a substring. </exception>
	public string Expect
	{
		get
		{
			return webHeaders["Expect"];
		}
		set
		{
			CheckRequestStarted();
			string text = value;
			if (text != null)
			{
				text = text.Trim().ToLower();
			}
			if (text == null || text.Length == 0)
			{
				webHeaders.RemoveInternal("Expect");
				return;
			}
			if (text == "100-continue")
			{
				throw new ArgumentException("100-Continue cannot be set with this property.", "value");
			}
			webHeaders.CheckUpdate("Expect", value);
		}
	}

	/// <summary>Gets a value that indicates whether a response has been received from an Internet resource.</summary>
	/// <returns>true if a response has been received; otherwise, false.</returns>
	public virtual bool HaveResponse => haveResponse;

	/// <summary>Specifies a collection of the name/value pairs that make up the HTTP headers.</summary>
	/// <returns>A <see cref="T:System.Net.WebHeaderCollection" /> that contains the name/value pairs that make up the headers for the HTTP request.</returns>
	/// <exception cref="T:System.InvalidOperationException">The request has been started by calling the <see cref="M:System.Net.HttpWebRequest.GetRequestStream" />, <see cref="M:System.Net.HttpWebRequest.BeginGetRequestStream(System.AsyncCallback,System.Object)" />, <see cref="M:System.Net.HttpWebRequest.GetResponse" />, or <see cref="M:System.Net.HttpWebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" /> method. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override WebHeaderCollection Headers
	{
		get
		{
			return webHeaders;
		}
		set
		{
			CheckRequestStarted();
			WebHeaderCollection webHeaderCollection = new WebHeaderCollection(WebHeaderCollectionType.HttpWebRequest);
			string[] allKeys = value.AllKeys;
			foreach (string name in allKeys)
			{
				webHeaderCollection.Add(name, value[name]);
			}
			webHeaders = webHeaderCollection;
		}
	}

	/// <summary>Get or set the Host header value to use in an HTTP request independent from the request URI.</summary>
	/// <returns>The Host header value in the HTTP request.</returns>
	/// <exception cref="T:System.ArgumentNullException">The Host header cannot be set to null. </exception>
	/// <exception cref="T:System.ArgumentException">The Host header cannot be set to an invalid value. </exception>
	/// <exception cref="T:System.InvalidOperationException">The Host header cannot be set after the <see cref="T:System.Net.HttpWebRequest" /> has already started to be sent. </exception>
	public string Host
	{
		get
		{
			Uri uri = hostUri ?? Address;
			if ((!(hostUri == null) && hostHasPort) || !Address.IsDefaultPort)
			{
				return uri.Host + ":" + uri.Port;
			}
			return uri.Host;
		}
		set
		{
			CheckRequestStarted();
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value.IndexOf('/') != -1 || !TryGetHostUri(value, out var uri))
			{
				throw new ArgumentException("The specified value is not a valid Host header string.", "value");
			}
			hostUri = uri;
			if (!hostUri.IsDefaultPort)
			{
				hostHasPort = true;
				return;
			}
			if (value.IndexOf(':') == -1)
			{
				hostHasPort = false;
				return;
			}
			int num = value.IndexOf(']');
			hostHasPort = num == -1 || value.LastIndexOf(':') > num;
		}
	}

	/// <summary>Gets or sets the value of the If-Modified-Since HTTP header.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> that contains the contents of the If-Modified-Since HTTP header. The default value is the current date and time.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public DateTime IfModifiedSince
	{
		get
		{
			string text = webHeaders["If-Modified-Since"];
			if (text == null)
			{
				return DateTime.Now;
			}
			try
			{
				return MonoHttpDate.Parse(text);
			}
			catch (Exception)
			{
				return DateTime.Now;
			}
		}
		set
		{
			CheckRequestStarted();
			webHeaders.SetInternal("If-Modified-Since", value.ToUniversalTime().ToString("r", null));
		}
	}

	/// <summary>Gets or sets a value that indicates whether to make a persistent connection to the Internet resource.</summary>
	/// <returns>true if the request to the Internet resource should contain a Connection HTTP header with the value Keep-alive; otherwise, false. The default is true.</returns>
	public bool KeepAlive
	{
		get
		{
			return keepAlive;
		}
		set
		{
			keepAlive = value;
		}
	}

	/// <summary>Gets or sets the maximum number of redirects that the request follows.</summary>
	/// <returns>The maximum number of redirection responses that the request follows. The default value is 50.</returns>
	/// <exception cref="T:System.ArgumentException">The value is set to 0 or less. </exception>
	public int MaximumAutomaticRedirections
	{
		get
		{
			return maxAutoRedirect;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentException("Must be > 0", "value");
			}
			maxAutoRedirect = value;
		}
	}

	/// <summary>Gets or sets the maximum allowed length of the response headers.</summary>
	/// <returns>The length, in kilobytes (1024 bytes), of the response headers.</returns>
	/// <exception cref="T:System.InvalidOperationException">The property is set after the request has already been submitted. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value is less than 0 and is not equal to -1. </exception>
	[System.MonoTODO("Use this")]
	public int MaximumResponseHeadersLength
	{
		get
		{
			return maxResponseHeadersLength;
		}
		set
		{
			CheckRequestStarted();
			if (value < 0 && value != -1)
			{
				throw new ArgumentOutOfRangeException("value", "The specified value must be greater than 0.");
			}
			maxResponseHeadersLength = value;
		}
	}

	/// <summary>Gets or sets the default for the <see cref="P:System.Net.HttpWebRequest.MaximumResponseHeadersLength" /> property.</summary>
	/// <returns>The length, in kilobytes (1024 bytes), of the default maximum for response headers received. The default configuration file sets this value to 64 kilobytes.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value is not equal to -1 and is less than zero. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Net.WebPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoTODO("Use this")]
	public static int DefaultMaximumResponseHeadersLength
	{
		get
		{
			return defaultMaxResponseHeadersLength;
		}
		set
		{
			defaultMaxResponseHeadersLength = value;
		}
	}

	/// <summary>Gets or sets a time-out in milliseconds when writing to or reading from a stream.</summary>
	/// <returns>The number of milliseconds before the writing or reading times out. The default value is 300,000 milliseconds (5 minutes).</returns>
	/// <exception cref="T:System.InvalidOperationException">The request has already been sent. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value specified for a set operation is less than or equal to zero and is not equal to <see cref="F:System.Threading.Timeout.Infinite" /></exception>
	public int ReadWriteTimeout
	{
		get
		{
			return readWriteTimeout;
		}
		set
		{
			CheckRequestStarted();
			if (value <= 0 && value != -1)
			{
				throw new ArgumentOutOfRangeException("value", "Timeout can be only be set to 'System.Threading.Timeout.Infinite' or a value > 0.");
			}
			readWriteTimeout = value;
		}
	}

	/// <summary>Gets or sets a timeout, in seconds, to wait for the server status after 100-continue is received.</summary>
	/// <returns>Returns <see cref="T:System.Int32" />.The timeout, in seconds, to wait for the server status after 100-continue is received.</returns>
	[System.MonoTODO]
	public int ContinueTimeout
	{
		get
		{
			return continueTimeout;
		}
		set
		{
			CheckRequestStarted();
			if (value < 0 && value != -1)
			{
				throw new ArgumentOutOfRangeException("value", "Timeout can be only be set to 'System.Threading.Timeout.Infinite' or a value >= 0.");
			}
			continueTimeout = value;
		}
	}

	/// <summary>Gets or sets the media type of the request.</summary>
	/// <returns>The media type of the request. The default value is null.</returns>
	public string MediaType
	{
		get
		{
			return mediaType;
		}
		set
		{
			mediaType = value;
		}
	}

	/// <summary>Gets or sets the method for the request.</summary>
	/// <returns>The request method to use to contact the Internet resource. The default value is GET.</returns>
	/// <exception cref="T:System.ArgumentException">No method is supplied.-or- The method string contains invalid characters. </exception>
	public override string Method
	{
		get
		{
			return method;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("Cannot set null or blank methods on request.", "value");
			}
			if (HttpValidationHelpers.IsInvalidMethodOrHeaderString(value))
			{
				throw new ArgumentException("Cannot set null or blank methods on request.", "value");
			}
			method = value.ToUpperInvariant();
			if (method != "HEAD" && method != "GET" && method != "POST" && method != "PUT" && method != "DELETE" && method != "CONNECT" && method != "TRACE" && method != "MKCOL")
			{
				method = value;
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether to pipeline the request to the Internet resource.</summary>
	/// <returns>true if the request should be pipelined; otherwise, false. The default is true.</returns>
	public bool Pipelined
	{
		get
		{
			return pipelined;
		}
		set
		{
			pipelined = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether to send an Authorization header with the request.</summary>
	/// <returns>true to send an  HTTP Authorization header with requests after authentication has taken place; otherwise, false. The default is false.</returns>
	public override bool PreAuthenticate
	{
		get
		{
			return preAuthenticate;
		}
		set
		{
			preAuthenticate = value;
		}
	}

	/// <summary>Gets or sets the version of HTTP to use for the request.</summary>
	/// <returns>The HTTP version to use for the request. The default is <see cref="F:System.Net.HttpVersion.Version11" />.</returns>
	/// <exception cref="T:System.ArgumentException">The HTTP version is set to a value other than 1.0 or 1.1. </exception>
	public Version ProtocolVersion
	{
		get
		{
			return version;
		}
		set
		{
			if (value != HttpVersion.Version10 && value != HttpVersion.Version11)
			{
				throw new ArgumentException("Only HTTP/1.0 and HTTP/1.1 version requests are currently supported.", "value");
			}
			force_version = true;
			version = value;
		}
	}

	/// <summary>Gets or sets proxy information for the request.</summary>
	/// <returns>The <see cref="T:System.Net.IWebProxy" /> object to use to proxy the request. The default value is set by calling the <see cref="P:System.Net.GlobalProxySelection.Select" /> property.</returns>
	/// <exception cref="T:System.ArgumentNullException">
	///   <see cref="P:System.Net.HttpWebRequest.Proxy" /> is set to null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The request has been started by calling <see cref="M:System.Net.HttpWebRequest.GetRequestStream" />, <see cref="M:System.Net.HttpWebRequest.BeginGetRequestStream(System.AsyncCallback,System.Object)" />, <see cref="M:System.Net.HttpWebRequest.GetResponse" />, or <see cref="M:System.Net.HttpWebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" />. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have permission for the requested operation. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Net.WebPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override IWebProxy Proxy
	{
		get
		{
			return proxy;
		}
		set
		{
			CheckRequestStarted();
			proxy = value;
			servicePoint = null;
			GetServicePoint();
		}
	}

	/// <summary>Gets or sets the value of the Referer HTTP header.</summary>
	/// <returns>The value of the Referer HTTP header. The default value is null.</returns>
	public string Referer
	{
		get
		{
			return webHeaders["Referer"];
		}
		set
		{
			CheckRequestStarted();
			if (value == null || value.Trim().Length == 0)
			{
				webHeaders.RemoveInternal("Referer");
			}
			else
			{
				webHeaders.SetInternal("Referer", value);
			}
		}
	}

	/// <summary>Gets the original Uniform Resource Identifier (URI) of the request.</summary>
	/// <returns>A <see cref="T:System.Uri" /> that contains the URI of the Internet resource passed to the <see cref="M:System.Net.WebRequest.Create(System.String)" /> method.</returns>
	public override Uri RequestUri => requestUri;

	/// <summary>Gets or sets a value that indicates whether to send data in segments to the Internet resource.</summary>
	/// <returns>true to send data to the Internet resource in segments; otherwise, false. The default value is false.</returns>
	/// <exception cref="T:System.InvalidOperationException">The request has been started by calling the <see cref="M:System.Net.HttpWebRequest.GetRequestStream" />, <see cref="M:System.Net.HttpWebRequest.BeginGetRequestStream(System.AsyncCallback,System.Object)" />, <see cref="M:System.Net.HttpWebRequest.GetResponse" />, or <see cref="M:System.Net.HttpWebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" /> method. </exception>
	public bool SendChunked
	{
		get
		{
			return sendChunked;
		}
		set
		{
			CheckRequestStarted();
			sendChunked = value;
		}
	}

	/// <summary>Gets the service point to use for the request.</summary>
	/// <returns>A <see cref="T:System.Net.ServicePoint" /> that represents the network connection to the Internet resource.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public ServicePoint ServicePoint => GetServicePoint();

	internal ServicePoint ServicePointNoLock => servicePoint;

	/// <summary>Gets a value that indicates whether the request provides support for a <see cref="T:System.Net.CookieContainer" />.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if a <see cref="T:System.Net.CookieContainer" /> is supported; otherwise, false. </returns>
	public virtual bool SupportsCookieContainer => true;

	/// <summary>Gets or sets the time-out value in milliseconds for the <see cref="M:System.Net.HttpWebRequest.GetResponse" /> and <see cref="M:System.Net.HttpWebRequest.GetRequestStream" /> methods.</summary>
	/// <returns>The number of milliseconds to wait before the request times out. The default value is 100,000 milliseconds (100 seconds).</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value specified is less than zero and is not <see cref="F:System.Threading.Timeout.Infinite" />.</exception>
	public override int Timeout
	{
		get
		{
			return timeout;
		}
		set
		{
			if (value < -1)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			timeout = value;
		}
	}

	/// <summary>Gets or sets the value of the Transfer-encoding HTTP header.</summary>
	/// <returns>The value of the Transfer-encoding HTTP header. The default value is null.</returns>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="P:System.Net.HttpWebRequest.TransferEncoding" /> is set when <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is false. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <see cref="P:System.Net.HttpWebRequest.TransferEncoding" /> is set to the value "Chunked". </exception>
	public string TransferEncoding
	{
		get
		{
			return webHeaders["Transfer-Encoding"];
		}
		set
		{
			CheckRequestStarted();
			if (string.IsNullOrWhiteSpace(value))
			{
				webHeaders.RemoveInternal("Transfer-Encoding");
				return;
			}
			if (value.ToLower().Contains("chunked"))
			{
				throw new ArgumentException("Chunked encoding must be set via the SendChunked property.", "value");
			}
			if (!SendChunked)
			{
				throw new InvalidOperationException("TransferEncoding requires the SendChunked property to be set to true.");
			}
			string value2 = HttpValidationHelpers.CheckBadHeaderValueChars(value);
			webHeaders.CheckUpdate("Transfer-Encoding", value2);
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that controls whether default credentials are sent with requests.</summary>
	/// <returns>true if the default credentials are used; otherwise false. The default value is false.</returns>
	/// <exception cref="T:System.InvalidOperationException">You attempted to set this property after the request was sent.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="USERNAME" />
	/// </PermissionSet>
	public override bool UseDefaultCredentials
	{
		get
		{
			return CredentialCache.DefaultCredentials == Credentials;
		}
		set
		{
			Credentials = (value ? CredentialCache.DefaultCredentials : null);
		}
	}

	/// <summary>Gets or sets the value of the User-agent HTTP header.</summary>
	/// <returns>The value of the User-agent HTTP header. The default value is null.NoteThe value for this property is stored in <see cref="T:System.Net.WebHeaderCollection" />. If WebHeaderCollection is set, the property value is lost.</returns>
	public string UserAgent
	{
		get
		{
			return webHeaders["User-Agent"];
		}
		set
		{
			webHeaders.SetInternal("User-Agent", value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether to allow high-speed NTLM-authenticated connection sharing.</summary>
	/// <returns>true to keep the authenticated connection open; otherwise, false.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Net.WebPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public bool UnsafeAuthenticatedConnectionSharing
	{
		get
		{
			return unsafe_auth_blah;
		}
		set
		{
			unsafe_auth_blah = value;
		}
	}

	internal bool GotRequestStream => gotRequestStream;

	internal bool ExpectContinue
	{
		get
		{
			return expectContinue;
		}
		set
		{
			expectContinue = value;
		}
	}

	internal Uri AuthUri => actualUri;

	internal bool ProxyQuery
	{
		get
		{
			if (servicePoint.UsesProxy)
			{
				return !servicePoint.UseConnect;
			}
			return false;
		}
	}

	internal ServerCertValidationCallback ServerCertValidationCallback => certValidationCallback;

	/// <summary>Gets or sets a callback function to validate the server certificate.</summary>
	/// <returns>Returns <see cref="T:System.Net.Security.RemoteCertificateValidationCallback" />.A callback function to validate the server certificate.</returns>
	public RemoteCertificateValidationCallback ServerCertificateValidationCallback
	{
		get
		{
			if (certValidationCallback == null)
			{
				return null;
			}
			return certValidationCallback.ValidationCallback;
		}
		set
		{
			if (value == null)
			{
				certValidationCallback = null;
			}
			else
			{
				certValidationCallback = new ServerCertValidationCallback(value);
			}
		}
	}

	internal bool FinishedReading
	{
		get
		{
			return finished_reading;
		}
		set
		{
			finished_reading = value;
		}
	}

	internal bool Aborted => Interlocked.CompareExchange(ref aborted, 0, 0) == 1;

	internal bool ReuseConnection { get; set; }

	static HttpWebRequest()
	{
		defaultMaxResponseHeadersLength = 64;
		defaultMaximumErrorResponseLength = 64;
		defaultCachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
		if (ConfigurationSettings.GetConfig("system.net/settings") is NetConfig netConfig)
		{
			defaultMaxResponseHeadersLength = netConfig.MaxResponseHeadersLength;
		}
	}

	internal HttpWebRequest(Uri uri)
	{
		allowAutoRedirect = true;
		allowBuffering = true;
		contentLength = -1L;
		keepAlive = true;
		maxAutoRedirect = 50;
		mediaType = string.Empty;
		method = "GET";
		initialMethod = "GET";
		pipelined = true;
		version = HttpVersion.Version11;
		timeout = 100000;
		continueTimeout = 350;
		locker = new object();
		readWriteTimeout = 300000;
		base._002Ector();
		requestUri = uri;
		actualUri = uri;
		proxy = WebRequest.InternalDefaultWebProxy;
		webHeaders = new WebHeaderCollection(WebHeaderCollectionType.HttpWebRequest);
		ThrowOnError = true;
		ResetAuthorization();
	}

	internal HttpWebRequest(Uri uri, MobileTlsProvider tlsProvider, MonoTlsSettings settings = null)
		: this(uri)
	{
		this.tlsProvider = tlsProvider;
		tlsSettings = settings;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.HttpWebRequest" /> class from the specified instances of the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> and <see cref="T:System.Runtime.Serialization.StreamingContext" /> classes.</summary>
	/// <param name="serializationInfo">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that contains the information required to serialize the new <see cref="T:System.Net.HttpWebRequest" /> object. </param>
	/// <param name="streamingContext">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> object that contains the source and destination of the serialized stream associated with the new <see cref="T:System.Net.HttpWebRequest" /> object. </param>
	[Obsolete("Serialization is obsoleted for this type.  http://go.microsoft.com/fwlink/?linkid=14202")]
	protected HttpWebRequest(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		allowAutoRedirect = true;
		allowBuffering = true;
		contentLength = -1L;
		keepAlive = true;
		maxAutoRedirect = 50;
		mediaType = string.Empty;
		method = "GET";
		initialMethod = "GET";
		pipelined = true;
		version = HttpVersion.Version11;
		timeout = 100000;
		continueTimeout = 350;
		locker = new object();
		readWriteTimeout = 300000;
		base._002Ector();
		throw new SerializationException();
	}

	private void ResetAuthorization()
	{
		auth_state = new AuthorizationState(this, isProxy: false);
		proxy_auth_state = new AuthorizationState(this, isProxy: true);
	}

	private void SetSpecialHeaders(string HeaderName, string value)
	{
		value = WebHeaderCollection.CheckBadChars(value, isHeaderValue: true);
		webHeaders.RemoveInternal(HeaderName);
		if (value.Length != 0)
		{
			webHeaders.AddInternal(HeaderName, value);
		}
	}

	private static Exception GetMustImplement()
	{
		return new NotImplementedException();
	}

	private void SetDateHeaderHelper(string headerName, DateTime dateTime)
	{
		if (dateTime == DateTime.MinValue)
		{
			SetSpecialHeaders(headerName, null);
		}
		else
		{
			SetSpecialHeaders(headerName, HttpProtocolUtils.date2string(dateTime));
		}
	}

	private bool TryGetHostUri(string hostName, out Uri hostUri)
	{
		return Uri.TryCreate(Address.Scheme + "://" + hostName + Address.PathAndQuery, UriKind.Absolute, out hostUri);
	}

	internal ServicePoint GetServicePoint()
	{
		lock (locker)
		{
			if (hostChanged || servicePoint == null)
			{
				servicePoint = ServicePointManager.FindServicePoint(actualUri, proxy);
				hostChanged = false;
			}
		}
		return servicePoint;
	}

	/// <summary>Adds a byte range header to a request for a specific range from the beginning or end of the requested data.</summary>
	/// <param name="range">The starting or ending point of the range. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="rangeSpecifier" /> is invalid. </exception>
	/// <exception cref="T:System.InvalidOperationException">The range header could not be added. </exception>
	public void AddRange(int range)
	{
		AddRange("bytes", (long)range);
	}

	/// <summary>Adds a byte range header to the request for a specified range.</summary>
	/// <param name="from">The position at which to start sending data. </param>
	/// <param name="to">The position at which to stop sending data. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="rangeSpecifier" /> is invalid. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="from" /> is greater than <paramref name="to" />-or- <paramref name="from" /> or <paramref name="to" /> is less than 0. </exception>
	/// <exception cref="T:System.InvalidOperationException">The range header could not be added. </exception>
	public void AddRange(int from, int to)
	{
		AddRange("bytes", (long)from, (long)to);
	}

	/// <summary>Adds a Range header to a request for a specific range from the beginning or end of the requested data.</summary>
	/// <param name="rangeSpecifier">The description of the range. </param>
	/// <param name="range">The starting or ending point of the range. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rangeSpecifier" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="rangeSpecifier" /> is invalid. </exception>
	/// <exception cref="T:System.InvalidOperationException">The range header could not be added. </exception>
	public void AddRange(string rangeSpecifier, int range)
	{
		AddRange(rangeSpecifier, (long)range);
	}

	/// <summary>Adds a range header to a request for a specified range.</summary>
	/// <param name="rangeSpecifier">The description of the range. </param>
	/// <param name="from">The position at which to start sending data. </param>
	/// <param name="to">The position at which to stop sending data. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rangeSpecifier" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="from" /> is greater than <paramref name="to" />-or- <paramref name="from" /> or <paramref name="to" /> is less than 0. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="rangeSpecifier" /> is invalid. </exception>
	/// <exception cref="T:System.InvalidOperationException">The range header could not be added. </exception>
	public void AddRange(string rangeSpecifier, int from, int to)
	{
		AddRange(rangeSpecifier, (long)from, (long)to);
	}

	/// <summary>Adds a byte range header to a request for a specific range from the beginning or end of the requested data.</summary>
	/// <param name="range">The starting or ending point of the range.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="rangeSpecifier" /> is invalid. </exception>
	/// <exception cref="T:System.InvalidOperationException">The range header could not be added. </exception>
	public void AddRange(long range)
	{
		AddRange("bytes", range);
	}

	/// <summary>Adds a byte range header to the request for a specified range.</summary>
	/// <param name="from">The position at which to start sending data.</param>
	/// <param name="to">The position at which to stop sending data.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="rangeSpecifier" /> is invalid. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="from" /> is greater than <paramref name="to" />-or- <paramref name="from" /> or <paramref name="to" /> is less than 0. </exception>
	/// <exception cref="T:System.InvalidOperationException">The range header could not be added. </exception>
	public void AddRange(long from, long to)
	{
		AddRange("bytes", from, to);
	}

	/// <summary>Adds a Range header to a request for a specific range from the beginning or end of the requested data.</summary>
	/// <param name="rangeSpecifier">The description of the range.</param>
	/// <param name="range">The starting or ending point of the range.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rangeSpecifier" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="rangeSpecifier" /> is invalid. </exception>
	/// <exception cref="T:System.InvalidOperationException">The range header could not be added. </exception>
	public void AddRange(string rangeSpecifier, long range)
	{
		if (rangeSpecifier == null)
		{
			throw new ArgumentNullException("rangeSpecifier");
		}
		if (!WebHeaderCollection.IsValidToken(rangeSpecifier))
		{
			throw new ArgumentException("Invalid range specifier", "rangeSpecifier");
		}
		string text = webHeaders["Range"];
		if (text == null)
		{
			text = rangeSpecifier + "=";
		}
		else
		{
			if (string.Compare(text.Substring(0, text.IndexOf('=')), rangeSpecifier, StringComparison.OrdinalIgnoreCase) != 0)
			{
				throw new InvalidOperationException("A different range specifier is already in use");
			}
			text += ",";
		}
		string text2 = range.ToString(CultureInfo.InvariantCulture);
		text = ((range >= 0) ? (text + text2 + "-") : (text + "0" + text2));
		webHeaders.ChangeInternal("Range", text);
	}

	/// <summary>Adds a range header to a request for a specified range.</summary>
	/// <param name="rangeSpecifier">The description of the range.</param>
	/// <param name="from">The position at which to start sending data.</param>
	/// <param name="to">The position at which to stop sending data.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rangeSpecifier" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="from" /> is greater than <paramref name="to" />-or- <paramref name="from" /> or <paramref name="to" /> is less than 0. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="rangeSpecifier" /> is invalid. </exception>
	/// <exception cref="T:System.InvalidOperationException">The range header could not be added. </exception>
	public void AddRange(string rangeSpecifier, long from, long to)
	{
		if (rangeSpecifier == null)
		{
			throw new ArgumentNullException("rangeSpecifier");
		}
		if (!WebHeaderCollection.IsValidToken(rangeSpecifier))
		{
			throw new ArgumentException("Invalid range specifier", "rangeSpecifier");
		}
		if (from > to || from < 0)
		{
			throw new ArgumentOutOfRangeException("from");
		}
		if (to < 0)
		{
			throw new ArgumentOutOfRangeException("to");
		}
		string text = webHeaders["Range"];
		text = ((text != null) ? (text + ",") : (rangeSpecifier + "="));
		text = $"{text}{from}-{to}";
		webHeaders.ChangeInternal("Range", text);
	}

	private WebOperation SendRequest(bool redirecting, BufferOffsetSize writeBuffer, CancellationToken cancellationToken)
	{
		lock (locker)
		{
			WebOperation webOperation;
			if (!redirecting && requestSent)
			{
				webOperation = currentOperation;
				if (webOperation == null)
				{
					throw new InvalidOperationException("Should never happen!");
				}
				return webOperation;
			}
			webOperation = new WebOperation(this, writeBuffer, isNtlmChallenge: false, cancellationToken);
			if (Interlocked.CompareExchange(ref currentOperation, webOperation, null) != null)
			{
				throw new InvalidOperationException("Invalid nested call.");
			}
			requestSent = true;
			if (!redirecting)
			{
				redirects = 0;
			}
			servicePoint = GetServicePoint();
			servicePoint.SendRequest(webOperation, connectionGroup);
			return webOperation;
		}
	}

	private Task<Stream> MyGetRequestStreamAsync(CancellationToken cancellationToken)
	{
		if (Aborted)
		{
			throw CreateRequestAbortedException();
		}
		bool flag = !(method == "GET") && !(method == "CONNECT") && !(method == "HEAD") && !(method == "TRACE");
		if (method == null || !flag)
		{
			throw new ProtocolViolationException("Cannot send a content-body with this verb-type.");
		}
		if (contentLength == -1 && !sendChunked && !allowBuffering && KeepAlive)
		{
			throw new ProtocolViolationException("Content-Length not set");
		}
		string transferEncoding = TransferEncoding;
		if (!sendChunked && transferEncoding != null && transferEncoding.Trim() != "")
		{
			throw new InvalidOperationException("TransferEncoding requires the SendChunked property to be set to true.");
		}
		WebOperation webOperation;
		lock (locker)
		{
			if (getResponseCalled)
			{
				throw new InvalidOperationException("This operation cannot be performed after the request has been submitted.");
			}
			webOperation = currentOperation;
			if (webOperation == null)
			{
				initialMethod = method;
				gotRequestStream = true;
				webOperation = SendRequest(redirecting: false, null, cancellationToken);
			}
		}
		return webOperation.GetRequestStream();
	}

	/// <summary>Begins an asynchronous request for a <see cref="T:System.IO.Stream" /> object to use to write data.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that references the asynchronous request.</returns>
	/// <param name="callback">The <see cref="T:System.AsyncCallback" /> delegate. </param>
	/// <param name="state">The state object for this request. </param>
	/// <exception cref="T:System.Net.ProtocolViolationException">The <see cref="P:System.Net.HttpWebRequest.Method" /> property is GET or HEAD.-or- <see cref="P:System.Net.HttpWebRequest.KeepAlive" /> is true, <see cref="P:System.Net.HttpWebRequest.AllowWriteStreamBuffering" /> is false, <see cref="P:System.Net.HttpWebRequest.ContentLength" /> is -1, <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is false, and <see cref="P:System.Net.HttpWebRequest.Method" /> is POST or PUT. </exception>
	/// <exception cref="T:System.InvalidOperationException">The stream is being used by a previous call to <see cref="M:System.Net.HttpWebRequest.BeginGetRequestStream(System.AsyncCallback,System.Object)" />-or- <see cref="P:System.Net.HttpWebRequest.TransferEncoding" /> is set to a value and <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is false.-or- The thread pool is running out of threads. </exception>
	/// <exception cref="T:System.NotSupportedException">The request cache validator indicated that the response for this request can be served from the cache; however, requests that write data must not use the cache. This exception can occur if you are using a custom cache validator that is incorrectly implemented. </exception>
	/// <exception cref="T:System.Net.WebException">
	///   <see cref="M:System.Net.HttpWebRequest.Abort" /> was previously called. </exception>
	/// <exception cref="T:System.ObjectDisposedException">In a .NET Compact Framework application, a request stream with zero content length was not obtained and closed correctly. For more information about handling zero content length requests, see Network Programming in the .NET Compact Framework.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.DnsPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.WebPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
	{
		return TaskToApm.Begin(RunWithTimeout(MyGetRequestStreamAsync), callback, state);
	}

	/// <summary>Ends an asynchronous request for a <see cref="T:System.IO.Stream" /> object to use to write data.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> to use to write request data.</returns>
	/// <param name="asyncResult">The pending request for a stream. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <exception cref="T:System.IO.IOException">The request did not complete, and no stream is available. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> was not returned by the current instance from a call to <see cref="M:System.Net.HttpWebRequest.BeginGetRequestStream(System.AsyncCallback,System.Object)" />. </exception>
	/// <exception cref="T:System.InvalidOperationException">This method was called previously using <paramref name="asyncResult" />. </exception>
	/// <exception cref="T:System.Net.WebException">
	///   <see cref="M:System.Net.HttpWebRequest.Abort" /> was previously called.-or- An error occurred while processing the request. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override Stream EndGetRequestStream(IAsyncResult asyncResult)
	{
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		try
		{
			return TaskToApm.End<Stream>(asyncResult);
		}
		catch (Exception e)
		{
			throw GetWebException(e);
		}
	}

	/// <summary>Gets a <see cref="T:System.IO.Stream" /> object to use to write request data.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> to use to write request data.</returns>
	/// <exception cref="T:System.Net.ProtocolViolationException">The <see cref="P:System.Net.HttpWebRequest.Method" /> property is GET or HEAD.-or- <see cref="P:System.Net.HttpWebRequest.KeepAlive" /> is true, <see cref="P:System.Net.HttpWebRequest.AllowWriteStreamBuffering" /> is false, <see cref="P:System.Net.HttpWebRequest.ContentLength" /> is -1, <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is false, and <see cref="P:System.Net.HttpWebRequest.Method" /> is POST or PUT. </exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="M:System.Net.HttpWebRequest.GetRequestStream" /> method is called more than once.-or- <see cref="P:System.Net.HttpWebRequest.TransferEncoding" /> is set to a value and <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is false. </exception>
	/// <exception cref="T:System.NotSupportedException">The request cache validator indicated that the response for this request can be served from the cache; however, requests that write data must not use the cache. This exception can occur if you are using a custom cache validator that is incorrectly implemented. </exception>
	/// <exception cref="T:System.Net.WebException">
	///   <see cref="M:System.Net.HttpWebRequest.Abort" /> was previously called.-or- The time-out period for the request expired.-or- An error occurred while processing the request. </exception>
	/// <exception cref="T:System.ObjectDisposedException">In a .NET Compact Framework application, a request stream with zero content length was not obtained and closed correctly. For more information about handling zero content length requests, see Network Programming in the .NET Compact Framework.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.DnsPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.WebPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override Stream GetRequestStream()
	{
		try
		{
			return GetRequestStreamAsync().Result;
		}
		catch (Exception e)
		{
			throw GetWebException(e);
		}
	}

	/// <summary>Gets a <see cref="T:System.IO.Stream" /> object to use to write request data and outputs the <see cref="T:System.Net.TransportContext" /> associated with the stream.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> to use to write request data.</returns>
	/// <param name="context">The <see cref="T:System.Net.TransportContext" /> for the <see cref="T:System.IO.Stream" />.</param>
	/// <exception cref="T:System.Exception">The <see cref="M:System.Net.HttpWebRequest.GetRequestStream" /> method was unable to obtain the <see cref="T:System.IO.Stream" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="M:System.Net.HttpWebRequest.GetRequestStream" /> method is called more than once.-or- <see cref="P:System.Net.HttpWebRequest.TransferEncoding" /> is set to a value and <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is false. </exception>
	/// <exception cref="T:System.NotSupportedException">The request cache validator indicated that the response for this request can be served from the cache; however, requests that write data must not use the cache. This exception can occur if you are using a custom cache validator that is incorrectly implemented. </exception>
	/// <exception cref="T:System.Net.ProtocolViolationException">The <see cref="P:System.Net.HttpWebRequest.Method" /> property is GET or HEAD.-or- <see cref="P:System.Net.HttpWebRequest.KeepAlive" /> is true, <see cref="P:System.Net.HttpWebRequest.AllowWriteStreamBuffering" /> is false, <see cref="P:System.Net.HttpWebRequest.ContentLength" /> is -1, <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is false, and <see cref="P:System.Net.HttpWebRequest.Method" /> is POST or PUT. </exception>
	/// <exception cref="T:System.Net.WebException">
	///   <see cref="M:System.Net.HttpWebRequest.Abort" /> was previously called.-or- The time-out period for the request expired.-or- An error occurred while processing the request. </exception>
	[System.MonoTODO]
	public Stream GetRequestStream(out TransportContext context)
	{
		throw new NotImplementedException();
	}

	public override Task<Stream> GetRequestStreamAsync()
	{
		return RunWithTimeout(MyGetRequestStreamAsync);
	}

	internal static Task<T> RunWithTimeout<T>(Func<CancellationToken, Task<T>> func, int timeout, Action abort, Func<bool> aborted, CancellationToken cancellationToken)
	{
		CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		return RunWithTimeoutWorker(func(cancellationTokenSource.Token), timeout, abort, aborted, cancellationTokenSource);
	}

	private static async Task<T> RunWithTimeoutWorker<T>(Task<T> workerTask, int timeout, Action abort, Func<bool> aborted, CancellationTokenSource cts)
	{
		try
		{
			if (await ServicePointScheduler.WaitAsync(workerTask, timeout).ConfigureAwait(continueOnCapturedContext: false))
			{
				return workerTask.Result;
			}
			try
			{
				cts.Cancel();
				abort();
			}
			catch
			{
			}
			workerTask.ContinueWith((Task<T> t) => t.Exception?.GetHashCode(), TaskContinuationOptions.OnlyOnFaulted);
			throw new WebException("The operation has timed out.", WebExceptionStatus.Timeout);
		}
		catch (Exception e)
		{
			throw GetWebException(e, aborted());
		}
		finally
		{
			cts.Dispose();
		}
	}

	private Task<T> RunWithTimeout<T>(Func<CancellationToken, Task<T>> func)
	{
		CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		return RunWithTimeoutWorker(func(cancellationTokenSource.Token), timeout, Abort, () => Aborted, cancellationTokenSource);
	}

	private async Task<HttpWebResponse> MyGetResponseAsync(CancellationToken cancellationToken)
	{
		if (Aborted)
		{
			throw CreateRequestAbortedException();
		}
		WebCompletionSource completion = new WebCompletionSource();
		WebOperation operation;
		lock (locker)
		{
			getResponseCalled = true;
			WebCompletionSource webCompletionSource = Interlocked.CompareExchange(ref responseTask, completion, null);
			if (webCompletionSource != null)
			{
				webCompletionSource.ThrowOnError();
				if (haveResponse && webCompletionSource.Task.IsCompleted)
				{
					return webResponse;
				}
				throw new InvalidOperationException("Cannot re-call start of asynchronous method while a previous call is still in progress.");
			}
			operation = currentOperation;
			if (currentOperation != null)
			{
				writeStream = currentOperation.WriteStream;
			}
			initialMethod = method;
			operation = SendRequest(redirecting: false, null, cancellationToken);
		}
		while (true)
		{
			WebException throwMe = null;
			HttpWebResponse response = null;
			WebResponseStream stream = null;
			bool redirect = false;
			bool mustReadAll = false;
			WebOperation ntlm = null;
			BufferOffsetSize writeBuffer = null;
			try
			{
				cancellationToken.ThrowIfCancellationRequested();
				writeStream = await operation.GetRequestStreamInternal().ConfigureAwait(continueOnCapturedContext: false);
				await writeStream.WriteRequestAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				stream = await operation.GetResponseStream();
				(response, redirect, mustReadAll, writeBuffer, ntlm) = await GetResponseFromData(stream, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			catch (Exception e)
			{
				throwMe = GetWebException(e);
			}
			lock (locker)
			{
				if (throwMe != null)
				{
					haveResponse = true;
					completion.TrySetException(throwMe);
					throw throwMe;
				}
				if (!redirect)
				{
					haveResponse = true;
					webResponse = response;
					completion.TrySetCompleted();
					return response;
				}
				finished_reading = false;
				haveResponse = false;
				webResponse = null;
				currentOperation = ntlm;
			}
			try
			{
				if (mustReadAll)
				{
					await stream.ReadAllAsync(redirect || ntlm != null, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				operation.Finish(ok: true);
				response.Close();
			}
			catch (Exception e2)
			{
				throwMe = GetWebException(e2);
			}
			lock (locker)
			{
				if (throwMe != null)
				{
					haveResponse = true;
					stream?.Close();
					completion.TrySetException(throwMe);
					throw throwMe;
				}
				operation = ((ntlm != null) ? ntlm : SendRequest(redirecting: true, writeBuffer, cancellationToken));
			}
		}
	}

	private async Task<(HttpWebResponse response, bool redirect, bool mustReadAll, BufferOffsetSize writeBuffer, WebOperation ntlm)> GetResponseFromData(WebResponseStream stream, CancellationToken cancellationToken)
	{
		HttpWebResponse response = new HttpWebResponse(actualUri, method, stream, cookieContainer);
		WebException throwMe = null;
		bool redirect = false;
		bool mustReadAll = false;
		WebOperation item = null;
		Task<BufferOffsetSize> task = null;
		BufferOffsetSize bufferOffsetSize = null;
		lock (locker)
		{
			(redirect, mustReadAll, task, throwMe) = CheckFinalStatus(response);
		}
		if (throwMe != null)
		{
			if (mustReadAll)
			{
				await stream.ReadAllAsync(resending: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			throw throwMe;
		}
		if (task != null)
		{
			bufferOffsetSize = await task.ConfigureAwait(continueOnCapturedContext: false);
		}
		lock (locker)
		{
			bool flag = ProxyQuery && proxy != null && !proxy.IsBypassed(actualUri);
			if (!redirect)
			{
				if ((flag ? proxy_auth_state : auth_state).IsNtlmAuthenticated && response.StatusCode < HttpStatusCode.BadRequest)
				{
					stream.Connection.NtlmAuthenticated = true;
				}
				if (writeStream != null)
				{
					writeStream.KillBuffer();
				}
				return (response: response, redirect: false, mustReadAll: false, writeBuffer: bufferOffsetSize, ntlm: null);
			}
			if (sendChunked)
			{
				sendChunked = false;
				webHeaders.RemoveInternal("Transfer-Encoding");
			}
			item = HandleNtlmAuth(stream, response, bufferOffsetSize, cancellationToken).Item1;
		}
		return (response: response, redirect: true, mustReadAll: mustReadAll, writeBuffer: bufferOffsetSize, ntlm: item);
	}

	internal static Exception FlattenException(Exception e)
	{
		if (e is AggregateException ex)
		{
			AggregateException ex2 = ex.Flatten();
			if (ex2.InnerExceptions.Count == 1)
			{
				return ex2.InnerException;
			}
		}
		return e;
	}

	private WebException GetWebException(Exception e)
	{
		return GetWebException(e, Aborted);
	}

	private static WebException GetWebException(Exception e, bool aborted)
	{
		e = FlattenException(e);
		if (e is WebException ex && (!aborted || ex.Status == WebExceptionStatus.RequestCanceled || ex.Status == WebExceptionStatus.Timeout))
		{
			return ex;
		}
		if (aborted || e is OperationCanceledException || e is ObjectDisposedException)
		{
			return CreateRequestAbortedException();
		}
		return new WebException(e.Message, e, WebExceptionStatus.UnknownError, null);
	}

	internal static WebException CreateRequestAbortedException()
	{
		return new WebException(global::SR.Format("The request was aborted: The request was canceled.", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
	}

	/// <summary>Begins an asynchronous request to an Internet resource.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that references the asynchronous request for a response.</returns>
	/// <param name="callback">The <see cref="T:System.AsyncCallback" /> delegate </param>
	/// <param name="state">The state object for this request. </param>
	/// <exception cref="T:System.InvalidOperationException">The stream is already in use by a previous call to <see cref="M:System.Net.HttpWebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" />-or- <see cref="P:System.Net.HttpWebRequest.TransferEncoding" /> is set to a value and <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is false.-or- The thread pool is running out of threads. </exception>
	/// <exception cref="T:System.Net.ProtocolViolationException">
	///   <see cref="P:System.Net.HttpWebRequest.Method" /> is GET or HEAD, and either <see cref="P:System.Net.HttpWebRequest.ContentLength" /> is greater than zero or <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is true.-or- <see cref="P:System.Net.HttpWebRequest.KeepAlive" /> is true, <see cref="P:System.Net.HttpWebRequest.AllowWriteStreamBuffering" /> is false, and either <see cref="P:System.Net.HttpWebRequest.ContentLength" /> is -1, <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is false and <see cref="P:System.Net.HttpWebRequest.Method" /> is POST or PUT.-or- The <see cref="T:System.Net.HttpWebRequest" /> has an entity body but the <see cref="M:System.Net.HttpWebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" /> method is called without calling the <see cref="M:System.Net.HttpWebRequest.BeginGetRequestStream(System.AsyncCallback,System.Object)" /> method. -or- The <see cref="P:System.Net.HttpWebRequest.ContentLength" /> is greater than zero, but the application does not write all of the promised data.</exception>
	/// <exception cref="T:System.Net.WebException">
	///   <see cref="M:System.Net.HttpWebRequest.Abort" /> was previously called. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.DnsPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.WebPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
	{
		if (Aborted)
		{
			throw CreateRequestAbortedException();
		}
		string transferEncoding = TransferEncoding;
		if (!sendChunked && transferEncoding != null && transferEncoding.Trim() != "")
		{
			throw new InvalidOperationException("TransferEncoding requires the SendChunked property to be set to true.");
		}
		return TaskToApm.Begin(RunWithTimeout(MyGetResponseAsync), callback, state);
	}

	/// <summary>Ends an asynchronous request to an Internet resource.</summary>
	/// <returns>A <see cref="T:System.Net.WebResponse" /> that contains the response from the Internet resource.</returns>
	/// <param name="asyncResult">The pending request for a response. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">This method was called previously using <paramref name="asyncResult." />-or- The <see cref="P:System.Net.HttpWebRequest.ContentLength" /> property is greater than 0 but the data has not been written to the request stream. </exception>
	/// <exception cref="T:System.Net.WebException">
	///   <see cref="M:System.Net.HttpWebRequest.Abort" /> was previously called.-or- An error occurred while processing the request. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> was not returned by the current instance from a call to <see cref="M:System.Net.HttpWebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" />. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override WebResponse EndGetResponse(IAsyncResult asyncResult)
	{
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		try
		{
			return TaskToApm.End<HttpWebResponse>(asyncResult);
		}
		catch (Exception e)
		{
			throw GetWebException(e);
		}
	}

	/// <summary>Ends an asynchronous request for a <see cref="T:System.IO.Stream" /> object to use to write data and outputs the <see cref="T:System.Net.TransportContext" /> associated with the stream.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> to use to write request data.</returns>
	/// <param name="asyncResult">The pending request for a stream.</param>
	/// <param name="context">The <see cref="T:System.Net.TransportContext" /> for the <see cref="T:System.IO.Stream" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> was not returned by the current instance from a call to <see cref="M:System.Net.HttpWebRequest.BeginGetRequestStream(System.AsyncCallback,System.Object)" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">This method was called previously using <paramref name="asyncResult" />. </exception>
	/// <exception cref="T:System.IO.IOException">The request did not complete, and no stream is available. </exception>
	/// <exception cref="T:System.Net.WebException">
	///   <see cref="M:System.Net.HttpWebRequest.Abort" /> was previously called.-or- An error occurred while processing the request. </exception>
	public Stream EndGetRequestStream(IAsyncResult asyncResult, out TransportContext context)
	{
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		context = null;
		return EndGetRequestStream(asyncResult);
	}

	/// <summary>Returns a response from an Internet resource.</summary>
	/// <returns>A <see cref="T:System.Net.WebResponse" /> that contains the response from the Internet resource.</returns>
	/// <exception cref="T:System.InvalidOperationException">The stream is already in use by a previous call to <see cref="M:System.Net.HttpWebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" />.-or- <see cref="P:System.Net.HttpWebRequest.TransferEncoding" /> is set to a value and <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is false. </exception>
	/// <exception cref="T:System.Net.ProtocolViolationException">
	///   <see cref="P:System.Net.HttpWebRequest.Method" /> is GET or HEAD, and either <see cref="P:System.Net.HttpWebRequest.ContentLength" /> is greater or equal to zero or <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is true.-or- <see cref="P:System.Net.HttpWebRequest.KeepAlive" /> is true, <see cref="P:System.Net.HttpWebRequest.AllowWriteStreamBuffering" /> is false, <see cref="P:System.Net.HttpWebRequest.ContentLength" /> is -1, <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is false, and <see cref="P:System.Net.HttpWebRequest.Method" /> is POST or PUT. -or- The <see cref="T:System.Net.HttpWebRequest" /> has an entity body but the <see cref="M:System.Net.HttpWebRequest.GetResponse" /> method is called without calling the <see cref="M:System.Net.HttpWebRequest.GetRequestStream" /> method. -or- The <see cref="P:System.Net.HttpWebRequest.ContentLength" /> is greater than zero, but the application does not write all of the promised data.</exception>
	/// <exception cref="T:System.NotSupportedException">The request cache validator indicated that the response for this request can be served from the cache; however, this request includes data to be sent to the server. Requests that send data must not use the cache. This exception can occur if you are using a custom cache validator that is incorrectly implemented. </exception>
	/// <exception cref="T:System.Net.WebException">
	///   <see cref="M:System.Net.HttpWebRequest.Abort" /> was previously called.-or- The time-out period for the request expired.-or- An error occurred while processing the request. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.DnsPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.WebPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override WebResponse GetResponse()
	{
		try
		{
			return GetResponseAsync().Result;
		}
		catch (Exception e)
		{
			throw GetWebException(e);
		}
	}

	/// <summary>Cancels a request to an Internet resource.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override void Abort()
	{
		if (Interlocked.CompareExchange(ref aborted, 1, 0) == 1)
		{
			return;
		}
		haveResponse = true;
		currentOperation?.Abort();
		responseTask?.TrySetCanceled();
		if (webResponse == null)
		{
			return;
		}
		try
		{
			webResponse.Close();
			webResponse = null;
		}
		catch
		{
		}
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data needed to serialize the target object.</summary>
	/// <param name="serializationInfo">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data. </param>
	/// <param name="streamingContext">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> that specifies the destination for this serialization.</param>
	void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		throw new SerializationException();
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data required to serialize the target object.</summary>
	/// <param name="serializationInfo">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data. </param>
	/// <param name="streamingContext">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> that specifies the destination for this serialization.</param>
	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	protected override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		throw new SerializationException();
	}

	private void CheckRequestStarted()
	{
		if (requestSent)
		{
			throw new InvalidOperationException("request started");
		}
	}

	internal void DoContinueDelegate(int statusCode, WebHeaderCollection headers)
	{
		if (continueDelegate != null)
		{
			continueDelegate(statusCode, headers);
		}
	}

	private void RewriteRedirectToGet()
	{
		method = "GET";
		webHeaders.RemoveInternal("Transfer-Encoding");
		sendChunked = false;
	}

	private bool Redirect(HttpStatusCode code, WebResponse response)
	{
		redirects++;
		Exception ex = null;
		string text = null;
		switch (code)
		{
		case HttpStatusCode.MultipleChoices:
			ex = new WebException("Ambiguous redirect.");
			break;
		case HttpStatusCode.MovedPermanently:
		case HttpStatusCode.Found:
			if (method == "POST")
			{
				RewriteRedirectToGet();
			}
			break;
		case HttpStatusCode.SeeOther:
			RewriteRedirectToGet();
			break;
		case HttpStatusCode.NotModified:
			return false;
		case HttpStatusCode.UseProxy:
			ex = new NotImplementedException("Proxy support not available.");
			break;
		default:
		{
			int num = (int)code;
			ex = new ProtocolViolationException("Invalid status code: " + num);
			break;
		}
		case HttpStatusCode.TemporaryRedirect:
			break;
		}
		if (method != "GET" && !InternalAllowBuffering && ResendContentFactory == null && (writeStream.WriteBufferLength > 0 || contentLength > 0))
		{
			ex = new WebException("The request requires buffering data to succeed.", null, WebExceptionStatus.ProtocolError, response);
		}
		if (ex != null)
		{
			throw ex;
		}
		if (AllowWriteStreamBuffering || method == "GET")
		{
			contentLength = -1L;
		}
		text = response.Headers["Location"];
		if (text == null)
		{
			throw new WebException($"No Location header found for {(int)code}", null, WebExceptionStatus.ProtocolError, response);
		}
		Uri uri = actualUri;
		try
		{
			actualUri = new Uri(actualUri, text);
		}
		catch (Exception)
		{
			throw new WebException($"Invalid URL ({text}) for {(int)code}", null, WebExceptionStatus.ProtocolError, response);
		}
		hostChanged = actualUri.Scheme != uri.Scheme || Host != uri.Authority;
		return true;
	}

	private string GetHeaders()
	{
		bool flag = false;
		if (sendChunked)
		{
			flag = true;
			webHeaders.ChangeInternal("Transfer-Encoding", "chunked");
			webHeaders.RemoveInternal("Content-Length");
		}
		else if (contentLength != -1)
		{
			if (auth_state.NtlmAuthState == NtlmAuthState.Challenge || proxy_auth_state.NtlmAuthState == NtlmAuthState.Challenge)
			{
				if (haveContentLength || gotRequestStream || contentLength > 0)
				{
					webHeaders.SetInternal("Content-Length", "0");
				}
				else
				{
					webHeaders.RemoveInternal("Content-Length");
				}
			}
			else
			{
				if (contentLength > 0)
				{
					flag = true;
				}
				if (haveContentLength || gotRequestStream || contentLength > 0)
				{
					webHeaders.SetInternal("Content-Length", contentLength.ToString());
				}
			}
			webHeaders.RemoveInternal("Transfer-Encoding");
		}
		else
		{
			webHeaders.RemoveInternal("Content-Length");
		}
		if (actualVersion == HttpVersion.Version11 && flag && servicePoint.SendContinue)
		{
			webHeaders.ChangeInternal("Expect", "100-continue");
			expectContinue = true;
		}
		else
		{
			webHeaders.RemoveInternal("Expect");
			expectContinue = false;
		}
		bool proxyQuery = ProxyQuery;
		string name = (proxyQuery ? "Proxy-Connection" : "Connection");
		webHeaders.RemoveInternal((!proxyQuery) ? "Proxy-Connection" : "Connection");
		Version protocolVersion = servicePoint.ProtocolVersion;
		bool flag2 = protocolVersion == null || protocolVersion == HttpVersion.Version10;
		if (keepAlive && (version == HttpVersion.Version10 || flag2))
		{
			if (webHeaders[name] == null || webHeaders[name].IndexOf("keep-alive", StringComparison.OrdinalIgnoreCase) == -1)
			{
				webHeaders.ChangeInternal(name, "keep-alive");
			}
		}
		else if (!keepAlive && version == HttpVersion.Version11)
		{
			webHeaders.ChangeInternal(name, "close");
		}
		string value = ((hostUri != null) ? ((!hostHasPort) ? hostUri.GetComponents(UriComponents.Host, UriFormat.Unescaped) : hostUri.GetComponents(UriComponents.HostAndPort, UriFormat.Unescaped)) : ((!Address.IsDefaultPort) ? Address.GetComponents(UriComponents.HostAndPort, UriFormat.Unescaped) : Address.GetComponents(UriComponents.Host, UriFormat.Unescaped)));
		webHeaders.SetInternal("Host", value);
		if (cookieContainer != null)
		{
			string cookieHeader = cookieContainer.GetCookieHeader(actualUri);
			if (cookieHeader != "")
			{
				webHeaders.ChangeInternal("Cookie", cookieHeader);
			}
			else
			{
				webHeaders.RemoveInternal("Cookie");
			}
		}
		string text = null;
		if ((auto_decomp & DecompressionMethods.GZip) != DecompressionMethods.None)
		{
			text = "gzip";
		}
		if ((auto_decomp & DecompressionMethods.Deflate) != DecompressionMethods.None)
		{
			text = ((text != null) ? "gzip, deflate" : "deflate");
		}
		if (text != null)
		{
			webHeaders.ChangeInternal("Accept-Encoding", text);
		}
		if (!usedPreAuth && preAuthenticate)
		{
			DoPreAuthenticate();
		}
		return webHeaders.ToString();
	}

	private void DoPreAuthenticate()
	{
		bool flag = proxy != null && !proxy.IsBypassed(actualUri);
		ICredentials credentials = ((!flag || this.credentials != null) ? this.credentials : proxy.Credentials);
		Authorization authorization = AuthenticationManager.PreAuthenticate(this, credentials);
		if (authorization != null)
		{
			webHeaders.RemoveInternal("Proxy-Authorization");
			webHeaders.RemoveInternal("Authorization");
			string name = ((flag && this.credentials == null) ? "Proxy-Authorization" : "Authorization");
			webHeaders[name] = authorization.Message;
			usedPreAuth = true;
		}
	}

	internal byte[] GetRequestHeaders()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = (ProxyQuery ? $"{actualUri.Scheme}://{Host}{actualUri.PathAndQuery}" : actualUri.PathAndQuery);
		if (!force_version && servicePoint.ProtocolVersion != null && servicePoint.ProtocolVersion < version)
		{
			actualVersion = servicePoint.ProtocolVersion;
		}
		else
		{
			actualVersion = version;
		}
		stringBuilder.AppendFormat("{0} {1} HTTP/{2}.{3}\r\n", method, text, actualVersion.Major, actualVersion.Minor);
		stringBuilder.Append(GetHeaders());
		string s = stringBuilder.ToString();
		return Encoding.UTF8.GetBytes(s);
	}

	private (WebOperation, bool) HandleNtlmAuth(WebResponseStream stream, HttpWebResponse response, BufferOffsetSize writeBuffer, CancellationToken cancellationToken)
	{
		bool flag = response.StatusCode == HttpStatusCode.ProxyAuthenticationRequired;
		if ((flag ? proxy_auth_state : auth_state).NtlmAuthState == NtlmAuthState.None)
		{
			return (null, false);
		}
		bool flag2 = auth_state.NtlmAuthState == NtlmAuthState.Challenge || proxy_auth_state.NtlmAuthState == NtlmAuthState.Challenge;
		WebOperation webOperation = new WebOperation(this, writeBuffer, flag2, cancellationToken);
		stream.Operation.SetPriorityRequest(webOperation);
		ICredentials credentials = ((!flag || proxy == null) ? this.credentials : proxy.Credentials);
		if (credentials != null)
		{
			stream.Connection.NtlmCredential = credentials.GetCredential(requestUri, "NTLM");
			stream.Connection.UnsafeAuthenticatedConnectionSharing = unsafe_auth_blah;
		}
		return (webOperation, flag2);
	}

	private bool CheckAuthorization(WebResponse response, HttpStatusCode code)
	{
		if (code != HttpStatusCode.ProxyAuthenticationRequired)
		{
			return auth_state.CheckAuthorization(response, code);
		}
		return proxy_auth_state.CheckAuthorization(response, code);
	}

	private (Task<BufferOffsetSize> task, WebException throwMe) GetRewriteHandler(HttpWebResponse response, bool redirect)
	{
		if (redirect)
		{
			if (!MethodWithBuffer)
			{
				return (task: null, throwMe: null);
			}
			if (writeStream.WriteBufferLength == 0 || contentLength == 0L)
			{
				return (task: null, throwMe: null);
			}
		}
		if (AllowWriteStreamBuffering)
		{
			return (task: Task.FromResult(writeStream.GetWriteBuffer()), throwMe: null);
		}
		if (ResendContentFactory == null)
		{
			return (task: null, throwMe: new WebException("The request requires buffering data to succeed.", null, WebExceptionStatus.ProtocolError, response));
		}
		return (task: ((Func<Task<BufferOffsetSize>>)async delegate
		{
			using MemoryStream ms = new MemoryStream();
			await ResendContentFactory(ms).ConfigureAwait(continueOnCapturedContext: false);
			byte[] array = ms.ToArray();
			return new BufferOffsetSize(array, 0, array.Length, copyBuffer: false);
		})(), throwMe: null);
	}

	private (bool redirect, bool mustReadAll, Task<BufferOffsetSize> writeBuffer, WebException throwMe) CheckFinalStatus(HttpWebResponse response)
	{
		WebException ex = null;
		bool item = false;
		HttpStatusCode httpStatusCode = (HttpStatusCode)0;
		Task<BufferOffsetSize> item2 = null;
		httpStatusCode = response.StatusCode;
		if (((!auth_state.IsCompleted && httpStatusCode == HttpStatusCode.Unauthorized && credentials != null) || (ProxyQuery && !proxy_auth_state.IsCompleted && httpStatusCode == HttpStatusCode.ProxyAuthenticationRequired)) && !usedPreAuth && CheckAuthorization(response, httpStatusCode))
		{
			item = true;
			if (!MethodWithBuffer)
			{
				return (redirect: true, mustReadAll: item, writeBuffer: null, throwMe: null);
			}
			(item2, ex) = GetRewriteHandler(response, redirect: false);
			if (ex == null)
			{
				return (redirect: true, mustReadAll: item, writeBuffer: item2, throwMe: null);
			}
			if (!ThrowOnError)
			{
				return (redirect: false, mustReadAll: item, writeBuffer: null, throwMe: null);
			}
			writeStream.InternalClose();
			writeStream = null;
			response.Close();
			return (redirect: false, mustReadAll: item, writeBuffer: null, throwMe: ex);
		}
		if (httpStatusCode >= HttpStatusCode.BadRequest)
		{
			ex = new WebException($"The remote server returned an error: ({(int)httpStatusCode}) {response.StatusDescription}.", null, WebExceptionStatus.ProtocolError, response);
			item = true;
		}
		else if (httpStatusCode == HttpStatusCode.NotModified && allowAutoRedirect)
		{
			ex = new WebException($"The remote server returned an error: ({(int)httpStatusCode}) {response.StatusDescription}.", null, WebExceptionStatus.ProtocolError, response);
		}
		else if (httpStatusCode >= HttpStatusCode.MultipleChoices && allowAutoRedirect && redirects >= maxAutoRedirect)
		{
			ex = new WebException("Max. redirections exceeded.", null, WebExceptionStatus.ProtocolError, response);
			item = true;
		}
		if (ex == null)
		{
			int num = (int)httpStatusCode;
			bool flag = false;
			if (allowAutoRedirect && num >= 300)
			{
				flag = Redirect(httpStatusCode, response);
				(item2, ex) = GetRewriteHandler(response, redirect: true);
				if (flag && !unsafe_auth_blah)
				{
					auth_state.Reset();
					proxy_auth_state.Reset();
				}
			}
			if (num >= 300 && num != 304)
			{
				item = true;
			}
			if (ex == null)
			{
				return (redirect: flag, mustReadAll: item, writeBuffer: item2, throwMe: null);
			}
		}
		if (!ThrowOnError)
		{
			return (redirect: false, mustReadAll: item, writeBuffer: null, throwMe: null);
		}
		if (writeStream != null)
		{
			writeStream.InternalClose();
			writeStream = null;
		}
		return (redirect: false, mustReadAll: item, writeBuffer: null, throwMe: ex);
	}

	internal static StringBuilder GenerateConnectionGroup(string connectionGroupName, bool unsafeConnectionGroup, bool isInternalGroup)
	{
		StringBuilder stringBuilder = new StringBuilder(connectionGroupName);
		stringBuilder.Append(unsafeConnectionGroup ? "U>" : "S>");
		if (isInternalGroup)
		{
			stringBuilder.Append("I>");
		}
		return stringBuilder;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.HttpWebRequest" /> class.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
	public HttpWebRequest()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
