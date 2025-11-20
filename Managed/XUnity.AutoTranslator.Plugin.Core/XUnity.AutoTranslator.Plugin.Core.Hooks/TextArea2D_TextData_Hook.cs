using System;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

[HookingHelperPriority(0)]
internal static class TextArea2D_TextData_Hook
{
	private static Action<Component, object> _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.TextArea2D != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Property(UnityTypes.TextArea2D.ClrType, "TextData")?.GetSetMethod();
	}

	private static void Postfix(Component __instance)
	{
		_Postfix(__instance);
	}

	private static void _Postfix(Component __instance)
	{
		AutoTranslationPlugin.Current.Hook_TextChanged(__instance, onEnable: false);
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Action<Component, object>>();
	}

	private static void MM_Detour(Component __instance, object value)
	{
		_original(__instance, value);
		Postfix(__instance);
	}
}
