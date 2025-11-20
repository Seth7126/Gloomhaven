using System.Runtime.InteropServices;
using Steamworks.Data;

namespace Steamworks;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void FnSteamNetworkingMessagesSessionRequest(ref SteamNetworkingMessagesSessionRequest_t arg);
