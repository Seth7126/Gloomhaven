using System;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.NGUI;

internal static class NGUIHooks
{
	public static readonly Type[] All = new Type[2]
	{
		typeof(UILabel_text_Hook),
		typeof(UILabel_OnEnable_Hook)
	};
}
