using System;
using System.Reflection;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.NGUI;

[HookingHelperPriority(0)]
internal static class UILabel_OnEnable_Hook
{
	private static Action<Component> _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.UIRect != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.UIRect?.ClrType, "OnEnable");
	}

	public static void _Postfix(Component __instance)
	{
		AutoTranslationPlugin.Current.Hook_TextChanged(__instance, onEnable: true);
	}

	private static void Postfix(Component __instance)
	{
		__instance = __instance.GetOrCreateNGUIDerivedProxy();
		if ((Object)(object)__instance != (Object)null)
		{
			_Postfix(__instance);
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
