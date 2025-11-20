using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.ResourceRedirector.Hooks;

internal static class AsyncOperation_set_priority_Hook
{
	private delegate void OriginalMethod(AsyncOperation __instance, int value);

	private static OriginalMethod _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.AsyncOperation != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Property(UnityTypes.AsyncOperation?.ClrType, "priority")?.GetSetMethod();
	}

	private static bool Prefix(AsyncOperation __instance)
	{
		return !ResourceRedirection.ShouldBlockAsyncOperationMethods(__instance);
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<OriginalMethod>();
	}

	private static void MM_Detour(AsyncOperation __instance, int value)
	{
		if (Prefix(__instance))
		{
			_original(__instance, value);
		}
	}
}
