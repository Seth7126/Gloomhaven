using System;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class UtageHooks
{
	public static readonly Type[] All = new Type[5]
	{
		typeof(AdvEngine_JumpScenario_Hook),
		typeof(UguiNovelTextGenerator_LengthOfView_Hook),
		typeof(TextArea2D_text_Hook),
		typeof(TextArea2D_TextData_Hook),
		typeof(TextData_ctor_Hook)
	};
}
