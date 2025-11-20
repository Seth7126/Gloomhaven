using System.Runtime.InteropServices;

namespace Sony.NP;

internal struct NativeInitResult
{
	[MarshalAs(UnmanagedType.I1)]
	internal bool initialized;

	internal uint sceSDKVersion;
}
