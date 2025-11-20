using System.Collections.Generic;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http;

/// <summary>The default message handler used by <see cref="T:System.Net.Http.HttpClient" />.  </summary>
public class HttpClientHandler : HttpMessageHandler
{
	private readonly IMonoHttpClientHandler _delegatingHandler;

	private ClientCertificateOption _clientCertificateOptions;

	public static Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> DangerousAcceptAnyServerCertificateValidator
	{
		get
		{
			throw new PlatformNotSupportedException();
		}
	}

	/// <summary>Gets a value that indicates whether the handler supports automatic response content decompression.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the if the handler supports automatic response content decompression; otherwise false. The default value is true.</returns>
	public virtual bool SupportsAutomaticDecompression => _delegatingHandler.SupportsAutomaticDecompression;

	/// <summary>Gets a value that indicates whether the handler supports proxy settings.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the if the handler supports proxy settings; otherwise false. The default value is true.</returns>
	public virtual bool SupportsProxy => true;

	/// <summary>Gets a value that indicates whether the handler supports configuration settings for the <see cref="P:System.Net.Http.HttpClientHandler.AllowAutoRedirect" /> and <see cref="P:System.Net.Http.HttpClientHandler.MaxAutomaticRedirections" /> properties.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the if the handler supports configuration settings for the <see cref="P:System.Net.Http.HttpClientHandler.AllowAutoRedirect" /> and <see cref="P:System.Net.Http.HttpClientHandler.MaxAutomaticRedirections" /> properties; otherwise false. The default value is true.</returns>
	public virtual bool SupportsRedirectConfiguration => true;

	/// <summary>Gets or sets a value that indicates whether the handler uses the  <see cref="P:System.Net.Http.HttpClientHandler.CookieContainer" /> property  to store server cookies and uses these cookies when sending requests.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the if the handler supports uses the  <see cref="P:System.Net.Http.HttpClientHandler.CookieContainer" /> property  to store server cookies and uses these cookies when sending requests; otherwise false. The default value is true.</returns>
	public bool UseCookies
	{
		get
		{
			return _delegatingHandler.UseCookies;
		}
		set
		{
			_delegatingHandler.UseCookies = value;
		}
	}

	/// <summary>Gets or sets the cookie container used to store server cookies by the handler.</summary>
	/// <returns>Returns <see cref="T:System.Net.CookieContainer" />.The cookie container used to store server cookies by the handler.</returns>
	public CookieContainer CookieContainer
	{
		get
		{
			return _delegatingHandler.CookieContainer;
		}
		set
		{
			_delegatingHandler.CookieContainer = value;
		}
	}

	/// <summary>Gets or sets the collection of security certificates that are associated with this handler.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.ClientCertificateOption" />.The collection of security certificates associated with this handler.</returns>
	public ClientCertificateOption ClientCertificateOptions
	{
		get
		{
			return _clientCertificateOptions;
		}
		set
		{
			switch (value)
			{
			case ClientCertificateOption.Manual:
				ThrowForModifiedManagedSslOptionsIfStarted();
				_clientCertificateOptions = value;
				_delegatingHandler.SslOptions.LocalCertificateSelectionCallback = (object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers) => CertificateHelper.GetEligibleClientCertificate(ClientCertificates);
				break;
			case ClientCertificateOption.Automatic:
				ThrowForModifiedManagedSslOptionsIfStarted();
				_clientCertificateOptions = value;
				_delegatingHandler.SslOptions.LocalCertificateSelectionCallback = (object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers) => CertificateHelper.GetEligibleClientCertificate();
				break;
			default:
				throw new ArgumentOutOfRangeException("value");
			}
		}
	}

	public X509CertificateCollection ClientCertificates
	{
		get
		{
			if (ClientCertificateOptions != ClientCertificateOption.Manual)
			{
				throw new InvalidOperationException(global::SR.Format("The {0} property must be set to '{1}' to use this property.", "ClientCertificateOptions", "Manual"));
			}
			return _delegatingHandler.SslOptions.ClientCertificates ?? (_delegatingHandler.SslOptions.ClientCertificates = new X509CertificateCollection());
		}
	}

	public Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> ServerCertificateCustomValidationCallback
	{
		get
		{
			return (_delegatingHandler.SslOptions.RemoteCertificateValidationCallback?.Target as ConnectHelper.CertificateCallbackMapper)?.FromHttpClientHandler;
		}
		set
		{
			ThrowForModifiedManagedSslOptionsIfStarted();
			_delegatingHandler.SslOptions.RemoteCertificateValidationCallback = ((value != null) ? new ConnectHelper.CertificateCallbackMapper(value).ForSocketsHttpHandler : null);
		}
	}

	public bool CheckCertificateRevocationList
	{
		get
		{
			return _delegatingHandler.SslOptions.CertificateRevocationCheckMode == X509RevocationMode.Online;
		}
		set
		{
			ThrowForModifiedManagedSslOptionsIfStarted();
			_delegatingHandler.SslOptions.CertificateRevocationCheckMode = (value ? X509RevocationMode.Online : X509RevocationMode.NoCheck);
		}
	}

	public SslProtocols SslProtocols
	{
		get
		{
			return _delegatingHandler.SslOptions.EnabledSslProtocols;
		}
		set
		{
			ThrowForModifiedManagedSslOptionsIfStarted();
			_delegatingHandler.SslOptions.EnabledSslProtocols = value;
		}
	}

