using System.Runtime.InteropServices;

namespace Sony.PS4.SaveData;

internal struct NativeInitResult
{
	[MarshalAs(UnmanagedType.I1)]
	internal bool initialized;

	internal uint sceSDKVersion;
}
