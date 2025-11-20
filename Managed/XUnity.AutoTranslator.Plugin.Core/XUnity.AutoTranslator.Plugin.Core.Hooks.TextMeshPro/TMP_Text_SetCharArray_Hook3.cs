using System;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.TextMeshPro;

[HookingHelperPriority(0)]
internal static class TMP_Text_SetCharArray_Hook3
{
	private static Action<Component, int[], int, int> _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.TMP_Text != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.TMP_Text?.ClrType, "SetCharArray", typeof(int[]), typeof(int), typeof(int));
	}

	private static void Postfix(Component __instance)
	{
		AutoTranslationPlugin.Current.Hook_TextChanged(__instance, onEnable: false);
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Action<Component, int[], int, int>>();
	}

	private static void MM_Detour(Component __instance, int[] value, int arg2, int arg3)
	{
		_original(__instance, value, arg2, arg3);
		Postfix(__instance);
	}
}
