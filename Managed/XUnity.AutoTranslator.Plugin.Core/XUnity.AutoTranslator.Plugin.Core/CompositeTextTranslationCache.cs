using System.Text.RegularExpressions;
using XUnity.AutoTranslator.Plugin.Core.Parsing;

namespace XUnity.AutoTranslator.Plugin.Core;

internal class CompositeTextTranslationCache : IReadOnlyTextTranslationCache
{
	private IReadOnlyTextTranslationCache _first;

	private IReadOnlyTextTranslationCache _second;

	public bool AllowGeneratingNewTranslations => _first.AllowFallback;

	public bool AllowFallback => _first.AllowFallback;

	public CompositeTextTranslationCache(IReadOnlyTextTranslationCache first, IReadOnlyTextTranslationCache second)
	{
		_first = first;
		_second = second;
	}

	public bool IsTranslatable(string text, bool isToken, int scope)
	{
		if (!_first.IsTranslatable(text, isToken, scope))
		{
			if (_first.AllowFallback)
			{
				return _second.IsTranslatable(text, isToken, scope);
			}
			return false;
		}
		return true;
	}

	public bool IsPartial(string text, int scope)
	{
		if (!_first.IsPartial(text, scope))
		{
			if (_first.AllowFallback)
			{
				return _second.IsPartial(text, scope);
			}
			return false;
		}
		return true;
	}

	public bool TryGetTranslation(UntranslatedText key, bool allowRegex, bool allowToken, int scope, out string value)
	{
		if (!_first.TryGetTranslation(key, allowRegex, allowToken, scope, out value))
		{
			if (_first.AllowFallback)
			{
				return _second.TryGetTranslation(key, allowRegex, allowToken, scope, out value);
			}
			return false;
		}
		return true;
	}

	public bool TryGetTranslationSplitter(string text, int scope, out Match match, out RegexTranslationSplitter splitter)
	{
		if (!_first.TryGetTranslationSplitter(text, scope, out match, out splitter))
		{
			if (_first.AllowFallback)
			{
				return _second.TryGetTranslationSplitter(text, scope, out match, out splitter);
			}
			return false;
		}
		return true;
	}
}
