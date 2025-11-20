using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.ResourceRedirector.Hooks;

internal static class AsyncOperation_progress_Hook
{
	private delegate float OriginalMethod(AsyncOperation __instance);

	private static OriginalMethod _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.AsyncOperation != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Property(UnityTypes.AsyncOperation?.ClrType, "progress")?.GetGetMethod();
	}

	private static bool Prefix(AsyncOperation __instance, ref float __result)
	{
		if (ResourceRedirection.ShouldBlockAsyncOperationMethods(__instance))
		{
			__result = 1f;
			return false;
		}
		return true;
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<OriginalMethod>();
	}

	private static float MM_Detour(AsyncOperation __instance)
	{
		float __result = 0f;
		if (Prefix(__instance, ref __result))
		{
			__result = _original(__instance);
		}
		return __result;
	}
}
