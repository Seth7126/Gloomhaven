using System;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class UIAtlas_spriteMaterial_Hook
{
	private static Action<object, Material> _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.UIAtlas != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Property(UnityTypes.UIAtlas?.ClrType, "spriteMaterial")?.GetSetMethod();
	}

	public static void Postfix(object __instance)
	{
		Texture2D texture = null;
		AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent(__instance, ref texture, isPrefixHooked: false, onEnable: false);
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Action<object, Material>>();
	}

	private static void MM_Detour(object __instance, Material value)
	{
		_original(__instance, value);
		Postfix(__instance);
	}
}
