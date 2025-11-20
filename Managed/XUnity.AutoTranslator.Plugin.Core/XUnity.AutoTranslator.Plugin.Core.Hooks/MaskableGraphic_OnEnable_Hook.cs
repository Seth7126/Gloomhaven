using System;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Extensions;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class MaskableGraphic_OnEnable_Hook
{
	private static Action<Component> _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.MaskableGraphic != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.MaskableGraphic?.ClrType, "OnEnable");
	}

	public static void Postfix(Component __instance)
	{
		Type unityType = __instance.GetUnityType();
		if ((UnityTypes.Image != null && UnityTypes.Image.IsAssignableFrom(unityType)) || (UnityTypes.RawImage != null && UnityTypes.RawImage.IsAssignableFrom(unityType)))
		{
			Texture2D texture = null;
			AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent(__instance, ref texture, isPrefixHooked: false, onEnable: true);
		}
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Action<Component>>();
	}

	private static void MM_Detour(Component __instance)
	{
		_original(__instance);
		Postfix(__instance);
	}
}
