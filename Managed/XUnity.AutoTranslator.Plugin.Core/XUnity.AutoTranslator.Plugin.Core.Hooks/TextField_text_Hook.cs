using System;
using System.Reflection;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

[HookingHelperPriority(0)]
internal static class TextField_text_Hook
{
	private static Action<object, string> _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.TextField != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Property(UnityTypes.TextField?.ClrType, "text")?.GetSetMethod();
	}

	private static void Postfix(object __instance)
	{
		AutoTranslationPlugin.Current.Hook_TextChanged(__instance, onEnable: false);
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
