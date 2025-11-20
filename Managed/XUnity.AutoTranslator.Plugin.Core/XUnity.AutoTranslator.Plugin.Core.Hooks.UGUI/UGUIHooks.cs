using System;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.UGUI;

internal static class UGUIHooks
{
	public static readonly Type[] All = new Type[2]
	{
		typeof(Text_text_Hook),
		typeof(Text_OnEnable_Hook)
	};
}
