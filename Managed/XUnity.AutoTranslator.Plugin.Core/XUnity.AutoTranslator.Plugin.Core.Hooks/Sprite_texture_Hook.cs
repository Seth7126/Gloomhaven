using System;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class Sprite_texture_Hook
{
	private static Func<Sprite, Texture2D> _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.Sprite != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Property(UnityTypes.Sprite?.ClrType, "texture")?.GetGetMethod();
	}

	private static void Postfix(ref Texture2D __result)
	{
		AutoTranslationPlugin.Current.Hook_ImageChanged(ref __result, isPrefixHooked: true);
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Func<Sprite, Texture2D>>();
	}

	private static Texture2D MM_Detour(Sprite __instance)
	{
		Texture2D __result = _original(__instance);
		Postfix(ref __result);
		return __result;
	}
}
