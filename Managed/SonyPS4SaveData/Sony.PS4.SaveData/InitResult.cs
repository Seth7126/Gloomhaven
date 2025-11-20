using System;
using System.Reflection;

namespace Sony.PS4.SaveData;

public struct InitResult
{
	internal bool initialized;

	internal uint sceSDKVersion;

	internal Version dllVersion;

	public bool Initialized => initialized;

	public uint SceSDKVersionValue => sceSDKVersion;

	public Version DllVersion => dllVersion;

	public SceSDKVersion SceSDKVersion
	{
		get
		{
			SceSDKVersion result = default(SceSDKVersion);
			result.Patch = sceSDKVersion & 0xFFF;
			result.Minor = (sceSDKVersion >> 12) & 0xFFF;
			result.Major = sceSDKVersion >> 24;
			return result;
		}
	}

	internal void Initialise(NativeInitResult nativeResult)
	{
		initialized = nativeResult.initialized;
		sceSDKVersion = nativeResult.sceSDKVersion;
		dllVersion = Assembly.GetExecutingAssembly().GetName().Version;
	}
}
