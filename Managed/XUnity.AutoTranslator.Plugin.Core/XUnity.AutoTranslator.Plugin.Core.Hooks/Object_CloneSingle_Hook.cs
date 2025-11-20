using System;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class Object_CloneSingle_Hook
{
	private static Func<Object, Object> _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.Object != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.Object.ClrType, "Internal_CloneSingle", typeof(Object));
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Func<Object, Object>>();
	}

	private static Object MM_Detour(Object data)
	{
		IReadOnlyTextTranslationCache textCache = CallOrigin.TextCache;
		try
		{
			CallOrigin.TextCache = CallOrigin.CalculateTextCacheFromStackTrace(null);
			Object val = _original(data);
			if (CallOrigin.TextCache != null)
			{
				GameObject val2 = (GameObject)(object)((val is GameObject) ? val : null);
				if (val2 != null && !val2.activeInHierarchy)
				{
					val2.SetTextCacheForAllObjectsInHierachy(CallOrigin.TextCache);
				}
			}
			return val;
		}
		finally
		{
			CallOrigin.TextCache = textCache;
		}
	}
}
