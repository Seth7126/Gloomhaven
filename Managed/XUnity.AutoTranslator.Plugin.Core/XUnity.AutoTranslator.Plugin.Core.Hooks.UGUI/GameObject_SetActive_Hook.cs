using System;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.UGUI;

[HookingHelperPriority(0)]
internal static class GameObject_SetActive_Hook
{
	private static Action<GameObject, bool> _original;

	private static bool Prepare(object instance)
	{
		if (UnityTypes.GameObject != null)
		{
			return UnityTypes.TextMesh != null;
		}
		return false;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.GameObject.ClrType, "SetActive", typeof(bool));
	}

	private static void _Postfix(GameObject __instance, bool value)
	{
		if (value)
		{
			Component[] componentsInChildren = __instance.GetComponentsInChildren(UnityTypes.TextMesh.UnityType);
			foreach (Component ui in componentsInChildren)
			{
				AutoTranslationPlugin.Current.Hook_TextChanged(ui, onEnable: true);
			}
		}
	}

	private static void Postfix(GameObject __instance, bool value)
	{
		_Postfix(__instance, value);
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Action<GameObject, bool>>();
	}

	private static void MM_Detour(GameObject __instance, bool value)
	{
		_original(__instance, value);
		Postfix(__instance, value);
	}
}
