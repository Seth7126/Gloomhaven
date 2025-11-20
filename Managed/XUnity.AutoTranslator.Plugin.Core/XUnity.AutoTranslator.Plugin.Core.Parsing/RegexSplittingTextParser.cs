using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core.Parsing;

internal class RegexSplittingTextParser
{
	public bool CanApply(object ui)
	{
		return !ui.IsSpammingComponent();
	}

	public ParserResult Parse(string input, int scope, IReadOnlyTextTranslationCache cache)
	{
		if (cache.TryGetTranslationSplitter(input, scope, out var match, out var splitter))
		{
			return new ParserResult(ParserResultOrigin.RegexTextParser, input, splitter.Translation, allowPartialTranslation: true, cacheCombinedResult: true, Settings.CacheRegexPatternResults, persistTokenResult: true, splitter.CompiledRegex, match);
		}
		return null;
	}
}
