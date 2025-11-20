using System;
using System.Reflection;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.TextMeshPro;

[HookingHelperPriority(0)]
internal static class TMP_Text_maxVisibleCharacters_Hook
{
	private static Action<Component, int> _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.TMP_Text != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Property(UnityTypes.TMP_Text?.ClrType, "maxVisibleCharacters")?.GetSetMethod();
	}

	private static void Prefix(Component __instance, ref int value)
	{
		TextTranslationInfo textTranslationInfo = __instance.GetTextTranslationInfo();
		if (textTranslationInfo != null && textTranslationInfo.IsTranslated && 0 < value)
		{
			value = 99999;
		}
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Action<Component, int>>();
	}

	private static void MM_Detour(Component __instance, int value)
	{
		Prefix(__instance, ref value);
		_original(__instance, value);
	}
}
