using System.ComponentModel;
using Unity;

namespace System.Net;

/// <summary>Provides data for the <see cref="E:System.Net.WebClient.UploadProgressChanged" /> event of a <see cref="T:System.Net.WebClient" />.</summary>
public class UploadProgressChangedEventArgs : ProgressChangedEventArgs
{
	/// <summary>Gets the number of bytes received.</summary>
	/// <returns>An <see cref="T:System.Int64" /> value that indicates the number of bytes received.</returns>
	public long BytesReceived { get; }

	/// <summary>Gets the total number of bytes in a <see cref="T:System.Net.WebClient" /> data upload operation.</summary>
	/// <returns>An <see cref="T:System.Int64" /> value that indicates the number of bytes that will be received.</returns>
	public long TotalBytesToReceive { get; }

	/// <summary>Gets the number of bytes sent.</summary>
	/// <returns>An <see cref="T:System.Int64" /> value that indicates the number of bytes sent.</returns>
	public long BytesSent { get; }

	/// <summary>Gets the total number of bytes to send.</summary>
	/// <returns>An <see cref="T:System.Int64" /> value that indicates the number of bytes that will be sent.</returns>
	public long TotalBytesToSend { get; }

	internal UploadProgressChangedEventArgs(int progressPercentage, object userToken, long bytesSent, long totalBytesToSend, long bytesReceived, long totalBytesToReceive)
		: base(progressPercentage, userToken)
	{
		BytesReceived = bytesReceived;
		TotalBytesToReceive = totalBytesToReceive;
		BytesSent = bytesSent;
		TotalBytesToSend = totalBytesToSend;
	}

	internal UploadProgressChangedEventArgs()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
