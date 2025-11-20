using System;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class CubismRenderer_MainTexture_Hook
{
	private static Action<Component, Texture2D> _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.CubismRenderer != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Property(UnityTypes.CubismRenderer.ClrType, "MainTexture")?.GetSetMethod();
	}

	public static void Prefix(Component __instance, ref Texture2D value)
	{
		AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent(__instance, ref value, isPrefixHooked: true, onEnable: false);
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Action<Component, Texture2D>>();
	}

	private static void MM_Detour(Component __instance, ref Texture2D value)
	{
		Prefix(__instance, ref value);
		_original(__instance, value);
	}
}
