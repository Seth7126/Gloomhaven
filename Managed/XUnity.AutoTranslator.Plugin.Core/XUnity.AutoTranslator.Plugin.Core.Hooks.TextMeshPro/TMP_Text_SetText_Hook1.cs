using System;
using System.Reflection;
using System.Text;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.TextMeshPro;

[HookingHelperPriority(0)]
internal static class TMP_Text_SetText_Hook1
{
	private static Action<Component, StringBuilder> _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.TMP_Text != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.TMP_Text?.ClrType, "SetText", typeof(StringBuilder));
	}

	private static void Postfix(Component __instance)
	{
		AutoTranslationPlugin.Current.Hook_TextChanged(__instance, onEnable: false);
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Action<Component, StringBuilder>>();
	}

	private static void MM_Detour(Component __instance, StringBuilder value)
	{
		_original(__instance, value);
		Postfix(__instance);
	}
}
