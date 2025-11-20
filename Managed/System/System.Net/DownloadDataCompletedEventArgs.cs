using System.ComponentModel;
using Unity;

namespace System.Net;

/// <summary>Provides data for the <see cref="E:System.Net.WebClient.DownloadDataCompleted" /> event.</summary>
public class DownloadDataCompletedEventArgs : AsyncCompletedEventArgs
{
	private readonly byte[] _result;

	/// <summary>Gets the data that is downloaded by a <see cref="Overload:System.Net.WebClient.DownloadDataAsync" /> method.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array that contains the downloaded data.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public byte[] Result
	{
		get
		{
			RaiseExceptionIfNecessary();
			return _result;
		}
	}

	internal DownloadDataCompletedEventArgs(byte[] result, Exception exception, bool cancelled, object userToken)
		: base(exception, cancelled, userToken)
	{
		_result = result;
	}

	internal DownloadDataCompletedEventArgs()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
