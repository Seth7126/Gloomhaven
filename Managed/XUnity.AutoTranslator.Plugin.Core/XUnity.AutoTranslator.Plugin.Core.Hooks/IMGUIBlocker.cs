using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class IMGUIBlocker
{
	private static HashSet<MethodInfo> HandledMethods = new HashSet<MethodInfo>();

	public static void BlockIfConfigured(WindowFunction function, int windowId)
	{
		if (Settings.BlacklistedIMGUIPlugins.Count == 0)
		{
			return;
		}
		MethodInfo method = ((Delegate)(object)function).Method;
		if (HandledMethods.Contains(method))
		{
			return;
		}
		HandledMethods.Add(method);
		try
		{
			if (IsBlackslisted(method))
			{
				XuaLogger.AutoTranslator.Info("Attempting to hook " + method.DeclaringType.FullName.ToString() + "." + method.Name + " to disable translation in window.");
				IMGUIWindow_Function_Hook.Register(method);
				HookingHelper.PatchType(typeof(IMGUIWindow_Function_Hook), Settings.ForceMonoModHooks);
				IMGUIWindow_Function_Hook.Clean();
			}
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred while attempting to hook " + method.DeclaringType.FullName.ToString() + " to disable translation in window.");
		}
	}

	public static bool IsBlackslisted(MethodInfo method)
	{
		Type declaringType = method.DeclaringType;
		Assembly assembly = declaringType.Assembly;
		if (assembly == typeof(IMGUIBlocker).Assembly)
		{
			return false;
		}
		if (!AutoTranslationPlugin.Current.IsTemporarilyDisabled() && !IsBlacklistedName(method.Name) && !IsBlacklistedName(declaringType.Name))
		{
			return IsBlacklistedName(assembly.GetName().Name);
		}
		return true;
	}

	public static bool IsBlacklistedName(string name)
	{
		return Settings.BlacklistedIMGUIPlugins.Any((string x) => name.IndexOf(x, StringComparison.OrdinalIgnoreCase) > -1);
	}
}
