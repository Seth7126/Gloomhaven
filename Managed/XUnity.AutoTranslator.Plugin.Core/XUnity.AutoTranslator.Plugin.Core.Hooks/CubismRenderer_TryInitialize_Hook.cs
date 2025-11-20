using System;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class CubismRenderer_TryInitialize_Hook
{
	private static Action<Component> _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.CubismRenderer != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.CubismRenderer.ClrType, "TryInitialize");
	}

	public static void Prefix(Component __instance)
	{
		Texture2D texture = null;
		AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent(__instance, ref texture, isPrefixHooked: true, onEnable: true);
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Action<Component>>();
	}

	private static void MM_Detour(Component __instance)
	{
		Prefix(__instance);
		_original(__instance);
	}
}
