using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class IMGUIPluginTranslationHooks
{
	private static HashSet<MethodInfo> HandledMethods = new HashSet<MethodInfo>();

	private static HashSet<MethodInfo> HookedMethods = new HashSet<MethodInfo>();

	public static void HookIfConfigured(WindowFunction function)
	{
		if (AutoTranslationPlugin.Current.PluginTextCaches.Count != 0)
		{
			HookIfConfigured(((Delegate)(object)function).Method);
		}
	}

	public static void ResetHandledForAllInAssembly(Assembly assembly)
	{
		HandledMethods.RemoveWhere((MethodInfo x) => x.DeclaringType.Assembly.Equals(assembly));
	}

	public static void HookIfConfigured(MethodInfo method)
	{
		if (HandledMethods.Contains(method))
		{
			return;
		}
		HandledMethods.Add(method);
		if (HookedMethods.Contains(method))
		{
			return;
		}
		string text = method.DeclaringType.FullName.ToString() + "." + method.Name;
		try
		{
			Assembly assembly = method.DeclaringType.Assembly;
			if (AutoTranslationPlugin.Current.PluginTextCaches.TryGetValue(assembly.GetName().Name, out var value))
			{
				XuaLogger.AutoTranslator.Info("Attempting to hook " + text + " to enable plugin specific translations.");
				Type type = method.DeclaringType.Assembly.GetTypes().FirstOrDefault((Type x) => typeof(MonoBehaviour).IsAssignableFrom(x));
				if (type == null)
				{
					XuaLogger.AutoTranslator.Warn("Could not find any MonoBehaviours in assembly owning method the method: " + text);
					return;
				}
				Type type2 = type.GetType();
				CompositeTextTranslationCache orCreateCompositeCache = AutoTranslationPlugin.Current.TextCache.GetOrCreateCompositeCache(value);
				Type type3 = typeof(PluginCallbacks_Function_Hook<>).MakeGenericType(type2);
				type3.GetMethod("Register", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Invoke(null, new object[1] { method });
				type3.GetMethod("SetTextCache", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Invoke(null, new object[1] { orCreateCompositeCache });
				HookingHelper.PatchType(type3, Settings.ForceMonoModHooks);
				HookedMethods.Add(method);
				type3.GetMethod("Clean", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Invoke(null, null);
			}
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred while attempting to hook " + text + " to disable translation in window.");
		}
	}
}
