using System.IO;
using Unity;

namespace System.Net;

/// <summary>Encapsulates a File Transfer Protocol (FTP) server's response to a request.</summary>
public class FtpWebResponse : WebResponse, IDisposable
{
	internal sealed class EmptyStream : MemoryStream
	{
		internal EmptyStream()
			: base(Array.Empty<byte>(), writable: false)
		{
		}
	}

	internal Stream _responseStream;

	private long _contentLength;

	private Uri _responseUri;

	private FtpStatusCode _statusCode;

	private string _statusLine;

	private WebHeaderCollection _ftpRequestHeaders;

	private DateTime _lastModified;

	private string _bannerMessage;

	private string _welcomeMessage;

	private string _exitMessage;

	/// <summary>Gets the length of the data received from the FTP server.</summary>
	/// <returns>An <see cref="T:System.Int64" /> value that contains the number of bytes of data received from the FTP server. </returns>
	public override long ContentLength => _contentLength;

	/// <summary>Gets an empty <see cref="T:System.Net.WebHeaderCollection" /> object.</summary>
	/// <returns>An empty <see cref="T:System.Net.WebHeaderCollection" /> object.</returns>
	public override WebHeaderCollection Headers
	{
		get
		{
			if (_ftpRequestHeaders == null)
			{
				lock (this)
				{
					if (_ftpRequestHeaders == null)
					{
						_ftpRequestHeaders = new WebHeaderCollection();
					}
				}
			}
			return _ftpRequestHeaders;
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="P:System.Net.FtpWebResponse.Headers" /> property is supported by the <see cref="T:System.Net.FtpWebResponse" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the <see cref="P:System.Net.FtpWebResponse.Headers" /> property is supported by the <see cref="T:System.Net.FtpWebResponse" /> instance; otherwise, false.</returns>
	public override bool SupportsHeaders => true;

	/// <summary>Gets the URI that sent the response to the request.</summary>
	/// <returns>A <see cref="T:System.Uri" /> instance that identifies the resource associated with this response.</returns>
	public override Uri ResponseUri => _responseUri;

	/// <summary>Gets the most recent status code sent from the FTP server.</summary>
	/// <returns>An <see cref="T:System.Net.FtpStatusCode" /> value that indicates the most recent status code returned with this response.</returns>
	public FtpStatusCode StatusCode => _statusCode;

	/// <summary>Gets text that describes a status code sent from the FTP server.</summary>
	/// <returns>A <see cref="T:System.String" /> instance that contains the status code and message returned with this response.</returns>
	public string StatusDescription => _statusLine;

	/// <summary>Gets the date and time that a file on an FTP server was last modified.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> that contains the last modified date and time for a file.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public DateTime LastModified => _lastModified;

	/// <summary>Gets the message sent by the FTP server when a connection is established prior to logon.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the banner message sent by the server; otherwise, <see cref="F:System.String.Empty" /> if no message is sent.</returns>
	public string BannerMessage => _bannerMessage;

	/// <summary>Gets the message sent by the FTP server when authentication is complete.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the welcome message sent by the server; otherwise, <see cref="F:System.String.Empty" /> if no message is sent.</returns>
	public string WelcomeMessage => _welcomeMessage;

	/// <summary>Gets the message sent by the server when the FTP session is ending.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the exit message sent by the server; otherwise, <see cref="F:System.String.Empty" /> if no message is sent.</returns>
	public string ExitMessage => _exitMessage;

	internal FtpWebResponse(Stream responseStream, long contentLength, Uri responseUri, FtpStatusCode statusCode, string statusLine, DateTime lastModified, string bannerMessage, string welcomeMessage, string exitMessage)
	{
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Enter(this, contentLength, statusLine);
		}
		_responseStream = responseStream;
		if (responseStream == null && contentLength < 0)
		{
			contentLength = 0L;
		}
		_contentLength = contentLength;
		_responseUri = responseUri;
		_statusCode = statusCode;
		_statusLine = statusLine;
		_lastModified = lastModified;
		_bannerMessage = bannerMessage;
		_welcomeMessage = welcomeMessage;
		_exitMessage = exitMessage;
	}

	internal void UpdateStatus(FtpStatusCode statusCode, string statusLine, string exitMessage)
	{
		_statusCode = statusCode;
		_statusLine = statusLine;
		_exitMessage = exitMessage;
	}

	/// <summary>Retrieves the stream that contains response data sent from an FTP server.</summary>
	/// <returns>A readable <see cref="T:System.IO.Stream" /> instance that contains data returned with the response; otherwise, <see cref="F:System.IO.Stream.Null" /> if no response data was returned by the server.</returns>
	/// <exception cref="T:System.InvalidOperationException">The response did not return a data stream. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override Stream GetResponseStream()
	{
		Stream stream = null;
		if (_responseStream != null)
		{
			return _responseStream;
		}
		return _responseStream = new EmptyStream();
	}

	internal void SetResponseStream(Stream stream)
	{
		if (stream != null && stream != Stream.Null && !(stream is EmptyStream))
		{
			_responseStream = stream;
		}
	}

	/// <summary>Frees the resources held by the response.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override void Close()
	{
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Enter(this, null, "Close");
		}
		_responseStream?.Close();
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Exit(this, null, "Close");
		}
	}

	internal FtpWebResponse()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
