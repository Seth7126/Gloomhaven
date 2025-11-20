using System.Runtime.InteropServices;

namespace Steamworks.Data;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct SteamNetworkingMessagesSessionFailed_t : ICallbackData
{
	internal ConnectionInfo Nfo;

	public static int _datasize = Marshal.SizeOf(typeof(SteamNetworkingMessagesSessionFailed_t));

	public int DataSize => _datasize;

	public CallbackType CallbackType => CallbackType.SteamNetworkingMessagesSessionFailed;
}
