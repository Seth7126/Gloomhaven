using System.Reflection;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.AssetRedirection;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class TextAsset_bytes_Hook
{
	private delegate byte[] OriginalMethod(TextAsset __instance);

	private static OriginalMethod _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.TextAsset != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Property(UnityTypes.TextAsset?.ClrType, "bytes")?.GetGetMethod();
	}

	private static void Postfix(TextAsset __instance, ref byte[] __result)
	{
		if (__result != null)
		{
			TextAssetExtensionData extensionData = __instance.GetExtensionData<TextAssetExtensionData>();
			if (extensionData != null)
			{
				__result = extensionData.Data;
			}
		}
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<OriginalMethod>();
	}

	private static byte[] MM_Detour(TextAsset __instance)
	{
		byte[] __result = _original(__instance);
		Postfix(__instance, ref __result);
		return __result;
	}
}
