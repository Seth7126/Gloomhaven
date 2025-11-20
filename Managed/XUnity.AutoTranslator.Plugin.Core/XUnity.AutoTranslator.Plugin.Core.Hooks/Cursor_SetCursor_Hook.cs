using System;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class Cursor_SetCursor_Hook
{
	private static Action<Texture2D, Vector2, CursorMode> _original;

	private static bool Prepare(object instance)
	{
		return true;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(typeof(Cursor), "SetCursor", typeof(Texture2D), typeof(Vector2), typeof(CursorMode));
	}

	public static void Prefix(ref Texture2D texture)
	{
		AutoTranslationPlugin.Current.Hook_ImageChanged(ref texture, isPrefixHooked: true);
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Action<Texture2D, Vector2, CursorMode>>();
	}

	private static void MM_Detour(Texture2D texture, Vector2 arg2, CursorMode arg3)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		Prefix(ref texture);
		_original(texture, arg2, arg3);
	}
}
