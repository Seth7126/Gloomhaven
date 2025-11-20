using System;
using System.Reflection;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class IMGUIWindow_Function_Hook
{
	private static MethodInfo _nextMethod;

	public static void Register(MethodInfo method)
	{
		_nextMethod = method;
	}

	public static void Clean()
	{
		_nextMethod = null;
	}

	private static bool Prepare(object instance)
	{
		return true;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return _nextMethod;
	}

	private static void Prefix()
	{
		AutoTranslationPlugin.Current.DisableAutoTranslator();
	}

	private static void Finalizer()
	{
		AutoTranslationPlugin.Current.EnableAutoTranslator();
	}

	private static MethodInfo Get_MM_Detour()
	{
		if (_nextMethod.IsStatic)
		{
			return typeof(IMGUIWindow_Function_Hook).GetMethod("MM_Detour_Static", BindingFlags.Static | BindingFlags.NonPublic);
		}
		return typeof(IMGUIWindow_Function_Hook).GetMethod("MM_Detour_Instance", BindingFlags.Static | BindingFlags.NonPublic);
	}

	private static void MM_Detour_Instance(Action<object, int> orig, object self, int id)
	{
		try
		{
			AutoTranslationPlugin.Current.DisableAutoTranslator();
			orig(self, id);
		}
		finally
		{
			AutoTranslationPlugin.Current.EnableAutoTranslator();
		}
	}

	private static void MM_Detour_Static(Action<int> orig, int id)
	{
		try
		{
			AutoTranslationPlugin.Current.DisableAutoTranslator();
			orig(id);
		}
		finally
		{
			AutoTranslationPlugin.Current.EnableAutoTranslator();
		}
	}
}
