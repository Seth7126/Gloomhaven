using System;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class Object_InstantiateSingleWithParent_Hook
{
	private static Func<Object, Transform, Vector3, Quaternion, Object> _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.Object != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.Object.ClrType, "Internal_InstantiateSingleWithParent", typeof(Object), typeof(Transform), typeof(Vector3), typeof(Quaternion));
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Func<Object, Transform, Vector3, Quaternion, Object>>();
	}

	private static Object MM_Detour(Object data, Transform parent, Vector3 pos, Quaternion rot)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		IReadOnlyTextTranslationCache textCache = CallOrigin.TextCache;
		try
		{
			CallOrigin.TextCache = CallOrigin.CalculateTextCacheFromStackTrace((parent != null) ? ((Component)parent).gameObject : null);
			Object val = _original(data, parent, pos, rot);
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
