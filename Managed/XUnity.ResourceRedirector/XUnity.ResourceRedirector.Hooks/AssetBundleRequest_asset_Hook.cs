using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.ResourceRedirector.Hooks;

internal static class AssetBundleRequest_asset_Hook
{
	private delegate Object OriginalMethod(AssetBundleRequest __instance);

	private static OriginalMethod _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.AssetBundleRequest != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Property(UnityTypes.AssetBundleRequest?.ClrType, "asset")?.GetGetMethod();
	}

	private static bool Prefix(AssetBundleRequest __instance, ref Object __result, ref AsyncAssetLoadInfo __state)
	{
		if (ResourceRedirection.TryGetAssetBundleLoadInfo(__instance, out __state))
		{
			if (__state.ResolveType == AsyncAssetLoadingResolve.ThroughAssets)
			{
				Object[] assets = __state.Assets;
				if (assets != null && assets.Length != 0)
				{
					__result = assets[0];
				}
				return false;
			}
			return true;
		}
		return true;
	}

	private static void Postfix(ref Object __result, ref AsyncAssetLoadInfo __state)
	{
		if (__state != null && !__state.SkipAllPostfixes)
		{
			ResourceRedirection.Hook_AssetLoaded_Postfix(__state.Parameters, __state.Bundle, ref __result);
		}
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<OriginalMethod>();
	}

	private static Object MM_Detour(AssetBundleRequest __instance)
	{
		Object __result = null;
		AsyncAssetLoadInfo __state = null;
		if (Prefix(__instance, ref __result, ref __state))
		{
			__result = _original(__instance);
		}
		Postfix(ref __result, ref __state);
		return __result;
	}
}
