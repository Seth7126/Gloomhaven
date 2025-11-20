using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.TextMeshPro;

[HookingHelperPriority(0)]
internal static class TMP_Text_SetText_Hook3
{
	private delegate void OriginalMethod(Component arg1, string arg2, float arg3, float arg4, float arg5);

	private static OriginalMethod _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.TMP_Text != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.TMP_Text.ClrType, "SetText", typeof(string), typeof(float), typeof(float), typeof(float));
	}

	private static void Postfix(Component __instance)
	{
		AutoTranslationPlugin.Current.Hook_TextChanged(__instance, onEnable: false);
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<OriginalMethod>();
	}

	private static void MM_Detour(Component __instance, string value, float arg2, float arg3, float arg4)
	{
		_original(__instance, value, arg2, arg3, arg4);
		Postfix(__instance);
	}
}
