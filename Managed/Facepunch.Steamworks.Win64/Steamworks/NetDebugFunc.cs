using System;
using System.Runtime.InteropServices;

namespace Steamworks;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void NetDebugFunc([In] NetDebugOutput nType, [In] IntPtr pszMsg);
