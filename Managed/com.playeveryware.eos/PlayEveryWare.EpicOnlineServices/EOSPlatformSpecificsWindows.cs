using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Epic.OnlineServices;
using Epic.OnlineServices.Platform;
using UnityEngine;
using UnityEngine.Scripting;

namespace PlayEveryWare.EpicOnlineServices;

public class EOSPlatformSpecificsWindows : IEOSManagerPlatformSpecifics
{
	private static string Xaudio2DllName = "xaudio2_9redist.dll";

	public static string SteamConfigPath = "eos_steam_config.json";

	private static GCHandle SteamOptionsGCHandle;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	[Preserve]
	public static void Register()
	{
		EOSManagerPlatformSpecifics.SetEOSManagerPlatformSpecificsInterface(new EOSPlatformSpecificsWindows());
	}

	public string GetTempDir()
	{
		return Application.temporaryCachePath;
	}

	public void AddPluginSearchPaths(ref List<string> pluginPaths)
	{
	}

	public string GetDynamicLibraryExtension()
	{
		return ".dll";
	}

	public void LoadDelegatesWithEOSBindingAPI()
	{
	}

	private static string GetPlatformPathComponent()
	{
		return "x64";
	}

	public Result InitializePlatformInterface(IEOSInitializeOptions options)
	{
		return PlatformInterface.Initialize(ref (options as EOSWindowsInitializeOptions).options);
	}

	public PlatformInterface CreatePlatformInterface(IEOSCreateOptions platformOptions)
	{
		return PlatformInterface.Create(ref (platformOptions as EOSWindowsOptions).options);
	}

	public IEOSInitializeOptions CreateSystemInitOptions()
	{
		return new EOSWindowsInitializeOptions();
	}

	public void ConfigureSystemInitOptions(ref IEOSInitializeOptions initializeOptions, EOSConfig configData)
	{
	}

	public IEOSCreateOptions CreateSystemPlatformOption()
	{
		return new EOSWindowsOptions();
	}

	public void ConfigureSystemPlatformCreateOptions(ref IEOSCreateOptions createOptions)
	{
		string platformPathComponent = GetPlatformPathComponent();
		if (platformPathComponent.Length <= 0)
		{
			return;
		}
		List<string> pathsToPlugins = DLLHandle.GetPathsToPlugins();
		WindowsRTCOptionsPlatformSpecificOptions value = default(WindowsRTCOptionsPlatformSpecificOptions);
		foreach (string item in pathsToPlugins)
		{
			string text = Path.Combine(item, "Windows", platformPathComponent, Xaudio2DllName);
			if (File.Exists(text))
			{
				value.XAudio29DllPath = text;
				break;
			}
			text = Path.Combine(item, platformPathComponent, Xaudio2DllName);
			if (File.Exists(text))
			{
				value.XAudio29DllPath = text;
				break;
			}
		}
		WindowsRTCOptions value2 = new WindowsRTCOptions
		{
			PlatformSpecificOptions = value
		};
		(createOptions as EOSWindowsOptions).options.RTCOptions = value2;
	}

	public void InitializeOverlay(IEOSCoroutineOwner owner)
	{
	}

	public void RegisterForPlatformNotifications()
	{
	}

	public bool IsApplicationConstrainedWhenOutOfFocus()
	{
		return false;
	}
}