	/// <summary>Gets or sets the type of decompression method used by the handler for automatic decompression of the HTTP content response.</summary>
	/// <returns>Returns <see cref="T:System.Net.DecompressionMethods" />.The automatic decompression method used by the handler. The default value is <see cref="F:System.Net.DecompressionMethods.None" />.</returns>
	public DecompressionMethods AutomaticDecompression
	{
		get
		{
			return _delegatingHandler.AutomaticDecompression;
		}
		set
		{
			_delegatingHandler.AutomaticDecompression = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the handler uses a proxy for requests. </summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the handler should use a proxy for requests; otherwise false. The default value is true.</returns>
	public bool UseProxy
	{
		get
		{
			return _delegatingHandler.UseProxy;
		}
		set
		{
			_delegatingHandler.UseProxy = value;
		}
	}

	/// <summary>Gets or sets proxy information used by the handler.</summary>
	/// <returns>Returns <see cref="T:System.Net.IWebProxy" />.The proxy information used by the handler. The default value is null.</returns>
	public IWebProxy Proxy
	{
		get
		{
			return _delegatingHandler.Proxy;
		}
		set
		{
			_delegatingHandler.Proxy = value;
		}
	}

	public ICredentials DefaultProxyCredentials
	{
		get
		{
			return _delegatingHandler.DefaultProxyCredentials;
		}
		set
		{
			_delegatingHandler.DefaultProxyCredentials = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the handler sends an Authorization header with the request.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true for the handler to send an HTTP Authorization header with requests after authentication has taken place; otherwise, false. The default is false.</returns>
	public bool PreAuthenticate
	{
		get
		{
			return _delegatingHandler.PreAuthenticate;
		}
		set
		{
			_delegatingHandler.PreAuthenticate = value;
		}
	}

	/// <summary>Gets or sets a value that controls whether default credentials are sent with requests by the handler.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the default credentials are used; otherwise false. The default value is false.</returns>
	public bool UseDefaultCredentials
	{
		get
		{
			return _delegatingHandler.Credentials == CredentialCache.DefaultCredentials;
		}
		set
		{
			if (value)
			{
				_delegatingHandler.Credentials = CredentialCache.DefaultCredentials;
			}
			else if (_delegatingHandler.Credentials == CredentialCache.DefaultCredentials)
			{
				_delegatingHandler.Credentials = null;
			}
		}
	}

	/// <summary>Gets or sets authentication information used by this handler.</summary>
	/// <returns>Returns <see cref="T:System.Net.ICredentials" />.The authentication credentials associated with the handler. The default is null.</returns>
	public ICredentials Credentials
	{
		get
		{
			return _delegatingHandler.Credentials;
		}
		set
		{
			_delegatingHandler.Credentials = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the handler should follow redirection responses.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the if the handler should follow redirection responses; otherwise false. The default value is true.</returns>
	public bool AllowAutoRedirect
	{
		get
		{
			return _delegatingHandler.AllowAutoRedirect;
		}
		set
		{
			_delegatingHandler.AllowAutoRedirect = value;
		}
	}

	/// <summary>Gets or sets the maximum number of redirects that the handler follows.</summary>
	/// <returns>Returns <see cref="T:System.Int32" />.The maximum number of redirection responses that the handler follows. The default value is 50.</returns>
	public int MaxAutomaticRedirections
	{
		get
		{
			return _delegatingHandler.MaxAutomaticRedirections;
		}
		set
		{
			_delegatingHandler.MaxAutomaticRedirections = value;
		}
	}

	public int MaxConnectionsPerServer
	{
		get
		{
			return _delegatingHandler.MaxConnectionsPerServer;
		}
		set
		{
			_delegatingHandler.MaxConnectionsPerServer = value;
		}
	}

	public int MaxResponseHeadersLength
	{
		get
		{
			return _delegatingHandler.MaxResponseHeadersLength;
		}
		set
		{
			_delegatingHandler.MaxResponseHeadersLength = value;
		}
	}

	/// <summary>Gets or sets the maximum request content buffer size used by the handler.</summary>
	/// <returns>Returns <see cref="T:System.Int32" />.The maximum request content buffer size in bytes. The default value is 2 gigabytes.</returns>
	public long MaxRequestContentBufferSize
	{
		get
		{
			return _delegatingHandler.MaxRequestContentBufferSize;
		}
		set
		{
			_delegatingHandler.MaxRequestContentBufferSize = value;
		}
	}

	public IDictionary<string, object> Properties => _delegatingHandler.Properties;

	private static IMonoHttpClientHandler CreateDefaultHandler()
	{
		return new MonoWebRequestHandler();
	}

	/// <summary>Creates an instance of a <see cref="T:System.Net.Http.HttpClientHandler" /> class.</summary>
	public HttpClientHandler()
		: this(CreateDefaultHandler())
	{
	}

	internal HttpClientHandler(IMonoHttpClientHandler handler)
	{
		_delegatingHandler = handler;
		ClientCertificateOptions = ClientCertificateOption.Manual;
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Net.Http.HttpClientHandler" /> and optionally disposes of the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to releases only unmanaged resources.</param>
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_delegatingHandler.Dispose();
		}
		base.Dispose(disposing);
	}

	private void ThrowForModifiedManagedSslOptionsIfStarted()
	{
		_delegatingHandler.SslOptions = _delegatingHandler.SslOptions;
	}

	internal void SetWebRequestTimeout(TimeSpan timeout)
	{
		_delegatingHandler.SetWebRequestTimeout(timeout);
	}

	/// <summary>Creates an instance of  <see cref="T:System.Net.Http.HttpResponseMessage" /> based on the information provided in the <see cref="T:System.Net.Http.HttpRequestMessage" /> as an operation that will not block.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation.</returns>
	/// <param name="request">The HTTP request message.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="request" /> was null.</exception>
	protected internal override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		return _delegatingHandler.SendAsync(request, cancellationToken);
	}
}
