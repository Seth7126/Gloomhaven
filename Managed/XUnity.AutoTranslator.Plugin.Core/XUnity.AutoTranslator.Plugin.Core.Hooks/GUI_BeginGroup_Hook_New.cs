using System;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

[HookingHelperPriority(0)]
internal static class GUI_BeginGroup_Hook_New
{
	private static Action<Rect, GUIContent, GUIStyle, Vector2> _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.GUI != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.GUI.ClrType, "BeginGroup", typeof(Rect), typeof(GUIContent), typeof(GUIStyle), typeof(Vector2));
	}

	private static void Prefix(GUIContent content)
	{
		AutoTranslationPlugin.Current.Hook_TextChanged(content, onEnable: false);
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Action<Rect, GUIContent, GUIStyle, Vector2>>();
	}

	private static void MM_Detour(Rect arg1, GUIContent arg2, GUIStyle arg3, Vector2 arg4)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		Prefix(arg2);
		_original(arg1, arg2, arg3, arg4);
	}
}
