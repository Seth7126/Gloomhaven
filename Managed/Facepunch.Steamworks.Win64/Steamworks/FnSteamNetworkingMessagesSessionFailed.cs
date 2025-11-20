using System.Runtime.InteropServices;
using Steamworks.Data;

namespace Steamworks;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void FnSteamNetworkingMessagesSessionFailed(ref SteamNetworkingMessagesSessionFailed_t arg);
