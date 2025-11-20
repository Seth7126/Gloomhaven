using System.Runtime.InteropServices;

namespace Sony.PS4.SaveData;

internal struct ThreadSettingsNative
{
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
	internal string name;

	internal ulong affinityMask;

	internal ThreadSettingsNative(ThreadAffinity affinityMask, string name)
	{
		this.name = name;
		this.affinityMask = (ulong)affinityMask;
	}
}
