using System.IO;
using System.Net.Cache;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Unity;

namespace System.Net;

/// <summary>Implements a File Transfer Protocol (FTP) client.</summary>
public sealed class FtpWebRequest : WebRequest
{
	private enum RequestStage
	{
		CheckForError,
		RequestStarted,
		WriteReady,
		ReadReady,
		ReleaseConnection
	}

	private object _syncObject;

	private ICredentials _authInfo;

	private readonly Uri _uri;

	private FtpMethodInfo _methodInfo;

	private string _renameTo;

	private bool _getRequestStreamStarted;

	private bool _getResponseStarted;

	private DateTime _startTime;

	private int _timeout;

	private int _remainingTimeout;

	private long _contentLength;

	private long _contentOffset;

	private X509CertificateCollection _clientCertificates;

	private bool _passive;

	private bool _binary;

	private string _connectionGroupName;

	private ServicePoint _servicePoint;

	private bool _async;

	private bool _aborted;

	private bool _timedOut;

	private Exception _exception;

	private TimerThread.Queue _timerQueue;

	private TimerThread.Callback _timerCallback;

	private bool _enableSsl;

	private FtpControlStream _connection;

	private Stream _stream;

	private RequestStage _requestStage;

	private bool _onceFailed;

	private WebHeaderCollection _ftpRequestHeaders;

	private FtpWebResponse _ftpWebResponse;

	private int _readWriteTimeout;

	private ContextAwareResult _writeAsyncResult;

	private LazyAsyncResult _readAsyncResult;

	private LazyAsyncResult _requestCompleteAsyncResult;

	private static readonly NetworkCredential s_defaultFtpNetworkCredential = new NetworkCredential("anonymous", "anonymous@", string.Empty);

	private const int s_DefaultTimeout = 100000;

	private static readonly TimerThread.Queue s_DefaultTimerQueue = TimerThread.GetOrCreateQueue(100000);

	internal FtpMethodInfo MethodInfo => _methodInfo;

	/// <summary>Defines the default cache policy for all FTP requests.</summary>
	/// <returns>A <see cref="T:System.Net.Cache.RequestCachePolicy" /> that defines the cache policy for FTP requests.</returns>
	/// <exception cref="T:System.ArgumentNullException">The caller tried to set this property to null.</exception>
	public new static RequestCachePolicy DefaultCachePolicy
	{
		get
		{
			return WebRequest.DefaultCachePolicy;
		}
		set
		{
		}
	}

