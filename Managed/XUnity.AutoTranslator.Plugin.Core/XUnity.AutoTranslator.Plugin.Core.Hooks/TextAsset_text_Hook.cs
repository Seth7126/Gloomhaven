using System.Reflection;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.AssetRedirection;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class TextAsset_text_Hook
{
	private delegate string OriginalMethod(TextAsset __instance);

	private static OriginalMethod _original;

	private static bool Prepare(object instance)
	{
		return UnityTypes.TextAsset != null;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return AccessToolsShim.Property(UnityTypes.TextAsset?.ClrType, "text")?.GetGetMethod();
	}

	private static void Postfix(TextAsset __instance, ref string __result)
	{
		if (__result != null)
		{
			TextAssetExtensionData extensionData = __instance.GetExtensionData<TextAssetExtensionData>();
			if (extensionData != null)
			{
				__result = extensionData.Text;
			}
		}
	}

	private static void MM_Init(object detour)
	{
		_original = detour.GenerateTrampolineEx<OriginalMethod>();
	}

	private static string MM_Detour(TextAsset __instance)
	{
		string __result = _original(__instance);
		Postfix(__instance, ref __result);
		return __result;
	}
}
