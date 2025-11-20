using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.Platform;

namespace PlayEveryWare.EpicOnlineServices;

public interface IEOSManagerPlatformSpecifics
{
	string GetTempDir();

	void AddPluginSearchPaths(ref List<string> pluginPaths);

	string GetDynamicLibraryExtension();

	void LoadDelegatesWithEOSBindingAPI();

	IEOSInitializeOptions CreateSystemInitOptions();

	void ConfigureSystemInitOptions(ref IEOSInitializeOptions initializeOptions, EOSConfig configData);

	IEOSCreateOptions CreateSystemPlatformOption();

	void ConfigureSystemPlatformCreateOptions(ref IEOSCreateOptions createOptions);

	Result InitializePlatformInterface(IEOSInitializeOptions options);

	PlatformInterface CreatePlatformInterface(IEOSCreateOptions platformOptions);

	void InitializeOverlay(IEOSCoroutineOwner owner);

	void RegisterForPlatformNotifications();

	bool IsApplicationConstrainedWhenOutOfFocus();
}
