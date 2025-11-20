using System;

namespace XUnity.AutoTranslator.Plugin.Core;

[Flags]
internal enum TextPostProcessing
{
	None = 0,
	ReplaceMacronWithCircumflex = 1,
	RemoveAllDiacritics = 2,
	RemoveApostrophes = 4,
	ReplaceWideCharacters = 8,
	ReplaceHtmlEntities = 0x10
}
