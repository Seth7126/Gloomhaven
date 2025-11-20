using System;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.ResourceRedirector.Hooks;

internal static class AssetBundle_LoadAssetWithSubAssetsAsync_Internal_Hook
{
	private delegate AssetBundleRequest OriginalMethod(AssetBundle __instance, string name, Type type);

	private static OriginalMethod _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.AssetBundle != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.AssetBundle?.ClrType, "LoadAssetWithSubAssetsAsync_Internal", typeof(string), typeof(Type));
	}

	private static bool Prefix(AssetBundle __instance, ref string name, ref Type type, ref AssetBundleRequest __result, ref AsyncAssetLoadingContext __state)
	{
		if (name == string.Empty)
		{
			AssetLoadingParameters parameters = new AssetLoadingParameters(null, type, AssetLoadType.LoadByType);
			__state = ResourceRedirection.Hook_AsyncAssetLoading_Prefix(parameters, __instance, ref __result);
		}
		else
		{
			AssetLoadingParameters parameters2 = new AssetLoadingParameters(name, type, AssetLoadType.LoadNamedWithSubAssets);
			__state = ResourceRedirection.Hook_AsyncAssetLoading_Prefix(parameters2, __instance, ref __result);
		}
		AssetLoadingParameters parameters3 = __state.Parameters;
		name = parameters3.Name;
		type = parameters3.Type;
		return !__state.SkipOriginalCall;
	}

	private static void Postfix(ref AssetBundleRequest __result, ref AsyncAssetLoadingContext __state)
	{
		ResourceRedirection.Hook_AssetLoading_Postfix(__state, __result);
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<OriginalMethod>();
	}

	private static AssetBundleRequest MM_Detour(AssetBundle __instance, string name, Type type)
	{
		AssetBundleRequest __result = null;
		AsyncAssetLoadingContext __state = null;
		if (Prefix(__instance, ref name, ref type, ref __result, ref __state))
		{
			__result = _original(__instance, name, type);
		}
		Postfix(ref __result, ref __state);
		return __result;
	}
}
