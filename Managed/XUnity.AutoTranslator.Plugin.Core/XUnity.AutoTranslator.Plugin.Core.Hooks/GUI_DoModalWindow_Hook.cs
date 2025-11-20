using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

[HookingHelperPriority(0)]
internal static class GUI_DoModalWindow_Hook
{
	private delegate Rect OriginalMethod(int arg1, Rect arg2, WindowFunction arg3, GUIContent arg4, GUIStyle arg5, GUISkin arg6);

	private static OriginalMethod _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.GUI != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.GUI.ClrType, "DoModalWindow", typeof(int), typeof(Rect), typeof(WindowFunction), typeof(GUIContent), typeof(GUIStyle), typeof(GUISkin));
	}

	private static void Prefix(int id, WindowFunction func, GUIContent content)
	{
		IMGUIBlocker.BlockIfConfigured(func, id);
		IMGUIPluginTranslationHooks.HookIfConfigured(func);
		AutoTranslationPlugin.Current.Hook_TextChanged(content, onEnable: false);
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<OriginalMethod>();
	}

	private static Rect MM_Detour(int arg1, Rect arg2, WindowFunction arg3, GUIContent arg4, GUIStyle arg5, GUISkin arg6)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Prefix(arg1, arg3, arg4);
		return _original(arg1, arg2, arg3, arg4, arg5, arg6);
	}
}
