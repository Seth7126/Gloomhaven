using System;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Extensions;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class Material_mainTexture_Hook
{
	private static Action<Material, Texture> _original;

	private static bool Prepare(object instance)
	{
		return true;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Property(typeof(Material), "mainTexture")?.GetSetMethod();
	}

	public static void Prefix(Material __instance, ref Texture value)
	{
		if (value.TryCastTo<Texture2D>(out var castedObject))
		{
			AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent(__instance, ref castedObject, isPrefixHooked: true, onEnable: false);
			value = (Texture)(object)castedObject;
		}
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Action<Material, Texture>>();
	}

	private static void MM_Detour(Material __instance, ref Texture value)
	{
		Prefix(__instance, ref value);
		_original(__instance, value);
	}
}
