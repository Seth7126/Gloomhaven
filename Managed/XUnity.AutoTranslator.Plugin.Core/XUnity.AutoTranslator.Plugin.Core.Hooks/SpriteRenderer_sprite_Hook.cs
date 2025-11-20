using System;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class SpriteRenderer_sprite_Hook
{
	private static Action<SpriteRenderer, Sprite> _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.SpriteRenderer != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Property(UnityTypes.SpriteRenderer?.ClrType, "sprite")?.GetSetMethod();
	}

	public static void Prefix(SpriteRenderer __instance, ref Sprite value)
	{
		bool imageHooksEnabled = CallOrigin.ImageHooksEnabled;
		CallOrigin.ImageHooksEnabled = false;
		Texture2D texture;
		try
		{
			texture = value.texture;
		}
		finally
		{
			CallOrigin.ImageHooksEnabled = imageHooksEnabled;
		}
		AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent(__instance, ref value, ref texture, isPrefixHooked: true, onEnable: false);
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Action<SpriteRenderer, Sprite>>();
	}

	private static void MM_Detour(SpriteRenderer __instance, Sprite sprite)
	{
		Prefix(__instance, ref sprite);
		_original(__instance, sprite);
	}
}
