using System;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class UI2DSprite_material_Hook
{
	private static Action<object, object> _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.UI2DSprite != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Property(UnityTypes.UI2DSprite?.ClrType, "material")?.GetSetMethod();
	}

	public static void Postfix(object __instance)
	{
		Texture2D texture = null;
		AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent(__instance, ref texture, isPrefixHooked: false, onEnable: false);
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Action<object, object>>();
	}

	private static void MM_Detour(object __instance, object value)
	{
		_original(__instance, value);
		Postfix(__instance);
	}
}
