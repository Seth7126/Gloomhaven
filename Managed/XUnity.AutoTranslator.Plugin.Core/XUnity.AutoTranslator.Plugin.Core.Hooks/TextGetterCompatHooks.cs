using System;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class TextGetterCompatHooks
{
	public static readonly Type[] All = new Type[3]
	{
		typeof(Text_text_Hook),
		typeof(TMP_Text_text_Hook),
		typeof(NGUI_Text_text_Hook)
	};
}
