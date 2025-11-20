using System;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.ResourceRedirector.Hooks;

internal static class Resources_GetBuiltinResource_New_Hook
{
	private delegate Object OriginalMethod(Type systemTypeInstance, string path);

	private static OriginalMethod _original;

	private static bool Prepare(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.Resources?.ClrType, "GetBuiltinResource", typeof(string), typeof(Type)) == null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.Resources?.ClrType, "GetBuiltinResource", typeof(Type), typeof(string));
	}

	private static void Postfix(Type __0, string __1, ref Object __result)
	{
		ResourceRedirection.Hook_ResourceLoaded_Postfix(new ResourceLoadedParameters(__1, __0, ResourceLoadType.LoadNamedBuiltIn), ref __result);
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<OriginalMethod>();
	}

	private static Object MM_Detour(Type systemTypeInstance, string path)
	{
		Object __result = _original(systemTypeInstance, path);
		Postfix(systemTypeInstance, path, ref __result);
		return __result;
	}
}
