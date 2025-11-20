using System;
using System.Reflection;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class GameObject_AddComponent_Hook
{
	private static Func<GameObject, Type, Component> _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.GameObject != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.GameObject.ClrType, "Internal_AddComponentWithType", typeof(Type));
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Func<GameObject, Type, Component>>();
	}

	private static Component MM_Detour(GameObject __instance, Type componentType)
	{
		Component val = _original(__instance, componentType);
		if (val.IsKnownTextType())
		{
			IReadOnlyTextTranslationCache readOnlyTextTranslationCache = CallOrigin.CalculateTextCacheFromStackTrace(null);
			if (readOnlyTextTranslationCache != null)
			{
				val.GetOrCreateTextTranslationInfo().TextCache = readOnlyTextTranslationCache;
			}
		}
		return val;
	}
}
