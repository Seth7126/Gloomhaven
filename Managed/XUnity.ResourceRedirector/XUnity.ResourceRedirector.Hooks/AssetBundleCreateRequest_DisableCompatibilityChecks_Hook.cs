using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.ResourceRedirector.Hooks;

internal static class AssetBundleCreateRequest_DisableCompatibilityChecks_Hook
{
	private delegate void OriginalMethod(AssetBundleCreateRequest __instance);

	private static OriginalMethod _original;

	private static bool Prepare(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.AssetBundleCreateRequest?.ClrType, "SetEnableCompatibilityChecks", typeof(bool)) == null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.AssetBundleCreateRequest?.ClrType, "DisableCompatibilityChecks");
	}

	private static bool Prefix(AssetBundleCreateRequest __instance)
	{
		return !ResourceRedirection.ShouldBlockAsyncOperationMethods(__instance);
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<OriginalMethod>();
	}

	private static void MM_Detour(AssetBundleCreateRequest __instance)
	{
		if (Prefix(__instance))
		{
			_original(__instance);
		}
	}
}
