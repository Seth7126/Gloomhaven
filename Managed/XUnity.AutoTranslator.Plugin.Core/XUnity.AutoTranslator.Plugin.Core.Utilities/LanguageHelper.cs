using System;
using System.Collections.Generic;
using System.Linq;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.Common.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities;

public static class LanguageHelper
{
	internal static bool HasRedirectedTexts = false;

	private const string MogolianVowelSeparatorString = "\u180e";

	private const char MogolianVowelSeparatorCharacter = '\u180e';

	private static Func<string, bool> DefaultSymbolCheck;

	private static readonly Dictionary<string, Func<string, bool>> LanguageSymbolChecks = new Dictionary<string, Func<string, bool>>(StringComparer.OrdinalIgnoreCase)
	{
		{ "ja", ContainsJapaneseSymbols },
		{ "ru", ContainsRussianSymbols },
		{ "zh-CN", ContainsChineseSymbols },
		{ "zh-TW", ContainsChineseSymbols },
		{ "zh-Hans", ContainsChineseSymbols },
		{ "zh-Hant", ContainsChineseSymbols },
		{ "zh", ContainsChineseSymbols },
		{ "ko", ContainsKoreanSymbols },
		{ "en", ContainsStandardLatinSymbols },
		{
			"auto",
			(string text) => true
		}
	};

	private static readonly HashSet<string> LanguagesNotUsingWhitespaceBetweenWords = new HashSet<string> { "ja", "zh", "zh-CN", "zh-TW", "zh-Hans", "zh-Hant" };

	internal static bool RequiresWhitespaceUponLineMerging(string code)
	{
		return !LanguagesNotUsingWhitespaceBetweenWords.Contains(code);
	}

	internal static Func<string, bool> GetSymbolCheck(string language)
	{
		if (LanguageSymbolChecks.TryGetValue(language, out var value))
		{
			return value;
		}
		return (string text) => true;
	}

	internal static bool ContainsLanguageSymbolsForSourceLanguage(string text)
	{
		if (DefaultSymbolCheck == null)
		{
			DefaultSymbolCheck = GetSymbolCheck(Settings.FromLanguage);
		}
		return DefaultSymbolCheck(text);
	}

	public static bool ContainsVariableSymbols(string text)
	{
		int num = text.IndexOf('{');
		if (num > -1)
		{
			return text.IndexOf('}', num) > num;
		}
		return false;
	}

	public static bool IsRedirected(this string text)
	{
		if (text.Length > 0)
		{
			RedirectedResourceDetection redirectedResourceDetectionStrategy = Settings.RedirectedResourceDetectionStrategy;
			if (redirectedResourceDetectionStrategy != RedirectedResourceDetection.None && (uint)(redirectedResourceDetectionStrategy - 1) <= 2u)
			{
				return text.Contains("\u180e");
			}
			return false;
		}
		return false;
	}

	public static string FixRedirected(this string text)
	{
		switch (Settings.RedirectedResourceDetectionStrategy)
		{
		case RedirectedResourceDetection.AppendMongolianVowelSeparatorAndRemoveAppended:
			if (text.Length > 0 && text[text.Length - 1] == '\u180e')
			{
				return text.Substring(0, text.Length - 1);
			}
			return text;
		case RedirectedResourceDetection.AppendMongolianVowelSeparatorAndRemoveAll:
			return text.Replace("\u180e", string.Empty);
		default:
			return text;
		}
	}

	public static string MakeRedirected(this string text)
	{
		RedirectedResourceDetection redirectedResourceDetectionStrategy = Settings.RedirectedResourceDetectionStrategy;
		if (redirectedResourceDetectionStrategy != RedirectedResourceDetection.None && (uint)(redirectedResourceDetectionStrategy - 1) <= 2u && (ContainsLanguageSymbolsForSourceLanguage(text) || ContainsVariableSymbols(text)))
		{
			HasRedirectedTexts = Settings.RedirectedResourceDetectionStrategy != RedirectedResourceDetection.None;
			return text + "\u180e";
		}
		return text;
	}

	public static bool IsTranslatable(string text)
	{
		if (ContainsLanguageSymbolsForSourceLanguage(text))
		{
			return !Settings.IgnoreTextStartingWith.Any((string x) => text.StartsWithStrict(x));
		}
		return false;
	}

	internal static bool ContainsJapaneseSymbols(string text)
	{
		foreach (char c in text)
		{
			if ((c >= '〡' && c <= '〩') || (c >= '〱' && c <= '〵') || (c >= 'ぁ' && c <= 'ゖ') || (c >= 'ァ' && c <= 'ヺ') || (c >= 'ｦ' && c <= 'ﾝ') || (c >= '一' && c <= '龯') || (c >= '㐀' && c <= '䶿') || (c >= '豈' && c <= '\ufaff'))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool ContainsKoreanSymbols(string text)
	{
		foreach (char c in text)
		{
			if (c >= '가' && c <= '\ud7af')
			{
				return true;
			}
		}
		return false;
	}

	internal static bool ContainsChineseSymbols(string text)
	{
		foreach (char c in text)
		{
			if ((c >= '一' && c <= '龯') || (c >= '㐀' && c <= '䶿') || (c >= '豈' && c <= '\ufaff'))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool ContainsRussianSymbols(string text)
	{
		foreach (char c in text)
		{
			if ((c >= 'Ѐ' && c <= 'ӿ') || (c >= 'Ԁ' && c <= 'ԯ') || (c >= '\u2de0' && c <= '\u2dff') || (c >= 'Ꙁ' && c <= '\ua69f') || (c >= 'ᲀ' && c <= 'ᲈ') || (c >= '\ufe2e' && c <= '\ufe2f') || c == 'ᴫ' || c == 'ᵸ')
			{
				return true;
			}
		}
		return false;
	}

	internal static bool ContainsStandardLatinSymbols(string text)
	{
		foreach (char c in text)
		{
			if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
			{
				return true;
			}
		}
		return false;
	}
}
