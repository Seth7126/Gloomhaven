using System.ComponentModel;

namespace XUnity.AutoTranslator.Plugin.Core.Web.Internal;

public class XUnityUploadProgressChangedEventArgs : ProgressChangedEventArgs
{
	private long received;

	private long sent;

	private long total_recv;

	private long total_send;

	public long BytesReceived => received;

	public long BytesSent => sent;

	public long TotalBytesToReceive => total_recv;

	public long TotalBytesToSend => total_send;

	internal XUnityUploadProgressChangedEventArgs(long bytesReceived, long totalBytesToReceive, long bytesSent, long totalBytesToSend, int progressPercentage, object userState)
		: base(progressPercentage, userState)
	{
		received = bytesReceived;
		total_recv = totalBytesToReceive;
		sent = bytesSent;
		total_send = totalBytesToSend;
	}
}
