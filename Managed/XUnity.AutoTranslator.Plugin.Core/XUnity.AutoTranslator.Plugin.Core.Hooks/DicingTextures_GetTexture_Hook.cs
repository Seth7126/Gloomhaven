using System;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class DicingTextures_GetTexture_Hook
{
	private static Func<object, string, Texture2D> _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.DicingTextures != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.DicingTextures?.ClrType, "GetTexture", typeof(string));
	}

	public static void Postfix(object __instance, ref Texture2D __result)
	{
		AutoTranslationPlugin.Current.Hook_ImageChanged(ref __result, isPrefixHooked: false);
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Func<object, string, Texture2D>>();
	}

	private static Texture2D MM_Detour(object __instance, string arg1)
	{
		Texture2D __result = _original(__instance, arg1);
		Postfix(__instance, ref __result);
		return __result;
	}
}
