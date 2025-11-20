using System;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Extensions;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class RawImage_texture_Hook
{
	private static Action<Component, Texture> _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.RawImage != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Property(UnityTypes.RawImage?.ClrType, "texture")?.GetSetMethod();
	}

	public static void Prefix(Component __instance, ref Texture value)
	{
		if (value.TryCastTo<Texture2D>(out var castedObject))
		{
			AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent(__instance, ref castedObject, isPrefixHooked: true, onEnable: false);
			value = (Texture)(object)castedObject;
		}
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Action<Component, Texture>>();
	}

	private static void MM_Detour(Component __instance, Texture value)
	{
		Prefix(__instance, ref value);
		_original(__instance, value);
	}
}
