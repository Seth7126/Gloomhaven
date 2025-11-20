using System;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.TextMeshPro;

[HookingHelperPriority(0)]
internal static class TeshMeshProUGUI_OnEnable_Hook
{
	private static Action<Component> _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.TextMeshProUGUI != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.TextMeshProUGUI?.ClrType, "OnEnable");
	}

	private static void _Postfix(Component __instance)
	{
		AutoTranslationPlugin.Current.Hook_TextChanged(__instance, onEnable: true);
	}

	private static void Postfix(Component __instance)
	{
		_Postfix(__instance);
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Action<Component>>();
	}

	private static void MM_Detour(Component __instance)
	{
		_original(__instance);
		Postfix(__instance);
	}
}
