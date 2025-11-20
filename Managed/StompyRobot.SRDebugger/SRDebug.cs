using System;
using SRDebugger.Services;
using SRF.Service;
using UnityEngine;

public static class SRDebug
{
	public const string Version = "1.12.1";

	public static Action<ConsoleEntry> CopyConsoleItemCallback = GetDefaultCopyConsoleItemCallback();

	public static bool IsInitialized { get; private set; }

	public static IDebugService Instance => SRServiceManager.GetService<IDebugService>();

	public static void Init()
	{
		IsInitialized = true;
		SRServiceManager.RegisterAssembly<IDebugService>();
		SRServiceManager.GetService<IConsoleService>();
		SRServiceManager.GetService<IDebugService>();
	}

	public static Action<ConsoleEntry> GetDefaultCopyConsoleItemCallback()
	{
		return delegate(ConsoleEntry entry)
		{
			GUIUtility.systemCopyBuffer = $"{entry.LogType}: {entry.Message}\n\r\n\r{entry.StackTrace}";
		};
	}
}
