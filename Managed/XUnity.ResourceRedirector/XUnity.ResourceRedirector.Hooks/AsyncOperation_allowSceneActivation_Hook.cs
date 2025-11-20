using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.ResourceRedirector.Hooks;

internal static class AsyncOperation_allowSceneActivation_Hook
{
	private delegate bool OriginalMethod(AsyncOperation __instance);

	private static OriginalMethod _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.AsyncOperation != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Property(UnityTypes.AsyncOperation?.ClrType, "allowSceneActivation")?.GetGetMethod();
	}

	private static bool Prefix(AsyncOperation __instance, ref bool __result)
	{
		if (ResourceRedirection.ShouldBlockAsyncOperationMethods(__instance))
		{
			__result = true;
			return false;
		}
		return true;
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<OriginalMethod>();
	}

	private static bool MM_Detour(AsyncOperation __instance)
	{
		bool __result = false;
		if (Prefix(__instance, ref __result))
		{
			__result = _original(__instance);
		}
		return __result;
	}
}
