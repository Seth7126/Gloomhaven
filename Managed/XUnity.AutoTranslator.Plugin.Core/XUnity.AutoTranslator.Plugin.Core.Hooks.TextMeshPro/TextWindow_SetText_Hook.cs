using System;
using System.Reflection;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.TextMeshPro;

[HookingHelperPriority(0)]
internal static class TextWindow_SetText_Hook
{
	private static Action<object, string> _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.TextWindow != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.TextWindow.ClrType, "SetText", typeof(string));
	}

	private static void Postfix(object __instance)
	{
		Settings.SetCurText?.Invoke(__instance);
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Action<object, string>>();
	}

	private static void MM_Detour(object __instance, string value)
	{
		_original(__instance, value);
		Postfix(__instance);
	}
}