	/// <summary>Gets or sets the command to send to the FTP server.</summary>
	/// <returns>A <see cref="T:System.String" /> value that contains the FTP command to send to the server. The default value is <see cref="F:System.Net.WebRequestMethods.Ftp.DownloadFile" />.</returns>
	/// <exception cref="T:System.InvalidOperationException">A new value was specified for this property for a request that is already in progress. </exception>
	/// <exception cref="T:System.ArgumentException">The method is invalid.- or -The method is not supported.- or -Multiple methods were specified.</exception>
	public override string Method
	{
		get
		{
			return _methodInfo.Method;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("FTP Method names cannot be null or empty.", "value");
			}
			if (InUse)
			{
				throw new InvalidOperationException("This operation cannot be performed after the request has been submitted.");
			}
			try
			{
				_methodInfo = FtpMethodInfo.GetMethodInfo(value);
			}
			catch (ArgumentException)
			{
				throw new ArgumentException("This method is not supported.", "value");
			}
		}
	}

	/// <summary>Gets or sets the new name of a file being renamed.</summary>
	/// <returns>The new name of the file being renamed.</returns>
	/// <exception cref="T:System.ArgumentException">The value specified for a set operation is null or an empty string.</exception>
	/// <exception cref="T:System.InvalidOperationException">A new value was specified for this property for a request that is already in progress. </exception>
	public string RenameTo
	{
		get
		{
			return _renameTo;
		}
		set
		{
			if (InUse)
			{
				throw new InvalidOperationException("This operation cannot be performed after the request has been submitted.");
			}
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("The RenameTo filename cannot be null or empty.", "value");
			}
			_renameTo = value;
		}
	}

	/// <summary>Gets or sets the credentials used to communicate with the FTP server.</summary>
	/// <returns>An <see cref="T:System.Net.ICredentials" /> instance; otherwise, null if the property has not been set.</returns>
	/// <exception cref="T:System.ArgumentNullException">The value specified for a set operation is null.</exception>
	/// <exception cref="T:System.ArgumentException">An <see cref="T:System.Net.ICredentials" /> of a type other than <see cref="T:System.Net.NetworkCredential" /> was specified for a set operation.</exception>
	/// <exception cref="T:System.InvalidOperationException">A new value was specified for this property for a request that is already in progress. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override ICredentials Credentials
	{
		get
		{
			return _authInfo;
		}
		set
		{
			if (InUse)
			{
				throw new InvalidOperationException("This operation cannot be performed after the request has been submitted.");
			}
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value == CredentialCache.DefaultNetworkCredentials)
			{
				throw new ArgumentException("Default credentials are not supported on an FTP request.", "value");
			}
			_authInfo = value;
		}
	}

	/// <summary>Gets the URI requested by this instance.</summary>
	/// <returns>A <see cref="T:System.Uri" /> instance that identifies a resource that is accessed using the File Transfer Protocol.</returns>
	public override Uri RequestUri => _uri;

	/// <summary>Gets or sets the number of milliseconds to wait for a request.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that contains the number of milliseconds to wait before a request times out. The default value is <see cref="F:System.Threading.Timeout.Infinite" />.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value specified is less than zero and is not <see cref="F:System.Threading.Timeout.Infinite" />. </exception>
	/// <exception cref="T:System.InvalidOperationException">A new value was specified for this property for a request that is already in progress. </exception>
	public override int Timeout
	{
		get
		{
			return _timeout;
		}
		set
		{
			if (InUse)
			{
				throw new InvalidOperationException("This operation cannot be performed after the request has been submitted.");
			}
			if (value < 0 && value != -1)
			{
				throw new ArgumentOutOfRangeException("value", "Timeout can be only be set to 'System.Threading.Timeout.Infinite' or a value >= 0.");
			}
			if (_timeout != value)
			{
				_timeout = value;
				_timerQueue = null;
			}
		}
	}

	internal int RemainingTimeout => _remainingTimeout;

	/// <summary>Gets or sets a time-out when reading from or writing to a stream.</summary>
	/// <returns>The number of milliseconds before the reading or writing times out. The default value is 300,000 milliseconds (5 minutes).</returns>
	/// <exception cref="T:System.InvalidOperationException">The request has already been sent. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value specified for a set operation is less than or equal to zero and is not equal to <see cref="F:System.Threading.Timeout.Infinite" />. </exception>
	public int ReadWriteTimeout
	{
		get
		{
			return _readWriteTimeout;
		}
		set
		{
			if (_getResponseStarted)
			{
				throw new InvalidOperationException("This operation cannot be performed after the request has been submitted.");
			}
			if (value <= 0 && value != -1)
			{
				throw new ArgumentOutOfRangeException("value", "Timeout can be only be set to 'System.Threading.Timeout.Infinite' or a value > 0.");
			}
			_readWriteTimeout = value;
		}
	}

	/// <summary>Gets or sets a byte offset into the file being downloaded by this request.</summary>
	/// <returns>An <see cref="T:System.Int64" /> instance that specifies the file offset, in bytes. The default value is zero.</returns>
	/// <exception cref="T:System.InvalidOperationException">A new value was specified for this property for a request that is already in progress. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value specified for this property is less than zero. </exception>
	public long ContentOffset
	{
		get
		{
			return _contentOffset;
		}
		set
		{
			if (InUse)
			{
				throw new InvalidOperationException("This operation cannot be performed after the request has been submitted.");
			}
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_contentOffset = value;
		}
	}

	/// <summary>Gets or sets a value that is ignored by the <see cref="T:System.Net.FtpWebRequest" /> class.</summary>
	/// <returns>An <see cref="T:System.Int64" /> value that should be ignored.</returns>
	public override long ContentLength
	{
		get
		{
			return _contentLength;
		}
		set
		{
			_contentLength = value;
		}
	}

	/// <summary>Gets or sets the proxy used to communicate with the FTP server.</summary>
	/// <returns>An <see cref="T:System.Net.IWebProxy" /> instance responsible for communicating with the FTP server.</returns>
	/// <exception cref="T:System.ArgumentNullException">This property cannot be set to null.</exception>
	/// <exception cref="T:System.InvalidOperationException">A new value was specified for this property for a request that is already in progress. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Net.WebPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override IWebProxy Proxy
	{
		get
		{
			return null;
		}
		set
		{
			if (InUse)
			{
				throw new InvalidOperationException("This operation cannot be performed after the request has been submitted.");
			}
		}
	}

	/// <summary>Gets or sets the name of the connection group that contains the service point used to send the current request.</summary>
	/// <returns>A <see cref="T:System.String" /> value that contains a connection group name.</returns>
	/// <exception cref="T:System.InvalidOperationException">A new value was specified for this property for a request that is already in progress. </exception>
	public override string ConnectionGroupName
	{
		get
		{
			return _connectionGroupName;
		}
		set
		{
			if (InUse)
			{
				throw new InvalidOperationException("This operation cannot be performed after the request has been submitted.");
			}
			_connectionGroupName = value;
		}
	}

	/// <summary>Gets the <see cref="T:System.Net.ServicePoint" /> object used to connect to the FTP server.</summary>
	/// <returns>A <see cref="T:System.Net.ServicePoint" /> object that can be used to customize connection behavior.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public ServicePoint ServicePoint
	{
		get
		{
			if (_servicePoint == null)
			{
				_servicePoint = ServicePointManager.FindServicePoint(_uri);
			}
			return _servicePoint;
		}
	}

	internal bool Aborted => _aborted;

	private TimerThread.Queue TimerQueue
	{
		get
		{
			if (_timerQueue == null)
			{
				_timerQueue = TimerThread.GetOrCreateQueue(RemainingTimeout);
			}
			return _timerQueue;
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that specifies whether the control connection to the FTP server is closed after the request completes.</summary>
	/// <returns>true if the connection to the server should not be destroyed; otherwise, false. The default value is true.</returns>
	/// <exception cref="T:System.InvalidOperationException">A new value was specified for this property for a request that is already in progress. </exception>
	public bool KeepAlive
	{
		get
		{
			return true;
		}
		set
		{
			if (InUse)
			{
				throw new InvalidOperationException("This operation cannot be performed after the request has been submitted.");
			}
		}
	}

	public override RequestCachePolicy CachePolicy
	{
		get
		{
			return DefaultCachePolicy;
		}
		set
		{
			if (InUse)
			{
				throw new InvalidOperationException("This operation cannot be performed after the request has been submitted.");
			}
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that specifies the data type for file transfers.</summary>
	/// <returns>true to indicate to the server that the data to be transferred is binary; false to indicate that the data is text. The default value is true.</returns>
	/// <exception cref="T:System.InvalidOperationException">A new value was specified for this property for a request that is already in progress.</exception>
	public bool UseBinary
	{
		get
		{
			return _binary;
		}
		set
		{
			if (InUse)
			{
				throw new InvalidOperationException("This operation cannot be performed after the request has been submitted.");
			}
			_binary = value;
		}
	}

	/// <summary>Gets or sets the behavior of a client application's data transfer process.</summary>
	/// <returns>false if the client application's data transfer process listens for a connection on the data port; otherwise, true if the client should initiate a connection on the data port. The default value is true.</returns>
	/// <exception cref="T:System.InvalidOperationException">A new value was specified for this property for a request that is already in progress. </exception>
	public bool UsePassive
	{
		get
		{
			return _passive;
		}
		set
		{
			if (InUse)
			{
				throw new InvalidOperationException("This operation cannot be performed after the request has been submitted.");
			}
			_passive = value;
		}
	}

	/// <summary>Gets or sets the certificates used for establishing an encrypted connection to the FTP server.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X509CertificateCollection" /> object that contains the client certificates.</returns>
	/// <exception cref="T:System.ArgumentNullException">The value specified for a set operation is null.</exception>
	public X509CertificateCollection ClientCertificates
	{
		get
		{
			return LazyInitializer.EnsureInitialized(ref _clientCertificates, ref _syncObject, () => new X509CertificateCollection());
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_clientCertificates = value;
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Boolean" /> that specifies that an SSL connection should be used.</summary>
	/// <returns>true if control and data transmissions are encrypted; otherwise, false. The default value is false.</returns>
	/// <exception cref="T:System.InvalidOperationException">The connection to the FTP server has already been established.</exception>
	public bool EnableSsl
	{
		get
		{
			return _enableSsl;
		}
		set
		{
			if (InUse)
			{
				throw new InvalidOperationException("This operation cannot be performed after the request has been submitted.");
			}
			_enableSsl = value;
		}
	}

	/// <summary>Gets an empty <see cref="T:System.Net.WebHeaderCollection" /> object.</summary>
	/// <returns>An empty <see cref="T:System.Net.WebHeaderCollection" /> object.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Net.WebPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override WebHeaderCollection Headers
	{
		get
		{
			if (_ftpRequestHeaders == null)
			{
				_ftpRequestHeaders = new WebHeaderCollection();
			}
			return _ftpRequestHeaders;
		}
		set
		{
			_ftpRequestHeaders = value;
		}
	}

	/// <summary>Always throws a <see cref="T:System.NotSupportedException" />.</summary>
	/// <returns>Always throws a <see cref="T:System.NotSupportedException" />.</returns>
	/// <exception cref="T:System.NotSupportedException">Content type information is not supported for FTP.</exception>
	public override string ContentType
	{
		get
		{
			throw ExceptionHelper.PropertyNotSupportedException;
		}
		set
		{
			throw ExceptionHelper.PropertyNotSupportedException;
		}
	}

	/// <summary>Always throws a <see cref="T:System.NotSupportedException" />.</summary>
	/// <returns>Always throws a <see cref="T:System.NotSupportedException" />.</returns>
	/// <exception cref="T:System.NotSupportedException">Default credentials are not supported for FTP.</exception>
	public override bool UseDefaultCredentials
	{
		get
		{
			throw ExceptionHelper.PropertyNotSupportedException;
		}
		set
		{
			throw ExceptionHelper.PropertyNotSupportedException;
		}
	}

	/// <summary>Always throws a <see cref="T:System.NotSupportedException" />.</summary>
	/// <returns>Always throws a <see cref="T:System.NotSupportedException" />.</returns>
	/// <exception cref="T:System.NotSupportedException">Preauthentication is not supported for FTP.</exception>
	public override bool PreAuthenticate
	{
		get
		{
			throw ExceptionHelper.PropertyNotSupportedException;
		}
		set
		{
			throw ExceptionHelper.PropertyNotSupportedException;
		}
	}

	private bool InUse
	{
		get
		{
			if (_getRequestStreamStarted || _getResponseStarted)
			{
				return true;
			}
			return false;
		}
	}

	internal FtpWebRequest(Uri uri)
	{
		_timeout = 100000;
		_passive = true;
		_binary = true;
		_timerQueue = s_DefaultTimerQueue;
		_readWriteTimeout = 300000;
		base._002Ector();
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Info(this, uri, ".ctor");
		}
		if ((object)uri.Scheme != Uri.UriSchemeFtp)
		{
			throw new ArgumentOutOfRangeException("uri");
		}
		_timerCallback = TimerCallback;
		_syncObject = new object();
		NetworkCredential networkCredential = null;
		_uri = uri;
		_methodInfo = FtpMethodInfo.GetMethodInfo("RETR");
		if (_uri.UserInfo != null && _uri.UserInfo.Length != 0)
		{
			string userInfo = _uri.UserInfo;
			string userName = userInfo;
			string password = "";
			int num = userInfo.IndexOf(':');
			if (num != -1)
			{
				userName = Uri.UnescapeDataString(userInfo.Substring(0, num));
				num++;
				password = Uri.UnescapeDataString(userInfo.Substring(num, userInfo.Length - num));
			}
			networkCredential = new NetworkCredential(userName, password);
		}
		if (networkCredential == null)
		{
			networkCredential = s_defaultFtpNetworkCredential;
		}
		_authInfo = networkCredential;
	}

	/// <summary>Returns the FTP server response.</summary>
	/// <returns>A <see cref="T:System.Net.WebResponse" /> reference that contains an <see cref="T:System.Net.FtpWebResponse" /> instance. This object contains the FTP server's response to the request.</returns>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Net.FtpWebRequest.GetResponse" /> or <see cref="M:System.Net.FtpWebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" /> has already been called for this instance.- or -An HTTP proxy is enabled, and you attempted to use an FTP command other than <see cref="F:System.Net.WebRequestMethods.Ftp.DownloadFile" />, <see cref="F:System.Net.WebRequestMethods.Ftp.ListDirectory" />, or <see cref="F:System.Net.WebRequestMethods.Ftp.ListDirectoryDetails" />.</exception>
	/// <exception cref="T:System.Net.WebException">
	///   <see cref="P:System.Net.FtpWebRequest.EnableSsl" /> is set to true, but the server does not support this feature.- or -A <see cref="P:System.Net.FtpWebRequest.Timeout" /> was specified and the timeout has expired.</exception>
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
		if (NetEventSource.IsEnabled)
		{
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Enter(this, null, "GetResponse");
			}
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Info(this, $"Method: {_methodInfo.Method}", "GetResponse");
			}
		}
		try
		{
			CheckError();
			if (_ftpWebResponse != null)
			{
				return _ftpWebResponse;
			}
			if (_getResponseStarted)
			{
				throw new InvalidOperationException("Cannot re-call BeginGetRequestStream/BeginGetResponse while a previous call is still in progress.");
			}
			_getResponseStarted = true;
			_startTime = DateTime.UtcNow;
			_remainingTimeout = Timeout;
			if (Timeout != -1)
			{
				_remainingTimeout = Timeout - (int)(DateTime.UtcNow - _startTime).TotalMilliseconds;
				if (_remainingTimeout <= 0)
				{
					throw ExceptionHelper.TimeoutException;
				}
			}
			RequestStage requestStage = FinishRequestStage(RequestStage.RequestStarted);
			if (requestStage >= RequestStage.RequestStarted)
			{
				if (requestStage < RequestStage.ReadReady)
				{
					lock (_syncObject)
					{
						if (_requestStage < RequestStage.ReadReady)
						{
							_readAsyncResult = new LazyAsyncResult(null, null, null);
						}
					}
					if (_readAsyncResult != null)
					{
						_readAsyncResult.InternalWaitForCompletion();
					}
					CheckError();
				}
			}
			else
			{
				SubmitRequest(isAsync: false);
				if (_methodInfo.IsUpload)
				{
					FinishRequestStage(RequestStage.WriteReady);
				}
				else
				{
					FinishRequestStage(RequestStage.ReadReady);
				}
				CheckError();
				EnsureFtpWebResponse(null);
			}
		}
		catch (Exception ex)
		{
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Error(this, ex, "GetResponse");
			}
			if (_exception == null)
			{
				if (NetEventSource.IsEnabled)
				{
					NetEventSource.Error(this, ex, "GetResponse");
				}
				SetException(ex);
				FinishRequestStage(RequestStage.CheckForError);
			}
			throw;
		}
		finally
		{
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Exit(this, _ftpWebResponse, "GetResponse");
			}
		}
		return _ftpWebResponse;
	}

	/// <summary>Begins sending a request and receiving a response from an FTP server asynchronously.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> instance that indicates the status of the operation.</returns>
	/// <param name="callback">An <see cref="T:System.AsyncCallback" /> delegate that references the method to invoke when the operation is complete. </param>
	/// <param name="state">A user-defined object that contains information about the operation. This object is passed to the <paramref name="callback" /> delegate when the operation completes. </param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Net.FtpWebRequest.GetResponse" /> or <see cref="M:System.Net.FtpWebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" /> has already been called for this instance. </exception>
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
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Enter(this, null, "BeginGetResponse");
			NetEventSource.Info(this, $"Method: {_methodInfo.Method}", "BeginGetResponse");
		}
		ContextAwareResult contextAwareResult;
		try
		{
			if (_ftpWebResponse != null)
			{
				contextAwareResult = new ContextAwareResult(this, state, callback);
				contextAwareResult.InvokeCallback(_ftpWebResponse);
				return contextAwareResult;
			}
			if (_getResponseStarted)
			{
				throw new InvalidOperationException("Cannot re-call BeginGetRequestStream/BeginGetResponse while a previous call is still in progress.");
			}
			_getResponseStarted = true;
			CheckError();
			RequestStage requestStage = FinishRequestStage(RequestStage.RequestStarted);
			contextAwareResult = (ContextAwareResult)(_readAsyncResult = new ContextAwareResult(captureIdentity: true, forceCaptureContext: true, this, state, callback));
			if (requestStage >= RequestStage.RequestStarted)
			{
				contextAwareResult.StartPostingAsyncOp();
				contextAwareResult.FinishPostingAsyncOp();
				if (requestStage >= RequestStage.ReadReady)
				{
					contextAwareResult = null;
				}
				else
				{
					lock (_syncObject)
					{
						if (_requestStage >= RequestStage.ReadReady)
						{
							contextAwareResult = null;
						}
					}
				}
				if (contextAwareResult == null)
				{
					contextAwareResult = (ContextAwareResult)_readAsyncResult;
					if (!contextAwareResult.InternalPeekCompleted)
					{
						contextAwareResult.InvokeCallback();
					}
				}
			}
			else
			{
				lock (contextAwareResult.StartPostingAsyncOp())
				{
					SubmitRequest(isAsync: true);
					contextAwareResult.FinishPostingAsyncOp();
				}
				FinishRequestStage(RequestStage.CheckForError);
			}
		}
		catch (Exception message)
		{
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Error(this, message, "BeginGetResponse");
			}
			throw;
		}
		finally
		{
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Exit(this, null, "BeginGetResponse");
			}
		}
		return contextAwareResult;
	}

	/// <summary>Ends a pending asynchronous operation started with <see cref="M:System.Net.FtpWebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" />.</summary>
	/// <returns>A <see cref="T:System.Net.WebResponse" /> reference that contains an <see cref="T:System.Net.FtpWebResponse" /> instance. This object contains the FTP server's response to the request.</returns>
	/// <param name="asyncResult">The <see cref="T:System.IAsyncResult" /> that was returned when the operation started. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> was not obtained by calling <see cref="M:System.Net.FtpWebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" />. </exception>
	/// <exception cref="T:System.InvalidOperationException">This method was already called for the operation identified by <paramref name="asyncResult" />. </exception>
	/// <exception cref="T:System.Net.WebException">An error occurred using an HTTP proxy. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Net.WebPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override WebResponse EndGetResponse(IAsyncResult asyncResult)
	{
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Enter(this, null, "EndGetResponse");
		}
		try
		{
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			LazyAsyncResult obj = (asyncResult as LazyAsyncResult) ?? throw new ArgumentException("The IAsyncResult object was not returned from the corresponding asynchronous method on this class.", "asyncResult");
			if (obj.EndCalled)
			{
				throw new InvalidOperationException(global::SR.Format("{0} can only be called once for each asynchronous operation.", "EndGetResponse"));
			}
			obj.InternalWaitForCompletion();
			obj.EndCalled = true;
			CheckError();
		}
		catch (Exception message)
		{
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Error(this, message, "EndGetResponse");
			}
			throw;
		}
		finally
		{
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Exit(this, null, "EndGetResponse");
			}
		}
		return _ftpWebResponse;
	}

	/// <summary>Retrieves the stream used to upload data to an FTP server.</summary>
	/// <returns>A writable <see cref="T:System.IO.Stream" /> instance used to store data to be sent to the server by the current request.</returns>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Net.FtpWebRequest.BeginGetRequestStream(System.AsyncCallback,System.Object)" /> has been called and has not completed. - or -An HTTP proxy is enabled, and you attempted to use an FTP command other than <see cref="F:System.Net.WebRequestMethods.Ftp.DownloadFile" />, <see cref="F:System.Net.WebRequestMethods.Ftp.ListDirectory" />, or <see cref="F:System.Net.WebRequestMethods.Ftp.ListDirectoryDetails" />.</exception>
	/// <exception cref="T:System.Net.WebException">A connection to the FTP server could not be established. </exception>
	/// <exception cref="T:System.Net.ProtocolViolationException">The <see cref="P:System.Net.FtpWebRequest.Method" /> property is not set to <see cref="F:System.Net.WebRequestMethods.Ftp.UploadFile" /> or <see cref="F:System.Net.WebRequestMethods.Ftp.AppendFile" />. </exception>
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
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Enter(this, null, "GetRequestStream");
			NetEventSource.Info(this, $"Method: {_methodInfo.Method}", "GetRequestStream");
		}
		try
		{
			if (_getRequestStreamStarted)
			{
				throw new InvalidOperationException("Cannot re-call BeginGetRequestStream/BeginGetResponse while a previous call is still in progress.");
			}
			_getRequestStreamStarted = true;
			if (!_methodInfo.IsUpload)
			{
				throw new ProtocolViolationException("Cannot send a content-body with this verb-type.");
			}
			CheckError();
			_startTime = DateTime.UtcNow;
			_remainingTimeout = Timeout;
			if (Timeout != -1)
			{
				_remainingTimeout = Timeout - (int)(DateTime.UtcNow - _startTime).TotalMilliseconds;
				if (_remainingTimeout <= 0)
				{
					throw ExceptionHelper.TimeoutException;
				}
			}
			FinishRequestStage(RequestStage.RequestStarted);
			SubmitRequest(isAsync: false);
			FinishRequestStage(RequestStage.WriteReady);
			CheckError();
			if (_stream.CanTimeout)
			{
				_stream.WriteTimeout = ReadWriteTimeout;
				_stream.ReadTimeout = ReadWriteTimeout;
			}
		}
		catch (Exception message)
		{
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Error(this, message, "GetRequestStream");
			}
			throw;
		}
		finally
		{
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Exit(this, null, "GetRequestStream");
			}
		}
		return _stream;
	}

	/// <summary>Begins asynchronously opening a request's content stream for writing.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> instance that indicates the status of the operation.</returns>
	/// <param name="callback">An <see cref="T:System.AsyncCallback" /> delegate that references the method to invoke when the operation is complete. </param>
	/// <param name="state">A user-defined object that contains information about the operation. This object is passed to the <paramref name="callback" /> delegate when the operation completes. </param>
	/// <exception cref="T:System.InvalidOperationException">A previous call to this method or <see cref="M:System.Net.FtpWebRequest.GetRequestStream" /> has not yet completed. </exception>
	/// <exception cref="T:System.Net.WebException">A connection to the FTP server could not be established. </exception>
	/// <exception cref="T:System.Net.ProtocolViolationException">The <see cref="P:System.Net.FtpWebRequest.Method" /> property is not set to <see cref="F:System.Net.WebRequestMethods.Ftp.UploadFile" />. </exception>
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
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Enter(this, null, "BeginGetRequestStream");
			NetEventSource.Info(this, $"Method: {_methodInfo.Method}", "BeginGetRequestStream");
		}
		ContextAwareResult contextAwareResult = null;
		try
		{
			if (_getRequestStreamStarted)
			{
				throw new InvalidOperationException("Cannot re-call BeginGetRequestStream/BeginGetResponse while a previous call is still in progress.");
			}
			_getRequestStreamStarted = true;
			if (!_methodInfo.IsUpload)
			{
				throw new ProtocolViolationException("Cannot send a content-body with this verb-type.");
			}
			CheckError();
			FinishRequestStage(RequestStage.RequestStarted);
			contextAwareResult = new ContextAwareResult(captureIdentity: true, forceCaptureContext: true, this, state, callback);
			lock (contextAwareResult.StartPostingAsyncOp())
			{
				_writeAsyncResult = contextAwareResult;
				SubmitRequest(isAsync: true);
				contextAwareResult.FinishPostingAsyncOp();
				FinishRequestStage(RequestStage.CheckForError);
				return contextAwareResult;
			}
		}
		catch (Exception message)
		{
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Error(this, message, "BeginGetRequestStream");
			}
			throw;
		}
		finally
		{
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Exit(this, null, "BeginGetRequestStream");
			}
		}
	}

	/// <summary>Ends a pending asynchronous operation started with <see cref="M:System.Net.FtpWebRequest.BeginGetRequestStream(System.AsyncCallback,System.Object)" />.</summary>
	/// <returns>A writable <see cref="T:System.IO.Stream" /> instance associated with this instance.</returns>
	/// <param name="asyncResult">The <see cref="T:System.IAsyncResult" /> object that was returned when the operation started. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> was not obtained by calling <see cref="M:System.Net.FtpWebRequest.BeginGetRequestStream(System.AsyncCallback,System.Object)" />. </exception>
	/// <exception cref="T:System.InvalidOperationException">This method was already called for the operation identified by <paramref name="asyncResult" />. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Net.WebPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override Stream EndGetRequestStream(IAsyncResult asyncResult)
	{
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Enter(this, null, "EndGetRequestStream");
		}
		Stream stream = null;
		try
		{
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			LazyAsyncResult obj = (asyncResult as LazyAsyncResult) ?? throw new ArgumentException("The IAsyncResult object was not returned from the corresponding asynchronous method on this class.", "asyncResult");
			if (obj.EndCalled)
			{
				throw new InvalidOperationException(global::SR.Format("{0} can only be called once for each asynchronous operation.", "EndGetResponse"));
			}
			obj.InternalWaitForCompletion();
			obj.EndCalled = true;
			CheckError();
			stream = _stream;
			obj.EndCalled = true;
			if (stream.CanTimeout)
			{
				stream.WriteTimeout = ReadWriteTimeout;
				stream.ReadTimeout = ReadWriteTimeout;
			}
		}
		catch (Exception message)
		{
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Error(this, message, "EndGetRequestStream");
			}
			throw;
		}
		finally
		{
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Exit(this, null, "EndGetRequestStream");
			}
		}
		return stream;
	}

	private void SubmitRequest(bool isAsync)
	{
		try
		{
			_async = isAsync;
			while (true)
			{
				FtpControlStream ftpControlStream = _connection;
				if (ftpControlStream == null)
				{
					if (isAsync)
					{
						CreateConnectionAsync();
						return;
					}
					ftpControlStream = (_connection = CreateConnection());
				}
				if (!isAsync && Timeout != -1)
				{
					_remainingTimeout = Timeout - (int)(DateTime.UtcNow - _startTime).TotalMilliseconds;
					if (_remainingTimeout <= 0)
					{
						break;
					}
				}
				if (NetEventSource.IsEnabled)
				{
					NetEventSource.Info(this, "Request being submitted", "SubmitRequest");
				}
				ftpControlStream.SetSocketTimeoutOption(RemainingTimeout);
				try
				{
					TimedSubmitRequestHelper(isAsync);
					return;
				}
				catch (Exception e)
				{
					if (AttemptedRecovery(e))
					{
						if (!isAsync && Timeout != -1)
						{
							_remainingTimeout = Timeout - (int)(DateTime.UtcNow - _startTime).TotalMilliseconds;
							if (_remainingTimeout <= 0)
							{
								throw;
							}
						}
						continue;
					}
					throw;
				}
			}
			throw ExceptionHelper.TimeoutException;
		}
		catch (WebException ex)
		{
			if (ex.InnerException is IOException { InnerException: SocketException { SocketErrorCode: SocketError.TimedOut } })
			{
				SetException(new WebException("The operation has timed out.", WebExceptionStatus.Timeout));
			}
			SetException(ex);
		}
		catch (Exception exception)
		{
			SetException(exception);
		}
	}

	private Exception TranslateConnectException(Exception e)
	{
		if (e is SocketException ex)
		{
			if (ex.SocketErrorCode == SocketError.HostNotFound)
			{
				return new WebException("The remote name could not be resolved", WebExceptionStatus.NameResolutionFailure);
			}
			return new WebException("Unable to connect to the remote server", WebExceptionStatus.ConnectFailure);
		}
		return e;
	}

	private async void CreateConnectionAsync()
	{
		string host = _uri.Host;
		int port = _uri.Port;
		TcpClient client = new TcpClient();
		object obj;
		try
		{
			await client.ConnectAsync(host, port).ConfigureAwait(continueOnCapturedContext: false);
			obj = new FtpControlStream(client);
		}
		catch (Exception e)
		{
			obj = TranslateConnectException(e);
		}
		AsyncRequestCallback(obj);
	}

	private FtpControlStream CreateConnection()
	{
		string host = _uri.Host;
		int port = _uri.Port;
		TcpClient tcpClient = new TcpClient();
		try
		{
			tcpClient.Connect(host, port);
		}
		catch (Exception e)
		{
			throw TranslateConnectException(e);
		}
		return new FtpControlStream(tcpClient);
	}

	private Stream TimedSubmitRequestHelper(bool isAsync)
	{
		if (isAsync)
		{
			if (_requestCompleteAsyncResult == null)
			{
				_requestCompleteAsyncResult = new LazyAsyncResult(null, null, null);
			}
			return _connection.SubmitRequest(this, isAsync: true, readInitalResponseOnConnect: true);
		}
		Stream stream = null;
		bool flag = false;
		TimerThread.Timer timer = TimerQueue.CreateTimer(_timerCallback, null);
		try
		{
			stream = _connection.SubmitRequest(this, isAsync: false, readInitalResponseOnConnect: true);
		}
		catch (Exception ex)
		{
			if ((!(ex is SocketException) && !(ex is ObjectDisposedException)) || !timer.HasExpired)
			{
				timer.Cancel();
				throw;
			}
			flag = true;
		}
		if (flag || !timer.Cancel())
		{
			_timedOut = true;
			throw ExceptionHelper.TimeoutException;
		}
		if (stream != null)
		{
			lock (_syncObject)
			{
				if (_aborted)
				{
					((ICloseEx)stream).CloseEx(CloseExState.Abort | CloseExState.Silent);
					CheckError();
					throw new InternalException();
				}
				_stream = stream;
			}
		}
		return stream;
	}

	private void TimerCallback(TimerThread.Timer timer, int timeNoticed, object context)
	{
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Info(this, null, "TimerCallback");
		}
		FtpControlStream connection = _connection;
		if (connection != null)
		{
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Info(this, "aborting connection", "TimerCallback");
			}
			connection.AbortConnect();
		}
	}

	private bool AttemptedRecovery(Exception e)
	{
		if (e is OutOfMemoryException || _onceFailed || _aborted || _timedOut || _connection == null || !_connection.RecoverableFailure)
		{
			return false;
		}
		_onceFailed = true;
		lock (_syncObject)
		{
			if (_connection == null)
			{
				return false;
			}
			_connection.CloseSocket();
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Info(this, $"Releasing connection: {_connection}", "AttemptedRecovery");
			}
			_connection = null;
		}
		return true;
	}

	private void SetException(Exception exception)
	{
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Info(this, null, "SetException");
		}
		if (exception is OutOfMemoryException)
		{
			_exception = exception;
			throw exception;
		}
		FtpControlStream connection = _connection;
		if (_exception == null)
		{
			if (exception is WebException)
			{
				EnsureFtpWebResponse(exception);
				_exception = new WebException(exception.Message, null, ((WebException)exception).Status, _ftpWebResponse);
			}
			else if (exception is AuthenticationException || exception is SecurityException)
			{
				_exception = exception;
			}
			else if (connection != null && connection.StatusCode != FtpStatusCode.Undefined)
			{
				EnsureFtpWebResponse(exception);
				_exception = new WebException(global::SR.Format("The remote server returned an error: {0}.", connection.StatusLine), exception, WebExceptionStatus.ProtocolError, _ftpWebResponse);
			}
			else
			{
				_exception = new WebException(exception.Message, exception);
			}
			if (connection != null && _ftpWebResponse != null)
			{
				_ftpWebResponse.UpdateStatus(connection.StatusCode, connection.StatusLine, connection.ExitMessage);
			}
		}
	}

	private void CheckError()
	{
		if (_exception != null)
		{
			ExceptionDispatchInfo.Throw(_exception);
		}
	}

	internal void RequestCallback(object obj)
	{
		if (_async)
		{
			AsyncRequestCallback(obj);
		}
		else
		{
			SyncRequestCallback(obj);
		}
	}

	private void SyncRequestCallback(object obj)
	{
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Enter(this, obj, "SyncRequestCallback");
		}
		RequestStage stage = RequestStage.CheckForError;
		try
		{
			bool flag = obj == null;
			Exception ex = obj as Exception;
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Info(this, $"exp:{ex} completedRequest:{flag}", "SyncRequestCallback");
			}
			if (ex != null)
			{
				SetException(ex);
				return;
			}
			if (!flag)
			{
				throw new InternalException();
			}
			FtpControlStream connection = _connection;
			if (connection != null)
			{
				EnsureFtpWebResponse(null);
				_ftpWebResponse.UpdateStatus(connection.StatusCode, connection.StatusLine, connection.ExitMessage);
			}
			stage = RequestStage.ReleaseConnection;
		}
		catch (Exception exception)
		{
			SetException(exception);
		}
		finally
		{
			FinishRequestStage(stage);
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Exit(this, null, "SyncRequestCallback");
			}
			CheckError();
		}
	}

	private void AsyncRequestCallback(object obj)
	{
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Enter(this, obj, "AsyncRequestCallback");
		}
		RequestStage stage = RequestStage.CheckForError;
		try
		{
			FtpControlStream ftpControlStream = obj as FtpControlStream;
			FtpDataStream ftpDataStream = obj as FtpDataStream;
			Exception ex = obj as Exception;
			bool flag = obj == null;
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Info(this, $"stream:{ftpDataStream} conn:{ftpControlStream} exp:{ex} completedRequest:{flag}", "AsyncRequestCallback");
			}
			while (true)
			{
				if (ex != null)
				{
					if (AttemptedRecovery(ex))
					{
						ftpControlStream = CreateConnection();
						if (ftpControlStream == null)
						{
							return;
						}
						ex = null;
					}
					if (ex != null)
					{
						SetException(ex);
						return;
					}
				}
				if (ftpControlStream == null)
				{
					break;
				}
				lock (_syncObject)
				{
					if (_aborted)
					{
						if (NetEventSource.IsEnabled)
						{
							NetEventSource.Info(this, $"Releasing connect:{ftpControlStream}", "AsyncRequestCallback");
						}
						ftpControlStream.CloseSocket();
						return;
					}
					_connection = ftpControlStream;
					if (NetEventSource.IsEnabled)
					{
						NetEventSource.Associate(this, _connection, "AsyncRequestCallback");
					}
				}
				try
				{
					ftpDataStream = (FtpDataStream)TimedSubmitRequestHelper(isAsync: true);
					return;
				}
				catch (Exception ex2)
				{
					ex = ex2;
				}
			}
			if (ftpDataStream != null)
			{
				lock (_syncObject)
				{
					if (_aborted)
					{
						((ICloseEx)ftpDataStream).CloseEx(CloseExState.Abort | CloseExState.Silent);
						return;
					}
					_stream = ftpDataStream;
				}
				ftpDataStream.SetSocketTimeoutOption(Timeout);
				EnsureFtpWebResponse(null);
				stage = (ftpDataStream.CanRead ? RequestStage.ReadReady : RequestStage.WriteReady);
			}
			else
			{
				if (!flag)
				{
					throw new InternalException();
				}
				ftpControlStream = _connection;
				if (ftpControlStream != null)
				{
					EnsureFtpWebResponse(null);
					_ftpWebResponse.UpdateStatus(ftpControlStream.StatusCode, ftpControlStream.StatusLine, ftpControlStream.ExitMessage);
				}
				stage = RequestStage.ReleaseConnection;
			}
		}
		catch (Exception exception)
		{
			SetException(exception);
		}
		finally
		{
			FinishRequestStage(stage);
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Exit(this, null, "AsyncRequestCallback");
			}
		}
	}

	private RequestStage FinishRequestStage(RequestStage stage)
	{
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Info(this, $"state:{stage}", "FinishRequestStage");
		}
		if (_exception != null)
		{
			stage = RequestStage.ReleaseConnection;
		}
		RequestStage requestStage;
		LazyAsyncResult writeAsyncResult;
		LazyAsyncResult readAsyncResult;
		FtpControlStream connection;
		lock (_syncObject)
		{
			requestStage = _requestStage;
			if (stage == RequestStage.CheckForError)
			{
				return requestStage;
			}
			if (requestStage == RequestStage.ReleaseConnection && stage == RequestStage.ReleaseConnection)
			{
				return RequestStage.ReleaseConnection;
			}
			if (stage > requestStage)
			{
				_requestStage = stage;
			}
			if (stage <= RequestStage.RequestStarted)
			{
				return requestStage;
			}
			writeAsyncResult = _writeAsyncResult;
			readAsyncResult = _readAsyncResult;
			connection = _connection;
			if (stage == RequestStage.ReleaseConnection)
			{
				if (_exception == null && !_aborted && requestStage != RequestStage.ReadReady && _methodInfo.IsDownload && !_ftpWebResponse.IsFromCache)
				{
					return requestStage;
				}
				_connection = null;
			}
		}
		try
		{
			if ((stage == RequestStage.ReleaseConnection || requestStage == RequestStage.ReleaseConnection) && connection != null)
			{
				try
				{
					if (_exception != null)
					{
						connection.Abort(_exception);
					}
				}
				finally
				{
					if (NetEventSource.IsEnabled)
					{
						NetEventSource.Info(this, $"Releasing connection: {connection}", "FinishRequestStage");
					}
					connection.CloseSocket();
					if (_async && _requestCompleteAsyncResult != null)
					{
						_requestCompleteAsyncResult.InvokeCallback();
					}
				}
			}
			return requestStage;
		}
		finally
		{
			try
			{
				if (stage >= RequestStage.WriteReady)
				{
					if (_methodInfo.IsUpload && !_getRequestStreamStarted)
					{
						if (_stream != null)
						{
							_stream.Close();
						}
					}
					else if (writeAsyncResult != null && !writeAsyncResult.InternalPeekCompleted)
					{
						writeAsyncResult.InvokeCallback();
					}
				}
			}
			finally
			{
				if (stage >= RequestStage.ReadReady && readAsyncResult != null && !readAsyncResult.InternalPeekCompleted)
				{
					readAsyncResult.InvokeCallback();
				}
			}
		}
	}

	/// <summary>Terminates an asynchronous FTP operation.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Net.WebPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override void Abort()
	{
		if (_aborted)
		{
			return;
		}
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Enter(this, null, "Abort");
		}
		try
		{
			Stream stream;
			FtpControlStream connection;
			lock (_syncObject)
			{
				if (_requestStage >= RequestStage.ReleaseConnection)
				{
					return;
				}
				_aborted = true;
				stream = _stream;
				connection = _connection;
				_exception = ExceptionHelper.RequestAbortedException;
			}
			if (stream != null)
			{
				if (!(stream is ICloseEx))
				{
					NetEventSource.Fail(this, "The _stream member is not CloseEx hence the risk of connection been orphaned.", "Abort");
				}
				((ICloseEx)stream).CloseEx(CloseExState.Abort | CloseExState.Silent);
			}
			connection?.Abort(ExceptionHelper.RequestAbortedException);
		}
		catch (Exception message)
		{
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Error(this, message, "Abort");
			}
			throw;
		}
		finally
		{
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Exit(this, null, "Abort");
			}
		}
	}

	private void EnsureFtpWebResponse(Exception exception)
	{
		if (_ftpWebResponse == null || (_ftpWebResponse.GetResponseStream() is FtpWebResponse.EmptyStream && _stream != null))
		{
			lock (_syncObject)
			{
				if (_ftpWebResponse == null || (_ftpWebResponse.GetResponseStream() is FtpWebResponse.EmptyStream && _stream != null))
				{
					Stream stream = _stream;
					if (_methodInfo.IsUpload)
					{
						stream = null;
					}
					if (_stream != null && _stream.CanRead && _stream.CanTimeout)
					{
						_stream.ReadTimeout = ReadWriteTimeout;
						_stream.WriteTimeout = ReadWriteTimeout;
					}
					FtpControlStream connection = _connection;
					long num = connection?.ContentLength ?? (-1);
					if (stream == null && num < 0)
					{
						num = 0L;
					}
					if (_ftpWebResponse != null)
					{
						_ftpWebResponse.SetResponseStream(stream);
					}
					else if (connection != null)
					{
						_ftpWebResponse = new FtpWebResponse(stream, num, connection.ResponseUri, connection.StatusCode, connection.StatusLine, connection.LastModified, connection.BannerMessage, connection.WelcomeMessage, connection.ExitMessage);
					}
					else
					{
						_ftpWebResponse = new FtpWebResponse(stream, -1L, _uri, FtpStatusCode.Undefined, null, DateTime.Now, null, null, null);
					}
				}
			}
		}
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Info(this, $"Returns {_ftpWebResponse} with stream {_ftpWebResponse._responseStream}", "EnsureFtpWebResponse");
		}
	}

	internal void DataStreamClosed(CloseExState closeState)
	{
		if ((closeState & CloseExState.Abort) == 0)
		{
			if (!_async)
			{
				if (_connection != null)
				{
					_connection.CheckContinuePipeline();
				}
			}
			else
			{
				_requestCompleteAsyncResult.InternalWaitForCompletion();
				CheckError();
			}
		}
		else
		{
			_connection?.Abort(ExceptionHelper.RequestAbortedException);
		}
	}

	internal FtpWebRequest()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
