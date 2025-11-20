using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using Unity;

namespace System.Net;

/// <summary>Provides connection management for HTTP connections.</summary>
public class ServicePoint
{
	private readonly Uri uri;

	private DateTime lastDnsResolve;

	private Version protocolVersion;

	private IPHostEntry host;

	private bool usesProxy;

	private bool sendContinue;

	private bool useConnect;

	private object hostE;

	private bool useNagle;

	private BindIPEndPoint endPointCallback;

	private bool tcp_keepalive;

	private int tcp_keepalive_time;

	private int tcp_keepalive_interval;

	private bool disposed;

	private int connectionLeaseTimeout;

	private int receiveBufferSize;

	private int connectionLimit;

	private int maxIdleTime;

	private object m_ServerCertificateOrBytes;

	private object m_ClientCertificateOrBytes;

	internal ServicePointManager.SPKey Key { get; }

	private ServicePointScheduler Scheduler { get; set; }

	/// <summary>Gets the Uniform Resource Identifier (URI) of the server that this <see cref="T:System.Net.ServicePoint" /> object connects to.</summary>
	/// <returns>An instance of the <see cref="T:System.Uri" /> class that contains the URI of the Internet server that this <see cref="T:System.Net.ServicePoint" /> object connects to.</returns>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Net.ServicePoint" /> is in host mode.</exception>
	public Uri Address => uri;

	/// <summary>Specifies the delegate to associate a local <see cref="T:System.Net.IPEndPoint" /> with a <see cref="T:System.Net.ServicePoint" />.</summary>
	/// <returns>A delegate that forces a <see cref="T:System.Net.ServicePoint" /> to use a particular local Internet Protocol (IP) address and port number. The default value is null.</returns>
	public BindIPEndPoint BindIPEndPointDelegate
	{
		get
		{
			return endPointCallback;
		}
		set
		{
			endPointCallback = value;
		}
	}

	/// <summary>Gets or sets the number of milliseconds after which an active <see cref="T:System.Net.ServicePoint" /> connection is closed.</summary>
	/// <returns>A <see cref="T:System.Int32" /> that specifies the number of milliseconds that an active <see cref="T:System.Net.ServicePoint" /> connection remains open. The default is -1, which allows an active <see cref="T:System.Net.ServicePoint" /> connection to stay connected indefinitely. Set this property to 0 to force <see cref="T:System.Net.ServicePoint" /> connections to close after servicing a request.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value specified for a set operation is a negative number less than -1.</exception>
	public int ConnectionLeaseTimeout
	{
		get
		{
			return connectionLeaseTimeout;
		}
		set
		{
			if (value < -1)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			connectionLeaseTimeout = value;
		}
	}

	/// <summary>Gets or sets the maximum number of connections allowed on this <see cref="T:System.Net.ServicePoint" /> object.</summary>
	/// <returns>The maximum number of connections allowed on this <see cref="T:System.Net.ServicePoint" /> object.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The connection limit is equal to or less than 0. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Net.DnsPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int ConnectionLimit
	{
		get
		{
			return connectionLimit;
		}
		set
		{
			connectionLimit = value;
			if (!disposed)
			{
				Scheduler.ConnectionLimit = value;
			}
		}
	}

	/// <summary>Gets the connection name. </summary>
	/// <returns>A <see cref="T:System.String" /> that represents the connection name. </returns>
	public string ConnectionName => uri.Scheme;

	/// <summary>Gets the number of open connections associated with this <see cref="T:System.Net.ServicePoint" /> object.</summary>
	/// <returns>The number of open connections associated with this <see cref="T:System.Net.ServicePoint" /> object.</returns>
	public int CurrentConnections
	{
		get
		{
			if (!disposed)
			{
				return Scheduler.CurrentConnections;
			}
			return 0;
		}
	}

