using System;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.ResourceRedirector.Hooks;

internal static class Resources_Load_Hook
{
	private delegate Object OriginalMethod(string path, Type systemTypeInstance);

	private static OriginalMethod _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.Resources != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.Resources?.ClrType, "Load", typeof(string), typeof(Type));
	}

	private static void Postfix(string __0, Type __1, ref Object __result)
	{
		ResourceRedirection.Hook_ResourceLoaded_Postfix(new ResourceLoadedParameters(__0, __1, ResourceLoadType.LoadNamed), ref __result);
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<OriginalMethod>();
	}

	private static Object MM_Detour(string path, Type systemTypeInstance)
	{
		Object __result = _original(path, systemTypeInstance);
		Postfix(path, systemTypeInstance, ref __result);
		return __result;
	}
}
