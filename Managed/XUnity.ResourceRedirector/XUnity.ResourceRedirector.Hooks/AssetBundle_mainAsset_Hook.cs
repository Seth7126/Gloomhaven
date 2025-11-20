using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.ResourceRedirector.Hooks;

internal static class AssetBundle_mainAsset_Hook
{
	private delegate Object OriginalMethod(AssetBundle __instance);

	private static OriginalMethod _original;

	private static bool Prepare(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.AssetBundle?.ClrType, "returnMainAsset", typeof(AssetBundle)) == null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Property(UnityTypes.AssetBundle?.ClrType, "mainAsset")?.GetGetMethod();
	}

	private static bool Prefix(AssetBundle __instance, ref Object __result, ref AssetLoadingContext __state)
	{
		AssetLoadingParameters parameters = new AssetLoadingParameters(null, null, AssetLoadType.LoadMainAsset);
		__state = ResourceRedirection.Hook_AssetLoading_Prefix(parameters, __instance, ref __result);
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

	private static Object MM_Detour(AssetBundle __instance)
	{
		Object __result = null;
		AssetLoadingContext __state = null;
		if (Prefix(__instance, ref __result, ref __state))
		{
			__result = _original(__instance);
		}
		Postfix(__instance, ref __result, ref __state);
		return __result;
	}
}
