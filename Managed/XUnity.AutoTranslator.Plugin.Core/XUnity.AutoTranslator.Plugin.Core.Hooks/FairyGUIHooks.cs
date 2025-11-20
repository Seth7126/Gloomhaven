using System;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class FairyGUIHooks
{
	public static readonly Type[] All = new Type[2]
	{
		typeof(TextField_text_Hook),
		typeof(TextField_htmlText_Hook)
	};
}
