#define ENABLE_LOGS
using Platforms.Steam;
using SM.Utils;

namespace Platforms.Utils;

public static class PlatformConstructor
{
	public static IPlatform BuildPlatform(IGameProvider gameProvider, bool initHydra, bool initEntitlements, bool initPros, bool isDevicePairingIncluded = true)
	{
		return new PlatformSteam(gameProvider, initHydra, initEntitlements, initPros, isDevicePairingIncluded);
	}

	private static void LogPlatformInfo(IPlatform platform)
	{
		LogUtils.Log($"[platformConstructor] Platform detected as {platform.GetType()}");
		if (platform.UserManagement != null)
		{
			LogUtils.Log($"{platform.UserManagement.GetType()} initialized successfully. CurrentUserInfo:");
			IPlatformUserData currentUser = platform.UserManagement.GetCurrentUser();
			LogUtils.Log("User Name: " + currentUser.GetPlatformDisplayName());
			LogUtils.Log("User Id: " + currentUser.GetPlatformUniqueUserID());
			LogUtils.Log("User is signedIn: " + currentUser.IsSignedInOnline());
			LogUtils.Log("Joystick Index: " + currentUser.GetInputDevicePlatformSerialNumber());
		}
	}
}
