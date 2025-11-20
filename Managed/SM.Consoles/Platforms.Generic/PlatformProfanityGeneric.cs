using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Platforms.Profanity;
using Profanity;

namespace Platforms.Generic;

public class PlatformProfanityGeneric : IPlatformProfanity
{
	private const char CensoredCharacter = '*';

	private readonly char[] _delimiters = new char[19]
	{
		',', ';', ' ', '-', '/', '\\', '!', ':', '|', '?',
		'_', '*', '+', '$', '#', '%', '^', '\'', '"'
	};

	private readonly BadWordsProvider _badWordsProvider;

	public PlatformProfanityGeneric()
	{
		_badWordsProvider = new BadWordsProvider();
	}

	public void CheckBadWordsAsync(string text, Action<OperationResult, bool> resultCallback)
	{
		HashSet<string> badWords = GetBadWords(text, stopIfFound: true);
		resultCallback?.Invoke(OperationResult.Success, badWords.Count > 0);
	}

	public void MaskBadWordsAsync(string text, Action<OperationResult, string> resultCallback)
	{
		string text2 = text;
		foreach (string badWord in GetBadWords(text, stopIfFound: false))
		{
			string pattern = ToRegexPattern(badWord);
			text2 = Regex.Replace(text2, pattern, StarCensoredMatch, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		}
		resultCallback?.Invoke(OperationResult.Success, text2);
	}

	private HashSet<string> GetBadWords(string text, bool stopIfFound)
	{
		HashSet<string> hashSet = new HashSet<string>();
		if (string.IsNullOrEmpty(text))
		{
			return hashSet;
		}
		List<string> list = text.Split(_delimiters, StringSplitOptions.RemoveEmptyEntries).ToList();
		HashSet<string> badWords = _badWordsProvider.BadWords;
		for (int i = 0; i < list.Count; i++)
		{
			string item = list[i];
			if (badWords.Contains(item))
			{
				hashSet.Add(item);
				if (stopIfFound)
				{
					return hashSet;
				}
			}
		}
		foreach (string item2 in badWords)
		{
			for (int j = 0; j < list.Count; j++)
			{
				string text2 = list[j];
				if (string.Compare(text2, item2, CultureInfo.InvariantCulture, CompareOptions.IgnoreNonSpace) == 0)
				{
					hashSet.Add(text2);
					if (stopIfFound)
					{
						return hashSet;
					}
				}
			}
		}
		return hashSet;
	}

	private static string StarCensoredMatch(Group m)
	{
		return new string('*', m.Captures[0].Value.Length);
	}

	private static string ToRegexPattern(string wildcardSearch)
	{
		string text = Regex.Escape(wildcardSearch);
		text = text.Replace("\\*", ".*?");
		text = text.Replace("\\?", ".");
		if (text.StartsWith(".*?", StringComparison.Ordinal))
		{
			text = text[3..];
			text = "(^\\b)*?" + text;
		}
		return "\\b" + text + "\\b";
	}
}
