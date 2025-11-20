using System;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class Image_overrideSprite_Hook
{
	private static Action<Component, Sprite> _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.Image != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Property(UnityTypes.Image?.ClrType, "overrideSprite")?.GetSetMethod();
	}

	public static void Postfix(Component __instance)
	{
		Texture2D texture = null;
		AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent(__instance, ref texture, isPrefixHooked: false, onEnable: false);
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Action<Component, Sprite>>();
	}

	private static void MM_Detour(Component __instance, Sprite value)
	{
		_original(__instance, value);
		Postfix(__instance);
	}
}
