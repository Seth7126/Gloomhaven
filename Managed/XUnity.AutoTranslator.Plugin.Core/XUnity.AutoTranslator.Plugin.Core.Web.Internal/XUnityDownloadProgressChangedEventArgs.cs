using System.ComponentModel;

namespace XUnity.AutoTranslator.Plugin.Core.Web.Internal;

public class XUnityDownloadProgressChangedEventArgs : ProgressChangedEventArgs
{
	private long received;

	private long total;

	public long BytesReceived => received;

	public long TotalBytesToReceive => total;

	internal XUnityDownloadProgressChangedEventArgs(long bytesReceived, long totalBytesToReceive, object userState)
		: base((int)((totalBytesToReceive != -1) ? (bytesReceived * 100 / totalBytesToReceive) : 0), userState)
	{
		received = bytesReceived;
		total = totalBytesToReceive;
	}
}
