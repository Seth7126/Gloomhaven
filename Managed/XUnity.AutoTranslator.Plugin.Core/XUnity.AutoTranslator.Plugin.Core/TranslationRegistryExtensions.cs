using System;
using System.Diagnostics;
using System.Reflection;

namespace XUnity.AutoTranslator.Plugin.Core;

public static class TranslationRegistryExtensions
{
	private static Assembly GetCallingPlugin()
	{
		MethodBase method = new StackFrame(2).GetMethod();
		if (method != null)
		{
			return method.DeclaringType.Assembly;
		}
		throw new ArgumentException("Could not automatically determine the calling plugin. Consider calling the overload of this method taking an assembly name.");
	}

	public static void RegisterPluginSpecificTranslations(this ITranslationRegistry registry, StreamTranslationPackage package)
	{
		Assembly callingPlugin = GetCallingPlugin();
		registry.RegisterPluginSpecificTranslations(callingPlugin, package);
	}

	public static void RegisterPluginSpecificTranslations(this ITranslationRegistry registry, KeyValuePairTranslationPackage package)
	{
		Assembly callingPlugin = GetCallingPlugin();
		registry.RegisterPluginSpecificTranslations(callingPlugin, package);
	}

	public static void EnablePluginTranslationFallback(this ITranslationRegistry registry)
	{
		Assembly callingPlugin = GetCallingPlugin();
		registry.EnablePluginTranslationFallback(callingPlugin);
	}
}
