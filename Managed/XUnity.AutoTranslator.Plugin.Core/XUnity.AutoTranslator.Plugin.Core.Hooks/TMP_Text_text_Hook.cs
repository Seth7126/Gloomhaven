using System;
using System.Reflection;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class TMP_Text_text_Hook
{
	private static Func<object, string> _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.TMP_Text != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Property(UnityTypes.TMP_Text.ClrType, "text")?.GetGetMethod();
	}

	private static void Postfix(object __instance, ref string __result)
	{
		TextGetterCompatModeHelper.ReplaceTextWithOriginal(__instance, ref __result);
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Func<object, string>>();
	}

	private static string MM_Detour(object __instance)
	{
		string __result = _original(__instance);
		Postfix(__instance, ref __result);
		return __result;
	}
}
