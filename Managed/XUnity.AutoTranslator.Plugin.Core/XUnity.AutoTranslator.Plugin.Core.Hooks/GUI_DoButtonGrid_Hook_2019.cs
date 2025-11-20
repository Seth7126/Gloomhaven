using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

[HookingHelperPriority(0)]
internal static class GUI_DoButtonGrid_Hook_2019
{
	private delegate int OriginalMethod(Rect arg1, int arg2, GUIContent[] arg3, string[] arg4, int arg5, GUIStyle arg6, GUIStyle arg7, GUIStyle arg8, GUIStyle arg9, int arg10, bool[] arg11);

	private static OriginalMethod _original;

	private static bool Prepare(object instance)
	{
		if (UnityTypes.GUI != null)
		{
			return UnityTypes.GUI_ToolbarButtonSize != null;
		}
		return false;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.GUI.ClrType, "DoButtonGrid", typeof(Rect), typeof(int), typeof(GUIContent[]), typeof(string[]), typeof(int), typeof(GUIStyle), typeof(GUIStyle), typeof(GUIStyle), typeof(GUIStyle), UnityTypes.GUI_ToolbarButtonSize.ClrType, typeof(bool[]));
	}

	private static void Prefix(GUIContent[] contents)
	{
		foreach (GUIContent ui in contents)
		{
			AutoTranslationPlugin.Current.Hook_TextChanged(ui, onEnable: false);
		}
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<OriginalMethod>();
	}

	private static int MM_Detour(Rect arg1, int arg2, GUIContent[] arg3, string[] arg4, int arg5, GUIStyle arg6, GUIStyle arg7, GUIStyle arg8, GUIStyle arg9, int arg10, bool[] arg11)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Prefix(arg3);
		return _original(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
	}
}
