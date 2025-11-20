using System;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.UGUI;

[HookingHelperPriority(0)]
internal static class Text_text_Hook
{
	private static Action<Component, string> _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.Text != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Property(UnityTypes.Text?.ClrType, "text")?.GetSetMethod();
	}

	private static void _Postfix(Component __instance)
	{
		AutoTranslationPlugin.Current.Hook_TextChanged(__instance, onEnable: false);
	}

	private static void Postfix(Component __instance)
	{
		_Postfix(__instance);
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Action<Component, string>>();
	}

	private static void MM_Detour(Component __instance, string value)
	{
		_original(__instance, value);
		Postfix(__instance);
	}
}
