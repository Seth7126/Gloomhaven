using System;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core;

public static class AutoTranslatorState
{
	public static int TranslationCount => Settings.TranslationCount;

	public static bool PluginInitialized { get; private set; }

	public static event Action PluginInitializationCompleted;

	internal static void OnPluginInitializationCompleted()
	{
		if (PluginInitialized)
		{
			return;
		}
		PluginInitialized = true;
		if (AutoTranslatorState.PluginInitializationCompleted == null)
		{
			return;
		}
		try
		{
			AutoTranslatorState.PluginInitializationCompleted();
		}
		catch (Exception ex)
		{
			XuaLogger.AutoTranslator.Error("Subscriber crash in PluginInitializationCompleted event: " + ex);
		}
	}
}
