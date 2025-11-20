using System;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

[HookingHelperPriority(0)]
internal static class GUI_DoLabel_Hook
{
	private static Action<Rect, GUIContent, IntPtr> _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.GUI != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.GUI.ClrType, "DoLabel", typeof(Rect), typeof(GUIContent), typeof(IntPtr));
	}

	private static void Prefix(GUIContent content)
	{
		AutoTranslationPlugin.Current.Hook_TextChanged(content, onEnable: false);
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Action<Rect, GUIContent, IntPtr>>();
	}

	private static void MM_Detour(Rect arg1, GUIContent arg2, IntPtr arg3)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Prefix(arg2);
		_original(arg1, arg2, arg3);
	}
}
