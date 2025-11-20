using System;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class Transform_SetParent_Hook
{
	private static Action<Transform, Transform, bool> _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.Transform != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Method(UnityTypes.Transform.ClrType, "SetParent", UnityTypes.Transform.ClrType, typeof(bool));
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<Action<Transform, Transform, bool>>();
	}

	private static void MM_Detour(Transform __instance, Transform parent, bool worldPositionStays)
	{
		if ((Object)(object)parent != (Object)null)
		{
			TransformInfo extensionData = parent.GetExtensionData<TransformInfo>();
			if (extensionData?.TextCache != null)
			{
				IReadOnlyTextTranslationCache textCache = CallOrigin.TextCache;
				CallOrigin.TextCache = extensionData.TextCache;
				try
				{
					CallOrigin.AssociateSubHierarchyWithTransformInfo(__instance, extensionData);
				}
				finally
				{
					CallOrigin.TextCache = textCache;
				}
			}
		}
		_original(__instance, parent, worldPositionStays);
	}
}
