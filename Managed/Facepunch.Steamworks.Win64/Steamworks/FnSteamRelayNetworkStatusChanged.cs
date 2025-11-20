using System.Runtime.InteropServices;
using Steamworks.Data;

namespace Steamworks;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void FnSteamRelayNetworkStatusChanged(ref SteamRelayNetworkStatus_t arg);
