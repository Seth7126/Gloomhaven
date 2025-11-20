using SRDebugger.Services;
using SRF.Service;
using UnityEngine;

namespace SRDebugger;

public static class AutoInitialize
{
	private const RuntimeInitializeLoadType InitializeLoadType = RuntimeInitializeLoadType.SubsystemRegistration;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	public static void OnLoadBeforeScene()
	{
		SRServiceManager.RegisterAssembly<IDebugService>();
		if (Settings.Instance.IsEnabled)
		{
			SRServiceManager.GetService<IConsoleService>();
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	public static void OnLoad()
	{
		if (Settings.Instance.IsEnabled)
		{
			SRDebug.Init();
		}
	}
}
