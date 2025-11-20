using System;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.ResourceRedirector.Hooks;

internal static class AssetBundle_LoadAsset_Internal_Hook
{
	private delegate Object OriginalMethod(AssetBundle __instance, string name, Type type);

	private static OriginalMethod _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.AssetBundle != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.AssetBundle?.ClrType, "LoadAsset_Internal", typeof(string), typeof(Type));
	}

	private static bool Prefix(AssetBundle __instance, ref string name, ref Type type, ref Object __result, ref AssetLoadingContext __state)
	{
		AssetLoadingParameters parameters = new AssetLoadingParameters(name, type, AssetLoadType.LoadNamed);
		__state = ResourceRedirection.Hook_AssetLoading_Prefix(parameters, __instance, ref __result);
		AssetLoadingParameters parameters2 = __state.Parameters;
		name = parameters2.Name;
		type = parameters2.Type;
		return !__state.SkipOriginalCall;
	}

	private static void Postfix(AssetBundle __instance, ref Object __result, ref AssetLoadingContext __state)
	{
		if (!__state.SkipAllPostfixes)
		{
			ResourceRedirection.Hook_AssetLoaded_Postfix(__state.Parameters, __instance, ref __result);
		}
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<OriginalMethod>();
	}

	private static Object MM_Detour(AssetBundle __instance, string name, Type type)
	{
		Object __result = null;
		AssetLoadingContext __state = null;
		if (Prefix(__instance, ref name, ref type, ref __result, ref __state))
		{
			__result = _original(__instance, name, type);
		}
		Postfix(__instance, ref __result, ref __state);
		return __result;
	}
}
