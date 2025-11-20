using System;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.TextMeshPro;

[HookingHelperPriority(0)]
internal static class TMP_Text_SetText_Hook2
{
	private static Action<Component, string, bool> _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.TMP_Text != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.TMP_Text?.ClrType, "SetText", typeof(string), typeof(bool));
	}

	private static void Postfix(Component __instance)
	{
		AutoTranslationPlugin.Current.Hook_TextChanged(__instance, onEnable: false);
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Action<Component, string, bool>>();
	}

	private static void MM_Detour(Component __instance, string value, bool arg2)
	{
		_original(__instance, value, arg2);
		Postfix(__instance);
	}
}