	/// <summary>Gets the date and time that the <see cref="T:System.Net.ServicePoint" /> object was last connected to a host.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> object that contains the date and time at which the <see cref="T:System.Net.ServicePoint" /> object was last connected.</returns>
	public DateTime IdleSince
	{
		get
		{
			if (disposed)
			{
				return DateTime.MinValue;
			}
			return Scheduler.IdleSince.ToLocalTime();
		}
	}

	/// <summary>Gets or sets the amount of time a connection associated with the <see cref="T:System.Net.ServicePoint" /> object can remain idle before the connection is closed.</summary>
	/// <returns>The length of time, in milliseconds, that a connection associated with the <see cref="T:System.Net.ServicePoint" /> object can remain idle before it is closed and reused for another connection.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <see cref="P:System.Net.ServicePoint.MaxIdleTime" /> is set to less than <see cref="F:System.Threading.Timeout.Infinite" /> or greater than <see cref="F:System.Int32.MaxValue" />. </exception>
	public int MaxIdleTime
	{
		get
		{
			return maxIdleTime;
		}
		set
		{
			maxIdleTime = value;
			if (!disposed)
			{
				Scheduler.MaxIdleTime = value;
			}
		}
	}

	/// <summary>Gets the version of the HTTP protocol that the <see cref="T:System.Net.ServicePoint" /> object uses.</summary>
	/// <returns>A <see cref="T:System.Version" /> object that contains the HTTP protocol version that the <see cref="T:System.Net.ServicePoint" /> object uses.</returns>
	public virtual Version ProtocolVersion => protocolVersion;

	/// <summary>Gets or sets the size of the receiving buffer for the socket used by this <see cref="T:System.Net.ServicePoint" />.</summary>
	/// <returns>A <see cref="T:System.Int32" /> that contains the size, in bytes, of the receive buffer. The default is 8192.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value specified for a set operation is greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	public int ReceiveBufferSize
	{
		get
		{
			return receiveBufferSize;
		}
		set
		{
			if (value < -1)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			receiveBufferSize = value;
		}
	}

	/// <summary>Indicates whether the <see cref="T:System.Net.ServicePoint" /> object supports pipelined connections.</summary>
	/// <returns>true if the <see cref="T:System.Net.ServicePoint" /> object supports pipelined connections; otherwise, false.</returns>
	public bool SupportsPipelining => HttpVersion.Version11.Equals(protocolVersion);

