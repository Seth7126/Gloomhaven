using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.ResourceRedirector.Hooks;

internal static class AssetBundleCreateRequest_assetBundle_Hook
{
	private delegate AssetBundle OriginalMethod(AssetBundleCreateRequest __instance);

	private static OriginalMethod _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.AssetBundleCreateRequest != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Property(UnityTypes.AssetBundleCreateRequest?.ClrType, "assetBundle")?.GetGetMethod();
	}

	private static bool Prefix(AssetBundleCreateRequest __instance, ref AssetBundle __result, ref AsyncAssetBundleLoadInfo __state)
	{
		if (ResourceRedirection.TryGetAssetBundle(__instance, out __state))
		{
			if (__state.ResolveType == AsyncAssetBundleLoadingResolve.ThroughBundle)
			{
				__result = __state.Bundle;
				return false;
			}
			return true;
		}
		return true;
	}

	private static void Postfix(ref AssetBundle __result, ref AsyncAssetBundleLoadInfo __state)
	{
		if (__state == null)
		{
			return;
		}
		if (!__state.SkipAllPostfixes)
		{
			ResourceRedirection.Hook_AssetBundleLoaded_Postfix(__state.Parameters, ref __result);
		}
		if ((Object)(object)__result != (Object)null && __state != null)
		{
			string path = __state.Parameters.Path;
			if (path != null)
			{
				__result.GetOrCreateExtensionData<AssetBundleExtensionData>().Path = path;
			}
		}
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<OriginalMethod>();
	}

	private static AssetBundle MM_Detour(AssetBundleCreateRequest __instance)
	{
		AssetBundle __result = null;
		AsyncAssetBundleLoadInfo __state = null;
		if (Prefix(__instance, ref __result, ref __state))
		{
			__result = _original(__instance);
		}
		Postfix(ref __result, ref __state);
		return __result;
	}
}
