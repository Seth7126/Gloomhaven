using UnityEngine;

namespace Photon.Bolt;

public static class UnitySettings
{
	public enum DotNetVersion
	{
		DOTNET_35,
		DOTNET_4x
	}

	public static bool IsBuildMono;

	public static bool IsBuildDotNet;

	public static bool IsBuildIL2CPP;

	public static DotNetVersion CurrentDotNetVersion;

	public static RuntimePlatform CurrentPlatform;
}
