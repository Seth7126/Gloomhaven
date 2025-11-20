using System;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.TextMeshPro;

internal static class TextMeshProHooks
{
	public static readonly Type[] All = new Type[12]
	{
		typeof(TeshMeshProUGUI_OnEnable_Hook),
		typeof(TeshMeshPro_OnEnable_Hook),
		typeof(TMP_Text_text_Hook),
		typeof(TMP_Text_SetText_Hook1),
		typeof(TMP_Text_SetText_Hook2),
		typeof(TMP_Text_SetText_Hook3),
		typeof(TMP_Text_SetCharArray_Hook1),
		typeof(TMP_Text_SetCharArray_Hook2),
		typeof(TMP_Text_SetCharArray_Hook3),
		typeof(TextWindow_SetText_Hook),
		typeof(TeshMeshProUGUI_text_Hook),
		typeof(TeshMeshPro_text_Hook)
	};

	public static readonly Type[] DisableScrollInTmp = new Type[1] { typeof(TMP_Text_maxVisibleCharacters_Hook) };
}
