using System.Runtime.InteropServices;
using Steamworks.Data;

namespace Steamworks;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void FnSteamNetConnectionStatusChanged(ref SteamNetConnectionStatusChangedCallback_t arg);