	/// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that determines whether 100-Continue behavior is used.</summary>
	/// <returns>true to expect 100-Continue responses for POST requests; otherwise, false. The default value is true.</returns>
	public bool Expect100Continue
	{
		get
		{
			return SendContinue;
		}
		set
		{
			SendContinue = value;
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that determines whether the Nagle algorithm is used on connections managed by this <see cref="T:System.Net.ServicePoint" /> object.</summary>
	/// <returns>true to use the Nagle algorithm; otherwise, false. The default value is true.</returns>
	public bool UseNagleAlgorithm
	{
		get
		{
			return useNagle;
		}
		set
		{
			useNagle = value;
		}
	}

	internal bool SendContinue
	{
		get
		{
			if (sendContinue)
			{
				if (!(protocolVersion == null))
				{
					return protocolVersion == HttpVersion.Version11;
				}
				return true;
			}
			return false;
		}
		set
		{
			sendContinue = value;
		}
	}

	internal bool UsesProxy
	{
		get
		{
			return usesProxy;
		}
		set
		{
			usesProxy = value;
		}
	}

	internal bool UseConnect
	{
		get
		{
			return useConnect;
		}
		set
		{
			useConnect = value;
		}
	}

	private bool HasTimedOut
	{
		get
		{
			int dnsRefreshTimeout = ServicePointManager.DnsRefreshTimeout;
			if (dnsRefreshTimeout != -1)
			{
				return lastDnsResolve + TimeSpan.FromMilliseconds(dnsRefreshTimeout) < DateTime.UtcNow;
			}
			return false;
		}
	}

	internal IPHostEntry HostEntry
	{
		get
		{
			lock (hostE)
			{
				string text = uri.Host;
				if (uri.HostNameType == UriHostNameType.IPv6 || uri.HostNameType == UriHostNameType.IPv4)
				{
					if (host != null)
					{
						return host;
					}
					if (uri.HostNameType == UriHostNameType.IPv6)
					{
						text = text.Substring(1, text.Length - 2);
					}
					host = new IPHostEntry();
					host.AddressList = new IPAddress[1] { IPAddress.Parse(text) };
					return host;
				}
				if (!HasTimedOut && host != null)
				{
					return host;
				}
				lastDnsResolve = DateTime.UtcNow;
				try
				{
					host = Dns.GetHostEntry(text);
				}
				catch
				{
					return null;
				}
			}
			return host;
		}
	}

	/// <summary>Gets the certificate received for this <see cref="T:System.Net.ServicePoint" /> object.</summary>
	/// <returns>An instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class that contains the security certificate received for this <see cref="T:System.Net.ServicePoint" /> object.</returns>
	public X509Certificate Certificate
	{
		get
		{
			object serverCertificateOrBytes = m_ServerCertificateOrBytes;
			if (serverCertificateOrBytes != null && serverCertificateOrBytes.GetType() == typeof(byte[]))
			{
				return (X509Certificate)(m_ServerCertificateOrBytes = new X509Certificate((byte[])serverCertificateOrBytes));
			}
			return serverCertificateOrBytes as X509Certificate;
		}
	}

	/// <summary>Gets the last client certificate sent to the server.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object that contains the public values of the last client certificate sent to the server.</returns>
	public X509Certificate ClientCertificate
	{
		get
		{
			object clientCertificateOrBytes = m_ClientCertificateOrBytes;
			if (clientCertificateOrBytes != null && clientCertificateOrBytes.GetType() == typeof(byte[]))
			{
				return (X509Certificate)(m_ClientCertificateOrBytes = new X509Certificate((byte[])clientCertificateOrBytes));
			}
			return clientCertificateOrBytes as X509Certificate;
		}
	}

	internal ServicePoint(ServicePointManager.SPKey key, Uri uri, int connectionLimit, int maxIdleTime)
	{
		sendContinue = true;
		hostE = new object();
		connectionLeaseTimeout = -1;
		receiveBufferSize = -1;
		base._002Ector();
		Key = key;
		this.uri = uri;
		this.connectionLimit = connectionLimit;
		this.maxIdleTime = maxIdleTime;
		Scheduler = new ServicePointScheduler(this, connectionLimit, maxIdleTime);
	}

	/// <summary>Enables or disables the keep-alive option on a TCP connection.</summary>
	/// <param name="enabled">If set to true, then the TCP keep-alive option on a TCP connection will be enabled using the specified <paramref name="keepAliveTime " />and <paramref name="keepAliveInterval" /> values. If set to false, then the TCP keep-alive option is disabled and the remaining parameters are ignored.The default value is false.</param>
	/// <param name="keepAliveTime">Specifies the timeout, in milliseconds, with no activity until the first keep-alive packet is sent. The value must be greater than 0.  If a value of less than or equal to zero is passed an <see cref="T:System.ArgumentOutOfRangeException" /> is thrown.</param>
	/// <param name="keepAliveInterval">Specifies the interval, in milliseconds, between when successive keep-alive packets are sent if no acknowledgement is received.The value must be greater than 0.  If a value of less than or equal to zero is passed an <see cref="T:System.ArgumentOutOfRangeException" /> is thrown.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value specified for <paramref name="keepAliveTime" /> or <paramref name="keepAliveInterval" /> parameter is less than or equal to 0.</exception>
	public void SetTcpKeepAlive(bool enabled, int keepAliveTime, int keepAliveInterval)
	{
		if (enabled)
		{
			if (keepAliveTime <= 0)
			{
				throw new ArgumentOutOfRangeException("keepAliveTime", "Must be greater than 0");
			}
			if (keepAliveInterval <= 0)
			{
				throw new ArgumentOutOfRangeException("keepAliveInterval", "Must be greater than 0");
			}
		}
		tcp_keepalive = enabled;
		tcp_keepalive_time = keepAliveTime;
		tcp_keepalive_interval = keepAliveInterval;
	}

	internal void KeepAliveSetup(Socket socket)
	{
		if (tcp_keepalive)
		{
			byte[] array = new byte[12];
			PutBytes(array, tcp_keepalive ? 1u : 0u, 0);
			PutBytes(array, (uint)tcp_keepalive_time, 4);
			PutBytes(array, (uint)tcp_keepalive_interval, 8);
			socket.IOControl(IOControlCode.KeepAliveValues, array, null);
		}
	}

	private static void PutBytes(byte[] bytes, uint v, int offset)
	{
		if (BitConverter.IsLittleEndian)
		{
			bytes[offset] = (byte)(v & 0xFF);
			bytes[offset + 1] = (byte)((v & 0xFF00) >> 8);
			bytes[offset + 2] = (byte)((v & 0xFF0000) >> 16);
			bytes[offset + 3] = (byte)((v & 0xFF000000u) >> 24);
		}
		else
		{
			bytes[offset + 3] = (byte)(v & 0xFF);
			bytes[offset + 2] = (byte)((v & 0xFF00) >> 8);
			bytes[offset + 1] = (byte)((v & 0xFF0000) >> 16);
			bytes[offset] = (byte)((v & 0xFF000000u) >> 24);
		}
	}

	internal void SetVersion(Version version)
	{
		protocolVersion = version;
	}

	internal void SendRequest(WebOperation operation, string groupName)
	{
		lock (this)
		{
			if (disposed)
			{
				throw new ObjectDisposedException(typeof(ServicePoint).FullName);
			}
			Scheduler.SendRequest(operation, groupName);
		}
	}

	/// <summary>Removes the specified connection group from this <see cref="T:System.Net.ServicePoint" /> object.</summary>
	/// <returns>A <see cref="T:System.Boolean" /> value that indicates whether the connection group was closed.</returns>
	/// <param name="connectionGroupName">The name of the connection group that contains the connections to close and remove from this service point. </param>
	public bool CloseConnectionGroup(string connectionGroupName)
	{
		lock (this)
		{
			if (disposed)
			{
				return true;
			}
			return Scheduler.CloseConnectionGroup(connectionGroupName);
		}
	}

	internal void FreeServicePoint()
	{
		disposed = true;
		Scheduler = null;
	}

	internal void UpdateServerCertificate(X509Certificate certificate)
	{
		if (certificate != null)
		{
			m_ServerCertificateOrBytes = certificate.GetRawCertData();
		}
		else
		{
			m_ServerCertificateOrBytes = null;
		}
	}

	internal void UpdateClientCertificate(X509Certificate certificate)
	{
		if (certificate != null)
		{
			m_ClientCertificateOrBytes = certificate.GetRawCertData();
		}
		else
		{
			m_ClientCertificateOrBytes = null;
		}
	}

	internal bool CallEndPointDelegate(Socket sock, IPEndPoint remote)
	{
		if (endPointCallback == null)
		{
			return true;
		}
		int num = 0;
		while (true)
		{
			IPEndPoint iPEndPoint = null;
			try
			{
				iPEndPoint = endPointCallback(this, remote, num);
			}
			catch
			{
				return false;
			}
			if (iPEndPoint == null)
			{
				return true;
			}
			try
			{
				sock.Bind(iPEndPoint);
			}
			catch (SocketException)
			{
				num = checked(num + 1);
				continue;
			}
			break;
		}
		return true;
	}

	internal ServicePoint()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
