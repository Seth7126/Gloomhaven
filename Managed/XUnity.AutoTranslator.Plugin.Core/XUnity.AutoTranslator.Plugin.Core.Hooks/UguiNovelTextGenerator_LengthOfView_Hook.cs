using System;
using System.Reflection;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class UguiNovelTextGenerator_LengthOfView_Hook
{
	private static Action<object, int> _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.UguiNovelTextGenerator != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Property(UnityTypes.UguiNovelTextGenerator?.ClrType, "LengthOfView").GetSetMethod(nonPublic: true);
	}

	private static void Prefix(ref int value)
	{
		value = -1;
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Action<object, int>>();
	}

	private static void MM_Detour(object __instance, int value)
	{
		Prefix(ref value);
		_original(__instance, value);
	}
}
