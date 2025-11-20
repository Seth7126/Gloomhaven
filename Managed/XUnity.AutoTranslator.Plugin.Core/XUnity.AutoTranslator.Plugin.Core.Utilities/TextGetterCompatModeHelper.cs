using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities;

internal static class TextGetterCompatModeHelper
{
	public static bool IsGettingText;

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static void ReplaceTextWithOriginal(object instance, ref string __result)
	{
		if (!Settings.TextGetterCompatibilityMode || IsGettingText)
		{
			return;
		}
		TextTranslationInfo textTranslationInfo = instance.GetTextTranslationInfo();
		if (textTranslationInfo == null || !textTranslationInfo.IsTranslated)
		{
			return;
		}
		Assembly assembly = instance.GetType().Assembly;
		if (assembly.IsAssemblyCsharp())
		{
			__result = textTranslationInfo.OriginalText;
			return;
		}
		Assembly obj = new StackFrame(4).GetMethod().DeclaringType?.Assembly;
		if (!assembly.Equals(obj))
		{
			__result = textTranslationInfo.OriginalText;
		}
	}
}
